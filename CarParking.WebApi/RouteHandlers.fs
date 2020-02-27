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

module RouteHandlers =
    let ok obj = Successful.ok (json obj)

    let toResponse okResult = function
        | Ok x -> okResult x
        | Error err ->
            match err with
            | EntityNotFound d -> RequestErrors.NOT_FOUND d
            | BadInput d -> RequestErrors.BAD_REQUEST d
            | TransitionError d -> RequestErrors.UNPROCESSABLE_ENTITY d

    let injectDctx handler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let cpdc = ctx.GetService<ICPDataContext>()
            let token = ctx.RequestAborted
            handler next ctx (cpdc, token)

    let getParkingHandler rawParkingId =
        fun next ctx dctx ->
            task {
                let! parking = getParking dctx rawParkingId
                return! toResponse (ParkingResponse.FromParking >> ok) parking next ctx
            }

    let getAllParkingsHandler =
        fun next ctx dctx ->
            task {
                let! list = getAllParkings dctx
                return! ok (list |> List.map ParkingResponse.FromParking) next ctx
            }

    let createParkingHandler =
        fun next ctx dctx ->
            task {
                let! newParking = createNewParking dctx DateTime.UtcNow
                return! ok (ParkingResponse.FromParking newParking) next ctx
            }
    
    let patchParkingHandler rawParkingId =
        fun next (ctx : HttpContext) dctx ->
            task {
                match! ctx.TryBindFormAsync<ParkingPatchRequest>() with
                | Ok req -> 
                    let! res = patchParking dctx (new TimeSpan(0, 10, 0)) rawParkingId req.Status DateTime.UtcNow
                    return! toResponse (fun _ -> Successful.NO_CONTENT) res next ctx
                | Error err ->
                    return! RequestErrors.BAD_REQUEST err next ctx
            }

    let createPaymentHandler rawParkingId =
        fun next ctx dctx ->
            task {
                let! payment = createPayment dctx (new TimeSpan(0, 10, 0)) rawParkingId DateTime.UtcNow
                return! toResponse (PaymentResponse.FromPayment >> ok) payment next ctx
            }