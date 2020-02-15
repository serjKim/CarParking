namespace CarParking.DataLayer

open System

module Dto =
    [<CLIMutable; NoEquality; NoComparison>]
    type PaymentDto =
        { PaymentId: Guid 
          CreateDate: DateTime }

    [<CLIMutable; NoEquality; NoComparison>]
    type ParkingDto = 
        { Id: Guid 
          Status: string
          ArrivalDate: DateTime
          CompleteDate: Nullable<DateTime>
          Tariff: string
          Payment: PaymentDto }

    [<CLIMutable; NoEquality; NoComparison>]
    type StartedFreeParkingDto = 
        { Id: Guid 
          ArrivalDate: DateTime }

    [<CLIMutable; NoEquality; NoComparison>]
    type FreeParkingDto = 
        { Id: Guid 
          CompleteDate: DateTime }

    [<CLIMutable; NoEquality; NoComparison>]
    type FirstParkingDto = 
        { Id: Guid 
          CompleteDate: DateTime
          Payment: PaymentDto }
        member x.PaymentId = x.Payment.PaymentId