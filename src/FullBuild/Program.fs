﻿//   Copyright 2014-2017 Pierre Chalamet
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

module Main
open CLI.Commands

let processMain argv =
    Env.CheckLicense ()
    Commands.Workspace.CheckMinVersion ()

    let cmd = CLI.CommandLine.Parse (argv |> Seq.toList)

    match cmd with
    // workspace
    | Command.SetupWorkspace setupInfo -> Commands.Workspace.Create setupInfo
    | Command.InitWorkspace initInfo -> Commands.Workspace.Init initInfo
    | Command.ConvertRepositories convertInfo -> Commands.Workspace.Convert convertInfo
    | Command.Doctor -> Commands.Workspace.Doctor()
    | Command.CheckoutWorkspace version -> Commands.Workspace.Checkout version
    | Command.BranchWorkspace branch -> Commands.Workspace.Branch branch
    | Command.PullWorkspace pullInfo -> Commands.Workspace.Pull pullInfo
    | Command.Exec cmdInfo -> Commands.Workspace.Exec cmdInfo
    | Command.CleanWorkspace -> Commands.Workspace.Clean ()
    | Command.UpdateGuids updInfo -> Commands.Workspace.UpdateGuid updInfo
    | Command.History histInfo -> Commands.Workspace.History histInfo
    | Command.InstallPackages -> Commands.Workspace.Restore ()
    | Command.PushWorkspace pushInfo -> Commands.Workspace.Push pushInfo

    | Command.TestAssemblies testInfo -> Commands.Test.TestAssemblies testInfo.Filters testInfo.Excludes

    // repository
    | Command.AddRepository addInfo -> Commands.Repo.Add addInfo
    | Command.CloneRepositories cloneInfo -> Commands.Repo.Clone cloneInfo
    | Command.ListRepositories -> Commands.Repo.List ()
    | Command.DropRepository dropInfo -> Commands.Repo.Drop dropInfo

    // view
    | Command.AddView viewInfo -> Commands.View.Add viewInfo
    | Command.DropView viewInfo -> Commands.View.Drop viewInfo.Name
    | Command.ListViews -> Commands.View.List ()
    | Command.DescribeView viewInfo -> Commands.View.Describe viewInfo.Name
    | Command.Graph viewInfo -> Commands.View.Graph viewInfo
    | Command.BuildView viewInfo -> Commands.View.Build viewInfo
    | Command.AlterView viewInfo -> Commands.View.Alter viewInfo
    | Command.OpenView viewInfo -> Commands.View.Open viewInfo
    | Command.FullBuildView viewInfo -> Commands.View.OpenFullBuildView viewInfo

    // nuget
    | Command.AddNuGet url -> Commands.NuGet.Add url.toString
    | Command.ListNuGets -> Commands.NuGet.List ()

    // package
    | Command.UpdatePackages -> Commands.Package.Update ()
    | Command.OutdatedPackages -> Commands.Package.Outdated ()
    | Command.ListPackages -> Commands.Package.List ()

    // applications
    | Command.ListApplications appInfo -> Commands.Application.List appInfo
    | Command.AddApplication appInfo -> Commands.Application.Add appInfo
    | Command.DropApplication name -> Commands.Application.Drop name.toString
    | Command.PublishApplications pubInfo -> Commands.Application.Publish pubInfo
    | Command.BindProject bindInfo -> Commands.Application.BindProject bindInfo

    // misc
    | Command.Query queryInfo -> Commands.Query.Query queryInfo
    | Command.Version -> CLI.CommandLine.PrintVersion ()
    | Command.Usage mainCmd -> CLI.CommandLine.PrintUsage mainCmd
    | Command.Error errInfo -> CLI.CommandLine.PrintUsage errInfo

    let retCode = match cmd with
                  | Command.Error _ -> 5
                  | _ -> 0
    retCode


let tryMain verbose argv =
    try
        processMain argv
    with
        x -> let err = if verbose |> not then sprintf "Error:\n%s" x.Message
                       else let sep = "---------------------------------------------------"
                            sprintf "%s\n%A\n%s" sep x sep
               
             ConsoleHelpers.DisplayError err
             5

[<EntryPoint>]
let main argv =
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let verbose, args = CLI.CommandLine.IsVerbose (argv |> List.ofArray)
    let retCode = tryMain verbose args
    stopWatch.Stop()
    let elapsed = stopWatch.Elapsed
    if verbose then printfn "Completed in %d seconds." ((int)elapsed.TotalSeconds)
    retCode
