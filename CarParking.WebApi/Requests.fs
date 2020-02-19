namespace CarParking.WebApi

module Requests = 
    [<CLIMutable; NoEquality; NoComparison>]
    type ParkingPatchRequest =
        { Status: string }