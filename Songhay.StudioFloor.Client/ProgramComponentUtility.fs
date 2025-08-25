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

open Songhay.StudioFloor.Client.Models

open Songhay.StudioFloor.Client.YouTubeScalars

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

let getCommandForGetReadMe (_: StudioFloorModel) =
    let success (result: Result<string, HttpStatusCode>) =
        let data = result |> Result.valueOr (fun code -> $"The expected README data is not here. [error code: {code}]")
        StudioFloorMessage.GotReadMe data
    let failure ex = ((jsRuntime |> Some), ex) ||> passFailureToConsole |> StudioFloorMessage.Error
    let uri = ("./README.html", UriKind.Relative) |> Uri
    let cmd = Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uri) success failure

    cmd

let update ytMsg model =
    let ytModel = {
        model with ytModel = YouTubeModel.updateModel ytMsg model.ytModel
    }

    let uriYtSetOption =
        (
            YtIndexSonghay |> Identifier.Alphanumeric,
            ytModel.ytModel.GetSelectedDocumentClientId()
        )
        ||> model.ytModel.GetPlaylistSetUri

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

    let failure ex = ((jsRuntime |> Some), ex) ||> ytMsg.FailureMessage |> StudioFloorMessage.YouTubeMessage

    match ytMsg with
    | YouTubeMessage.CallYtItems key ->
        let uriOption = key |> Identifier.Alphanumeric |> model.ytModel.GetPlaylistUri
        match uriOption with
        | None -> ytModel, Cmd.none
        | Some uri ->
        let cmd = Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uri) successYtItems failure

        ytModel, cmd

    | YouTubeMessage.CallYtIndexAndSetForThumbsSet
    | YouTubeMessage.CallYtIndexAndSetForThumbsSetOverlay ->
        let success (result: Result<string, HttpStatusCode>) =
            let dataGetter = ServiceHandlerUtility.toPublicationIndexData
            let index = (dataGetter, result) ||> toHandlerOutput None
            let ytItemsSuccessMsg = YouTubeMessage.CalledYtSetIndex index
            StudioFloorMessage.YouTubeMessage ytItemsSuccessMsg

        match YtIndexSonghay |> Identifier.Alphanumeric |> model.ytModel.GetPlaylistIndexUri with
        | None -> ytModel, Cmd.none
        | Some uriIdx ->
            match uriYtSetOption with
            | None -> ytModel, Cmd.none
            | Some uriYtSet ->
            let cmdBatch = Cmd.batch [
                Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uriIdx) success failure
                Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uriYtSet) successYtSet failure
            ]

            ytModel, cmdBatch

    | YouTubeMessage.CallYtSet _ ->
        match uriYtSetOption with
        | None -> ytModel, Cmd.none
        | Some uriYtSet ->
        let cmd = Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uriYtSet) successYtSet failure

        ytModel, cmd

    | YouTubeMessage.GetYtManifestAndPlaylist key ->
        let manifestUriOption = key |> model.ytModel.GetPresentationManifestUri
        let playlistUriOption = key |> model.ytModel.GetPresentationYtItemsUri

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

        if manifestUriOption.IsSome && playlistUriOption.IsSome then
            let cmdBatch = Cmd.batch [
                Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, manifestUriOption.Value) successManifest failure
                Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, playlistUriOption.Value) successYtItems failure
            ]
            ytModel, cmdBatch
        else
            ytModel, Cmd.none

    | YouTubeMessage.OpenYtSetOverlay ->
        if ytModel.ytModel.ytSetIndex.IsNone && ytModel.ytModel.ytSet.IsNone then
            ytModel, Cmd.ofMsg (StudioFloorMessage.YouTubeMessage CallYtIndexAndSetForThumbsSetOverlay)
        else
            ytModel, Cmd.none

    | _ -> ytModel, Cmd.none
