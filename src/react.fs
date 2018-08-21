namespace Elmish.React

open System
open Fable.Import.React
open Fable.Core
open Fable.Helpers.React

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

    // TODO: Provide an alternative for browsers that don't support it
    let private now() = performance.now()

    type private Dispatcher<'Value>(f: 'Value -> unit) =
        let mutable onHold: 'Value option = None
        let mutable frameLapse = 0.
        let mutable lastUpdate = now()
        let rec loop t1 t2: unit =
            if t1 > 0. then
                frameLapse <- t2 - t1
            match onHold with
            | None -> ()
            | Some v -> onHold <- None; f v
            window.requestAnimationFrame(FrameRequestCallback(loop t2)) |> ignore
        do loop 0. frameLapse

        member __.Dispatch(v) =
            let currentUpdate = now()
            match onHold with
            | Some _ -> onHold <- Some v
            | None ->
                if frameLapse > 0. && frameLapse > currentUpdate - lastUpdate
                then onHold <- Some v
                else f v
            lastUpdate <- currentUpdate

    /// Setup rendering of root React component inside html element identified by placeholderId
    let withReact placeholderId (program:Elmish.Program<_,_,_,_>) =
        let el = document.getElementById(placeholderId)

        let dispatcher = Dispatcher(fun (model, dispatch) ->
            Fable.Import.ReactDom.render(
                lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) program.view model dispatch,
                el
            ))

        { program with setState = fun m d -> dispatcher.Dispatch(m, d) }

    [<Obsolete("Please use `withReact`")>]
    let withReactUnoptimized placeholderId (program:Elmish.Program<_,_,_,_>) =
        let setState model dispatch =
            Fable.Import.ReactDom.render(
                lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) program.view model dispatch,
                document.getElementById(placeholderId)
            )

        { program with setState = setState }

    /// Setup rendering of root React component inside html element identified by placeholderId using React.hydrate
    let withReactHydrate placeholderId (program:Elmish.Program<_,_,_,_>) =
        let setState model dispatch =
            Fable.Import.ReactDom.hydrate(
                lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) program.view model dispatch,
                document.getElementById(placeholderId)
            )

        { program with setState = setState }
