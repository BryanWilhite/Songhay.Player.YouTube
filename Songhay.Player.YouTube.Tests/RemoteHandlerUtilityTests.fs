namespace Songhay.Player.YouTube.Tests

open System.Net.Http
open System.Text.Json
open Microsoft.Extensions.Logging
open Xunit

open FsUnit.Xunit
open FsUnit.CustomMatchers
open FsToolkit.ErrorHandling

open NSubstitute

open Songhay.Modules.Models
open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Modules.Bolero.RemoteHandlerUtility
open Songhay.Player.YouTube.YtUriUtility
open Xunit.Abstractions

type RemoteHandlerUtilityTests(outputHelper: ITestOutputHelper) =

    [<SkippableTheory>]
    [<InlineData(YtIndexSonghay, "songhay-index.json")>]
    member this.``getPlaylistIndexUri request test (async)`` (indexName: string, jsonFileName: string) =
        async {
            Skip.If(studioSettingsPath.IsNone, studioSettingsPathMessage)

            let uri = indexName |> Identifier.Alphanumeric |> model.getPlaylistIndexUri
            outputHelper.WriteLine uri.Value.OriginalString

            let mockLogger = Substitute.For<ILogger>() |> Some
            let dataGetter (result: Result<JsonElement, JsonException>) =
                result |> should be (ofCase <@ Result<JsonElement, JsonException>.Ok @>)
                result |> Option.ofResult

            let! responseResult = client |> trySendAsync (get uri.Value) |> Async.AwaitTask

            responseResult |> should be (ofCase <@ Result<HttpResponseMessage,exn>.Ok @>)

            let! handlerResult = responseResult |> toHandlerOutputAsync mockLogger dataGetter |> Async.AwaitTask
            handlerResult |> should be (ofCase <@ Option<JsonElement>.Some @>)

            (jsonFileName, handlerResult.Value.ToString()) ||> writeJsonAsync |> Async.AwaitTask |> ignore
        }
