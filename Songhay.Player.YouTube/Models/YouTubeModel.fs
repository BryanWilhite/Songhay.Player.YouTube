namespace Songhay.Player.YouTube.Models

open System
open System.Net.Http

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open FsToolkit.ErrorHandling
open Bolero

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models

open Songhay.Modules.Bolero.JsRuntimeUtility

open Songhay.Player.YouTube.Models
open Songhay.Player.YouTube.PresentationUtility

type YouTubeModel =
    {
        blazorServices: {|
                          httpClient: HttpClient
                          jsRuntime: IJSRuntime
                          presentationContainerElementRef: HtmlRef option
                          navigationManager: NavigationManager
                        |}
        error: string option
        presentation: Presentation option
        presentationKey: Identifier option
        ytItems: YouTubeItem[] option
        ytSet: (DisplayText * YouTubeItem []) [] option
        ytSetIndex: (ClientId * Name * (DisplayItemModel * ClientId []) []) option
        ytVisualStates: AppStateSet<YouTubeVisualState>
    }

    static member initialize (httpClient: HttpClient) (jsRuntime: IJSRuntime) (navigationManager: NavigationManager) =
        {
            blazorServices = {|
                               httpClient = httpClient
                               jsRuntime = jsRuntime
                               presentationContainerElementRef = None
                               navigationManager = navigationManager
                            |}
            error = None
            presentation = None
            presentationKey = None
            ytItems = None
            ytSet = None
            ytSetIndex = None
            ytVisualStates = AppStateSet.initialize
                .addState(YtSetOverlayIsUntouched)
                .addState(YtSetIndexSelectedDocument (DisplayText "News", "news" |> ClientId.fromString))
        }

    static member updateModel (message: YouTubeMessage) (model: YouTubeModel) =
        let sort (list: (DisplayText * YouTubeItem[])[]) =
            list
            |> Array.sortBy (fun (displayText, _) ->
                displayText.Value.ToLowerInvariant().Replace("the", String.Empty).Trim())

        match message with
        | Error exn -> { model with error = Some exn.Message }
        | CalledYtItems items -> { model with ytItems = items }
        | CalledYtSet set ->
            { model with
                ytSet = set |> Option.map sort
                ytVisualStates = model.ytVisualStates.removeState YtSetIsRequested
            }
        | CalledYtSetIndex index ->
            { model with
                ytSetIndex = index
                ytVisualStates = model.ytVisualStates.removeState YtSetIsRequested
            }
        | CallYtIndexAndSet ->
            { model with
                ytSet = None
                ytSetIndex = None
                ytVisualStates = model.ytVisualStates.addStates(YtSetIsRequested, YtSetOverlayIsVisible)
            }
        | CallYtItems _ -> { model with ytItems = None }
        | CallYtSet (displayText, id) ->
            { model with
                ytSet = None
                ytVisualStates = model
                                     .setSelectedDocument(displayText, id)
                                     .removeState(YtSetRequestSelection)
            }
        | ChangeVisualState state ->
            match state with
            | YtSetIsRequested -> { model with ytVisualStates = model.ytVisualStates.addState YtSetIsRequested }
            | _ -> { model with ytVisualStates = model.ytVisualStates.toggleState state }
        | CloseYtSetOverlay -> { model with ytVisualStates = model.ytVisualStates.removeState(YtSetOverlayIsVisible) }
        | GetYtManifestAndPlaylist _ -> { model with presentation = None; ytItems = None }
        | GotPresentationSection elementRef ->
            {
                model with
                    blazorServices = {|
                                       httpClient = model.blazorServices.httpClient
                                       jsRuntime = model.blazorServices.jsRuntime
                                       presentationContainerElementRef = elementRef |> Some
                                       navigationManager = model.blazorServices.navigationManager
                                    |}
            }
        | GotYtManifest data ->
            model.setComputedStyles()
            {
                model with
                    presentation = data |> toPresentationOption
                    presentationKey = data |> fst |> Some 
            }

        | OpenYtSetOverlay ->
            {
                model with ytVisualStates = model.ytVisualStates
                                                .removeState(YtSetOverlayIsUntouched)
                                                .addState(YtSetOverlayIsVisible)
            }
        | PresentationCreditsClick ->
            { model with ytVisualStates =  model.ytVisualStates.toggleState PresentationCreditsModalVisible }
        | SelectYtSet -> { model with ytVisualStates = model.ytVisualStates.addState YtSetRequestSelection }

    member private this.getVisualState (getter: YouTubeVisualState -> 'o option) =
        this.ytVisualStates.states
        |> List.ofSeq
        |> List.choose getter
        |> List.head

    member this.getSelectedDocument() =
        this.getVisualState(function YtSetIndexSelectedDocument (dt, id) -> Some (dt, id) | _ -> None)

    member this.getSelectedDocumentClientId() = snd (this.getSelectedDocument())

    member this.getSelectedDocumentDisplayText() = fst (this.getSelectedDocument())

    member this.selectedDocumentEquals (clientId: ClientId) =
        clientId = this.getSelectedDocumentClientId()

    member this.setComputedStyles() =
        option {
            let! elementRef = this.blazorServices.presentationContainerElementRef
            getConventionalCssProperties()
            |> List.iter
                    (
                        fun vv ->
                            let n, v = vv.Pair
            
                            this.blazorServices.jsRuntime
                                |> setComputedStylePropertyValueAsync elementRef n.Value v.Value
                                |> ignore
                    )
        }
        |> Option.defaultWith
            (
                fun _ ->
                    this.blazorServices.jsRuntime
                    |> consoleWarnAsync [|
                        $"{nameof this.setComputedStyles} failed!"
                        if this.blazorServices.presentationContainerElementRef.IsNone then
                            "The reference to the Presentation container is not here."
                    |] |> ignore
            )

    member this.setSelectedDocument (dt, id) : AppStateSet<YouTubeVisualState> =
        let current = this.getSelectedDocument()
        this.ytVisualStates
            .removeState(YtSetIndexSelectedDocument current)
            .addState(YtSetIndexSelectedDocument (dt, id))
