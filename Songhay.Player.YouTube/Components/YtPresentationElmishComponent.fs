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

    static member EComp (model: YouTubeModel) dispatch =
        ecomp<YtPresentationElmishComponent, _, _> model dispatch { attr.empty() }

    static member val Id = "yt-presentation-block" with get

    override this.ShouldRender(oldModel, newModel) =
        oldModel.presentation <> newModel.presentation ||
        oldModel.presentationKey <> newModel.presentationKey ||
        oldModel.ytItems <> newModel.ytItems

    override this.View model dispatch =
        div {
            [ "rx"; "b-roll"; "video"; "presentation"; "yt" ] |> CssClasses.toHtmlClassFromList

            cond model.presentation.IsSome <| function
            | true ->
                concat {
                    YtThumbsContainerElmishComponent.EComp None { model with ytItems = model.ytItems } dispatch

                    div {
                        ["description" ] |> CssClasses.toHtmlClassFromList
                        rawHtml model.presentation.Value.description.Value
                    }
                }
            | false ->
                bulmaContainer
                    ContainerWidthFluid
                    (HasClasses (CssClasses [m (All, L6); elementTextAlign AlignCentered]))
                        (bulmaLoader
                            (HasClasses (CssClasses (imageContainer (Square Square128) @ [p (All, L6)]))))
        }
