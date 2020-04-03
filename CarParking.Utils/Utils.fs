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
    static member inline ToNullable(this: Option<_>) = 
        this
        |> Option.map (fun v -> Nullable(v))
        |> Option.defaultValue (Nullable())

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
    open Microsoft.FSharp.Quotations

    let nameOf (q:Expr<_>) = 
        match q with 
        | Patterns.PropertyGet(_, mi, _) -> mi.Name
        | Patterns.ValueWithName (_,_,name) -> name
        | _ -> failwith "Unexpected format"

    let p<'T> : 'T = failwith "!"
