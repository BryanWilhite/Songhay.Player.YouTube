namespace Songhay.Player.YouTube.Tests

open Xunit
open FsToolkit.ErrorHandling

open Songhay.Player.YouTube.DisplayItemModelUtility

module DisplayItemModelUtilityTests =

    [<Theory>]
    [<InlineData("songhay-index.json")>]
    let ``Index.fromInput test`` (fileName: string) =

        let videoJsonDocument = fileName |> getJsonDocument
        let actualResult =
            videoJsonDocument.RootElement
            |> Index.fromInput

        actualResult.IsOk
        |> Assert.True

    [<Theory>]
    [<InlineData("songhay-news-playlist.json")>]
    let ``ThumbsSet.fromInput test`` (fileName: string) =

        use videoJsonDocument = fileName |> getJsonDocument
        let actualResult =
            videoJsonDocument.RootElement
            |> ThumbsSet.fromInput

        actualResult.IsOk
        |> Assert.True

        let actual = actualResult |> Result.valueOr raise
        actual |> Assert.NotEmpty
