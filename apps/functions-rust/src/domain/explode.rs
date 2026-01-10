//! Recursive BOM explosion: per-level cost tree, material and labor rollup, what-if deltas.
//!
//! Costing rule. `Product.StandardCost` of a made item already contains the material and labor of
//! everything below it, so the engine prices each branch at the first node that has a price and marks
//! the rest of that branch `shadowed`. Unpriced nodes (cost `0`) are rolled up from their components
//! and add their own routing labor. The root is always rolled up; its own `StandardCost` is the
//! figure being reproduced, not an input.

use std::collections::HashSet;

use rust_decimal::Decimal;

use super::{
    availability::check_availability,
    error::DomainError,
    model::{
        AppliedOverride, BomCostResult, BomGraph, BomNode, CostBasis, CostBreakdown, IgnoredOverride, PriceOverrides,
        ProductId, ProductSummary, WhatIf,
    },
};
use crate::limits::{DECIMAL_PLACES, MAX_BOM_DEPTH, MAX_TREE_NODES};

const REASON_ROOT: &str = "the requested product itself is never priced";
const REASON_SHADOWED: &str = "sits under a component priced from StandardCost or another override";
const REASON_NOT_IN_BOM: &str = "not part of this bill of materials";

/// Costs `quantity` units of the graph's root with catalogue prices.
pub fn explode(graph: &BomGraph, quantity: u32) -> Result<BomCostResult, DomainError> {
    Ok(evaluate(graph, quantity, &PriceOverrides::new())?.result)
}

/// Costs `quantity` units with `overrides` applied and reports the delta from the catalogue baseline.
pub fn explode_what_if(
    graph: &BomGraph,
    quantity: u32,
    overrides: &PriceOverrides,
) -> Result<BomCostResult, DomainError> {
    let baseline = evaluate(graph, quantity, &PriceOverrides::new())?;
    let mut scenario = evaluate(graph, quantity, overrides)?;

    let delta_unit = subtract(&scenario.result.unit_cost, &baseline.result.unit_cost);
    let delta_batch = subtract(&scenario.result.batch_cost, &baseline.result.batch_cost);
    let baseline_total = baseline.result.unit_cost.total;
    let delta_percent = (!baseline_total.is_zero())
        .then(|| (delta_unit.total / baseline_total * Decimal::ONE_HUNDRED).round_dp(DECIMAL_PLACES));

    let applied = overrides
        .iter()
        .filter(|(id, _)| scenario.used_overrides.contains(id))
        .map(|(id, price)| AppliedOverride {
            product_id: *id,
            unit_price: *price,
        })
        .collect();
    let ignored = overrides
        .keys()
        .filter(|id| !scenario.used_overrides.contains(id))
        .map(|id| IgnoredOverride {
            product_id: *id,
            reason: ignore_reason(graph, &scenario, *id).to_owned(),
        })
        .collect();

    scenario.result.what_if = Some(WhatIf {
        applied,
        ignored,
        baseline_unit: baseline.result.unit_cost,
        delta_unit,
        delta_batch,
        delta_percent,
    });
    Ok(scenario.result)
}

struct Evaluation {
    result: BomCostResult,
    used_overrides: HashSet<ProductId>,
    visited: HashSet<ProductId>,
}

fn ignore_reason(graph: &BomGraph, evaluation: &Evaluation, id: ProductId) -> &'static str {
    if id == graph.root {
        REASON_ROOT
    } else if evaluation.visited.contains(&id) {
        REASON_SHADOWED
    } else {
        REASON_NOT_IN_BOM
    }
}

fn evaluate(graph: &BomGraph, quantity: u32, overrides: &PriceOverrides) -> Result<Evaluation, DomainError> {
    let root_info = graph
        .products
        .get(&graph.root)
        .ok_or(DomainError::ProductNotFound(graph.root))?;
    if graph.edges.get(&graph.root).is_none_or(Vec::is_empty) {
        return Err(DomainError::NoBillOfMaterials(graph.root));
    }

    let mut walker = Walker {
        graph,
        overrides,
        nodes: 0,
        used: HashSet::new(),
        visited: HashSet::new(),
    };
    let mut path = Vec::new();
    let (tree, unit) = walker.visit(graph.root, Decimal::ONE, Decimal::ONE, 0, &mut path, false)?;

    let unit_cost = breakdown(unit.material, unit.labor);
    let batch = Decimal::from(quantity);
    let batch_cost = breakdown(unit.material * batch, unit.labor * batch);

    let result = BomCostResult {
        product: ProductSummary {
            id: root_info.id,
            name: root_info.name.clone(),
            product_number: root_info.product_number.clone(),
            standard_cost: root_info.standard_cost,
        },
        quantity,
        unit_cost,
        batch_cost,
        feasibility: check_availability(graph, &tree, quantity),
        tree,
        what_if: None,
    };
    Ok(Evaluation {
        result,
        used_overrides: walker.used,
        visited: walker.visited,
    })
}

/// Exact (unrounded) cost of one unit of a node.
#[derive(Clone, Copy, Default)]
struct UnitCost {
    material: Decimal,
    labor: Decimal,
}

struct Walker<'a> {
    graph: &'a BomGraph,
    overrides: &'a PriceOverrides,
    nodes: usize,
    used: HashSet<ProductId>,
    visited: HashSet<ProductId>,
}

impl Walker<'_> {
    fn visit(
        &mut self,
        id: ProductId,
        per_assembly_qty: Decimal,
        parent_cumulative: Decimal,
        level: usize,
        path: &mut Vec<ProductId>,
        shadowed: bool,
    ) -> Result<(BomNode, UnitCost), DomainError> {
        if path.contains(&id) {
            let mut cycle = path.clone();
            cycle.push(id);
            return Err(DomainError::CircularReference(cycle));
        }
        if level > MAX_BOM_DEPTH {
            return Err(DomainError::DepthExceeded { max: MAX_BOM_DEPTH });
        }
        self.nodes += 1;
        if self.nodes > MAX_TREE_NODES {
            return Err(DomainError::TreeTooLarge { max: MAX_TREE_NODES });
        }

        let info = self.graph.products.get(&id).ok_or(DomainError::ProductNotFound(id))?;
        let cumulative_qty = parent_cumulative * per_assembly_qty;
        let is_root = level == 0;
        if shadowed {
            self.visited.insert(id);
        }

        let (basis, price) = if shadowed {
            (CostBasis::Shadowed, None)
        } else if is_root {
            (CostBasis::Rollup, None)
        } else if let Some(price) = self.overrides.get(&id) {
            self.used.insert(id);
            (CostBasis::Override, Some(*price))
        } else if info.standard_cost > Decimal::ZERO {
            (CostBasis::StandardCost, Some(info.standard_cost))
        } else {
            (CostBasis::Rollup, None)
        };
        let child_shadowed = shadowed || price.is_some();

        let graph = self.graph;
        let edges = graph.edges.get(&id).map_or(&[][..], Vec::as_slice);
        let mut children = Vec::with_capacity(edges.len());
        let mut rolled = UnitCost::default();
        path.push(id);
        for edge in edges {
            let (child, child_unit) = self.visit(
                edge.component_id,
                edge.per_assembly_qty,
                cumulative_qty,
                level + 1,
                path,
                child_shadowed,
            )?;
            rolled.material += edge.per_assembly_qty * child_unit.material;
            rolled.labor += edge.per_assembly_qty * child_unit.labor;
            children.push(child);
        }
        path.pop();

        let unit = match (basis, price) {
            (CostBasis::Shadowed, _) => UnitCost::default(),
            (_, Some(price)) => UnitCost {
                material: price,
                labor: Decimal::ZERO,
            },
            (_, None) => UnitCost {
                material: rolled.material,
                labor: rolled.labor + graph.labor.get(&id).copied().unwrap_or_default(),
            },
        };

        let node = BomNode {
            product_id: id,
            name: info.name.clone(),
            level,
            per_assembly_qty,
            cumulative_qty,
            cost_basis: basis,
            unit_cost: breakdown(unit.material, unit.labor),
            extended_cost: breakdown(unit.material * cumulative_qty, unit.labor * cumulative_qty),
            children,
        };
        Ok((node, unit))
    }
}

fn breakdown(material: Decimal, labor: Decimal) -> CostBreakdown {
    CostBreakdown {
        material: material.round_dp(DECIMAL_PLACES),
        labor: labor.round_dp(DECIMAL_PLACES),
        total: (material + labor).round_dp(DECIMAL_PLACES),
    }
}

fn subtract(scenario: &CostBreakdown, baseline: &CostBreakdown) -> CostBreakdown {
    CostBreakdown {
        material: scenario.material - baseline.material,
        labor: scenario.labor - baseline.labor,
        total: scenario.total - baseline.total,
    }
}

#[cfg(test)]
mod tests {
    use rust_decimal::Decimal;
    use rust_decimal_macros::dec;

    use super::*;
    use crate::domain::model::{BomEdge, ProductInfo, Shortage};

    /// Fluent builder for in-memory graphs.
    struct Graph(BomGraph);

    impl Graph {
        fn new(root: ProductId) -> Self {
            Self(BomGraph {
                root,
                ..BomGraph::default()
            })
        }

        fn product(mut self, id: ProductId, standard_cost: Decimal) -> Self {
            self.0.products.insert(
                id,
                ProductInfo {
                    id,
                    name: format!("P{id}"),
                    product_number: format!("PN-{id}"),
                    standard_cost,
                },
            );
            self
        }

        fn edge(mut self, assembly: ProductId, component: ProductId, qty: Decimal) -> Self {
            self.0.edges.entry(assembly).or_default().push(BomEdge {
                assembly_id: assembly,
                component_id: component,
                per_assembly_qty: qty,
            });
            self
        }

        fn labor(mut self, id: ProductId, cost: Decimal) -> Self {
            self.0.labor.insert(id, cost);
            self
        }

        fn stock(mut self, id: ProductId, quantity: Decimal) -> Self {
            self.0.on_hand.insert(id, quantity);
            self
        }

        fn build(self) -> BomGraph {
            self.0
        }
    }

    /// Root 1 = 2 x Sub 2 (unpriced, labor 7) + 1 x Part 5 (cost 10).
    /// Sub 2 = 3 x Part 3 (cost 4) + 1 x Part 4 (cost 1).
    fn multi_level() -> Graph {
        Graph::new(1)
            .product(1, dec!(999))
            .product(2, dec!(0))
            .product(3, dec!(4))
            .product(4, dec!(1))
            .product(5, dec!(10))
            .edge(1, 2, dec!(2))
            .edge(1, 5, dec!(1))
            .edge(2, 3, dec!(3))
            .edge(2, 4, dec!(1))
            .labor(1, dec!(3))
            .labor(2, dec!(7))
            .stock(3, dec!(1000))
            .stock(4, dec!(1000))
            .stock(5, dec!(1000))
    }

    #[test]
    fn single_level_sums_components_and_root_labor() {
        let graph = Graph::new(1)
            .product(1, dec!(500))
            .product(2, dec!(5))
            .product(3, dec!(3))
            .edge(1, 2, dec!(2))
            .edge(1, 3, dec!(1))
            .labor(1, dec!(10))
            .stock(2, dec!(100))
            .stock(3, dec!(100))
            .build();

        let result = explode(&graph, 3).unwrap();

        assert_eq!(
            result.unit_cost,
            CostBreakdown {
                material: dec!(13),
                labor: dec!(10),
                total: dec!(23)
            }
        );
        assert_eq!(
            result.batch_cost,
            CostBreakdown {
                material: dec!(39),
                labor: dec!(30),
                total: dec!(69)
            }
        );
        assert_eq!(result.tree.children.len(), 2);
        assert_eq!(result.tree.cost_basis, CostBasis::Rollup);
        assert_eq!(result.tree.children[0].cost_basis, CostBasis::StandardCost);
        assert!(result.feasibility.feasible);
    }

    #[test]
    fn multi_level_multiplies_quantities_down_the_tree() {
        let result = explode(&multi_level().build(), 1).unwrap();

        let sub = &result.tree.children[0];
        assert_eq!(sub.level, 1);
        assert_eq!(sub.unit_cost.material, dec!(13));
        assert_eq!(sub.unit_cost.labor, dec!(7));
        assert_eq!(sub.extended_cost.material, dec!(26));

        let part = &sub.children[0];
        assert_eq!(part.level, 2);
        assert_eq!(part.per_assembly_qty, dec!(3));
        assert_eq!(part.cumulative_qty, dec!(6));
        assert_eq!(part.extended_cost.material, dec!(24));

        assert_eq!(result.unit_cost.material, dec!(36));
        assert_eq!(result.unit_cost.labor, dec!(17));
        assert_eq!(result.unit_cost.total, dec!(53));
        assert_eq!(result.tree.extended_cost, result.unit_cost);
    }

    #[test]
    fn priced_assembly_shadows_its_subtree_and_its_labor() {
        let graph = Graph::new(1)
            .product(1, dec!(0))
            .product(2, dec!(100))
            .product(3, dec!(5))
            .edge(1, 2, dec!(1))
            .edge(2, 3, dec!(4))
            .labor(2, dec!(50))
            .build();

        let result = explode(&graph, 1).unwrap();

        assert_eq!(result.unit_cost.material, dec!(100));
        assert_eq!(result.unit_cost.labor, dec!(0));
        let shadowed = &result.tree.children[0].children[0];
        assert_eq!(shadowed.cost_basis, CostBasis::Shadowed);
        assert_eq!(shadowed.extended_cost.total, dec!(0));
    }

    #[test]
    fn circular_reference_is_reported_with_its_path() {
        let graph = Graph::new(1)
            .product(1, dec!(0))
            .product(2, dec!(0))
            .edge(1, 2, dec!(1))
            .edge(2, 1, dec!(1))
            .build();

        assert_eq!(
            explode(&graph, 1).unwrap_err(),
            DomainError::CircularReference(vec![1, 2, 1])
        );
    }

    #[test]
    fn self_reference_is_circular() {
        let graph = Graph::new(1).product(1, dec!(0)).edge(1, 1, dec!(1)).build();

        assert_eq!(
            explode(&graph, 1).unwrap_err(),
            DomainError::CircularReference(vec![1, 1])
        );
    }

    #[test]
    fn shared_subassembly_is_not_a_cycle() {
        let graph = Graph::new(1)
            .product(1, dec!(0))
            .product(2, dec!(0))
            .product(3, dec!(0))
            .product(4, dec!(2))
            .edge(1, 2, dec!(1))
            .edge(1, 3, dec!(1))
            .edge(2, 4, dec!(1))
            .edge(3, 4, dec!(1))
            .build();

        assert_eq!(explode(&graph, 1).unwrap().unit_cost.material, dec!(4));
    }

    #[test]
    fn depth_at_the_limit_is_accepted() {
        let mut graph = Graph::new(0).product(0, dec!(0));
        for id in 1..=MAX_BOM_DEPTH as i32 {
            graph = graph.product(id, dec!(1)).edge(id - 1, id, dec!(1));
        }

        assert!(explode(&graph.build(), 1).is_ok());
    }

    #[test]
    fn depth_beyond_the_limit_is_rejected() {
        let mut graph = Graph::new(0).product(0, dec!(0));
        for id in 1..=(MAX_BOM_DEPTH as i32 + 1) {
            graph = graph.product(id, dec!(0)).edge(id - 1, id, dec!(1));
        }

        assert_eq!(
            explode(&graph.build(), 1).unwrap_err(),
            DomainError::DepthExceeded { max: MAX_BOM_DEPTH }
        );
    }

    #[test]
    fn exponential_fan_out_is_capped() {
        let mut graph = Graph::new(0).product(0, dec!(0));
        for id in 1..=9 {
            graph = graph.product(id, dec!(0));
            for _ in 0..3 {
                graph = graph.edge(id - 1, id, dec!(1));
            }
        }

        assert_eq!(
            explode(&graph.build(), 1).unwrap_err(),
            DomainError::TreeTooLarge { max: MAX_TREE_NODES }
        );
    }

    #[test]
    fn missing_product_and_empty_bom_are_distinct_errors() {
        assert_eq!(
            explode(
                &BomGraph {
                    root: 9,
                    ..BomGraph::default()
                },
                1
            )
            .unwrap_err(),
            DomainError::ProductNotFound(9)
        );

        let no_bom = Graph::new(1).product(1, dec!(5)).build();
        assert_eq!(explode(&no_bom, 1).unwrap_err(), DomainError::NoBillOfMaterials(1));

        let dangling = Graph::new(1).product(1, dec!(0)).edge(1, 7, dec!(1)).build();
        assert_eq!(explode(&dangling, 1).unwrap_err(), DomainError::ProductNotFound(7));
    }

    #[test]
    fn what_if_override_reports_delta_from_baseline() {
        let overrides = PriceOverrides::from([(3, dec!(6))]);

        let result = explode_what_if(&multi_level().build(), 2, &overrides).unwrap();

        let what_if = result.what_if.expect("what-if block");
        // Part 3 appears 6 times per unit: (6 - 4) * 6 = 12 per unit.
        assert_eq!(what_if.delta_unit.material, dec!(12));
        assert_eq!(what_if.delta_unit.labor, dec!(0));
        assert_eq!(what_if.delta_batch.total, dec!(24));
        assert_eq!(what_if.baseline_unit.total, dec!(53));
        assert_eq!(result.unit_cost.total, dec!(65));
        assert_eq!(what_if.delta_percent, Some(dec!(22.6415)));
        assert_eq!(
            what_if.applied,
            vec![AppliedOverride {
                product_id: 3,
                unit_price: dec!(6)
            }]
        );
        assert!(what_if.ignored.is_empty());
        assert_eq!(result.tree.children[0].children[0].cost_basis, CostBasis::Override);
    }

    #[test]
    fn override_on_priced_assembly_shadows_overrides_below_it() {
        let overrides = PriceOverrides::from([(2, dec!(1)), (3, dec!(99)), (1, dec!(1)), (42, dec!(1))]);

        let what_if = explode_what_if(&multi_level().build(), 1, &overrides)
            .unwrap()
            .what_if
            .unwrap();

        let reasons: Vec<_> = what_if
            .ignored
            .iter()
            .map(|i| (i.product_id, i.reason.as_str()))
            .collect();
        assert_eq!(
            reasons,
            vec![(1, REASON_ROOT), (3, REASON_SHADOWED), (42, REASON_NOT_IN_BOM)]
        );
        assert_eq!(what_if.applied.len(), 1);
        // Sub 2 now costs 1 flat: 2 * 1 + 10 = 12 material, labor only the root's 3.
        assert_eq!(what_if.delta_unit.material, dec!(12) - dec!(36));
    }

    #[test]
    fn zero_override_is_a_valid_free_part() {
        let overrides = PriceOverrides::from([(5, dec!(0))]);

        let what_if = explode_what_if(&multi_level().build(), 1, &overrides)
            .unwrap()
            .what_if
            .unwrap();

        assert_eq!(what_if.delta_unit.material, dec!(-10));
    }

    #[test]
    fn what_if_percent_is_absent_when_baseline_is_zero() {
        let graph = Graph::new(1)
            .product(1, dec!(0))
            .product(2, dec!(0))
            .edge(1, 2, dec!(1))
            .build();
        let overrides = PriceOverrides::from([(2, dec!(3))]);

        let what_if = explode_what_if(&graph, 1, &overrides).unwrap().what_if.unwrap();

        assert_eq!(what_if.delta_percent, None);
        assert_eq!(what_if.delta_unit.total, dec!(3));
    }

    #[test]
    fn shortage_reports_required_available_and_deficit() {
        // Part 3 is needed 6 times per unit; 10 on hand covers one unit only.
        let graph = multi_level().stock(3, dec!(10)).build();

        let result = explode(&graph, 2).unwrap();

        assert!(!result.feasibility.feasible);
        assert_eq!(result.feasibility.components_checked, 3);
        assert_eq!(
            result.feasibility.shortages,
            vec![Shortage {
                product_id: 3,
                name: "P3".to_owned(),
                required: dec!(12),
                available: dec!(10),
                deficit: dec!(2),
            }]
        );
    }

    #[test]
    fn shared_leaf_is_checked_against_its_combined_requirement() {
        // Leaf 4 is used 1 x directly and 1 x through the sub-assembly: 2 per unit.
        let graph = Graph::new(1)
            .product(1, dec!(0))
            .product(2, dec!(0))
            .product(4, dec!(1))
            .edge(1, 2, dec!(1))
            .edge(1, 4, dec!(1))
            .edge(2, 4, dec!(1))
            .stock(4, dec!(5))
            .build();

        assert!(explode(&graph, 2).unwrap().feasibility.feasible);
        let result = explode(&graph, 3).unwrap();
        assert_eq!(result.feasibility.components_checked, 1);
        assert_eq!(result.feasibility.shortages[0].required, dec!(6));
        assert_eq!(result.feasibility.shortages[0].deficit, dec!(1));
    }

    #[test]
    fn product_without_stock_row_counts_as_zero_available() {
        let graph = Graph::new(1)
            .product(1, dec!(0))
            .product(2, dec!(1))
            .edge(1, 2, dec!(1))
            .build();

        let result = explode(&graph, 1).unwrap();

        assert_eq!(result.feasibility.shortages[0].available, dec!(0));
    }

    #[test]
    fn shortages_are_ordered_by_deficit_then_id() {
        let graph = Graph::new(1)
            .product(1, dec!(0))
            .product(2, dec!(1))
            .product(3, dec!(1))
            .product(4, dec!(1))
            .edge(1, 2, dec!(5))
            .edge(1, 3, dec!(5))
            .edge(1, 4, dec!(9))
            .build();

        let ids: Vec<_> = explode(&graph, 1)
            .unwrap()
            .feasibility
            .shortages
            .iter()
            .map(|s| s.product_id)
            .collect();

        assert_eq!(ids, vec![4, 2, 3]);
    }

    #[test]
    fn result_round_trips_through_json_for_the_cache() {
        let result = explode(&multi_level().build(), 4).unwrap();

        let raw = serde_json::to_string(&result).unwrap();

        assert_eq!(serde_json::from_str::<BomCostResult>(&raw).unwrap(), result);
    }
}
