namespace Songhay.Player.YouTube

module YouTubeScalars =

    /// <summary> conventional scalar </summary>
    [<Literal>]
    let rxAkyinkyinSvgDataUrl = "'data:image/svg+xml,<svg xmlns:svg=\"http://www.w3.org/2000/svg\" xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"744.09003\" height=\"1052.3622\"><g transform=\"matrix(8.9893045,0,0,-8.9893045,-789.08414,4880.2518)\"><path d=\"m 144.81201,454.46436 0,-19.54541 -8.40381,0 0,19.54541 -13.88574,0 0,-21.02832 -8.40381,0 0,21.02832 -13.88672,0 0,-22.51123 -8.4038,0 0,30.13232 67.04394,0 c 0.0117,6.51318 -3.7041,11.43115 -10.52783,11.43115 l -56.08203,0 0,7.62061 c 0,10.45361 8.20215,19.07519 18.7583,19.05224 l 48.84033,-0.10547 c 0.0278,7.21192 -4.18799,11.45557 -11.5166,11.43116 l -56.08203,-0.1875 0,7.6206 c 0,10.46045 8.20996,19.05176 18.7583,19.05176 l 55.5542,0 0,-7.62061 -55.5542,0 c -7.82959,0 -10.36475,-4.56347 -11.36963,-11.33984 l 48.69336,0.0962 c 10.5581,0.021 18.7583,-8.59814 18.7583,-19.05176 l 0,-7.62109 -56.08203,0.10547 c -7.33106,0.0142 -11.54444,-4.20898 -11.5166,-11.43115 l 48.84033,0 c 10.55029,0 18.7583,-8.59229 18.7583,-19.05176 l 0,-25.68359 -8.40381,0 0,18.0625 -13.88672,0 z\" style=\"fill:%23006633;fill-opacity:0.7;fill-rule:nonzero;stroke:none\"/></g></svg>'"

    /// <summary> conventional scalar </summary>
    [<Literal>]
    let rxPlayerVideoRoot = "https://songhaystorage.blob.core.windows.net/player-video/"

    /// <summary>
    /// base URI representing the b-roll endpoint
    /// for YouTube curation
    /// </summary>
    [<Literal>]
    let YouTubeApiRootUri = "https://songhay-system-player.azurewebsites.net/api/Player/v1/video/youtube/"

    /// <summary>
    /// URI path representing a YouTube channel
    /// and/or YouTube `uploads`
    /// curated as a `playlist`
    /// </summary>
    [<Literal>]
    let YouTubeApiPlaylistPath = "playlist/uploads/"

    /// <summary>
    /// URI path representing sets of YouTube `uploads`
    /// curated as `playlists`
    /// </summary>
    [<Literal>]
    let YouTubeApiPlaylistsPath = "playlists/"

    /// <summary>
    /// URI path representing indices of curated as `playlists`
    /// in @type {GenericWebIndex} format
    /// </summary>
    [<Literal>]
    let YouTubeApiPlaylistsIndexPath = "playlist/index/"

    /// <summary>
    /// URI path representing curated `videos`
    /// or YouTube `items` for a gen-web
    /// </summary>
    [<Literal>]
    let YouTubeApiVideosPath = "videos/"

    /// <summary>
    /// base URI representing YouTube channel browsing
    /// </summary>
    [<Literal>]
    let YouTubeChannelRootUri = "https://www.youtube.com/channel/"

    /// <summary>
    /// base URI representing YouTube item watching
    /// </summary>
    [<Literal>]
    let YouTubeWatchRootUri = "https://www.youtube.com/watch?v="
