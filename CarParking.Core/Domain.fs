namespace CarParking.Core

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
    | StartedFreeParking of StartedFreeParking
    | CompletedFreeParking of CompletedFreeParking
    | CompletedFirstParking of CompletedFirstParking

[<RequireQualifiedAccess>]
module ParkingStatus =
    let parse str =
        let parseStatus status = 
            String.Equals 
                (status.ToString(), str, 
                StringComparison.Ordinal)

        if parseStatus Started then Ok Started
        elif parseStatus Completed then Ok Completed
        else Error (sprintf "Couldn't parse %s status" str)

[<RequireQualifiedAccess>]
module ParkingId =
    let parse (str: string) = 
        match Guid.TryParse str with
        | true, result -> Ok (ParkingId result)
        | false, _ -> Error (sprintf "Couldn't parse %s parkingId" str)

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
        else Error (sprintf "Couldn't parse %s tariff" str)

module Parking =

    let freeInterval = new TimeSpan (0, 10, 0)

    let (|FirstTariff|_|) (prk: StartedFreeParking, date: DateTime) =
        let diff = prk.ArrivalDate - date
        if Math.Abs(diff.TotalMinutes) > freeInterval.TotalMinutes then
            Some First
        else
            None

    let calculateTariff prk date =
        match prk,date with
        | FirstTariff t -> t
        | _ -> Free

    let transitionToCompletedFree prk completeDate =
        match calculateTariff prk completeDate with
        | Free ->
            Ok { Id = prk.Id
                 ArrivalDate = prk.ArrivalDate 
                 CompleteDate = completeDate }
        | First ->
            Error "Free was expired"

    let transitionToCompletedFirst prk (completeDate, paymentId) =
        match calculateTariff prk completeDate with
        | Free ->
            Error "Payment is not applicable for Free tariff"
        | First ->
            { Id = prk.Id
              ArrivalDate = prk.ArrivalDate 
              CompleteDate = completeDate
              Payment =
                { Id = paymentId
                  CreateDate = completeDate}} |> Ok