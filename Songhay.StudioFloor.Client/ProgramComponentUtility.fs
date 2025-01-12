module Songhay.StudioFloor.Client.ProgramComponentUtility

open System
open System.Net
open System.Net.Http
open System.Text.Json
open Microsoft.JSInterop

open Elmish
open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Modules.Bolero.RemoteHandlerUtility
open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility

open Songhay.Modules.Publications.Models
open Songhay.Player.YouTube
open Songhay.Player.YouTube.Models
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

let httpClient = Songhay.Modules.Bolero.ServiceProviderUtility.getHttpClient()
let jsRuntime = Songhay.Modules.Bolero.ServiceProviderUtility.getIJSRuntime()

let passFailureToConsole (jsRuntime: IJSRuntime option) ex =
    if jsRuntime.IsSome then
        jsRuntime.Value |> consoleErrorAsync [|
            "failure:", ex
        |] |> ignore
    ex

let getCommandForGetReadMe (model: StudioFloorModel) =
    let success (result: Result<string, HttpStatusCode>) =
        let data = result |> Result.valueOr (fun code -> $"The expected README data is not here. [error code: {code}]")
        StudioFloorMessage.GotReadMe data
    let failure ex = ((jsRuntime |> Some), ex) ||> passFailureToConsole |> StudioFloorMessage.Error
    let uri = ("./README.html", UriKind.Relative) |> Uri
    let cmd = Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uri)  success failure

    cmd

let update ytMsg model =
    let ytModel = {
        model with ytModel = YouTubeModel.updateModel ytMsg model.ytModel
    }

    let uriYtSet =
        (
            YtIndexSonghay |> Identifier.Alphanumeric,
            ytModel.ytModel.getSelectedDocumentClientId()
        )
        ||> getPlaylistSetUri

    let rec successYtItems (result: Result<string, HttpStatusCode>) =
        let dataGetter = ServiceHandlerUtility.toYtItems
        let items = (dataGetter, result) ||> toHandlerOutput None
        let message = YouTubeMessage.CalledYtItems items
        StudioFloorMessage.YouTubeMessage message

    let successYtSet (result: Result<string, HttpStatusCode>) =
        let dataGetter = ServiceHandlerUtility.toYtSet
        let set = (dataGetter, result) ||> toHandlerOutput None
        let message = YouTubeMessage.CalledYtSet set
        StudioFloorMessage.YouTubeMessage message

    let failure ex = ((jsRuntime |> Some), ex) ||> ytMsg.failureMessage |> StudioFloorMessage.YouTubeMessage

    match ytMsg with
    | YouTubeMessage.CallYtItems key ->
        let uri = key |> Identifier.Alphanumeric |> getPlaylistUri
        let cmd = Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uri)  successYtItems failure
        ytModel, cmd

    | YouTubeMessage.CallYtIndexAndSetForThumbsSet
    | YouTubeMessage.CallYtIndexAndSetForThumbsSetOverlay ->
        let success (result: Result<string, HttpStatusCode>) =
            let dataGetter = ServiceHandlerUtility.toPublicationIndexData
            let index = (dataGetter, result) ||> toHandlerOutput None
            let ytItemsSuccessMsg = YouTubeMessage.CalledYtSetIndex index
            StudioFloorMessage.YouTubeMessage ytItemsSuccessMsg
        let uriIdx = YtIndexSonghay |> Identifier.Alphanumeric |> getPlaylistIndexUri
        let cmdBatch = Cmd.batch [
            Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uriIdx) success failure
            Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uriYtSet) successYtSet failure
        ]
        ytModel, cmdBatch

    | YouTubeMessage.CallYtSet _ ->
        let cmd = Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uriYtSet) successYtSet failure
        ytModel, cmd

    | YouTubeMessage.GetYtManifestAndPlaylist key ->
        let manifestUri = key |> getPresentationManifestUri
        let playlistUri = key |> getPresentationYtItemsUri

        let successManifest (result: Result<string, HttpStatusCode>) =
            result
            |> Result.either
                Presentation.fromInput
                (
                    fun statusCode ->
                        let ex = JsonException($"{nameof HttpStatusCode}: {statusCode}")
                        Result.Error ex
                )
            |> Result.either
                (
                    fun presentation ->
                        let id = Identifier.fromString key
                        let ytMessage = YouTubeMessage.GotYtManifest <| (id, Some presentation)
                        StudioFloorMessage.YouTubeMessage ytMessage
                )
                (
                    fun ex ->
                        let label = $"{nameof Presentation}.{nameof Presentation.fromInput}:" |> Some
                        jsRuntime |> passErrorToConsole label ex |> StudioFloorMessage.Error
                )

        let cmdBatch = Cmd.batch [
            Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, manifestUri) successManifest failure
            Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, playlistUri) successYtItems failure
        ]
        ytModel, cmdBatch

    | YouTubeMessage.OpenYtSetOverlay ->
        if ytModel.ytModel.ytSetIndex.IsNone && ytModel.ytModel.ytSet.IsNone then
            ytModel, Cmd.ofMsg (StudioFloorMessage.YouTubeMessage CallYtIndexAndSetForThumbsSetOverlay)
        else
            ytModel, Cmd.none

    | _ -> ytModel, Cmd.none
