namespace CarParking.DataLayer.CmdDefs
{
    using System;
    using System.Data;
    using System.Threading;
    using Dapper;
    using static CarParking.DataLayer.Dto;

    public static class ParkingCmdDefs
    {
        public static (CommandDefinition, Func<ParkingDto, PaymentDto, ParkingDto>, string) ParkingById(Guid parkingId, CancellationToken token = default)
        {
            var sqlQuery = $@"
                {GetSqlQuery()}
                where p.ParkingID = @{nameof(parkingId)}
            ";

            var command = new CommandDefinition(sqlQuery, new { parkingId }, cancellationToken: token);

            static ParkingDto Mapping(ParkingDto parking, PaymentDto payment)
            {
                parking.Payment = payment;
                return parking;
            }

            var splitOn = nameof(PaymentDto.PaymentId);

            return (command, Mapping, splitOn);
        }

        public static (CommandDefinition, Func<ParkingDto, PaymentDto, ParkingDto>, string) AllParking(CancellationToken token = default)
        {
            var sqlQuery = GetSqlQuery();

            var command = new CommandDefinition(sqlQuery, cancellationToken: token);

            static ParkingDto Mapping(ParkingDto parking, PaymentDto payment)
            {
                parking.Payment = payment;
                return parking;
            }

            var splitOn = nameof(PaymentDto.PaymentId);

            return (command, Mapping, splitOn);
        }

        public static CommandDefinition InsertStartedFree(StartedFreeParkingDto dto, CancellationToken token = default)
        {
            var sqlQuery = $@"
                insert into dbo.Parking 
                    (ParkingID, StatusID, ArrivalDate, TariffID)
                values(
                    @{nameof(StartedFreeParkingDto.Id)},
                    (select ParkingStatusID from dbo.ParkingStatus where [Name] = 'Started'),
                    @{nameof(StartedFreeParkingDto.ArrivalDate)},
                    (select TariffID from dbo.Tariff where [Name] = 'Free'))
            ";

            return new CommandDefinition(sqlQuery, dto, cancellationToken: token);
        }

        public static CommandDefinition TransitionToCompletedFree(FreeParkingDto parkingDto, CancellationToken token = default)
        {
            var sqlQuery = $@"
                update dbo.Parking
                    set StatusID = (select ParkingStatusID from dbo.ParkingStatus where Name = 'Completed'),
                        CompleteDate = @{nameof(FreeParkingDto.CompleteDate)}
                where ParkingID = @{nameof(FreeParkingDto.Id)}
            ";

            return new CommandDefinition(sqlQuery, parkingDto, cancellationToken: token);
        }

        public static CommandDefinition TransitionToCompletedFirst(FirstParkingDto parkingDto, IDbTransaction tran, CancellationToken token = default)
        {
            var sqlQuery = $@"
                update dbo.Parking
                    set StatusID = (select ParkingStatusID from dbo.ParkingStatus where Name = 'Completed'),
                        TariffID = (select TariffID from dbo.Tariff where Name = 'First'),
                        CompleteDate = @{nameof(FirstParkingDto.CompleteDate)},
                        PaymentID = @{nameof(FirstParkingDto.PaymentId)}
                where ParkingID = @{nameof(FirstParkingDto.Id)}
            ";

            return new CommandDefinition(sqlQuery, parkingDto, transaction: tran, cancellationToken: token);
        }

        private static string GetSqlQuery() => $@"
            select 
	            p.ParkingID [{nameof(ParkingDto.Id)}],
	            p.ArrivalDate [{nameof(ParkingDto.ArrivalDate)}],
	            ps.Name [{nameof(ParkingDto.Status)}],
                p.CompleteDate [{nameof(ParkingDto.CompleteDate)}],
                t.Name [{nameof(ParkingDto.Tariff)}],
                pt.PaymentID [{nameof(PaymentDto.PaymentId)}],
                pt.CreateDate [{nameof(PaymentDto.CreateDate)}]
            from dbo.Parking p 
	        inner join dbo.ParkingStatus ps
		        on ps.ParkingStatusID = p.StatusID
            left join dbo.Payment pt
                on pt.PaymentID = p.PaymentID
            left join dbo.Tariff t
                on t.TariffID = p.TariffID
        ";
    }
}
