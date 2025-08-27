namespace Songhay.Player.YouTube.Components

open Bolero
open Bolero.Html
open Elmish

open Microsoft.AspNetCore.Components
open Songhay.Modules.Models
open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.SvgElement
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Modules.Bolero.Visuals.Bulma.Component
open Songhay.Modules.Bolero.Visuals.Bulma.Element
open Songhay.Modules.Bolero.Visuals.Bulma.Layout

open Songhay.Player.YouTube.Models

type YtThumbsSetElmishComponent() =
    inherit ElmishComponent<YouTubeModel, YouTubeMessage>()

    static let click = DomElementEvent.Click

    static let bulmaDropdown (dispatch: Dispatch<YouTubeMessage>) (model: YouTubeModel) =
        if model.ytSetIndex.IsNone then empty()
        else
            let _, segmentName, documents = model.ytSetIndex.Value
            let displayText = segmentName.Value |> Option.defaultWith (fun _ -> "[missing]")
            let isActive = model.ytVisualStates.hasState YtSetRequestSelection
            let callback = (fun _ -> SelectYtSet |> dispatch)
            let dropDownContent =
                forEach documents <| fun (display, _) ->
                    if display.displayText.IsSome then
                        let clientId = ClientId.fromIdentifier display.id
                        let itemIsActive = model.SelectedDocumentEquals clientId
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

    static let ytThumbsSetNode (dispatch: Dispatch<YouTubeMessage>) (model: YouTubeModel) isOverlayMode=
        let overlayClasses =
            CssClasses [
                "rx"
                "b-roll"
                if isOverlayMode then
                    "overlay"
                    match model.ytVisualStates.hasState(YtSetOverlayIsVisible) with
                    | true ->
                        "animate"
                        "fade-in"
                    | false ->
                        if not <| model.ytVisualStates.hasState(YtSetOverlayIsUntouched) then
                            "animate"
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

                        div { levelItem |> CssClasses.toHtmlClass ; (dispatch, model) ||> bulmaDropdown }
                        div {
                            [ levelItem; fontSize Size2 ] |> CssClasses.toHtmlClassFromList
                            text (model.GetSelectedDocumentDisplayText()).Value
                        }
                    }
                    cond isOverlayMode <| function
                    | true -> levelRight
                    | false -> empty()
                }
            | false ->
                nav {
                    [ levelContainer; m (All, L2)] |> CssClasses.toHtmlClassFromList

                    cond isOverlayMode <| function
                    | true -> levelRight
                    | false -> empty()
                }
            cond model.ytSet.IsSome <| function
            | true ->
                div {
                    "set" |> CssClasses.toHtmlClass

                    forEach model.ytSet.Value <| fun (_, items) ->
                        YtThumbsContainerElmishComponent.EComp None { model with ytItems = Some items } dispatch
                }
            | false ->
                bulmaContainer
                    ContainerWidthFluid
                    (HasClasses (CssClasses [m (All, L6); elementTextAlign AlignCentered]))
                        (bulmaLoader
                            (HasClasses (CssClasses (imageContainer (Square Square128) @ [p (All, L6)]))))
        }

    static member val Id = "yt-thumbs-set-block" with get

    static member EComp (isOverlayMode: bool) (model: YouTubeModel) dispatch =
        ecomp<YtThumbsSetElmishComponent, _, _> model dispatch { "YtSetOverlayModeIsEnabled" => isOverlayMode }

    [<Parameter>]
    member val YtSetOverlayModeIsEnabled = Unchecked.defaultof<bool> with get, set

    override this.ShouldRender(oldModel, newModel) =
        oldModel.ytVisualStates <> newModel.ytVisualStates
        || oldModel.ytSetIndex <> newModel.ytSetIndex
        || oldModel.ytSet <> newModel.ytSet

    override this.View model dispatch =
        (model, this.YtSetOverlayModeIsEnabled) ||> ytThumbsSetNode dispatch
