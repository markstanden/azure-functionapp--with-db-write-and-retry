using System.Text.Json;
using AddShipmentNotification.Tests.Unit.TestHelpers;
using Azure.Messaging.ServiceBus;
using interview.Models.Domain;
using interview.Services.Database;
using interview.Services.Retry;
using interview.Services.Validation;
using interview.Services.Webhook;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;

namespace AddShipmentNotification.Tests.Unit.Functions;

public class AddShipmentNotificationTests
{
    private readonly Mock<ServiceBusMessageActions> _mockActions;
    private readonly Mock<ILogger<interview.Functions.AddShipmentNotification>> _mockLog;
    private readonly Mock<ISqlDbService> _mockSqlDbService;
    private readonly Mock<IWebhookService> _mockWebhookService;
    private readonly IRetryService _testRetryService;
    private readonly ISanitation _testSanitation;

    public AddShipmentNotificationTests()
    {
        _mockLog = new Mock<ILogger<interview.Functions.AddShipmentNotification>>();
        _testRetryService = new RetryService() { DelaySeconds = 0 };
        _testSanitation = new Sanitation();
        _mockSqlDbService = new Mock<ISqlDbService>();
        _mockActions = new Mock<ServiceBusMessageActions>();
        _mockWebhookService = new Mock<IWebhookService>();

        DbWriteSuccess(true);
    }

    private void DbWriteSuccess(bool success = true)
    {
        _mockSqlDbService
            .Setup<Task<IRetryable>>(mock =>
                mock.WriteNotificationAsync(It.IsAny<ShipmentNotification>())
            )
            .ReturnsAsync(new Retryable { success = success, message = "success" });
    }

    private void WebhookServiceSetup()
    {
        _mockWebhookService.Setup<Task>(mock =>
            mock.SendMessage(It.IsAny<string>(), It.IsAny<string>())
        );
    }

    [Fact]
    public async Task AddShipmentNotification_WithOneValidLine_ShouldSendServiceBusSucceed()
    {
        // Arrange - Set variables specific to this test
        var testJson = JsonTestData.CreateSingleLine();
        var stubMessage = FunctionAppHelpers.CreateServiceBusReceivedMessage(testJson);

        // Act - trigger the system under test with the arranged variables
        var sut = new interview.Functions.AddShipmentNotification(
            _mockLog.Object,
            _testRetryService,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockWebhookService.Object
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
        var testJson = JsonTestData.CreateJson();
        var stubMessage = FunctionAppHelpers.CreateServiceBusReceivedMessage(testJson);

        var sut = new interview.Functions.AddShipmentNotification(
            _mockLog.Object,
            _testRetryService,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockWebhookService.Object
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

        var sut = new interview.Functions.AddShipmentNotification(
            _mockLog.Object,
            _testRetryService,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockWebhookService.Object
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

        var sut = new interview.Functions.AddShipmentNotification(
            _mockLog.Object,
            _testRetryService,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockWebhookService.Object
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

        var sut = new interview.Functions.AddShipmentNotification(
            _mockLog.Object,
            _testRetryService,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockWebhookService.Object
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

        var testJson = JsonTestData.CreateSingleLine(
            shipmentId: shipmentId,
            shipmentDate: shipmentDate
        );
        var stubMessage = FunctionAppHelpers.CreateServiceBusReceivedMessage(testJson);

        var sut = new interview.Functions.AddShipmentNotification(
            _mockLog.Object,
            _testRetryService,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockWebhookService.Object
        );
        await sut.Run(stubMessage, _mockActions.Object);

        _mockSqlDbService.Verify(
            service =>
                service.WriteNotificationAsync(
                    It.Is<ShipmentNotification>(notification =>
                        notification.shipmentId == shipmentId
                        && notification.shipmentDate == DateTime.Parse(shipmentDate)
                        && notification.shipmentLines.Length == 1
                        && notification.shipmentLines[0].sku == "Sku001"
                        && notification.shipmentLines[0].quantity == 1
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AddShipmentNotification_WithSqlServiceAddError_ShouldSendServiceBusDeadLetter()
    {
        var shipmentId = "TestShipmentId";
        var shipmentDate = "2024-12-09T08:00:00Z";
        var testJson = JsonTestData.CreateJson(shipmentId, shipmentDate);
        var stubMessage = FunctionAppHelpers.CreateServiceBusReceivedMessage(testJson);
        DbWriteSuccess(false);

        var sut = new interview.Functions.AddShipmentNotification(
            _mockLog.Object,
            _testRetryService,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockWebhookService.Object
        );
        await sut.Run(stubMessage, _mockActions.Object);

        _mockActions.Verify(
            actions =>
                actions.DeadLetterMessageAsync(
                    It.Is<ServiceBusReceivedMessage>(message => message == stubMessage),
                    It.IsAny<Dictionary<string, object>>(),
                    It.Is<string>(message =>
                        message == interview.Functions.AddShipmentNotification.DatabaseWriteError
                    ),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AddShipmentNotification_WithSqlServiceAddError_ShouldNotSendHttpSuccessMessage()
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
        DbWriteSuccess(false);
        WebhookServiceSetup();

        var sut = new interview.Functions.AddShipmentNotification(
            _mockLog.Object,
            _testRetryService,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockWebhookService.Object
        );
        await sut.Run(stubMessage, _mockActions.Object);

        _mockWebhookService.Verify(
            actions => actions.SendMessage(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task AddShipmentNotification_WithSqlWriteSuccess_ShouldSendHttpSuccessMessage()
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
        DbWriteSuccess(true);
        WebhookServiceSetup();

        var sut = new interview.Functions.AddShipmentNotification(
            _mockLog.Object,
            _testRetryService,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockWebhookService.Object
        );
        await sut.Run(stubMessage, _mockActions.Object);

        _mockWebhookService.Verify(
            actions => actions.SendMessage(It.IsRegex(nameof(shipmentId)), It.IsRegex(shipmentId)),
            Times.Once
        );
    }

    [Fact]
    public async Task AddShipmentNotification_WithSqlWriteSuccess_HttpSuccessMessageContainsShipmentId()
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
        DbWriteSuccess(true);
        WebhookServiceSetup();

        var sut = new interview.Functions.AddShipmentNotification(
            _mockLog.Object,
            _testRetryService,
            _testSanitation,
            _mockSqlDbService.Object,
            _mockWebhookService.Object
        );
        await sut.Run(stubMessage, _mockActions.Object);

        _mockWebhookService.Verify(
            actions => actions.SendMessage(It.IsRegex(nameof(shipmentId)), It.IsRegex(shipmentId)),
            Times.Once
        );
    }
}
