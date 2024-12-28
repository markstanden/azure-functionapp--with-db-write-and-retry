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
    [Theory]
    [InlineData("", "Empty string provided as key")]
    [InlineData("ShipmentId", "Invalid capitalization")]
    [InlineData("shipmentID", "Invalid capitalization")]
    [InlineData("shipment_Id", "Additional underscore within key")]
    public async Task AddShipmentNotification_WithInvalidJSONIdFieldName_ShouldSendServiceBusDeadLetter(
        string invalidFieldname,
        string reason
    )
    {
        // Invalid Id fieldname - Should fail to serialize
        var testData = new Dictionary<string, object>
        {
            { invalidFieldname, "TestValue" },
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
            Times.Once,
            $"Expected JSON serialization to fail, with key '{invalidFieldname}' ({reason}) - as it is not part of the expected JSON schema."
        );
    }

    /// <summary>
    /// Attempts to catch any provided JSON not matching the expected schema
    /// </summary>
    [Theory]
    [InlineData("", "Empty string provided as key")]
    [InlineData("ShipmentDate", "Invalid capitalization")]
    [InlineData("shipmentDATE", "Invalid capitalization")]
    [InlineData("shipment_Date", "Additional underscore within key")]
    public async Task AddShipmentNotification_WithInvalidJSONDateFieldName_ShouldSendServiceBusDeadLetter(
        string invalidFieldname,
        string reason
    )
    {
        // Invalid Id fieldname - Should fail to serialize
        var testData = new Dictionary<string, object>
        {
            { "shipmentId", "TestValue" },
            { invalidFieldname, "2024-12-09T08:00:00Z" },
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
            Times.Once,
            $"Expected JSON serialization to fail, with key '{invalidFieldname}' ({reason}) - as it is not part of the expected JSON schema."
        );
    }

    [Theory]
    [InlineData(null, "No Date provided")]
    [InlineData("", "Empty String Provided")]
    [InlineData("2024/12/09", "Invalid date format")]
    [InlineData("2024-02-30T08:00:00Z", "Invalid date")]
    public async Task AddShipmentNotification_WithInvalidJSONDateValue_ShouldFail(
        string? dateValue,
        string reason
    )
    {
        // Invalid Date value - Should fail to serialize
        var testData = new Dictionary<string, object>
        {
            { "shipmentId", "TestValue" },
            { "shipmentDate", dateValue! },
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
            Times.Once,
            $"Expected date with {dateValue} to fail ({reason})"
        );
    }

    [Fact]
    public async Task AddShipmentNotification_WithTwoValidLines_ShouldProvideSqlServiceWithValidData()
    {
        var shipmentId = "TestShipmentId";
        var shipmentDate = "2024-12-09T08:00:00Z";
        var testData = new Dictionary<string, object>
        {
            { "shipmentId", shipmentId },
            { "shipmentDate", shipmentDate },
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

        _mockSqlDbService.Verify(
            service =>
                service.WriteNotification(
                    It.Is<ShipmentNotification>(notification =>
                        notification.shipmentId == shipmentId
                        && notification.shipmentDate == DateTime.Parse(shipmentDate)
                        && notification.shipmentLines.Length == 2
                        && notification.shipmentLines[0].sku == "TestSku01"
                        && notification.shipmentLines[0].quantity == 1
                        && notification.shipmentLines[1].sku == "TestSku02"
                        && notification.shipmentLines[1].quantity == 2
                    ),
                    It.IsAny<ISanitation>()
                ),
            Times.Once
        );
    }
}
