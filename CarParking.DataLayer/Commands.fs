namespace CarParking.DataLayer

module Commands =
    open Dto
    open System.Threading.Tasks
    open DataContext
    open CarParking.Core

    let insertParking (dctx: ISQLServerDataContext) status arrivalDate =
        let dto = mem.Add(status.ToString(), arrivalDate)
        let parkingId = ParkingId.FromLong dto.Id

        { Id = parkingId
          Status = status
          ArrivalDate = arrivalDate } |> Task.FromResult

    let updateParking (dctx: ISQLServerDataContext) parking =
        let id = parking.Id.LongValue
        match mem.Get(id) with
        | Some dto ->
            let newDto : ParkingDto =
                { Id = parking.Id.LongValue
                  Status = parking.Status.ToString()
                  ArrivalDate = parking.ArrivalDate }
            mem.Delete(dto) |> ignore
            mem.Append(newDto)
            Task.CompletedTask
        | None -> 
            Task.CompletedTask
