#!/usr/bin/env -S dotnet fsi
#r "nuget: Fake.Core.Target, 5.23.1"
#r "nuget: Fake.IO.FileSystem, 5.23.1"
#r "nuget: Fake.DotNet.Cli, 5.23.1"
#r "nuget: Fake.Core.Target, 5.23.1"
#r "nuget: Fake.Core.ReleaseNotes, 5.23.1"
#r "nuget: Fake.Tools.Git, 5.23.1"
#r "nuget: MSBuild.StructuredLogger, 2.2.441"

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
      !! "react/**.fsproj"
      ++ "react-native/**.fsproj"

System.Environment.GetCommandLineArgs() 
|> Array.skip 2 // fsi.exe; build.fsx
|> Array.toList
|> Context.FakeExecutionContext.Create false __SOURCE_FILE__
|> Context.RuntimeContext.Fake
|> Context.setExecutionContext

Target.create "Clean" (fun _ ->
    Shell.cleanDir "react/obj"
    Shell.cleanDir "react/bin"
    Shell.cleanDir "react-native/obj"
    Shell.cleanDir "react-native/bin"
)

Target.create "Restore" (fun _ ->
    projects
    |> Seq.iter (Path.GetDirectoryName >> DotNet.restore id)
)

Target.create "Build" (fun _ ->
    projects
     |> Seq.iter (Path.GetDirectoryName >> DotNet.build id)
)

let release = ReleaseNotes.load "RELEASE_NOTES.md"

Target.create "Meta" (fun _ ->
    $"""
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <None Include="$(MSBuildThisFileDirectory)/docs/static/img/logo.png" Pack="true" PackagePath="\" />
    <None Include="$(MSBuildThisFileDirectory)/LICENSE.md" Pack="true" PackagePath="\" />
    <None Include="$(MSBuildThisFileDirectory)/README.md" Pack="true" PackagePath="\"/>
    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="1.0.0" PrivateAssets="All"/>
  </ItemGroup>
  <PropertyGroup>
    <EmbedUntrackedSources>true</EmbedUntrackedSources>
    <AllowedOutputExtensionsInPackageBuildOutputFolder>$(AllowedOutputExtensionsInPackageBuildOutputFolder);.pdb</AllowedOutputExtensionsInPackageBuildOutputFolder>
    <Description>Elmish extensions for writing Fable apps with React</Description>
    <PackageProjectUrl>http://{gitOwner}.github.io/{gitName}</PackageProjectUrl>
    <PackageLicenseFile>LICENSE.md</PackageLicenseFile>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageIcon>logo.png</PackageIcon>
    <RepositoryUrl>{gitHome}/{gitName}</RepositoryUrl>
    <PackageTags>fable;elmish;fsharp;React;React-Native</PackageTags>
    <PackageReleaseNotes>{List.head release.Notes}</PackageReleaseNotes>
    <Authors>Eugene Tolmachev</Authors>
    <Version>{string release.SemVer}</Version>
  </PropertyGroup>
</Project>"""
    |> List.singleton
    |> File.write false "Directory.Build.props"
)

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target.create "Package" (fun _ ->
    projects
    |> Seq.iter (Path.GetDirectoryName >> DotNet.pack id)
)

Target.create "PublishNuget" (fun _ ->
    let exec dir = DotNet.exec (DotNet.Options.withWorkingDirectory dir)
    let key = Environment.environVar "nugetkey"

    let pushPkg dir name =
        let args = sprintf "push %s.%s.nupkg -s nuget.org -k %s" name (string release.SemVer) key
        let result = exec dir "nuget" args
        if (not result.OK) then failwithf "%A" result.Errors

    pushPkg "react/bin/Release" "Fable.Elmish.React"
    pushPkg "react-native/bin/Release" "Fable.Elmish.ReactNative"
)


// --------------------------------------------------------------------------------------
// Generate the documentation
Target.create "GenerateDocs" (fun _ ->
    let res = Shell.Exec("npm", "run docs:build")

    if res <> 0 then
        failwithf "Failed to generate docs"
)

Target.create "WatchDocs" (fun _ ->
    let res = Shell.Exec("npm", "run docs:watch")

    if res <> 0 then
        failwithf "Failed to watch docs: %d" res
)

// --------------------------------------------------------------------------------------
// Release Scripts

Target.create "ReleaseDocs" (fun _ ->
    let res = Shell.Exec("npm", "run docs:publish")

    if res <> 0 then
        failwithf "Failed to publish docs: %d" res
)

Target.create "Test" (fun _ ->
    let res = Shell.Exec("npm", "test")
    if res <> 0 then failwithf "Tests failed: %d" res
)

Target.create "Publish" ignore

// Build order
"Clean"
    ==> "Meta"
    ==> "Restore"
    ==> "Build"
    ==> "Test"
    ==> "Package"
    ==> "PublishNuget"
    ==> "Publish"

// start build
Target.runOrDefault "Build"
