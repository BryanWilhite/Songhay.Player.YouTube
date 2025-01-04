namespace Songhay.StudioFloor.Client.Models

open Bolero

open Songhay.Player.YouTube.Models

type StudioFloorPage =
    | [<EndPoint "/">] ReadMePage
    | [<EndPoint "/yt-presentation/{key}">] YtPresentationPage of key: string
    | [<EndPoint "/yt-thumbs/{key}">] YtThumbsPage of key: string
    | [<EndPoint "/yt-thumbs-set">] YtThumbsSetPage

type StudioFloorMessage =
    | Error of exn
    | GetReadMe | GotReadMe of string
    | NavigateTo of StudioFloorPage
    | YouTubeMessage of YouTubeMessage
