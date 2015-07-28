﻿// Copyright (c) 2014-2015, Pierre Chalamet
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of Pierre Chalamet nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL PIERRE CHALAMET BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
module Workspace

open System
open System.IO
open FileExtensions
open WellknownFolders
open Configuration
open Vcs
open Anthology


type BinaryRef = 
    { Target : string }
    static member From(assName : string) = { Target = assName.ToUpperInvariant() }
    static member From(ass : Assembly) = let name = match ass with
                                                    | GacAssembly { AssemblyName=assName }  -> assName
                                                    | LocalAssembly { AssemblyName=assName } -> assName
                                         BinaryRef.From name

type PackageRef = 
    { Target : string }
    static member From(id : string) : PackageRef = { Target = id.ToUpperInvariant() }
    static member From(pkg : Package) : PackageRef = PackageRef.From pkg.Id

type ProjectRef = 
    { Target : Guid }
    static member From(prj : Project) : ProjectRef = { Target = prj.ProjectGuid }

let private FindKnownProjects (repoDir : DirectoryInfo) =
    ["*.csproj"; "*.vbproj"; "*.fsproj"] |> Seq.map (fun x -> repoDir.EnumerateFiles (x, SearchOption.AllDirectories)) 
                                         |> Seq.concat

let private ParseRepositoryProjects (parser) (repoDir : DirectoryInfo) =
    repoDir |> FindKnownProjects 
            |> Seq.map (parser repoDir)

let private ParseWorkspaceProjects (parser) (wsDir : DirectoryInfo) (repos : string seq) : ProjectParser.ProjectDescriptor seq = 
    repos |> Seq.map (GetSubDirectory wsDir) 
          |> Seq.filter (fun x -> x.Exists) 
          |> Seq.map (ParseRepositoryProjects parser) 
          |> Seq.concat




let Create(path : string) = 
    let wsDir = new DirectoryInfo(path)
    wsDir.Create()
    if IsWorkspaceFolder wsDir then failwith "Workspace already exists"
    VcsCloneRepo wsDir GlobalConfig.Repository

    let vwDir = WorkspaceViewFolder ()
    vwDir.Create ()


let Index () = 
    let wsDir = WorkspaceFolder()
    let antho = LoadAnthology()
    let repos = antho.Repositories |> Seq.map (fun x -> x.Name)
    let projects = ParseWorkspaceProjects ProjectParser.ParseProject wsDir repos

    // FIXME: before merging, it would be better to tell about conflicts

    // merge binaries
    let foundBinaries = projects |> Seq.map (fun x -> x.Binaries) |> Seq.concat
    let newBinaries = antho.Binaries |> Seq.append foundBinaries |> Seq.distinctBy BinaryRef.From |> Seq.toList

    // merge packages
    let foundPackages = projects |> Seq.map (fun x -> x.Packages) |> Seq.concat
    let newPackages = antho.Packages |> Seq.append foundPackages |> Seq.distinctBy PackageRef.From |> Seq.toList

    // merge projects
    let foundProjects = projects |> Seq.map (fun x -> x.Project)
    let newProjects = antho.Projects |> Seq.append foundProjects |> Seq.distinctBy ProjectRef.From |> Seq.toList

    let newAntho = { antho 
                     with Binaries = newBinaries
                          Packages = newPackages 
                          Projects = newProjects }

    SaveAnthology newAntho


let Convert () = 
    ()