namespace CarParking.Utils

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
    static member inline MapOrDefault(this: Option<'T>, mapping) = 
        this
        |> Option.map mapping
        |> Option.defaultValue Unchecked.defaultof<'U>

module Result =
    let toOption = function
        | Ok x -> Some x
        | Error _ -> None

module NameOf =
    let prop<'T> : 'T = failwith "!"