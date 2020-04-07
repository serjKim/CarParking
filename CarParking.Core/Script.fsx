#load "..\\CarParking.Error\Error.fs"
#r "..\\..\\..\\..\\.nuget\\packages\\fstoolkit.errorhandling\\1.2.6\\lib\\netstandard2.0\\FsToolkit.ErrorHandling.dll"
#load "Domain.fs"

open System
open CarParking.Core
open CarParking.Core.Parking

let s =
    { Id = ParkingId (Guid.NewGuid())
      ArrivalDate = DateTime.UtcNow.AddMinutes(-20.) }
let d = DateTime.UtcNow
let freeLimit = TimeSpan(0, 1, 0)

match ParkingInterval.create (s.ArrivalDate, d) with
| Ok interval -> Ok <| calculateTariff freeLimit interval
| Error err -> Error err

// Complete
match Transitions.toCompletedFree freeLimit s d with
| Ok prk ->
    printfn "Save %A to DB" prk
| Error x ->
    printfn "%A" x

let p = { Id = PaymentId (Guid.NewGuid()); CreateDate = d }

// Pay
match Transitions.toCompletedFirst freeLimit s p with
| Ok prk ->
    printfn "%A" prk
| Error x ->
    printfn "%A" x