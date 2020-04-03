namespace CarParking.DataLayer

[<RequireQualifiedAccess>]
module internal DbConstants =
    [<RequireQualifiedAccess>]
    module Transitions =
        [<Literal>]
        let StartedFree = "StartedFree"

        [<Literal>]
        let CompletedFree = "CompletedFree"

        [<Literal>]
        let CompletedFirst = "CompletedFirst"
