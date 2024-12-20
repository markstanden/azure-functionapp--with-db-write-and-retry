SELECT
    shipments.shipmentId,
    shipments.shipmentDate,
    lines.sku,
    lines.quantity
FROM
    (SELECT * FROM dbo.markShipment) AS shipments
        INNER JOIN
    (SELECT * FROM dbo.markShipment_Line) AS lines
    ON shipments.shipmentId = lines.shipmentId;