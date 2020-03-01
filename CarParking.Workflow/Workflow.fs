namespace CarParking.Workflow

open System
open FSharp.Control.Tasks.V2.ContextInsensitive
open CarParking.Error
open CarParking.Core
open CarParking.Core.Parking
open CarParking.DataLayer.Queries
open CarParking.DataLayer
open FsToolkit.ErrorHandling

module Parking =

    let createNewParking dctx arrivalDate =
        task {
            let parking = 
                { Id = ParkingId (Guid.NewGuid())
                  ArrivalDate = arrivalDate }
            let! _ = Commands.insertStartedFree dctx parking
            return StartedFree parking
        }

    let getAllParkings dctx =
        task {
            let! prks = queryAllPacking dctx
            return prks 
            |> Seq.choose id
            |> Seq.toList
        }

    let getParking dctx rawParkingId =
        taskResult {
            let! parkingId = ParkingId.parse rawParkingId
            let! parking = queryParkingById dctx parkingId
            match parking with
            | Some prk ->
                return! Ok prk
            | None ->
                return! Error <| EntityNotFound "Parking not existing"
        }

    let patchParking dctx freeLimit rawParkingId rawStatus completeDate =
        taskResult {
            match! ParkingStatus.parse rawStatus with
            | Started ->
                return! Error <| BadInput "Only Complete status is supported"
            | Completed ->
                match! getParking dctx rawParkingId with
                | StartedFree prk ->

                    match transitionToCompletedFree freeLimit prk completeDate with
                    | Ok freePrk ->
                        do! Commands.transitionToCompletedFree dctx freePrk
                        return! CompletedFree freePrk |> Ok
                    | Error err ->
                        return! Error err
                | CompletedFree _ 
                | CompletedFirst _ ->
                    return! Error <| TransitionError "Parking was already completed"
        }

    let createPayment dctx freeLimit rawParkingId completeDate =
        taskResult {
            match! getParking dctx rawParkingId with
            | StartedFree prk ->
                let paymentId = PaymentId (Guid.NewGuid())
                match transitionToCompletedFirst freeLimit prk (paymentId, completeDate) with
                | Ok firstPrk ->
                    do! Commands.transitionToCompletedFirst dctx firstPrk
                    return! firstPrk.Payment |> Ok
                | Error err ->
                    return! Error err
            | CompletedFree _ 
            | CompletedFirst _ ->
                return! Error <| TransitionError "Parking was already completed"
        }