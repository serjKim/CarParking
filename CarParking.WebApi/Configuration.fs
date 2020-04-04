namespace CarParking.WebApi

open System
open System.Collections.Generic

module Configuration =
    [<CLIMutable; NoEquality; NoComparison>]
    type CorsPolicy =
        { Policy: string
          Origins: IReadOnlyList<string> }

    [<CLIMutable; NoEquality; NoComparison>]
    type Cors =
        { WebUI: CorsPolicy }

    [<CLIMutable; NoEquality; NoComparison>]
    type CarParkingSettings =
        { FreeLimitMinutes: uint32 }

    let getFreeLimit (settings: CarParkingSettings) =
        new TimeSpan(0, int settings.FreeLimitMinutes, 0)