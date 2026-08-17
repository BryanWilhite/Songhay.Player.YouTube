module Songhay.Player.YouTube.Client.ProgramComponentUtility

open System
open System.Net
open System.Net.Http
open System.Text.Json
open Elmish
open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Modules.Bolero.RemoteHandlerUtility
open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Modules.Publications.Models

open Songhay.Player.YouTube
open Songhay.Player.YouTube.Client.Models
open Songhay.Player.YouTube.Models

module Remote =
    let tryDownloadToStringAsync (client: HttpClient, uri: Uri) =
        async {
            let! responseResult = client |> trySendAsync (get uri) |> Async.AwaitTask
            let! output =
                (None, responseResult) ||> tryDownloadToStringAsync
                |> Async.AwaitTask

            return output
        }

[<Literal>]
let YtIndexKinte = "kintespace"

let httpClient = Songhay.Modules.Bolero.ServiceProviderUtility.getHttpClient()
let jsRuntime = Songhay.Modules.Bolero.ServiceProviderUtility.getIJSRuntime()

let getCommandForSetPage page =
    match page with
    | BRollYtPage key ->
        let msg = YTMessage <| YouTubeMessage.GetYtManifestAndPlaylist key
        Cmd.ofMsg msg
    | BRollYtSetPage ->
        let msg = YTMessage <| YouTubeMessage.CallYtIndexAndSetForThumbsSet
        Cmd.ofMsg msg
    | _ -> Cmd.none

let update ytMsg model =

    let ytModel = {
        model with ytModel = YouTubeModel.updateModel ytMsg model.ytModel
    }

    let uriYtSetOption =
        (
            YtIndexKinte |> Identifier.Alphanumeric,
            ytModel.ytModel.GetSelectedDocumentClientId()
        )
        ||> model.ytModel.GetPlaylistSetUri

    let rec successYtItems (result: Result<string, HttpStatusCode>) =
        let dataGetter = ServiceHandlerUtility.toYtItems
        let items = (dataGetter, result) ||> toHandlerOutput None
        let message = YouTubeMessage.CalledYtItems items
        ClientMessage.YTMessage message

    let successYtSet (result: Result<string, HttpStatusCode>) =
        let dataGetter = ServiceHandlerUtility.toYtSet
        let set = (dataGetter, result) ||> toHandlerOutput None
        let message = YouTubeMessage.CalledYtSet set
        ClientMessage.YTMessage message

    let failure ex = ((jsRuntime |> Some), ex) ||> ytMsg.FailureMessage |> ClientMessage.YTMessage

    match ytMsg with
    | YouTubeMessage.CallYtItems key ->
        let uriOption = key |> Identifier.Alphanumeric |> model.ytModel.GetPlaylistUri
        let cmd =
            uriOption
            |> Option.either
                (fun uri -> Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uri)  successYtItems failure)
                (fun() -> Cmd.none)

        ytModel, cmd

    | YouTubeMessage.CallYtIndexAndSetForThumbsSet ->
        let success (result: Result<string, HttpStatusCode>) =
            let dataGetter = ServiceHandlerUtility.toPublicationIndexData
            let index = (dataGetter, result) ||> toHandlerOutput None
            let ytItemsSuccessMsg = YouTubeMessage.CalledYtSetIndex index
            ClientMessage.YTMessage ytItemsSuccessMsg
        let uriIdxOption = YtIndexKinte |> Identifier.Alphanumeric |> model.ytModel.GetPlaylistIndexUri
        let cmdBatch =
            Option.zip uriIdxOption uriYtSetOption
            |> Option.either
                (
                    fun (uriIdx, uriYtSet) ->
                        Cmd.batch [
                            Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uriIdx) success failure
                            Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uriYtSet) successYtSet failure
                        ]
                )
                (fun() -> Cmd.none)

        ytModel, cmdBatch

    | YouTubeMessage.CallYtSet _ ->
        let cmd =
            uriYtSetOption
            |> Option.either
                (fun uriYtSet -> Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, uriYtSet) successYtSet failure)
                (fun() -> Cmd.none)

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
                        ClientMessage.YTMessage ytMessage
                )
                (
                    fun ex ->
                        let label = $"{nameof Presentation}.{nameof Presentation.fromInput}:" |> Some
                        jsRuntime |> passErrorToConsole label ex |> ClientMessage.Error
                )

        let cmdBatch =
            Option.zip manifestUriOption playlistUriOption
            |> Option.either
                (
                    fun (manifestUri, playlistUri) ->
                        Cmd.batch [
                            Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, manifestUri) successManifest failure
                            Cmd.OfAsync.either Remote.tryDownloadToStringAsync (httpClient, playlistUri) successYtItems failure
                        ]
                )
                (fun () -> Cmd.none)

        ytModel, cmdBatch

    | YouTubeMessage.OpenYtSetOverlay ->
        if ytModel.ytModel.ytSetIndex.IsNone && ytModel.ytModel.ytSet.IsNone then
            ytModel, Cmd.ofMsg (ClientMessage.YTMessage CallYtIndexAndSetForThumbsSet)
        else
            ytModel, Cmd.none

    | _ -> ytModel, Cmd.none
