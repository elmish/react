namespace Elmish.React

[<AutoOpen>]
module Helpers =
    open Fable.React
    open Fable.Core.JsInterop

    /// `Ref` callback that sets the value of an input textbox after DOM element is created.
    /// Can be used instead of `DefaultValue` and `Value` props to override input box value.
    let inline valueOrDefault (value: string): IHTMLProp =
        !!("ref", Internal.updateInputValue value)

[<RequireQualifiedAccess>]
module Program =

    module Internal =

        open Fable.React
        open Browser
        open Elmish

        // Use the new rendering API in React 18+
        let useRootApi = try int ReactBindings.React.version.[ .. 1 ] >= 18 with _ -> false

        let withReactBatchedUsing lazyView2With placeholderId (program:Program<_,_,_,_>) =
            let setState =
                let mutable lastRequest = None

                if useRootApi then
                    let root = ReactDomClient.createRoot (document.getElementById placeholderId)

                    fun model dispatch ->
                        match lastRequest with
                        | Some r -> window.cancelAnimationFrame r
                        | _ -> ()

                        lastRequest <- Some (window.requestAnimationFrame (fun _ ->
                            root.render (lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) (Program.view program) model dispatch)))
                else
                    fun model dispatch ->
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
            let setState =
                if useRootApi then
                    let root = ReactDomClient.createRoot (document.getElementById placeholderId)

                    fun model dispatch ->
                        root.render (lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) (Program.view program) model dispatch)
                else
                    fun model dispatch ->
                        ReactDom.render(
                            lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) (Program.view program) model dispatch,
                            document.getElementById placeholderId
                        )

            program
            |> Program.withSetState setState

        let withReactHydrateUsing lazyView2With placeholderId (program:Elmish.Program<_,_,_,_>) =
            let setState =
                if useRootApi then
                    let mutable root = None

                    fun model dispatch ->
                        match root with
                        | None ->
                            root <-
                                ReactDomClient.hydrateRoot (
                                    document.getElementById placeholderId,
                                    lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) (Program.view program) model dispatch
                                ) |> Some
                        | Some root ->
                            root.render (lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) (Program.view program) model dispatch)
                else
                    fun model dispatch ->
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

    /// Renders React root component inside html element identified by placeholderId using `React.hydrate`.
    let withReactHydrate placeholderId (program:Elmish.Program<_,_,_,_>) =
        Internal.withReactHydrateUsing lazyView2With placeholderId program
