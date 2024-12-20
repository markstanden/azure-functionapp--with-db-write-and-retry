using System.ComponentModel.DataAnnotations;

namespace interview
{
    /// <summary>
    /// Class <c>DbShipment</c> describes entries to the Shipment database table
    /// </summary>
    [Serializable]
    public class DbShipment
    {
        [Required]
        public required string shipmentId { get; init; }

        [Required]
        public required DateTime shipmentDate { get; init; }
    }
}
