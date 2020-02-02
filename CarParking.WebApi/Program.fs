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

module Program =
    let webApp =
        choose [
            GET >=> routeCi "/ping" >=> text "pong"

            GET >=> routeCif "/parkings/%s" getParkingHandler
            POST >=> routeCi "/parkings" >=> createParkingHandler
            PATCH >=> routeCif "/parkings/%s" updateParkingHandler ]

    let configuration = 
        ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .Build()  

    let configureApp (app : IApplicationBuilder) =
        app.UseGiraffe webApp

    let configureServices (services : IServiceCollection) =
        services
            .AddGiraffe()
            .AddTransient<ISQLServerDataContext>(fun _ -> 
                { new ISQLServerDataContext with member __.Connection = null }) |> ignore

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
