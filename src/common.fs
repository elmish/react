namespace Elmish.React

open Fable.React
open Elmish

module Internal =
    open Browser.Types
    open Fable.Core.JsInterop

    let updateInputValue (value: string) (e: HTMLInputElement) =
        if not(isNull e) && e.value <> value then
            e.value <- value

type MemoProps1<'model> = {
    model:'model
    equal:'model->'model->bool
}

type MemoProps2<'model,'msg> = {
    model:'model
    dispatch:'msg Dispatch
    equal:'model->'model->bool
}

[<AutoOpen>]
module Common =
    open Fable.Core.JsInterop

    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model
    /// state: new state to render
    let lazyViewWith (equal:'model->'model->bool)
                     (view:'model->ReactElement)
                     (state:'model) =
        let memoized : ReactElementType<MemoProps1<'model>> =
            emitJsExpr
                (view, fun () ->
                    let m =
                        ReactBindings.React.memo(
                            (fun (props: MemoProps1<'model>) -> view props.model),
                            (fun (prev: MemoProps1<'model>) (next: MemoProps1<'model>) -> next.equal prev.model next.model))
                    emitJsStatement (m, view) "$0.displayName = $1.name || void 0"
                    m)
                "$0.__memo || ($0.__memo = $1())"
        ReactBindings.React.createElement(memoized, { model = state; equal = equal }, [])

    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model using the dispatch
    /// Partially apply with equal and view to get a cached rendering function:
    /// let render = lazyView2With equal view
    /// render state dispatch
    let lazyView2With (equal:'model->'model->bool)
                      (view:'model->'msg Dispatch->ReactElement) =
        let memoized : ReactElementType<MemoProps2<'model,'msg>> =
            emitJsExpr
                (view, fun () ->
                    let m =
                        ReactBindings.React.memo(
                            (fun (props: MemoProps2<'model,'msg>) -> view props.model props.dispatch),
                            (fun (prev: MemoProps2<'model,'msg>) (next: MemoProps2<'model,'msg>) -> next.equal prev.model next.model))
                    emitJsStatement (m, view) "$0.displayName = $1.name || void 0"
                    m)
                "$0.__memo || ($0.__memo = $1())"
        fun (state:'model) (dispatch:'msg Dispatch) ->
            ReactBindings.React.createElement(memoized, { model = state; dispatch = dispatch; equal = equal }, [])

    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new model (a tuple of two states)
    /// view: function to render the model using the dispatch
    /// Partially apply with equal and view to get a cached rendering function:
    /// let render = lazyView3With equal view
    /// render state1 state2 dispatch
    let lazyView3With (equal:_->_->bool) (view:_->_->_->ReactElement) =
        let memoized : ReactElementType<MemoProps2<_,_>> =
            emitJsExpr
                (view, fun () ->
                    let m =
                        ReactBindings.React.memo(
                            (fun (props: MemoProps2<_,_>) ->
                                let (s1, s2) = props.model
                                view s1 s2 props.dispatch),
                            (fun (prev: MemoProps2<_,_>) (next: MemoProps2<_,_>) -> next.equal prev.model next.model))
                    emitJsStatement (m, view) "$0.displayName = $1.name || void 0"
                    m)
                "$0.__memo || ($0.__memo = $1())"
        fun state1 state2 (dispatch:'msg Dispatch) ->
            ReactBindings.React.createElement(memoized, { model = (state1, state2); dispatch = dispatch; equal = equal }, [])

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

    /// Set the React key on an element for list reconciliation.
    /// Useful with lazyView to differentiate sibling memo components:
    /// lazyView2 view model dispatch |> withKey "my-id"
    let withKey (key:string) (element:ReactElement) : ReactElement =
        emitJsExpr (element, key) "({...$0, key: $1})"
