namespace Songhay.Player.YouTube.Client.Components

open System

open Elmish
open Bolero
open Bolero.Html

open Songhay.Modules.Models
open Songhay.Modules.Bolero.SvgUtility
open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.BodyElement
open Songhay.Modules.Bolero.Visuals.SvgElement
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Modules.Bolero.Visuals.Bulma.Element
open Songhay.Modules.Bolero.Visuals.Bulma.Layout

open Songhay.Player.YouTube.Components
open Songhay.Player.YouTube.Client
open Songhay.Player.YouTube.Client.Models

module pcu = ProgramComponentUtility

type ClientProgramComponent() =
    inherit ProgramComponent<ClientModel, ClientMessage>()

    let appVersions =
        let boleroVersion = $"{typeof<Node>.Assembly.GetName().Version}"
        let dotnetRuntimeVersion = $"{Environment.Version.Major:D}.{Environment.Version.Minor:D2}"

        [
            {|
                id = SonghaySvgKeys.MDI_BOLERO_DANCE_24PX.ToAlphanumeric
                title = DisplayText $"Bolero {boleroVersion}"
                version = boleroVersion
            |}
            {|
                id = SonghaySvgKeys.MDI_DOTNET_24PX.ToAlphanumeric
                title = DisplayText $".NET Runtime {dotnetRuntimeVersion}"
                version = dotnetRuntimeVersion
            |}
        ]

    let svgVersionNode (data: {| id: Identifier; title: DisplayText; version: string |}) =
        let classes = CssClasses [
            levelItem
            ShadeGreyLight.TextCssClass
            elementTextIsUnselectable
            elementTextAlign AlignCentered
        ]

        bulmaLevelItem
            (HasClasses classes)
            (HasAttr (attr.title data.title.Value))
            (concat {
                bulmaIcon (((svgViewBoxSquare 24), SonghaySvgData.Get(data.id)) ||> svgElement)
                span { fontSize Size7 |> CssClasses.toHtmlClass; text data.version }
            })

    let appAnchor (title: DisplayText, href: Uri, id: Identifier) =
            anchorElement
                (HasClasses <| CssClasses [ levelItem; elementTextAlign AlignCentered ])
                (HasAttr <| attrs{ attr.href href.OriginalString; TargetBlank.Value; attr.title title.Value })
                (bulmaIcon
                    (((svgViewBoxSquare 24), SonghaySvgData.Get(id)) ||> svgElement))

    let footerNode() =
        let cssClassesParentLevel = CssClasses [ levelContainer; isMobileModifier ]
        let cssClassesSvgVersionNodes = [ ShadeGreyLight.TextCssClass ] |> cssClassesParentLevel.AppendList
        let versionsNode =
            div {
                cssClassesSvgVersionNodes.ToHtmlClassAttribute
                forEach appVersions <| svgVersionNode
            }
        bulmaContainer
            ContainerWidthFluid
            (HasClasses <| CssClasses [ p (T, L4) ])
            (concat {
                bulmaColumnsContainer
                    NoCssClasses
                    (concat {
                        bulmaColumn
                            (HasClasses <| CssClasses [ HSize4.CssClass; elementTextAlign AlignCentered ])
                            (empty())
                        bulmaColumn
                            (HasClasses <| CssClasses [ HSize4.CssClass ])
                            versionsNode
                        bulmaColumn
                            (HasClasses <| CssClasses [ HSize4.CssClass; elementTextAlign AlignCentered ])
                            (empty())
                    })
                bulmaColumnsContainer
                    (HasClasses <| CssClasses [ AlignCentered.CssClass ])
                    (concat {
                        bulmaColumn
                            (HasClasses <| CssClasses [ HSize4.CssClass ])
                            (empty())
                    })
                })

    let update message (model: ClientModel) =
        match message with
        | Error _ -> model, Cmd.none
        | SetPage page ->
            let m = { model with page = page }
            let cmd = pcu.getCommandForSetPage page
            m, cmd
        | YTMessage ytMsg -> pcu.update ytMsg model

    let view model dispatch =
        match model.page with
        | BRollYtPage _ ->
            concat {
                bulmaSection
                    (HasClasses <| CssClasses [ p (All, L4) ])
                    NoAttr
                    (empty())
                YtPresentationElmishComponent.EComp
                    false
                    model.ytModel
                    (YTMessage >> dispatch)
                footerElement
                    (HasClasses <| CssClasses [ nameof footer ])
                    (footerNode())
            }
        | BRollYtSetPage ->
            concat {
                bulmaSection
                    (HasClasses <| CssClasses [ p (All, L4) ])
                    NoAttr
                    (empty())
                YtThumbsSetElmishComponent.EComp
                    false
                    model.ytModel
                    (YTMessage >> dispatch)
                footerElement
                    (HasClasses <| CssClasses [ nameof footer ])
                    (footerNode())
            }
        | NoContentPage ->
            concat {
                bulmaSection
                    (HasClasses <| CssClasses [ p (All, L4); m (All, L4); elementTextAlign AlignCentered ])
                    NoAttr
                    (h1 {
                        title DefaultBulmaFontSize |> CssClasses.toHtmlClassFromList
                        text "No Content"
                    })
                footerElement
                    (HasClasses <| CssClasses [ nameof footer ])
                    (footerNode())
            }

    override this.Program =
        let m = ClientModel.initialize this.Services

        Program.mkProgram (fun _ -> m, Cmd.none) update view
        |> Program.withRouter ElmishRoutes.router
