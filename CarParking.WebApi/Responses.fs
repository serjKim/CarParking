namespace CarParking.WebApi

open CarParking.Core
open System

module Responses =

    type ParkingResponse =
        { Id: int64
          Status: string
          ArrivalDate: DateTime }
        static member FromParking(x: Parking) = 
            { Id = x.Id.LongValue
              Status = x.Status.ToString()
              ArrivalDate = x.ArrivalDate }


            
