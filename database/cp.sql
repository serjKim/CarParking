CREATE DATABASE CarParking
GO
USE CarParking
GO

CREATE TABLE [dbo].[Payment] (
    [PaymentID]  UNIQUEIDENTIFIER NOT NULL,
    [CreateDate] DATETIME2 (7)    NOT NULL
    PRIMARY KEY CLUSTERED ([PaymentID])
);
GO


CREATE TABLE [dbo].[ParkingStatus] (
    [ParkingStatusID] UNIQUEIDENTIFIER NOT NULL,
    [Name]            NVARCHAR (100)   NOT NULL,
    PRIMARY KEY CLUSTERED ([ParkingStatusID])
);
GO
CREATE NONCLUSTERED INDEX [IX_ParkingStatus_Name]
    ON [dbo].[ParkingStatus]([Name]);


CREATE TABLE [dbo].[Tariff] (
    [TariffID] UNIQUEIDENTIFIER NOT NULL,
    [Name]     NVARCHAR (100)   NOT NULL,
    PRIMARY KEY CLUSTERED ([TariffID] ASC)
);
GO
CREATE NONCLUSTERED INDEX [IX_Tariff_Name]
    ON [dbo].[Tariff]([Name] ASC);


GO
CREATE TABLE [dbo].[Parking] (
    [ParkingID]    UNIQUEIDENTIFIER NOT NULL,
    [ArrivalDate]  DATETIME2 (7)    NOT NULL,
    [StatusID]     UNIQUEIDENTIFIER NOT NULL,
    [TariffID]     UNIQUEIDENTIFIER NOT NULL,
    [CompleteDate] DATETIME         NULL,
    [PaymentID]    UNIQUEIDENTIFIER NULL,
    PRIMARY KEY CLUSTERED ([ParkingID]),
    CONSTRAINT [FK_Parking_ToParkingStatus] FOREIGN KEY ([StatusID]) REFERENCES [dbo].[ParkingStatus] ([ParkingStatusID]),
    CONSTRAINT [FK_Parking_ToPaymentID] FOREIGN KEY ([PaymentID]) REFERENCES [dbo].[Payment] ([PaymentID]),
    CONSTRAINT [FK_Parking_ToTariffID] FOREIGN KEY ([TariffID]) REFERENCES [dbo].[Tariff] ([TariffID])
);
GO
CREATE NONCLUSTERED INDEX [IX_Parking_StatusID]
    ON [dbo].[Parking]([StatusID]);


GO
insert into dbo.ParkingStatus (ParkingStatusID, Name)
values ('7F73FB8E-1D39-4D9D-84A6-69AFBFC81367', 'Started'), ('9DBD2A20-975D-47EF-9948-2CF6ECFC1583', 'Completed')
GO
insert into dbo.Tariff([TariffID],[Name])
values ('8FB74D7A-4BBC-46CF-BBAB-9F159C8508A8', 'Free'), ('7FE0F00B-035B-49CE-A34A-BCE59CD31921', 'First')
GO