namespace Elmish.React

[<AutoOpen>]
module Helpers =
    open Fable.Helpers.React.Props
    open Fable.Core.JsInterop

    /// `Ref` callback that sets the value of an input textbox after DOM element is created.
    /// Can be used override input box value.
    let inline valueOrDefault value =
        Ref <| (fun e -> if e |> isNull |> not && !!e?value <> !!value then e?value <- !!value)

[<RequireQualifiedAccess>]
module Program =
    open Fable.Import.Browser

    /// Setup rendering of root React component inside html element identified by placeholderId
    ///
    /// This version uses `requestAnimationFrame` to optimize rendering in scenarios with updates
    /// at a higher rate than 60FPS. While it can be faster it also break few React idoms like
    /// [Controlled Components](https://reactjs.org/docs/forms.html#controlled-components) and
    /// should be used with caution.
    ///
    /// For more information see [Issue #12](https://github.com/fable-elmish/react/issues/12).
    let withReactAnimationFrameOptimized placeholderId (program:Elmish.Program<_,_,_,_>) =
        let mutable lastRequest = None
        let setState dispatch =
            let viewWithDispatch = program.view dispatch
            fun model ->
                match lastRequest with
                | Some r -> window.cancelAnimationFrame r
                | _ -> ()

                lastRequest <- Some (window.requestAnimationFrame (fun _ ->
                    Fable.Import.ReactDom.render(
                        lazyViewWith (fun x y -> obj.ReferenceEquals(x,y)) viewWithDispatch model,
                        document.getElementById(placeholderId)
                    )))

        { program with setState = setState }

    /// `withReact` uses `requestAnimationFrame` to optimize rendering in scenarios with updates at a higher rate than 60FPS, but this makes the cursor jump to the end in `input` elements.
    /// This function works around the glitch if you don't need the optimization (see https://github.com/fable-elmish/react/issues/12).
    let withReactUnoptimized placeholderId (program:Elmish.Program<_,_,_,_>) =
        let setState dispatch =
            let viewWithDispatch = program.view dispatch
            fun model ->
                Fable.Import.ReactDom.render(
                    lazyViewWith (fun x y -> obj.ReferenceEquals(x,y)) viewWithDispatch model,
                    document.getElementById(placeholderId)
                )

        { program with setState = setState }
