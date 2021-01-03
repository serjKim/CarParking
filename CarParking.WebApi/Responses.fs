namespace CarParking.WebApi

open CarParking.Core
open CarParking.Core.ParkingInterval
open CarParking.Utils
open System

module Responses =
    [<CLIMutable; NoEquality; NoComparison>]
    type PaymentResponse =
        { Id: Guid 
          CreateDate: DateTimeOffset }
        static member FromPayment(x: Payment) =
            { Id = PaymentId.toGuid x.Id
              CreateDate = x.CreateDate }
        static member Null = 
            Unchecked.defaultof<PaymentResponse>

    [<CLIMutable; NoEquality; NoComparison>]
    type ParkingResponse =
        { Id: Guid
          Type: string
          ArrivalDate: DateTimeOffset
          CompleteDate: Nullable<DateTimeOffset>
          Payment: PaymentResponse }
        static member FromParking(x: Parking) = 
            let parkingType = ClientConstants.Parking.ofParking x
            match x with
            | StartedFree prk ->
                { Id           = ParkingId.toGuid prk.Id
                  Type         = parkingType
                  ArrivalDate  = prk.ArrivalDate
                  CompleteDate = Nullable()
                  Payment      = PaymentResponse.Null }
            | CompletedFree prk ->
                { Id           = ParkingId.toGuid prk.Id
                  Type         = parkingType
                  ArrivalDate  = prk.Interval.ArrivalDate
                  CompleteDate = Nullable(prk.Interval.CompleteDate)
                  Payment      = PaymentResponse.Null }
            | CompletedFirst prk ->
                let interval = prk.PaidInterval |> PaidInterval.getInterval
                { Id           = ParkingId.toGuid prk.Id
                  Type         = parkingType
                  ArrivalDate  = interval.ArrivalDate
                  CompleteDate = Nullable(interval.CompleteDate)
                  Payment      = prk.PaidInterval |> PaidInterval.getPayment |> PaymentResponse.FromPayment }

    [<CLIMutable; NoEquality; NoComparison>]
    type TransitionErrorReponse = 
        { ErrorType: string }

    [<CLIMutable; NoEquality; NoComparison>]
    type TransitionResponse =
        { Name: string
          FromTariff: string 
          FromStatus: string 
          ToTariff: string
          ToStatus: string }
        static member FromTransition (transition: Transition) =
            { Name = transition.Name
              FromTariff = transition.FromTariff.MapOrDefault Tariff.toString
              FromStatus = transition.FromStatus.MapOrDefault ParkingStatus.toString
              ToTariff = transition.ToTariff |> Tariff.toString
              ToStatus = transition.ToStatus |> ParkingStatus.toString }
