using System.Text.Json;
using Azure.Messaging.ServiceBus;
using interview.Models.Domain;
using interview.Sanitation;
using interview.Services.Database;
using interview.Services.Webhook;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace interview.Functions
{
    public class AddShipmentNotification
    {
        // made public as used to verify correct messaging within tests.
        public const string InvalidJsonError =
            "Serialization of ServiceBus message failed - Invalid/Incomplete JSON";

        public const string DatabaseWriteError = "Error while adding shipment notification";
        public const string DatabaseWriteSuccess = "Successfully added shipment notification";

        private readonly ILogger<AddShipmentNotification> _logger;
        private readonly IRetry _retryFn;
        private readonly ISanitation _sanitation;
        private readonly ISqlDbService _sqlDbService;

        private readonly IWebhookService _webhookService;

        /// <summary>
        /// Function constructor, used for Dependency Injection
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="retryFn"></param>
        /// <param name="sanitation"></param>
        /// <param name="sqlDbService"></param>
        /// <param name="webhookService"></param>
        public AddShipmentNotification(
            ILogger<AddShipmentNotification> logger,
            IRetry retryFn,
            ISanitation sanitation,
            ISqlDbService sqlDbService,
            IWebhookService webhookService
        )
        {
            _logger = logger;
            _retryFn = retryFn;
            _sanitation = sanitation;
            _sqlDbService = sqlDbService;
            _webhookService = webhookService;
        }

        [Function(nameof(AddShipmentNotification))]
        public async Task Run(
            [ServiceBusTrigger("ShipmentNotification", Connection = "marksdevuksouth1_SERVICEBUS")]
                ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions
        )
        {
            // Parse servicebus message JSON into ShipmentNotification instance
            ShipmentNotification? notification = ParseMessageJson(message);

            // Type guard for DB add below.
            // Exit early if notification parsing has failed.
            if (notification is null)
            {
                await messageActions.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: InvalidJsonError
                );
                return;
            }

            // Call the retry function and pass the method to add the notification to the DB to it
            // The retry function attempts to do the write 3 times (by default) with a 10 second delay between attempts (default)
            // returns true if successful, false if unsuccessful
            var dbWriteResult = await _retryFn.Attempt(
                () => _sqlDbService.WriteNotificationAsync(notification)
            );

            if (!dbWriteResult.success)
            {
                // DB Write failed
                // Add the servicebus message to the dead letter queue
                await messageActions.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: DatabaseWriteError,
                    deadLetterErrorDescription: dbWriteResult.message
                );
                return;
            }

            // DB Write successful
            _logger.LogInformation($"{DatabaseWriteSuccess} [MessageId: {message.MessageId}]");

            // Mark the servicebus message as complete
            await messageActions.CompleteMessageAsync(message);

            // Send the success message
            await _webhookService.SendMessage(
                "shipmentId",
                _sanitation.AlphaNumericsWithSpecialCharacters(notification.shipmentId, ['-'])
            );
        }

        /// <summary>
        /// Parses the servicebus message JSON into
        /// a ShipmentNotification Class instance.
        /// </summary>
        /// <param name="message">Servicebus message</param>
        /// <returns></returns>
        private ShipmentNotification? ParseMessageJson(ServiceBusReceivedMessage message)
        {
            try
            {
                // Attempts to deserialize the provided JSON into class instances.
                // JSON schema is enforced by required fields on data classes, deserialization will
                // throw if JSON is invalid.
                return message.Body.ToObjectFromJson<ShipmentNotification>();
            }
            catch (JsonException)
            {
                _logger.LogInformation(
                    "{error} [MessageId: {id}]",
                    InvalidJsonError,
                    message.MessageId
                );
                return null;
            }
        }
    }
}
