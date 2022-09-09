#r "paket:
storage: packages
nuget FSharp.Core 4.7
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.Core.Target
nuget Fake.Core.ReleaseNotes
nuget Fake.Tools.Git //"
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

Target.create "Clean" (fun _ ->
    Shell.cleanDir "src/obj"
    Shell.cleanDir "src/bin"
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
    <Description>Elmish extensions for writing Fable apps with React and ReactNative</Description>
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

    let args = sprintf "push Fable.Elmish.React.%s.nupkg -s nuget.org -k %s" (string release.SemVer) (Environment.environVar "nugetkey")
    let result = exec "src/bin/Release" "nuget" args
    if (not result.OK) then failwithf "%A" result.Errors
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

Target.create "Publish" ignore

// Build order
"Clean"
    ==> "Meta"
    ==> "Restore"
    ==> "Build"
    ==> "Package"
    ==> "PublishNuget"
    ==> "Publish"

// start build
Target.runOrDefault "Build"
