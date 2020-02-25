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
            return StartedFreeParking parking
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

    let patchParking dctx rawParkingId rawStatus completeDate =
        taskResult {
            match! ParkingStatus.parse rawStatus with
            | Started ->
                return! Error <| BadInput "Only Complete status is supported"
            | Completed ->
                match! getParking dctx rawParkingId with
                | StartedFreeParking prk ->

                    match transitionToCompletedFree prk completeDate with
                    | Ok freePrk ->
                        do! Commands.transitionToCompletedFree dctx freePrk
                        return! CompletedFreeParking freePrk |> Ok
                    | Error err ->
                        return! Error err
                | CompletedFreeParking _ 
                | CompletedFirstParking _ ->
                    return! Error <| TransitionError "Parking was already completed"
        }

    let createPayment dctx rawParkingId =
        taskResult {
            let createDate = DateTime.UtcNow

            match! getParking dctx rawParkingId with
            | StartedFreeParking prk ->
                let paymentId = PaymentId (Guid.NewGuid())
                match transitionToCompletedFirst prk (createDate, paymentId) with
                | Ok firstPrk ->
                    do! Commands.transitionToCompletedFirst dctx firstPrk
                    return! firstPrk.Payment |> Ok
                | Error err ->
                    return! Error err
            | CompletedFreeParking _ 
            | CompletedFirstParking _ ->
                return! Error <| TransitionError "Parking was already completed"
        }