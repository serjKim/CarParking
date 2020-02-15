namespace CarParking.Utils

open System.Threading.Tasks
open FSharp.Control.Tasks.V2
open System
open System.Runtime.CompilerServices

[<Extension>]
type NullableExtensions() =
    [<Extension>]
    static member inline ToOption(this: Nullable<'T>) = 
        if this.HasValue then
            Some this.Value
        else
            None

[<Extension>]
type OptionExtensions() =
    [<Extension>]
    static member inline ToNullable(this: Option<_>) = 
        this
        |> Option.map (fun v -> Nullable(v))
        |>  Option.defaultValue (Nullable())

    [<Extension>]
    static member inline MapOrDefault(this: Option<'T>, mapping) = 
        this
        |> Option.map mapping
        |> Option.defaultValue Unchecked.defaultof<'U>

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
