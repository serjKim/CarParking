#r "nuget: FsToolkit.ErrorHandling, 1.2.6"
#load "..\\CarParking.Error\Error.fs"
#load "Domain.fs"

open System
open CarParking.Core
open CarParking.Core.Parking

let s =
    { Id = ParkingId (Guid.NewGuid())
      ArrivalDate = DateTimeOffset.UtcNow.AddMinutes(-20.) }
let d = DateTimeOffset.UtcNow
let freeLimit = TimeSpan(0, 1, 0)

match ParkingInterval.createInterval (s.ArrivalDate, d) with
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