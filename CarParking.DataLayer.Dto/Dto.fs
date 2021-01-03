namespace CarParking.DataLayer

open System

module Dto =
    [<CLIMutable; NoEquality; NoComparison>]
    type PaymentDto =
        { PaymentId: Guid 
          CreateDate: DateTimeOffset }

    [<CLIMutable; NoEquality; NoComparison>]
    type ParkingDto = 
        { Id: Guid 
          Status: string
          ArrivalDate: DateTimeOffset
          CompleteDate: Nullable<DateTimeOffset>
          Tariff: string
          mutable Payment: PaymentDto }

    [<CLIMutable; NoEquality; NoComparison>]
    type StartedFreeParkingDto = 
        { Id: Guid 
          ArrivalDate: DateTimeOffset }

    [<CLIMutable; NoEquality; NoComparison>]
    type CompletedFreeParkingDto = 
        { Id: Guid 
          CompleteDate: DateTimeOffset }

    [<CLIMutable; NoEquality; NoComparison>]
    type CompletedFirstParkingDto = 
        { Id: Guid 
          CompleteDate: DateTimeOffset
          Payment: PaymentDto }
        member x.PaymentId = x.Payment.PaymentId

    [<CLIMutable; NoEquality; NoComparison>]
    type TransitionDto =
        { Name: string 
          FromTariff: string 
          FromStatus: string 
          ToTariff: string
          ToStatus: string }
