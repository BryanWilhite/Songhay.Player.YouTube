namespace Songhay.StudioFloor.Client.Components

open System.Net.Http
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Elmish
open Bolero

open Songhay.Player.YouTube.Models
open Songhay.StudioFloor.Client
open Songhay.StudioFloor.Client.Models

module pcu = ProgramComponentUtility

type StudioFloorProgramComponent() =
    inherit ProgramComponent<StudioFloorModel, StudioFloorMessage>()

    let update message (model: StudioFloorModel) =
        match message with
        | Error _ -> model, Cmd.none
        | GetReadMe ->
            let cmd = pcu.getCommandForGetReadMe model
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
        | StudioFloorMessage.YouTubeMessage ytMsg -> pcu.update ytMsg model

    let view model dispatch =
        TabsElmishComponent.EComp model dispatch

    [<Inject>]
    member val HttpClient = Unchecked.defaultof<HttpClient> with get, set

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.Program =
        let initModel = StudioFloorModel.initialize this.HttpClient this.JSRuntime this.NavigationManager
        let init = (fun _ -> initModel, Cmd.ofMsg StudioFloorMessage.GetReadMe)
        let update = update
        Program.mkProgram init update view
