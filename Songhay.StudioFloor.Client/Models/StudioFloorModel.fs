namespace Songhay.StudioFloor.Client.Models

open System.Net.Http
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Songhay.Player.YouTube.Models
open Songhay.StudioFloor.Client.Models

type StudioFloorModel =
    {
        blazorServices: {| httpClient: HttpClient; jsRuntime: IJSRuntime; navigationManager: NavigationManager |}
        readMeData: string option
        page: StudioFloorPage
        ytModel: YouTubeModel
    }

    static member initialize (httpClient: HttpClient) (jsRuntime: IJSRuntime) (navigationManager: NavigationManager) =
        {
            blazorServices = {| httpClient = httpClient; jsRuntime = jsRuntime; navigationManager = navigationManager |}
            page = ReadMePage
            readMeData = None
            ytModel = YouTubeModel.initialize httpClient jsRuntime navigationManager
        }
