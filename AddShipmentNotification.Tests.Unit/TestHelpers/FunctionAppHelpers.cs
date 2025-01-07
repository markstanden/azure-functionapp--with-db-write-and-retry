using Azure.Messaging.ServiceBus;

namespace AddShipmentNotification.Tests.Unit.TestHelpers;

/// <summary>
/// Static helper methods to help test functionapps
/// </summary>
public static class FunctionAppHelpers
{
    /// <summary>
    /// Creates a custom instance of a ServiceBusReceivedMessage
    /// for use within testing
    /// </summary>
    /// <param name="json"></param>
    /// <param name="messageId">[optional]</param>
    /// <returns>custom ServiceBusReceivedMessage</returns>
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