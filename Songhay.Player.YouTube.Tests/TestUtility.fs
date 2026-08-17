[<AutoOpen>]
module Songhay.Player.YouTube.Tests.TestUtility

open System
open System.IO
open System.Linq
open System.Net.Http
open System.Reflection
open System.Text.Json

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Abstractions

open FsToolkit.ErrorHandling

open Songhay.Modules
open Songhay.Modules.Models
open Songhay.Modules.ProgramFileUtility
open Songhay.Modules.Bolero.ServiceProviderUtility

open Songhay.Player.YouTube.Models

let nullLogger = NullLogger.Instance :> ILogger

let httpClient = new HttpClient()

let projectDirectoryInfo =
    Assembly.GetExecutingAssembly()
    |> ProgramAssemblyInfo.getPathFromAssembly "../../../"
    |> Result.valueOr raiseProgramFileError
    |> DirectoryInfo

let appSettingsPath = projectDirectoryInfo
                          .Parent.GetDirectories()
                          .First(_.Name.EndsWith(".Client"))
                          .GetDirectories()
                          .First(_.Name.Equals("wwwroot"))
                          .GetFiles()
                          .First(_.Name.Equals("appsettings.json"))
                          .FullName

let studioFloorConfiguration = ConfigurationBuilder().AddJsonFile(appSettingsPath).Build()
let studioFloorProvider = ServiceCollection().AddSingleton<IConfiguration>(studioFloorConfiguration).BuildServiceProvider()
let studioFloorModel = { YouTubeModel.initialize(studioFloorProvider) with
                                restApiMetadataOption =
                                    "PlayerApi"
                                    |> RestApiMetadata.fromConfiguration (getIConfiguration())
                                    |> RestApiMetadata.toRestApiMetadataOption (getILogger().LogException)
                            }

[<Literal>]
let studioSettingsPathMessage = "The expected Studio settings path is not here."

let studioSettingsPath = Environment.GetEnvironmentVariable("SONGHAY_APP_SETTINGS_PATH") |> Option.ofNull

let configuration = ConfigurationBuilder().AddJsonFile(studioSettingsPath.Value).Build()
let provider = ServiceCollection().AddSingleton<IConfiguration>(configuration).BuildServiceProvider()
let model = { YouTubeModel.initialize(provider) with
                                restApiMetadataOption =
                                    "PlayerYouTube"
                                    |> RestApiMetadata.fromConfiguration (getIConfiguration())
                                    |> RestApiMetadata.toRestApiMetadataOption (getILogger().LogException)
                            }

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
