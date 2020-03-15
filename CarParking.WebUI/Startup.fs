namespace CarParking.WebUI

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Configuration

type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    member this.ConfigureServices(services: IServiceCollection) =
        services
            .AddControllersWithViews()
            .AddRazorRuntimeCompilation() |> ignore
        services.AddRazorPages() |> ignore
        services.Configure<CarParkingUISettings>(this.Configuration.GetSection("CarParkingUI")) |> ignore

    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
            
        app.UseHttpsRedirection() |> ignore
        
        app.UseStaticFiles()
           .UseRouting()
           .UseAuthorization()
           .UseEndpoints(fun endpoints ->
                endpoints.MapDefaultControllerRoute() |> ignore
                endpoints.MapRazorPages() |> ignore) |> ignore

    member val Configuration : IConfiguration = null with get, set
