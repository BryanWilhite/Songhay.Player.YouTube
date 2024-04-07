namespace Songhay.Player.YouTube.Models

open Songhay.Modules.Models

type YouTubeVisualState =
    | PresentationCreditsModalVisible
    | YtSetIndexSelectedDocument of DisplayText * ClientId
    | YtSetOverlayIsVisible
    | YtSetOverlayIsUntouched
    | YtSetIsRequested
    | YtSetRequestSelection
