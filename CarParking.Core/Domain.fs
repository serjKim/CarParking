﻿namespace CarParking.Core

open CarParking.Error
open System
open FsToolkit.ErrorHandling

type ParkingStatus =
    | Started
    | Completed

type ParkingId = ParkingId of Guid
type PaymentId = PaymentId of Guid

type Payment = 
    { Id: PaymentId
      CreateDate: DateTimeOffset }

type Tariff =
    | Free
    | First

type ParkingInterval = private ParkingInterval of arrival: DateTimeOffset * complete: DateTimeOffset

type PaidInterval =
    private { Interval: ParkingInterval
              Payment: Payment }

type StartedFreeParking = 
    { Id: ParkingId
      ArrivalDate: DateTimeOffset }

type CompletedFreeParking =    
    { Id: ParkingId
      Interval: ParkingInterval }

type CompletedFirstParking = 
    { Id: ParkingId
      PaidInterval: PaidInterval }

type Parking =
    | StartedFree of StartedFreeParking
    | CompletedFree of CompletedFreeParking
    | CompletedFirst of CompletedFirstParking

type TransitionName = string

type Transition = 
    { Name: TransitionName 
      FromTariff: Tariff option
      FromStatus: ParkingStatus option
      ToTariff: Tariff
      ToStatus: ParkingStatus }

[<RequireQualifiedAccess>]
module ParkingStatus =
    [<Literal>]
    let StartedName = "Started"

    [<Literal>]
    let CompletedName = "Completed"

    let parse str =
        let parseStatus status = 
            String.Equals 
                (status, str, 
                StringComparison.Ordinal)

        if parseStatus StartedName then Ok Started
        elif parseStatus CompletedName then Ok Completed
        else Error <| BadInput (sprintf "Couldn't parse %s status" str)

    let toConstant = function 
        | Started -> StartedName
        | Completed -> CompletedName

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
    [<Literal>]
    let FreeName = "Free"

    [<Literal>]
    let FirstName = "First"

    let parse str =
        let parseTariff tariff = 
            String.Equals 
                (tariff, str, 
                StringComparison.Ordinal)

        if parseTariff FreeName then Ok Free
        elif parseTariff FirstName then Ok First
        else Error <| BadInput (sprintf "Couldn't parse %s tariff" str)

    let toConstant = function 
        | Free -> FreeName
        | First -> FirstName

module ParkingInterval =
    let private checkInterval (arrival: DateTimeOffset, complete: DateTimeOffset) =
        if complete < arrival then 
            Error <| BadInput ("Complete date can't be less than the arrival date")
        else
            Ok (arrival, complete)

    let createInterval = checkInterval >> Result.map ParkingInterval
    let duration (ParkingInterval (arrival, complete)) = complete - arrival
    let getArrivalDate (ParkingInterval (a, _)) = a
    let getCompleteDate (ParkingInterval (_, c)) = c

    type ParkingInterval with
        member x.ArrivalDate = getArrivalDate x
        member x.CompleteDate = getCompleteDate x

[<RequireQualifiedAccess>]
module PaidInterval =
    let private checkDates (interval, payment) =
        let completeDate = ParkingInterval.getCompleteDate interval
        if completeDate = payment.CreateDate then
            Ok (interval, payment)
        else
            Error <| BadInput (sprintf "Complete date and payment's create date must be equal. Payment = %A" payment)

    let private createPayment (interval, payment) =
        { Interval = interval
          Payment = payment }

    let create = checkDates >> Result.map createPayment
    let getInterval (x: PaidInterval) = x.Interval
    let getPayment (x: PaidInterval) = x.Payment

module Parking =
    open ParkingInterval

    let private (|FirstTariff|_|) (freeLimit: TimeSpan) interval =
        let diff = duration interval
        if diff.TotalMinutes > freeLimit.TotalMinutes then
            Some First
        else
            None

    let calculateTariff freeLimit interval =
        match interval with
        | FirstTariff freeLimit t -> t
        | _ -> Free


    [<Literal>]
    let StartedFreeName = "StartedFree"

    [<Literal>]
    let CompletedFreeName = "CompletedFree"

    [<Literal>]
    let CompletedFirstName = "CompletedFirst"

    let toConstant = function 
        | StartedFree _ -> StartedFreeName
        | CompletedFree _ -> CompletedFreeName
        | CompletedFirst _ -> CompletedFirstName

    [<RequireQualifiedAccess>]
    module Transitions =
        let toCompletedFree freeLimit completeDate prk =
            result {
                let! interval = createInterval (prk.ArrivalDate, completeDate)
                match calculateTariff freeLimit interval with
                | Free ->
                    return! Ok { Id = prk.Id
                                 Interval = interval }
                | First ->
                    return! Error <| TransitionError FreeExpired
            }

        let toCompletedFirst freeLimit payment prk =
            result {
                let! interval = createInterval (prk.ArrivalDate, payment.CreateDate)
                let! paidInterval = PaidInterval.create (interval, payment)
                match calculateTariff freeLimit interval with
                | Free ->
                    return! Error <| TransitionError PaymentNotApplicable
                | First ->
                    return! Ok { Id = prk.Id
                                 PaidInterval = paidInterval }
            }
