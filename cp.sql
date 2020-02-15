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
--insert into dbo.ParkingStatus (ParkingStatusID, Name) values (newid(), 'Started'), (newid(), 'Completed')
--insert into dbo.Tariff([TariffID],[Name]) values (newid(), 'Free'), (newid(), 'First')

-- delete from dbo.Parking
-- delete from dbo.ParkingStatus
-- delete from dbo.Payment
-- delete from dbo.Tariff

select * from dbo.ParkingStatus
select * from dbo.Parking where ParkingID =3
select * from dbo.Tariff
select * from dbo.Payment

--create SEQUENCE dbo.ParkingSeq  
--    START WITH 8  
--    INCREMENT BY 1 ; 

--CREATE TABLE [MyTable]
--(
--    [ID] [bigint] PRIMARY KEY NOT NULL DEFAULT (NEXT VALUE FOR dbo.ParkingSeq),
--    [Title] [nvarchar](64) NOT NULL
--);

insert into dbo.Parking 
    (ParkingID, StatusID, ArrivalDate, TariffID)
values
    (newid(),
    (select ParkingStatusID from dbo.ParkingStatus where [Name] = 'Started'),
    getdate(),
    (select TariffID from dbo.Tariff where [Name] = 'Free'))
