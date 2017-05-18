(*** hide ***)
#I "../../src/bin/Debug/netstandard1.6"
#I "../../packages/Fable.Core/lib/netstandard1.6"
#I "../../packages/Fable.Elmish/lib/netstandard1.6"
#I "../../packages/Fable.React/lib/netstandard1.6"
#r "Fable.Core.dll"
#r "Fable.React.dll"
#r "Fable.Elmish.dll"
#r "Fable.Elmish.React.dll"
open Elmish

(**
A React SPA
=======

Let's define our model
*)

type Model = int

type Msg = Increment | Decrement

(**
### Handle our state initialization and updates
*)

open Elmish

let init () =
  0

let update (msg:Msg) count =
  match msg with
  | Increment -> count + 1
  | Decrement -> count - 1
(**
### Rendering views with React
Let's open React bindings and define our view using them:

*)

open Fable.Helpers.React.Props
module R = Fable.Helpers.React

let view model dispatch =

  R.div []
      [ R.button [ OnClick (fun _ -> dispatch Decrement) ] [ R.str "-" ]
        R.div [] [ R.str (sprintf "%A" model) ]
        R.button [ OnClick (fun _ -> dispatch Increment) ] [ R.str "+" ] ]

(**
### Create the program instance
Now we need to tell React to render our view in the div placeholder we defined in our `index.html`:
```
<div id="elmish-app"></div>
```
*)

open Elmish.React

Program.mkSimple init update view
|> Program.withReact "elmish-app"
|> Program.run

(**
And that's it.
*)
