module ParkingTests

open Xunit
open FSharp.Control.Tasks.V2.ContextInsensitive
open CarParking.Workflow.Parking
open System.Data.SqlClient
open CarParking.DataLayer.DataContext
open CarParking.Core
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

(*
    This is just a PoC. Refactor the code
*)

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
    task {
        let! _ = conn.ExecuteAsync("delete from Parking")
        let! _ = conn.ExecuteAsync("delete from Payment")
        return ()
    }

[<Property(MaxTest = 1000)>]
let ``Should create a StartedFreeParking`` (arrivalDate: DateTime) =
    taskResult {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx
        
        // create a started parking
        match! createNewParking dctx arrivalDate with
        | StartedFreeParking parking ->
            Assert.Equal (arrivalDate, parking.ArrivalDate)
            
            let rawParkingId = parking.Id |> ParkingId.toString

            // get the parking
            match! getParking dctx rawParkingId with
            | StartedFreeParking prk ->
                return prk = parking
            | _ ->
                return false
            
        | _  ->
            return false
    } |> Task.map Result.isOk

[<Fact>]
let ``Should create a bunch of StartedFreeParking`` () =
    let arrivalDates =
        Arb.Default.DateTime()
        |> Arb.mapFilter (fun x -> DateTime.SpecifyKind(x, DateTimeKind.Utc)) (fun _ -> true)
        |> Arb.toGen
        |> Gen.sample 1000
    
    task {
        // clean db
        do! createDctx () |> cleanDb 

        // create a bunch
        let! createdParkings =
            arrivalDates
            |> Array.map (fun arrivalDate -> createNewParking (createDctx ()) arrivalDate)
            |> Task.WhenAll
        
        let createdArrivalDates = 
            createdParkings 
                |> Array.choose (fun parking -> 
                    match parking with
                    | StartedFreeParking prk ->
                        Some prk.ArrivalDate
                    | _ as p ->
                        Assert.True(false, sprintf "Expected Started but actual %A" p)
                        None)
                |> Array.sort
        
        // check arrival dates
        Assert.True((createdArrivalDates = (arrivalDates |> Array.sort)))

        // get all
        let! loadedParkings = getAllParkings (createDctx ())

        let sortedLoadedParkings = loadedParkings |> List.sort
        let sortedCreatedParkings = createdParkings |> List.ofArray |> List.sort

        Assert.True((sortedLoadedParkings = sortedCreatedParkings))
    }

[<Property(MaxTest = 1000)>]
let ``Should return EntityNotFound if parking is not exising`` (id: Guid) =
    task {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx
        
        let rawId = id.ToString()

        match! getParking dctx rawId with
        | Ok _ ->
            return false
        | Error err ->
            match err with
            | EntityNotFound message ->
                return true
            | _ ->
                return false
    }

[<Fact>]
let ``Should return an empty array if there isn't parkings`` () =
    task {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx

        match! getAllParkings dctx with
        | [] ->
            return true
        | _ ->
            return false
    }

(*
    Knowing that a StartedFreeParking can be complete if Free tariff is not exceeded. In other words, 
    we can describe the following condition:
    
    CompleteDate ≤ ArrivalDate + Δ
    
        where 0 ≤ Δ ≤ FreeLimit

    That's the property we need to test. Lets implement an Arbitrary instance that would generate data, satisfing our condition.
*)

type FreeParkingDates = Dates of DateTime * DateTime * TimeSpan

let generateFreeDatesArb condition =
    Arb.generate<DateTime * uint32 * uint32> 
    |> Gen.filter condition
    |> Gen.map (fun (date, delta, freeLimit) ->
        let arrivalDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0)
        let completeDate  = arrivalDate.AddMinutes (float delta)
        Dates (arrivalDate, completeDate, TimeSpan(0, int freeLimit, 0)))
    |> Arb.fromGen

type ArbitaryFreeParkingDates =
    static member FreeParkingDates() =
        generateFreeDatesArb (fun (_, delta, freeLimit) -> delta <= freeLimit)
        
[<Property(MaxTest = 5000, Arbitrary = [| typeof<ArbitaryFreeParkingDates> |])>]
let ``Should patch a StartedFreeParking, transferring to Complete if Free tariff is not expired`` (Dates (arrivalDate, completeDate, freeLimit)) =
    taskResult {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx

        // create a started parking
        match! createNewParking dctx arrivalDate with
        | StartedFreeParking parking ->
            
            let rawParkingId = parking.Id |> ParkingId.toString

            match! patchParking dctx freeLimit rawParkingId (Completed.ToString()) completeDate with
            | CompletedFreeParking prk ->
                Assert.True(prk.Id = parking.Id && prk.ArrivalDate = parking.ArrivalDate && prk.CompleteDate = completeDate)

                let rawId = prk.Id |> ParkingId.toString

                match! getParking dctx rawId with
                | CompletedFreeParking p ->
                    return p = prk
                | _ ->
                    return false

            | _ ->
                return false
        
        | _  ->
            return false
    } |> Task.map Result.isOk

type ArbitaryExpiredParkingDates =
    static member FreeParkingDates() =
        generateFreeDatesArb (fun (_, delta, freeLimit) -> delta > freeLimit)

[<Property(MaxTest = 5000, Arbitrary = [| typeof<ArbitaryExpiredParkingDates> |])>]
let ``Shouldn't patch a StartedFreeParking, returning TransitionError if Free tariff is expired`` (Dates (arrivalDate, completeDate, freeLimit)) =
    task {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx

        // create a started parking
        match! createNewParking dctx arrivalDate with
        | StartedFreeParking parking ->
        
            let rawParkingId = parking.Id |> ParkingId.toString

            match! patchParking dctx freeLimit rawParkingId (Completed.ToString()) completeDate with
            | Ok _ ->
                return false
            | Error err ->
                match err with
                | TransitionError message ->
                    return (message = "Free was expired")
                | _ ->
                    return false
    
        | _  ->
            return false
    }

[<Property(MaxTest = 5000)>]
let ``Patch returns BadInput if status is invalid`` (statusRaw: string)
                                                    (arrivalDate: DateTime)
                                                    (completeDate: DateTime)
                                                    (freeLimit: TimeSpan) =
    task {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx

        // create a started parking
        match! createNewParking dctx arrivalDate with
        | StartedFreeParking parking ->
            
            let rawParkingId = parking.Id |> ParkingId.toString

            match! patchParking dctx freeLimit rawParkingId statusRaw completeDate with
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

[<Property(MaxTest = 5000)>]
let ``Patch supports changing from Started to Completed`` (arrivalDate: DateTime)
                                                          (completeDate: DateTime)
                                                          (freeLimit: TimeSpan) =
    task {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx

        // create a started parking
        match! createNewParking dctx arrivalDate with
        | StartedFreeParking parking ->
            
            let rawParkingId = parking.Id |> ParkingId.toString

            // try patch to Started status
            match! patchParking dctx freeLimit rawParkingId (Started.ToString()) completeDate with
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
[<Property(MaxTest = 1000, Arbitrary = [| typeof<ArbitaryFreeParkingDates> |])>]
let ``Shouldn't patch an already completed parking`` (Dates (arrivalDate, completeDate, freeLimit)) =
    taskResult {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx

        // create a started parking
        match! createNewParking dctx arrivalDate with
        | StartedFreeParking parking ->
        
            let rawParkingId = parking.Id |> ParkingId.toString
            let rawStatus = Completed.ToString()

            match! patchParking dctx freeLimit rawParkingId rawStatus completeDate with
            | CompletedFreeParking prk ->
                
                // try patch again
                match! patchParking dctx freeLimit (prk.Id |> ParkingId.toString) rawStatus completeDate with
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

[<Property(MaxTest = 1000, Arbitrary = [| typeof<ArbitaryExpiredParkingDates> |])>]
let ``Should create a payment and complete a StartedFreeParking if Free tariff is expired`` (Dates (arrivalDate, completeDate, freeLimit)) =
    taskResult {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx

        // create a started parking
        match! createNewParking dctx arrivalDate with
        | StartedFreeParking parking ->
        
            let rawParkingId = parking.Id |> ParkingId.toString

            let! payment = createPayment dctx freeLimit rawParkingId completeDate

            return payment.CreateDate = completeDate
    
        | _  ->
            return false

    } |> Task.map Result.isOk

[<Property(MaxTest = 1000, Arbitrary = [| typeof<ArbitaryExpiredParkingDates> |])>]
let ``Shouldn't create a payment and complete if parking is already completed`` (Dates (arrivalDate, completeDate, freeLimit)) =
    taskResult {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx

        // create a started parking
        match! createNewParking dctx arrivalDate with
        | StartedFreeParking parking ->
        
            let rawParkingId = parking.Id |> ParkingId.toString

            let! _ = createPayment dctx freeLimit rawParkingId completeDate

            // try pay again
            match! createPayment (createDctx ()) freeLimit rawParkingId completeDate with
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

    } |> Task.map Result.isOk

[<Property(MaxTest = 1000, Arbitrary = [| typeof<ArbitaryExpiredParkingDates> |])>]
let ``Shouldn't complete an already payed parking`` (Dates (arrivalDate, completeDate, freeLimit)) =
    taskResult {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx

        // create a started parking
        match! createNewParking dctx arrivalDate with
        | StartedFreeParking parking ->
        
            let rawParkingId = parking.Id |> ParkingId.toString

            let! _ = createPayment dctx freeLimit rawParkingId completeDate

            // try patch after payment
            match! patchParking (createDctx ()) freeLimit rawParkingId (Completed.ToString()) completeDate with
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

    } |> Task.map Result.isOk