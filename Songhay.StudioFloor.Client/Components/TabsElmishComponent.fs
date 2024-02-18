namespace Songhay.StudioFloor.Client.Components

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.BodyElement
open Songhay.Modules.Bolero.Visuals.Bulma.Element
open Songhay.Modules.Bolero.Visuals.Bulma.Layout

open Songhay.Player.YouTube.Components
open Songhay.StudioFloor.Client
open Songhay.StudioFloor.Client.Models

type TabsElmishComponent() =
    inherit ElmishComponent<StudioFloorModel, StudioFloorMessage>()

    static member EComp model dispatch =
        ecomp<TabsElmishComponent, _, _> model dispatch { attr.empty() }

    override this.ShouldRender(oldModel, newModel) =
        oldModel.page <> newModel.page
        || oldModel.readMeData <> newModel.readMeData
        || oldModel.ytModel <> newModel.ytModel

    override this.View model dispatch =
        let tabs = [
            ("README", ReadMePage)
            ("YouTube Thumbs", YtThumbsPage)
            ("YouTube Presentation", YtPresentationPage)
        ]

        concat {
            div {
                [
                    "tabs";
                    "has-background-grey-light";
                    "is-toggle";
                    "is-fullwidth";
                    "is-large"
                ] |> CssClasses.toHtmlClassFromList

                ul {
                    forEach tabs <| fun (label, pg) ->
                    li {
                        anchorElement
                            NoCssClasses
                            (HasAttr <| ElmishRoutes.router.HRef pg)
                            (text label)
                    }
                }
            }

            cond model.page <| function
            | ReadMePage ->
                if model.readMeData.IsNone then
                    text "loading…"
                else
                    bulmaContainer
                        ContainerWidthFluid
                        NoCssClasses
                        (bulmaNotification
                            (HasClasses (CssClasses [ "is-info" ] ))
                            (rawHtml model.readMeData.Value))
            | YtPresentationPage ->
                text "presentation"
            | YtThumbsPage ->
                YtThumbsComponent.EComp (Some "songhay tube") model.ytModel (StudioFloorMessage.YouTubeMessage >> dispatch)

            YtThumbsSetComponent.EComp model.ytModel (StudioFloorMessage.YouTubeMessage >> dispatch)
        }
