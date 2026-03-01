module Tests.Helpers

open Fable.Core
open Fable.Core.JsInterop
open Fable.React

/// Synchronously flush all pending React renders.
/// Wraps react-dom's flushSync to force immediate rendering in tests.
[<Import("flushSync", "react-dom")>]
let flushSync (fn: unit -> unit) : unit = jsNative

/// Extract the JS `type` property from a ReactElement.
/// For memo components this is the memo wrapper object.
let getElementType (el: ReactElement) : obj =
    emitJsExpr el "$0.type"

/// Extract the displayName from a ReactElement's type (memo wrapper).
let getDisplayName (el: ReactElement) : string option =
    let v: string = emitJsExpr el "$0.type && $0.type.displayName"
    if isNull v then None else Some v

/// Extract the React key from a ReactElement.
let getElementKey (el: ReactElement) : string option =
    let v: string = emitJsExpr el "$0.key"
    if isNull v then None else Some v
