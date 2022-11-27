﻿namespace Perla

open Perla.Types
open Perla.Plugins

module Plugins =
    val PluginList: unit -> seq<RunnablePlugin>
    val LoadPlugins: config: EsbuildConfig -> unit
    val HasPluginsForExtension: string -> bool
    val ApplyPlugins: content: string * extension: string -> Async<FileTransform>