namespace Songhay.StudioFloor.Client.Components

open System
open System.Net
open System.Net.Http
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Elmish
open Bolero

open Songhay.Player.YouTube.Models
open Songhay.StudioFloor.Client
open Songhay.StudioFloor.Client.Models

type StudioFloorProgramComponent() =
    inherit ProgramComponent<StudioFloorModel, StudioFloorMessage>()

    let update (jsRuntime: IJSRuntime) (client: HttpClient) message model =
        match message with
        | Error _ -> model, Cmd.none
        | GetReadMe ->
            let success (result: Result<string, HttpStatusCode>) =
                let data = result |> Result.valueOr (fun code -> $"The expected README data is not here. [error code: {code}]")
                StudioFloorMessage.GotReadMe data
            let failure ex = ((jsRuntime |> Some), ex) ||> ClientUtility.passFailureToConsole |> StudioFloorMessage.Error
            let uri = ("./README.html", UriKind.Relative) |> Uri
            let cmd = Cmd.OfAsync.either ClientUtility.Remote.tryDownloadToStringAsync (client, uri)  success failure
            model, cmd
        | GotReadMe data ->
            let m = { model with readMeData = (data |> Some) }
            m, Cmd.none
        | NavigateTo page ->
            let m = { model with page = page }
            m, Cmd.none
        | StudioFloorMessage.SetTab tab ->
            let m = { model with tab = tab }
            match tab with
            | YtThumbsTab -> m, Cmd.ofMsg (StudioFloorMessage.YouTubeMessage YouTubeMessage.CallYtItems)
            | _ -> m, Cmd.none
        | StudioFloorMessage.YouTubeMessage ytMsg -> ClientUtility.update jsRuntime client ytMsg model

    let view model dispatch =
        TabsElmishComponent.EComp model dispatch

    [<Inject>]
    member val HttpClient = Unchecked.defaultof<HttpClient> with get, set

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.Program =
        let initModel = {
            tab = ReadMeTab
            page = ReadMePage
            readMeData = None
            ytModel = YouTubeModel.initialize this.HttpClient this.JSRuntime this.NavigationManager
        }
        let init = (fun _ -> initModel, Cmd.ofMsg StudioFloorMessage.GetReadMe)
        let update = update this.JSRuntime this.HttpClient
        Program.mkProgram init update view
