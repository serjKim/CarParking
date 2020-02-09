namespace CarParking.WebApi

open CarParking.Core
open System

module Responses =

    type ParkingResponse =
        { Id: int64
          Status: string
          ArrivalDate: DateTime }
        static member FromParking(x: Parking) = 
            { Id = ParkingId.toLong x.Id
              Status = ParkingStatus.toString x.Status
              ArrivalDate = x.ArrivalDate }


            
