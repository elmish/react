namespace Elmish.React

open System
open Fable.Import.React
open Fable.Core

[<RequireQualifiedAccess>]
module Program =
    module R = Fable.Helpers.React

    type private SingleObservable<'T>() =
        let mutable listener: IObserver<'T> option = None
        member __.Trigger v =
            listener |> Option.iter (fun lis -> lis.OnNext(v))
        interface IObservable<'T> with
            member __.Subscribe w =
                listener <- Some w
                { new IDisposable with
                    member __.Dispose() = listener <- None }

    type [<Pojo>] private Props<'S, 'Msg> = {
        view: 'S -> ('Msg->unit) -> ReactElement
        dispatch: 'Msg->unit
        observable: IObservable<'S>
    }

    type [<Pojo>] private State<'T> = { value: 'T }

    type private ReactCom<'S, 'Msg>(initProps, initState) =
        inherit Component<Props<'S, 'Msg>, State<'S>>(initProps)
        let mutable disp = None
        do base.setInitState { value = initState }

        member this.componentDidMount() =
            disp <- Some (this.props.observable.Subscribe(fun model ->
                this.setState({ value = model })))

        member __.componentWillUnmount() =
            disp |> Option.iter (fun d -> d.Dispose())

        member this.shouldComponentUpdate(_, nextState) =
            not(obj.ReferenceEquals(this.state.value, nextState.value))

        member this.render() =
            this.props.view this.state.value this.props.dispatch

    open Fable.Import.Browser

    /// Setup rendering of root React component inside html element identified by placeholderId
    let withReact placeholderId (program:Elmish.Program<_,_,_,_>) =
        let mutable rootEl = None
        let mutable lastRequest = None
        let observable = SingleObservable()

        let setState model dispatch =
            match rootEl with
            | Some _ ->
                match lastRequest with
                | Some r -> window.cancelAnimationFrame r
                | None -> ()
                lastRequest <- Some (window.requestAnimationFrame (fun _ ->
                    observable.Trigger(model)))
            | None ->
                let domEl = document.getElementById(placeholderId)
                let reactEl = R.com<ReactCom<_,_>,_,_>
                                { view = program.view
                                  dispatch = dispatch
                                  observable = observable } []
                rootEl <- Some reactEl
                Fable.Import.ReactDom.render(reactEl, domEl)

        { program with setState = setState }
