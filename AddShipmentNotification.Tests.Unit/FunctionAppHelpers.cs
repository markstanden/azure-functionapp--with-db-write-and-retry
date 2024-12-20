using Azure.Messaging.ServiceBus;

public static class FunctionAppHelpers
{
    public static ServiceBusReceivedMessage CreateServiceBusReceivedMessage(
        string json,
        string? messageId = null
    )
    {
        var serviceBusMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(json),
            messageId: messageId ?? Guid.NewGuid().ToString()
        );

        return serviceBusMessage;
    }
}
