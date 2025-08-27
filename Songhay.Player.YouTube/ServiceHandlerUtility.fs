namespace Songhay.Player.YouTube

open System.Text.Json

open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

open Microsoft.Extensions.Logging

open Songhay.Modules.Bolero
open Songhay.Player.YouTube
open Songhay.Player.YouTube.DisplayItemModelUtility

module ServiceHandlerUtility =

    let logger = ServiceProviderUtility.getILogger()
    let log level message = if logger <> null then logger.Log(level, message) else ()

    let toPublicationIndexData (jsonElementResult: Result<JsonElement, JsonException>) =
        jsonElementResult
        >>= fun el -> el |> Index.fromInput
        |> Result.teeError logger.LogException
        |> Option.ofResult

    let toYtItems (jsonElementResult: Result<JsonElement, JsonException>) =
        jsonElementResult
        >>= fun el -> el |> YtItemUtility.fromInput
        |> Result.map (fun input -> input |> List.toArray)
        |> Result.teeError logger.LogException
        |> Option.ofResult

    let toYtSet (jsonElementResult: Result<JsonElement, JsonException>) =
        jsonElementResult
        >>= fun el -> el |> ThumbsSet.fromInput
        |> Result.map (fun input -> input |> List.toArray)
        |> Result.teeError logger.LogException
        |> Option.ofResult
