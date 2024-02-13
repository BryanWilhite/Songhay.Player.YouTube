namespace Songhay.Player.YouTube.Models

open System
open System.Net.Http

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models
open Songhay.Player.YouTube.Models

type YouTubeModel =
    {
        blazorServices: {| httpClient: HttpClient; jsRuntime: IJSRuntime; navigationManager: NavigationManager |}
        error: string option
        ytItems: YouTubeItem[] option
        ytSet: (DisplayText * YouTubeItem []) [] option
        ytSetIndex: (ClientId * Name * (DisplayItemModel * ClientId []) []) option
        ytSetIndexSelectedDocument: DisplayText * ClientId
        ytSetOverlayIsVisible: bool option
        ytSetIsRequested: bool
        ytSetRequestSelection: bool
        ytVisualStates: AppStateSet<YouTubeVisualState>
    }

    static member initialize (httpClient: HttpClient) (jsRuntime: IJSRuntime) (navigationManager: NavigationManager) =
        {
            blazorServices = {| httpClient = httpClient; jsRuntime = jsRuntime; navigationManager = navigationManager |}
            error = None
            ytItems = None
            ytSet = None
            ytSetIndex = None
            ytSetIndexSelectedDocument = (DisplayText "News", "news" |> ClientId.fromString)
            ytSetOverlayIsVisible = None
            ytSetIsRequested = false
            ytSetRequestSelection = false
            ytVisualStates = AppStateSet.initialize
                .addState (YtSetIndexSelectedDocument (DisplayText "News", "news" |> ClientId.fromString))
        }

    static member updateModel (message: YouTubeMessage) (model: YouTubeModel) =
        let sort (list: (DisplayText * YouTubeItem[])[]) =
            list
            |> Array.sortBy (fun (displayText, _) ->
                displayText.Value.ToLowerInvariant().Replace("the", String.Empty).Trim())

        match message with
        | Error exn -> { model with error = Some exn.Message }
        | CalledYtItems items -> { model with ytItems = items }
        | CalledYtSet set -> { model with ytSet = set |> Option.map sort; ytSetIsRequested = false }
        | CalledYtSetIndex index -> { model with ytSetIndex = index; ytSetIsRequested = false }
        | CallYtIndexAndSet -> { model with ytSet = None; ytSetIndex = None; ytSetOverlayIsVisible = Some true; ytSetIsRequested = true }
        | CallYtItems -> { model with ytItems = None }
        | CallYtSet (displayText, id) -> { model with ytSet = None; ytSetIndexSelectedDocument = (displayText, id); ytSetRequestSelection = false }
        | ChangeVisualState state ->
            match state with
            | _ -> { model with ytVisualStates = model.ytVisualStates.toggleState state }
        | CloseYtSetOverlay -> { model with ytSetOverlayIsVisible = Some false }
        | OpenYtSetOverlay -> { model with ytSetOverlayIsVisible = Some true }
        | SelectYtSet -> { model with ytSetRequestSelection = true }
