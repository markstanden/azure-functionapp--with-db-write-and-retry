/*
 Drops existing tables and creates new, ready for use.
 The use of a foreign key constraint prevents records being
 created in the markShipment_Line table without a matching
 shipmentId in the markShipment table.
 It also prevents records being deleted from the markShipment table if
 referenced shipmentIds are present.
 */

DROP TABLE IF EXISTS [dbo].[markShipment_Line];
DROP TABLE IF EXISTS [dbo].[markShipment];

/* Create the shipment table, I've used the shipmentId as the primary key here.
   I made this choice as it will allow for a duplicate primary key sql insert error to be generated easily */
CREATE TABLE [dbo].[markShipment]
(
    [shipmentId] VARCHAR(255) NOT NULL PRIMARY KEY,
    [shipmentDate] DATETIME NOT NULL
);

/* Create the shipment_lines table, I've created a sequential primary key here, as 
   I couldn't be 100% sure that a composite key made of shipmentId and Sku would be unique */
CREATE TABLE [dbo].[markShipment_Line]
(
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [sku] VARCHAR(255) NOT NULL,
    [quantity] INT NOT NULL,
    [shipmentId] VARCHAR(255) NOT NULL,
    FOREIGN KEY ([shipmentId]) REFERENCES [dbo].[markShipment] ([shipmentId])
);