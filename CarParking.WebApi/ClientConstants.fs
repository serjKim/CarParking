namespace CarParking.WebApi

open CarParking.Core

[<RequireQualifiedAccess>]
module ClientConstants =
    [<RequireQualifiedAccess>]
    module Parking =
        [<Literal>]
        let StartedFree = "StartedFree"

        [<Literal>]
        let CompletedFree = "CompletedFree"

        [<Literal>]
        let CompletedFirst = "CompletedFirst"

        let ofParking = function 
            | Parking.StartedFree _ -> StartedFree
            | Parking.CompletedFree _ -> CompletedFree
            | Parking.CompletedFirst _ -> CompletedFirst
