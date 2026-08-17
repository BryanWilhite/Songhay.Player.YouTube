namespace Songhay.Player.YouTube.Client.Models

open Bolero

type ClientPage =
    | [<EndPoint "/">] NoContentPage
    | [<EndPoint "/{key}">] BRollYtPage of key: string
    | [<EndPoint "/set">] BRollYtSetPage
