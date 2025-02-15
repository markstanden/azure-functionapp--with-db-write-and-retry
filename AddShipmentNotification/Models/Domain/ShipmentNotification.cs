using System.ComponentModel.DataAnnotations;

namespace AddShipmentNotification.Models.Domain
{
    /// <summary>
    /// Class <c>ShipmentNotification</c> models a shipment notification
    /// sent from a provider, and is used for JSON deserialization of the
    /// servicebus message
    /// </summary>
    [Serializable]
    public class ShipmentNotification
    {
        /// <summary>
        /// For serialisation and sanitation purposes I have assumed the
        /// Shipment is a alphanumeric string that may contain hyphens.
        /// Sanitisation will remove all other special characters and whitespace.
        /// </summary>
        [Required]
        public required string shipmentId { get; init; }

        /// <summary>
        /// JSON serialisation will throw if the supplied value is not a valid datetime string
        /// </summary>
        [Required]
        public required DateTime shipmentDate { get; init; }

        /// <summary>
        /// The order lines within the notification.
        /// Assumed as required as a shipment will always contain items to deliver.
        /// </summary>
        [Required]
        public required ShipmentLine[] shipmentLines { get; init; }
    }
}
