
--insert into dbo.ParkingStatus values ('Started'), ('Complete')
--insert into dbo.Tariff([Name]) values ('Free'), ('First')

alter table dbo.Parking add ChargeID bigint

select * from dbo.ParkingStatus
select * from dbo.Parking where ParkingID =3
select * from dbo.Tariff

update 

insert into dbo.Parking (StatusID, ArrivalDate)
	values (1, GETDATE())

select 
	p.ParkingID,
	p.ArrivalDate,
	p.CompleteDate,
	ps.Name
from dbo.Parking p 
	inner join dbo.ParkingStatus ps
		on ps.ParkingStatusID = p.StatusID
where p.ParkingID = 3