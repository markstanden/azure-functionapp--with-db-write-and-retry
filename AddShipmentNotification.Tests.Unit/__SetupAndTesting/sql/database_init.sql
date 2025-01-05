DROP TABLE IF EXISTS [dbo].[markShipment];
DROP TABLE IF EXISTS [dbo].[markShipment_Line];

CREATE TABLE [dbo].[markShipment]
(
    [shipmentId] VARCHAR(255) NOT NULL PRIMARY KEY,
    [shipmentDate] DATETIME NOT NULL
);

CREATE TABLE [dbo].[markShipment_Line]
(
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [sku] VARCHAR(255) NOT NULL,
    [quantity] INT NOT NULL,
    [shipmentId] VARCHAR(255) NOT NULL,
    FOREIGN KEY ([shipmentId]) REFERENCES [dbo].[markShipment] ([shipmentId])
);