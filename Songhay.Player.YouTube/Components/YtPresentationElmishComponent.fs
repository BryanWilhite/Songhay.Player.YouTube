namespace Songhay.Player.YouTube.Components

open Bolero
open Bolero.Html

open Microsoft.AspNetCore.Components
open Songhay.Modules.Models
open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Modules.Bolero.Visuals.Bulma.Layout

open Songhay.Player.YouTube.Models

type YtPresentationElmishComponent() =
    inherit ElmishComponent<YouTubeModel, YouTubeMessage>()

    /// <summary><see cref="HtmlRef"/> for the <c>section</c> element in the <see cref="sectionNode"/></summary>
    let sectionElementRef = HtmlRef()

    let sectionNode model dispatch =
        section {
            [ "rx"; "b-roll"; "video"; "presentation"; "yt" ] |> CssClasses.toHtmlClassFromList

            attr.ref sectionElementRef

            cond (model.presentation.IsSome && model.ytItems.IsSome) <| function
            | true ->
                concat {
                    YtThumbsContainerElmishComponent.EComp None { model with ytItems = model.ytItems } dispatch

                    div {
                        [ "description"; p(All, L4); fontSize Size4 ] |> CssClasses.toHtmlClassFromList
                        rawHtml model.presentation.Value.description.Value
                    }
                    div {
                        "credits" |> CssClasses.toHtmlClass

                        (model, dispatch) ||> PresentationCreditsElmishComponent.EComp
                    }
                }
            | false ->
                bulmaContainer
                    ContainerWidthFluid
                    (HasClasses (CssClasses [m(All, L6); elementTextAlign AlignCentered]))
                        (bulmaLoader
                            (HasClasses (CssClasses (imageContainer (Square Square128) @ [p (All, L6)]))))
        }

    static member EComp (dispatchPresentationContainerElementRefOnce: bool) (model: YouTubeModel) dispatch =
        ecomp<YtPresentationElmishComponent, _, _> model dispatch
            {
                "DispatchPresentationContainerElementRefOnce" => dispatchPresentationContainerElementRefOnce
            }

    static member val Id = "yt-presentation-block" with get

     [<Parameter>]
     member val DispatchPresentationContainerElementRefOnce = true with get, set

    override this.ShouldRender(oldModel, newModel) =
        oldModel.ytVisualStates <> newModel.ytVisualStates ||
        oldModel.presentation <> newModel.presentation ||
        oldModel.presentationKey <> newModel.presentationKey ||
        oldModel.ytItems <> newModel.ytItems

    override this.View model dispatch =
        match this.DispatchPresentationContainerElementRefOnce with
        | false ->
            dispatch <| GotPresentationSection sectionElementRef
            ()
        | true when model.blazorServices.presentationContainerElementRef.IsNone ->
            dispatch <| GotPresentationSection sectionElementRef
            ()
        | true -> ()

        (model, dispatch) ||> sectionNode
