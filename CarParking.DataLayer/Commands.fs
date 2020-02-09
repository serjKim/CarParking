namespace CarParking.DataLayer

module Commands =
    open Mapping
    open CarParking.Core
    open DataContext
    open CarParking.DataLayer.CmdDefs
    open FSharp.Control.Tasks.V2
    open Dapper

    let insertParking (dctx, token) status arrivalDate =
        let statusName = ParkingStatus.toString status
        let conn = getConn dctx
        let cmd = ParkingCmdDefs.InsertParking(statusName, arrivalDate, token)
        task {
            let! newId = conn.QuerySingleOrDefaultAsync<int64>(cmd)
            return ParkingId newId
        }

    let updateParking (dctx, token) parking =
        let conn = getConn dctx
        let dto = toParkingDto parking
        let cmd = ParkingCmdDefs.UpdateParking(dto, token)
        task {
            let! rows = conn.ExecuteAsync(cmd)
            return (rows > 0)            
        }
