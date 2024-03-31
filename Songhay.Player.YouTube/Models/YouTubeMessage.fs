namespace Songhay.Player.YouTube.Models

open Microsoft.JSInterop

open Songhay.Modules.Bolero
open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Player.YouTube.Models

type YouTubeMessage =
    | Error of exn
    | GetYtManifestAndPlaylist of string
    | GotYtManifest of (Identifier * Presentation option)
    | CallYtItems | CalledYtItems of YouTubeItem[] option
    | CallYtIndexAndSet
    | CallYtSet of DisplayText * ClientId
    | CalledYtSet of (DisplayText * YouTubeItem []) [] option
    | CalledYtSetIndex of (ClientId * Name * (DisplayItemModel * ClientId []) []) option
    | ChangeVisualState of YouTubeVisualState
    | CloseYtSetOverlay
    | OpenYtSetOverlay
    | SelectYtSet

    member this.displayText =
        match this with
        | Error _ -> $"{nameof YouTubeMessage}.{nameof Error}"
        | CallYtItems -> $"{nameof YouTubeMessage}.{nameof CallYtItems}"
        | CalledYtItems _ -> $"{nameof YouTubeMessage}.{nameof CalledYtItems}"
        | CallYtIndexAndSet -> $"{nameof YouTubeMessage}.{nameof CallYtIndexAndSet}"
        | CallYtSet _ -> $"{nameof YouTubeMessage}.{nameof CallYtSet}"
        | CalledYtSet _ -> $"{nameof YouTubeMessage}.{nameof CalledYtSet}"
        | CalledYtSetIndex _ -> $"{nameof YouTubeMessage}.{nameof CalledYtSetIndex}"
        | ChangeVisualState _ -> $"{nameof YouTubeMessage}.{nameof ChangeVisualState}"
        | CloseYtSetOverlay -> $"{nameof YouTubeMessage}.{nameof CloseYtSetOverlay}"
        | GetYtManifestAndPlaylist _ -> $"{nameof YouTubeMessage}.{nameof GetYtManifestAndPlaylist}"
        | GotYtManifest _ -> $"{nameof YouTubeMessage}.{nameof GotYtManifest}"
        | OpenYtSetOverlay -> $"{nameof YouTubeMessage}.{nameof OpenYtSetOverlay}"
        | SelectYtSet -> $"{nameof YouTubeMessage}.{nameof SelectYtSet}"

    member this.failureMessage (jsRuntime: IJSRuntime option) ex =
        let ytFailureMsg = YouTubeMessage.Error ex

        if jsRuntime.IsSome then
            jsRuntime.Value |> JsRuntimeUtility.consoleErrorAsync [|
                $"{this.displayText} failure:", ex
            |] |> ignore

        ytFailureMsg
