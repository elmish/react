(**
---
layout: standard
title: Expo App
toc: false
---
**)

(*** hide ***)

#load "./../prelude.fsx"
#r "nuget: Fable.React.Native"

(**
Let's define our model:
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
### Rendering views with ReactNative
Let's open ReactNative bindings and define our view using them:

*)

open Fable.ReactNative
open Fable.ReactNative.Props

// define our button element
let button label onPress =
    text [ TextProperties.Style
            [ TextStyle.Color "#FFFFFF"
              TextAlign TextAlignment.Center
              Margin (dip 5.)
              FontSize 15. ]]
          label
    |> touchableHighlightWithChild
        [ TouchableHighlightProperties.Style
            [ BackgroundColor "#428bca"
              BorderRadius 4.
              Margin (dip 5.) ]
          TouchableHighlightProperties.UnderlayColor "#5499C4"
          OnPress onPress ]

let view count (dispatch:Dispatch<Msg>) =
    let onClick msg =
        fun () -> msg |> dispatch

    // construct RN view
    view [ ViewProperties.Style
            [ AlignSelf Alignment.Stretch
              Padding (dip 20.)
              ShadowColor "#000000"
              ShadowOpacity 0.8
              ShadowRadius 3.
              JustifyContent JustifyContent.Center
              Flex 1.
              BackgroundColor "#615A5B" ]]
         [ button "-" (onClick Decrement)
           text [] (string count)
           button "+" (onClick Increment) ]

(**
### Create the program instance
For Expo apps, install the `Fable.Elmish.ReactNative` package:
```sh
dotnet add package Fable.Elmish.ReactNative
```

and the `expo` npm package (included by default in Expo projects):
```sh
yarn add expo react react-native
```

Then use `Program.withExpo` which integrates with Expo's `registerRootComponent`:

*)
open Elmish.Expo

Program.mkSimple init update view
|> Program.withExpo
|> Program.run

(**

Unlike `withReactNative`, `withExpo` does not require an app key — Expo handles component registration automatically.
It also provides Expo dev tools integration and proper web platform rendering support.

### Project setup

Set the `"main"` field in `package.json` to point to the Fable-compiled entry point:
```json
{
  "main": "out/App.js"
}
```

No `index.js` file is needed — Expo reads the `"main"` field directly.

Configure Metro to find the Fable output in `metro.config.js`:
```js
const { getDefaultConfig } = require('expo/metro-config');
const config = getDefaultConfig(__dirname);
config.watchFolders = [__dirname + '/out'];
module.exports = config;
```

Use `babel-preset-expo` in `babel.config.js`:
```js
module.exports = function (api) {
  api.cache(true);
  return { presets: ['babel-preset-expo'] };
};
```

### Running

1. `dotnet fable watch src/app.fsproj -o out` — compile F# to JS in watch mode
2. `npx expo start` — start the Expo dev server

For a complete example, see [sample-expo-counter](https://github.com/nicholaslawrence/sample-expo-counter).
*)
