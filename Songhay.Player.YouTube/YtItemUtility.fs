namespace Songhay.Player.YouTube

open System
open System.Text.Json

open FsToolkit.ErrorHandling

open Microsoft.FSharp.Core
open Songhay.Modules.JsonDocumentUtility

open Songhay.Player.YouTube.Models
open Songhay.Player.YouTube.YouTubeScalars

module YtItemUtility =

    [<Literal>]
    let YtItemsPropertyName = "items"

    [<Literal>]
    let YtItemSnippetPropertyName = "snippet"

    [<Literal>]
    let YtItemContentDetailsPropertyName = "contentDetails"

    [<Literal>]
    let YtItemThumbnailsPropertyName = "thumbnails"

    let getYtItemsPair (item: YouTubeItem) =
        let uri = $"{YouTubeChannelRootUri}{item.snippet.channelId}"
        let title = item.snippet.channelTitle
        uri, title

    let tryGetYtContentDetails (element: JsonElement) : Result<YouTubeContentDetails, JsonException> =
        let videoIdResult = element |> tryGetProperty "videoId" |> Result.map (_.GetString())
        let videoPublishedAtResult =
            element
            |> tryGetProperty "videoPublishedAt"
            |> Result.map ( _.GetDateTime() )
        let durationResult = element |> tryGetProperty "duration" |> Result.map (_.GetString())
        let dimensionResult = element |> tryGetProperty "dimension" |> Result.map (_.GetString())
        let definitionResult = element |> tryGetProperty "definition" |> Result.map (_.GetString())
        let captionResult =
            element
            |> tryGetProperty "caption"
            |> Result.either
                (
                    fun el ->
                        let caption = el.GetString()
                        match Boolean.TryParse caption with
                        | false, _ -> resultError "caption"
                        | true, b -> Ok b
                )
                Result.Error
        let licensedContentResult = element |> tryGetProperty "licensedContent" |> Result.map (_.GetBoolean())
        let regionRestrictionResult =
            element
            |> tryGetProperty "regionRestriction"
            |> Result.bind (tryGetProperty "blocked")
            |> Result.eitherMap
                (
                    fun el ->
                        let blocked = el.EnumerateArray() |> Array.ofSeq |> Array.map (_.ToString())
                        {| blocked = blocked |}
                )
                id
        let projectionResult = element |> tryGetProperty "projection" |> Result.map (_.GetString())

        [
            durationResult |> Result.map (fun _ -> true)
            dimensionResult |> Result.map (fun _ -> true)
            definitionResult |> Result.map (fun _ -> true)
            captionResult |> Result.map (fun _ -> true)
            licensedContentResult |> Result.map (fun _ -> true)
            projectionResult |> Result.map (fun _ -> true)
        ]
        |> List.sequenceResultM
        |> Result.eitherMap
            (
                fun _ ->
                    {
                        videoId = videoIdResult |> Option.ofResult
                        videoPublishedAt = videoPublishedAtResult |> Option.ofResult
                        duration = durationResult |> Result.valueOr raise
                        dimension = dimensionResult |> Result.valueOr raise
                        definition = definitionResult |> Result.valueOr raise
                        caption = captionResult |> Result.valueOr raise
                        licensedContent = licensedContentResult |> Result.valueOr raise
                        projection = projectionResult |> Result.valueOr raise
                        regionRestriction = regionRestrictionResult |> Option.ofResult
                    }
            )
            id

    let tryGetYtThumbnail (element: JsonElement) : Result<YouTubeThumbnail, JsonException> =
        let urlResult = element |> tryGetProperty "url" |> Result.map (_.GetString())
        let widthResult = element |> tryGetProperty "width" |> Result.map (_.GetInt32())
        let heightResult = element |> tryGetProperty "height" |> Result.map (_.GetInt32())

        [
            urlResult |> Result.map (fun _ -> true)
            widthResult |> Result.map (fun _ -> true)
            heightResult |> Result.map (fun _ -> true)
        ]
        |> List.sequenceResultM
        |> Result.eitherMap
            (
                fun _ ->
                    {
                        url = urlResult |> Result.valueOr raise
                        width = widthResult |> Result.valueOr raise
                        height = heightResult |> Result.valueOr raise
                    }
            )
            id

    let tryGetYtThumbnails (element: JsonElement) =
        let thumbnailsResult = element |> tryGetProperty YtItemThumbnailsPropertyName |> Result.map id

        let defaultResult = thumbnailsResult |> Result.bind (tryGetProperty "default") |> Result.map id
        let mediumResult = thumbnailsResult |> Result.bind (tryGetProperty "medium") |> Result.map id
        let highResult = thumbnailsResult |> Result.bind (tryGetProperty "high") |> Result.map id

        [
            defaultResult
            mediumResult
            highResult
        ]
        |> List.sequenceResultM
        |> Result.eitherMap
            (
                fun _ ->
                    {
                        ``default`` = defaultResult |> Result.bind tryGetYtThumbnail |> Result.valueOr raise
                        medium = mediumResult |> Result.bind tryGetYtThumbnail |> Result.valueOr raise
                        high = highResult |> Result.bind tryGetYtThumbnail |> Result.valueOr raise
                        standard = None
                        maxres = None
                    }
            )
            id

    let tryGetYtResourceId (element: JsonElement) : Result<YouTubeResourceId, JsonException> =
        element
        |> tryGetProperty "resourceId"
        |> Result.bind (tryGetProperty "videoId")
        |> Result.map (fun el -> { videoId = el.GetString() })

    let tryGetYtSnippet (element: JsonElement)  : Result<YouTubeSnippet, JsonException> =

        let publishedAtResult =
            element
            |> tryGetProperty "publishedAt"
            |> Result.map ( _.GetDateTime() )
        let channelIdResult = element |> tryGetProperty "channelId" |> Result.map (_.GetString())
        let titleResult = element |> tryGetProperty "title" |> Result.map (_.GetString())
        let descriptionResult = element |> tryGetProperty "description" |> Result.map (_.GetString())
        let thumbnailsResult = element |> tryGetYtThumbnails
        let channelTitleResult = element |> tryGetProperty "channelTitle" |> Result.map (_.GetString())
        let playlistIdResult = element |> tryGetProperty "playlistId" |> Result.map (_.GetString())
        let positionResult = element |> tryGetProperty "position" |> Result.map (_.GetInt32())
        let resourceIdResult = element |> tryGetYtResourceId
        let tagsResult =
            element
            |> tryGetProperty "tags"
            |> Result.map (fun el -> el.EnumerateArray() |> Array.ofSeq |> Array.map (_.ToString()))

        let localizedResult = element |> tryGetProperty "localized" |> Result.map id
        let localizedDescResult =
            localizedResult
            |> Result.bind (tryGetProperty "description")
            |> Result.map (_.GetString())
        let localizedTitleResult =
            localizedResult
            |> Result.bind (tryGetProperty "title")
            |> Result.map (_.GetString())

        [
            publishedAtResult |> Result.map (fun _ -> true)
            channelIdResult |> Result.map (fun _ -> true)
            titleResult |> Result.map (fun _ -> true)
            descriptionResult |> Result.map (fun _ -> true)
            thumbnailsResult |> Result.map (fun _ -> true)
            channelTitleResult |> Result.map (fun _ -> true)
            localizedDescResult |> Result.map (fun _ -> true)
            localizedTitleResult |> Result.map (fun _ -> true)
        ]
        |> List.sequenceResultM
        |> Result.eitherMap
            (
                fun _ ->
                    {
                        channelId = channelIdResult |> Result.valueOr raise
                        channelTitle = channelTitleResult |> Result.valueOr raise
                        description = descriptionResult |> Result.valueOr raise
                        localized =
                            {|
                                description = localizedDescResult |> Result.valueOr raise
                                title = localizedTitleResult |> Result.valueOr raise
                            |}
                        playlistId = playlistIdResult |> Option.ofResult
                        position = positionResult |> Option.ofResult
                        publishedAt = publishedAtResult |> Result.valueOr raise
                        resourceId = resourceIdResult |> Option.ofResult
                        tags = tagsResult |> Option.ofResult
                        thumbnails = thumbnailsResult |> Result.valueOr raise
                        title = titleResult |> Result.valueOr raise
                    }
            )
            id

    let tryGetYtItem (element: JsonElement) : Result<YouTubeItem, JsonException> =
        let etagResult = element |> tryGetProperty "etag" |> Result.map (_.GetString())
        let idResult = element |> tryGetProperty "id" |> Result.map (_.GetString())
        let kindResult = element |> tryGetProperty "kind" |> Result.map (_.GetString())
        let snippetResult = element |> tryGetProperty YtItemSnippetPropertyName |> Result.bind tryGetYtSnippet
        let contentDetailsResult = element |> tryGetProperty YtItemContentDetailsPropertyName |> Result.bind tryGetYtContentDetails

        [
            etagResult |> Result.map (fun _ -> true)
            idResult |> Result.map (fun _ -> true)
            kindResult |> Result.map (fun _ -> true)
            snippetResult |> Result.map (fun _ -> true)
            contentDetailsResult |> Result.map (fun _ -> true)
        ]
        |> List.sequenceResultM
        |> Result.eitherMap
            (
                fun _ ->
                    {
                        etag = etagResult |> Result.valueOr raise
                        id = idResult |> Result.valueOr raise
                        kind = kindResult |> Result.valueOr raise
                        snippet = snippetResult |> Result.valueOr raise
                        contentDetails = contentDetailsResult |> Result.valueOr raise
                    }
            )
            id

    let fromInput (element: JsonElement) =
        element
        |> tryGetProperty YtItemsPropertyName
        |> Result.bind
            (
                fun el ->
                    el.EnumerateArray()
                    |> List.ofSeq
                    |> List.map (fun i -> i |> tryGetYtItem)
                    |> List.sequenceResultM
            )
