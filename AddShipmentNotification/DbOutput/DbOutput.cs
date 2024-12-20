using interview;
using interview.Sanitation;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;

/// <summary>
/// Now unused class, left in to demonstrate the alternate method of writing to
/// a DB within a functionApp.
///
/// The [SqlOutput] bindings below create an Async operation performed on returning of the class
/// instance from the function's Run method.
///
/// Whilst this method works I was unable to implement the required retry logic as the
/// write is conducted after the function completes.
/// </summary>
public class DbOutput
{
    [SqlOutput("dbo.markShipment", connectionStringSetting: "SQLConnectionString")]
    public required DbShipment DbShipment { get; init; }

    [SqlOutput("dbo.markShipment_Line", connectionStringSetting: "SQLConnectionString")]
    public required DbShipmentLine[] DbShipmentLines { get; init; }

    /// <summary>
    /// Creates a DbOutput to be returned from the function's Run method.
    /// On return the supplied notification will be written to the DB.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="sanitation"></param>
    /// <returns></returns>
    public static DbOutput Create(ShipmentNotification notification, ISanitation sanitation)
    {
        return new DbOutput()
        {
            DbShipment = new DbShipment
            {
                shipmentId = sanitation.AlphaNumericsOnly(notification.shipmentId),
                shipmentDate = notification.shipmentDate,
            },
            DbShipmentLines = notification
                .shipmentLines.Select(shipmentLine => new DbShipmentLine()
                {
                    shipmentId = sanitation.AlphaNumericsOnly(notification.shipmentId),
                    sku = sanitation.AlphaNumericsOnly(shipmentLine.sku),
                    quantity = shipmentLine.quantity,
                })
                .ToArray(),
        };
    }
}
