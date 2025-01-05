SELECT
    shipments.shipmentId,
    shipments.shipmentDate,
    lines.sku,
    lines.quantity
FROM dbo.markShipment AS shipments
         INNER JOIN dbo.markShipment_Line AS lines
                    ON shipments.shipmentId = lines.shipmentId
WHERE shipments.shipmentId LIKE '%2025-01-06%';