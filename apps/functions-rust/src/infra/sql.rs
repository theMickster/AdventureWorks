//! SQL Server access through a `bb8` pool over `tiberius`.
//!
//! `bb8-tiberius` 0.16 depends on `tiberius` 0.12, which cannot share types with `tiberius` 0.13, so
//! this module supplies the small `bb8::ManageConnection` adapter itself.
//!
//! Every statement is a `SELECT` with bound parameters. The only dynamic text is the placeholder list of
//! an `IN (...)` clause, generated from the number of ids and never from their values.

use std::{collections::HashMap, sync::Arc};

use azure_core::credentials::TokenCredential;
use rust_decimal::Decimal;
use thiserror::Error;
use tiberius::{AuthMethod, Client, Config, FromSql, Row, ToSql, error::Error as TdsError};
use tokio::net::TcpStream;
use tokio_util::compat::{Compat, TokioAsyncWriteCompatExt};

use crate::{
    domain::model::{BomEdge, ProductId, ProductInfo},
    error::InfraError,
    infra::cosmos::entra_credential,
    limits::{SQL_IN_CHUNK, SQL_POOL_IDLE_TIMEOUT, SQL_POOL_MAX_SIZE, SQL_POOL_TIMEOUT},
    ports::BomRepository,
};

type SqlClient = Client<Compat<TcpStream>>;

/// Token scope for Azure SQL Database.
const SQL_TOKEN_SCOPE: &str = "https://database.windows.net/.default";

/// Why a pooled SQL connection could not be opened or validated.
#[derive(Debug, Error)]
pub enum SqlConnectError {
    /// The TCP connection, the TLS handshake or the login failed.
    #[error(transparent)]
    Tds(#[from] TdsError),
    /// No Microsoft Entra token could be obtained for the login.
    #[error("could not get a Microsoft Entra token for SQL Server: {0}")]
    Token(String),
}

/// Opens and validates `tiberius` connections for `bb8`.
pub struct SqlConnectionManager {
    config: Config,
    /// Set when the connection string asks for Microsoft Entra authentication; every new connection then logs in
    /// with a fresh token instead of a SQL login.
    credential: Option<Arc<dyn TokenCredential>>,
}

impl bb8::ManageConnection for SqlConnectionManager {
    type Connection = SqlClient;
    type Error = SqlConnectError;

    async fn connect(&self) -> Result<Self::Connection, Self::Error> {
        let mut config = self.config.clone();
        if let Some(credential) = &self.credential {
            let token = credential
                .get_token(&[SQL_TOKEN_SCOPE], None)
                .await
                .map_err(|error| SqlConnectError::Token(error.to_string()))?;
            config.authentication(AuthMethod::aad_token(token.token.secret()));
        }
        let client = match open(&config).await {
            // Azure SQL answers a login that comes from inside Azure with a redirect to another address; from
            // outside Azure it proxies, so a local run never sees one. tiberius leaves following it to the
            // caller, and a single redirect is expected.
            Err(TdsError::Routing { host, port }) => open(&routed_config(&config, &host, port)).await,
            other => other,
        };
        client.map_err(SqlConnectError::from)
    }

    async fn is_valid(&self, connection: &mut Self::Connection) -> Result<(), Self::Error> {
        connection.simple_query("SELECT 1").await?.into_row().await?;
        Ok(())
    }

    fn has_broken(&self, _connection: &mut Self::Connection) -> bool {
        false
    }
}

/// Opens one TCP connection to the address in `config` and logs in.
async fn open(config: &Config) -> Result<SqlClient, TdsError> {
    let tcp = TcpStream::connect(config.get_addr()).await?;
    tcp.set_nodelay(true)?;
    Client::connect(config.clone(), tcp.compat_write()).await
}

/// `config` pointed at the address a login was redirected to. TLS still validates the certificate against the
/// server name that was configured, not the redirect address.
fn routed_config(config: &Config, host: &str, port: u16) -> Config {
    let address = config.get_addr();
    let original_host = address
        .rsplit_once(':')
        .map_or(address.as_str(), |(original, _)| original);
    let mut routed = config.clone();
    routed.host(host);
    routed.port(port);
    routed.hostname_in_certificate(original_host);
    routed
}

/// bb8 retries a failed connect until the pool timeout and reports only "Timed out in bb8", so the cause is
/// logged here.
#[derive(Debug, Clone, Copy)]
struct LogConnectErrors;

impl bb8::ErrorSink<SqlConnectError> for LogConnectErrors {
    fn sink(&self, error: SqlConnectError) {
        tracing::warn!(%error, "SQL connection attempt failed");
    }

    fn boxed_clone(&self) -> Box<dyn bb8::ErrorSink<SqlConnectError>> {
        Box::new(*self)
    }
}

/// True when the connection string selects Microsoft Entra authentication with the `Authentication` keyword the
/// .NET apps use. Any other value, or none, keeps the SQL login in the string.
fn uses_entra_authentication(ado_connection_string: &str) -> bool {
    ado_connection_string
        .split(';')
        .filter_map(|pair| pair.split_once('='))
        .any(|(key, value)| {
            key.trim().eq_ignore_ascii_case("authentication")
                && matches!(
                    value.trim().trim_matches(['\'', '"']).to_ascii_lowercase().as_str(),
                    "active directory default" | "active directory managed identity"
                )
        })
}

/// SQL Server implementation of [`BomRepository`].
#[derive(Clone)]
pub struct SqlBomRepository {
    pool: bb8::Pool<SqlConnectionManager>,
}

impl SqlBomRepository {
    /// Builds the pool from an ADO.NET connection string and marks connections read-only
    /// (`ApplicationIntent=ReadOnly`). `Authentication=Active Directory Default` (or `Active Directory Managed
    /// Identity`) logs in with a Microsoft Entra token, the managed identity in Azure and the developer login
    /// locally; otherwise the SQL login in the string is used. Connections are opened lazily on first use.
    pub async fn connect(ado_connection_string: &str) -> Result<Self, InfraError> {
        let mut config = Config::from_ado_string(ado_connection_string).map_err(sql_error)?;
        config.readonly(true);
        let credential = if uses_entra_authentication(ado_connection_string) {
            Some(entra_credential()?)
        } else {
            None
        };
        let pool = bb8::Pool::builder()
            .max_size(SQL_POOL_MAX_SIZE)
            .connection_timeout(SQL_POOL_TIMEOUT)
            .idle_timeout(Some(SQL_POOL_IDLE_TIMEOUT))
            .error_sink(Box::new(LogConnectErrors))
            .build_unchecked(SqlConnectionManager { config, credential });
        Ok(Self { pool })
    }

    /// Runs `template` once per chunk of `ids`, replacing `{ids}` with a bound placeholder list.
    async fn query_chunked<T>(
        &self,
        template: &str,
        ids: &[ProductId],
        read: impl Fn(&Row) -> Result<T, InfraError>,
    ) -> Result<Vec<T>, InfraError> {
        let mut out = Vec::new();
        for chunk in ids.chunks(SQL_IN_CHUNK) {
            let sql = template.replace("{ids}", &placeholders(chunk.len()));
            let params: Vec<&dyn ToSql> = chunk.iter().map(|id| id as &dyn ToSql).collect();
            let mut connection = self.pool.get().await.map_err(pool_error)?;
            let rows = connection
                .query(sql, &params)
                .await
                .map_err(sql_error)?
                .into_first_result()
                .await
                .map_err(sql_error)?;
            for row in &rows {
                out.push(read(row)?);
            }
        }
        Ok(out)
    }
}

const COMPONENTS_SQL: &str = "\
SELECT ProductAssemblyID, ComponentID, CAST(PerAssemblyQty AS decimal(19,4))
FROM Production.BillOfMaterials
WHERE ProductAssemblyID IN ({ids})
  AND StartDate <= GETDATE()
  AND (EndDate IS NULL OR EndDate > GETDATE())
ORDER BY ProductAssemblyID, BillOfMaterialsID";

const PRODUCTS_SQL: &str = "\
SELECT ProductID, Name, ProductNumber, CAST(StandardCost AS decimal(19,4))
FROM Production.Product
WHERE ProductID IN ({ids})";

/// Labor per unit: a routing operation's `PlannedCost` is identical on every work order of a product
/// regardless of order quantity, so average it per operation and sum the operations.
const LABOR_SQL: &str = "\
SELECT ProductID, CAST(SUM(OperationCost) AS decimal(19,4))
FROM (
    SELECT ProductID, OperationSequence, AVG(CAST(PlannedCost AS decimal(19,4))) AS OperationCost
    FROM Production.WorkOrderRouting
    WHERE ProductID IN ({ids})
    GROUP BY ProductID, OperationSequence
) AS Operations
GROUP BY ProductID";

const ON_HAND_SQL: &str = "\
SELECT ProductID, CAST(SUM(CAST(Quantity AS bigint)) AS decimal(19,4))
FROM Production.ProductInventory
WHERE ProductID IN ({ids})
GROUP BY ProductID";

impl BomRepository for SqlBomRepository {
    async fn ping(&self) -> Result<(), InfraError> {
        let mut connection = self.pool.get().await.map_err(pool_error)?;
        connection
            .simple_query("SELECT 1")
            .await
            .map_err(sql_error)?
            .into_row()
            .await
            .map_err(sql_error)?;
        Ok(())
    }

    async fn components_of(&self, assemblies: &[ProductId]) -> Result<Vec<BomEdge>, InfraError> {
        self.query_chunked(COMPONENTS_SQL, assemblies, |row| {
            Ok(BomEdge {
                assembly_id: column(row, 0)?,
                component_id: column(row, 1)?,
                per_assembly_qty: column(row, 2)?,
            })
        })
        .await
    }

    async fn products(&self, ids: &[ProductId]) -> Result<Vec<ProductInfo>, InfraError> {
        self.query_chunked(PRODUCTS_SQL, ids, |row| {
            Ok(ProductInfo {
                id: column(row, 0)?,
                name: column::<&str>(row, 1)?.to_owned(),
                product_number: column::<&str>(row, 2)?.to_owned(),
                standard_cost: column(row, 3)?,
            })
        })
        .await
    }

    async fn labor_per_unit(&self, ids: &[ProductId]) -> Result<HashMap<ProductId, Decimal>, InfraError> {
        self.decimal_by_product(LABOR_SQL, ids).await
    }

    async fn on_hand(&self, ids: &[ProductId]) -> Result<HashMap<ProductId, Decimal>, InfraError> {
        self.decimal_by_product(ON_HAND_SQL, ids).await
    }
}

impl SqlBomRepository {
    async fn decimal_by_product(
        &self,
        template: &str,
        ids: &[ProductId],
    ) -> Result<HashMap<ProductId, Decimal>, InfraError> {
        let rows = self
            .query_chunked(template, ids, |row| {
                Ok((column::<ProductId>(row, 0)?, column::<Decimal>(row, 1)?))
            })
            .await?;
        Ok(rows.into_iter().collect())
    }
}

fn placeholders(count: usize) -> String {
    (1..=count)
        .map(|index| format!("@P{index}"))
        .collect::<Vec<_>>()
        .join(",")
}

fn column<'a, T: FromSql<'a>>(row: &'a Row, index: usize) -> Result<T, InfraError> {
    row.try_get::<T, _>(index)
        .map_err(sql_error)?
        .ok_or_else(|| InfraError::Sql(format!("column {index} was unexpectedly NULL")))
}

fn sql_error(error: TdsError) -> InfraError {
    InfraError::Sql(error.to_string())
}

fn pool_error(error: bb8::RunError<SqlConnectError>) -> InfraError {
    InfraError::Sql(error.to_string())
}

#[cfg(test)]
mod tests {
    use tiberius::Config;

    use super::{placeholders, routed_config, uses_entra_authentication};

    const ENTRA_STRING: &str = "Server=tcp:sql.example.net,1433;Initial Catalog=AdventureWorks;Authentication=Active Directory Default;Encrypt=True";

    #[test]
    fn placeholder_list_is_one_based_and_comma_separated() {
        assert_eq!(placeholders(3), "@P1,@P2,@P3");
        assert_eq!(placeholders(1), "@P1");
    }

    #[test]
    fn redirect_targets_the_new_address_and_leaves_the_original_config_alone() {
        let config =
            Config::from_ado_string("Server=tcp:sql.example.net,1433;User ID=u;Password=p;Encrypt=True").unwrap();

        let routed = routed_config(&config, "10.1.2.3", 11005);

        assert_eq!(routed.get_addr(), "10.1.2.3:11005");
        assert_eq!(config.get_addr(), "sql.example.net:1433");
    }

    #[test]
    fn entra_authentication_is_recognised_in_both_spellings_whatever_the_case_or_spacing() {
        assert!(uses_entra_authentication(ENTRA_STRING));
        assert!(uses_entra_authentication(
            "Server=tcp:s,1433; authentication = 'Active Directory Managed Identity' ;Encrypt=True"
        ));
        assert!(uses_entra_authentication("AUTHENTICATION=active directory default"));
    }

    #[test]
    fn a_sql_login_string_is_not_entra_authentication() {
        assert!(!uses_entra_authentication(
            "Server=tcp:sql.example.net,1433;User ID=u;Password=p;Encrypt=True"
        ));
        assert!(!uses_entra_authentication(
            "Server=s;Authentication=Sql Password;User ID=u;Password=p"
        ));
        assert!(!uses_entra_authentication(
            "Server=s;Password=Authentication=Active Directory Default"
        ));
    }

    #[test]
    fn tiberius_still_parses_an_entra_string_that_has_no_credentials() {
        let config = Config::from_ado_string(ENTRA_STRING).unwrap();

        assert_eq!(config.get_addr(), "sql.example.net:1433");
    }
}
