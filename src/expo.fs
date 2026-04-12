namespace Elmish.Expo

open Fable.React
open Fable.Core
open Elmish

module internal Interop =
    [<Import("registerRootComponent","expo")>]
    let registerRootComponent (component': ReactElementType<'T>) : unit =
        failwith "JS only"

[<RequireQualifiedAccess>]
module Program =
    open Elmish.React
    open Elmish.ReactNative.Components

    /// Setup rendering of root Expo component using registerRootComponent from expo.
    /// This provides Expo dev tools, proper web platform rendering, and Expo.fx initialization.
    let withExpo (program:Program<_,_,_,_>) =
        Interop.registerRootComponent (unbox JsInterop.jsConstructor<App>)
        let setState m d =
             match appState with
             | Some state ->
                state.setState { state with render = fun () -> (Program.view program) m d }
             | _ ->
                appState <- Some { render = fun () -> (Program.view program) m d
                                   setState = ignore }

        program
        |> Program.withSetState setState
