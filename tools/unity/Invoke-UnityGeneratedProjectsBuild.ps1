param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
    [switch]$SkipRestore
)

$ErrorActionPreference = 'Stop'

$projects = @(
    Get-ChildItem -LiteralPath $ProjectRoot -File -Filter '*.csproj' |
        Sort-Object Name
)

if ($projects.Count -eq 0) {
    Write-Error "No Unity-generated C# projects found at '$ProjectRoot'. Open Unity and regenerate project files."
    exit 2
}

Write-Output "Unity C# projects discovered: $($projects.Count)"
$projects | ForEach-Object { Write-Output "  - $($_.Name)" }

if (-not $SkipRestore) {
    foreach ($project in $projects) {
        Write-Output "Restoring $($project.Name)..."
        & dotnet restore $project.FullName
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Restore failed for $($project.Name) with exit code $LASTEXITCODE."
            exit $LASTEXITCODE
        }
    }
}

foreach ($project in $projects) {
    Write-Output "Building $($project.Name)..."
    & dotnet build $project.FullName --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed for $($project.Name) with exit code $LASTEXITCODE."
        exit $LASTEXITCODE
    }
}

Write-Output "Unity generated-project build PASS: $($projects.Count) project(s)."
exit 0
