namespace Songhay.Player.YouTube

open System.Text.Json

open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

open Songhay.Player.YouTube
open Songhay.Player.YouTube.DisplayItemModelUtility

module ServiceHandlerUtility =

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
