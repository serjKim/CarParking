namespace CarParking.Core

open System

type ParkingStatus =
    | Started
    | Complete
    static member Parse(str) =
        let parseStatus status = 
            String.Equals 
                (status.ToString(), str, 
                StringComparison.InvariantCultureIgnoreCase)

        if parseStatus Started then Ok Started
        elif parseStatus Complete then Ok Complete
        else Error (sprintf "Couldn't parse %s status" str)

type ParkingId =
    private | ParkingId of int64

    static member Parse(str: string) = 
        match Int64.TryParse str with
        | true, result -> Ok (ParkingId result)
        | false, _ -> Error (sprintf "Couldn't parse %s parkingId" str)

    static member FromLong(x) = ParkingId x

    member this.LongValue = match this with | ParkingId x -> x

type Parking =
    { Id: ParkingId
      Status: ParkingStatus 
      ArrivalDate: DateTime }

type Tariff =
    | Free
    | First

type CompleteError =
    | FreeExpired of string

module Parking =

    let freeInterval = new TimeSpan (0, 10, 0)

    let (|FirstTariff|_|) (prk,date) =
        if prk.ArrivalDate - date > freeInterval then 
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
    
