namespace Songhay.StudioFloor.Client.Components

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.Element
open Songhay.Modules.Bolero.Visuals.Bulma.Layout

open Songhay.Player.YouTube.Components
open Songhay.StudioFloor.Client.Models

type TabsElmishComponent() =
    inherit ElmishComponent<StudioFloorModel, StudioFloorMessage>()

    static member EComp model dispatch =
        ecomp<TabsElmishComponent, _, _> model dispatch { attr.empty() }

    override this.ShouldRender(oldModel, newModel) =
        oldModel.tab <> newModel.tab
        || oldModel.readMeData <> newModel.readMeData
        || oldModel.ytModel <> newModel.ytModel

    override this.View model dispatch =
        let tabs = [
            ("README", ReadMeTab)
            ("YouTube Thumbs", YtThumbsTab)
            ("YouTube Presentation", YtPresentationTab)
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
                        a {
                            attr.href "#"
                            DomElementEvent.Click.PreventDefault
                            on.click (fun _ -> SetTab pg |> dispatch)
                            text label
                        }
                    }
                }
            }

            cond model.tab <| function
            | ReadMeTab ->
                if model.readMeData.IsNone then
                    text "loading…"
                else
                    bulmaContainer
                        ContainerWidthFluid
                        NoCssClasses
                        (bulmaNotification
                            (HasClasses (CssClasses [ "is-info" ] ))
                            (rawHtml model.readMeData.Value))
            | YtPresentationTab ->
                text "presentation"
            | YtThumbsTab ->
                YtThumbsComponent.EComp (Some "songhay tube") model.ytModel (StudioFloorMessage.YouTubeMessage >> dispatch)

            YtThumbsSetComponent.EComp model.ytModel (StudioFloorMessage.YouTubeMessage >> dispatch)
        }
