namespace Songhay.Player.YouTube

open FsToolkit.ErrorHandling

open Songhay.Modules.Models
open Songhay.Modules.Publications.Models

open Songhay.Player.YouTube.YouTubeScalars

module PresentationUtility =

    /// <summary>
    /// Gets the conventional custom CSS properties for a <c>800×600</c> <see cref="Presentation"/>.
    /// </summary>
    let getConventionalCssProperties() =
        let buttonImgUrl = $"url({rxAkyinkyinSvgDataUrl})"
        [
            CssCustomPropertyAndValue
                (CssCustomProperty.fromInput "rx-player-credits-button-background-image", CssValue buttonImgUrl)
        ]

    let toPresentationOption (data: Identifier * Presentation option) =
        option {
            let! presentation = data |> snd

            return { presentation with parts = presentation.parts }
        }
