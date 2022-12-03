module Songhay.StudioFloor.Client.ElmishProgram

open System.Net.Http
open Elmish
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.Layout
open Songhay.Player.YouTube
open Songhay.Player.YouTube.Components
open Songhay.StudioFloor.Client.ElmishTypes

let update (jsRuntime: IJSRuntime) (client: HttpClient) message model =
    match message with
    | Message.SetPage page ->
        let m = { model with page = page }
        match page with
        | YtThumbsPage -> m, Cmd.ofMsg (Message.YouTubeMessage YouTubeMessage.CallYtItems)
        | _ -> m, Cmd.none
    | Message.YouTubeMessage ytMsg -> ClientUtility.update jsRuntime client ytMsg model

let view model dispatch =
    let tabs = [
        ("README", ReadMePage)
        ("YouTube Thumbs", YtThumbsPage)
        ("YouTube Presentation", YtPresentationPage)
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
                        on.click (fun _ -> SetPage pg |> dispatch)
                        text label
                    }
                }
            }
        }

        bulmaContainer
            ContainerWidthFluid
            NoCssClasses
            (cond model.page <| function
            | ReadMePage ->
                text "read me"
            | YtPresentationPage ->
                text "presentation"
            | YtThumbsPage ->
                YtThumbsComponent.EComp (Some "songhay tube") model.ytModel (Message.YouTubeMessage >> dispatch)
            )

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
            page = ReadMePage
            readMeData = None
            ytModel = YouTubeModel.initialize
        }
        let init = (fun _ -> initModel, Cmd.none)
        let update = update this.JSRuntime this.HttpClient
        Program.mkProgram init update view
