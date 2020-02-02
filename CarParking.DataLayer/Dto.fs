namespace CarParking.DataLayer

open System

module Dto =
    type ParkingDto = 
        { Id: int64 
          Status: string
          ArrivalDate: DateTime }

    type InMemoryParkings() =
        let mutable id = 0L
        let mutable parkings = new System.Collections.Generic.List<ParkingDto>() 

        member _.Add(status, arrivalDate) = 
            id <- id + 1L
            let newDto = {
                Id = id
                Status = status
                ArrivalDate = arrivalDate }
            parkings.Add(newDto)
            newDto

        member _.Get(id) =
            let dto = parkings.Find(fun x -> x.Id = id)
            if isNull (box dto) then
                None
            else
                Some dto

        member _.Delete(dto) =
            parkings.Remove(dto)

        member _.Append(dto) =
            parkings.Add(dto)

    let mem = new InMemoryParkings()