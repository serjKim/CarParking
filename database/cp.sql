CREATE DATABASE CarParking
GO
USE CarParking
GO

CREATE TABLE [dbo].[Payment] (
    [PaymentID]  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [CreateDate] DATETIMEOFFSET NOT NULL
)
GO

CREATE TABLE [dbo].[ParkingStatus] (
    [ParkingStatusID] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Name]            NVARCHAR (100)   NOT NULL,
    CONSTRAINT ParkingStatus_U_Name UNIQUE ([Name])
)
GO
CREATE NONCLUSTERED INDEX [IX_ParkingStatus_Name]
    ON [dbo].[ParkingStatus]([Name]);

CREATE TABLE [dbo].[Tariff] (
    [TariffID] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Name]     NVARCHAR (100)   NOT NULL,
    CONSTRAINT Tariff_U_Name UNIQUE ([Name])
)
GO
CREATE NONCLUSTERED INDEX [IX_Tariff_Name]
    ON [dbo].[Tariff]([Name] ASC);

GO
CREATE TABLE [dbo].[Parking] (
    [ParkingID]    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ArrivalDate]  DATETIMEOFFSET   NOT NULL,
    [StatusID]     UNIQUEIDENTIFIER NOT NULL,
    [TariffID]     UNIQUEIDENTIFIER NOT NULL,
    [CompleteDate] DATETIMEOFFSET   NULL,
    [PaymentID]    UNIQUEIDENTIFIER NULL,
    CONSTRAINT [FK_Parking_ToParkingStatus] FOREIGN KEY ([StatusID]) REFERENCES [dbo].[ParkingStatus] ([ParkingStatusID]),
    CONSTRAINT [FK_Parking_ToPaymentID] FOREIGN KEY ([PaymentID]) REFERENCES [dbo].[Payment] ([PaymentID]),
    CONSTRAINT [FK_Parking_ToTariffID] FOREIGN KEY ([TariffID]) REFERENCES [dbo].[Tariff] ([TariffID])
)
GO
CREATE NONCLUSTERED INDEX [IX_Parking_StatusID]
    ON [dbo].[Parking]([StatusID]);

GO
CREATE TABLE [dbo].[Transition] (
    [TransitionID] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Name] NVARCHAR(100) NOT NULL, 
    [FromTariff] UNIQUEIDENTIFIER NULL, 
    [FromStatus] UNIQUEIDENTIFIER NULL, 
    [ToTariff] UNIQUEIDENTIFIER NOT NULL, 
    [ToStatus] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [FK_Transition_FromTariff] FOREIGN KEY ([FromTariff]) REFERENCES [dbo].[Tariff]([TariffID]),
    CONSTRAINT [FK_Transition_FromStatus] FOREIGN KEY ([FromStatus]) REFERENCES [dbo].[ParkingStatus]([ParkingStatusID]),
    CONSTRAINT [FK_Transition_ToTariff] FOREIGN KEY ([ToTariff]) REFERENCES [dbo].[Tariff]([TariffID]),
    CONSTRAINT [FK_Transition_ToStatus] FOREIGN KEY ([ToStatus]) REFERENCES [dbo].[ParkingStatus]([ParkingStatusID]),
    CONSTRAINT Transition_U_Name UNIQUE ([Name])
)

GO
insert into dbo.ParkingStatus (ParkingStatusID, Name)
values ('7F73FB8E-1D39-4D9D-84A6-69AFBFC81367', 'Started'), ('9DBD2A20-975D-47EF-9948-2CF6ECFC1583', 'Completed')
GO
insert into dbo.Tariff([TariffID],[Name])
values ('8FB74D7A-4BBC-46CF-BBAB-9F159C8508A8', 'Free'), ('7FE0F00B-035B-49CE-A34A-BCE59CD31921', 'First')
GO

insert into dbo.Transition (TransitionID, [Name], FromTariff, FromStatus, ToTariff, ToStatus)
values ('A3BD1995-FC10-4243-A89F-680ED9FDA9E7', 'StartedFree', NULL, NULL, '8FB74D7A-4BBC-46CF-BBAB-9F159C8508A8', '7F73FB8E-1D39-4D9D-84A6-69AFBFC81367'),
       ('B5988B00-656D-4BF7-95E0-7E122DED8053', 'CompletedFree', '8FB74D7A-4BBC-46CF-BBAB-9F159C8508A8', '7F73FB8E-1D39-4D9D-84A6-69AFBFC81367', '8FB74D7A-4BBC-46CF-BBAB-9F159C8508A8', '9DBD2A20-975D-47EF-9948-2CF6ECFC1583'),
       ('1217B3AA-C2BC-45AD-AA71-CB3C46E9ED2C', 'CompletedFirst', '8FB74D7A-4BBC-46CF-BBAB-9F159C8508A8', '7F73FB8E-1D39-4D9D-84A6-69AFBFC81367', '7FE0F00B-035B-49CE-A34A-BCE59CD31921', '9DBD2A20-975D-47EF-9948-2CF6ECFC1583')
GO
