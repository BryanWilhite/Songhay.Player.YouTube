namespace Songhay.Player.YouTube.Components

open Bolero
open Bolero.Html

open Songhay.Modules.Models
open Songhay.Modules.StringUtility
open Songhay.Modules.Bolero.Models
open Songhay.Modules.Bolero.Visuals.Bulma.CssClass
open Songhay.Modules.Bolero.Visuals.Bulma.Component
open Songhay.Modules.Publications.Models
open Songhay.Player.YouTube.Models

/// <summary>
/// Defines the player Credits <see cref="ElmishComponent{TModel,TMessage}"/>.
/// </summary>
type PresentationCreditsElmishComponent() =
    inherit ElmishComponent<YouTubeModel, YouTubeMessage>()

    /// <summary>the child node customizing <see cref="bulmaModalContainer"/></summary>
    let modalNode (model: YouTubeModel) dispatch =
        let creditItemsNode =
            let hasCredits = model.presentation.IsSome && model.presentation.Value.credits.IsSome
            cond hasCredits <| function
            | true ->
                let roleCredits = model.presentation.Value.credits.Value 
                forEach roleCredits <| fun roleCredit ->
                    li {
                        [
                            (nameof RoleCredit |> toKabobCase).Value
                            elementIsFlex
                            elementFlexDirection Row
                            elementFlexItemsAlignment End
                            elementFlexJustifyContent SpaceBetween
                        ] |> CssClasses.toHtmlClassFromList
                        span {
                            [ nameof roleCredit.name; fontSize Size3 ] |> CssClasses.toHtmlClassFromList
                            text roleCredit.name
                        }
                        span {
                            [ nameof roleCredit.role ] |> CssClasses.toHtmlClassFromList
                            text roleCredit.role
                        }
                    }
            | false ->
                text "[missing!]"
        let modalNode =
            article {
                [ "message"; "is-success" ] |> CssClasses.toHtmlClassFromList

                div {
                    [ "message-header" ] |> CssClasses.toHtmlClassFromList
                    Html.p { text "Credits" }

                    button {
                        [ "delete" ] |> CssClasses.toHtmlClassFromList
                        attr.aria "label" "delete"

                        on.click <| fun _ -> YouTubeMessage.PresentationCreditsClick |> dispatch
                    }
                }
                div {
                    [ "message-body" ] |> CssClasses.toHtmlClassFromList
                    ul {
                        creditItemsNode
                    }
                }
            }

        bulmaModalContainer
            NoCssClasses
            (model.ytVisualStates.hasState PresentationCreditsModalVisible)
            (concat {
                bulmaModalBackground
                    (HasAttr (on.click <| fun _ -> YouTubeMessage.PresentationCreditsClick |> dispatch))
                bulmaModalContent NoCssClasses modalNode
            })

    /// <summary>the <c>button.credits</c> element and <see cref="modalNode"/></summary>
    let buttonNode model dispatch =
        concat {
            button {
                [ "credits" ] |> CssClasses.toHtmlClassFromList

                on.click <| fun _ -> YouTubeMessage.PresentationCreditsClick |> dispatch

                span {
                    empty()
                }
            }

            (model, dispatch) ||> modalNode
        }

    /// <summary>
    /// The conventional static member that returns
    /// this instance with <see cref="ecomp"/>
    /// </summary>
    /// <param name="model">the Elmish model of this domain</param>
    /// <param name="dispatch">the Elmish message dispatcher</param>
    static member EComp model dispatch =
        ecomp<PresentationCreditsElmishComponent, _, _> model dispatch { attr.empty() }

    /// <summary>
    /// Overrides <see cref="ElmishComponent.View"/>
    /// </summary>
    /// <param name="model">the Elmish model of this domain</param>
    /// <param name="dispatch">the Elmish message dispatcher</param>
    override this.View model dispatch =
        (model, dispatch) ||> buttonNode
