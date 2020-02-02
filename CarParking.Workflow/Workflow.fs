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

    let createNewParking dctx () =
        workflow {
            return! insertParking dctx Started DateTime.UtcNow
        }

    let getParking dctx rawParkingId =
        workflow {
            let! parkingId = ParkingId.Parse rawParkingId
            let! parking = queryParkingById dctx parkingId
            match parking with
            | Some prk ->
                return Ok prk
            | None ->
                return Error "Parking not existing"
        }

    let updateParking dctx rawParkingId rawStatus =
        workflow {
            let! status = ParkingStatus.Parse rawStatus

            match status with
            | Started ->
                return Error "Only Complete status is supported"
            | Complete ->
                let! parking = getParking dctx rawParkingId

                match tryCompleteParking parking DateTime.UtcNow with
                | Ok cprk ->
                    do! updateParking dctx cprk

                    return Ok cprk
                | Error err ->
                    match err with
                    | FreeExpired message -> return Error message
        }

