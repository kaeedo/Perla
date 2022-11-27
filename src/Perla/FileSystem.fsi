﻿namespace Perla.FileSystem

open System
open System.IO
open System.Runtime.InteropServices
open System.Text.Json.Nodes
open System.Threading
open System.Threading.Tasks
open FSharp.UMX
open Perla.Types
open Perla.Units
open Perla.PackageManager.Types


[<RequireQualifiedAccess>]
type PerlaFileChange =
    | Index
    | PerlaConfig
    | ImportMap


[<RequireQualifiedAccess>]
module FileSystem =
    module Operators =
        val inline (/): a: string -> b: string -> string

    val AssemblyRoot: string<SystemPath>
    val PerlaArtifactsRoot: string<SystemPath>
    val Database: string<SystemPath>
    val Templates: string<SystemPath>
    val PerlaConfigPath: string<SystemPath>
    val LiveReloadScript: Lazy<string>
    val WorkerScript: Lazy<string>
    val TestingHelpersScript: Lazy<string>
    val MochaRunnerScript: Lazy<string>
    val CurrentWorkingDirectory: unit -> string<SystemPath>
    val GetConfigPath: fileName: string -> fromDirectory: string<SystemPath> option -> string<SystemPath>

    val ExtractTemplateZip:
        username: string * repository: string * branch: string -> stream: Stream -> string<SystemPath>

    val RemoveTemplateDirectory: path: string<SystemPath> -> unit
    val EsbuildBinaryPath: unit -> string<SystemPath>
    val TryReadTsConfig: unit -> string option
    val GetTempDir: unit -> string
    val TplRepositoryChildTemplates: path: string<SystemPath> -> string<SystemPath> seq

[<Class>]
type FileSystem =
    static member PerlaConfigText: ?fromDirectory: string<SystemPath> -> string option
    static member SetCwdToPerlaRoot: ?fromPath: string<SystemPath> -> unit
    static member GetImportMap: ?fromDirectory: string<SystemPath> -> ImportMap
    static member SetupEsbuild:
        esbuildVersion: string<Semver> * [<Optional>] ?cancellationToken: CancellationToken -> Task<unit>
    static member WriteImportMap: map: ImportMap * ?fromDirectory: string<SystemPath> -> ImportMap
    static member WritePerlaConfig: ?config: JsonObject * ?fromDirectory: string<SystemPath> -> unit
    static member PathForTemplate: username: string * repository: string * branch: string * ?tplName: string -> string
    static member WriteTplRepositoryToDisk:
        origin: string<SystemPath> * target: string<UserPath> * ?payload: obj -> unit
    static member GetTemplateScriptContent:
        username: string * repository: string * branch: string * ?tplName: string -> string option
    static member IndexFile: fromConfig: string<SystemPath> -> string
    static member PluginFiles: unit -> (string * string) array
    static member ObservePerlaFiles:
        indexPath: string * [<Optional>] ?cancellationToken: CancellationToken -> IObservable<PerlaFileChange>