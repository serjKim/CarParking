namespace CarParking.WebUI

module Configuration =
    [<CLIMutable; NoEquality; NoComparison>]
    type CarParkingUISettings =
        { ApiUrl: string }