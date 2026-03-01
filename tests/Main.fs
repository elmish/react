module Tests.Main

open Fable.Mocha

let allTests = testList "All" [
    LazyViewTests.tests
]

[<EntryPoint>]
let main _ = Mocha.runTests allTests
