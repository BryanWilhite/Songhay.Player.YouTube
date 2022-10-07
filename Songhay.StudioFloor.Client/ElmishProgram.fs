module Songhay.StudioFloor.Client.ElmishProgram

open Elmish
open Bolero
open Bolero.Html

type Model =
    {
        x: string
    }

let initModel =
    {
        x = ""
    }

type Message =
    | Ping

let update message model =
    match message with
    | Ping -> model

let view model dispatch =
    p { "Hello, world!" }

type StudioFloorProgramComponent() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        Program.mkSimple (fun _ -> initModel) update view
