using System.Data;
using System.Text.Json;
using AdventureWorks.SalesOrderSaga.Contracts;
using Azure.Messaging.ServiceBus;
using Microsoft.Data.SqlClient;

namespace AdventureWorks.SalesOrderSaga.TestHarness;

public interface ISagaEventPublisher
{
    Task PublishOrderCreatedAsync(FixtureOrder order, CancellationToken cancellationToken);
    Task PublishPaymentResultAsync(SimulatedAuthorization authorization, CancellationToken cancellationToken);
}

public sealed class ServiceBusSagaEventPublisher(ServiceBusClient client, IConfiguration configuration) : ISagaEventPublisher
{
    private string Topic => configuration["ServiceBusSalesOrderEventsTopicName"] ?? throw new InvalidOperationException("ServiceBusSalesOrderEventsTopicName is required.");

    public Task PublishOrderCreatedAsync(FixtureOrder order, CancellationToken cancellationToken) =>
        SendAsync(SagaEventNames.OrderCreated, SagaIds.MessageId(SagaEventNames.OrderCreated, order.SalesOrderId), order.ToEvent(), cancellationToken);

    public Task PublishPaymentResultAsync(SimulatedAuthorization authorization, CancellationToken cancellationToken)
    {
        var approved = authorization.State == AuthorizationState.Approved;
        var eventName = approved ? SagaEventNames.PaymentApproved : SagaEventNames.PaymentDeclined;
        var payload = new PaymentResultEvent(authorization.SalesOrderId, authorization.IdempotencyKey, authorization.AuthorizationCode, authorization.DeclineReason);
        return SendAsync(eventName, SagaIds.MessageId(eventName, authorization.SalesOrderId), payload, cancellationToken);
    }

    private async Task SendAsync<T>(string subject, string messageId, T payload, CancellationToken cancellationToken)
    {
        await using var sender = client.CreateSender(Topic);
        await sender.SendMessageAsync(new ServiceBusMessage(BinaryData.FromObjectAsJson(payload)) { Subject = subject, MessageId = messageId }, cancellationToken);
    }
}

public interface ISagaStatusClient { Task<SagaStatusDocument?> GetAsync(int salesOrderId, CancellationToken cancellationToken); }

public sealed class SagaStatusClient(HttpClient client) : ISagaStatusClient
{
    public async Task<SagaStatusDocument?> GetAsync(int salesOrderId, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync($"api/saga-status/{salesOrderId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var root = json.RootElement;
        var runtime = root.GetProperty("runtimeStatus").GetString() ?? "Unknown";
        if (!root.TryGetProperty("result", out var result) || result.ValueKind == JsonValueKind.Null)
            return new(runtime, null, null);
        return new(runtime,
            result.TryGetProperty("status", out var status) ? status.ToString() : null,
            result.TryGetProperty("failureReason", out var reason) && reason.ValueKind != JsonValueKind.Null ? reason.GetString() : null);
    }
}

public interface IFixtureStore
{
    Task<FixtureOrder> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<SagaEvidence?> GetEvidenceAsync(int salesOrderId, CancellationToken cancellationToken);
    Task<int> CleanupAsync(CancellationToken cancellationToken);
}

public sealed class MonotonicOrderIdAllocator
{
    private int _last = 899999;
    public int Next(int databaseMaximum)
    {
        while (true)
        {
            var current = Volatile.Read(ref _last);
            var next = Math.Max(current + 1, Math.Max(900000, databaseMaximum + 1));
            if (Interlocked.CompareExchange(ref _last, next, current) == current) return next;
        }
    }
}

public sealed class SqlFixtureStore(IConfiguration configuration, MonotonicOrderIdAllocator allocator) : IFixtureStore
{
    public const string Marker = "AW-SAGA-HARNESS:";
    private string ConnectionString => configuration.GetConnectionString("DefaultConnection")
        ?? configuration["SqlConnectionString"]
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

    public async Task<FixtureOrder> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0 || request.UnitPrice <= 0) throw new ArgumentException("Quantity and unit price must be positive.");
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var maximumCommand = connection.CreateCommand();
        maximumCommand.Transaction = transaction;
        maximumCommand.CommandText = "SELECT ISNULL(MAX(SalesOrderID), 0) FROM Sales.SalesOrderHeader WITH (UPDLOCK, HOLDLOCK);";
        var salesOrderId = allocator.Next(Convert.ToInt32(await maximumCommand.ExecuteScalarAsync(cancellationToken)));
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            DECLARE @CustomerID int, @BillTo int, @ShipTo int, @ShipMethod int;
            SELECT TOP (1) @CustomerID=CustomerID, @BillTo=BillToAddressID, @ShipTo=ShipToAddressID, @ShipMethod=ShipMethodID FROM Sales.SalesOrderHeader ORDER BY SalesOrderID DESC;
            DECLARE @SpecialOfferID int = (SELECT TOP (1) SpecialOfferID FROM Sales.SpecialOfferProduct WHERE ProductID=776 ORDER BY SpecialOfferID);
            IF @CustomerID IS NULL OR @SpecialOfferID IS NULL THROW 50001, 'AdventureWorks reference data for the harness is missing.', 1;
            DECLARE @Now datetime2 = SYSUTCDATETIME(), @Comment nvarchar(128) = CONCAT('AW-SAGA-HARNESS:', CONVERT(varchar(36), NEWID()));
            BEGIN TRY
                SET IDENTITY_INSERT Sales.SalesOrderHeader ON;
                INSERT Sales.SalesOrderHeader (SalesOrderID, RevisionNumber, OrderDate, DueDate, Status, OnlineOrderFlag, CustomerID, BillToAddressID, ShipToAddressID, ShipMethodID, SubTotal, TaxAmt, Freight, Comment, rowguid, ModifiedDate)
                VALUES (@SalesOrderID, 0, @Now, DATEADD(day, 7, @Now), 1, 1, @CustomerID, @BillTo, @ShipTo, @ShipMethod, @Quantity*@UnitPrice, 0, 0, @Comment, NEWID(), @Now);
                SET IDENTITY_INSERT Sales.SalesOrderHeader OFF;
            END TRY
            BEGIN CATCH
                SET IDENTITY_INSERT Sales.SalesOrderHeader OFF;
                THROW;
            END CATCH;
            INSERT Sales.SalesOrderDetail (SalesOrderID, OrderQty, ProductID, SpecialOfferID, UnitPrice, UnitPriceDiscount, rowguid, ModifiedDate)
            VALUES (@SalesOrderID, @Quantity, 776, @SpecialOfferID, @UnitPrice, 0, NEWID(), @Now);
            SELECT @SalesOrderID, @CustomerID, @Now, CAST(776 AS int), @Quantity, @UnitPrice, CAST(1 AS tinyint), CAST(0 AS tinyint),
                   (SELECT SUM(CONVERT(int, Quantity)) FROM Production.ProductInventory WHERE ProductID=776), @Comment;
            """;
        command.Parameters.AddWithValue("@Quantity", request.Quantity);
        command.Parameters.AddWithValue("@UnitPrice", request.UnitPrice);
        command.Parameters.AddWithValue("@SalesOrderID", salesOrderId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        var fixture = new FixtureOrder(reader.GetInt32(0), reader.GetInt32(1), new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc)), reader.GetInt32(3), reader.GetInt16(4), reader.GetDecimal(5), reader.GetByte(6), reader.GetByte(7), reader.GetInt32(8), reader.GetString(9));
        await reader.CloseAsync();
        await transaction.CommitAsync(cancellationToken);
        return fixture;
    }

    public async Task<SagaEvidence?> GetEvidenceAsync(int salesOrderId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT h.SalesOrderID, h.Status, h.RevisionNumber,
              (SELECT SUM(CONVERT(int, Quantity)) FROM Production.ProductInventory WHERE ProductID=776),
              (SELECT COUNT(*) FROM dbo.SalesOrderSagaInventoryAllocation WHERE SalesOrderID=h.SalesOrderID),
              (SELECT COUNT(*) FROM dbo.SalesOrderSagaInventoryAllocation WHERE SalesOrderID=h.SalesOrderID AND ReversedAt IS NOT NULL),
              o.EventName, o.Payload, o.DispatchedAt, ISNULL(o.DispatchAttemptCount,0)
            FROM Sales.SalesOrderHeader h
            OUTER APPLY (SELECT TOP (1) EventName, Payload, DispatchedAt, DispatchAttemptCount FROM dbo.SalesOrderSagaOutboxMessage WHERE MessageID LIKE CONCAT('%sales-order-saga-', h.SalesOrderID) ORDER BY OccurredAt DESC) o
            WHERE h.SalesOrderID=@id AND h.Comment LIKE 'AW-SAGA-HARNESS:%';
            """;
        command.Parameters.AddWithValue("@id", salesOrderId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new SagaEvidence(reader.GetInt32(0), reader.GetByte(1), reader.GetByte(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.IsDBNull(7) ? null : reader.GetString(7), reader.IsDBNull(8) ? null : reader.GetDateTime(8), reader.GetInt32(9));
    }

    public async Task<int> CleanupAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            DECLARE @ids TABLE (SalesOrderID int PRIMARY KEY);
            INSERT @ids SELECT SalesOrderID FROM Sales.SalesOrderHeader WITH (UPDLOCK) WHERE Comment LIKE 'AW-SAGA-HARNESS:%';
            UPDATE inventory
            SET Quantity = inventory.Quantity + restored.Quantity, rowguid = NEWID(), ModifiedDate = SYSUTCDATETIME()
            FROM Production.ProductInventory inventory
            INNER JOIN (
                SELECT ProductID, LocationID, SUM(CONVERT(int, Quantity)) Quantity
                FROM dbo.SalesOrderSagaInventoryAllocation
                WHERE SalesOrderID IN (SELECT SalesOrderID FROM @ids) AND ReversedAt IS NULL
                GROUP BY ProductID, LocationID
            ) restored ON restored.ProductID=inventory.ProductID AND restored.LocationID=inventory.LocationID;
            DELETE FROM dbo.SalesOrderSagaOutboxMessage WHERE MessageID IN (SELECT CONCAT('OrderApproved:sales-order-saga-',SalesOrderID) FROM @ids UNION ALL SELECT CONCAT('OrderFailed:sales-order-saga-',SalesOrderID) FROM @ids UNION ALL SELECT CONCAT('PaymentTimedOut:sales-order-saga-',SalesOrderID) FROM @ids);
            DELETE FROM dbo.SalesOrderSagaInventoryAllocation WHERE SalesOrderID IN (SELECT SalesOrderID FROM @ids);
            DELETE FROM Production.TransactionHistory WHERE ReferenceOrderID IN (SELECT SalesOrderID FROM @ids);
            DELETE FROM Sales.SalesOrderDetail WHERE SalesOrderID IN (SELECT SalesOrderID FROM @ids);
            DELETE FROM Sales.SalesOrderHeader WHERE SalesOrderID IN (SELECT SalesOrderID FROM @ids);
            SELECT COUNT(*) FROM @ids;
            """;
        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return count;
    }
}
