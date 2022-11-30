module Songhay.StudioFloor.Client.ElmishProgram

open System
open System.Net
open System.Net.Http
open Elmish
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Songhay.Modules.Models
open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.RemoteHandlerUtility
open Songhay.Modules.Bolero.Visuals.Bulma.Layout
open Songhay.Player.YouTube
open Songhay.Player.YouTube.Components
open Songhay.Player.YouTube.YtUriUtility

    module Remote =
        let tryDownloadToStringAsync (client: HttpClient, uri: Uri) =
            async {
                let! responseResult = client |> trySendAsync (get uri) |> Async.AwaitTask
                let! output =
                    (None, responseResult) ||> tryDownloadToStringAsync
                    |> Async.AwaitTask

                return output
            }

type Page =
    | [<EndPoint "/">] ReadMePage
    | [<EndPoint "/thumbs">] YtThumbsPage
    | [<EndPoint "/presentation">] YtPresentationPage

type Model = { page: Page; ytModel: YouTubeModel }

let initModel = {
    page = ReadMePage
    ytModel = YouTubeModel.initialize
}

type Message =
    | SetPage of Page
    | YouTubeMessage of YouTubeMessage

let update (jsRuntime: IJSRuntime) (client: HttpClient) message model =
    match message with
    | Message.SetPage page ->
        let m = { model with page = page }
        match page with
        | YtThumbsPage -> m, Cmd.ofMsg (Message.YouTubeMessage YouTubeMessage.CallYtItems)
        | _ -> m, Cmd.none
    | Message.YouTubeMessage ytMsg ->
        let ytModel = {
            model with ytModel = YouTubeModel.updateModel ytMsg model.ytModel
        }
        let uriYtSet =
            (
                YtIndexSonghay |> Identifier.Alphanumeric,
                snd ytModel.ytModel.YtSetIndexSelectedDocument
            )
            ||> getPlaylistSetUri
        let successYtItems (result: Result<string, HttpStatusCode>) =
                let dataGetter = ServiceHandlerUtility.toYtSet
                let set = (dataGetter, result) ||> toHandlerOutput None
                let ytItemsSuccessMsg = YouTubeMessage.CalledYtSet set
                Message.YouTubeMessage ytItemsSuccessMsg

        let failure ex = ((jsRuntime |> Some), ex) ||> ytMsg.failureMessage |> Message.YouTubeMessage

        match ytMsg with
        | YouTubeMessage.CallYtItems ->
            let success (result: Result<string, HttpStatusCode>) =
                let dataGetter = ServiceHandlerUtility.toYtItems
                let items = (dataGetter, result) ||> toHandlerOutput None
                let ytItemsSuccessMsg = YouTubeMessage.CalledYtItems items
                Message.YouTubeMessage ytItemsSuccessMsg
            let uri = YtIndexSonghayTopTen |> Identifier.Alphanumeric |> getPlaylistUri
            let cmd = Cmd.OfAsync.either Remote.tryDownloadToStringAsync (client, uri)  success failure
            ytModel, cmd

        | YouTubeMessage.CallYtIndexAndSet ->
            let success (result: Result<string, HttpStatusCode>) =
                let dataGetter = ServiceHandlerUtility.toPublicationIndexData
                let index = (dataGetter, result) ||> toHandlerOutput None
                let ytItemsSuccessMsg = YouTubeMessage.CalledYtSetIndex index
                Message.YouTubeMessage ytItemsSuccessMsg
            let uriIdx = YtIndexSonghay |> Identifier.Alphanumeric |> getPlaylistIndexUri
            let cmdBatch = Cmd.batch [
                Cmd.OfAsync.either Remote.tryDownloadToStringAsync (client, uriIdx) success failure
                Cmd.OfAsync.either Remote.tryDownloadToStringAsync (client, uriYtSet) successYtItems failure
            ]
            ytModel, cmdBatch

        | YouTubeMessage.CallYtSet _ ->
            let cmd = Cmd.OfAsync.either Remote.tryDownloadToStringAsync (client, uriYtSet) successYtItems failure
            ytModel, cmd

        | YouTubeMessage.OpenYtSetOverlay ->
            if ytModel.ytModel.YtSetIndex.IsNone && ytModel.ytModel.YtSet.IsNone then
                ytModel, Cmd.ofMsg (Message.YouTubeMessage CallYtIndexAndSet)
            else
                ytModel, Cmd.none

        | _ -> ytModel, Cmd.none

let view model dispatch =
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
                forEach [
                    ("README", ReadMePage)
                    ("YouTube Thumbs", YtThumbsPage)
                    ("YouTube Presentation", YtPresentationPage)
                ] <| fun (label, pg) ->
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
        let init = (fun _ -> initModel, Cmd.ofMsg (Message.YouTubeMessage YouTubeMessage.CallYtItems))
        let update = update this.JSRuntime this.HttpClient
        Program.mkProgram init update view
