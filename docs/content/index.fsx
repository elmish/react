(*** hide ***)
#I "../../src/bin/Debug/netstandard1.6"
#I "../../.paket/load/netstandard2.0"
#r "Fable.React.dll"
#r "Fable.Elmish.dll"
#r "Fable.Elmish.React.dll"

let view1 model : Fable.React.ReactElement = failwith "not implemented"
let view2 model (dispatch:Elmish.Dispatch<'msg>) : Fable.React.ReactElement = failwith "not implemented"
let view3 model1 model2 (dispatch:Elmish.Dispatch<'msg>) : Fable.React.ReactElement = failwith "not implemented"

let model = Some ""
let model1 = Some 1
let model2 = Some 2.

let dispatch : Elmish.Dispatch<unit> = failwith "not implemented"

(**
React extensions for Elmish apps
=======

Elmish-React implements boilerplate to wire up the rendering of React and React Native components and several rendering optimization functions.


## Installation

```shell
paket add nuget Fable.Elmish.React
```

You also need to install React:

```shell
yarn add react react-dom
```

## Program module extensions
Both React and React Native applications need a root component to be rendered at the specified placeholder, see
[browser](./browser.html) and [native](./native.html) tutorials for details.


## Lazy views
By default, every time the main update function is called (upon receiving and processing a message), the entire DOM is constructed anew and passed to React for [reconciliation](https://reactjs.org/docs/reconciliation.html).
If there are no changes in the model of some component, its view function will under normal circumstances not return a different result. React will then still perform reconciliation and realize that there is no need to update the component's UI.
Consequently, when the DOM is sufficiently large or its construction extremely time-consuming, this unnecessary work may have noticeable repercussions in terms of application performance.
Thanks to lazy views however, the update process can be optimized by avoiding DOM reconciliation and construction steps, but only if the model remains unchanged.

`lazyView` can be used with equatable models (most F# core types: records, tuples, etc).

`lazyViewWith` can be used with types that don't implement the `equality` constraint (such as types/instances coming from JS libraries) by passing the custom `equal` function that compares the previous and the new model.

These functions work for both React and React Native views. They are used in the following way.

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

(** Elmish-React will skip calling the `view3` for as long as both `model1` and `model2` remain unmodified.
*)
