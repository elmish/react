namespace Elmish.React

open System
open Fable.Import.React
open Fable.Core

[<RequireQualifiedAccess>]
module Program =
    module R = Fable.Helpers.React

    [<Emit("typeof module === 'object' && module.hot")>]
    let private isHotReloadEnabled(): bool = jsNative

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
          getInitState: unit -> ('S * ('Msg->unit)) }

    type [<Pojo>] private State<'S, 'Msg> =
        { model: 'S
          dispatch: 'Msg->unit }

    type private ReactCom<'S, 'Msg>(p) =
        inherit Component<Props<'S, 'Msg>, State<'S, 'Msg>>(p)
        let mutable disp = None
        let model, dispatch = p.getInitState()
        do base.setInitState { model = model; dispatch = dispatch }

        member this.componentDidMount() =
            disp <- Some (this.props.observable.Subscribe(fun (model, dispatch) ->
                this.setState { model = model; dispatch = dispatch }))

        member __.componentWillUnmount() =
            disp |> Option.iter (fun d -> d.Dispose())

        member this.shouldComponentUpdate(_, nextState) =
            not(obj.ReferenceEquals(this.state.model, nextState.model))

        member this.render() =
            this.props.view this.state.model this.state.dispatch

    open Fable.Import.Browser

    /// Setup rendering of root React component inside html element identified by placeholderId
    let withReact placeholderId (program:Elmish.Program<_,_,_,_>) =
        let mutable observable: SingleObservable<_> option = None

        let setState model dispatch =
            #if DEBUG
            if isHotReloadEnabled() then
                Fable.Import.ReactDom.render(
                    lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) program.view model dispatch,
                    document.getElementById(placeholderId))
            else
            #endif
                match observable with
                | Some obs ->
                    obs.Trigger(model, dispatch)
                | None ->
                    let obs = SingleObservable()
                    observable <- Some obs
                    let domEl = document.getElementById(placeholderId)
                    let reactEl = R.com<ReactCom<_,_>,_,_>
                                    { view = program.view
                                      observable = obs
                                      getInitState = fun () -> (model, dispatch) } []
                    Fable.Import.ReactDom.render(reactEl, domEl)

        { program with setState = setState }
