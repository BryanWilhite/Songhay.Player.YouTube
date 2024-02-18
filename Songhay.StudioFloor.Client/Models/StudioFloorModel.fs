namespace Songhay.StudioFloor.Client.Models

open Songhay.Player.YouTube.Models

type StudioFloorModel = {
    readMeData: string option
    page: StudioFloorPage
    tab: StudioFloorTab
    ytModel: YouTubeModel
}
