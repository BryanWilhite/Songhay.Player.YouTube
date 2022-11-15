namespace Songhay.StudioFloor.Client

open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Microsoft.Extensions.DependencyInjection
open System
open System.Net.Http

open Songhay.Player.YouTube.Models.YouTubeScalars

module Program =

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        builder.RootComponents.Add<ElmishProgram.StudioFloorProgramComponent>("#studio-floor")
        builder.Services.AddScoped<HttpClient>(fun _ ->
            new HttpClient(BaseAddress = Uri YouTubeApiRootUri)) |> ignore
        builder.Build().RunAsync() |> ignore
        0
