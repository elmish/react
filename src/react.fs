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

    type [<Pojo>] private Props<'S, 'Msg> =
        { view: 'S -> ('Msg->unit) -> ReactElement
          observable: IObservable<'S * ('Msg->unit)>
          initState: 'S * ('Msg->unit) }

    type [<Pojo>] private State<'S, 'Msg> =
        { model: 'S
          dispatch: 'Msg->unit }

    type private ReactCom<'S, 'Msg>(p) =
        // Don't keep properties to release memory of initial state
        inherit Component<obj, State<'S, 'Msg>>(null)
        let view = p.view
        let obs = p.observable
        let mutable disp = None
        do base.setInitState { model = fst p.initState; dispatch = snd p.initState }

        member this.componentDidMount() =
            disp <- Some (obs.Subscribe(fun (model, dispatch) ->
                this.setState { model = model; dispatch = dispatch }))

        member __.componentWillUnmount() =
            disp |> Option.iter (fun d -> d.Dispose())

        member this.shouldComponentUpdate(_, nextState) =
            not(obj.ReferenceEquals(this.state.model, nextState.model))

        member this.render() =
            view this.state.model this.state.dispatch

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
                    observable.Trigger(model, dispatch)))
            | None ->
                let domEl = document.getElementById(placeholderId)
                let reactEl = R.com<ReactCom<_,_>,_,_>
                                { view = program.view
                                  observable = observable
                                  initState = (model, dispatch) } []
                rootEl <- Some reactEl
                Fable.Import.ReactDom.render(reactEl, domEl)

        { program with setState = setState }
