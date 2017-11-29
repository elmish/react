namespace Elmish.React

open System
open Fable.Import.React
open Fable.Core
open FSharp
open Elmish
open Fable.Core.JsInterop
open Fable.Import.React

module AppComponents =
    type [<Pojo>] AppState = {
        render : unit -> ReactElement
    }

    type [<Pojo>] AppProps = {
        getInternalSetState: (AppState -> unit) -> unit
    }

    type App(props) as this =
        inherit Component<AppProps, AppState>(props)

        do this.setInitState { render = fun _ -> Unchecked.defaultof<ReactElement> }
        do (props.getInternalSetState this.setState)

        member this.render () =
            this.state.render()

    let inline app p c = Fable.Helpers.React.com<App, AppProps, AppState> p c
[<RequireQualifiedAccess>]
module Program =
    open Fable.Import
    open AppComponents
    module R = Fable.Helpers.React

    // global setInternalState should stay in top level, otherwise hmr won't working.
    let mutable private setInternalState: (AppState -> unit) option = None


    /// Setup rendering of root React component inside html element identified by placeholderId
    let withReact placeholderId (program:Elmish.Program<_,_,_,_>) =
        let setState model dispatch =
            match setInternalState with
            | Some setState ->
                let render () =
                    lazyView2With (fun a b -> obj.ReferenceEquals (a, b)) program.view model dispatch
                setState { render = render }
            | None ->
                failwith "withReact init failed."
        let props =  { getInternalSetState=(fun setState -> setInternalState <- Some setState) }
        let app = AppComponents.app props []
        ReactDom.render(
            app, Browser.document.getElementById(placeholderId)
        )
        { program with setState = setState }
