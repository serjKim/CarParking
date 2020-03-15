#load "..\\CarParking.Error\Error.fs"
#load "Domain.fs"

open System
open CarParking.Core
open CarParking.Core.Parking

let s =
    { Id = ParkingId (Guid.NewGuid())
      ArrivalDate = DateTime.UtcNow.AddMinutes(-20.) }
let d = DateTime.UtcNow
let freeLimit = TimeSpan(0, 1, 0)

calculateTariff freeLimit s d

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