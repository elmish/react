Elmish-React: React extensions for [elmish](https://github.com/elmish/elmish) applications.
=======
[![Windows Build](https://ci.appveyor.com/api/projects/status/vg3200aksdbvx5me/branch/v4.x?svg=true)](https://ci.appveyor.com/project/et1975/react/branch/v4.x) [![NuGet version](https://badge.fury.io/nu/Fable.Elmish.React.svg)](https://badge.fury.io/nu/Fable.Elmish.React)

React and ReactNative support for Elmish apps.

For more information see [the docs](https://elmish.github.io/react).

## Installation
The easiest way to start with Elmish and React is to use the [template](https://github.com/elmish/templates):


```shell
dotnet new -i "Fable.Template.Elmish.React::*"
dotnet new elmish-react -n MyProject
```

Alternatively, you can just add it to an existing project via paket:

```shell
paket add nuget Fable.Elmish.React
```

As with any JS dependency, if you are authoring an application (as opposed to a library), you'll also need to install React (separately, via `npm` or `yarn`):

```shell
yarn add react react-dom
```
