namespace AdventureWorks.SalesOrderSaga.Contracts;

public static class SagaEventNames
{
    public const string OrderCreated = "OrderCreated";
    public const string PaymentApproved = "PaymentApproved";
    public const string PaymentDeclined = "PaymentDeclined";
    public const string PaymentTimedOut = "PaymentTimedOut";
    public const string OrderApproved = "OrderApproved";
    public const string OrderFailed = "OrderFailed";
}

public static class SagaIds
{
    public static string InstanceIdFor(int salesOrderId) => $"sales-order-saga-{salesOrderId}";
    public static string MessageId(string eventName, int salesOrderId) => $"{eventName}:{InstanceIdFor(salesOrderId)}";
}

public sealed record OrderCreatedEvent(
    int SalesOrderId,
    int CustomerId,
    DateTimeOffset OrderDate,
    IReadOnlyList<SagaOrderLine> Lines);

public sealed record SagaOrderLine(int ProductId, short OrderQty, decimal UnitPrice);

public sealed record PaymentAuthorizationRequest(
    int SalesOrderId,
    decimal Amount,
    string Currency,
    string IdempotencyKey,
    IReadOnlyList<SagaOrderLine> Lines);

public sealed record PaymentAuthorizationAcknowledgement(string AuthorizationRequestId);

public sealed record PaymentResultEvent(
    int SalesOrderId,
    string IdempotencyKey,
    string? AuthorizationCode = null,
    string? Reason = null);

public sealed record OrderApprovedEvent(
    int SalesOrderId,
    IReadOnlyList<OrderApprovedLine> Lines,
    OrderShippingAddress ShipTo,
    OrderShipMethod ShipMethod);

public sealed record OrderApprovedLine(int ProductId, short OrderQty, decimal UnitPrice);
public sealed record OrderShippingAddress(int AddressId, string AddressLine1, string? AddressLine2, string City, string PostalCode);
public sealed record OrderShipMethod(int ShipMethodId, string Name);
public sealed record OrderFailedEvent(int SalesOrderId, string Status, string? Reason);
public sealed record PaymentTimedOutEvent(int SalesOrderId, string IdempotencyKey, DateTimeOffset TimedOutAt);
