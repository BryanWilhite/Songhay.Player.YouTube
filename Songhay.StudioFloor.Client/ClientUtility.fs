module Songhay.StudioFloor.Client.ClientUtility

open System
open System.Net
open System.Net.Http
open Elmish
open Bolero.Remoting.Client
open Microsoft.JSInterop

open Songhay.Modules.Bolero
open Songhay.Modules.Models
open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Player.YouTube
open Songhay.Player.YouTube.Models
open Songhay.Modules.Bolero.RemoteHandlerUtility
open Songhay.Player.YouTube.YtUriUtility
open Songhay.StudioFloor.Client.Models

    module Remote =
        let tryDownloadToStringAsync (client: HttpClient, uri: Uri) =
            async {
                let! responseResult = client |> trySendAsync (get uri) |> Async.AwaitTask
                let! output =
                    (None, responseResult) ||> tryDownloadToStringAsync
                    |> Async.AwaitTask

                return output
            }

let passFailureToConsole (jsRuntime: IJSRuntime option) ex =
    if jsRuntime.IsSome then
        jsRuntime.Value |> JsRuntimeUtility.consoleErrorAsync [|
            "failure:", ex
        |] |> ignore
    ex

let update (jsRuntime: IJSRuntime) (client: HttpClient) ytMsg model =
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
            StudioFloorMessage.YouTubeMessage ytItemsSuccessMsg

    let failure ex = ((jsRuntime |> Some), ex) ||> ytMsg.failureMessage |> StudioFloorMessage.YouTubeMessage

    match ytMsg with
    | YouTubeMessage.CallYtItems ->
        let success (result: Result<string, HttpStatusCode>) =
            let dataGetter = ServiceHandlerUtility.toYtItems
            let items = (dataGetter, result) ||> toHandlerOutput None
            let ytItemsSuccessMsg = YouTubeMessage.CalledYtItems items
            StudioFloorMessage.YouTubeMessage ytItemsSuccessMsg
        let uri = YtIndexSonghayTopTen |> Identifier.Alphanumeric |> getPlaylistUri
        let cmd = Cmd.OfAsync.either Remote.tryDownloadToStringAsync (client, uri)  success failure
        ytModel, cmd

    | YouTubeMessage.CallYtIndexAndSet ->
        let success (result: Result<string, HttpStatusCode>) =
            let dataGetter = ServiceHandlerUtility.toPublicationIndexData
            let index = (dataGetter, result) ||> toHandlerOutput None
            let ytItemsSuccessMsg = YouTubeMessage.CalledYtSetIndex index
            StudioFloorMessage.YouTubeMessage ytItemsSuccessMsg
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
            ytModel, Cmd.ofMsg (StudioFloorMessage.YouTubeMessage CallYtIndexAndSet)
        else
            ytModel, Cmd.none

    | _ -> ytModel, Cmd.none
