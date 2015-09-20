﻿module RepoTests

open NUnit.Framework
open FsUnit
open Anthology
open Repo

[<Test>]
let CheckFilter () =
    let repos = Set [ { Vcs = VcsType.Git; Name = RepositoryId.from "cassandra-sharp"; Url = RepositoryUrl.from "https://github.com/pchalamet/cassandra-sharp" }
                      { Vcs = VcsType.Git; Name = RepositoryId.from "cassandra-sharp-contrib"; Url = RepositoryUrl.from "https://github.com/pchalamet/cassandra-sharp-contrib" } ]
  
    MatchRepo repos (RepositoryId.from "cassandra*") |> should equal repos
