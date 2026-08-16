namespace Songhay.Player.YouTube.Tests

open System.Text.Json
open Microsoft.Extensions.Logging
open Xunit
open Xunit.Abstractions

open FsToolkit.ErrorHandling

open NSubstitute

open Songhay.Modules.Models
open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Modules.Bolero.RemoteHandlerUtility

open Songhay.StudioFloor.Client.YouTubeScalars

type RemoteHandlerUtilityTests(outputHelper: ITestOutputHelper) =

    [<SkippableTheory>]
    [<InlineData(YtIndexSonghay, "songhay-index.json")>]
    member this.``getPlaylistIndexUri request test (async)`` (indexName: string, jsonFileName: string) =
        async {
            Skip.If(studioSettingsPath.IsNone, studioSettingsPathMessage)

            let uri = indexName |> Identifier.Alphanumeric |> model.GetPlaylistIndexUri
            outputHelper.WriteLine uri.Value.OriginalString

            let mockLogger = Substitute.For<ILogger>() |> Some
            let dataGetter (result: Result<JsonElement, JsonException>) =
                result.IsOk |> Assert.True
                result |> Option.ofResult

            let! responseResult = client |> trySendAsync (get uri.Value) |> Async.AwaitTask

            responseResult.IsOk |> Assert.True

            let! handlerResult = responseResult |> toHandlerOutputAsync mockLogger dataGetter |> Async.AwaitTask
            handlerResult.IsSome |> Assert.True

            (jsonFileName, handlerResult.Value.ToString()) ||> writeJsonAsync |> Async.AwaitTask |> ignore
        }
