#requires -Version 7.0
<#[.SYNOPSIS] Read-only Roslyn reference query for explicit generated csproj files.
.EXAMPLE
pwsh -File tools/architecture/semantic-pilot/Get-SemanticReferences.ps1 -Project CindarsHope.Foundation.csproj -Symbol T:CindarsHope.Foundation.StableHash32
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Project,
    [Parameter(Mandatory)][string]$Symbol,
    [ValidateRange(1,1000)][int]$MaxResults = 20
)
$ErrorActionPreference = 'Stop'
try {
    $roslyn = [Microsoft.CodeAnalysis.SyntaxTree].Assembly.Location
    $csharp = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation].Assembly.Location
    $source = Join-Path $PSScriptRoot 'SemanticPilot.cs'
    $dotnetRoot = [IO.Path]::GetDirectoryName((Get-Command dotnet -ErrorAction Stop).Source)
    $runtimeMajor = [Environment]::Version.Major
    $referencePack = Get-ChildItem (Join-Path $dotnetRoot 'packs/Microsoft.NETCore.App.Ref') -Directory | Where-Object { ([version]$_.Name).Major -eq $runtimeMajor } | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
    if (-not $referencePack) { throw "SDK reference pack for .NET runtime major $runtimeMajor unavailable." }
    $referenceDir = Get-ChildItem (Join-Path $referencePack.FullName 'ref') -Directory | Sort-Object Name -Descending | Select-Object -First 1
    $framework = @(Get-ChildItem $referenceDir.FullName -Filter '*.dll' -File | ForEach-Object FullName)
    Add-Type -Path $source -ReferencedAssemblies ($framework + @($roslyn, $csharp)) -IgnoreWarnings -ErrorAction Stop 3>$null
    $result = [SemanticPilot.Query]::Run([IO.Path]::GetFullPath($Project), $Symbol, $MaxResults, $source, $PSCommandPath)
    $result.Json
    exit $result.ExitCode
}
catch {
    [ordered]@{ status='ERROR'; complete=$false; diagnostics=@($_.Exception.Message); matches=@(); totalMatches=0; omittedMatches=0 } | ConvertTo-Json -Depth 6 -Compress
    exit 1
}
