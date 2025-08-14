namespace Songhay.Player.YouTube.Tests

open Microsoft.Extensions.Logging

open NSubstitute

open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers

open Songhay.Modules.Models
open Songhay.Modules.Bolero.JsonDocumentUtility

open Songhay.Player.YouTube.Models
open Songhay.Player.YouTube.ServiceHandlerUtility

type ServiceHandlerUtilityTests() =

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
