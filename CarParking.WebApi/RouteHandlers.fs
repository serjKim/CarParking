namespace CarParking.WebApi

open Microsoft.AspNetCore.Http
open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open CarParking.Error
open CarParking.Workflow.Parking
open CarParking.DataLayer.DataContext
open Responses
open Requests
open System
open Microsoft.Extensions.Options
open Configuration

module RouteHandlers =
    let ok obj = Successful.ok (json obj)

    let toResponse okResult = function
        | Ok x -> okResult x
        | Error err ->
            match err with
            | EntityNotFound message -> RequestErrors.NOT_FOUND message
            | BadInput message -> RequestErrors.BAD_REQUEST message
            | TransitionError error -> RequestErrors.UNPROCESSABLE_ENTITY { ErrorType = error.ToString() }

    let withDctx handler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let cpdc = ctx.GetService<ICPDataContext>()
            let token = ctx.RequestAborted
            handler next ctx (cpdc, token)

    let withSettings handler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let settings = ctx.GetService<IOptionsMonitor<CarParkingSettings>>()
            handler next ctx settings.CurrentValue

    let getParkingHandler rawParkingId =
        fun next ctx dctx ->
            task {
                let! parking = getParking dctx rawParkingId
                return! toResponse (ParkingResponse.FromParking
                                    >> ParkingResponseModel.FromResponse 
                                    >> ok) parking next ctx
            }

    let getAllParkingsHandler =
        fun next ctx dctx ->
            task {
                let! list = getAllParkings dctx
                return! ok (list 
                            |> List.map ParkingResponse.FromParking
                            |> ParkingsResponseModel.FromResponse) next ctx
            }

    let createParkingHandler =
        fun next ctx dctx ->
            task {
                let! newParking = createNewParking dctx DateTime.UtcNow
                return! ok (newParking
                            |> ParkingResponse.FromParking
                            |> ParkingResponseModel.FromResponse) next ctx
            }
    
    let patchParkingHandler rawParkingId =
        fun next (ctx : HttpContext) dctx settings ->
            task {
                match! ctx.TryBindFormAsync<ParkingPatchRequest>() with
                | Ok req -> 
                    let freeLimit = getFreeLimit settings
                    let! res = patchParking dctx freeLimit rawParkingId req.Status DateTime.UtcNow
                    return! toResponse (fun _ -> Successful.NO_CONTENT) res next ctx
                | Error err ->
                    return! RequestErrors.BAD_REQUEST err next ctx
            }

    let createPaymentHandler rawParkingId =
        fun next ctx dctx settings ->
            task {
                let freeLimit = getFreeLimit settings
                let! payment = createPayment dctx freeLimit rawParkingId DateTime.UtcNow
                return! toResponse (PaymentResponse.FromPayment >> ok) payment next ctx
            }