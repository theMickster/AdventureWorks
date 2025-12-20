using Azure.Messaging.ServiceBus;

const string connectionString = "Endpoint=sb://localhost:5673;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
const string topicName = "sales-order-events";

var salesOrderId = args.Length > 0 ? int.Parse(args[0]) : 71774;

var payload = $$"""
{
  "SalesOrderId": {{salesOrderId}},
  "CustomerId": 29825,
  "OrderDate": "2026-07-21T00:00:00+00:00",
  "Lines": [
    { "ProductId": 776, "OrderQty": 1, "UnitPrice": 2024.994 }
  ]
}
""";

await using var client = new ServiceBusClient(connectionString);
var sender = client.CreateSender(topicName);
await sender.SendMessageAsync(new ServiceBusMessage(payload));

Console.WriteLine($"Sent OrderCreated for SalesOrderId={salesOrderId} to topic '{topicName}'.");
