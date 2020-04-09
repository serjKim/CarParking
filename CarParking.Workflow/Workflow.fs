namespace CarParking.Workflow

open System
open FSharp.Control.Tasks.V2.ContextInsensitive
open CarParking.Utils
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

    let getAllParkings dctx transitionNames =
        task {
            let! prks = queryAllPacking dctx (QueryAllParkingFilter.ByTransitionNames transitionNames)
            return prks 
            |> Seq.choose Result.toOption
            |> Seq.toList
        }

    let getParking dctx parkingId =
        taskResult {
            let! parking = queryParkingById dctx parkingId
            match parking with
            | Ok prk ->
                return! Ok prk
            | Error _ ->
                return! Error <| EntityNotFound "Parking not found"
        }

    let patchParking dctx freeLimit parkingId status completeDate =
        taskResult {
            match status with
            | Started ->
                return! Error <| BadInput "Only Complete status is supported"
            | Completed ->
                match! getParking dctx parkingId with
                | StartedFree prk ->
                    match Transitions.toCompletedFree freeLimit completeDate prk with
                    | Ok completedFree ->
                        do! Commands.saveCompletedFree dctx completedFree
                        return! CompletedFree completedFree |> Ok
                    | Error err ->
                        return! Error err
                | CompletedFree _ 
                | CompletedFirst _ ->
                    return! Error <| TransitionError AlreadyCompleted
        }

    let createPayment dctx freeLimit parkingId completeDate =
        taskResult {
            match! getParking dctx parkingId with
            | StartedFree prk ->
                let payment = 
                    { Id = PaymentId (Guid.NewGuid())
                      CreateDate = completeDate }
                match Transitions.toCompletedFirst freeLimit payment prk with
                | Ok firstPrk ->
                    do! Commands.saveCompletedFirst dctx firstPrk
                    return! firstPrk.PaidInterval
                            |> PaidInterval.getPayment
                            |> Ok
                | Error err ->
                    return! Error err
            | CompletedFree _ 
            | CompletedFirst _ ->
                return! Error <| TransitionError AlreadyCompleted
        }