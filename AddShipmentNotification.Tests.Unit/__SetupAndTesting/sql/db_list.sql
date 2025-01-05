/*
  creates an inner join between the two tables and displays the results with
  a shipmentId containing the required text
 */
SELECT
    shipments.shipmentId,
    shipments.shipmentDate,
    lines.sku,
    lines.quantity
FROM dbo.markShipment AS shipments
         INNER JOIN dbo.markShipment_Line AS lines
                    ON shipments.shipmentId = lines.shipmentId
WHERE shipments.shipmentId LIKE '%2025-01-06%';