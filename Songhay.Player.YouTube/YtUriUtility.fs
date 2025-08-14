namespace Songhay.Player.YouTube

open System
open Songhay.Modules.Models
open Songhay.Modules.StringUtility

open Songhay.Player.YouTube.YouTubeScalars

module YtUriUtility =

    [<Literal>]
    let YtIndexSonghay = "songhay"

    [<Literal>]
    let YtIndexSonghayTopTen = "youtube-index-songhay-top-ten"

    let getPlaylistUri (id: Identifier) =
        Uri($"{YouTubeApiRootUri}{YouTubeApiPlaylistPath}{id.StringValue}", UriKind.Absolute)

    let getPresentationManifestUri (presentationKey: string ) =
        ($"{rxPlayerVideoRoot}{presentationKey}/{presentationKey}_presentation.json", UriKind.Absolute) |> Uri

    let getPresentationUri (id: Identifier) =
        Uri($"{YouTubeApiRootUri}{YouTubeApiVideosPath}{id.StringValue}", UriKind.Absolute)

    let getPresentationYtItemsUri (presentationKey: string ) =
        ($"{rxPlayerVideoRoot}{presentationKey}/youtube-videos.json", UriKind.Absolute) |> Uri

    let getYtSetKey seed (uri: Uri) =
        let prefix = seed |> toKabobCase |> Option.defaultValue String.Empty
        $"{prefix}-{uri.Segments |> Array.last}"
