## 6.0.0-beta-2
* Split Native and React
* React Native and Expo views now use `lazyView2With` (React.memo) to skip re-renders when the model reference is unchanged

## 5.6.0
* Initial Expo support for React Native

## 5.5.0
* Fixes root element remounting on each loop iteration
* Replaces lazy view function implementations with memo
* Lazy views preserve original view function's name
* Introduces `withKey` function for lazy views to support sibling element differentiation (#14)

## 5.0.1
* Update to latest Elmish (5.0.2)

## 5.0.0
* Breaking: Elmish v5 reference, Fable v4

## 5.0.0-beta-2
* Breaking: Remove support for React 17 (#99), thanks @kerams

## 4.0.0
* Breaking: Use Fable.ReactDom.Types dependency (#71), thanks Alfonso Garcia-Caro!
* Support for React 18
* Breaking: Support for elmish v4

## 4.0.0-beta-4
* Breaking: Use Fable.ReactDom.Types dependency (#71), thanks Alfonso Garcia-Caro!

## 4.0.0-beta-3
* Breaking: Use Fable.React.Types dependency (#68), thanks Alfonso Garcia-Caro!

## 4.0.0-beta-2

* Support for React 18

## 4.0.0-beta-1

* Support for elmish v4

## 3.0.1

* Releasing 3.0

## 3.0.0-beta-5

* Update to latest Fable.React (beta-009)

## 3.0.0-beta-4

* Update to latest Elmish (beta-7) and opaque `Program`

## 3.0.0-beta-3

* Update to latest Elmish (beta-5) to make `withConsoleTrace` work in latest Fable 3

## 3.0.0-beta-2

* Fable 3 support courtesy of Alfonso

## 3.0.0-beta-1

* Elmish 3.0 compat

## 2.2.0

* Fix `withReactNative` for Fable 2

## 2.1.0

* Rework internal implementation to help HMR support

## 2.0.0
* Release stable for Fable2

## 2.0.0-beta-4

* Re-releasing v1 for Fable2

## 1.0.3

* Add doc xml

## 1.0.2

* Adding support for SSR, credit: @zaaack

## 1.0.1

* Use new React bindings, various text input jumping cursor workarounds

## 0.9.0

* Stable fable 1.x release

## 0.9.0-beta-4

* Fable 1.x

## 0.8.3

* Optimizing rendering #41

## 0.8.2

* Fixed relative path to elmish

## 0.8.1

* Updated React native root to pass (unused) `props`

## 0.8.0

* Lookup placeholdId by Id, instead of class

## 0.7.2

* Fix dependencies

## 0.7.1

* Handling unmounted properly

## 0.7.1-alpha.5

* Working around deprecated RN API

## 0.7.1-alpha.3

* Update dependencies

## 0.7.1-alpha.2

* Support for new `Program` API

## 0.7.0-alpha.1

* Migrate to Fable 0.7
