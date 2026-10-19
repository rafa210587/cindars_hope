# Manifesto explicito da tarefa; nao infere autoria a partir de todo o dirty state.
function Read-ValidationScope {
    param([string]$ScopePath, [string]$ProjectRoot = (Get-Location).Path)
    $ProjectRoot = [IO.Path]::GetFullPath($ProjectRoot).TrimEnd('\','/')
    $manifest = Get-Content -LiteralPath $ScopePath -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
    $normalized = @{}
    foreach ($field in @('changedFiles','allowedPaths','reportPaths')) {
        if ($field -eq 'reportPaths' -and $manifest.PSObject.Properties.Name -notcontains $field) {
            $normalized[$field] = @()
            continue
        }
        if ($manifest.$field -isnot [array] -or ($field -ne 'reportPaths' -and $manifest.$field.Count -eq 0)) {
            throw "Scope requires an array of paths (non-empty for changedFiles/allowedPaths): $field"
        }
        $entries = @()
        foreach ($entry in @($manifest.$field)) {
            if ($entry -isnot [string] -or [string]::IsNullOrWhiteSpace($entry) -or
                $entry -match '[*?]' -or [IO.Path]::IsPathRooted($entry)) { throw "Scope paths must be exact repository-relative paths: $field" }
            $full = [IO.Path]::GetFullPath((Join-Path $ProjectRoot $entry))
            if (-not $full.StartsWith($ProjectRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
                throw "Scope path escapes project: $entry"
            }
            $entries += $full.Substring($ProjectRoot.Length + 1).Replace('\','/')
        }
        $normalized[$field] = @($entries | Select-Object -Unique)
    }
    foreach ($entry in @($normalized.changedFiles) + @($normalized.reportPaths)) {
        if ($entry -notin $normalized.allowedPaths) { throw "Scope path is outside authorized paths: $entry" }
    }
    foreach ($reportPath in $normalized.reportPaths) {
        if (-not (Test-Path -LiteralPath (Join-Path $ProjectRoot $reportPath) -PathType Leaf)) {
            throw "Scope report does not exist as a file: $reportPath"
        }
    }
    return [pscustomobject]$normalized
}
