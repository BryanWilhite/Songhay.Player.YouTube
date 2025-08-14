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

    let getPresentationUri (id: Identifier) =
        Uri($"{YouTubeApiRootUri}{YouTubeApiVideosPath}{id.StringValue}", UriKind.Absolute)

    let getYtSetKey seed (uri: Uri) =
        let prefix = seed |> toKabobCase |> Option.defaultValue String.Empty
        $"{prefix}-{uri.Segments |> Array.last}"
