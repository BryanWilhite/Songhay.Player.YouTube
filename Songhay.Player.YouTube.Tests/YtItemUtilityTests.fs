namespace Songhay.Player.YouTube.Tests

open Xunit
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

open Songhay.Modules.JsonDocumentUtility
open Songhay.Player.YouTube.YtItemUtility

type YtItemUtilityTests() =

    [<Theory>]
    [<InlineData("60-minutes.json")>]
    [<InlineData("youtube-index-songhay-top-ten.json")>]
    member this.``should have `items` property`` (fileName: string) =
        let videoJsonDocument = fileName |> getJsonDocument
        let actual =
            match videoJsonDocument.RootElement.TryGetProperty YtItemsPropertyName with
            | true, _ -> true
            | _ -> false

        actual |> Assert.True

    [<Theory>]
    [<InlineData("60-minutes.json")>]
    [<InlineData("youtube-index-songhay-top-ten.json")>]
    member this.``tryGetYtContentDetails test`` (fileName: string) =
        let videoJsonDocument = fileName |> getJsonDocument
        let itemElementResult =
            videoJsonDocument.RootElement
            |> tryGetProperty YtItemsPropertyName
            |> Result.eitherMap ( fun el -> (el.EnumerateArray() |> List.ofSeq |> List.head) ) id

        itemElementResult.IsOk |> Assert.True

        let contentDetailsElementResult =
            itemElementResult |> Result.bind (tryGetProperty YtItemContentDetailsPropertyName)

        contentDetailsElementResult.IsOk |> Assert.True

        let actualResult =
            contentDetailsElementResult
            |> Result.valueOr raise
            |> tryGetYtContentDetails

        actualResult.IsOk |> Assert.True

    [<Theory>]
    [<InlineData("60-minutes.json")>]
    member this.``tryGetYtResourceId test`` (fileName: string) =
        let videoJsonDocument = fileName |> getJsonDocument
        let itemElementResult =
            videoJsonDocument.RootElement
            |> tryGetProperty YtItemsPropertyName
            |> Result.eitherMap ( fun el -> (el.EnumerateArray() |> List.ofSeq |> List.head) ) id

        itemElementResult.IsOk |> Assert.True

        let snippetElementResult =
            itemElementResult >>= (tryGetProperty YtItemSnippetPropertyName)
        snippetElementResult.IsOk |> Assert.True

        let actualResult =
            snippetElementResult
            |> Result.valueOr raise
            |> tryGetYtResourceId

        actualResult.IsOk |> Assert.True

    [<Theory>]
    [<InlineData("60-minutes.json")>]
    [<InlineData("youtube-index-songhay-top-ten.json")>]
    member this.``tryGetYtThumbnails test`` (fileName: string) =
        let videoJsonDocument = fileName |> getJsonDocument
        let itemElementResult =
            videoJsonDocument.RootElement
            |> tryGetProperty YtItemsPropertyName
            |> Result.eitherMap ( fun el -> (el.EnumerateArray() |> List.ofSeq |> List.head) ) id

        itemElementResult.IsOk |> Assert.True

        let snippetElementResult =
            itemElementResult |> Result.bind (tryGetProperty YtItemSnippetPropertyName)
        snippetElementResult.IsOk |> Assert.True

        let actualResult =
            snippetElementResult
            |> Result.valueOr raise
            |> tryGetYtThumbnails

        actualResult.IsOk |> Assert.True

    [<Theory>]
    [<InlineData("60-minutes.json")>]
    [<InlineData("youtube-index-songhay-top-ten.json")>]
    member this.``tryGetYtSnippet test`` (fileName: string) =
        let videoJsonDocument = fileName |> getJsonDocument
        let itemElementResult =
            videoJsonDocument.RootElement
            |> tryGetProperty YtItemsPropertyName
            |> Result.eitherMap ( fun el -> (el.EnumerateArray() |> List.ofSeq |> List.head) ) id

        itemElementResult.IsOk |> Assert.True

        let snippetElementResult =
            itemElementResult >>= (tryGetProperty YtItemSnippetPropertyName)
        snippetElementResult.IsOk |> Assert.True

        let actualResult =
            snippetElementResult
            |> Result.valueOr raise
            |> tryGetYtSnippet

        actualResult.IsOk |> Assert.True

    [<Theory>]
    [<InlineData("60-minutes.json")>]
    [<InlineData("youtube-index-songhay-top-ten.json")>]
    member this.``tryGetYtItem test`` (fileName: string) =
        let videoJsonDocument = fileName |> getJsonDocument
        let itemElementResult =
            videoJsonDocument.RootElement
            |> tryGetProperty YtItemsPropertyName
            |> Result.eitherMap ( fun el -> (el.EnumerateArray() |> List.ofSeq |> List.head) ) id

        itemElementResult.IsOk |> Assert.True

        let actualResult =
            itemElementResult
            |> Result.valueOr raise
            |> tryGetYtItem

        actualResult.IsOk |> Assert.True

    [<Theory>]
    [<InlineData("60-minutes.json")>]
    [<InlineData("youtube-index-songhay-top-ten.json")>]
    member this.``fromInput test`` (fileName: string) =

        use videoJsonDocument = fileName |> getJsonDocument
        let actualResult =
            videoJsonDocument.RootElement
            |> fromInput

        actualResult.IsOk |> Assert.True
