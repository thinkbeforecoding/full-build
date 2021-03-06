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

module PatternMatching
open Collections

let private (|MatchZeroOrMore|_|) c =
    match c with
    | '*' -> Some c
    | _ -> None

let rec private matchRec (content : char list) (pattern : char list) =
    let matchZeroOrMore remainingPattern =
        match content with
        | [] -> matchRec content remainingPattern
        | _ :: t1 -> if matchRec content remainingPattern then true // match 0 time
                     else matchRec t1 pattern // try match one more time

    let matchChar firstPatternChar remainingPattern =
        match content with
        | firstContentChar :: remainingContent when firstContentChar = firstPatternChar -> matchRec remainingContent remainingPattern
        | _ -> false

    match pattern with
    | [] -> content = []
    | MatchZeroOrMore _ :: tail -> matchZeroOrMore tail
    | head :: tail -> matchChar head tail

let Match (content : string) (pattern : string) =
    matchRec (content.ToLowerInvariant() |> Seq.toList) (pattern.ToLowerInvariant() |> Seq.toList)


let FilterMatch<'T when 'T : comparison> (items : 'T set) (strOf : 'T -> string) (filters : string set) : 'T set =
    let mapItems = items |> Set.map (fun x -> (x, (strOf x).ToLowerInvariant()))
    let matchItems filter = mapItems |> Set.filter (fun x -> Match (snd x) filter)
                                     |> Set.map fst
    let matches = filters |> Set.map (fun x -> x.ToLowerInvariant()) 
                          |> Set.map matchItems
                          |> Set.unionMany
    matches
