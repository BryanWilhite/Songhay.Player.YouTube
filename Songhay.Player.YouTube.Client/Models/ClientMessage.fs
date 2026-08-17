namespace Songhay.Player.YouTube.Client.Models

open Songhay.Player.YouTube.Models
open Songhay.Player.YouTube.Client.Models

type ClientMessage =
    | Error of exn
    | YTMessage of YouTubeMessage
    | SetPage of ClientPage
