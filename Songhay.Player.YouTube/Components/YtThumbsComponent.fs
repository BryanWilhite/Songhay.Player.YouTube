namespace Songhay.Player.YouTube.Components

open System.Collections.Generic
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Microsoft.JSInterop

open FsToolkit.ErrorHandling
open Humanizer

open Bolero
open Bolero.Html
open Elmish

open Songhay.Modules.StringUtility
open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.Element
open Songhay.Modules.Bolero.Visuals.Bulma.Layout
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Modules.Bolero.Visuals.BodyElement

open Songhay.Player.YouTube
open Songhay.Player.YouTube.Models
open Songhay.Player.YouTube.YtItemUtility

type SlideDirection = | Left | Right

type YtThumbsComponent() =
    inherit ElmishComponent<YouTubeModel, YouTubeMessage>()

    [<Literal>] // see `$var-thumbs-container-wrapper-left` in `Songhay.Player.YouTube/src/scss/you-tube-css-variables.scss`
    static let CssVarThumbsContainerWrapperLeft = "--thumbs-container-wrapper-left"

    [<Literal>] // see `$thumbnail-margin-right` in `Songhay.Player.YouTube/src/scss/you-tube-thumbs.scss`
    static let ThumbnailMarginRight = 4

    static let click = DomElementEvent.Click

    static let getYtThumbsAnchor (item: YouTubeItem) =
        let limit = 60
        let caption =
            if item.snippet.title.Length > limit then
                $"{item.snippet.title.Substring(0, limit)}…"
            else
                item.snippet.title

        a {
            attr.href (item.tryGetUri |> Result.valueOr raise)
            attr.target "_blank"
            attr.title item.snippet.title

            text caption
        }

    static let getYtThumbsTitle (dispatch: Dispatch<YouTubeMessage>) (_: IJSRuntime)
        (itemsTitle: string option) (model: YouTubeModel) =

        let items = model.YtItems

        cond items.IsNone <| function
        | true -> rawHtml "&#160;"
        | _ ->
            cond itemsTitle.IsNone <| function
            | true ->
                let pair = items.Value |> Array.head |> getYtItemsPair
                a { attr.href (fst pair); attr.target "_blank"; text (snd pair) }
            | _ ->
                a {
                    attr.href "#" ; attr.title $"{itemsTitle.Value}: show curated YouTube™ channels"
                    click.PreventDefault
                    on.click (fun _ -> YouTubeMessage.OpenYtSetOverlay |> dispatch)
                    text itemsTitle.Value
                }

    ///<remarks>
    /// this member is needed to ‘jumpstart’ CSS animations
    /// without this member, the first interop with CSS will not function as expected
    ///</remarks>
    static let initAsync (initCache: Dictionary<DomElementEvent, bool>) (blockWrapperRef: HtmlRef) (jsRuntime: IJSRuntime) =
        task {
            if not initCache[Load] then
                let! wrapperLeftStr = jsRuntime |> getComputedStylePropertyValueAsync blockWrapperRef "left"

                jsRuntime
                |> setComputedStylePropertyValueAsync blockWrapperRef CssVarThumbsContainerWrapperLeft wrapperLeftStr
                |> ignore

            initCache[Load] <- true
        }

    static let ytThumbnailsNode (_: IJSRuntime) (blockWrapperRef: HtmlRef) (items: YouTubeItem[] option) =

        let toSpan (item: YouTubeItem) =
            let duration =
                match item.tryGetDuration with
                | Ok ts -> ts.ToString() |> text
                | _ -> text ":00"

            span {
                a
                    {
                        attr.href (item.tryGetUri |> Result.valueOr raise).OriginalString
                        attr.target "_blank"
                        attr.title item.snippet.title

                        img
                            {
                                attr.src item.snippet.thumbnails.medium.url
                                attr.width item.snippet.thumbnails.medium.width
                                attr.height item.snippet.thumbnails.medium.height
                            }
                    }
                span { [ "published-at"; fontSize Size6 ] |> CssClasses.toHtmlClassFromList; item.getPublishedAt.Humanize() |> text }
                span { [ "caption"; elementFontWeight Semibold; fontSize Size6 ] |> CssClasses.toHtmlClassFromList; item |> getYtThumbsAnchor }
                span { [ "duration"; fontSize Size6 ] |> CssClasses.toHtmlClassFromList ; span { duration } }
            }

        cond items.IsSome <| function
            | true -> div { attr.ref blockWrapperRef; forEach items.Value <| toSpan }
            | false ->
                bulmaContainer
                    ContainerWidthFluid
                    (HasClasses (CssClasses [m (All, L6); elementTextAlign AlignCentered]))
                        (bulmaLoader
                            (HasClasses (CssClasses (imageContainer (Square Square128) @ [p (All, L3)]))))

    static let ytThumbsNode (dispatch: Dispatch<YouTubeMessage>) (jsRuntime: IJSRuntime)
        (initCache: Dictionary<DomElementEvent, bool>) (thumbsContainerRef: HtmlRef) (blockWrapperRef: HtmlRef)
        (itemsTitle: string option) (model: YouTubeModel) =

        let items = model.YtItems
        let slideAsync (direction: SlideDirection) (_: MouseEventArgs) =
            async {
                if items.IsNone then ()
                else
                    jsRuntime |> initAsync initCache blockWrapperRef |> ignore
                    let! wrapperContainerWidthStr =
                        jsRuntime
                        |> getComputedStylePropertyValueAsync thumbsContainerRef "width"
                        |> Async.AwaitTask
                    let! wrapperLeftStr =
                        jsRuntime
                        |> getComputedStylePropertyValueAsync blockWrapperRef "left"
                        |> Async.AwaitTask

                    let wrapperContainerWidth = wrapperContainerWidthStr |> toNumericString |> Option.defaultValue "0" |> int
                    let wrapperLeft = wrapperLeftStr |> toNumericString |> Option.defaultValue "0" |> int

                    let cannotSlideLeft =
                        let itemsHead = items.Value |> Array.head
                        let fixedBlockWidth = itemsHead.snippet.thumbnails.medium.width + ThumbnailMarginRight
                        let totalWidth = fixedBlockWidth * (items.Value |> Array.length)
                        let slideLeftLength = abs(wrapperLeft) + wrapperContainerWidth

                        slideLeftLength >= totalWidth

                    let getSlideRightLength =
                        let l = abs wrapperLeft
                        if l > wrapperContainerWidth then wrapperContainerWidth
                        else l

                    let nextLeft =
                        match direction with
                        | Left ->
                            if cannotSlideLeft then wrapperLeft
                            else wrapperLeft - wrapperContainerWidth
                        | Right ->
                            if wrapperLeft >= 0 then wrapperLeft
                            else wrapperLeft + getSlideRightLength

                    jsRuntime
                    |> setComputedStylePropertyValueAsync blockWrapperRef CssVarThumbsContainerWrapperLeft $"{nextLeft}px"
                    |> ignore
            }

        div {
            [ "rx"; "b-roll" ] |> CssClasses.toHtmlClassFromList

            nav {
                [ levelContainer; "video"; "thumbs"; "header" ] |> CssClasses.toHtmlClassFromList
                div {
                    level AlignLeft |> CssClasses.toHtmlClass

                    span {
                        ([ levelItem ] @ imageContainer (Square Square48)) |> CssClasses.toHtmlClassFromList
                        svgElement (bulmaIconSvgViewBox Square24) (SonghaySvgData.Get(SonghaySvgKeys.MDI_YOUTUBE_24PX.ToAlphanumeric))
                    }
                    span {
                        [ levelItem; fontSize Size2 ] |> CssClasses.toHtmlClassFromList
                        (jsRuntime, itemsTitle, model) |||> getYtThumbsTitle dispatch
                    }
                }
            }
            div {
                [ "video"; "thumbs"; "thumbs-container" ] |> CssClasses.toHtmlClassFromList
                attr.ref thumbsContainerRef

                items |> ytThumbnailsNode jsRuntime blockWrapperRef

                a {
                    attr.href "#"; [ "command"; "left" ] @ imageContainer (Square Square48) |> CssClasses.toHtmlClassFromList
                    click.PreventDefault
                    on.async.click (slideAsync SlideDirection.Right)
                    svgElement (bulmaIconSvgViewBox Square24) (SonghaySvgData.Get(SonghaySvgKeys.MDI_ARROW_LEFT_DROP_CIRCLE_24PX.ToAlphanumeric))
                }
                a {
                    attr.href "#"; [ "command"; "right"; ] @ imageContainer (Square Square48) |> CssClasses.toHtmlClassFromList
                    click.PreventDefault
                    on.async.click (slideAsync SlideDirection.Left)
                    svgElement (bulmaIconSvgViewBox Square24) (SonghaySvgData.Get(SonghaySvgKeys.MDI_ARROW_RIGHT_DROP_CIRCLE_24PX.ToAlphanumeric))
                }
            }
        }

    let blockWrapperRef = HtmlRef()
    let initCache = Dictionary<DomElementEvent, bool>()
    let thumbsContainerRef = HtmlRef()

    static member EComp (title: string option) (model: YouTubeModel) dispatch =
        ecomp<YtThumbsComponent, _, _> model dispatch {
            if title.IsSome then
                "YtThumbsTitle" => title.Value
            else
                attr.empty()
        }

    [<Parameter>]
    member val YtThumbsTitle = Unchecked.defaultof<string> with get, set

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.ShouldRender(oldModel, newModel) = oldModel.YtItems <> newModel.YtItems

    override this.View model dispatch =
        if not(initCache.ContainsKey(Load)) then initCache.Add(Load, false)
        let title = (this.YtThumbsTitle |> Option.ofObj)
        (title, model)
        ||> ytThumbsNode dispatch this.JSRuntime initCache thumbsContainerRef blockWrapperRef
