namespace Elmish.React

[<AutoOpen>]
module Helpers =
    open Fable.Helpers.React.Props
    open Fable.Core.JsInterop

    /// `Ref` callback that sets the value of an input textbox after DOM element is created.
    /// Can be used to override input box value.
    let inline valueOrDefault value =
        Ref <| (fun e -> if e |> isNull |> not && !!e?value <> !!value then e?value <- !!value)

[<RequireQualifiedAccess>]
module Program =
    open Fable.Import.Browser

    /// Setup rendering of root React component inside html element identified by placeholderId
    ///
    /// This version uses `requestAnimationFrame` to optimize rendering in scenarios with updates
    /// at a higher rate than 60 per second. It also breaks a few React idioms like
    /// [Controlled Components](https://reactjs.org/docs/forms.html#controlled-components).
    ///
    /// See [Issue #12](https://github.com/fable-elmish/react/issues/12) for details.
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

    /// Setup rendering of root React component inside html element identified by placeholderId
    let withReact placeholderId (program:Elmish.Program<_,_,_,_>) =
        let setState dispatch =
            let viewWithDispatch = program.view dispatch
            fun model ->
                Fable.Import.ReactDom.render(
                    lazyViewWith (fun x y -> obj.ReferenceEquals(x,y)) viewWithDispatch model,
                    document.getElementById(placeholderId)
                )

        { program with setState = setState }

    /// Setup rendering of root React component inside html element identified by placeholderId using React.hydrate
    let withReactHydrate placeholderId (program:Elmish.Program<_,_,_,_>) =
        let setState dispatch =
            let viewWithDispatch = program.view dispatch
            fun model ->
                Fable.Import.ReactDom.hydrate(
                    lazyViewWith (fun x y -> obj.ReferenceEquals(x,y)) viewWithDispatch model,
                    document.getElementById(placeholderId)
                )

        { program with setState = setState }
