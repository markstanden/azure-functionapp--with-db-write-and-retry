using System.Text.Json;
using Azure.Messaging.ServiceBus;
using interview.Sanitation;
using interview.SqlDbService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace interview
{
    public class AddShipmentNotification
    {
        public const string InvalidJsonError =
            "Serialization of ServiceBus message failed - Invalid/Incomplete JSON";

        public const string DatabaseWriteError = "Error while adding shipment notification";
        public const string DatabaseWriteSuccess = "Successfully added shipment notification";

        // https://webhook.site/#!/view/ed4785fc-49db-4c2c-a40f-ceb775e72d96/6ecd6e52-0471-4cef-9c8e-eba216982c43/1
        public const string WebHookUrl =
            "https://webhook.site/ed4785fc-49db-4c2c-a40f-ceb775e72d96";

        private readonly HttpClient _httpClient;
        private readonly ILogger<AddShipmentNotification> _logger;
        private readonly IRetry _retryFn;
        private readonly ISanitation _sanitation;
        private readonly ISqlDbService _sqlDbService;

        /// <summary>
        /// Function constructor, used for Dependency Injection
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="retryFn"></param>
        /// <param name="sanitation"></param>
        /// <param name="sqlDbService"></param>
        /// <param name="httpClient"></param>
        public AddShipmentNotification(
            ILogger<AddShipmentNotification> logger,
            IRetry retryFn,
            ISanitation sanitation,
            ISqlDbService sqlDbService,
            HttpClient httpClient
        )
        {
            _logger = logger;
            _retryFn = retryFn;
            _sanitation = sanitation;
            _sqlDbService = sqlDbService;
            _httpClient = httpClient;
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
            if (notification == null)
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
            bool result = await _retryFn.Attempt(
                () => _sqlDbService.WriteNotification(notification, _sanitation)
            );

            if (!result)
            {
                // DB Write failed
                // Add the servicebus message to the dead letter queue
                await messageActions.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: DatabaseWriteError
                );
                return;
            }

            // DB Write successful
            _logger.LogInformation(
                "{success} [MessageId: {id}]",
                DatabaseWriteSuccess,
                message.MessageId
            );

            // Mark the servicebus message as complete
            await messageActions.CompleteMessageAsync(message);

            // Send the success message
            await SendSuccessMessage("shipmentId", notification.shipmentId);
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
                // Attempts to deserialise the provided JSON into class instances.
                // JSON schema enforced by required fields on data classes, deserialisation will
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

        /// <summary>
        /// Method sends an HTTP Request to the provided url to
        /// register the successful addition of the record to the DB
        /// </summary>
        /// <param name="query"></param>
        /// <param name="message"></param>
        /// <param name="url"></param>
        private async Task SendSuccessMessage(string query, string message, string url = WebHookUrl)
        {
            await _httpClient.GetAsync($"{url}?{query}={message}");
        }
    }
}
