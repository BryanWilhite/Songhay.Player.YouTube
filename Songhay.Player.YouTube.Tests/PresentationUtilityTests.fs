namespace Songhay.Player.YouTube.Tests

open Xunit

open Songhay.Player.YouTube.PresentationUtility

type PresentationUtilityTests() =

    [<Fact>]
    member this.``getConventionalCssProperties test``() =
        let actual = getConventionalCssProperties()
        actual |> List.length |> fun length -> length > 0 |> Assert.True
