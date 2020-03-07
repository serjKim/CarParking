namespace CarParking.WebUI

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting

module Program =
    [<EntryPoint>]
    let main args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStartup<Startup>() |> ignore)
            .Build()
            .Run()
        0
