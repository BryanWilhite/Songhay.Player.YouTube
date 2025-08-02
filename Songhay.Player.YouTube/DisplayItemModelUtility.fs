namespace Songhay.Player.YouTube

open System.Text.Json
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

open Songhay.Modules.Models
open Songhay.Modules.JsonDocumentUtility
open Songhay.Modules.Publications.Models
open Songhay.Modules.Publications.DisplayItemModelUtility

open Songhay.Player.YouTube
open Songhay.Player.YouTube.Models

module DisplayItemModelUtility =

    module Index =

        let tryGetDisplayTupleFromDocument (element: JsonElement) =
            result {
                let! id = ((nameof ClientId), element) ||> Identifier.fromInputElementName
                let! title = (false, element) ||> defaultDocumentDisplayTextGetter
                let! fragmentClientIds =
                    element
                    |> tryGetProperty $"{nameof Fragment}s"
                    >>= fun el ->
                        el.EnumerateArray()
                        |> List.ofSeq
                        |> List.map (fun i -> (false, i) ||> ClientId.fromInput)
                        |> List.sequenceResultM

                return
                    (
                        {
                            id = id
                            itemName = None
                            displayText = title
                            resourceIndicator = None
                        },
                        fragmentClientIds |> Array.ofList
                    )
            }

        let fromInput (element: JsonElement) =
            result {
                let! segmentClientId = (false, element) ||> ClientId.fromInput
                let! segmentName = element |> Name.fromInput PublicationItem.Segment false
                let! displayItemModels =
                    element |> tryGetProperty $"{nameof Document}s"
                    >>= fun el ->
                        el.EnumerateArray()
                        |> List.ofSeq
                        |> List.map (fun i -> i |> tryGetDisplayTupleFromDocument)
                        |> List.sequenceResultM

                return
                    (
                      segmentClientId,
                      segmentName,
                      displayItemModels |> Array.ofList
                    )
            }

    module ThumbsSet =
        let fromInput (element: JsonElement) =
            element |> tryGetProperty "set"
            >>= fun el ->
                el.EnumerateArray()
                |> List.ofSeq
                |> List.map ( fun el ->
                        el |> YtItemUtility.fromInput
                        |> Result.map (
                            fun l ->
                                DisplayText (l |> List.head).snippet.channelTitle, l |> Array.ofList
                            )
                    )
                |> List.sequenceResultM
