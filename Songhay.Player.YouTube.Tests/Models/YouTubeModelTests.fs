namespace Songhay.Player.YouTube.Tests.Models

open System
open System.Net
open Xunit
open Xunit.Abstractions
open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.HttpClientUtility
open Songhay.Modules.HttpRequestMessageUtility
open Songhay.Modules.HttpResponseMessageUtility
open Songhay.Player.YouTube.Tests.TestUtility

open Songhay.StudioFloor.Client.YouTubeScalars

type YouTubeModelTests(testOutputHelper: ITestOutputHelper) =

    static let downloadJsonAsync(uriOption: Uri option) (testOutputHelper: ITestOutputHelper) =
        task {
            let! responseResult = httpClient |> trySendAsync (get uriOption.Value)
            responseResult
            |> Result.teeError(fun err -> testOutputHelper.WriteLine $"{nameof err}: {err.Message}")
            |> _.IsOk |> Assert.True
            let response = responseResult |> Result.valueOr raise

            let! jsonResult = response |> tryDownloadToStringAsync
            jsonResult
            |> Result.teeError(fun status -> testOutputHelper.WriteLine $"{nameof status}: {status}")
            |> _.IsOk |> Assert.True

            let json =
                jsonResult
                |> Result.mapError ( fun code -> exn $"{nameof HttpStatusCode}: {code.ToString()}" )
                |> Result.valueOr raise

            return json
        }

    [<Theory>]
    [<InlineData(YtIndexSonghay)>]
    member this.``GetPlaylistIndexUri test`` (idString: string) =
        task {
            //arrange, act, assert:
            let id = Identifier.fromString(idString)
            let uriOption = id |> studioFloorModel.GetPlaylistIndexUri
            uriOption
            |> Option.teeSome(fun uri -> testOutputHelper.WriteLine $"{nameof uri}: {uri}")
            |> _.IsSome |> Assert.True

            //archive:
            let! json = testOutputHelper |> downloadJsonAsync uriOption
            do! json |> writeJsonAsync $"{idString}-index.json"
        }

    [<Theory>]
    [<InlineData(YtIndexSonghay, "news")>]
    [<InlineData(YtIndexSonghay, "code")>]
    [<InlineData(YtIndexSonghay, "media-building")>]
    member this.``GetPlaylistSetUri test`` (indexIdString: string, clientIdString: string) =

        testOutputHelper.WriteLine $"{nameof(indexIdString)}: {indexIdString}"
        testOutputHelper.WriteLine $"{nameof(clientIdString)}: {clientIdString}"

        task {
            //arrange, act, assert:
            let indexId = Identifier.fromString(indexIdString)
            let clientId = ClientId.fromString(clientIdString)
            let uriOption = (indexId, clientId) ||> studioFloorModel.GetPlaylistSetUri
            uriOption
            |> Option.teeSome(fun uri -> testOutputHelper.WriteLine $"{nameof uri}: {uri}")
            |> _.IsSome |> Assert.True

            //archive:
            let! json = testOutputHelper |> downloadJsonAsync uriOption
            do! json |> writeJsonAsync $"{indexIdString}-{clientIdString}-playlist.json"
        }

    [<Theory>]
    [<InlineData(YtIndexSonghayTopTen)>]
    member this.``GetPlaylistUri test`` (idString: string) =

        task {
            //arrange, act, assert:
            let id = Identifier.fromString(idString)
            let uriOption = id |> studioFloorModel.GetPlaylistUri
            uriOption
            |> Option.teeSome(fun uri -> testOutputHelper.WriteLine $"{nameof uri}: {uri}")
            |> _.IsSome |> Assert.True

            //archive:
            let! json = testOutputHelper |> downloadJsonAsync uriOption
            do! json |> writeJsonAsync $"{idString}.json"
        }

    [<Theory>]
    [<InlineData("default")>]
    [<InlineData("sha_cage")>]
    member this.``GetPresentationManifestUri test`` (presentationKey: string) =

        task {
            //arrange, act, assert:
            let uriOption = presentationKey |> studioFloorModel.GetPresentationManifestUri
            uriOption
            |> Option.teeSome(fun uri -> testOutputHelper.WriteLine $"{nameof uri}: {uri}")
            |> _.IsSome |> Assert.True

            //archive:
            let! json = testOutputHelper |> downloadJsonAsync uriOption
            do! json |> writeJsonAsync $"{presentationKey}_presentation.json"
        }

    [<Theory>]
    [<InlineData("default")>]
    [<InlineData("sha_cage")>]
    member this.``GetPresentationYtItemsUri test`` (presentationKey: string) =

        task {
            //arrange, act, assert:
            let uriOption = presentationKey |> studioFloorModel.GetPresentationYtItemsUri
            uriOption
            |> Option.teeSome(fun uri -> testOutputHelper.WriteLine $"{nameof uri}: {uri}")
            |> _.IsSome |> Assert.True

            //archive:
            let! json = testOutputHelper |> downloadJsonAsync uriOption
            do! json |> writeJsonAsync $"{presentationKey}_videos.json.json"
        }
