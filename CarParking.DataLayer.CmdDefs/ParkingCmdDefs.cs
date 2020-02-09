namespace CarParking.DataLayer.CmdDefs
{
    using System;
    using System.Threading;
    using Dapper;
    using static CarParking.DataLayer.Dto;

    public static class ParkingCmdDefs
    {
        public static CommandDefinition ParkingById(long parkingId, CancellationToken token = default)
        {
            var sqlQuery = $@"
                {GetSqlQuery()}
                where p.ParkingID = @{nameof(parkingId)}
            ";

            return new CommandDefinition(sqlQuery, new { parkingId }, cancellationToken: token);
        }

        public static CommandDefinition AllParking(CancellationToken token = default)
        {
            var sqlQuery = GetSqlQuery();

            return new CommandDefinition(sqlQuery, cancellationToken: token);
        }

        public static CommandDefinition InsertParking(string statusName, DateTime arrivalDate, CancellationToken token = default)
        {
            var sqlQuery = $@"
                insert into dbo.Parking 
                    (StatusID, ArrivalDate)
                select 
                    ParkingStatusID,
                    @{nameof(arrivalDate)}    
                from dbo.ParkingStatus 
                where Name = @{nameof(statusName)}

                select SCOPE_IDENTITY()
            ";

            return new CommandDefinition(sqlQuery, new { statusName, arrivalDate }, cancellationToken: token);
        }

        public static CommandDefinition UpdateParking(ParkingDto parkingDto, CancellationToken token = default)
        {
            var sqlQuery = $@"
                update dbo.Parking
                    set StatusID = (select ParkingStatusID from dbo.ParkingStatus where Name = @{nameof(ParkingDto.Status)}),
                        ArrivalDate = @{nameof(ParkingDto.ArrivalDate)}
                where ParkingID = @{nameof(ParkingDto.Id)}
            ";

            return new CommandDefinition(sqlQuery, parkingDto, cancellationToken: token);
        }

        private static string GetSqlQuery() => $@"
            select 
	            p.ParkingID [{nameof(ParkingDto.Id)}],
	            p.ArrivalDate [{nameof(ParkingDto.ArrivalDate)}],
	            ps.Name [{nameof(ParkingDto.Status)}]
            from dbo.Parking p 
	            inner join dbo.ParkingStatus ps
		            on ps.ParkingStatusID = p.StatusID
        ";
    }
}
