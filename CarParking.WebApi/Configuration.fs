namespace CarParking.WebApi

open System

module Configuration =
    [<CLIMutable; NoEquality; NoComparison>]
    type CarParkingSettings =
        { FreeLimit: uint32 }

    let getFreeLimit (settings: CarParkingSettings) =
        new TimeSpan(0, int settings.FreeLimit, 0)