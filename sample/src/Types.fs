﻿module Types

open System
open Fable.Core

[<AttachMembers>]
type Language =
  | EnUs
  | DeDe
  | EsMx
  | Unknown of mappingName: string

  member this.AsString =
    match this with
    | EnUs -> "en-US"
    | DeDe -> "de-DE"
    | EsMx -> "es-MX"
    | Unknown mappingName -> mappingName

  static member FromString(value: string) =
    match value.ToLowerInvariant() with
    | "en-us" -> EnUs
    | "es-mx" -> EsMx
    | "de-de" -> DeDe
    | value ->
      JS.console.warn ($"'%s{value}' is not a known value")
      Unknown value


[<AbstractClass>]
type TranslationMap =

  [<EmitIndexer>]
  member _.Item
    with get (key: string): string option = jsNative

[<AbstractClass>]
type TranslationCollection =

  [<EmitIndexer>]
  member _.Item
    with get (key: string): TranslationMap option = jsNative


[<AttachMembers>]
type Notification =
  {
    id: Guid
    header: string
    kind: string
    message: string
  }

  static member Create(header, message, ?kind: string) =
    let kind = defaultArg kind "info"

    {
      id = Guid.NewGuid()
      header = header
      message = message
      kind = kind
    }
