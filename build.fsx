#r "paket:
storage: packages
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.Core.Target
nuget Fake.Core.ReleaseNotes
nuget Fake.Tools.Git
nuget FSharp.Formatting
nuget FSharp.Formatting.CommandTool
nuget Fake.DotNet.FSFormatting //"
#if !FAKE
#load ".fake/build.fsx/intellisense.fsx"
#r "Facades/netstandard"
#endif

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.Tools
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open System
open System.IO


let gitName = "react"
let gitOwner = "elmish"
let gitHome = sprintf "https://github.com/%s" gitOwner
let gitRepo = sprintf "git@github.com:%s/%s" gitOwner gitName

// Filesets
let projects  =
      !! "src/**.fsproj"


let withWorkDir = DotNet.Options.withWorkingDirectory

Target.create "Clean" (fun _ ->
    Shell.cleanDir "src/obj"
    Shell.cleanDir "src/bin"
)

Target.create "Restore" (fun _ ->
    projects
    |> Seq.iter (fun s ->
        let dir = Path.GetDirectoryName s
        DotNet.restore (fun a -> a.WithCommon (withWorkDir dir)) s
    )
)

Target.create "Build" (fun _ ->
    projects
    |> Seq.iter (fun s ->
        let dir = Path.GetDirectoryName s
        DotNet.build (fun a ->
            a.WithCommon
                (fun c ->
                    let c = c |> withWorkDir dir
                    {c with CustomParams = Some "/p:SourceLinkCreate=true"}))
            s
    )
)

let release = ReleaseNotes.load "RELEASE_NOTES.md"

Target.create "Meta" (fun _ ->
    [ "<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">"
      "<PropertyGroup>"
      "<Description>Elmish extensions for writing Fable apps with React and ReactNative</Description>"
      sprintf "<PackageProjectUrl>http://%s.github.io/%s</PackageProjectUrl>" gitOwner gitName
      "<PackageLicenseUrl>https://raw.githubusercontent.com/elmish/react/master/LICENSE.md</PackageLicenseUrl>"
      "<PackageIconUrl>https://raw.githubusercontent.com/elmish/elmish/master/docs/files/img/logo.png</PackageIconUrl>"
      sprintf "<RepositoryUrl>%s/%s</RepositoryUrl>" gitHome gitName
      "<PackageTags>fable;elmish;fsharp;React;React-Native</PackageTags>"
      sprintf "<PackageReleaseNotes>%s</PackageReleaseNotes>" (List.head release.Notes)
      "<Authors>Eugene Tolmachev</Authors>"
      sprintf "<Version>%s</Version>" (string release.SemVer)
      "</PropertyGroup>"
      "</Project>"]
    |> File.write false "Directory.Build.props"
)

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target.create "Package" (fun _ ->
    projects
    |> Seq.iter (fun s ->
        let dir = Path.GetDirectoryName s
        DotNet.pack (fun a ->
            a.WithCommon (withWorkDir dir)
        ) s
    )
)

Target.create "PublishNuget" (fun _ ->
    let exec dir =
        DotNet.exec (fun a ->
            a.WithCommon (withWorkDir dir)
        )

    let args = sprintf "push Fable.Elmish.React.%s.nupkg -s nuget.org -k %s" (string release.SemVer) (Environment.environVar "nugetkey")
    let result = exec "src/bin/Release" "nuget" args
    if (not result.OK) then failwithf "%A" result.Errors
)


// --------------------------------------------------------------------------------------
// Generate the documentation
let docs_out = "docs/output"
let docsHome = "https://elmish.github.io/react"

let copyFiles() =
    let header =
        Fake.Core.String.splitStr "\n" """(*** hide ***)
#I "../../src/bin/Release/netstandard2.0"
#r "Fable.Core.dll"
#r "Fable.Elmish.dll"
#r "Fable.Elmish.React.dll"

(**
*)"""

    !!"src/*.fs"
    |> Seq.map (fun fn -> File.read fn |> Seq.append header, fn)
    |> Seq.iter (fun (lines,fn) ->
        let fsx = Path.Combine("docs/content",Path.ChangeExtension(fn |> Path.GetFileName, "fsx"))
        lines |> File.writeNew fsx)

let generateDocs _ =
    copyFiles()
    let info =
      [ "project-name", "elmish-react"
        "project-author", "Eugene Tolmachev"
        "project-summary", "Elmish React extensions"
        "project-github", sprintf "%s/%s" gitHome gitName
        "project-nuget", "http://nuget.org/packages/Fable.Elmish.React" ]

    FSFormatting.createDocs (fun args ->
            { args with
                Source = "docs/content"
                OutputDirectory = docs_out
                LayoutRoots = [ "docs/tools/templates"
                                ".fake/build.fsx/packages/fsharp.formatting/templates" ]
                ProjectParameters  = ("root", docsHome)::info
                Template = "docpage.cshtml" } )

Target.create "GenerateDocs" generateDocs


Target.create "WatchDocs" (fun _ ->
    use watcher =
        (!! "docs/content/**/*.*")
        |> ChangeWatcher.run generateDocs

    Trace.traceImportant "Waiting for help edits. Press any key to stop."

    System.Console.ReadKey() |> ignore

    watcher.Dispose()
)

// --------------------------------------------------------------------------------------
// Release Scripts

Target.create "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    Shell.cleanDir tempDocsDir
    Git.Repository.cloneSingleBranch "" gitRepo "gh-pages" tempDocsDir

    Shell.copyRecursive docs_out tempDocsDir true |> Trace.tracefn "%A"
    Git.Staging.stageAll tempDocsDir
    Git.Commit.exec tempDocsDir (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Git.Branches.push tempDocsDir
)

Target.create "Publish" ignore

// Build order
"Clean"
  ==> "Meta"
  ==> "Restore"
  ==> "Build"
  ==> "Package"
  ==> "GenerateDocs"
  ==> "ReleaseDocs"
  ==> "Publish"


// start build
Target.runOrDefault "Build"
