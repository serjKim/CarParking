namespace CarParking.WebUI.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open CarParking.WebUI.Configuration

[<CLIMutable>]
type IndexModel =
    { ApiUrl: string }

type HomeController (logger : ILogger<HomeController>, settings: IOptionsMonitor<CarParkingUISettings>) =
    inherit Controller()

    member this.Index () =
        this.View("dist/index.cshtml", { ApiUrl = settings.CurrentValue.ApiUrl })
