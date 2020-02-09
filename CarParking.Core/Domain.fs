namespace CarParking.Core

open System

type ParkingStatus =
    | Started
    | Complete

type ParkingId = ParkingId of int64
type ChargeId = ChargeId of int64

type Charge = 
    { Id: ChargeId
      CreateDate: DateTime }

type Parking =
    { Id: ParkingId
      Status: ParkingStatus 
      ArrivalDate: DateTime }

type Tariff =
    | Free
    | First

type CompleteError =
    | FreeExpired of string

[<RequireQualifiedAccess>]
module ParkingStatus =
    let parse str =
        let parseStatus status = 
            String.Equals 
                (status.ToString(), str, 
                StringComparison.InvariantCultureIgnoreCase)

        if parseStatus Started then Ok Started
        elif parseStatus Complete then Ok Complete
        else Error (sprintf "Couldn't parse %s status" str)

    let toString (status: ParkingStatus) = status.ToString()

[<RequireQualifiedAccess>]
module ParkingId =
    let parse (str: string) = 
        match Int64.TryParse str with
        | true, result -> Ok (ParkingId result)
        | false, _ -> Error (sprintf "Couldn't parse %s parkingId" str)

    let toLong = function
        | ParkingId x -> x

module Parking =

    let freeInterval = new TimeSpan (0, 10, 0)

    let (|FirstTariff|_|) (prk,date: DateTime) =
        let diff = prk.ArrivalDate - date
        if Math.Abs(diff.TotalMinutes) > freeInterval.TotalMinutes then
            Some First
        else
            None

    let calculateTariff prk date =
        match prk,date with
        | FirstTariff t -> t
        | _ -> Free

    let tryCompleteParking prk completeDate =
        let tariff = calculateTariff prk completeDate
        match tariff with
        | Free -> Ok { prk with Status = Complete }
        | First -> Error (FreeExpired "Free tariff was expired. You have to pay the First tariff")
