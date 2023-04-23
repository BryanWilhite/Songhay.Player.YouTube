namespace Songhay.StudioFloor.Client.Models

open Songhay.Player.YouTube.Models

type StudioFloorMessage =
    | Error of exn
    | GetReadMe | GotReadMe of string
    | SetTab of StudioFloorTab
    | YouTubeMessage of YouTubeMessage
