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
open CarParking.Core
open FsToolkit.ErrorHandling
open System.Threading.Tasks

module RouteHandlers =
    let ok obj = Successful.ok (json obj)

    let toResponse okResult = function
        | Ok x -> okResult x
        | Error err ->
            match err with
            | EntityNotFound message -> RequestErrors.NOT_FOUND message
            | BadInput message -> RequestErrors.BAD_REQUEST message
            | TransitionError error -> RequestErrors.UNPROCESSABLE_ENTITY { ErrorType = error.ToString() }

    let toResponseAsync okResult next ctx (result: Task<_>) = 
        task {
            let! x = result
            return! toResponse okResult x next ctx
        }

    let withDctx handler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let cpdc = ctx.GetService<ICPDataContext>()
            let token = ctx.RequestAborted
            handler next ctx (cpdc, token)

    let withSettings handler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let settings = ctx.GetService<IOptionsMonitor<CarParkingSettings>>()
            handler next ctx settings.CurrentValue

    let deserializeFilterItems (ctx : HttpContext) =
        match ctx.TryGetQueryStringValue "types" with
        | Some raw ->
            if String.IsNullOrEmpty (raw) then
                []
            else
                raw.Split ','
                |> Array.filter (not << String.IsNullOrWhiteSpace)
                |> Array.choose (fun rawType ->
                    match rawType.Split '|' with
                    | [| rawTariff; rawStatus |] ->
                        let r = result {
                            let! tariff = Tariff.parse rawTariff
                            let! status = ParkingStatus.parse rawStatus
                            return (tariff, status) } 
                        match r with
                        | Ok x -> Some x
                        | Error _ -> None
                    | _ -> None)
                |> List.ofArray
        | None -> []

    let getParkingHandler rawParkingId =
        fun next ctx dctx ->
            taskResult {
                let! parkingId = ParkingId.parse rawParkingId
                return! getParking dctx parkingId
            } |> toResponseAsync (ParkingResponse.FromParking
                                  >> ParkingResponseModel.FromResponse 
                                  >> ok) next ctx

    let getAllParkingsHandler =
        fun next (ctx : HttpContext) dctx ->
            taskResult {
                let byTypes = deserializeFilterItems ctx
                return! getAllParkings dctx byTypes
            } |> toResponseAsync (List.map (ParkingResponse.FromParking) 
                                  >> ParkingsResponseModel.FromResponse 
                                  >> ok) next ctx

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
            taskResult {
                match! ctx.TryBindFormAsync<ParkingPatchRequest>() with
                | Ok (req: ParkingPatchRequest) -> 
                    let freeLimit = getFreeLimit settings
                    let! parkingId = ParkingId.parse rawParkingId
                    let! status = ParkingStatus.parse req.Status
                    return! patchParking dctx freeLimit parkingId status DateTime.UtcNow
                | Error err ->
                    return! Error <| BadInput err
            } |> toResponseAsync (fun _ -> Successful.NO_CONTENT) next ctx

    let createPaymentHandler rawParkingId =
        fun next ctx dctx settings ->
            taskResult {
                let freeLimit = getFreeLimit settings
                let! parkingId = ParkingId.parse rawParkingId
                return! createPayment dctx freeLimit parkingId DateTime.UtcNow
            } |> toResponseAsync (PaymentResponse.FromPayment >> ok) next ctx