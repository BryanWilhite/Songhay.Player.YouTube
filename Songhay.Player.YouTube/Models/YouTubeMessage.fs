namespace Songhay.Player.YouTube.Models

open Bolero
open Microsoft.JSInterop

open Songhay.Modules.Bolero
open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Player.YouTube.Models

type YouTubeMessage =
    | Error of exn
    | GetYtManifestAndPlaylist of string
    | GotYtManifest of (Identifier * Presentation option)
    | CallYtItems of string | CalledYtItems of YouTubeItem[] option
    | CallYtIndexAndSetForThumbsSet
    | CallYtIndexAndSetForThumbsSetOverlay
    | CallYtSet of DisplayText * ClientId
    | CalledYtSet of (DisplayText * YouTubeItem []) [] option
    | CalledYtSetIndex of (ClientId * Name * (DisplayItemModel * ClientId []) []) option
    | ChangeVisualState of YouTubeVisualState
    | CloseYtSetOverlay
    | GotPresentationSection of HtmlRef
    | OpenYtSetOverlay
    | PresentationCreditsClick
    | SelectYtSet

    member this.DisplayText =
        match this with
        | Error _ -> $"{nameof YouTubeMessage}.{nameof Error}"
        | CallYtItems _ -> $"{nameof YouTubeMessage}.{nameof CallYtItems}"
        | CalledYtItems _ -> $"{nameof YouTubeMessage}.{nameof CalledYtItems}"
        | CallYtIndexAndSetForThumbsSet -> $"{nameof YouTubeMessage}.{nameof CallYtIndexAndSetForThumbsSet}"
        | CallYtIndexAndSetForThumbsSetOverlay -> $"{nameof YouTubeMessage}.{nameof CallYtIndexAndSetForThumbsSetOverlay}"
        | CallYtSet _ -> $"{nameof YouTubeMessage}.{nameof CallYtSet}"
        | CalledYtSet _ -> $"{nameof YouTubeMessage}.{nameof CalledYtSet}"
        | CalledYtSetIndex _ -> $"{nameof YouTubeMessage}.{nameof CalledYtSetIndex}"
        | ChangeVisualState _ -> $"{nameof YouTubeMessage}.{nameof ChangeVisualState}"
        | CloseYtSetOverlay -> $"{nameof YouTubeMessage}.{nameof CloseYtSetOverlay}"
        | GetYtManifestAndPlaylist _ -> $"{nameof YouTubeMessage}.{nameof GetYtManifestAndPlaylist}"
        | GotPresentationSection _ -> $"{nameof YouTubeMessage}.{nameof GotPresentationSection}"
        | GotYtManifest _ -> $"{nameof YouTubeMessage}.{nameof GotYtManifest}"
        | OpenYtSetOverlay -> $"{nameof YouTubeMessage}.{nameof OpenYtSetOverlay}"
        | PresentationCreditsClick -> $"{nameof YouTubeMessage}.{nameof PresentationCreditsClick}"
        | SelectYtSet -> $"{nameof YouTubeMessage}.{nameof SelectYtSet}"

    member this.FailureMessage (jsRuntime: IJSRuntime option) ex =
        let ytFailureMsg = YouTubeMessage.Error ex

        if jsRuntime.IsSome then
            jsRuntime.Value |> JsRuntimeUtility.consoleErrorAsync [|
                $"{this.DisplayText} failure:", ex
            |] |> ignore

        ytFailureMsg
