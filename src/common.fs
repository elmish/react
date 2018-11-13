namespace Elmish.React

open System
open Fable.Import.React
open Fable.Helpers.React
open Fable.Core
open Elmish

type LazyProps<'model> = {
    model:'model
    render:unit->ReactElement
    equal:'model->'model->bool
}

module Components =
    open Fable.Core.JsInterop

    let internal createLazyView (displayName: string): ComponentClass<LazyProps<'model>> =
        importMember "./util.js"

    type LazyView<'model>(props) =
        inherit Component<LazyProps<'model>,obj>(props)

        override this.shouldComponentUpdate(nextProps, _nextState) =
            not <| this.props.equal this.props.model nextProps.model

        override this.render () =
            this.props.render ()

[<AutoOpen>]
module Common =
    /// Build a React component that avoids rendering the view unless the model has changed.
    /// Store the result of applying the first argument and use it in your render function. Example:
    /// ```
    ///     let hello = lazyViewBuilder "Hello"
    ///     let render model =
    ///        hello equal view state
    /// ```
    /// * displayName: name to be displayed in React dev tools
    /// * equal: function to compare the previous and the new states
    /// * view: function to render the model
    /// * state: new state to render
    let lazyViewBuilder (displayName: string) =
        let com = Components.createLazyView displayName
        fun (equal:'model->'model->bool) (view:'model->ReactElement) (state:'model) ->
            from com { render = fun () -> view state
                       equal = equal
                       model = state } []

    /// Build a React component that avoids rendering the view unless the model has changed.
    /// Store the result of applying the first argument and use it in your render function. Example:
    /// ```
    ///     let hello = lazyView2Builder "Hello"
    ///     let render model =
    ///        hello equal view state dispatch
    /// ```
    /// * displayName: name to be displayed in React dev tools
    /// * equal: function to compare the previous and the new states
    /// * view: function to render the model using the dispatch
    /// * state: new state to render
    /// * dispatch: dispatch function
    let lazyView2Builder (displayName: string) =
        let com = Components.createLazyView displayName
        fun (equal:'model->'model->bool)
             (view:'model->'msg Dispatch->ReactElement)
             (state:'model)
             (dispatch:'msg Dispatch) ->
            from com { render = fun () -> view state dispatch
                       equal = equal
                       model = state } []

    /// Avoid rendering the view unless the model has changed.
    /// * equal: function to compare the previous and the new states
    /// * view: function to render the model
    /// * state: new state to render
    let lazyViewWith (equal:'model->'model->bool)
                     (view:'model->ReactElement)
                     (state:'model) =
        ofType<Components.LazyView<_>,_,_>
            { render = fun () -> view state
              equal = equal
              model = state }
            []

    /// Avoid rendering the view unless the model has changed.
    /// * equal: function to compare the previous and the new states
    /// * view: function to render the model using the dispatch
    /// * state: new state to render
    /// * dispatch: dispatch function
    let lazyView2With (equal:'model->'model->bool)
                      (view:'model->'msg Dispatch->ReactElement)
                      (state:'model)
                      (dispatch:'msg Dispatch) =
        ofType<Components.LazyView<_>,_,_>
            { render = fun () -> view state dispatch
              equal = equal
              model = state }
            []

    /// Avoid rendering the view unless the model has changed.
    /// * equal: function to compare the previous and the new model (a tuple of two states)
    /// * view: function to render the model using the dispatch
    /// * state1: new state to render
    /// * state2: new state to render
    /// * dispatch: dispatch function
    let lazyView3With (equal:('model1*'model2)->('model1*'model2)->bool)
                      (view:'model1->'model2->'msg Dispatch->ReactElement)
                      (state1:'model1)
                      (state2:'model2)
                      (dispatch:'msg Dispatch) =
        ofType<Components.LazyView<_>,_,_>
            { render = fun () -> view state1 state2 dispatch
              equal = equal
              model = (state1,state2) }
            []

    /// Avoid rendering the view unless the model has changed.
    /// * view: function of model to render the view
    let lazyView (view:'model->ReactElement) =
        lazyViewWith (=) view

    /// Avoid rendering the view unless the model has changed.
    /// * view: function of two arguments to render the model using the dispatch
    let lazyView2 (view:'model->'msg Dispatch->ReactElement) =
        lazyView2With (=) view

    /// Avoid rendering the view unless the model has changed.
    /// view: function of three arguments to render the model using the dispatch
    let lazyView3 (view:'model1->'model2->'msg Dispatch->ReactElement) =
        lazyView3With (=) view
