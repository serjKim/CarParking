﻿module ParkingTests

open Xunit
open FSharp.Control.Tasks.V2.ContextInsensitive
open CarParking.Workflow.Parking
open System.Data.SqlClient
open CarParking.DataLayer.DataContext
open CarParking.Core
open CarParking.Core.ParkingInterval
open CarParking.Error
open System.Data
open System.Threading
open System
open FsCheck
open FsCheck.Xunit
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open Dapper
open Microsoft.Extensions.Configuration

let expectTrue = Task.map (function
    | Ok x -> x 
    | Error _ -> false)

let configuration = 
    ConfigurationBuilder()
        .SetBasePath(Environment.CurrentDirectory)
        .AddJsonFile("appsettings.json", false, false)
        .Build()  

let testConnString = configuration.GetConnectionString("CarParkingTests")

let createCPDataContext (connStr: string) =
    let conn = new SqlConnection(connStr) :> IDbConnection
    { new ICPDataContext with 
        member __.Connection = conn }

let createDctx () = (createCPDataContext testConnString, CancellationToken.None)

let cleanDb (cpdc, _) =
    let conn = getConn cpdc
    conn.Execute("
        delete from Parking
        delete from Payment
    ") |> ignore

[<RequireQualifiedAccess>]
type FreeParkingDates = Dates of DateTimeOffset * DateTimeOffset * TimeSpan

let generateFreeDatesArb condition =
    Arb.generate<DateTimeOffset * uint32 * uint32> 
    |> Gen.filter condition
    |> Gen.map (fun (date, delta, freeLimit) ->
        let arrivalDate = new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0, TimeSpan.Zero)
        let completeDate  = arrivalDate.AddMinutes (float delta)
        FreeParkingDates.Dates (arrivalDate, completeDate, TimeSpan(0, int freeLimit, 0)))
    |> Arb.fromGen

type ArbitaryFreeParkingDates =
    static member FreeParkingDates() =
        generateFreeDatesArb (fun (_, delta, freeLimit) -> delta <= freeLimit)

type ArbitaryExpiredParkingDates =
    static member FreeParkingDates() =
        generateFreeDatesArb (fun (_, delta, freeLimit) -> delta > freeLimit)

[<RequireQualifiedAccess>]
type ArrivalDates = Dates of DateTimeOffset list

type ArbitaryArrivalDates =
    static member ArrivalDates() =
        Gen.listOf Arb.generate<DateTimeOffset>
        |> Gen.filter (not << List.isEmpty)
        |> Gen.map (List.sort >> ArrivalDates.Dates)
        |> Arb.fromGen

[<Properties(MaxTest = 2000, Parallelism = 8)>]
type ParkingWorkflowTests () =
    do createDctx () |> cleanDb
 
    [<Property>]
    member _.``Should create a StartedFreeParking`` (arrivalDate: DateTimeOffset) =
        taskResult {
            let dctx = createDctx ()

            // create a started parking
            match! createNewParking dctx arrivalDate with
            | StartedFree parking ->
                Assert.Equal (arrivalDate, parking.ArrivalDate)
    
                // get the parking
                match! getParking dctx parking.Id with
                | StartedFree prk ->
                    return prk = parking
                | _ ->
                    return false
            | _  ->
                return false
        } |> expectTrue

    [<Property(MaxTest = 1, Arbitrary = [| typeof<ArbitaryArrivalDates> |])>]
    member _.``Should get a bunch of StatedFreeParking`` (ArrivalDates.Dates arrivalDates) =
        taskResult {
            let createParking date = createNewParking (createDctx ()) date

            // create a bunch
            let! createdParkings =
                arrivalDates
                |> List.map createParking
                |> Task.WhenAll
                |> Task.map (List.ofArray >> List.sort)
        
            let createdArrivalDates = 
                createdParkings 
                |> List.choose (function
                    | StartedFree prk ->
                        Some prk.ArrivalDate
                    | _ as p ->
                        Assert.True(false, sprintf "Expected Started but actual %A" p)
                        None)
                |> List.sort
        
            // check arrival dates
            Assert.True((createdArrivalDates = arrivalDates))

            // get all
            let! loadedParkings = getAllParkings (createDctx ()) []

            return ((loadedParkings |> List.sort) = createdParkings)
        } |> expectTrue

    [<Property>]
    member _.``Should return EntityNotFound if parking is not exising`` (id: Guid) =
        task {
            let dctx = createDctx ()
            let parkingId = ParkingId id

            match! getParking dctx parkingId with
            | Ok _ ->
                return false
            | Error err ->
                match err with
                | EntityNotFound _ ->
                    return true
                | _ ->
                    return false
        }

    [<Fact>]
    member _.``Should return an empty array if there isn't parkings`` () =
        task {
            let dctx = createDctx ()
            let! parkings = getAllParkings dctx []

            return List.isEmpty parkings
        }

    (*
        Knowing that a StartedFreeParking can be completed if Free tariff is not exceeded. In other words, 
        we can describe the following condition:
    
        CompleteDate ≤ ArrivalDate + Δ
    
            where 0 ≤ Δ ≤ FreeLimit

        That's the property we need to test. Lets implement an Arbitrary instance that would generate data, satisfing our condition.
    *)
    [<Property(Arbitrary = [| typeof<ArbitaryFreeParkingDates> |])>]
    member _.``Should patch a StartedFreeParking, transiting to Complete if Free tariff is not expired`` (FreeParkingDates.Dates (arrivalDate, completeDate, freeLimit)) =
        taskResult {
            let dctx = createDctx ()

            // create a started parking
            match! createNewParking dctx arrivalDate with
            | StartedFree parking ->

                match! patchParking dctx freeLimit parking.Id Completed completeDate with
                | CompletedFree prk ->
                    Assert.True(prk.Id = parking.Id &&
                                prk.Interval.ArrivalDate = parking.ArrivalDate &&
                                prk.Interval.CompleteDate = completeDate)

                    match! getParking dctx prk.Id with
                    | CompletedFree p ->
                        return p = prk
                    | _ ->
                        return false

                | _ ->
                    return false
        
            | _  ->
                return false
        } |> expectTrue

    [<Property(Arbitrary = [| typeof<ArbitaryExpiredParkingDates> |])>]
    member _.``Shouldn't patch a StartedFreeParking, returning TransitionError if Free tariff is expired`` (FreeParkingDates.Dates (arrivalDate, completeDate, freeLimit)) =
        task {
            let dctx = createDctx ()

            // create a started parking
            match! createNewParking dctx arrivalDate with
            | StartedFree parking ->

                match! patchParking dctx freeLimit parking.Id Completed completeDate with
                | Ok _ ->
                    return false
                | Error err ->
                    match err with
                    | TransitionError te ->
                        match te with
                        | FreeExpired ->
                            return true
                        | _ ->
                            return false
                    | _ ->
                        return false
    
            | _  ->
                return false
        }

    [<Property>]
    member _.``Patch return BadInput when Completed is changed to Started`` (arrivalDate: DateTimeOffset)
                                                                            (completeDate: DateTimeOffset)
                                                                            (freeLimit: TimeSpan) =
        task {
            let dctx = createDctx ()

            // create a started parking
            match! createNewParking dctx arrivalDate with
            | StartedFree parking ->
            
                // try patch to Started status
                match! patchParking dctx freeLimit parking.Id Started completeDate with
                | Ok _ ->
                    return false
                | Error err ->
                    match err with
                    | BadInput _ ->
                        return true
                    | _ ->
                        return false
            | _  ->
                return false
        }

    [<Property(Arbitrary = [| typeof<ArbitaryFreeParkingDates> |])>]
    member _.``Shouldn't patch an already completed parking`` (FreeParkingDates.Dates (arrivalDate, completeDate, freeLimit)) =
        taskResult {
            let dctx = createDctx ()

            // create a started parking
            match! createNewParking dctx arrivalDate with
            | StartedFree parking ->

                match! patchParking dctx freeLimit parking.Id Completed completeDate with
                | CompletedFree prk ->

                    // try patch again
                    match! patchParking dctx freeLimit prk.Id Completed completeDate with
                    | Ok _ -> 
                        return false
                    | Error err ->
                        match err with
                        | TransitionError _ ->
                            return true
                        | _ ->
                            return false

                | _ ->
                    return false
    
            | _  ->
                return false
        } |> Task.map Result.isOk

    [<Property(Arbitrary = [| typeof<ArbitaryExpiredParkingDates> |])>]
    member _.``Should create a payment and complete a StartedFreeParking if Free tariff is expired`` (FreeParkingDates.Dates (arrivalDate, completeDate, freeLimit)) =
        taskResult {
            let dctx = createDctx ()

            // create a started parking
            match! createNewParking dctx arrivalDate with
            | StartedFree parking ->
                let! payment = createPayment dctx freeLimit parking.Id completeDate
                Assert.True(payment.CreateDate = completeDate)

                match! getParking (createDctx ()) parking.Id with
                | CompletedFirst p ->
                    let interval = PaidInterval.getInterval p.PaidInterval
                    let payment = PaidInterval.getPayment p.PaidInterval
                    return interval.CompleteDate = payment.CreateDate
                | _ ->
                    return false
            | _  ->
                return false

        } |> expectTrue

    [<Property(Arbitrary = [| typeof<ArbitaryExpiredParkingDates> |])>]
    member _.``Shouldn't create a payment and complete if parking is already completed`` (FreeParkingDates.Dates (arrivalDate, completeDate, freeLimit)) =
        taskResult {
            let dctx = createDctx ()

            // create a started parking
            match! createNewParking dctx arrivalDate with
            | StartedFree parking ->
        
                let! _ = createPayment dctx freeLimit parking.Id completeDate

                // try pay again
                match! createPayment (createDctx ()) freeLimit parking.Id completeDate with
                | Ok _ ->
                    return false
                | Error err ->
                    match err with
                    | TransitionError _ ->
                        return true
                    | _ ->
                        return false
    
            | _  ->
                return false

        } |> expectTrue

    [<Property(Arbitrary = [| typeof<ArbitaryExpiredParkingDates> |])>]
    member _.``Shouldn't complete an already payed parking`` (FreeParkingDates.Dates (arrivalDate, completeDate, freeLimit)) =
        taskResult {
            let dctx = createDctx ()

            // create a started parking
            match! createNewParking dctx arrivalDate with
            | StartedFree parking ->
        
                let! _ = createPayment dctx freeLimit parking.Id completeDate

                // try patch after payment
                match! patchParking (createDctx ()) freeLimit parking.Id Completed completeDate with
                | Ok _ -> 
                    return false
                | Error err ->
                    match err with
                    | TransitionError _ ->
                        return true
                    | _ ->
                        return false
    
            | _  ->
                return false

        } |> expectTrue