namespace Songhay.Player.YouTube.Models

open System

open FsToolkit.ErrorHandling
open Bolero

open Songhay.Modules.Bolero
open Songhay.Modules.Models
open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.ServiceProviderUtility
open Songhay.Modules.Publications.Models

open Songhay.Player.YouTube.Models
open Songhay.Player.YouTube.PresentationUtility

type YouTubeModel =
    {
        blazorServices: {| presentationContainerElementRef: HtmlRef option |}
        error: string option
        presentation: Presentation option
        presentationKey: Identifier option
        restApiMetadataOption: RestApiMetadata option
        ytItems: YouTubeItem[] option
        ytSet: (DisplayText * YouTubeItem []) [] option
        ytSetIndex: (ClientId * Name * (DisplayItemModel * ClientId []) []) option
        ytVisualStates: AppStateSet<YouTubeVisualState>
    }

    static member initialize (serviceProvider: IServiceProvider) =
        setBlazorServiceProvider serviceProvider
        {
            blazorServices = {| presentationContainerElementRef = None |}
            error = None
            presentation = None
            presentationKey = None
            restApiMetadataOption =
                "PlayerApi"
                |> RestApiMetadata.fromConfiguration (getIConfiguration())
                |> RestApiMetadata.toRestApiMetadataOption (getILogger().LogException)
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
        | CallYtIndexAndSetForThumbsSet ->
            { model with
                ytSet = None
                ytSetIndex = None
                ytVisualStates = model.ytVisualStates.addStates(YtSetIsRequested)
            }
        | CallYtIndexAndSetForThumbsSetOverlay ->
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
                                     .SetSelectedDocument(displayText, id)
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
                model with blazorServices = {| presentationContainerElementRef = elementRef |> Some |}
            }
        | GotYtManifest data ->
            model.SetComputedStyles()
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

    /// <summary>
    /// An <c>option</c> wrapper for <see cref="RestApiMetadata.GetClaim"/>.
    /// </summary>
    member this.GetClaim(key: string) =
        this.restApiMetadataOption
        |> Option.either _.GetClaim(key) (fun () -> None)

    /// <summary>
    /// An <c>option</c> wrapper for <see cref="RestApiMetadata.ToUriResultFromClaim"/>.
    /// </summary>
    member this.ToUriResultFromClaim(key: string, [<ParamArray>] args: string[]) =
        this.restApiMetadataOption
        |> Option.either
            (
                fun restApiMetadata ->
                    restApiMetadata.ToUriResultFromClaim(key, args)
                    |> Result.teeError (getILogger().LogException)
            )
            (fun () -> Result.Error <| exn $"The expected {nameof this.restApiMetadataOption} is not here.")

    /// <summary>Returns the URI for a b-roll player API endpoint</summary>
    /// <param name="indexId">fills in the <c>suffix</c> for the endpoint route</param>
    /// <remarks>
    /// The route for this endpoint is of the form <c>video/youtube/playlist/index/{suffix}</c>
    /// </remarks>
    member this.GetPlaylistIndexUri (indexId: Identifier) =
        this.ToUriResultFromClaim("route-for-video-yt-index", indexId.StringValue)
        |> Option.ofResult

    /// <summary>Returns the URI for a b-roll player API endpoint</summary>
    /// <param name="indexId">fills in the <c>suffix</c> for the endpoint route</param>
    /// <param name="clientId">fills in the client <c>id</c> for the endpoint route</param>
    /// <remarks>
    /// The route for this endpoint is of the form <c>video/youtube/playlists/{suffix}/{id}</c>
    /// </remarks>
    member this.GetPlaylistSetUri (indexId: Identifier) (clientId: ClientId) =
        let id = clientId.toIdentifier.StringValue
        this.ToUriResultFromClaim("route-for-video-yt-playlist-set", indexId.StringValue, id)
        |> Option.ofResult

    /// <summary>Returns the URI for a b-roll player API endpoint</summary>
    /// <param name="playlistFileName">fills in the <c>blobName</c> for the endpoint route (do not include any file extension as this member defaults to <c>.json</c>)</param>
    /// <remarks>
    /// The route for this endpoint is of the form <c>video/youtube/playlist/{subFolder}/{blobName}</c>
    /// where <c>subFolder</c> is hard-coded to <c>uploads</c>
    /// </remarks>
    member this.GetPlaylistUri (playlistFileName: Identifier) =
        this.ToUriResultFromClaim("route-for-video-yt-playlist", "uploads", playlistFileName.StringValue)
        |> Option.ofResult

    /// <summary>Returns the URI for a b-roll player API endpoint</summary>
    /// <param name="presentationKey">fills in the <c>presentationKey</c> for the endpoint route</param>
    /// <remarks>
    /// The route for this endpoint is of the form <c>video/youtube/{presentationKey}</c>
    /// </remarks>
    member this.GetPresentationManifestUri (presentationKey: string ) =
        this.ToUriResultFromClaim("route-for-video-yt-manifest", presentationKey)
        |> Option.ofResult

    /// <summary>Returns the URI for a b-roll player API endpoint</summary>
    /// <param name="presentationKey">fills in the <c>presentationKey</c> for the endpoint route</param>
    /// <remarks>
    /// The route for this endpoint is of the form <c>video/youtube/videos/{presentationKey}</c>
    /// </remarks>
    member this.GetPresentationYtItemsUri (presentationKey: string ) =
        this.ToUriResultFromClaim("route-for-video-yt-curated-manifest", presentationKey)
        |> Option.ofResult

    member this.GetSelectedDocument() =
        this.getVisualState(function YtSetIndexSelectedDocument (dt, id) -> Some (dt, id) | _ -> None)

    member this.GetSelectedDocumentClientId() = snd (this.GetSelectedDocument())

    member this.GetSelectedDocumentDisplayText() = fst (this.GetSelectedDocument())

    member this.SelectedDocumentEquals (clientId: ClientId) =
        clientId = this.GetSelectedDocumentClientId()

    member this.SetComputedStyles() =
        let jsRuntime = getIJSRuntime()

        option {
            let! elementRef = this.blazorServices.presentationContainerElementRef
            getConventionalCssProperties()
            |> List.iter
                    (
                        fun vv ->
                            let n, v = vv.Pair
            
                            jsRuntime
                                |> setComputedStylePropertyValueAsync elementRef n.Value v.Value
                                |> ignore
                    )
        }
        |> Option.defaultWith
            (
                fun _ ->
                    jsRuntime
                    |> consoleWarnAsync [|
                        $"{nameof this.SetComputedStyles} failed!"
                        if this.blazorServices.presentationContainerElementRef.IsNone then
                            "The reference to the Presentation container is not here."
                    |] |> ignore
            )

    member this.SetSelectedDocument (dt, id) : AppStateSet<YouTubeVisualState> =
        let current = this.GetSelectedDocument()
        this.ytVisualStates
            .removeState(YtSetIndexSelectedDocument current)
            .addState(YtSetIndexSelectedDocument (dt, id))
