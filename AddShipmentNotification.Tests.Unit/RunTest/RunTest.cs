using System.Text.Json;
using Azure.Messaging.ServiceBus;
using interview;
using interview.Retry;
using interview.Sanitation;
using interview.SqlDbService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AddShipmentNotification.Tests.Unit.RunTest;

public class RunTest
{
    private readonly Mock<ServiceBusMessageActions> _mockActions;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<HttpClient> _mockHttp;
    private readonly Mock<ILogger<interview.AddShipmentNotification>> _mockLog;
    private readonly Mock<ISqlDbService> _mockSqlDbService;
    private readonly IRetry _testRetry;
    private readonly ISanitation _testSanitation;

    public RunTest()
    {
        _mockLog = new Mock<ILogger<interview.AddShipmentNotification>>();
        _mockConfig = new Mock<IConfiguration>();
        _testRetry = new Retry() { DelaySeconds = 0 };
        _testSanitation = new Sanitation();
        _mockSqlDbService = new Mock<ISqlDbService>();
        _mockActions = new Mock<ServiceBusMessageActions>();
        _mockHttp = new Mock<HttpClient>();

        DbWriteSuccess(true);
    }

    public void DbWriteSuccess(bool success = true)
    {
        _mockSqlDbService
            .Setup<Task<bool>>(mock =>
                mock.WriteNotification(It.IsAny<ShipmentNotification>(), It.IsAny<ISanitation>())
            )
            .ReturnsAsync(success);
    }

    [Fact]
    public async Task AddShipmentNotification_WithOneValidLine_ShouldSendServiceBusSucceed()
    {
        // Arrange - Set variables specific to this test
        var testData = new Dictionary<string, object>
        {
            { "shipmentId", "TestValue" },
            { "shipmentDate", "2024-12-09T08:00:00Z" },
            { "shipmentLines", new[] { new { sku = "TestSku01", quantity = 1 } } },
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
        await sut.Run(stubMessage, _mockActions.Object);

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

    [Fact]
    public async Task AddShipmentNotification_WithTwoValidLines_ShouldSendServiceBusSucceed()
    {
        var testData = new Dictionary<string, object>
        {
            { "shipmentId", "TestValue" },
            { "shipmentDate", "2024-12-09T08:00:00Z" },
            {
                "shipmentLines",
                new[]
                {
                    new { sku = "TestSku01", quantity = 1 },
                    new { sku = "TestSku02", quantity = 2 },
                }
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
        await sut.Run(stubMessage, _mockActions.Object);

        _mockActions.Verify(
            actions =>
                actions.CompleteMessageAsync(
                    It.Is<ServiceBusReceivedMessage>(message => message == stubMessage),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Attempts to catch any provided JSON not matching the expected schema
    /// </summary>
    [Fact]
    public async Task AddShipmentNotification_WithInvalidJSONIdFieldName_ShouldSendServiceBusDeadLetter()
    {
        // Invalid Id fieldname - Should fail to serialize
        var testData = new Dictionary<string, object>
        {
            { "shipment_Id", "TestValue" },
            { "shipmentDate", "2024-12-09T08:00:00Z" },
            { "shipmentLines", new[] { new { sku = "TestSku01", quantity = 1 } } },
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
        await sut.Run(stubMessage, _mockActions.Object);

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

    [Theory]
    [InlineData(null, "No Date provided")]
    [InlineData("", "Empty String Provided")]
    [InlineData("2024/12/09", "Invalid date format")]
    [InlineData("2024-02-30T08:00:00Z", "Invalid date")]
    public async Task AddShipmentNotification_WithInvalidJSONDateValue_ShouldFail(
        string dateValue,
        string errorMessage
    )
    {
        // Invalid Date value - Should fail to serialize
        var testData = new Dictionary<string, object>
        {
            { "shipmentId", "TestValue" },
            { "shipmentDate", dateValue },
            { "shipmentLines", new[] { new { sku = "TestSku01", quantity = 1 } } },
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
        await sut.Run(stubMessage, _mockActions.Object);

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
