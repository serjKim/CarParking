namespace CarParking.Utils

open System.Threading.Tasks
open FSharp.Control.Tasks.V2

module Result =

    let bindAsync (binder: 'T -> Task<_>) (taskResult: Task<_>) = 
        task {
            let! result = taskResult
            match result with
            | Error e ->
                return Error e
            | Ok x -> 
                let! r = binder x
                return r
        }

    let OkAsync (taskResult: Task<_>) = 
        task {
           let! result = taskResult
           return Ok result
        }

module Option =

    let bindAsync (binder: 'T -> Task<_>) (taskResult: Task<_>) = 
        task {
            let! result = taskResult
            match result with
            | None ->
                return None
            | Some x -> 
                let! r = binder x
                return r
        }

    let someAsync (taskResult: Task<_>) =
        task {
            let! result = taskResult
            return Some result
        }