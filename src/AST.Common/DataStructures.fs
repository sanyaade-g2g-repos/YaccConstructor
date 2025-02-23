﻿//   Copyright 2013, 2014 YaccConstructor Software Foundation
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

module Yard.Generators.Common.DataStructures

open System
        
[<Struct>]
type UsualOne<'T> =
    val mutable first : 'T
    val mutable other : 'T[]
    new (f,o) = {first = f; other = o}    

[<Struct>]
type ResizableUsualOne<'T> =
    val mutable first : 'T
    val other : ref<list<'T>>
    new (f,o) = {first = f; other = ref o}
    new (f) = {first = f; other = ref []}
    member this.Add x =        
        this.other := x :: !this.other
    member this.TryFind f =
        if f this.first
        then Some this.first
        else List.tryFind f !this.other

[<Struct>]
type ResizableUsualFive<'T when 'T:equality> =
    val mutable first  : 'T
    val mutable second : 'T
    val mutable third  : 'T
    val mutable fourth : 'T
    val mutable fifth  : 'T
    val mutable other          : list<'T>
    member this.Eq (o : ResizableUsualFive<_>) = 
        this.first = o.first && this.second = o.second && this.third = o.third && this.fourth = o.fourth && this.fifth = o.fifth
    member this.Add x =
        if this.second <> Unchecked.defaultof<_> 
        then 
            if this.third <> Unchecked.defaultof<_>
            then
                if this.fourth <> Unchecked.defaultof<_>
                then
                    if this.fifth <> Unchecked.defaultof<_>
                    then
                        this.other <- x :: this.other
                    else
                        this.fifth <- x
                else
                    this.fourth <- x
            else
                this.third <- x
        else
            this.second <- x
        
    member this.TryFind f =
        if f this.first
        then Some this.first
        else 
            if f this.second
            then Some this.second
            else
                if f this.third
                then Some this.third
                else
                    if f this.fourth
                    then Some this.fourth
                    else
                        if f this.fifth
                        then
                            Some this.fifth
                        else
                            List.tryFind f this.other
    new (f) = {first = f; second = Unchecked.defaultof<_>; third = Unchecked.defaultof<_>; fourth = Unchecked.defaultof<_>; fifth = Unchecked.defaultof<_>; other = []}
    


