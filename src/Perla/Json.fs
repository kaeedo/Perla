module Perla.Json

open System
open System.Text.Json
open System.Text.Json.Serialization
open System.Text.Json.Nodes

open Perla.PackageManager.Types
open Perla.Types

open Perla.Units
open Thoth.Json.Net
open FsToolkit.ErrorHandling
open FSharp.UMX

[<RequireQualifiedAccess; Struct>]
type PerlaConfigSection =
  | Index of index: string option
  | Fable of fable: FableConfig option
  | DevServer of devServer: DevServerConfig option
  | Build of build: BuildConfig option
  | Dependencies of dependencies: Dependency seq option
  | DevDependencies of devDependencies: Dependency seq option

let DefaultJsonOptions () =
  JsonSerializerOptions(
    WriteIndented = true,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
  )

let DefaultJsonNodeOptions () =
  JsonNodeOptions(PropertyNameCaseInsensitive = true)

let DefaultJsonDocumentOptions () =
  JsonDocumentOptions(
    AllowTrailingCommas = true,
    CommentHandling = JsonCommentHandling.Skip
  )

module ConfigDecoders =

  type DecodedFableConfig =
    { project: string<SystemPath> option
      extension: string<FileExtension> option
      sourceMaps: bool option
      outDir: string<SystemPath> option }

  type DecodedDevServer =
    { port: int option
      host: string option
      liveReload: bool option
      useSSL: bool option
      proxy: Map<string, string> option }

  type DecodedEsbuild =
    { esBuildPath: string<SystemPath> option
      version: string<Semver> option
      ecmaVersion: string option
      minify: bool option
      injects: string seq option
      externals: string seq option
      fileLoaders: Map<string, string> option
      jsxFactory: string option
      jsxFragment: string option }

  type DecodedBuild =
    { includes: string seq option
      excludes: string seq option
      outDir: string<SystemPath> option
      emitEnvFile: bool option }

  type DecodedPerlaConfig =
    { index: string<SystemPath> option
      runConfiguration: RunConfiguration option
      provider: Provider option
      build: DecodedBuild option
      devServer: DecodedDevServer option
      fable: DecodedFableConfig option
      esbuild: DecodedEsbuild option
      mountDirectories: Map<string<ServerUrl>, string<UserPath>> option
      enableEnv: bool option
      envPath: string<ServerUrl> option
      dependencies: Dependency seq option
      devDependencies: Dependency seq option }

  let FableFileDecoder: Decoder<DecodedFableConfig> =
    Decode.object (fun get ->
      { project =
          get.Optional.Field "project" Decode.string
          |> Option.map UMX.tag<SystemPath>
        extension =
          get.Optional.Field "extension" Decode.string
          |> Option.map UMX.tag<FileExtension>
        sourceMaps = get.Optional.Field "sourceMaps" Decode.bool
        outDir =
          get.Optional.Field "outDir" Decode.string
          |> Option.map UMX.tag<SystemPath> })

  let DevServerDecoder: Decoder<DecodedDevServer> =
    Decode.object (fun get ->
      { port = get.Optional.Field "port" Decode.int
        host = get.Optional.Field "host" Decode.string
        liveReload = get.Optional.Field "liveReload" Decode.bool
        useSSL = get.Optional.Field "useSSL" Decode.bool
        proxy = get.Optional.Field "proxy" (Decode.dict Decode.string) })

  let EsbuildDecoder: Decoder<DecodedEsbuild> =
    Decode.object (fun get ->
      { fileLoaders =
          get.Optional.Field "fileLoaders" (Decode.dict Decode.string)
        esBuildPath =
          get.Optional.Field "esBuildPath" Decode.string
          |> Option.map UMX.tag<SystemPath>
        version =
          get.Optional.Field "version" Decode.string
          |> Option.map UMX.tag<Semver>
        ecmaVersion = get.Optional.Field "ecmaVersion" Decode.string
        minify = get.Optional.Field "minify" Decode.bool
        injects =
          get.Optional.Field "injects" (Decode.list Decode.string)
          |> Option.map List.toSeq
        externals =
          get.Optional.Field "externals" (Decode.list Decode.string)
          |> Option.map List.toSeq
        jsxFactory = get.Optional.Field "jsxFactory" Decode.string
        jsxFragment = get.Optional.Field "jsxFragment" Decode.string })

  let BuildDecoder: Decoder<DecodedBuild> =
    Decode.object (fun get ->
      { includes =
          get.Optional.Field "includes" (Decode.list Decode.string)
          |> Option.map List.toSeq
        excludes =
          get.Optional.Field "excludes" (Decode.list Decode.string)
          |> Option.map List.toSeq
        outDir =
          get.Optional.Field "outDir" Decode.string
          |> Option.map UMX.tag<SystemPath>
        emitEnvFile = get.Optional.Field "emitEnvFile" Decode.bool })

  let DependencyDecoder: Decoder<Dependency> =
    Decode.object (fun get ->
      { name = get.Required.Field "name" Decode.string
        version = get.Optional.Field "version" Decode.string
        alias = get.Optional.Field "alias" Decode.string })

  let PerlaDecoder: Decoder<DecodedPerlaConfig> =
    Decode.object (fun get ->
      let runConfigDecoder =
        Decode.string
        |> Decode.andThen (function
          | "dev"
          | "development" -> Decode.succeed RunConfiguration.Development
          | "prod"
          | "production" -> Decode.succeed RunConfiguration.Production
          | value -> Decode.fail $"{value} is not a valid run configuration")

      let providerDecoder =
        Decode.string
        |> Decode.andThen (function
          | "jspm" -> Decode.succeed Provider.Jspm
          | "skypack" -> Decode.succeed Provider.Skypack
          | "unpkg" -> Decode.succeed Provider.Unpkg
          | "jsdelivr" -> Decode.succeed Provider.Jsdelivr
          | "jspm.system" -> Decode.succeed Provider.JspmSystem
          | value -> Decode.fail $"{value} is not a valid run configuration")

      { index =
          get.Optional.Field "index" Decode.string
          |> Option.map UMX.tag<SystemPath>
        runConfiguration =
          get.Optional.Field "runConfiguration" runConfigDecoder
        provider = get.Optional.Field "provider" providerDecoder
        build = get.Optional.Field "build" BuildDecoder
        devServer = get.Optional.Field "devServer" DevServerDecoder
        fable = get.Optional.Field "fable" FableFileDecoder
        esbuild = get.Optional.Field "esbuild" EsbuildDecoder
        mountDirectories =
          get.Optional.Field "mountDirectories" (Decode.dict Decode.string)
          |> Option.map (fun m ->
            m
            |> Map.toSeq
            |> Seq.map (fun (k, v) -> UMX.tag<ServerUrl> k, UMX.tag<UserPath> v)
            |> Map.ofSeq)
        enableEnv = get.Optional.Field "enableEnv" Decode.bool
        envPath =
          get.Optional.Field "envPath" Decode.string
          |> Option.map UMX.tag<ServerUrl>
        dependencies =
          get.Optional.Field "dependencies" (Decode.list DependencyDecoder)
          |> Option.map Seq.ofList
        devDependencies =
          get.Optional.Field "devDependencies" (Decode.list DependencyDecoder)
          |> Option.map Seq.ofList })

[<RequireQualifiedAccess>]
module internal TestDecoders =

  let TestStats: Decoder<TestStats> =
    Decode.object (fun get ->
      { suites = get.Required.Field "suites" Decode.int
        tests = get.Required.Field "tests" Decode.int
        passes = get.Required.Field "passes" Decode.int
        pending = get.Required.Field "pending" Decode.int
        failures = get.Required.Field "failures" Decode.int
        start = get.Required.Field "start" Decode.datetime
        ``end`` = get.Optional.Field "end" Decode.datetime })

  let Test: Decoder<Test> =
    Decode.object (fun get ->
      { body = get.Required.Field "body" Decode.string
        duration = get.Optional.Field "duration" Decode.float
        fullTitle = get.Required.Field "fullTitle" Decode.string
        id = get.Required.Field "id" Decode.string
        pending = get.Required.Field "pending" Decode.bool
        speed = get.Optional.Field "speed" Decode.string
        state = get.Optional.Field "state" Decode.string
        title = get.Required.Field "title" Decode.string
        ``type`` = get.Required.Field "type" Decode.string })

  let Suite: Decoder<Suite> =
    Decode.object (fun get ->
      { id = get.Required.Field "id" Decode.string
        title = get.Required.Field "title" Decode.string
        fullTitle = get.Required.Field "fullTitle" Decode.string
        root = get.Required.Field "root" Decode.bool
        parent = get.Optional.Field "parent" Decode.string
        pending = get.Required.Field "pending" Decode.bool
        tests = get.Required.Field "tests" (Decode.list Test) })

[<RequireQualifiedAccess>]
module internal EventDecoders =

  let SessionStart: Decoder<TestStats * int> =
    Decode.object (fun get ->
      get.Required.Field "stats" TestDecoders.TestStats,
      get.Required.Field "totalTests" Decode.int)

  let SessionEnd: Decoder<TestStats> =
    Decode.object (fun get -> get.Required.Field "stats" TestDecoders.TestStats)

  let SuiteEvent: Decoder<TestStats * Suite> =
    Decode.object (fun get ->
      get.Required.Field "stats" TestDecoders.TestStats,
      get.Required.Field "suite" TestDecoders.Suite)

  let TestPass: Decoder<TestStats * Test> =
    Decode.object (fun get ->
      get.Required.Field "stats" TestDecoders.TestStats,
      get.Required.Field "test" TestDecoders.Test)

  let TestFailed: Decoder<TestStats * Test * string * string> =
    Decode.object (fun get ->
      get.Required.Field "stats" TestDecoders.TestStats,
      get.Required.Field "test" TestDecoders.Test,
      get.Required.Field "message" Decode.string,
      get.Required.Field "stack" Decode.string)

  let ImportFailed: Decoder<string * string> =
    Decode.object (fun get ->
      get.Required.Field "message" Decode.string,
      get.Required.Field "stack" Decode.string)


type Json =
  static member ToBytes value =
    JsonSerializer.SerializeToUtf8Bytes(value, DefaultJsonOptions())

  static member FromBytes<'T>(value: byte array) =
    JsonSerializer.Deserialize<'T>(ReadOnlySpan value, DefaultJsonOptions())

  static member ToText(value, ?minify) =
    let opts = DefaultJsonOptions()
    let minify = defaultArg minify false
    opts.WriteIndented <- minify
    JsonSerializer.Serialize(value, opts)

  static member ToNode value =
    JsonSerializer.SerializeToNode(value, DefaultJsonOptions())

  static member FromConfigFile(content: string) =
    Decode.fromString ConfigDecoders.PerlaDecoder content

  static member TestEventFromJson(value: string) =
    // test events
    // { event: string
    //   stats: TestingStats
    //   suite?: Suite
    //   test?: Test
    //   // message and stack are the error
    //   message?: string
    //   stack?: string }
    result {
      match! Decode.fromString (Decode.field "event" Decode.string) value with
      | "session-start" ->
        return!
          Decode.fromString EventDecoders.SessionStart value
          |> Result.map SessionStart
      | "suite-start"
      | "suite-end" ->
        return!
          Decode.fromString EventDecoders.SuiteEvent value
          |> Result.map SuiteStart
      | "test-pass" ->
        return!
          Decode.fromString EventDecoders.TestPass value |> Result.map TestPass
      | "test-failed" ->
        return!
          Decode.fromString EventDecoders.TestFailed value
          |> Result.map TestFailed
      | "session-end" ->
        return!
          Decode.fromString EventDecoders.SessionEnd value
          |> Result.map SessionEnd
      | "test-import-failed" ->
        return!
          Decode.fromString EventDecoders.ImportFailed value
          |> Result.map TestImportFailed
      | "test-run-finished" -> return TestRunFinished
      | unknown -> return! Error($"'{unknown}' is not a known event")
    }