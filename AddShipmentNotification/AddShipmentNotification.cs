using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace interview
{
    public class AddShipmentNotification
    {
        private const string InvalidJsonError =
            "Serialization of ServiceBus message failed - Invalid/Incomplete JSON";

        private const string DatabaseWriteError = "Error while adding shipment notification";
        private const string DatabaseWriteSuccess = "Successfully added shipment notification";
        private const int MaxDbWriteAttempts = 3;
        private const int DbWriteDelaySeconds = 10;

        // https://webhook.site/#!/view/ed4785fc-49db-4c2c-a40f-ceb775e72d96/6ecd6e52-0471-4cef-9c8e-eba216982c43/1
        private const string WebHookUrl =
            "https://webhook.site/ed4785fc-49db-4c2c-a40f-ceb775e72d96";

        private readonly IConfiguration _configuration;
        private readonly string _dbName;

        private readonly HttpClient _httpClient;
        private readonly ILogger<AddShipmentNotification> _logger;
        private readonly string _shipmentLinesTableName;
        private readonly string _shipmentTableName;

        private readonly string _sqlConnectionString;

        /// <summary>
        /// Function constructor, used for Dependency Injection
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpClient"></param>
        public AddShipmentNotification(
            ILogger<AddShipmentNotification> logger,
            IConfiguration configuration,
            HttpClient httpClient
        )
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;

            _dbName = configuration.GetValue<string>("dbName");
            _shipmentTableName = configuration.GetValue<string>("shipmentTableName");
            _shipmentLinesTableName = configuration.GetValue<string>("shipmentLinesTableName");
            _sqlConnectionString = configuration.GetValue<string>("sqlConnectionString");
        }

        [Function(nameof(AddShipmentNotification))]
        public async Task Run(
            [ServiceBusTrigger("ShipmentNotification", Connection = "marksdevuksouth1_SERVICEBUS")]
                ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions
        )
        {
            _logger.LogInformation(_dbName);
            _logger.LogInformation(_shipmentTableName);
            _logger.LogInformation(_shipmentLinesTableName);
            _logger.LogInformation(_sqlConnectionString);

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

            // Creates a sql connection class to add the notification to the DB
            var sql = new SqlDbService<AddShipmentNotification>(_sqlConnectionString, _logger);

            var retry = new Retry.Retry();
            bool result = await retry.Attempt(
                () => sql.WriteNotification(notification, new Sanitation.Sanitation())
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
        /// <param name="shipmentId"></param>
        private async Task SendSuccessMessage(string query, string message, string url = WebHookUrl)
        {
            await _httpClient.GetAsync($"{url}?{query}={message}");
        }
    }
}
