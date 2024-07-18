namespace Songhay.StudioFloor.Client.Models

open System

open Songhay.Player.YouTube.Models
open Songhay.StudioFloor.Client.Models

type StudioFloorModel =
    {
        readMeData: string option
        page: StudioFloorPage
        ytModel: YouTubeModel
    }

    static member initialize (serviceProvider: IServiceProvider) =
        Songhay.Modules.Bolero.ServiceProviderUtility.setBlazorServiceProvider serviceProvider 
        {
            page = ReadMePage
            readMeData = None
            ytModel = YouTubeModel.initialize serviceProvider
        }
