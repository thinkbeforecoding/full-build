﻿module AnthologyGraph
open Anthology
open Collections

// find all referencing projects of a project
let private referencingProjects (projects : Project set) (current : ProjectId) =
    projects |> Set.filter (fun x -> x.ProjectReferences |> Set.contains current)

let rec private computePaths (findParents : ProjectId -> Project set) (goal : ProjectId set) (path : ProjectId set) (current : Project) =
    let currentId = current.ProjectId
    let parents = findParents currentId
    let newPath = Set.add currentId path
    let paths = parents |> Set.map (computePaths findParents goal (Set.add currentId path))
                        |> Set.unionMany
    if Set.contains currentId goal then Set.union newPath paths
    else paths

let ComputeProjectSelectionClosure (allProjects : Project set) (goal : ProjectId set) =
    let findParents = referencingProjects allProjects

    let seeds = allProjects |> Set.filter (fun x -> Set.contains (x.ProjectId) goal)
    let transitiveClosure = seeds |> Set.map (computePaths findParents goal Set.empty)
                                  |> Set.unionMany
    transitiveClosure


let rec public ComputeAllProjectsSelectionSourceOnly (allProjects : Project set) (selectionId : ProjectId set) =
    let selection = allProjects |> Set.filter (fun x -> selectionId |> Set.contains x.ProjectId)

    let dependenciesId = selection |> Seq.map (fun x -> x.ProjectReferences)
                                   |> Seq.collect id
                                   |> Set
    let newSelectionId = allProjects |> Set.filter (fun x -> dependenciesId |> Set.contains x.ProjectId)
                                   |> Set.map (fun x -> x.ProjectId)
                                   |> Set.union selectionId

    match newSelectionId <> selectionId with
    | false -> ComputeAllProjectsSelectionSourceOnly allProjects newSelectionId
    | _ -> newSelectionId




let ComputeRepositoriesDependencies (allProjects : Project set) (selectedRepos : RepositoryId set) =
    let selectedProjects = allProjects |> Set.filter (fun x -> selectedRepos |> Set.contains x.Repository)
                                       |> Set.map (fun x -> x.ProjectId)
    let transitiveProjects = ComputeAllProjectsSelectionSourceOnly allProjects selectedProjects
    let repositories = allProjects |> Set.filter (fun x -> transitiveProjects |> Set.contains x.ProjectId)
                                   |> Set.map (fun x -> x.Repository)
    repositories

