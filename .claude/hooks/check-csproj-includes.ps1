# Verifica os includes locais dos csproj do Unity para os arquivos C# alterados.
# Este repo usa dotnet build como fallback de compile, entao scripts recem-adicionados precisam
# aparecer nos csproj gerados quando o Unity ainda nao os regenerou.

param(
    [switch]$FailOnMissing
)

$ErrorActionPreference = "Stop"

$changed = @()
try {
    $status = git status --porcelain
    if ($status) {
        $changed = $status |
            ForEach-Object { $_.Substring(3) -replace '/', '\' } |
            Where-Object { $_ -match '\.cs$' -and $_ -notmatch '^Packages\\' }
    }
}
catch {
    Write-Warning "Could not inspect git status: $($_.Exception.Message)"
    exit 0
}

if (-not $changed -or $changed.Count -eq 0) {
    Write-Host "[CSPROJ] No changed C# files detected."
    exit 0
}

$projects = @(
    Get-ChildItem -Path . -File -Filter '*.csproj' |
        ForEach-Object {
            [PSCustomObject]@{
                Name = $_.Name
                Text = Get-Content -LiteralPath $_.FullName -Raw
            }
        }
)

if ($projects.Count -eq 0) {
    Write-Warning "[CSPROJ] No generated C# projects found. Open Unity and regenerate project files."
    if ($FailOnMissing) { exit 1 }
    exit 0
}

$missing = @()
foreach ($file in $changed) {
    $slashPath = $file -replace '\\', '/'
    $owners = @(
        $projects | Where-Object {
            $_.Text -like "*$file*" -or $_.Text -like "*$slashPath*"
        }
    )

    if ($owners.Count -eq 0) {
        $missing += "$file -> no generated project"
    }
}

if ($missing.Count -gt 0) {
    Write-Warning "[CSPROJ] Missing Compile Include entries:"
    foreach ($item in $missing) {
        Write-Warning "  $item"
    }

    if ($FailOnMissing) {
        exit 1
    }
}
else {
    Write-Host "[CSPROJ] All changed C# files are present in one of $($projects.Count) generated project(s)."
}

exit 0
