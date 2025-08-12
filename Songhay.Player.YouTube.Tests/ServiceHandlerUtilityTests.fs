namespace Songhay.Player.YouTube.Tests

open System.Net
open System.IO
open System.Net.Http
open System.Reflection
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open NSubstitute

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
open Songhay.Modules.Bolero.JsonDocumentUtility

open Songhay.Player.YouTube.Models
open Songhay.Player.YouTube.ServiceHandlerUtility
open Songhay.Player.YouTube.YtUriUtility

type ServiceHandlerUtilityTests(testOutputHelper: ITestOutputHelper) =

    let projectDirectoryInfo =
        Assembly.GetExecutingAssembly()
        |> ProgramAssemblyInfo.getPathFromAssembly "../../../"
        |> Result.valueOr raiseProgramFileError
        |> DirectoryInfo

    let getJson (fileName: string) =
        let path =
            $"./json/{fileName}"
            |> tryGetCombinedPath projectDirectoryInfo.FullName
            |> Result.valueOr raiseProgramFileError
        File.ReadAllText(path)

    let client = new HttpClient()

    let provider = ServiceCollection().BuildServiceProvider()

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

    [<Theory>]
    [<InlineData(YtIndexSonghay, "news")>]
    [<InlineData(YtIndexSonghay, "code")>]
    [<InlineData(YtIndexSonghay, "media-building")>]
    member this.``getPlaylistSetUri test`` (indexIdString: string, clientIdString: string) =
        testOutputHelper.WriteLine $"{nameof(indexIdString)}: {indexIdString}"
        testOutputHelper.WriteLine $"{nameof(clientIdString)}: {clientIdString}"

        task {
            let indexId = Identifier.fromString(indexIdString)
            let clientId = ClientId.fromString(clientIdString)
            let uri = (indexId, clientId) ||> getPlaylistSetUri

            testOutputHelper.WriteLine $"{nameof(uri)}: {uri.OriginalString}"

            let! responseResult = client |> trySendAsync (get uri)
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

    [<Theory>]
    [<InlineData(YtIndexSonghayTopTen)>]
    member this.``getPlaylistUri test`` (idString: string) =
        task {
            let id = Identifier.fromString(idString)
            let uri = id |> getPlaylistUri
            let! responseResult = client |> trySendAsync (get uri)
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

    [<Theory>]
    [<InlineData("songhay/news", "called-yt-set-news")>]
    member this.``getYtSetKey test`` (pathSegments: string, expected: string) =
        let pathSegmentsArray = pathSegments.Split('/')
        pathSegmentsArray.Length |> should equal 2

        let tuple = ((Identifier.fromString pathSegmentsArray[0]), (ClientId.fromString pathSegmentsArray[1]))
        let uri = tuple ||> getPlaylistSetUri
        let seed = nameof YouTubeMessage.CalledYtSet
        let cacheKey = uri |> getYtSetKey seed
        cacheKey |> should equal expected

    [<Theory>]
    [<InlineData("youtube-index-songhay-top-ten.json")>]
    [<InlineData("video-yt-bowie0-videos.json")>]
    member this.``toYtItems test`` (fileName: string) =
        let jsonResult = fileName |> getJson |> Ok
        let mockLogger = Substitute.For<ILogger>() |> Some

        let actual = (mockLogger, jsonResult) ||> tryGetJsonElement |> toYtItems
        actual |> should be (ofCase <@ Option<YouTubeItem[]>.Some @>)

    [<Theory>]
    [<InlineData("songhay-code-playlist.json")>]
    [<InlineData("songhay-news-playlist.json")>]
    member this.``toYtSet test`` (fileName: string) =
        let jsonResult = fileName |> getJson |> Ok
        let mockLogger = Substitute.For<ILogger>() |> Some

        let actual = (mockLogger, jsonResult) ||> tryGetJsonElement |> toYtSet
        actual |> should be (ofCase <@ Option<(DisplayText * YouTubeItem array)[]>.Some @>)
