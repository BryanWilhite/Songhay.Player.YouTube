namespace Songhay.Player.YouTube.Tests

open Xunit
open FsUnit.Xunit

open Songhay.Player.YouTube.PresentationUtility

type PresentationUtilityTests() =

    [<Fact>]
    member this.``getConventionalCssProperties test``() =
        let actual = getConventionalCssProperties()
        actual |> List.length |> should be (greaterThan 0)
