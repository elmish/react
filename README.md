Elmish-React: React extensions for [fable-elmish](https://github.com/fable-compiler/fable-elmish) applications.
=======
[![Windows Build](https://ci.appveyor.com/api/projects/status/cx5i4xwsfusulw7u?svg=true)](https://ci.appveyor.com/project/et1975/elmish-react) [![Mono/OSX build](https://travis-ci.org/fable-elmish/react.svg?branch=master)](https://travis-ci.org/fable-elmish/react) [![NuGet version](https://badge.fury.io/nu/react.svg)](https://badge.fury.io/nu/react)

React and ReactNative support for Elmish apps.

For more information see [the docs](https://fable-elmish.github.io/react).

## Installation
The easiest way to start with Elmish and React is to use the [template](https://github.com/fable-elmish/templates):


```shell
dotnet new -i "Fable.Template.Elmish.React::*"
dotnet new fable-elmish-react -n MyProject
```

Alternatively, you can just add it to an existing project via paket:

```shell
paket add nuget Fable.Elmish.React
```

