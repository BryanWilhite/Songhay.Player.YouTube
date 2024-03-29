namespace Songhay.Player.YouTube

open System
open Songhay.Modules.Models

open Songhay.Player.YouTube.YouTubeScalars

module YtUriUtility =

    [<Literal>]
    let YtIndexSonghay = "songhay"

    [<Literal>]
    let YtIndexSonghayTopTen = "youtube-index-songhay-top-ten"

    let getPlaylistIndexUri (id: Identifier) =
        Uri($"{YouTubeApiRootUri}{YouTubeApiPlaylistsIndexPath}{id.StringValue}", UriKind.Absolute)

    let getPlaylistSetUri (indexId: Identifier) (clientId: ClientId) =
        let suffix = clientId.toIdentifier.StringValue
        Uri($"{YouTubeApiRootUri}{YouTubeApiPlaylistsPath}{indexId.StringValue}/{suffix}", UriKind.Absolute)

    let getPlaylistUri (id: Identifier) =
        Uri($"{YouTubeApiRootUri}{YouTubeApiPlaylistPath}{id.StringValue}", UriKind.Absolute)

    let getPresentationUri (id: Identifier) =
        Uri($"{YouTubeApiRootUri}{YouTubeApiVideosPath}{id.StringValue}", UriKind.Absolute)
