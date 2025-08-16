[<AutoOpen>]
module Songhay.Player.YouTube.Tests.TestUtility

open System
open System.IO
open System.Net.Http
open System.Reflection
open System.Text.Json
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

open FsToolkit.ErrorHandling

open Songhay.Modules.Bolero.Models
open Songhay.Modules.Models
open Songhay.Modules.ProgramFileUtility
open Songhay.Player.YouTube.Models

[<Literal>]
let studioSettingsPathMessage = "The expected Studio settings path is not here."

let studioSettingsPath = Environment.GetEnvironmentVariable("SONGHAY_APP_SETTINGS_PATH") |> Option.ofNull

let client = new HttpClient()
let configuration = ConfigurationBuilder().AddJsonFile(studioSettingsPath.Value).Build()
let provider = ServiceCollection().AddSingleton<IConfiguration>(configuration).BuildServiceProvider()
let model = { YouTubeModel.initialize(provider) with
                                restApiMetadata =
                                    "PlayerYouTube"
                                    |> RestApiMetadata.fromConfiguration (Songhay.Modules.Bolero.ServiceProviderUtility.getIConfiguration())
                            }

let projectDirectoryInfo =
    Assembly.GetExecutingAssembly()
    |> ProgramAssemblyInfo.getPathFromAssembly "../../../"
    |> Result.valueOr raiseProgramFileError
    |> DirectoryInfo

let getJson (fileName: string) =
    let path =
        $"./json/{fileName}"
        |> tryGetCombinedPath projectDirectoryInfo.FullName
        |> Result.valueOr raiseProgramFileError
    File.ReadAllText(path)

let getJsonDocument (fileName: string) =
    JsonDocument.Parse(getJson(fileName))

let writeJsonAsync (fileName: string) (json:string) =
    let path =
        $"./json/{fileName}"
        |> tryGetCombinedPath projectDirectoryInfo.FullName
        |> Result.valueOr raiseProgramFileError
    File.WriteAllTextAsync(path, json)
