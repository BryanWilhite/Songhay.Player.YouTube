namespace Songhay.Player.YouTube

open System
open System.Text.Json
open Microsoft.JSInterop

open Bolero
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

open Songhay.Modules.Models
open Songhay.Modules.StringUtility
open Songhay.Modules.Bolero.JsRuntimeUtility
open Songhay.Modules.Publications.Models

open Songhay.Player.YouTube
open Songhay.Player.YouTube.DisplayItemModelUtility

module ServiceHandlerUtility =

    let getYtSetKey seed (uri: Uri) =
        let prefix = seed |> toKabobCase |> Option.defaultValue String.Empty
        $"{prefix}-{uri.Segments |> Array.last}"

    /// <summary>
    /// Maps the specified <see cref="Presentation"/> data
    /// for the current browser.
    /// </summary>
    /// <param name="jsRuntime">the <see cref="IJSRuntime"/></param>
    /// <param name="sectionElementRef">the <see cref="HtmlRef"/> targeted for <see cref="setComputedStylePropertyValueAsync"/></param>
    /// <param name="playListMapper">maps the relative playlist paths to absolute paths</param>
    /// <param name="data"><see cref="Presentation"/> data pair</param>
    let toPresentationOption
        (jsRuntime: IJSRuntime)
        (sectionElementRef: HtmlRef option)
        (playListMapper: DisplayText * Uri -> DisplayText * Uri)
        (data: Identifier * Presentation option) =

        option {
            let! presentation = data |> snd
            let! elementRef = sectionElementRef

            //TODO: getConventionalCssProperties(key.StringValue) @
            presentation.cssVariables
            |> List.iter
                    (
                        fun vv ->
                            let n, v = vv.Pair

                            jsRuntime
                                |> setComputedStylePropertyValueAsync elementRef n.Value v.Value
                                |> ignore
                    )

            let map list =
                list
                |> List.map playListMapper
                |> Playlist

            let parts =
                presentation.parts
                |> List.map(fun part -> match part with | Playlist list -> list |> map | _ -> part)

            return { presentation with parts = parts }
        }

    let toPublicationIndexData (jsonElementResult: Result<JsonElement, JsonException>) =
        jsonElementResult
        >>= fun el -> el |> Index.fromInput
        |> Option.ofResult

    let toYtItems (jsonElementResult: Result<JsonElement, JsonException>) =
        jsonElementResult
        >>= fun el -> el |> YtItemUtility.fromInput
        |> Result.map (fun input -> input |> List.toArray)
        |> Option.ofResult

    let toYtSet (jsonElementResult: Result<JsonElement, JsonException>) =
        jsonElementResult
        >>= fun el -> el |> ThumbsSet.fromInput
        |> Result.map (fun input -> input |> List.toArray)
        |> Option.ofResult
