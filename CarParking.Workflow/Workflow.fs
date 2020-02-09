namespace CarParking.Workflow

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.V2
open CarParking.Utils
open CarParking.Core
open CarParking.Core.Parking
open CarParking.DataLayer.Queries
open CarParking.DataLayer.Commands

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
            let status = Started
            let arrivalDate = DateTime.UtcNow
            let! newId = insertParking (dctx, token) status arrivalDate

            return {
                Id = newId
                Status = status
                ArrivalDate = arrivalDate }
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
                return Error "Parking not existing"
        }

    let updateParking (dctx, token) rawParkingId rawStatus =
        workflow {
            let! status = ParkingStatus.parse rawStatus

            match status with
            | Started ->
                return Error "Only Complete status is supported"
            | Complete ->
                let! parking = getParking (dctx, token) rawParkingId

                match tryCompleteParking parking DateTime.UtcNow with
                | Ok cprk ->
                    do! updateParking (dctx, token) cprk

                    return Ok cprk
                | Error err ->
                    match err with
                    | FreeExpired message -> return Error message
        }

