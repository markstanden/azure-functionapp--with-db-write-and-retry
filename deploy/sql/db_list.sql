/*
  creates an inner join between the two tables and displays the results with
  a shipmentId containing the required text
 */

-- Replace the string here with the shipmentId search term 
DECLARE
@SearchShipmentId VARCHAR(255) = '2025-01-06';

SELECT
    shipments.shipmentId,
    shipments.shipmentDate,
    lines.sku,
    lines.quantity
FROM dbo.markShipment AS shipments
         INNER JOIN dbo.markShipment_Line AS lines
                    ON shipments.shipmentId = lines.shipmentId
WHERE shipments.shipmentId LIKE CONCAT('%', @SearchShipmentId, '%');