namespace Songhay.Player.YouTube.Components

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

open Bolero
open Bolero.Html
open Elmish

open Songhay.Modules.Models
open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.SvgElement
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Modules.Bolero.Visuals.Bulma.Component
open Songhay.Modules.Bolero.Visuals.Bulma.Element
open Songhay.Modules.Bolero.Visuals.Bulma.Layout

open Songhay.Player.YouTube.Models

type YtThumbsSetComponent() =
    inherit ElmishComponent<YouTubeModel, YouTubeMessage>()

    static let click = DomElementEvent.Click

    static let bulmaDropdown (dispatch: Dispatch<YouTubeMessage>) (_: IJSRuntime) (model: YouTubeModel) =
        if model.ytSetIndex.IsNone then empty()
        else
            let _, segmentName, documents = model.ytSetIndex.Value
            let displayText = segmentName.Value |> Option.defaultWith (fun _ -> "[missing]")
            let isActive = model.ytSetRequestSelection
            let callback = (fun _ -> SelectYtSet |> dispatch)
            let dropDownContent =
                forEach documents <| fun (display, _) ->
                    if display.displayText.IsSome then
                        let clientId = ClientId.fromIdentifier display.id
                        let itemIsActive = (clientId = snd model.ytSetIndexSelectedDocument)
                        let itemCallback = (fun _ -> CallYtSet (display.displayText.Value, clientId) |> dispatch)
                        let itemDisplayText =
                            (display.displayText |> Option.defaultWith (fun _ -> DisplayText "[missing]")).Value

                        (itemIsActive, itemCallback, itemDisplayText) |||> bulmaDropdownItem

                    else empty()

            dropDownContent |> bulmaDropdown isActive displayText callback

    static let ytSetOverlayCloseCommand (dispatch: Dispatch<YouTubeMessage>) =
        a {
            imageContainer (Square Square48) |> CssClasses.toHtmlClassFromList
            attr.href "#"
            attr.title "close b-roll overlay"
            click.PreventDefault
            on.click (fun _ -> YouTubeMessage.CloseYtSetOverlay |> dispatch)

            svgElement (bulmaIconSvgViewBox Square24) (SonghaySvgData.Get(SonghaySvgKeys.MDI_CLOSE_BOX_24PX.ToAlphanumeric))
        }

    static let ytThumbsSetNode (dispatch: Dispatch<YouTubeMessage>) (jsRuntime: IJSRuntime) (model: YouTubeModel) =
        let overlayClasses =
            CssClasses [
                "rx"
                "b-roll"
                "overlay"
                match model.ytSetOverlayIsVisible with
                | None -> ()
                | Some b ->
                    "animate"
                    if b then
                        "fade-in"
                    else
                        "fade-out"
            ]

        let levelRight =
            div {
                level AlignRight |> CssClasses.toHtmlClass

                div { levelItem |> CssClasses.toHtmlClass; ytSetOverlayCloseCommand dispatch }
            }

        div {
            overlayClasses.ToHtmlClassAttribute
            cond model.ytSetIndex.IsSome <| function
            | true ->
                nav {
                    [ levelContainer; m (All, L2)] |> CssClasses.toHtmlClassFromList

                    div {
                        level AlignLeft |> CssClasses.toHtmlClass

                        div { levelItem |> CssClasses.toHtmlClass ; (dispatch, jsRuntime, model) |||> bulmaDropdown }
                        div {
                            [ levelItem; fontSize Size2 ] |> CssClasses.toHtmlClassFromList
                            text (fst model.ytSetIndexSelectedDocument).Value
                        }
                    }
                    levelRight
                }
            | false ->
                nav {
                    [ levelContainer; m (All, L2)] |> CssClasses.toHtmlClassFromList

                    levelRight
                }
            cond model.ytSet.IsSome <| function
            | true ->
                div {
                    "set" |> CssClasses.toHtmlClass

                    forEach model.ytSet.Value <| fun (_, items) ->
                        YtThumbsComponent.EComp None { model with ytItems = Some items } dispatch
                }
            | false ->
                bulmaContainer
                    ContainerWidthFluid
                    (HasClasses (CssClasses [m (All, L6); elementTextAlign AlignCentered]))
                        (bulmaLoader
                            (HasClasses (CssClasses (imageContainer (Square Square128) @ [p (All, L6)]))))
        }

    static member val Id = "yt-thumbs-set-block" with get

    static member EComp (model: YouTubeModel) dispatch =
        ecomp<YtThumbsSetComponent, _, _> model dispatch { attr.empty() }

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.ShouldRender(oldModel, newModel) =
        (oldModel.ytSetIndex <> newModel.ytSetIndex)
        || (oldModel.ytSet <> newModel.ytSet)
        || (oldModel.ytSetIndexSelectedDocument <> newModel.ytSetIndexSelectedDocument)
        || (oldModel.ytSetOverlayIsVisible <> newModel.ytSetOverlayIsVisible)
        || (oldModel.ytSetRequestSelection <> newModel.ytSetRequestSelection)

    override this.View model dispatch =
        model |> ytThumbsSetNode dispatch this.JSRuntime
