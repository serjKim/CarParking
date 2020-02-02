namespace CarParking.WebApi

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Giraffe
open System
open RouteHandlers
open CarParking.DataLayer.DataContext

module Program =

    let webApp =
        choose [
            GET >=>
                route "/ping" >=> text "pong"
                routeCif "/parkings/%s" getParkingHandler ]

    let configuration = 
        ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .Build()  

    type Startup private () =
        new (configuration: IConfiguration) as this =
            Startup() then
            this.Configuration <- configuration

        member __.ConfigureServices(services: IServiceCollection) =
            services
                .AddGiraffe()
                .AddTransient<ISQLServerDataContext>(fun _ -> { new ISQLServerDataContext with member __.Connection = null } )

        member __.Configure(app: IApplicationBuilder,
                              env: IWebHostEnvironment) =
            app.UseGiraffe webApp

        member val Configuration : IConfiguration = null with get, set

    [<EntryPoint>]
    let main _ =
        WebHostBuilder()
            .UseKestrel()
            .UseStartup<Startup>()
            .UseConfiguration(configuration)
            .Build()
            .Run()
        0
