//! SQL Server access through a `bb8` pool over `tiberius`.
//!
//! `bb8-tiberius` 0.16 depends on `tiberius` 0.12, which cannot share types with `tiberius` 0.13, so
//! this module supplies the small `bb8::ManageConnection` adapter itself.
//!
//! Every statement is a `SELECT` with bound parameters. The only dynamic text is the placeholder list of
//! an `IN (...)` clause, generated from the number of ids and never from their values.

use std::collections::HashMap;

use rust_decimal::Decimal;
use tiberius::{Client, Config, FromSql, Row, ToSql, error::Error as TdsError};
use tokio::net::TcpStream;
use tokio_util::compat::{Compat, TokioAsyncWriteCompatExt};

use crate::{
    domain::model::{BomEdge, ProductId, ProductInfo},
    error::InfraError,
    limits::{SQL_IN_CHUNK, SQL_POOL_IDLE_TIMEOUT, SQL_POOL_MAX_SIZE, SQL_POOL_TIMEOUT},
    ports::BomRepository,
};

type SqlClient = Client<Compat<TcpStream>>;

/// Opens and validates `tiberius` connections for `bb8`.
pub struct SqlConnectionManager {
    config: Config,
}

impl bb8::ManageConnection for SqlConnectionManager {
    type Connection = SqlClient;
    type Error = TdsError;

    async fn connect(&self) -> Result<Self::Connection, Self::Error> {
        let tcp = TcpStream::connect(self.config.get_addr()).await?;
        tcp.set_nodelay(true)?;
        Client::connect(self.config.clone(), tcp.compat_write()).await
    }

    async fn is_valid(&self, connection: &mut Self::Connection) -> Result<(), Self::Error> {
        connection.simple_query("SELECT 1").await?.into_row().await?;
        Ok(())
    }

    fn has_broken(&self, _connection: &mut Self::Connection) -> bool {
        false
    }
}

/// SQL Server implementation of [`BomRepository`].
#[derive(Clone)]
pub struct SqlBomRepository {
    pool: bb8::Pool<SqlConnectionManager>,
}

impl SqlBomRepository {
    /// Builds the pool from an ADO.NET connection string and marks connections read-only
    /// (`ApplicationIntent=ReadOnly`). Connections are opened lazily on first use.
    pub async fn connect(ado_connection_string: &str) -> Result<Self, InfraError> {
        let mut config = Config::from_ado_string(ado_connection_string).map_err(sql_error)?;
        config.readonly(true);
        let pool = bb8::Pool::builder()
            .max_size(SQL_POOL_MAX_SIZE)
            .connection_timeout(SQL_POOL_TIMEOUT)
            .idle_timeout(Some(SQL_POOL_IDLE_TIMEOUT))
            .build_unchecked(SqlConnectionManager { config });
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

fn pool_error(error: bb8::RunError<TdsError>) -> InfraError {
    InfraError::Sql(error.to_string())
}

#[cfg(test)]
mod tests {
    use super::placeholders;

    #[test]
    fn placeholder_list_is_one_based_and_comma_separated() {
        assert_eq!(placeholders(3), "@P1,@P2,@P3");
        assert_eq!(placeholders(1), "@P1");
    }
}
