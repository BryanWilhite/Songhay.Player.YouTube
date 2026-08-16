namespace Songhay.Player.YouTube.Tests

open Microsoft.Extensions.Logging

open NSubstitute
open Xunit

open Songhay.Modules.Bolero.JsonDocumentUtility
open Songhay.Player.YouTube.ServiceHandlerUtility

type ServiceHandlerUtilityTests() =

    [<Theory>]
    [<InlineData("youtube-index-songhay-top-ten.json")>]
    [<InlineData("video-yt-bowie0-videos.json")>]
    member this.``toYtItems test`` (fileName: string) =
        let jsonResult = fileName |> getJson |> Ok
        let mockLogger = Substitute.For<ILogger>() |> Some

        let actual = (mockLogger, jsonResult) ||> tryGetJsonElement |> toYtItems
        actual.IsSome |> Assert.True

    [<Theory>]
    [<InlineData("songhay-code-playlist.json")>]
    [<InlineData("songhay-news-playlist.json")>]
    member this.``toYtSet test`` (fileName: string) =
        let jsonResult = fileName |> getJson |> Ok
        let mockLogger = Substitute.For<ILogger>() |> Some

        let actual = (mockLogger, jsonResult) ||> tryGetJsonElement |> toYtSet
        actual.IsSome |> Assert.True
