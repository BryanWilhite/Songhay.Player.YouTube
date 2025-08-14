namespace Songhay.Player.YouTube.Tests.Models

open System.Net
open System.IO
open System.Net.Http


open Xunit
open Xunit.Abstractions
open FsUnit.Xunit
open FsUnit.CustomMatchers
open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Modules.HttpResponseMessageUtility
open Songhay.Modules.ProgramFileUtility

open Songhay.Player.YouTube.Models
open Songhay.Player.YouTube.YtUriUtility
open Songhay.Player.YouTube.Tests.TestUtility

type YouTubeModelTests(testOutputHelper: ITestOutputHelper) =

    [<Theory>]
    [<InlineData(YtIndexSonghay)>]
    member this.``getPlaylistIndexUri test`` (idString: string) =
        task {
            let model = YouTubeModel.initialize(provider)
            let id = Identifier.fromString(idString)
            let uri = id |> model.getPlaylistIndexUri
            let! responseResult = client |> trySendAsync (get uri.Value)
            responseResult |> should be (ofCase <@ Result<HttpResponseMessage,exn>.Ok @>)
            let response = responseResult |> Result.valueOr raise

            let! jsonResult = response |> tryDownloadToStringAsync
            jsonResult |> should be (ofCase <@ Result<string,HttpStatusCode>.Ok @>)

            let json =
                jsonResult
                |> Result.mapError ( fun code -> exn $"{nameof HttpStatusCode}: {code.ToString()}" )
                |> Result.valueOr raise

            let path =
                $"./json/{idString}-index.json"
                |> tryGetCombinedPath projectDirectoryInfo.FullName
                |> Result.valueOr raiseProgramFileError

            File.WriteAllText(path, json)
        }

    [<SkippableTheory>]
    [<InlineData(YtIndexSonghay, "news")>]
    [<InlineData(YtIndexSonghay, "code")>]
    [<InlineData(YtIndexSonghay, "media-building")>]
    member this.``getPlaylistSetUri test`` (indexIdString: string, clientIdString: string) =
        Skip.If(studioSettingsPath.IsNone, studioSettingsPathMessage)

        testOutputHelper.WriteLine $"{nameof(indexIdString)}: {indexIdString}"
        testOutputHelper.WriteLine $"{nameof(clientIdString)}: {clientIdString}"

        task {
            let indexId = Identifier.fromString(indexIdString)
            let clientId = ClientId.fromString(clientIdString)
            let uri = (indexId, clientId) ||> model.getPlaylistSetUri
            uri |> should be (ofCase <@ Option.Some @>)

            testOutputHelper.WriteLine $"{nameof(uri)}: {uri.Value.OriginalString}"

            let! responseResult = client |> trySendAsync (get uri.Value)
            responseResult |> should be (ofCase <@ Result<HttpResponseMessage,exn>.Ok @>)
            let response = responseResult |> Result.valueOr raise

            let! jsonResult = response |> tryDownloadToStringAsync
            jsonResult |> should be (ofCase <@ Result<string,HttpStatusCode>.Ok @>)

            let json =
                jsonResult
                |> Result.mapError ( fun code -> exn $"{nameof HttpStatusCode}: {code.ToString()}" )
                |> Result.valueOr raise

            let path =
                $"./json/{indexIdString}-{clientIdString}-playlist.json"
                |> tryGetCombinedPath projectDirectoryInfo.FullName
                |> Result.valueOr raiseProgramFileError

            File.WriteAllText(path, json)
        }

    [<SkippableTheory>]
    [<InlineData(YtIndexSonghayTopTen)>]
    member this.``getPlaylistUri test`` (idString: string) =
        Skip.If(studioSettingsPath.IsNone, studioSettingsPathMessage)

        task {
            let id = Identifier.fromString(idString)
            let uri = id |> model.getPlaylistUri
            uri |> should be (ofCase <@ Option.Some @>)

            let! responseResult = client |> trySendAsync (get uri.Value)
            responseResult |> should be (ofCase <@ Result<HttpResponseMessage,exn>.Ok @>)
            let response = responseResult |> Result.valueOr raise

            let! jsonResult = response |> tryDownloadToStringAsync
            jsonResult |> should be (ofCase <@ Result<string,HttpStatusCode>.Ok @>)
            let json =
                jsonResult
                |> Result.mapError ( fun code -> exn $"{nameof HttpStatusCode}: {code.ToString()}" )
                |> Result.valueOr raise

            let path =
                $"./json/{idString}.json"
                |> tryGetCombinedPath projectDirectoryInfo.FullName
                |> Result.valueOr raiseProgramFileError

            File.WriteAllText(path, json)
        }
