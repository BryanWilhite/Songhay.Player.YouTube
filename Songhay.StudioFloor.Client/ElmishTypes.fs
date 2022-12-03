namespace Songhay.StudioFloor.Client.ElmishTypes

open Bolero
open Songhay.Player.YouTube

type Page =
    | [<EndPoint "/">] ReadMePage
    | [<EndPoint "/thumbs">] YtThumbsPage
    | [<EndPoint "/presentation">] YtPresentationPage

type Model = {
    page: Page
    readMeData: string option
    ytModel: YouTubeModel
}

type Message =
    | SetPage of Page
    | YouTubeMessage of YouTubeMessage
