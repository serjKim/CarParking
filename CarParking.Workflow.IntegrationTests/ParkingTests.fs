module ParkingTests

open Xunit
open FSharp.Control.Tasks.V2.ContextInsensitive
open CarParking.Workflow.Parking
open System.Data.SqlClient
open CarParking.DataLayer.DataContext
open CarParking.Core
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
        let! _ = conn.ExecuteAsync("delete from Payment")
        let! _ = conn.ExecuteAsync("delete from Parking")
        return ()
    }

[<Property(MaxTest = 1000)>]
let ``Should create a StartedFreeParking and get`` (arrivalDate: DateTime) =
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
let ``Should create a bunch of StartedFreeParking and get them`` () =
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

(*
    Knowing that a StartedFreeParking can be complete if Free tariff is not exceeded. In other words, 
    we can describe the following condition:
    
    CompleteDate ≤ ArrivalDate + Δ
    
        where 0 ≤ Δ ≤ FreeLimit

    That's the property we need to test. Lets implement an Arbitrary instance that would generate data, satisfing our condition.
*)

type FreeParkingDates = Dates of DateTime * DateTime * uint32

type ArbitaryFreeParkingDates =
    static member FreeParkingDates() =
        Arb.generate<DateTime * uint32 * uint32> 
        |> Gen.filter (fun (_, delta, freeLimit) -> delta <= freeLimit)
        |> Gen.map (fun (date, delta, freeLimit) ->
            let arrivalDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0)
            let completeDate  = arrivalDate.AddMinutes (float delta)
            Dates (arrivalDate, completeDate, freeLimit))
        |> Arb.fromGen
        
[<Property(MaxTest = 5000, Arbitrary = [| typeof<ArbitaryFreeParkingDates> |])>]
let ``Should patch a StartedFreeParking status, transferring to Complete if Free tariff is not exceeded`` (Dates (arrivalDate, completeDate, freeLimit)) =
    taskResult {
        let dctx = createDctx ()

        // clean db
        do! cleanDb dctx

        // create a started parking
        match! createNewParking dctx arrivalDate with
        | StartedFreeParking parking ->
            
            let rawParkingId = parking.Id |> ParkingId.toString

            match! patchParking dctx (TimeSpan(0, int freeLimit, 0)) rawParkingId (Completed.ToString()) completeDate with
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