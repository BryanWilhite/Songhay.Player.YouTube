namespace Songhay.StudioFloor.Client.Components

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.BodyElement
open Songhay.Modules.Bolero.Visuals.Bulma
open Songhay.Modules.Bolero.Visuals.Bulma.Component
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

        let tabPairs =
            [
                ( text "README", ReadMePage )
                ( text "YouTube Thumbs", YtThumbsPage )
                ( text "YouTube Presentation", YtPresentationPage )
            ]
            |> List.map (fun (n, page) ->
                    anchorElement NoCssClasses (HasAttr <| ElmishRoutes.router.HRef page) n, page
            )

        concat {

            bulmaTabs
                (HasClasses <| CssClasses [
                    ColorEmpty.BackgroundCssClassLight
                    CssClass.tabsElementIsToggle
                    CssClass.elementIsFullWidth
                    SizeLarge.CssClass
                ])
                (fun page -> model.page = page)
                tabPairs

            cond model.page <| function
            | ReadMePage -> ReadMeElmishComponent.EComp model dispatch
            | YtPresentationPage ->
                text "presentation"
            | YtThumbsPage ->
                YtThumbsComponent.EComp (Some "songhay tube") model.ytModel (StudioFloorMessage.YouTubeMessage >> dispatch)

            YtThumbsSetComponent.EComp model.ytModel (StudioFloorMessage.YouTubeMessage >> dispatch)
        }
