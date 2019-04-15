namespace Elmish.React

[<AutoOpen>]
module Helpers =
    open Fable.React.Props
    open Fable.Core.JsInterop

    /// `Ref` callback that sets the value of an input textbox after DOM element is created.
    /// Can be used instead of `DefaultValue` and `Value` props to override input box value.
    let inline valueOrDefault value =
        Ref <| (fun e -> if e |> isNull |> not && !!e?value <> !!value then e?value <- !!value)

[<RequireQualifiedAccess>]
module Program =

    module Internal =

        open Fable.React
        open Browser
        open Elmish

        let withReactBatchedUsing lazyView2With placeholderId (program:Program<_,_,_,_>) =
            let mutable lastRequest = None
            let setState model dispatch =
                match lastRequest with
                | Some r -> window.cancelAnimationFrame r
                | _ -> ()

                lastRequest <- Some (window.requestAnimationFrame (fun _ ->
                    ReactDom.render(
                        lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) (Program.view program) model dispatch,
                        document.getElementById placeholderId
                    )))

            program
            |> Program.withSetState setState

        let withReactSynchronousUsing lazyView2With placeholderId (program:Elmish.Program<_,_,_,_>) =
            let setState model dispatch =
                ReactDom.render(
                    lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) (Program.view program) model dispatch,
                    document.getElementById placeholderId
                )

            program
            |> Program.withSetState setState

        let withReactHydrateUsing lazyView2With placeholderId (program:Elmish.Program<_,_,_,_>) =
            let setState model dispatch =
                ReactDom.hydrate(
                    lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) (Program.view program) model dispatch,
                    document.getElementById placeholderId
                )

            program
            |> Program.withSetState setState


    /// Renders React root component inside html element identified by placeholderId.
    /// Uses `requestAnimationFrame` to batch updates to prevent drops in frame rate.
    /// NOTE: This may have unexpected effects in React controlled inputs, see https://github.com/elmish/react/issues/12
    let withReactBatched placeholderId (program:Elmish.Program<_,_,_,_>) =
        Internal.withReactBatchedUsing lazyView2With placeholderId program

    /// Renders React root component inside html element identified by placeholderId.
    /// New renders are triggered immediately after an update.
    let withReactSynchronous placeholderId (program:Elmish.Program<_,_,_,_>) =
        Internal.withReactSynchronousUsing lazyView2With placeholderId program

    [<System.Obsolete("Use withReactBatched")>]
    let withReact placeholderId (program:Elmish.Program<_,_,_,_>) =
        Internal.withReactBatchedUsing lazyView2With placeholderId program

    [<System.Obsolete("Use withReactSynchronous")>]
    let withReactUnoptimized placeholderId (program:Elmish.Program<_,_,_,_>) =
        Internal.withReactSynchronousUsing lazyView2With placeholderId program

    /// Renders React root component inside html element identified by placeholderId using `React.hydrate`.
    let withReactHydrate placeholderId (program:Elmish.Program<_,_,_,_>) =
        Internal.withReactHydrateUsing lazyView2With placeholderId program
