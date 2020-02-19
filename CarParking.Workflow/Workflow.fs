namespace CarParking.Workflow

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
open CarParking.Utils
open CarParking.Error
open CarParking.Core
open CarParking.Core.Parking
open CarParking.DataLayer.Queries
open CarParking.DataLayer

[<AutoOpen>]
module internal Workflow =

    type WorkflowBuilder() =
        member _.Bind(x, f) = x |> Task.FromResult |> Result.bindAsync f
        member _.Bind(x, f) = Result.bindAsync f x
        member _.Bind(x: Task<_>, f) = 
            task {
                let! result = x
                return! f result
            }
        member _.Bind(x: Task, f) = 
            task {
                do! x
                return! f()
            }
        member _.ReturnFrom(x) = x
        member _.Return(x) = x |> Task.FromResult

    let workflow = new WorkflowBuilder()    

module Parking =

    let createNewParking (dctx, token) =
        workflow {
            let parking = 
                { Id = ParkingId (Guid.NewGuid())
                  ArrivalDate = DateTime.UtcNow }
            do! Commands.insertStartedFree (dctx, token) parking
            return StartedFreeParking parking
        }

    let getAllParkings (dctx, token) =
        workflow {
            return! queryAllPacking (dctx, token)
        }

    let getParking (dctx, token) rawParkingId =
        workflow {
            let! parkingId = ParkingId.parse rawParkingId
            let! parking = queryParkingById (dctx, token) parkingId
            match parking with
            | Some prk ->
                return Ok prk
            | None ->
                return Error <| EntityNotFound "Parking not existing"
        }

    let patchParking (dctx, token) rawParkingId rawStatus =
        workflow {
            match! ParkingStatus.parse rawStatus with
            | Started ->
                return Error <| BadInput "Only Complete status is supported"
            | Completed ->
                match! getParking (dctx, token) rawParkingId with
                | StartedFreeParking prk ->

                    match transitionToCompletedFree prk DateTime.UtcNow with
                    | Ok freePrk ->
                        do! Commands.transitionToCompletedFree (dctx, token) freePrk
                        return CompletedFreeParking freePrk |> Ok
                    | Error err ->
                        return Error err
                | CompletedFreeParking _ 
                | CompletedFirstParking _ ->
                    return Error <| TransitionError "Parking was already completed"
        }

    let createPayment (dctx, token) rawParkingId =
        workflow {
            let createDate = DateTime.UtcNow

            match! getParking (dctx, token) rawParkingId with
            | StartedFreeParking prk ->
                let paymentId = PaymentId (Guid.NewGuid())
                match transitionToCompletedFirst prk (createDate, paymentId) with
                | Ok firstPrk ->
                    do! Commands.transitionToCompletedFirst (dctx, token) firstPrk
                    return CompletedFirstParking firstPrk |> Ok
                | Error err ->
                    return Error err
            | CompletedFreeParking _ 
            | CompletedFirstParking _ ->
                return Error <| TransitionError "Parking was already completed"
        }