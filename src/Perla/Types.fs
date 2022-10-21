﻿namespace Perla

module Units =

  [<Measure>]
  type Semver

  [<Measure>]
  type SystemPath

  [<Measure>]
  type FileExtension

  [<Measure>]
  type ServerUrl

  [<Measure>]
  type UserPath


module Types =
  open FSharp.UMX
  open Units
  open Perla.PackageManager.Types

  [<Struct; RequireQualifiedAccess>]
  type RunConfiguration =
    | Production
    | Development

  type FableConfig =
    { project: string<SystemPath>
      extension: string<FileExtension>
      sourceMaps: bool
      outDir: string<SystemPath> option }

  type DevServerConfig =
    { port: int
      host: string
      liveReload: bool
      useSSL: bool
      proxy: Map<string, string> }

  type EsbuildConfig =
    { esBuildPath: string<SystemPath>
      version: string<Semver>
      ecmaVersion: string
      minify: bool
      injects: string seq
      externals: string seq
      fileLoaders: Map<string, string>
      jsxFactory: string option
      jsxFragment: string option }

  type BuildConfig =
    { includes: string seq
      excludes: string seq
      outDir: string<SystemPath>
      emitEnvFile: bool }

  type Dependency =
    { name: string
      version: string option
      alias: string option }

    member internal this.AsVersionedString =
      let version =
        match this.version with
        | Some version -> $"@{version}"
        | None -> ""

      $"{this.name}{version}"


  type PerlaConfig =
    { index: string<SystemPath>
      runConfiguration: RunConfiguration
      provider: Provider
      build: BuildConfig
      devServer: DevServerConfig
      fable: FableConfig option
      esbuild: EsbuildConfig
      mountDirectories: Map<string<ServerUrl>, string<UserPath>>
      enableEnv: bool
      envPath: string<ServerUrl>
      dependencies: Dependency seq
      devDependencies: Dependency seq }

  exception CommandNotParsedException of string
  exception HelpRequestedException
  exception MissingPackageNameException
  exception MissingImportMapPathException
  exception PackageNotFoundException
  exception HeaderNotFoundException of string
  exception FailedToParseNameException of string

  type RunConfiguration with

    member this.AsString =
      match this with
      | Production -> "production"
      | Development -> "development"

    static member FromString(value: string) =
      match value.ToLowerInvariant() with
      | "production"
      | "prod" -> Production
      | "development"
      | "dev"
      | _ -> Development
