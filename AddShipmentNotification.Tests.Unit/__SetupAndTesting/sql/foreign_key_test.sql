/*
 This INSERT should fail because 'InvalidShipmentId' cannot be cross-referenced in the
 [dbo].[markShipment] table (as it hasn't been inserted yet)
 */
INSERT INTO [dbo].[markShipment_Line] (shipmentId, sku, quantity)
VALUES ('InvalidShipmentId', 'Sku001', 1);