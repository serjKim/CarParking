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
            | StartedFreeParking prk ->
                { Id           = ParkingId.toGuid prk.Id
                  Type         = "StartedFreeParking"
                  ArrivalDate  = prk.ArrivalDate
                  CompleteDate = Nullable()
                  Payment      = Unchecked.defaultof<PaymentResponse> }
            | CompletedFreeParking prk ->
                { Id           = ParkingId.toGuid prk.Id
                  Type         = "CompletedFreeParking"
                  ArrivalDate  = prk.ArrivalDate
                  CompleteDate = Nullable(prk.CompleteDate)
                  Payment      = Unchecked.defaultof<PaymentResponse> }
            | CompletedFirstParking prk ->
                { Id           = ParkingId.toGuid prk.Id
                  Type         = "CompletedFirstParking"
                  ArrivalDate  = prk.ArrivalDate
                  CompleteDate = Nullable(prk.CompleteDate)
                  Payment      = PaymentResponse.FromPayment prk.Payment }                
            
