//! Input and output shapes of the cost engine. Everything here is plain data.

use std::collections::{BTreeMap, HashMap};

use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};

/// `Production.Product.ProductID`.
pub type ProductId = i32;

/// One current row of `Production.BillOfMaterials`.
#[derive(Debug, Clone, PartialEq)]
pub struct BomEdge {
    /// Parent assembly.
    pub assembly_id: ProductId,
    /// Component used by the assembly.
    pub component_id: ProductId,
    /// Units of the component per unit of the assembly.
    pub per_assembly_qty: Decimal,
}

/// Catalogue data for one product.
#[derive(Debug, Clone, PartialEq)]
pub struct ProductInfo {
    /// Product id.
    pub id: ProductId,
    /// Product name.
    pub name: String,
    /// Product number.
    pub product_number: String,
    /// Catalogue standard cost per unit.
    pub standard_cost: Decimal,
}

/// Everything the engine reads from the database, loaded up front so the computation stays pure.
#[derive(Debug, Clone, Default)]
pub struct BomGraph {
    /// Product being costed.
    pub root: ProductId,
    /// Current components keyed by assembly.
    pub edges: HashMap<ProductId, Vec<BomEdge>>,
    /// Catalogue data keyed by product.
    pub products: HashMap<ProductId, ProductInfo>,
    /// Planned routing labor per unit, keyed by product.
    pub labor: HashMap<ProductId, Decimal>,
    /// Quantity on hand summed over every location.
    pub on_hand: HashMap<ProductId, Decimal>,
}

/// What-if unit prices keyed by component.
pub type PriceOverrides = BTreeMap<ProductId, Decimal>;

/// How a node's cost was derived.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub enum CostBasis {
    /// Priced from `Product.StandardCost`; the subtree and its labor are already inside that figure.
    StandardCost,
    /// Priced from a what-if override.
    Override,
    /// Unpriced: material is the sum of its components and labor is its routing plus theirs.
    Rollup,
    /// Sits under a priced ancestor and adds nothing to the totals.
    Shadowed,
}

/// Material, labor and their sum.
#[derive(Debug, Clone, Copy, PartialEq, Default, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct CostBreakdown {
    /// Material cost.
    pub material: Decimal,
    /// Labor cost.
    pub labor: Decimal,
    /// Material plus labor.
    pub total: Decimal,
}

/// One node of the per-level cost tree.
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct BomNode {
    /// Product at this node.
    pub product_id: ProductId,
    /// Product name.
    pub name: String,
    /// Depth below the root; the root is level 0.
    pub level: usize,
    /// Quantity per immediate parent.
    pub per_assembly_qty: Decimal,
    /// Quantity per one unit of the root product.
    pub cumulative_qty: Decimal,
    /// How the cost was derived.
    pub cost_basis: CostBasis,
    /// Cost of one unit of this node.
    pub unit_cost: CostBreakdown,
    /// Contribution to one unit of the root product (`unit_cost` times `cumulative_qty`).
    pub extended_cost: CostBreakdown,
    /// Components, one node per BOM row.
    pub children: Vec<BomNode>,
}

/// A component whose aggregate on-hand quantity cannot cover the build.
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Shortage {
    /// Component that is short.
    pub product_id: ProductId,
    /// Component name.
    pub name: String,
    /// Units the build consumes.
    pub required: Decimal,
    /// Units on hand across all locations.
    pub available: Decimal,
    /// Required minus available.
    pub deficit: Decimal,
}

/// Manufacturing feasibility for the requested quantity.
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Feasibility {
    /// True when no component is short.
    pub feasible: bool,
    /// Distinct leaf components checked.
    pub components_checked: usize,
    /// Shortages, largest deficit first.
    pub shortages: Vec<Shortage>,
}

/// A what-if override that was applied.
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct AppliedOverride {
    /// Overridden component.
    pub product_id: ProductId,
    /// Unit price used.
    pub unit_price: Decimal,
}

/// A what-if override that changed nothing, with the reason.
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct IgnoredOverride {
    /// Overridden component.
    pub product_id: ProductId,
    /// Why the override changed nothing.
    pub reason: String,
}

/// Difference between a what-if run and the baseline.
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct WhatIf {
    /// Overrides that changed the result.
    pub applied: Vec<AppliedOverride>,
    /// Overrides that changed nothing.
    pub ignored: Vec<IgnoredOverride>,
    /// Baseline cost of one unit.
    pub baseline_unit: CostBreakdown,
    /// What-if minus baseline, per unit.
    pub delta_unit: CostBreakdown,
    /// What-if minus baseline, for the whole batch.
    pub delta_batch: CostBreakdown,
    /// Percentage change of the unit total; absent when the baseline is zero.
    pub delta_percent: Option<Decimal>,
}

/// Root product summary.
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct ProductSummary {
    /// Product id.
    pub id: ProductId,
    /// Product name.
    pub name: String,
    /// Product number.
    pub product_number: String,
    /// Catalogue standard cost per unit.
    pub standard_cost: Decimal,
}

/// Cost and feasibility of building `quantity` units of a product.
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct BomCostResult {
    /// Product being costed.
    pub product: ProductSummary,
    /// Units costed.
    pub quantity: u32,
    /// Cost of one unit.
    pub unit_cost: CostBreakdown,
    /// Cost of `quantity` units.
    pub batch_cost: CostBreakdown,
    /// Availability check for `quantity` units.
    pub feasibility: Feasibility,
    /// Per-level cost tree.
    pub tree: BomNode,
    /// Present only on what-if results.
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub what_if: Option<WhatIf>,
}
