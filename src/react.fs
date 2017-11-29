namespace Elmish.React

open System
open Fable.Import.React
open Fable.Core

[<RequireQualifiedAccess>]
module Program =
    open Fable.Import.Browser
    module R = Fable.Helpers.React

    /// Setup rendering of root React component inside html element identified by placeholderId
    let withReact placeholderId (program:Elmish.Program<_,_,_,_>) =
        let setState model dispatch =
            Fable.Import.ReactDom.render(
                lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) program.view model dispatch,
                document.getElementById(placeholderId)
            )

        { program with setState = setState }