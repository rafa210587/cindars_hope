param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
    [switch]$SkipRestore
)
$ErrorActionPreference = 'Stop'
$ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$solution = Get-ChildItem -LiteralPath $ProjectRoot -File -Filter '*.slnx' | Sort-Object Name | Select-Object -First 1
$projects = @()
if ($null -ne $solution) {
    [xml]$solutionXml = Get-Content -LiteralPath $solution.FullName -Raw
    $projects = @($solutionXml.SelectNodes('//Project[@Path]') | ForEach-Object {
        $projectPath = Join-Path $ProjectRoot $_.Path
        if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) { throw "Solution references missing project '$($_.Path)'." }
        Get-Item -LiteralPath $projectPath
    })
} else {
    $projects = @(Get-ChildItem -LiteralPath $ProjectRoot -File -Filter '*.csproj' | Sort-Object Name)
}
if ($projects.Count -eq 0) { Write-Host 'No Unity-generated projects found. Regenerate project files in Unity.'; exit 2 }
Write-Host "Unity C# projects discovered: $($projects.Count)"
$projects | ForEach-Object { Write-Host "  - $($_.Name)" }

# .slnx e suportado pelo SDK 9.0.200+. SDK anterior usa todos os projetos descobertos.
$sdkText = @(& dotnet --version)
$sdkExit = $LASTEXITCODE
$sdkVersion = $null
if ($sdkExit -ne 0 -or $sdkText.Count -eq 0 -or
    -not [version]::TryParse(($sdkText[-1] -split '-')[0].Trim(), [ref]$sdkVersion)) {
    Write-Host 'Unable to identify dotnet SDK.'; exit 2
}
$solutionSupported = $sdkVersion -ge [version]'9.0.200'
if ($null -ne $solution -and $solutionSupported) {
    Write-Host "Build graph: $($solution.Name); SDK: $sdkVersion"
    $buildArguments = @('build',$solution.FullName)
    if ($SkipRestore) { $buildArguments += '--no-restore' }
    & dotnet @buildArguments
    $buildExit = $LASTEXITCODE
    if ($null -eq $buildExit -or $buildExit -ne 0) {
        Write-Host "Unity solution build FAILED: exit $buildExit"; exit 1
    }
} else {
    Write-Host "Per-project fallback: no supported .slnx (SDK $sdkVersion). All $($projects.Count) projects retained."
    if (-not $SkipRestore) {
        foreach ($project in $projects) {
            & dotnet restore $project.FullName
            if ($LASTEXITCODE -ne 0) { exit 1 }
        }
    }
    foreach ($project in $projects) {
        & dotnet build $project.FullName --no-restore
        if ($LASTEXITCODE -ne 0) { exit 1 }
    }
}
Write-Host "Unity generated-project build PASS: $($projects.Count) project(s)."
exit 0
