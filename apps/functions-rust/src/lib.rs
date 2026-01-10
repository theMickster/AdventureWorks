//! BOM cost explosion and manufacturing feasibility engine, hosted as an Azure Functions custom handler.

#![warn(missing_docs)]

pub mod batch;
pub mod config;
pub mod domain;
pub mod error;
pub mod http;
pub mod infra;
pub mod limits;
pub mod ports;
pub mod service;
pub mod validation;
