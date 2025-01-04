namespace Songhay.StudioFloor.Client.Components

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
            let m =
                  {
                    model with
                        page = page
                        ytModel =
                                    {
                                        blazorServices = model.ytModel.blazorServices
                                        error = model.ytModel.error
                                        presentation = None
                                        presentationKey = model.ytModel.presentationKey
                                        ytItems = None
                                        ytSet = model.ytModel.ytSet
                                        ytSetIndex = model.ytModel.ytSetIndex
                                        ytVisualStates =
                                            model.ytModel.ytVisualStates
                                                .addState(YtSetOverlayIsUntouched)
                                                .removeState(YtSetOverlayIsVisible)
                                    } 
                  }
            match page with
            | YtPresentationPage key ->
                let cmd =
                    if m.ytModel.presentation.IsNone then
                        Cmd.ofMsg (StudioFloorMessage.YouTubeMessage <| YouTubeMessage.GetYtManifestAndPlaylist key)
                    else
                        Cmd.none
                m, cmd
            | YtThumbsPage key ->
                let cmd =
                    if m.ytModel.ytItems.IsNone then
                        Cmd.ofMsg (StudioFloorMessage.YouTubeMessage <| YouTubeMessage.CallYtItems key)
                    else
                        Cmd.none
                m, cmd
            | YtThumbsSetPage ->
                let cmd =
                    if m.ytModel.ytItems.IsNone then
                        Cmd.ofMsg (StudioFloorMessage.YouTubeMessage <| YouTubeMessage.CallYtIndexAndSet)
                    else
                        Cmd.none
                m, cmd
            | _ -> m, Cmd.none
        | YouTubeMessage ytMsg -> pcu.update ytMsg model

    let view model dispatch = TabsElmishComponent.EComp model dispatch

    override this.Program =
        let initModel = StudioFloorModel.initialize this.Services
        let init = (fun _ -> initModel, Cmd.ofMsg StudioFloorMessage.GetReadMe)
        let update = update
        Program.mkProgram init update view
        |> Program.withRouter ElmishRoutes.router
