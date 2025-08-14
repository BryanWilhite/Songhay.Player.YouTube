namespace Songhay.Player.YouTube.Tests

open Microsoft.Extensions.Logging

open NSubstitute

open Xunit
open Xunit.Abstractions
open FsUnit.Xunit
open FsUnit.CustomMatchers

open Songhay.Modules.Models
open Songhay.Modules.Bolero.JsonDocumentUtility

open Songhay.Player.YouTube.Models
open Songhay.Player.YouTube.ServiceHandlerUtility
open Songhay.Player.YouTube.YtUriUtility

type ServiceHandlerUtilityTests(testOutputHelper: ITestOutputHelper) =

    [<SkippableTheory>]
    [<InlineData("songhay/news", "called-yt-set-news")>]
    member this.``getYtSetKey test`` (pathSegments: string, expected: string) =
        Skip.If(studioSettingsPath.IsNone, studioSettingsPathMessage)

        let pathSegmentsArray = pathSegments.Split('/')
        pathSegmentsArray.Length |> should equal 2

        let tuple = ((Identifier.fromString pathSegmentsArray[0]), (ClientId.fromString pathSegmentsArray[1]))
        let uriOption = tuple ||> model.getPlaylistSetUri
        uriOption |> should be (ofCase <@ Option.Some @>)

        testOutputHelper.WriteLine $"{nameof uriOption}: {uriOption.Value.OriginalString}"

        let seed = nameof YouTubeMessage.CalledYtSet
        let cacheKey = uriOption.Value |> getYtSetKey seed
        cacheKey |> should equal expected

        testOutputHelper.WriteLine $"{nameof cacheKey}: {cacheKey}"

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
