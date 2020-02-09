namespace CarParking.WebApi

open Microsoft.AspNetCore.Http
open Giraffe
open FSharp.Control.Tasks.V2
open CarParking.Workflow.Parking
open CarParking.DataLayer.DataContext
open Responses
open Requests

module RouteHandlers =

    let ok obj = Successful.ok (json obj)

    let f h =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let dctx = ctx.GetService<ISQLServerDataContext>()
            let token = ctx.RequestAborted
            h next ctx dctx token

    let getParkingHandler rawParkingId =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let dctx = ctx.GetService<ISQLServerDataContext>()
            let token = ctx.RequestAborted
            task {
                match! getParking (dctx, token) rawParkingId with
                | Ok x ->
                    return! ok (ParkingResponse.FromParking x) next ctx
                | Error err -> 
                    return! RequestErrors.BAD_REQUEST err next ctx
            }

    let getAllParkingsHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let dctx = ctx.GetService<ISQLServerDataContext>()
            let token = ctx.RequestAborted
            task {
                let! list = getAllParkings (dctx, token)
                return! ok (list |> List.map ParkingResponse.FromParking) next ctx
            }

    let createParkingHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let dctx = ctx.GetService<ISQLServerDataContext>()
            let token = ctx.RequestAborted
            task {
                let! newParking = createNewParking (dctx, token)
                return! ok (ParkingResponse.FromParking newParking) next ctx
            }
    
    let updateParkingHandler rawParkingId =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let dctx = ctx.GetService<ISQLServerDataContext>()
            let token = ctx.RequestAborted
            task {
                match! ctx.TryBindFormAsync<ParkingPatchRequest>() with
                | Ok req -> 
                    match! updateParking (dctx, token) rawParkingId req.Status with
                    | Ok _ ->
                        return! Successful.NO_CONTENT next ctx
                    | Error err ->
                        return! RequestErrors.BAD_REQUEST err next ctx
                | Error err ->
                    return! RequestErrors.BAD_REQUEST err next ctx
            }