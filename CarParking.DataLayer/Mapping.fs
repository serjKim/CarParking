namespace CarParking.DataLayer

open System

module internal Mapping =
    open CarParking.Core
    open CarParking.DataLayer.Dto

    let toParking dto =
        if (isNull (box dto)) then
            None
        else
            match ParkingStatus.parse dto.Status with
            | Ok status ->
                Some { 
                    Id = ParkingId (dto.Id)
                    Status = status
                    ArrivalDate = dto.ArrivalDate }
            | Error _ -> 
                None

    let toParkingDto (prk: Parking) =
        { Id = ParkingId.toLong prk.Id
          Status = ParkingStatus.toString prk.Status
          ArrivalDate = prk.ArrivalDate
          CompleteDate = Nullable<DateTime>() }