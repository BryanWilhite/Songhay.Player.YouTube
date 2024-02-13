namespace Songhay.Player.YouTube.Models

open Songhay.Modules.Models

type YouTubeVisualState =
    | YtSetIndexSelectedDocument of DisplayText * ClientId
    | YtSetOverlayIsVisible
    | YtSetIsRequested
    | YtSetRequestSelection