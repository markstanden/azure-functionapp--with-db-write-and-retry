using System.ComponentModel.DataAnnotations;

namespace AddShipmentNotification.Models.Domain
{
    /// <summary>
    /// Class <c>ShipmentLine</c> describes individual lines within the
    /// <c>ShipmentNotification</c> sent from a provider, and is used for
    /// JSON deserialization of the servicebus message
    /// </summary>
    [Serializable]
    public class ShipmentLine
    {
        /// <summary>
        /// For serialisation and sanitation purposes I have assumed the
        /// Sku is a alphanumeric string only.
        /// Sanitisation will remove all special characters and whitespace.
        /// </summary>
        [Required]
        public required string sku { get; init; }

        /// <summary>
        /// JSON serialisation will throw if the supplied value is not an integer
        /// </summary>
        [Required]
        public required int quantity { get; init; }
    }
}
