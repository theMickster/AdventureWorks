//! Boundary validation. Every inbound value is checked here before it reaches the engine.

use std::str::FromStr;

use rust_decimal::Decimal;
use thiserror::Error;

use crate::{
    domain::model::{PriceOverrides, ProductId},
    limits::{
        FUNCTION_KEY_PARAM, MAX_CORRELATION_ID_LEN, MAX_OVERRIDE_PRICE, MAX_OVERRIDES, MAX_QUANTITY, MIN_QUANTITY,
        OVERRIDE_PARAM, QUANTITY_PARAM,
    },
};

/// A request value that failed validation.
#[derive(Debug, Error, PartialEq, Eq)]
pub enum ValidationError {
    /// `productId` is not a positive integer.
    #[error("productId must be a positive integer")]
    ProductId,
    /// Quantity is missing or out of bounds.
    #[error("quantity must be an integer between {min} and {max}", min = MIN_QUANTITY, max = MAX_QUANTITY)]
    Quantity,
    /// More overrides than allowed.
    #[error("at most {max} price overrides are allowed", max = MAX_OVERRIDES)]
    TooManyOverrides,
    /// An override is malformed or out of range.
    #[error("override must look like componentId:price with a positive componentId and a price between 0 and {max}", max = MAX_OVERRIDE_PRICE)]
    Override,
    /// The same component was overridden twice.
    #[error("component {0} is overridden more than once")]
    DuplicateOverride(ProductId),
    /// A query parameter this endpoint does not accept.
    #[error("unknown query parameter '{0}'")]
    UnknownParameter(String),
    /// A batch with no items or too many.
    #[error("a batch must contain between 1 and {max} items", max = crate::limits::MAX_BATCH_ITEMS)]
    BatchSize,
}

/// A validated costing request.
#[derive(Debug, Clone, PartialEq)]
pub struct CostRequest {
    /// Product to cost.
    pub product_id: ProductId,
    /// Units to build.
    pub quantity: u32,
    /// What-if prices; empty for a baseline request.
    pub overrides: PriceOverrides,
}

/// Parses a positive product id.
pub fn parse_product_id(raw: &str) -> Result<ProductId, ValidationError> {
    raw.parse::<ProductId>()
        .ok()
        .filter(|id| *id > 0)
        .ok_or(ValidationError::ProductId)
}

/// Checks a quantity against the allowed bounds.
pub fn check_quantity(quantity: u32) -> Result<u32, ValidationError> {
    (MIN_QUANTITY..=MAX_QUANTITY)
        .contains(&quantity)
        .then_some(quantity)
        .ok_or(ValidationError::Quantity)
}

/// Builds a request from the path id and the raw query pairs. Unknown parameters are rejected so
/// typos never silently change the answer.
pub fn parse_cost_request(product_id: &str, query: &[(String, String)]) -> Result<CostRequest, ValidationError> {
    let product_id = parse_product_id(product_id)?;
    let mut quantity = MIN_QUANTITY;
    let mut overrides = PriceOverrides::new();
    let mut seen_quantity = false;

    for (key, value) in query {
        match key.as_str() {
            QUANTITY_PARAM if !seen_quantity => {
                quantity = value
                    .parse()
                    .map_err(|_| ValidationError::Quantity)
                    .and_then(check_quantity)?;
                seen_quantity = true;
            }
            QUANTITY_PARAM => return Err(ValidationError::Quantity),
            OVERRIDE_PARAM => {
                if overrides.len() >= MAX_OVERRIDES {
                    return Err(ValidationError::TooManyOverrides);
                }
                let (id, price) = parse_override(value)?;
                if overrides.insert(id, price).is_some() {
                    return Err(ValidationError::DuplicateOverride(id));
                }
            }
            FUNCTION_KEY_PARAM => {}
            other => return Err(ValidationError::UnknownParameter(other.to_owned())),
        }
    }
    Ok(CostRequest {
        product_id,
        quantity,
        overrides,
    })
}

fn parse_override(raw: &str) -> Result<(ProductId, Decimal), ValidationError> {
    let (id, price) = raw.split_once(':').ok_or(ValidationError::Override)?;
    let id = id
        .parse::<ProductId>()
        .ok()
        .filter(|id| *id > 0)
        .ok_or(ValidationError::Override)?;
    let price = Decimal::from_str(price).map_err(|_| ValidationError::Override)?;
    if price.is_sign_negative() || price > Decimal::from(MAX_OVERRIDE_PRICE) {
        return Err(ValidationError::Override);
    }
    Ok((id, price))
}

/// Accepts an inbound correlation id only if it is short and made of header-safe characters.
pub fn sanitize_correlation_id(raw: &str) -> Option<String> {
    let valid = !raw.is_empty()
        && raw.len() <= MAX_CORRELATION_ID_LEN
        && raw
            .chars()
            .all(|c| c.is_ascii_alphanumeric() || matches!(c, '-' | '_' | '.' | ':'));
    valid.then(|| raw.to_owned())
}

#[cfg(test)]
mod tests {
    use rust_decimal_macros::dec;

    use super::*;

    fn pairs(raw: &[(&str, &str)]) -> Vec<(String, String)> {
        raw.iter().map(|(k, v)| ((*k).to_owned(), (*v).to_owned())).collect()
    }

    #[test]
    fn product_id_must_be_a_positive_integer() {
        assert_eq!(parse_product_id("771"), Ok(771));
        for bad in ["0", "-4", "abc", "", "2147483648", "7.5"] {
            assert_eq!(parse_product_id(bad), Err(ValidationError::ProductId), "{bad}");
        }
    }

    #[test]
    fn quantity_defaults_to_one_and_honours_bounds() {
        assert_eq!(parse_cost_request("1", &[]).unwrap().quantity, 1);
        assert_eq!(
            parse_cost_request("1", &pairs(&[("quantity", "10000")]))
                .unwrap()
                .quantity,
            10_000
        );
        for bad in ["0", "10001", "-1", "x", ""] {
            assert_eq!(
                parse_cost_request("1", &pairs(&[("quantity", bad)])),
                Err(ValidationError::Quantity),
                "{bad}"
            );
        }
        assert_eq!(
            parse_cost_request("1", &pairs(&[("quantity", "2"), ("quantity", "3")])),
            Err(ValidationError::Quantity)
        );
    }

    #[test]
    fn overrides_are_parsed_into_a_map() {
        let request = parse_cost_request("1", &pairs(&[("override", "527:1.25"), ("override", "3:0")])).unwrap();

        assert_eq!(request.overrides.get(&527), Some(&dec!(1.25)));
        assert_eq!(request.overrides.get(&3), Some(&dec!(0)));
    }

    #[test]
    fn malformed_overrides_are_rejected() {
        for bad in ["527", "0:1", "-1:1", "a:1", "5:-1", "5:x", "5:1000001", ":1", "5:"] {
            assert_eq!(
                parse_cost_request("1", &pairs(&[("override", bad)])),
                Err(ValidationError::Override),
                "{bad}"
            );
        }
    }

    #[test]
    fn duplicate_overrides_are_rejected() {
        assert_eq!(
            parse_cost_request("1", &pairs(&[("override", "5:1"), ("override", "5:2")])),
            Err(ValidationError::DuplicateOverride(5))
        );
    }

    #[test]
    fn override_count_is_capped() {
        let at_cap: Vec<_> = (1..=MAX_OVERRIDES)
            .map(|i| ("override".to_owned(), format!("{i}:1")))
            .collect();
        assert!(parse_cost_request("1", &at_cap).is_ok());

        let over_cap: Vec<_> = (1..=MAX_OVERRIDES + 1)
            .map(|i| ("override".to_owned(), format!("{i}:1")))
            .collect();
        assert_eq!(
            parse_cost_request("1", &over_cap),
            Err(ValidationError::TooManyOverrides)
        );
    }

    #[test]
    fn function_key_parameter_is_ignored() {
        let request = parse_cost_request("1", &pairs(&[("code", "host-key"), ("quantity", "3")])).unwrap();

        assert_eq!(request.quantity, 3);
        assert!(request.overrides.is_empty());
    }

    #[test]
    fn unknown_parameters_are_rejected() {
        assert_eq!(
            parse_cost_request("1", &pairs(&[("qty", "2")])),
            Err(ValidationError::UnknownParameter("qty".to_owned()))
        );
    }

    #[test]
    fn correlation_ids_are_sanitized() {
        assert_eq!(
            sanitize_correlation_id("abc-123_x.y:z"),
            Some("abc-123_x.y:z".to_owned())
        );
        assert_eq!(
            sanitize_correlation_id(&"a".repeat(MAX_CORRELATION_ID_LEN)).map(|s| s.len()),
            Some(64)
        );
        for bad in [
            "",
            "has space",
            "new\nline",
            "semi;colon",
            &"a".repeat(MAX_CORRELATION_ID_LEN + 1),
        ] {
            assert_eq!(sanitize_correlation_id(bad), None, "{bad:?}");
        }
    }

    #[test]
    fn check_quantity_enforces_bounds() {
        assert_eq!(check_quantity(0), Err(ValidationError::Quantity));
        assert_eq!(check_quantity(1), Ok(1));
        assert_eq!(check_quantity(MAX_QUANTITY + 1), Err(ValidationError::Quantity));
    }
}
