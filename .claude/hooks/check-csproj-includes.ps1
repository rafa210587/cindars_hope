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

$runtimeProject = "Assembly-CSharp.csproj"
$editorProject = "Assembly-CSharp-Editor.csproj"
$runtimeText = if (Test-Path $runtimeProject) { Get-Content $runtimeProject -Raw } else { "" }
$editorText = if (Test-Path $editorProject) { Get-Content $editorProject -Raw } else { "" }

$missing = @()
foreach ($file in $changed) {
    $isEditor = $file -match '\\Editor\\'
    $projectText = if ($isEditor) { $editorText } else { $runtimeText }
    $projectName = if ($isEditor) { $editorProject } else { $runtimeProject }

    if ($projectText -notlike "*$file*") {
        $missing += "$file -> $projectName"
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
    Write-Host "[CSPROJ] All changed C# files are present in local csproj files."
}

exit 0
