namespace CarParking.WebApi

module Requests = 

    [<CLIMutable>]
    type ParkingPatchRequest =
        { Status: string }