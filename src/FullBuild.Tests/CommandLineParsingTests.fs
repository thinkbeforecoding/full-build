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

module CommandLineParsingTests

open NUnit.Framework
open FsUnit
open CLI.Commands
open Anthology

[<Test>]
let CheckErrorInvoked () =
    let result = CLI.CommandLine.Parse [ "workspace"; "blah blah" ]
    let expected = Command.Error MainCommand.Usage
    result |> should equal expected

[<Test>]
let CheckUsageInvoked () =
    let result = CLI.CommandLine.Parse [ "help" ]
    let expected = Command.Usage MainCommand.Unknown
    result |> should equal expected


[<Test>]
let CheckRepositoriesConvert () =
    let result = CLI.CommandLine.Parse [ "convert"; "*" ]
    let expected = Command.ConvertRepositories { Filters = set ["*"]; Check = false; Reset = false }
    result |> should equal expected
