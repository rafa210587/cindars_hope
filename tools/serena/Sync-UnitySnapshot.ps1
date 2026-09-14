param(
    [string]$Repository = 'D:\Projetos\Cindars_Hope\cindars_hope',
    [string]$Snapshot = 'D:\Projetos\Cindars_Hope\.serena-runtime\cindars_hope',
    [switch]$CheckOnly
)
$ErrorActionPreference = 'Stop'
$projects = @('CindarsHope.Foundation.csproj','CindarsHope.Gameplay.csproj','CindarsHope.Runtime.csproj')
$repo = (Resolve-Path -LiteralPath $Repository).Path
$snapshotFull = [IO.Path]::GetFullPath($Snapshot)
if ($snapshotFull.StartsWith($repo + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or $snapshotFull -eq $repo) { throw 'Snapshot must be external to repository' }
function Assert-NoReparse([string]$path) {
    $cursor = [IO.Path]::GetFullPath($path)
    while ($cursor) {
        if (Test-Path -LiteralPath $cursor) {
            if ((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Reparse path not supported: $cursor" }
        }
        $cursor = [IO.Path]::GetDirectoryName($cursor)
    }
}
Assert-NoReparse $snapshotFull
if (-not $CheckOnly -and (Test-Path -LiteralPath $snapshotFull)) { throw 'Snapshot already exists; choose a new session path' }
$sourceRows = [Collections.Generic.List[object]]::new()
$projectRows = [Collections.Generic.List[object]]::new()
$dllRows = [Collections.Generic.List[object]]::new()
foreach ($name in $projects) {
    $original = Join-Path $repo $name
    [xml]$doc = Get-Content -LiteralPath $original -Raw
    foreach ($node in $doc.SelectNodes('//Compile[@Include]')) {
        $relative = $node.Include.Replace('\','/')
        if ([IO.Path]::IsPathRooted($relative) -or $relative -match '(^|/)\.\.(/|$)' -or $relative.StartsWith('/') -or $relative.Contains(':')) { throw "Unsafe Compile path: $relative" }
        $src = Join-Path $repo $relative
        $srcFull = [IO.Path]::GetFullPath($src)
        if (-not $srcFull.StartsWith($repo + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw "Compile escapes repository: $relative" }
        if (-not (Test-Path -LiteralPath $src -PathType Leaf)) { throw "Missing Compile input: $relative" }
        Assert-NoReparse $src
        $sourceRows.Add([pscustomobject]@{ Path=$relative; Sha256=(Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash; Project=$name })
    }
    foreach ($hint in $doc.SelectNodes('//Reference/HintPath')) {
        $value = $hint.InnerText
        if (-not [IO.Path]::IsPathRooted($value)) { $value = Join-Path $repo $value }
        if (-not (Test-Path -LiteralPath $value -PathType Leaf)) { throw "Missing DLL: $value" }
        $resolvedDll = [IO.Path]::GetFullPath($value)
        $dllRows.Add([pscustomobject]@{Path=$resolvedDll; Sha256=(Get-FileHash -LiteralPath $resolvedDll -Algorithm SHA256).Hash})
        $hint.InnerText = $resolvedDll
    }
    foreach ($analyzer in $doc.SelectNodes('//Analyzer[@Include]')) {
        $value = $analyzer.Include
        if (-not [IO.Path]::IsPathRooted($value)) { $value = Join-Path $repo $value }
        $value = [IO.Path]::GetFullPath($value)
        if (-not (Test-Path -LiteralPath $value -PathType Leaf)) { throw "Missing analyzer: $value" }
        $dllRows.Add([pscustomobject]@{Path=$value; Sha256=(Get-FileHash -LiteralPath $value -Algorithm SHA256).Hash})
        $analyzer.SetAttribute('Include', $value)
    }
    foreach ($prop in $doc.SelectNodes('//BaseIntermediateOutputPath|//IntermediateOutputPath|//OutputPath')) {
        if ($prop.Name -eq 'BaseIntermediateOutputPath') { $prop.InnerText = 'obj\$(MSBuildProjectName)\' }
        elseif ($prop.Name -eq 'IntermediateOutputPath') { $prop.InnerText = '$(BaseIntermediateOutputPath)' }
        else { $prop.InnerText = 'bin\Debug\' }
    }
    $projectRows.Add([pscustomobject]@{Name=$name; Sha256=(Get-FileHash -LiteralPath $original -Algorithm SHA256).Hash; Xml=$doc})
}
$unique = @($sourceRows | Group-Object Path | ForEach-Object { $_.Group[0] })
$manifest = [pscustomobject]@{ repository=$repo; snapshot=$snapshotFull; projects=@($projectRows | ForEach-Object { [pscustomobject]@{name=$_.Name; sha256=$_.Sha256} }); sources=@($unique | Sort-Object Path); dlls=@($dllRows | Sort-Object Path -Unique) }
$manifestPath = Join-Path $snapshotFull 'source-manifest.json'
if ($CheckOnly) {
    if (-not (Test-Path -LiteralPath $manifestPath)) { throw 'Snapshot manifest missing; restart Serena' }
    $previous = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $manifest.projects = @($projectRows | ForEach-Object {
        $name = $_.Name
        $record = @($previous.projects | Where-Object name -eq $name)[0]
        [pscustomobject]@{name=$name; sha256=$_.Sha256; snapshotSha256=$record.snapshotSha256}
    })
    $expected = $manifest | ConvertTo-Json -Depth 8 -Compress
    $actual = $previous | ConvertTo-Json -Depth 8 -Compress
    if ($expected -cne $actual) { throw 'Source/project drift detected; restart Serena' }
    foreach ($item in $previous.sources) {
        $copy = Join-Path $snapshotFull $item.Path
        if (-not (Test-Path -LiteralPath $copy -PathType Leaf) -or (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash -cne $item.Sha256) { throw "Snapshot source changed: $($item.Path)" }
    }
    foreach ($project in $previous.projects) {
        $copy = Join-Path $snapshotFull $project.name
        if (-not (Test-Path -LiteralPath $copy -PathType Leaf) -or (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash -cne $project.snapshotSha256) { throw "Snapshot project changed: $($project.name)" }
    }
    Write-Output "CURRENT sources=$($unique.Count) projects=$($projects.Count)"
    exit 0
}
New-Item -ItemType Directory -Path $snapshotFull -Force | Out-Null
foreach ($item in $unique) {
    $target = Join-Path $snapshotFull $item.Path
    New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $repo $item.Path) -Destination $target -Force
    if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -cne $item.Sha256) { throw "Copy mismatch: $($item.Path)" }
}
foreach ($project in $projectRows) {
    $project.Xml.Save((Join-Path $snapshotFull $project.Name))
    New-Item -ItemType Directory -Path (Join-Path $snapshotFull ('obj\' + [IO.Path]::GetFileNameWithoutExtension($project.Name))) -Force | Out-Null
}
$manifest.projects = @($projectRows | ForEach-Object { [pscustomobject]@{name=$_.Name; sha256=$_.Sha256; snapshotSha256=(Get-FileHash -LiteralPath (Join-Path $snapshotFull $_.Name) -Algorithm SHA256).Hash} })
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $manifestPath -Encoding utf8
Write-Output "SYNCED sources=$($unique.Count) projects=$($projects.Count) snapshot=$snapshotFull"
