namespace Songhay.Player.YouTube.Tests

open System.Text.Json
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers
open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models

open Songhay.Player.YouTube.DisplayItemModelUtility
open Songhay.Player.YouTube.Models

module DisplayItemModelUtilityTests =

    [<Theory>]
    [<InlineData("songhay-index.json")>]
    let ``Index.fromInput test`` (fileName: string) =

        let videoJsonDocument = fileName |> getJsonDocument
        let actualResult =
            videoJsonDocument.RootElement
            |> Index.fromInput

        actualResult
        |> should be (ofCase <@ Result<ClientId * Name * (DisplayItemModel * ClientId []) [],JsonException>.Ok @>)

    [<Theory>]
    [<InlineData("songhay-news-playlist.json")>]
    let ``ThumbsSet.fromInput test`` (fileName: string) =

        use videoJsonDocument = fileName |> getJsonDocument
        let actualResult =
            videoJsonDocument.RootElement
            |> ThumbsSet.fromInput

        actualResult
        |> should be (ofCase <@ Result<(DisplayText * YouTubeItem []) list,JsonException>.Ok @>)

        let actual = actualResult |> Result.valueOr raise
        actual |> should not' (be Empty)
