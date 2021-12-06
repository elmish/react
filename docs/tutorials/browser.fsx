(**
---
layout: standard
title: Browser SPA
toc: false
---
**)

(*** hide ***)

#load "./../prelude.fsx"

(**
Let's define our model
*)

type Model = int

type Msg =
    | Increment
    | Decrement

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

open Fable.React
open Fable.React.Props

let view model dispatch =

    div []
        [
            button [ OnClick (fun _ -> dispatch Decrement) ] [ str "-" ]
            div [] [ str (sprintf "%A" model) ]
            button [ OnClick (fun _ -> dispatch Increment) ] [ str "+" ]
        ]

(**
### Create the program instance
Now we need to tell React to render our view in the div placeholder we defined in our `index.html`:
```
<div id="elmish-app"></div>
```

by augumenting our program instance and passing the placeholder id:
*)

open Elmish.React

Program.mkSimple init update view
|> Program.withReactBatched "elmish-app"
|> Program.run

(**

And that's it.

*)
