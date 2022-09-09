namespace Elmish.React

open Fable.React
open Elmish

module Internal =
    open Browser.Types
    open Fable.Core.JsInterop

    let inline ofType<'T,'P,'S when 'T :> Component<'P,'S>> (props: 'P) (children: ReactElement seq): ReactElement =
        ReactBindings.React.createElement(jsConstructor<'T>, props, children)

    let updateInputValue (value: string) (e: HTMLInputElement) =
        if not(isNull e) && e.value <> value then
            e.value <- value

type LazyProps<'model> = {
    model:'model
    render:unit->ReactElement
    equal:'model->'model->bool
}

module Components =
    type LazyView<'model>(props) =
        inherit Component<LazyProps<'model>,obj>(props)

        override this.shouldComponentUpdate(nextProps, _nextState) =
            not <| this.props.equal this.props.model nextProps.model

        override this.render () =
            this.props.render ()

[<AutoOpen>]
module Common =

    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model
    /// state: new state to render
    let lazyViewWith (equal:'model->'model->bool)
                     (view:'model->ReactElement)
                     (state:'model) =
        Internal.ofType<Components.LazyView<_>,_,_>
            { render = fun () -> view state
              equal = equal
              model = state }
            []

    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model using the dispatch
    /// state: new state to render
    /// dispatch: dispatch function
    let lazyView2With (equal:'model->'model->bool)
                      (view:'model->'msg Dispatch->ReactElement)
                      (state:'model)
                      (dispatch:'msg Dispatch) =
        Internal.ofType<Components.LazyView<_>,_,_>
            { render = fun () -> view state dispatch
              equal = equal
              model = state }
            []

    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new model (a tuple of two states)
    /// view: function to render the model using the dispatch
    /// state1: new state to render
    /// state2: new state to render
    /// dispatch: dispatch function
    let lazyView3With (equal:_->_->bool) (view:_->_->_->ReactElement) state1 state2 (dispatch:'msg Dispatch) =
        Internal.ofType<Components.LazyView<_>,_,_>
            { render = fun () -> view state1 state2 dispatch
              equal = equal
              model = (state1,state2) }
            []

    /// Avoid rendering the view unless the model has changed.
    /// view: function of model to render the view
    let lazyView (view:'model->ReactElement) =
        lazyViewWith (=) view

    /// Avoid rendering the view unless the model has changed.
    /// view: function of two arguments to render the model using the dispatch
    let lazyView2 (view:'model->'msg Dispatch->ReactElement) =
        lazyView2With (=) view

    /// Avoid rendering the view unless the model has changed.
    /// view: function of three arguments to render the model using the dispatch
    let lazyView3 (view:_->_->_->ReactElement) =
        lazyView3With (=) view


