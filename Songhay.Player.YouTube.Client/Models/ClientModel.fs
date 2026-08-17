namespace Songhay.Player.YouTube.Client.Models

open System

open Songhay.Player.YouTube.Models

type ClientModel =
    {
        page: ClientPage
        ytModel: YouTubeModel
    }

    static member initialize (serviceProvider: IServiceProvider) =
        {
            page = NoContentPage
            ytModel = YouTubeModel.initialize serviceProvider
        }
