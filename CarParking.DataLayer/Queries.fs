namespace CarParking.DataLayer

open DataContext
open Dto
open CarParking.Core
open CarParking.Core.Parking
open System.Threading.Tasks

module Queries =

    let queryParkingById (dctx: ISQLServerDataContext)
                         (parkingId: ParkingId) =
        let result =
            match Dto.mem.Get(parkingId.LongValue) with
            | Some dto -> 
                let parkingId = ParkingId.FromLong dto.Id
                match ParkingStatus.Parse dto.Status with
                | Ok status -> 
                    Some { Id = parkingId
                           Status = status 
                           ArrivalDate = dto.ArrivalDate}
                | Error _ -> 
                    None
            | None ->
                None

        result |> Task.FromResult
    