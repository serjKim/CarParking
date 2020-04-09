namespace CarParking.Core

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
      CreateDate: DateTime }

type Tariff =
    | Free
    | First

type ParkingInterval = private ParkingInterval of arrival: DateTime * complete: DateTime

type PaidInterval =
    private { Interval: ParkingInterval
              Payment: Payment }

type StartedFreeParking = 
    { Id: ParkingId
      ArrivalDate: DateTime }

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

    let toString = function 
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

    let toString = function 
        | Free -> FreeName
        | First -> FirstName

[<RequireQualifiedAccess>]
module ParkingInterval =
    let private checkInterval (arrival: DateTime, complete: DateTime) =
        if complete < arrival then 
            Error <| BadInput ("Complete date can't be less than the arrival date")
        else
            Ok (arrival, complete)

    let create dates =
        result {
            let! validDates = checkInterval dates
            return ParkingInterval validDates
        }

    let diff (ParkingInterval (arrival, complete)) = complete - arrival
    let getDates (ParkingInterval(a, c)) = (a, c)
    let getArrivalDate (ParkingInterval (a, _)) = a
    let getCompleteDate (ParkingInterval (_, c)) = c

[<RequireQualifiedAccess>]
module PaidInterval =
    let private checkDates (interval, payment) =
        let completeDate = ParkingInterval.getCompleteDate interval
        if completeDate = payment.CreateDate then
            Ok (interval, payment)
        else
            Error <| BadInput (sprintf "Complete date and payment's create date must be equal. Payment = %A" payment)

    let create interval payment =
        result {
            let! (i, p) = checkDates (interval, payment)
            return { Interval = i
                     Payment = p }
        }

    let getInterval (x: PaidInterval) = x.Interval
    let getPayment (x: PaidInterval) = x.Payment

module Parking =
    let private (|FirstTariff|_|) (freeLimit: TimeSpan) interval =
        let diff = ParkingInterval.diff interval
        if diff.TotalMinutes > freeLimit.TotalMinutes then
            Some First
        else
            None

    let calculateTariff freeLimit interval =
        match interval with
        | FirstTariff freeLimit t -> t
        | _ -> Free

    [<RequireQualifiedAccess>]
    module Transitions =
        let toCompletedFree freeLimit prk completeDate =
            result {
                let! interval = ParkingInterval.create (prk.ArrivalDate, completeDate)
                match calculateTariff freeLimit interval with
                | Free ->
                    return! Ok { Id = prk.Id
                                 Interval = interval }
                | First ->
                    return! Error <| TransitionError FreeExpired
            }

        let toCompletedFirst freeLimit prk payment =
            result {
                let! interval = ParkingInterval.create (prk.ArrivalDate, payment.CreateDate)
                let! paidInterval = PaidInterval.create interval payment
                match calculateTariff freeLimit interval with
                | Free ->
                    return! Error <| TransitionError PaymentNotApplicable
                | First ->
                    return! Ok { Id = prk.Id
                                 PaidInterval = paidInterval }
            }
