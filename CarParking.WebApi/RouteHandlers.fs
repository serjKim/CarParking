namespace CarParking.WebApi

module RouteHandlers =
    open Microsoft.AspNetCore.Http
    open Giraffe
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open CarParking.Error
    open CarParking.Workflow.Parking
    open CarParking.DataLayer.DataContext
    open CarParking.DataLayer.Queries
    open Responses
    open Requests
    open System
    open Microsoft.Extensions.Options
    open Configuration
    open CarParking.Core
    open FsToolkit.ErrorHandling
    open System.Threading.Tasks

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
        match ctx.TryGetQueryStringValue "transitions" with
        | Some raw ->
            if String.IsNullOrEmpty (raw) then
                []
            else
                raw.Split ','
                |> Array.filter (not << String.IsNullOrWhiteSpace)
                |> List.ofArray
        | None -> []

    let getParkingHandler rawParkingId =
        fun next ctx dctx ->
            taskResult {
                let! parkingId = ParkingId.parse rawParkingId
                return! getParking dctx parkingId
            } |> toResponseAsync (ParkingResponse.FromParking >> ok) next ctx

    let getAllParkingsHandler =
        fun next (ctx : HttpContext) dctx ->
            taskResult {
                let transitionNames = deserializeFilterItems ctx
                return! getAllParkings dctx transitionNames
            } |> toResponseAsync (List.map ParkingResponse.FromParking >> ok) next ctx

    let createParkingHandler =
        fun next ctx dctx ->
            task {
                let! newParking = createNewParking dctx DateTimeOffset.UtcNow
                return! ok (newParking |> ParkingResponse.FromParking) next ctx
            }
    
    let patchParkingHandler rawParkingId =
        fun next (ctx : HttpContext) dctx settings ->
            taskResult {
                match! ctx.TryBindFormAsync<ParkingPatchRequest>() with
                | Ok (req: ParkingPatchRequest) -> 
                    let freeLimit = getFreeLimit settings
                    let! parkingId = ParkingId.parse rawParkingId
                    let! status = ParkingStatus.parse req.Status
                    return! patchParking dctx freeLimit parkingId status DateTimeOffset.UtcNow
                | Error err ->
                    return! Error <| BadInput err
            } |> toResponseAsync (fun _ -> Successful.NO_CONTENT) next ctx

    let createPaymentHandler rawParkingId =
        fun next ctx dctx settings ->
            taskResult {
                let freeLimit = getFreeLimit settings
                let! parkingId = ParkingId.parse rawParkingId
                return! createPayment dctx freeLimit parkingId DateTimeOffset.UtcNow
            } |> toResponseAsync (PaymentResponse.FromPayment >> ok) next ctx

    let getAllTransitions =
        fun next ctx dctx ->
            taskResult {
                return! queryAllTransitions dctx
            } |> toResponseAsync (Seq.choose id 
                                  >> Seq.map TransitionResponse.FromTransition
                                  >> ok) next ctx
