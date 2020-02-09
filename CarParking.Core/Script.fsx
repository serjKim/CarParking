#load "Domain.fs"

open System
open CarParking.Core
open CarParking.Core.Parking

let x = { Id = ParkingId 1L; ArrivalDate = DateTime.UtcNow.AddMinutes(-20.); Status = Started }
let d = DateTime.UtcNow
calculateTariff x d
let r = tryCompleteParking x d