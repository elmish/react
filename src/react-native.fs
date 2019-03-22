namespace Elmish.ReactNative

open Fable.React
open Fable.Core
open Elmish

module Components =
    type AppState = {
        render : unit -> ReactElement
        setState : AppState -> unit
    }

    let mutable appState = None

    type App(props) as this =
        inherit Component<obj,AppState>(props)
        do
            match appState with
            | Some state ->
                appState <- Some { state with AppState.setState = this.setInitState }
                this.setInitState state
            | _ -> failwith "was Elmish.ReactNative.Program.withReactNative called?"

        override this.componentDidMount() =
            appState <- Some { appState.Value with setState = fun s -> this.setState(fun _ _ -> s) }

        override this.componentWillUnmount() =
            appState <- Some { appState.Value with setState = ignore; render = this.state.render }

        override this.render () =
            this.state.render()

[<Import("AppRegistry","react-native")>]
type AppRegistry =
    static member registerComponent(appKey:string, getComponentFunc:unit->ReactElementType<_>) : unit =
        failwith "JS only"

[<RequireQualifiedAccess>]
module Program =
    open Elmish.React
    open Components

    /// Setup rendering of root ReactNative component
    let withReactNative appKey (program:Program<_,_,_,_>) =
        AppRegistry.registerComponent(appKey, fun () -> unbox JsInterop.jsConstructor<App>)
        let setState m d =
             match appState with
             | Some state ->
                state.setState { state with render = fun () -> (Program.view program) m d }
             | _ ->
                appState <- Some { render = fun () -> (Program.view program) m d
                                   setState = ignore }

        program
        |> Program.withSetState setState
