namespace Songhay.StudioFloor.Client.ElmishTypes

open Bolero
open Songhay.Player.YouTube

type Tab =
    | ReadMeTab
    | YtPresentationTab
    | YtThumbsTab

type Model = {
    readMeData: string option
    tab: Tab
    ytModel: YouTubeModel
}

type Message =
    | Error of exn
    | GetReadMe | GotReadMe of string
    | SetTab of Tab
    | YouTubeMessage of YouTubeMessage
