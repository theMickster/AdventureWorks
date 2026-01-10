//! In-memory fakes and request helpers shared by the integration tests.
#![allow(dead_code)]

use std::{
    collections::{HashMap, HashSet},
    sync::{
        Arc, Mutex,
        atomic::{AtomicBool, AtomicUsize, Ordering},
    },
    time::Duration,
};

use aw_bom_cost::{
    batch::ResultDocument,
    domain::model::{BomEdge, ProductId, ProductInfo},
    error::InfraError,
    http::{AppState, router},
    ports::{BomRepository, ResultCache, ResultStore},
    service::BomService,
};
use axum::{
    Router,
    body::Body,
    http::{Request, Response},
};
use http_body_util::BodyExt;
use rust_decimal::Decimal;
use rust_decimal_macros::dec;
use serde_json::Value;
use tower::ServiceExt;

/// Product ids of the fixture BOM.
pub const ROOT: ProductId = 1;
pub const SUB_ASSEMBLY: ProductId = 2;
pub const PART_A: ProductId = 3;
pub const PART_B: ProductId = 4;
pub const PART_C: ProductId = 5;

#[derive(Default)]
pub struct FakeRepo {
    pub edges: Vec<BomEdge>,
    pub products: Vec<ProductInfo>,
    pub labor: HashMap<ProductId, Decimal>,
    pub on_hand: Mutex<HashMap<ProductId, Decimal>>,
    pub component_queries: AtomicUsize,
    pub down: AtomicBool,
}

impl FakeRepo {
    /// Root 1 = 2 x Sub 2 + 1 x Part 5. Sub 2 = 3 x Part 3 + 1 x Part 4. Unit cost 36 material + 17 labor.
    pub fn fixture() -> Self {
        let product = |id: ProductId, cost: Decimal| ProductInfo {
            id,
            name: format!("Product {id}"),
            product_number: format!("PN-{id}"),
            standard_cost: cost,
        };
        let edge = |assembly, component, qty| BomEdge {
            assembly_id: assembly,
            component_id: component,
            per_assembly_qty: qty,
        };
        Self {
            edges: vec![
                edge(ROOT, SUB_ASSEMBLY, dec!(2)),
                edge(ROOT, PART_C, dec!(1)),
                edge(SUB_ASSEMBLY, PART_A, dec!(3)),
                edge(SUB_ASSEMBLY, PART_B, dec!(1)),
            ],
            products: vec![
                product(ROOT, dec!(999)),
                product(SUB_ASSEMBLY, dec!(0)),
                product(PART_A, dec!(4)),
                product(PART_B, dec!(1)),
                product(PART_C, dec!(10)),
            ],
            labor: HashMap::from([(ROOT, dec!(3)), (SUB_ASSEMBLY, dec!(7))]),
            on_hand: Mutex::new(HashMap::from([
                (PART_A, dec!(1000)),
                (PART_B, dec!(1000)),
                (PART_C, dec!(1000)),
            ])),
            ..Self::default()
        }
    }

    pub fn set_stock(&self, id: ProductId, quantity: Decimal) {
        self.on_hand.lock().unwrap().insert(id, quantity);
    }

    fn check_up(&self) -> Result<(), InfraError> {
        if self.down.load(Ordering::SeqCst) {
            return Err(InfraError::Sql("connection refused".to_owned()));
        }
        Ok(())
    }
}

impl BomRepository for FakeRepo {
    async fn ping(&self) -> Result<(), InfraError> {
        self.check_up()
    }

    async fn components_of(&self, assemblies: &[ProductId]) -> Result<Vec<BomEdge>, InfraError> {
        self.check_up()?;
        self.component_queries.fetch_add(1, Ordering::SeqCst);
        Ok(self
            .edges
            .iter()
            .filter(|e| assemblies.contains(&e.assembly_id))
            .cloned()
            .collect())
    }

    async fn products(&self, ids: &[ProductId]) -> Result<Vec<ProductInfo>, InfraError> {
        self.check_up()?;
        Ok(self.products.iter().filter(|p| ids.contains(&p.id)).cloned().collect())
    }

    async fn labor_per_unit(&self, ids: &[ProductId]) -> Result<HashMap<ProductId, Decimal>, InfraError> {
        self.check_up()?;
        Ok(self
            .labor
            .iter()
            .filter(|(id, _)| ids.contains(id))
            .map(|(k, v)| (*k, *v))
            .collect())
    }

    async fn on_hand(&self, ids: &[ProductId]) -> Result<HashMap<ProductId, Decimal>, InfraError> {
        self.check_up()?;
        Ok(self
            .on_hand
            .lock()
            .unwrap()
            .iter()
            .filter(|(id, _)| ids.contains(id))
            .map(|(k, v)| (*k, *v))
            .collect())
    }
}

#[derive(Default)]
pub struct FakeCache {
    pub entries: Mutex<HashMap<String, String>>,
    pub reads: AtomicUsize,
    pub writes: AtomicUsize,
    pub last_ttl: Mutex<Option<Duration>>,
    pub down: AtomicBool,
}

impl FakeCache {
    fn check_up(&self) -> Result<(), InfraError> {
        if self.down.load(Ordering::SeqCst) {
            return Err(InfraError::Cache("connection refused".to_owned()));
        }
        Ok(())
    }
}

impl ResultCache for FakeCache {
    async fn ping(&self) -> Result<(), InfraError> {
        self.check_up()
    }

    async fn get(&self, key: &str) -> Result<Option<String>, InfraError> {
        self.check_up()?;
        self.reads.fetch_add(1, Ordering::SeqCst);
        Ok(self.entries.lock().unwrap().get(key).cloned())
    }

    async fn set(&self, key: &str, value: &str, ttl: Duration) -> Result<(), InfraError> {
        self.check_up()?;
        self.writes.fetch_add(1, Ordering::SeqCst);
        *self.last_ttl.lock().unwrap() = Some(ttl);
        self.entries.lock().unwrap().insert(key.to_owned(), value.to_owned());
        Ok(())
    }
}

#[derive(Default)]
pub struct FakeStore {
    pub documents: Mutex<HashMap<String, ResultDocument>>,
    pub upserts: AtomicUsize,
    pub fail_for: Mutex<HashSet<ProductId>>,
}

impl ResultStore for FakeStore {
    async fn upsert(&self, document: &ResultDocument) -> Result<(), InfraError> {
        if self.fail_for.lock().unwrap().contains(&document.product_id) {
            return Err(InfraError::Store("request rate too large".to_owned()));
        }
        self.upserts.fetch_add(1, Ordering::SeqCst);
        self.documents
            .lock()
            .unwrap()
            .insert(document.id.clone(), document.clone());
        Ok(())
    }
}

/// A router over fakes, with handles to inspect them.
pub struct TestApp {
    pub router: Router,
    pub repo: Arc<FakeRepo>,
    pub cache: Arc<FakeCache>,
    pub store: Arc<FakeStore>,
}

pub const TEST_TTL: Duration = Duration::from_secs(120);

impl TestApp {
    pub fn new() -> Self {
        Self::with_repo(FakeRepo::fixture())
    }

    pub fn with_repo(repo: FakeRepo) -> Self {
        let (repo, cache, store) = (
            Arc::new(repo),
            Arc::new(FakeCache::default()),
            Arc::new(FakeStore::default()),
        );
        let service = BomService::new(repo.clone(), cache.clone(), TEST_TTL);
        let router = router(Arc::new(AppState {
            service,
            store: store.clone(),
        }));
        Self {
            router,
            repo,
            cache,
            store,
        }
    }

    pub async fn send(&self, request: Request<Body>) -> (Response<Body>, Value) {
        let response = self.router.clone().oneshot(request).await.unwrap();
        let (parts, body) = response.into_parts();
        let bytes = body.collect().await.unwrap().to_bytes();
        let json = serde_json::from_slice(&bytes).unwrap_or(Value::Null);
        (Response::from_parts(parts, Body::empty()), json)
    }

    pub async fn get(&self, uri: &str) -> (Response<Body>, Value) {
        self.send(Request::get(uri).body(Body::empty()).unwrap()).await
    }

    pub async fn get_with(&self, uri: &str, header: (&str, &str)) -> (Response<Body>, Value) {
        self.send(
            Request::get(uri)
                .header(header.0, header.1)
                .body(Body::empty())
                .unwrap(),
        )
        .await
    }

    /// Posts a Functions host invocation envelope whose `message` binding holds `body`.
    pub async fn invoke_batch(&self, body: &str) -> (Response<Body>, Value) {
        let envelope = serde_json::json!({ "Data": { "message": body }, "Metadata": {} });
        self.send(
            Request::post("/bom-batch")
                .header("content-type", "application/json")
                .body(Body::from(envelope.to_string()))
                .unwrap(),
        )
        .await
    }
}

pub fn header<'a>(response: &'a Response<Body>, name: &str) -> &'a str {
    response
        .headers()
        .get(name)
        .and_then(|v| v.to_str().ok())
        .unwrap_or_default()
}
