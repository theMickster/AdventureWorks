//! Messaging integration tests over RabbitMQ: the retry and dead-letter contract of `POST /bom-batch`.
//!
//! Ignored by default. Run with `scripts/test-live.sh` after starting the `rabbitmq` compose profile. Service Bus stays
//! the real transport; RabbitMQ stands in for it here because it needs no Azure resources and no SQL-backed emulator.
//! A small test-only consumer plays the Functions host: it hands each message to the handler as the `Data`/`Metadata`
//! envelope, acknowledges on 2xx, redelivers on any other status, and dead-letters after the delivery limit. That
//! verifies the handler's contract, not the Service Bus trigger binding, which stays unverified until it runs against
//! the Service Bus emulator or the real namespace.

use std::sync::Arc;

use aw_bom_cost::{
    http::{AppState, router},
    infra::{cosmos::CosmosResultStore, key_vault, redis_cache::RedisCache, sql::SqlBomRepository},
    service::BomService,
};
use axum::{Router, body::Body, http::Request};
use lapin::{
    BasicProperties, Channel, Connection, ConnectionProperties,
    options::{
        BasicAckOptions, BasicGetOptions, BasicNackOptions, BasicPublishOptions, BasicRejectOptions,
        ExchangeDeclareOptions, QueueBindOptions, QueueDeclareOptions, QueueDeleteOptions,
    },
    types::{AMQPValue, FieldTable},
};
use serde_json::json;
use tower::ServiceExt;
use uuid::Uuid;

/// Local-only credentials of the compose `rabbitmq` profile; override with `RABBITMQ_URL`.
const DEFAULT_RABBITMQ_URL: &str = "amqp://aw:aw-local-only@localhost:5675/%2f";
/// Same as the max delivery count of the Service Bus queue `bom-batch-requests-dev`.
const DELIVERY_LIMIT: u32 = 3;
/// Mountain-100 Silver, 38.
const MOUNTAIN_100: i32 = 771;
/// Quantity unlikely to collide with the other live tests or manual runs, so the Cosmos id is `bom:771:11`.
const TEST_QUANTITY: u32 = 11;
const GOOD_DOCUMENT_ID: &str = "bom:771:11";

struct Broker {
    channel: Channel,
    queue: String,
    dead_letter_queue: String,
    dead_letter_exchange: String,
    // Kept alive for the channel.
    _connection: Connection,
}

impl Broker {
    /// Declares a uniquely named work queue that dead-letters into its own exchange and queue.
    async fn start() -> Self {
        let url = std::env::var("RABBITMQ_URL").unwrap_or_else(|_| DEFAULT_RABBITMQ_URL.to_owned());
        let connection = Connection::connect(&url, ConnectionProperties::default())
            .await
            .expect("RabbitMQ is not reachable; start it with --profile rabbitmq");
        let channel = connection.create_channel().await.unwrap();
        let suffix = Uuid::new_v4().simple().to_string();
        let queue = format!("bom-batch-requests-test-{suffix}");
        let dead_letter_queue = format!("{queue}.dlq");
        let dead_letter_exchange = format!("{queue}.dlx");

        channel
            .exchange_declare(
                dead_letter_exchange.as_str().into(),
                lapin::ExchangeKind::Direct,
                ExchangeDeclareOptions::default(),
                FieldTable::default(),
            )
            .await
            .unwrap();
        channel
            .queue_declare(
                dead_letter_queue.as_str().into(),
                QueueDeclareOptions::durable(),
                FieldTable::default(),
            )
            .await
            .unwrap();
        channel
            .queue_bind(
                dead_letter_queue.as_str().into(),
                dead_letter_exchange.as_str().into(),
                dead_letter_queue.as_str().into(),
                QueueBindOptions::default(),
                FieldTable::default(),
            )
            .await
            .unwrap();
        let mut arguments = FieldTable::default();
        arguments.insert(
            "x-dead-letter-exchange".into(),
            AMQPValue::LongString(dead_letter_exchange.as_str().into()),
        );
        arguments.insert(
            "x-dead-letter-routing-key".into(),
            AMQPValue::LongString(dead_letter_queue.as_str().into()),
        );
        channel
            .queue_declare(queue.as_str().into(), QueueDeclareOptions::durable(), arguments)
            .await
            .unwrap();

        Self {
            channel,
            queue,
            dead_letter_queue,
            dead_letter_exchange,
            _connection: connection,
        }
    }

    async fn publish(&self, body: &str) {
        self.channel
            .basic_publish(
                "".into(),
                self.queue.as_str().into(),
                BasicPublishOptions::default(),
                body.as_bytes(),
                BasicProperties::default(),
            )
            .await
            .unwrap()
            .await
            .unwrap();
    }

    /// Plays the Functions host until the work queue is empty. Returns how many invocations the message took.
    async fn consume(&self, router: &Router) -> u32 {
        let mut attempts = 0;
        // A requeued message is available again immediately, so one empty read means the queue is drained.
        while let Some(message) = self
            .channel
            .basic_get(self.queue.as_str().into(), BasicGetOptions::default())
            .await
            .unwrap()
        {
            attempts += 1;
            let body = String::from_utf8(message.data.clone()).unwrap();
            let request = Request::post("/bom-batch")
                .header("content-type", "application/json")
                .body(Body::from(
                    json!({ "Data": { "message": body }, "Metadata": {} }).to_string(),
                ))
                .unwrap();
            let status = router.clone().oneshot(request).await.unwrap().status();
            if status.is_success() {
                message.ack(BasicAckOptions::default()).await.unwrap();
            } else if attempts >= DELIVERY_LIMIT {
                message.reject(BasicRejectOptions { requeue: false }).await.unwrap();
            } else {
                message
                    .nack(BasicNackOptions {
                        requeue: true,
                        ..BasicNackOptions::default()
                    })
                    .await
                    .unwrap();
            }
        }
        attempts
    }

    async fn dead_letter(&self) -> Option<String> {
        self.channel
            .basic_get(self.dead_letter_queue.as_str().into(), BasicGetOptions::default())
            .await
            .unwrap()
            .map(|message| String::from_utf8(message.data.clone()).unwrap())
    }

    async fn cleanup(self) {
        for queue in [&self.queue, &self.dead_letter_queue] {
            let _ = self
                .channel
                .queue_delete(queue.as_str().into(), QueueDeleteOptions::default())
                .await;
        }
        let _ = self
            .channel
            .exchange_delete(self.dead_letter_exchange.as_str().into(), Default::default())
            .await;
    }
}

async fn handler() -> (Router, CosmosResultStore) {
    let settings = key_vault::settings_from_env()
        .await
        .expect("run via scripts/test-live.sh so settings are in the environment");
    let repository = SqlBomRepository::connect(&settings.sql_connection_string)
        .await
        .unwrap();
    let cache = RedisCache::new(&settings.redis_url).unwrap();
    let store = CosmosResultStore::new(&settings.cosmos).unwrap();
    let state = Arc::new(AppState {
        service: BomService::new(repository, cache, settings.cache_ttl),
        store: store.clone(),
    });
    (router(state), store)
}

#[tokio::test]
#[ignore = "needs RabbitMQ (compose profile rabbitmq), local SQL Server, Redis, Key Vault access and the real Cosmos DB account"]
async fn good_message_is_acknowledged_once_and_lands_in_cosmos() {
    let (router, store) = handler().await;
    let broker = Broker::start().await;
    broker
        .publish(&json!({ "items": [{ "productId": MOUNTAIN_100, "quantity": TEST_QUANTITY }] }).to_string())
        .await;

    let attempts = broker.consume(&router).await;

    assert_eq!(attempts, 1, "a 2xx response acknowledges on the first delivery");
    assert!(broker.dead_letter().await.is_none());
    let document = store.read(MOUNTAIN_100, GOOD_DOCUMENT_ID).await.unwrap();
    assert_eq!(document.result.unit_cost.material.to_string(), "1748.2544");
    broker.cleanup().await;
}

#[tokio::test]
#[ignore = "needs RabbitMQ (compose profile rabbitmq), local SQL Server, Redis, Key Vault access and the real Cosmos DB account"]
async fn poison_message_is_redelivered_up_to_the_limit_then_dead_lettered() {
    let (router, _) = handler().await;
    let broker = Broker::start().await;
    let poison = "this is not a batch message";
    broker.publish(poison).await;

    let attempts = broker.consume(&router).await;

    assert_eq!(attempts, DELIVERY_LIMIT);
    assert_eq!(broker.dead_letter().await.as_deref(), Some(poison));
    broker.cleanup().await;
}
