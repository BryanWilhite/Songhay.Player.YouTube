namespace Songhay.StudioFloor.Client.Models

open Bolero

open Songhay.Player.YouTube.Models

type StudioFloorPage =
    | [<EndPoint "/">] ReadMePage
    | [<EndPoint "/yt-presentation/">] YtPresentationPage
    | [<EndPoint "/yt-thumbs/">] YtThumbsPage

type StudioFloorMessage =
    | Error of exn
    | GetReadMe | GotReadMe of string
    | NavigateTo of StudioFloorPage
    | YouTubeMessage of YouTubeMessage
