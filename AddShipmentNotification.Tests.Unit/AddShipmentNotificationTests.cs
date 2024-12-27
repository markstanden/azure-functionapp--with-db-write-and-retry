using System.Text.Json;
using Azure.Messaging.ServiceBus;
using interview.Retry;
using interview.Sanitation;
using interview.SqlDbService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AddShipmentNotification.Tests.Unit;

public class AddShipmentNotificationTests
{
    private readonly Mock<ServiceBusMessageActions> _mockActions;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<HttpClient> _mockHttp;
    private readonly Mock<ILogger<interview.AddShipmentNotification>> _mockLog;
    private readonly Mock<ISqlDbService> _mockSqlDbService;
    private readonly IRetry _testRetry;
    private readonly ISanitation _testSanitation;

    public AddShipmentNotificationTests()
    {
        _mockLog = new Mock<ILogger<interview.AddShipmentNotification>>();
        _mockConfig = new Mock<IConfiguration>();
        _testRetry = new Retry();
        _testSanitation = new Sanitation();
        _mockSqlDbService = new Mock<ISqlDbService>();
        _mockHttp = new Mock<HttpClient>();
        _mockActions = new Mock<ServiceBusMessageActions>();
    }

    [Fact(Skip = "Awaiting DI implementation of SqlDbService")]
    public void AddShipmentNotification_WithOneValidLine_ShouldSucceed()
    {
        // Arrange - Set variables specific to this test
        var testData = new
        {
            shipmentId = "ValidOneLine",
            shipmentDate = "2024-12-09T08:00:00Z",
            shipmentLines = new[] { new { sku = "TestSku01", quantity = 1 } },
        };
        var json = JsonSerializer.Serialize(testData);
        var stubMessage = FunctionAppHelpers.CreateServiceBusReceivedMessage(json);

        // Act - trigger the system under test with the arranged variables
        var sut = new interview.AddShipmentNotification(
            _mockLog.Object,
            _mockConfig.Object,
            _testRetry,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockHttp.Object
        );
        var result = sut.Run(stubMessage, _mockActions.Object);

        // Assert that the result is as expected.
        _mockActions.Verify(
            actions =>
                actions.CompleteMessageAsync(
                    It.Is<ServiceBusReceivedMessage>(message => message == stubMessage),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact(Skip = "Awaiting DI implementation of SqlDbService")]
    public void AddShipmentNotification_WithTwoValidLines_ShouldSucceed()
    {
        var testData = new
        {
            shipmentId = "ValidTwoLines",
            shipmentDate = "2024-12-09T08:00:00Z",
            shipmentLines = new[]
            {
                new { sku = "TestSku01", quantity = 1 },
                new { sku = "TestSku02", quantity = 2 },
            },
        };
        var json = JsonSerializer.Serialize(testData);
        var stubMessage = FunctionAppHelpers.CreateServiceBusReceivedMessage(json);

        var sut = new interview.AddShipmentNotification(
            _mockLog.Object,
            _mockConfig.Object,
            _testRetry,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockHttp.Object
        );
        var result = sut.Run(stubMessage, _mockActions.Object);

        _mockActions.Verify(
            actions =>
                actions.CompleteMessageAsync(
                    It.Is<ServiceBusReceivedMessage>(message => message == stubMessage),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact(Skip = "Awaiting DI implementation of SqlDbService")]
    public void AddShipmentNotification_WithInvalidJSONIdFieldName_ShouldFail()
    {
        // Invalid Id fieldname
        var testData = new
        {
            Id = "InvalidIdFieldName",
            shipmentDate = "2024-12-09T08:00:00Z",
            shipmentLines = new[] { new { sku = "TestSku01", quantity = 1 } },
        };
        var json = JsonSerializer.Serialize(testData);
        var stubMessage = FunctionAppHelpers.CreateServiceBusReceivedMessage(json);

        var sut = new interview.AddShipmentNotification(
            _mockLog.Object,
            _mockConfig.Object,
            _testRetry,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockHttp.Object
        );
        var result = sut.Run(stubMessage, _mockActions.Object);

        _mockActions.Verify(
            actions =>
                actions.DeadLetterMessageAsync(
                    It.Is<ServiceBusReceivedMessage>(message => message == stubMessage),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact(Skip = "Awaiting DI implementation of SqlDbService")]
    public void AddShipmentNotification_WithInvalidJSONDateValue_ShouldFail()
    {
        // Invalid Id fieldname
        var testData = new
        {
            shipmentId = "InvalidDateValue",
            shipmentDate = "2024/12/09",
            shipmentLines = new[] { new { sku = "TestSku01", quantity = 1 } },
        };
        var json = JsonSerializer.Serialize(testData);
        var stubMessage = FunctionAppHelpers.CreateServiceBusReceivedMessage(json);

        var sut = new interview.AddShipmentNotification(
            _mockLog.Object,
            _mockConfig.Object,
            _testRetry,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockHttp.Object
        );
        var result = sut.Run(stubMessage, _mockActions.Object);

        _mockActions.Verify(
            actions =>
                actions.DeadLetterMessageAsync(
                    It.Is<ServiceBusReceivedMessage>(message => message == stubMessage),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
