(*** hide ***)
#I ".paket/load/netstandard2.0"
#I "../../.paket/load/netstandard2.0"
#I "../../src/bin/Debug/netstandard2.0"
#load "Fable.React.fsx"
#load "Fable.Elmish.fsx"
#r "Fable.Elmish.React.dll"

(**
*)
namespace Elmish.ReactNative

open Fable.Import.React
open Fable.Core
open Elmish

module Components =
    type [<Pojo>] AppState = {
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
            appState <- Some { appState.Value with setState = this.setState }

        override this.componentWillUnmount() =
            appState <- Some { appState.Value with setState = ignore; render = this.state.render }

        override this.render () =
            this.state.render()

[<Import("AppRegistry","react-native")>]
type AppRegistry =
    static member registerComponent(appKey:string, getComponentFunc:unit->ComponentClass<_>) : unit =
        failwith "JS only"

[<RequireQualifiedAccess>]
module Program =
    open Elmish.React
    open Components

    /// Setup rendering of root ReactNative component
    let withReactNative appKey (program:Program<_,_,_,_>) =
        AppRegistry.registerComponent(appKey, fun () -> unbox typeof<App>)
        let render dispatch =
            let viewWithDispatch = program.view dispatch
            fun model ->
                match appState with
                | Some state ->
                    state.setState { state with render = fun () -> viewWithDispatch model }
                | _ ->
                    appState <- Some { render = fun () -> viewWithDispatch model
                                       setState = ignore }
        { program with setState = render }
