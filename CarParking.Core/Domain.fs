namespace CarParking.Core

open CarParking.Error
open System

type ParkingStatus =
    | Started
    | Completed

type ParkingId = ParkingId of Guid
type PaymentId = PaymentId of Guid

type Payment = 
    { Id: PaymentId
      CreateDate: DateTime }

type Tariff =
    | Free
    | First

type StartedFreeParking = 
    { Id: ParkingId
      ArrivalDate: DateTime }

type CompletedFreeParking =    
    { Id: ParkingId
      ArrivalDate: DateTime
      CompleteDate: DateTime }

type CompletedFirstParking = 
    { Id: ParkingId
      ArrivalDate: DateTime
      CompleteDate: DateTime
      Payment: Payment }

type Parking =
    | StartedFree of StartedFreeParking
    | CompletedFree of CompletedFreeParking
    | CompletedFirst of CompletedFirstParking

[<RequireQualifiedAccess>]
module ParkingStatus =
    let parse str =
        let parseStatus status = 
            String.Equals 
                (status.ToString(), str, 
                StringComparison.Ordinal)

        if parseStatus Started then Ok Started
        elif parseStatus Completed then Ok Completed
        else Error <| BadInput (sprintf "Couldn't parse %s status" str)

[<RequireQualifiedAccess>]
module ParkingId =
    let parse (str: string) = 
        match Guid.TryParse str with
        | true, result -> Ok (ParkingId result)
        | false, _ -> Error <| BadInput (sprintf "Couldn't parse %s parkingId" str)

    let toGuid = function
        | ParkingId x -> x

[<RequireQualifiedAccess>]
module PaymentId =
    let toGuid = function
        | PaymentId x -> x

[<RequireQualifiedAccess>]
module Tariff =
    let parse str =
        let parseTariff tariff = 
            String.Equals 
                (tariff.ToString(), str, 
                StringComparison.Ordinal)

        if parseTariff Free then Ok Free
        elif parseTariff First then Ok First
        else Error <| BadInput (sprintf "Couldn't parse %s tariff" str)

module Parking =
    let (|FirstTariff|_|) (freeLimit: TimeSpan) (prk: StartedFreeParking, date: DateTime) =
        let diff = prk.ArrivalDate - date
        if Math.Abs(diff.TotalMinutes) > freeLimit.TotalMinutes then
            Some First
        else
            None

    let calculateTariff freeLimit prk date =
        match prk,date with
        | FirstTariff freeLimit t -> t
        | _ -> Free

    [<RequireQualifiedAccess>]
    module Transitions =
        let toCompletedFree freeLimit prk completeDate =
            match calculateTariff freeLimit prk completeDate with
            | Free ->
                Ok { Id = prk.Id
                     ArrivalDate = prk.ArrivalDate 
                     CompleteDate = completeDate }
            | First ->
                Error <| TransitionError FreeExpired

        let toCompletedFirst freeLimit prk payment =
            match calculateTariff freeLimit prk payment.CreateDate with
            | Free ->
                Error <| TransitionError PaymentNotApplicable
            | First ->
                Ok { Id = prk.Id
                     ArrivalDate = prk.ArrivalDate 
                     CompleteDate = payment.CreateDate
                     Payment = payment }