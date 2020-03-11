namespace CarParking.WebUI.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open CarParking.WebUI.Configuration
open System.Text.Json
type HomeController (logger : ILogger<HomeController>, settings: IOptionsMonitor<CarParkingUISettings>) =
    inherit Controller()

    member this.Index () =
        let settingsJson = JsonSerializer.Serialize ({| apiUrl = settings.CurrentValue.ApiUrl |})
        this.View("dist/index.cshtml", settingsJson)
