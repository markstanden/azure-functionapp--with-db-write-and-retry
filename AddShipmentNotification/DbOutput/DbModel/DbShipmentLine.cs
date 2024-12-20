using System.ComponentModel.DataAnnotations;

namespace interview
{
    /// <summary>
    /// Class <c>DbShipmentLine</c> describes entries to the Shipment_Lines database table
    /// </summary>
    [Serializable]
    public class DbShipmentLine
    {
        [Required]
        public required string shipmentId { get; init; }

        [Required]
        public required string sku { get; init; }

        [Required]
        public required int quantity { get; init; }
    }
}
