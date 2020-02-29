namespace CarParking.WebApi

open System
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Giraffe
open CarParking.DataLayer.DataContext
open RouteHandlers
open System.Data.SqlClient
open System.Data
open Newtonsoft.Json
open Giraffe.Serialization
open Configuration

module Program =
    let inline private ( <>> ) f g = g f
    let inline private ( ^ ) f x = f x

    let webApp =
        choose [
            GET >=> routeCi "/ping" >=> text "pong"

            GET >=> routeCi "/parkings" >=> (getAllParkingsHandler <>> withDctx)
            GET >=> routeCif "/parkings/%s" (getParkingHandler      >> withDctx)
            POST >=> routeCi "/parkings" >=> (createParkingHandler <>> withDctx)
            PATCH >=> routeCif "/parkings/%s" (patchParkingHandler  >> withDctx >> withSettings)
            POST >=> routeCif "/parkings/%s/payments" (createPaymentHandler >> withDctx >> withSettings) ]

    let configuration = 
        ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .Build()  

    let createCPDataContext (connStr: string) =
        let conn = new SqlConnection(connStr) :> IDbConnection
        { new ICPDataContext with 
            member __.Connection = conn }

    let configureApp (app : IApplicationBuilder) =
        app.UseGiraffe webApp

    let configureServices (host: WebHostBuilderContext) (services : IServiceCollection) =
        let connStr = host.Configuration.GetConnectionString("CarParking")
        let jsonSettings = JsonSerializerSettings(DateTimeZoneHandling = DateTimeZoneHandling.Utc)
        services
            .AddGiraffe()
            .AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer(jsonSettings))
            .AddTransient<ICPDataContext>(fun _ -> createCPDataContext connStr)
            .Configure<CarParkingSettings>(host.Configuration.GetSection("CarParking")) |> ignore

    [<EntryPoint>]
    let main args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder -> 
                webBuilder
                    .UseKestrel()
                    .UseConfiguration(configuration)
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices) |> ignore)
            .Build()
            .Run()
        0
