namespace CarParking.DataLayer

open System

module Dto =
    [<CLIMutable; NoEquality; NoComparison>]
    type ParkingDto = 
        { Id: int64 
          Status: string
          ArrivalDate: DateTime
          CompleteDate: Nullable<DateTime> }