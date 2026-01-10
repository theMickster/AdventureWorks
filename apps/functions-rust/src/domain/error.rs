//! Failures raised by the pure cost engine.

use thiserror::Error;

use super::model::ProductId;

/// A BOM that cannot be costed.
#[derive(Debug, Error, PartialEq, Eq)]
pub enum DomainError {
    /// The product id has no row in `Production.Product`.
    #[error("product {0} does not exist")]
    ProductNotFound(ProductId),
    /// The product has no current BOM rows.
    #[error("product {0} has no current bill of materials")]
    NoBillOfMaterials(ProductId),
    /// The BOM loops back on itself; the path ends at the repeated product.
    #[error("circular bill of materials: {}", format_path(.0))]
    CircularReference(Vec<ProductId>),
    /// The BOM is deeper than the depth limit.
    #[error("bill of materials is deeper than {max} levels")]
    DepthExceeded {
        /// The depth limit.
        max: usize,
    },
    /// The BOM expands to more nodes than the limit.
    #[error("bill of materials expands to more than {max} nodes")]
    TreeTooLarge {
        /// The node limit.
        max: usize,
    },
}

fn format_path(path: &[ProductId]) -> String {
    path.iter().map(ToString::to_string).collect::<Vec<_>>().join(" -> ")
}
