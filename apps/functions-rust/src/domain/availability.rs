//! Aggregate availability check over the leaf components of an expanded tree.

use std::collections::BTreeMap;

use rust_decimal::Decimal;

use super::model::{BomGraph, BomNode, Feasibility, ProductId, Shortage};
use crate::limits::DECIMAL_PLACES;

/// Checks whether on-hand stock, summed over all locations, covers `quantity` units.
///
/// The parts consumed by a build are the leaves of the tree (components with no current BOM of their
/// own). A leaf used in several places is checked once against its combined requirement.
pub fn check_availability(graph: &BomGraph, tree: &BomNode, quantity: u32) -> Feasibility {
    let mut required: BTreeMap<ProductId, (&str, Decimal)> = BTreeMap::new();
    let mut stack = vec![tree];
    while let Some(node) = stack.pop() {
        if node.children.is_empty() {
            let entry = required
                .entry(node.product_id)
                .or_insert((node.name.as_str(), Decimal::ZERO));
            entry.1 += node.cumulative_qty * Decimal::from(quantity);
        } else {
            stack.extend(node.children.iter());
        }
    }

    let components_checked = required.len();
    let mut shortages: Vec<Shortage> = required
        .into_iter()
        .filter_map(|(product_id, (name, required))| {
            let available = graph.on_hand.get(&product_id).copied().unwrap_or_default();
            (required > available).then(|| Shortage {
                product_id,
                name: name.to_owned(),
                required: required.round_dp(DECIMAL_PLACES),
                available,
                deficit: (required - available).round_dp(DECIMAL_PLACES),
            })
        })
        .collect();
    shortages.sort_by(|a, b| b.deficit.cmp(&a.deficit).then(a.product_id.cmp(&b.product_id)));

    Feasibility {
        feasible: shortages.is_empty(),
        components_checked,
        shortages,
    }
}
