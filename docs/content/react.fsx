(*** hide ***)
#I ".paket/load/netstandard2.0"
#I "../../.paket/load/netstandard2.0"
#I "../../src/bin/Debug/netstandard2.0"
#load "Fable.Elmish.fsx"
#r "Fable.Elmish.React.dll"

(**
*)
namespace Elmish.React

open System
open Fable.React
open Fable.Core

[<AutoOpen>]
module Helpers =
    open Fable.React.Props
    open Fable.Core.JsInterop

    /// `Ref` callback that sets the value of an input textbox after DOM element is created.
    /// Can be used override input box value.
    let inline valueOrDefault value =
        Ref <| (fun e -> if e |> isNull |> not && !!e?value <> !!value then e?value <- !!value)

[<RequireQualifiedAccess>]
module Program =
    open Browser

    /// Setup rendering of root React component inside html element identified by placeholderId
    let withReact placeholderId (program:Elmish.Program<_,_,_,_>) =
        let mutable lastRequest = None
        let setState model dispatch =
            match lastRequest with
            | Some r -> window.cancelAnimationFrame r
            | _ -> ()

            lastRequest <- Some (window.requestAnimationFrame (fun _ ->
                Fable.ReactDom.render(
                    lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) program.view model dispatch,
                    document.getElementById(placeholderId)
                )))

        { program with setState = setState }

    /// `withReact` uses `requestAnimationFrame` to optimize rendering in scenarios with updates at a higher rate than 60FPS, but this makes the cursor jump to the end in `input` elements.
    /// This function works around the glitch if you don't need the optimization (see https://github.com/elmish/react/issues/12).
    let withReactUnoptimized placeholderId (program:Elmish.Program<_,_,_,_>) =
        let setState model dispatch =
            Fable.ReactDom.render(
                lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) program.view model dispatch,
                document.getElementById(placeholderId)
            )

        { program with setState = setState }

    /// Setup rendering of root React component inside html element identified by placeholderId using React.hydrate
    let withReactHydrate placeholderId (program:Elmish.Program<_,_,_,_>) =
        let setState model dispatch =
            Fable.ReactDom.hydrate(
                lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) program.view model dispatch,
                document.getElementById(placeholderId)
            )

        { program with setState = setState }
