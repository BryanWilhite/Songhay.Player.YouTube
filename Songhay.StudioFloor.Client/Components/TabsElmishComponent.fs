namespace Songhay.StudioFloor.Client.Components

open Bolero
open Bolero.Html

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.BodyElement
open Songhay.Modules.Bolero.Visuals.Bulma
open Songhay.Modules.Bolero.Visuals.Bulma.Component

open Songhay.Player.YouTube.Components
open Songhay.Player.YouTube.YouTubeScalars

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
                ( text "YouTube Thumbs", YtThumbsPage YtIndexSonghayTopTen )
                ( text "YouTube Presentation", YtPresentationPage "default" )
                ( text "YouTube Thumbs Set", YtThumbsSetPage)
            ]
            |> List.map (fun (n, page) -> anchorElement NoCssClasses (HasAttr <| ElmishRoutes.router.HRef page) n, page)

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
            | YtPresentationPage _ ->
                YtPresentationElmishComponent.EComp
                    false
                    model.ytModel
                    (StudioFloorMessage.YouTubeMessage >> dispatch)
            | YtThumbsPage key ->
                let title =
                    match key with
                    | YtIndexSonghayTopTen -> (Some "songhay tube")
                    | _ -> None
                concat {
                    YtThumbsContainerElmishComponent.EComp
                        title
                        model.ytModel
                        (StudioFloorMessage.YouTubeMessage >> dispatch)

                    YtThumbsSetElmishComponent.EComp true model.ytModel (StudioFloorMessage.YouTubeMessage >> dispatch)
                }
            | YtThumbsSetPage ->
                YtThumbsSetElmishComponent.EComp false model.ytModel (StudioFloorMessage.YouTubeMessage >> dispatch)
        }
