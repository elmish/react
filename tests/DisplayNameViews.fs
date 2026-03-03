module Tests.DisplayNameViews

open Fable.React
open Elmish

/// Minimal helper to create a react element without the Fable.React DSL package.
let private el tag (text: string) : ReactElement =
    ReactBindings.React.createElement(tag, null, [ unbox<ReactElement> text ])

let namedView1 (model: int) =
    el "div" (string model)

let namedView2 (model: int) (_dispatch: unit Dispatch) =
    el "div" (string model)

let namedView3 (s1: int) (s2: string) (_dispatch: unit Dispatch) =
    el "div" (sprintf "%d-%s" s1 s2)
