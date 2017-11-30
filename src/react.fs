namespace Elmish.React

open System
open Fable.Import.React
open Fable.Core

[<RequireQualifiedAccess>]
module Program =
    open Fable.Import.Browser
    module R = Fable.Helpers.React

    /// Setup rendering of root React component inside html element identified by placeholderId
    let withReact placeholderId (program:Elmish.Program<_,_,_,_>) =
        let mutable lastRequest = None
        let setState model dispatch =
            match lastRequest with
            | Some r -> window.cancelAnimationFrame r
            | _ -> ()

            lastRequest <- Some (window.requestAnimationFrame (fun _ ->
                Fable.Import.ReactDom.render(
                    lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) program.view model dispatch,
                    document.getElementById(placeholderId)
                )))

        { program with setState = setState }

    /// `withReact` uses `requestAnimationFrame` to optimize rendering in scenarios with updates at a higher rate than 60FPS, but this makes the cursor jump to the end in `input` elements.
    /// Use this function to fix the glitch if you don't need the optimization (see https://github.com/fable-elmish/react/issues/12).
    let withReactNotOptimized placeholderId (program:Elmish.Program<_,_,_,_>) =
        let setState model dispatch =
            Fable.Import.ReactDom.render(
                lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) program.view model dispatch,
                document.getElementById(placeholderId)
            )

        { program with setState = setState }
