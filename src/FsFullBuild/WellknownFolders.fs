﻿module WellknownFolders

open System
open System.IO
open FileExtensions



let WORKSPACE_CONFIG_FOLDER = ".full-build"

let WORKSPACE_CONFIG_FILE = ".full-build"


let rec WorkspaceFolderSearch (dir : DirectoryInfo) =
    if dir = null || not dir.Exists then failwith "Can't find workspace root directory. Check you are in a workspace."
    let fbdir = dir |> GetSubDirectory WORKSPACE_CONFIG_FOLDER
    if fbdir.Exists then dir
    else WorkspaceFolderSearch dir.Parent

let WorkspaceFolder () : DirectoryInfo =
    let currDir = new DirectoryInfo(Environment.CurrentDirectory)
    WorkspaceFolderSearch currDir
