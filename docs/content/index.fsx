(*** hide ***)
#I "../../src/bin/Debug/netstandard1.6"
#I "../../packages/Fable.Elmish/lib/netstandard1.6"
#I "../../packages/Fable.React/lib/netstandard1.6"
#r "Fable.React.dll"
#r "Fable.Elmish.dll"
#r "Fable.Elmish.React.dll"

let view1 model : Fable.Import.React.ReactElement = failwith "not implemented"
let view2 model (dispatch:Elmish.Dispatch<'msg>) : Fable.Import.React.ReactElement = failwith "not implemented"
let view3 model1 model2 (dispatch:Elmish.Dispatch<'msg>) : Fable.Import.React.ReactElement = failwith "not implemented"

let model = Some ""
let model1 = Some 1
let model2 = Some 2.

let dispatch : Elmish.Dispatch<unit> = failwith "not implemented"

(**
Elmish-react: React extensions for Elmish apps
=======

Elmish-react implements boilerplate to wireup the rendering of React and ReactNative components and several rendering optimization functions.


## Installation

```shell
paket add nuget Fable.Elmish.React
```

## Program module extensions 
Both React and React Native applications needs a root component to be rendered at the specified placeholder, see
[browser](/browser.html) and [native](/native.html) tutorials for details.


## Lazy views
Rendering of any view can be optimizied by avoiding the DOM reconciliation and skipping the DOM construction entierly if there are no changes in the model.

`lazyView` can be used with equattable models (most F# core types: records, tuples,etc).

`lazyViewWith` can be used with types that don't implement `equality` constraint, such as types/instances coming from JS libraries, by passing the custom `equal` function that compares the previous and the new models.

These functions work for both React and React Native views, here are some examples.

Given a view function of one argument:
*)

open Elmish.React

// val view : 'a -> ReactElement
lazyView view1 model 

// or given a typical view function, defined like this:
// val view : 'a -> Dispatch<'msg> -> ReactElement
lazyView2 view2 model dispatch 

(** the rendered view will be cached for as long as `model` remains the same.

Given a view function of three arguments:
*)

// val view : 'a -> 'b -> Dispatch<'msg> -> ReactElement
lazyView3 view3 model1 model2 dispatch

(** Elmish.React will skip calling the `view3` for as long as both `model1` and `model2` remain unmodified.
*)
