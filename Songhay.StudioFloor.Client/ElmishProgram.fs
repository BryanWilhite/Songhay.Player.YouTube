module Songhay.StudioFloor.Client.ElmishProgram

open System
open System.Net
open System.Net.Http
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Elmish
open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.Element
open Songhay.Modules.Bolero.Visuals.Bulma.Layout
open Songhay.Player.YouTube
open Songhay.Player.YouTube.Components
open Songhay.StudioFloor.Client.ElmishTypes

let update (jsRuntime: IJSRuntime) (client: HttpClient) message model =
    match message with
    | Error _ -> model, Cmd.none
    | GetReadMe ->
        let success (result: Result<string, HttpStatusCode>) =
            let data = result |> Result.valueOr (fun code -> $"The expected README data is not here. [error code: {code}]")
            Message.GotReadMe data
        let failure ex = ((jsRuntime |> Some), ex) ||> ClientUtility.passFailureToConsole |> Message.Error
        let uri = ("./README.html", UriKind.Relative) |> Uri
        let cmd = Cmd.OfAsync.either ClientUtility.Remote.tryDownloadToStringAsync (client, uri)  success failure
        model, cmd
    | GotReadMe data ->
        let m = { model with readMeData = (data |> Some) }
        m, Cmd.none
    | Message.SetTab tab ->
        let m = { model with tab = tab }
        match tab with
        | YtThumbsTab -> m, Cmd.ofMsg (Message.YouTubeMessage YouTubeMessage.CallYtItems)
        | _ -> m, Cmd.none
    | Message.YouTubeMessage ytMsg -> ClientUtility.update jsRuntime client ytMsg model

let view model dispatch =
    let tabs = [
        ("README", ReadMeTab)
        ("YouTube Thumbs", YtThumbsTab)
        ("YouTube Presentation", YtPresentationTab)
    ]

    concat {
        div {
            [
                "tabs";
                "has-background-grey-light";
                "is-toggle";
                "is-fullwidth";
                "is-large"
            ] |> CssClasses.toHtmlClassFromList

            ul {
                forEach tabs <| fun (label, pg) ->
                li {
                    a {
                        attr.href "#"
                        DomElementEvent.Click.PreventDefault
                        on.click (fun _ -> SetTab pg |> dispatch)
                        text label
                    }
                }
            }
        }

        cond model.tab <| function
        | ReadMeTab ->
            if model.readMeData.IsNone then
                text "loading…"
            else
                bulmaContainer
                    ContainerWidthFluid
                    NoCssClasses
                    (bulmaNotification
                        (HasClasses (CssClasses [ "is-info" ] ))
                        (rawHtml model.readMeData.Value))
        | YtPresentationTab ->
            text "presentation"
        | YtThumbsTab ->
            YtThumbsComponent.EComp (Some "songhay tube") model.ytModel (Message.YouTubeMessage >> dispatch)

        YtThumbsSetComponent.EComp model.ytModel (Message.YouTubeMessage >> dispatch)
    }

type StudioFloorProgramComponent() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val HttpClient = Unchecked.defaultof<HttpClient> with get, set

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.Program =
        let initModel = {
            tab = ReadMeTab
            readMeData = None
            ytModel = YouTubeModel.initialize
        }
        let init = (fun _ -> initModel, Cmd.ofMsg Message.GetReadMe)
        let update = update this.JSRuntime this.HttpClient
        Program.mkProgram init update view
