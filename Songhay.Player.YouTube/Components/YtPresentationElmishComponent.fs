namespace Songhay.Player.YouTube.Components

open Bolero
open Bolero.Html

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

            model.blazorServices.jsRuntime
            |> Songhay.Modules.Bolero.JsRuntimeUtility.consoleWarnAsync [|
                $"{nameof YtPresentationElmishComponent} can render section: {(model.presentation.IsSome && model.ytItems.IsSome)}"
                $"has presentation data: {model.presentation.IsSome}"
                $"has ytItems data: {model.ytItems.IsSome}"
            |] |> ignore
            cond (model.presentation.IsSome && model.ytItems.IsSome) <| function
            | true ->
                concat {
                    YtThumbsContainerElmishComponent.EComp None { model with ytItems = model.ytItems } dispatch

                    div {
                        [ "description"; p(All, L4); Size4.Value ] |> CssClasses.toHtmlClassFromList
                        rawHtml model.presentation.Value.description.Value
                    }

                    (model, dispatch) ||> PresentationCreditsElmishComponent.EComp
                }
            | false ->
                bulmaContainer
                    ContainerWidthFluid
                    (HasClasses (CssClasses [m(All, L6); elementTextAlign AlignCentered]))
                        (bulmaLoader
                            (HasClasses (CssClasses (imageContainer (Square Square128) @ [p (All, L6)]))))
        }

    static member EComp (model: YouTubeModel) dispatch =
        ecomp<YtPresentationElmishComponent, _, _> model dispatch { attr.empty() }

    static member val Id = "yt-presentation-block" with get

    override this.ShouldRender(oldModel, newModel) =
        oldModel.presentation <> newModel.presentation ||
        oldModel.presentationKey <> newModel.presentationKey ||
        oldModel.ytItems <> newModel.ytItems

    override this.View model dispatch =
        // if model.blazorServices.presentationContainerElementRef.IsNone then
        //     dispatch <| GotPresentationSection sectionElementRef
        // else
        //     ()

        (model, dispatch) ||> sectionNode
