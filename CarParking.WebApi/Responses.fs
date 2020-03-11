namespace CarParking.WebApi

open CarParking.Core
open System

module Responses =
    [<CLIMutable; NoEquality; NoComparison>]
    type PaymentResponse =
        { Id: Guid 
          CreateDate: DateTime }
        static member FromPayment(x: Payment) =
            { Id = PaymentId.toGuid x.Id
              CreateDate = x.CreateDate }

    [<CLIMutable; NoEquality; NoComparison>]
    type ParkingResponse =
        { Id: Guid
          Type: string
          ArrivalDate: DateTime
          CompleteDate: Nullable<DateTime>
          Payment: PaymentResponse }
        static member FromParking(x: Parking) = 
            match x with
            | StartedFree prk ->
                { Id           = ParkingId.toGuid prk.Id
                  Type         = "StartedFree"
                  ArrivalDate  = prk.ArrivalDate
                  CompleteDate = Nullable()
                  Payment      = Unchecked.defaultof<PaymentResponse> }
            | CompletedFree prk ->
                { Id           = ParkingId.toGuid prk.Id
                  Type         = "CompletedFree"
                  ArrivalDate  = prk.ArrivalDate
                  CompleteDate = Nullable(prk.CompleteDate)
                  Payment      = Unchecked.defaultof<PaymentResponse> }
            | CompletedFirst prk ->
                { Id           = ParkingId.toGuid prk.Id
                  Type         = "CompletedFirst"
                  ArrivalDate  = prk.ArrivalDate
                  CompleteDate = Nullable(prk.CompleteDate)
                  Payment      = PaymentResponse.FromPayment prk.Payment }
    
    [<CLIMutable; NoEquality; NoComparison>]
    type ParkingResponseModel =
        { Parking: ParkingResponse }
        static member FromResponse x = { Parking = x }

    [<CLIMutable; NoEquality; NoComparison>]
    type ParkingsResponseModel =
        { Parkings: ParkingResponse list }
        static member FromResponse x = { Parkings = x }

    [<CLIMutable; NoEquality; NoComparison>]
    type TransitionErrorReponse = 
        { ErrorType: string }
