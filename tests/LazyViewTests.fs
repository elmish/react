module Tests.LazyViewTests

open Fable.Mocha
open Fable.React
open Elmish
open Elmish.React
open Browser
open Tests.Helpers

let private refEq a b = obj.ReferenceEquals(a, b)

/// Minimal helper to create a react element without the Fable.React DSL package.
let private el tag (text: string) : ReactElement =
    ReactBindings.React.createElement(tag, null, [ unbox<ReactElement> text ])

let tests = testList "LazyView memo" [

    testList "lazyView2With (partial application)" [

        testCase "skips render when model reference is unchanged" <| fun _ ->
            let mutable renderCount = 0
            let view (model: {| value: int |}) (_dispatch: unit Dispatch) =
                renderCount <- renderCount + 1
                el "div" (string model.value)

            // Partial-apply once to capture the memo component in the closure
            let render = lazyView2With refEq view

            let container = document.createElement "div"
            let root = ReactDomClient.createRoot container
            let model = {| value = 42 |}

            flushSync (fun () ->
                root.render (render model ignore))
            Expect.equal renderCount 1 "first render"

            flushSync (fun () ->
                root.render (render model ignore))
            Expect.equal renderCount 1 "same model ref should skip render"

        testCase "re-renders when model changes" <| fun _ ->
            let mutable renderCount = 0
            let view (model: {| value: int |}) (_dispatch: unit Dispatch) =
                renderCount <- renderCount + 1
                el "div" (string model.value)

            let render = lazyView2With refEq view

            let container = document.createElement "div"
            let root = ReactDomClient.createRoot container

            flushSync (fun () ->
                root.render (render {| value = 1 |} ignore))
            Expect.equal renderCount 1 "first render"

            flushSync (fun () ->
                root.render (render {| value = 2 |} ignore))
            Expect.equal renderCount 2 "different model should re-render"

        testCase "ignores dispatch changes when model is unchanged" <| fun _ ->
            let mutable renderCount = 0
            let view (model: {| value: int |}) (_dispatch: string Dispatch) =
                renderCount <- renderCount + 1
                el "div" (string model.value)

            let render = lazyView2With refEq view

            let container = document.createElement "div"
            let root = ReactDomClient.createRoot container
            let model = {| value = 42 |}

            flushSync (fun () ->
                root.render (render model (fun _ -> ())))
            Expect.equal renderCount 1 "first render"

            flushSync (fun () ->
                root.render (render model (fun _ -> ())))
            Expect.equal renderCount 1 "different dispatch same model should skip"
    ]

    testList "lazyViewWith" [

        testCase "skips render when model unchanged" <| fun _ ->
            let mutable renderCount = 0
            let view (model: {| value: int |}) =
                renderCount <- renderCount + 1
                el "div" (string model.value)

            let container = document.createElement "div"
            let root = ReactDomClient.createRoot container
            let model = {| value = 42 |}

            // lazyViewWith uses __memo stamping — works inline (1-arg, no curry adapter)
            flushSync (fun () ->
                root.render (lazyViewWith refEq view model))
            Expect.equal renderCount 1 "first render"

            flushSync (fun () ->
                root.render (lazyViewWith refEq view model))
            Expect.equal renderCount 1 "same model should skip"
    ]

    testList "lazyView3With (partial application)" [

        testCase "structural equality skips on same values" <| fun _ ->
            let mutable renderCount = 0
            let view (s1: int) (s2: string) (_dispatch: unit Dispatch) =
                renderCount <- renderCount + 1
                el "div" (sprintf "%d-%s" s1 s2)

            // lazyView3 = lazyView3With (=) — partial apply captures memo
            let render = lazyView3 view

            let container = document.createElement "div"
            let root = ReactDomClient.createRoot container

            flushSync (fun () ->
                root.render (render 1 "a" ignore))
            Expect.equal renderCount 1 "first render"

            flushSync (fun () ->
                root.render (render 1 "a" ignore))
            Expect.equal renderCount 1 "structural eq on same values should skip"

        testCase "ReferenceEquals re-renders due to new tuple allocation" <| fun _ ->
            let mutable renderCount = 0
            let view (s1: int) (s2: string) (_dispatch: unit Dispatch) =
                renderCount <- renderCount + 1
                el "div" (sprintf "%d-%s" s1 s2)

            let render = lazyView3With refEq view

            let container = document.createElement "div"
            let root = ReactDomClient.createRoot container

            flushSync (fun () ->
                root.render (render 1 "a" ignore))
            Expect.equal renderCount 1 "first render"

            flushSync (fun () ->
                root.render (render 1 "a" ignore))
            Expect.equal renderCount 2 "new tuple alloc = different ref = re-render"
    ]

    testList "component identity" [

        testCase "same partial application produces same memo component type" <| fun _ ->
            let view (model: int) (_dispatch: unit Dispatch) =
                el "div" (string model)

            // Single partial application — one memo in the closure
            let render = lazyView2With (=) view
            let el1 = render 1 ignore
            let el2 = render 2 ignore

            Expect.isTrue
                (obj.ReferenceEquals(getElementType el1, getElementType el2))
                "same partial application should yield same memo component type"

        testCase "different view functions produce different memo component types" <| fun _ ->
            let view1 (model: int) (_dispatch: unit Dispatch) =
                el "div" (string model)
            let view2 (model: int) (_dispatch: unit Dispatch) =
                el "span" (string model)

            let render1 = lazyView2With (=) view1
            let render2 = lazyView2With (=) view2

            let el1 = render1 1 ignore
            let el2 = render2 1 ignore

            Expect.isFalse
                (obj.ReferenceEquals(getElementType el1, getElementType el2))
                "different views should yield different memo component types"
    ]

    testList "inline usage (no partial application)" [

        testCase "lazyView2 inline skips render with same model" <| fun _ ->
            let mutable renderCount = 0
            let view (model: {| value: int |}) (_dispatch: unit Dispatch) =
                renderCount <- renderCount + 1
                el "div" (string model.value)

            let container = document.createElement "div"
            let root = ReactDomClient.createRoot container
            let model = {| value = 42 |}

            // Inline usage like: lazyView2 view model dispatch (no partial application)
            flushSync (fun () ->
                root.render (lazyView2 view model ignore))
            Expect.equal renderCount 1 "first render"

            flushSync (fun () ->
                root.render (lazyView2 view model ignore))
            Expect.equal renderCount 1 "same model should skip via __memo stamping"

        testCase "lazyView3 inline skips render with structurally equal values" <| fun _ ->
            let mutable renderCount = 0
            let view (s1: int) (s2: string) (_dispatch: unit Dispatch) =
                renderCount <- renderCount + 1
                el "div" (sprintf "%d-%s" s1 s2)

            let container = document.createElement "div"
            let root = ReactDomClient.createRoot container

            flushSync (fun () ->
                root.render (lazyView3 view 1 "a" ignore))
            Expect.equal renderCount 1 "first render"

            flushSync (fun () ->
                root.render (lazyView3 view 1 "a" ignore))
            Expect.equal renderCount 1 "same values should skip via __memo stamping"
    ]

    testList "displayName" [

        testCase "lazyViewWith sets displayName from view function name" <| fun _ ->
            let element = lazyViewWith (=) Tests.DisplayNameViews.namedView1 42
            let name = getDisplayName element
            Expect.equal name (Some "namedView1") "displayName should match the F# function name"

        testCase "lazyView2With sets displayName from view function name" <| fun _ ->
            let render = lazyView2With (=) Tests.DisplayNameViews.namedView2
            let element = render 42 ignore
            let name = getDisplayName element
            Expect.equal name (Some "namedView2") "displayName should match the F# function name"

        testCase "lazyView3With sets displayName from view function name" <| fun _ ->
            let render = lazyView3With (=) Tests.DisplayNameViews.namedView3
            let element = render 1 "a" ignore
            let name = getDisplayName element
            Expect.equal name (Some "namedView3") "displayName should match the F# function name"

        testCase "anonymous lambda has no displayName" <| fun _ ->
            let render = lazyView2With (=) (fun (model: int) (_dispatch: unit Dispatch) ->
                el "div" (string model))

            let element = render 42 ignore
            let name = getDisplayName element
            Expect.equal name None "anonymous lambda should have no displayName"
    ]

    testList "withKey" [

        testCase "sets key on a lazyView2 element" <| fun _ ->
            let view (model: int) (_dispatch: unit Dispatch) = el "div" (string model)
            let render = lazyView2With (=) view
            let element = render 42 ignore |> withKey "my-key"
            Expect.equal (getElementKey element) (Some "my-key") "key should be set"

        testCase "sets key on a lazyViewWith element" <| fun _ ->
            let view (model: int) = el "div" (string model)
            let element = lazyViewWith (=) view 42 |> withKey "k1"
            Expect.equal (getElementKey element) (Some "k1") "key should be set"

        testCase "sets key on a lazyView3 element" <| fun _ ->
            let view s1 s2 (_dispatch: unit Dispatch) = el "div" (sprintf "%d-%s" s1 s2)
            let render = lazyView3With (=) view
            let element = render 1 "a" ignore |> withKey "k3"
            Expect.equal (getElementKey element) (Some "k3") "key should be set"

        testCase "sets key on a plain ReactElement" <| fun _ ->
            let element = el "span" "hello" |> withKey "plain"
            Expect.equal (getElementKey element) (Some "plain") "key should be set on plain element"

        testCase "element without withKey has no key" <| fun _ ->
            let view (model: int) (_dispatch: unit Dispatch) = el "div" (string model)
            let render = lazyView2With (=) view
            let element = render 42 ignore
            Expect.equal (getElementKey element) None "key should be None"

        testCase "preserves element type" <| fun _ ->
            let view (model: int) (_dispatch: unit Dispatch) = el "div" (string model)
            let render = lazyView2With (=) view
            let original = render 42 ignore
            let keyed = original |> withKey "k"
            Expect.isTrue
                (obj.ReferenceEquals(getElementType original, getElementType keyed))
                "element type should be preserved"

        testCase "memo still skips render with withKey" <| fun _ ->
            let mutable renderCount = 0
            let view (model: {| value: int |}) (_dispatch: unit Dispatch) =
                renderCount <- renderCount + 1
                el "div" (string model.value)

            let render = lazyView2With refEq view

            let container = document.createElement "div"
            let root = ReactDomClient.createRoot container
            let model = {| value = 42 |}

            flushSync (fun () ->
                root.render (render model ignore |> withKey "k1"))
            Expect.equal renderCount 1 "first render"

            flushSync (fun () ->
                root.render (render model ignore |> withKey "k1"))
            Expect.equal renderCount 1 "memo should still skip with withKey"

        testCase "keyed elements are reordered without remount" <| fun _ ->
            // Shared mount counter accessible from JS
            let counter : obj = Fable.Core.JsInterop.emitJsExpr () "({current: 0})"
            let getMountCount () : int = Fable.Core.JsInterop.emitJsExpr counter "$0.current"

            // Create a stable React function component via JS.
            // It captures `counter` and the react import for useEffect/createElement.
            let react : obj = Fable.Core.JsInterop.emitJsExpr ReactBindings.React "$0"
            let comp : obj =
                Fable.Core.JsInterop.emitJsExpr
                    (react, counter)
                    """(function() {
                        var R = $0, ctr = $1;
                        function TrackedComp(props) {
                            R.useEffect(function() { ctr.current++; }, []);
                            return R.createElement('span', null, props.label);
                        }
                        return TrackedComp;
                    })()"""

            let makeEl label key =
                ReactBindings.React.createElement(unbox comp, {| label = label |}, [])
                |> withKey key

            let container = document.createElement "div"
            let root = ReactDomClient.createRoot container

            // Render [A, B, C]
            flushSync (fun () ->
                root.render (
                    ReactBindings.React.createElement("div", null, [
                        makeEl "A" "a"
                        makeEl "B" "b"
                        makeEl "C" "c"
                    ])))
            Expect.equal (getMountCount()) 3 "initial mount of 3 items"
            Expect.equal container.textContent "ABC" "initial order"

            // Reorder to [C, A, B]
            flushSync (fun () ->
                root.render (
                    ReactBindings.React.createElement("div", null, [
                        makeEl "C" "c"
                        makeEl "A" "a"
                        makeEl "B" "b"
                    ])))
            Expect.equal (getMountCount()) 3 "keyed reorder should not remount"
            Expect.equal container.textContent "CAB" "reordered content"
    ]
]
