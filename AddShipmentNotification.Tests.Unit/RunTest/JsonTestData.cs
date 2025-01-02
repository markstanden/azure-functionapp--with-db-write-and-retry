using System.Text.Json;

namespace AddShipmentNotification.Tests.Unit.RunTest;

public class JsonTestData
{
    /// <summary>
    /// Creates valid test JSON data with defaults.
    /// defaults to 5 shipment lines.
    /// </summary>
    /// <param name="shipmentId"></param>
    /// <param name="shipmentDate"></param>
    /// <param name="shipmentLines"></param>
    /// <returns></returns>
    public static Dictionary<string, object> CreateData(
        string? shipmentId = null,
        string? shipmentDate = null,
        params TestShipmentLine[]? shipmentLines
    )
    {
        return new Dictionary<string, object>
        {
            { "shipmentId", shipmentId ?? "TestValue" },
            { "shipmentDate", shipmentDate ?? "2024-12-09T08:00:00Z" },
            {
                "shipmentLines",
                shipmentLines
                    ??
                    [
                        new TestShipmentLine("Sku001", 1),
                        new TestShipmentLine("Sku002", 2),
                        new TestShipmentLine("Sku003", 3),
                        new TestShipmentLine("Sku004", 4),
                        new TestShipmentLine("Sku005", 5),
                    ]
            },
        };
    }

    /// <summary>
    /// Creates Test json data from provided shipment information
    /// </summary>
    /// <param name="shipmentId"></param>
    /// <param name="shipmentDate"></param>
    /// <param name="shipmentLines"></param>
    /// <returns></returns>
    public static string CreateJson(
        string? shipmentId = null,
        string? shipmentDate = null,
        params TestShipmentLine[]? shipmentLines
    )
    {
        return JsonSerializer.Serialize(CreateData(shipmentId, shipmentDate, shipmentLines));
    }

    /// <summary>
    /// Creates test JSON with a customisable single line item
    /// with defaults for line item fields
    /// </summary>
    /// <param name="sku"></param>
    /// <param name="quantity"></param>
    /// <param name="shipmentId"></param>
    /// <param name="shipmentDate"></param>
    /// <returns></returns>
    public static string CreateSingleLine(
        string sku = "Sku001",
        int quantity = 1,
        string? shipmentId = null,
        string? shipmentDate = null
    )
    {
        TestShipmentLine[] lines = [new TestShipmentLine(sku, quantity)];
        return CreateJson(shipmentId: shipmentId, shipmentDate: shipmentDate, shipmentLines: lines);
    }

    /// <summary>
    /// Creates test JSON representing an empty shipment notification
    /// with no items in the shipment.
    /// </summary>
    /// <param name="sku"></param>
    /// <param name="quantity"></param>
    /// <param name="shipmentId"></param>
    /// <param name="shipmentDate"></param>
    /// <returns></returns>
    public static string CreateEmpty(string? shipmentId = null, string? shipmentDate = null)
    {
        return CreateJson(shipmentId: shipmentId, shipmentDate: shipmentDate, shipmentLines: []);
    }

    /// <summary>
    /// Record class to produce serializable shipment lines
    /// </summary>
    /// <param name="Sku"></param>
    /// <param name="Quantity"></param>
    public record TestShipmentLine(string sku, int quantity);
}
