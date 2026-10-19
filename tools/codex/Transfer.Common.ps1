# Shared primitives for offline workstation transfer. Requires PowerShell 7 on Windows.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-TransferPath([string]$Path) {
    $full = [IO.Path]::GetFullPath($Path)
    if ($full -eq [IO.Path]::GetPathRoot($full)) { return $full }
    $full.TrimEnd([char[]]'\/')
}

function Assert-TransferOutside([string]$Child, [string]$Parent) {
    $childPath = Get-TransferPath $Child
    $parentPath = Get-TransferPath $Parent
    if ($childPath.Equals($parentPath, 'OrdinalIgnoreCase') -or
        $childPath.StartsWith($parentPath.TrimEnd([char[]]'\/') + [IO.Path]::DirectorySeparatorChar, 'OrdinalIgnoreCase')) {
        throw "Destino deve ficar fora da origem: $childPath"
    }
}

function Assert-TransferAppsClosed([switch]$ProjectOnly) {
    $names = if ($ProjectOnly) { @('Unity') } else { @('Unity', 'Codex', 'codex', 'codex-app-server') }
    $running = @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -in $names })
    if ($running.Count) { throw "Feche Unity/Codex e execute em PowerShell externo. Processos: $($running.ProcessName -join ', ')" }
}

function Copy-TransferTree([string]$Source, [string]$Destination, [string[]]$Exclude = @()) {
    if (-not (Test-Path -LiteralPath $Source -PathType Container)) { throw "Pasta ausente: $Source" }
    # Never follow junctions/symlinks into other workspaces. Report them in the guide.
    $copyArgs = @($Source, $Destination, '/E', '/COPY:DAT', '/DCOPY:DAT', '/R:1', '/W:1', '/XJ', '/SL', '/NFL', '/NDL', '/NJH', '/NJS', '/NP')
    if ($Exclude.Count) { $copyArgs += '/XD'; $copyArgs += $Exclude }
    & robocopy @copyArgs | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Robocopy falhou ($LASTEXITCODE): $Source -> $Destination" }
}

function Get-TransferFiles([string]$Root) {
    # Reject links rather than hashing/reading content outside the copied package.
    $items = @(Get-ChildItem -LiteralPath $Root -Force -Recurse)
    $links = @($items | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint })
    if ($links.Count) { throw "Links exigem copia manual do alvo; primeiro encontrado: $($links[0].FullName)" }
    @($items | Where-Object { -not $_.PSIsContainer })
}

function Write-TransferHashes([string]$Root) {
    $rows = @(foreach ($file in Get-TransferFiles $Root) {
        if ($file.FullName -eq (Join-Path $Root 'SHA256.json')) { continue }
        [ordered]@{ Path = [IO.Path]::GetRelativePath($Root, $file.FullName); Length = $file.Length
            SHA256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash }
    })
    ConvertTo-Json -InputObject $rows -Depth 4 | Set-Content -LiteralPath (Join-Path $Root 'SHA256.json') -Encoding utf8
}

function Test-TransferHashes([string]$Root) {
    $hashPath = Join-Path $Root 'SHA256.json'
    $rows = @(Get-Content -LiteralPath $hashPath -Raw | ConvertFrom-Json)
    if (-not $rows.Count) { throw 'Manifesto de hashes vazio.' }
    $files = @(Get-TransferFiles $Root | Where-Object FullName -NE $hashPath)
    if ($files.Count -ne $rows.Count) { throw 'Quantidade de arquivos diverge do manifesto.' }
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($row in $rows) {
        $path = Get-TransferPath (Join-Path $Root $row.Path)
        if (-not $path.StartsWith((Get-TransferPath $Root) + [IO.Path]::DirectorySeparatorChar, 'OrdinalIgnoreCase')) { throw 'Path fora do pacote.' }
        if (-not $seen.Add($path)) { throw "Path duplicado: $path" }
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Arquivo ausente: $($row.Path)" }
        if ((Get-Item -LiteralPath $path -Force).Length -ne $row.Length -or
            (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $row.SHA256) { throw "Hash diverge: $($row.Path)" }
    }
    Write-Host "Integridade: $($rows.Count) arquivos PASS."
}
