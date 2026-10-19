# Contrato compartilhado dos guards. Dot-source apenas; nao executa ferramenta nem escreve arquivos.
# Entrada invalida lanca erro sem reproduzir payload (pode conter dados sensiveis).

function Get-EditPayloadPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path) -or $Path.IndexOf([char]0) -ge 0) {
        throw 'Edit payload has an invalid file path.'
    }
    $root = [System.IO.Path]::GetFullPath((Get-Location).Path).TrimEnd('\', '/')
    $absolute = if ([System.IO.Path]::IsPathRooted($Path)) {
        [System.IO.Path]::GetFullPath($Path)
    } else {
        [System.IO.Path]::GetFullPath((Join-Path $root $Path))
    }
    $prefix = $root + [System.IO.Path]::DirectorySeparatorChar
    if ($absolute.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $absolute.Substring($prefix.Length).Replace('\', '/')
    }
    return $absolute.Replace('\', '/')
}

function ConvertFrom-EditToolPayload {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$RawJson)

    try { $envelope = ConvertFrom-Json -InputObject $RawJson.TrimStart([char]0xFEFF) -ErrorAction Stop }
    catch { throw 'Edit payload is not valid JSON; guard inspection was not performed.' }
    if ($null -eq $envelope) { throw 'Edit payload is empty; guard inspection was not performed.' }

    $toolName = [string]$envelope.tool_name
    $inputData = $envelope.tool_input
    if ($envelope -is [string]) {
        $toolName = 'apply_patch'
        $inputData = $envelope
    }
    if ($inputData -is [string] -and $inputData.TrimStart().StartsWith('{')) {
        try { $inputData = ConvertFrom-Json -InputObject $inputData -ErrorAction Stop }
        catch { throw 'Edit tool_input contains malformed JSON.' }
    }

    if ($toolName -in @('Write', 'Edit', 'write_file')) {
        if ($null -eq $inputData -or $inputData -is [string]) { throw 'Edit operation requires object input.' }
        $filePath = $inputData.file_path
        if (-not $filePath) { $filePath = $inputData.path }
        $content = if ($toolName -eq 'Edit') { $inputData.new_string } else { $inputData.content }
        if ($null -eq $content -or $content -isnot [string]) { throw 'Edit operation requires string content (empty is allowed).' }
        return [pscustomobject]@{
            Path = Get-EditPayloadPath -Path $filePath
            PreviousPath = $null
            Operation = if ($toolName -eq 'Edit') { 'Update' } else { 'Write' }
            AddedContent = $content
            IsFullWrite = $toolName -ne 'Edit'
        }
    }

    if ($toolName -ne 'apply_patch') { throw 'Unsupported edit tool; guard inspection was not performed.' }
    $patch = $inputData
    if ($null -ne $inputData -and $inputData -isnot [string]) {
        $patch = $inputData.patch
        if ($null -eq $patch) { $patch = $inputData.input }
    }
    if ($patch -isnot [string]) { throw 'apply_patch requires a patch string.' }
    $lines = $patch -split '\r?\n'
    if ($lines.Count -lt 2 -or $lines[0] -ne '*** Begin Patch') { throw 'Patch begin marker is missing.' }
    $operations = [System.Collections.Generic.List[object]]::new()
    $index = 1
    $ended = $false
    while ($index -lt $lines.Count) {
        $line = $lines[$index]
        if ($line -eq '*** End Patch') {
            $ended = $true
            $index++
            break
        }
        if ($line -notmatch '^\*\*\* (Add|Update|Delete) File: (.+)$') { throw 'Malformed patch file header.' }
        $operation = $Matches[1]
        $path = Get-EditPayloadPath -Path $Matches[2]
        $previousPath = $null
        $added = [System.Collections.Generic.List[string]]::new()
        $index++
        if ($operation -eq 'Update' -and $index -lt $lines.Count -and $lines[$index] -match '^\*\*\* Move to: (.+)$') {
            $previousPath = $path
            $path = Get-EditPayloadPath -Path $Matches[1]
            $operation = 'Move'
            $index++
        }
        while ($index -lt $lines.Count -and $lines[$index] -notmatch '^\*\*\* (?:Add|Update|Delete) File: ' -and $lines[$index] -ne '*** End Patch') {
            $line = $lines[$index]
            if ($operation -eq 'Delete') { throw 'Delete patch contains unexpected content.' }
            if ($line.StartsWith('+')) {
                $added.Add($line.Substring(1))
            } elseif ($operation -eq 'Add') {
                throw 'Add patch contains a line without the addition prefix.'
            } elseif ($line -ne '*** End of File' -and -not $line.StartsWith('@@') -and
                -not $line.StartsWith(' ') -and -not $line.StartsWith('-') -and $line -ne '') {
                throw 'Malformed patch hunk.'
            }
            $index++
        }
        $operations.Add([pscustomobject]@{
            Path = $path
            PreviousPath = $previousPath
            Operation = $operation
            AddedContent = [string]::Join("`n", $added)
            IsFullWrite = $operation -eq 'Add'
        })
    }
    if (-not $ended -or $operations.Count -eq 0) { throw 'Patch is incomplete or contains no file operations.' }
    while ($index -lt $lines.Count) {
        if (-not [string]::IsNullOrWhiteSpace($lines[$index])) { throw 'Unexpected content after patch end marker.' }
        $index++
    }
    return $operations.ToArray()
}

function Read-EditToolOperations {
    $reader = $null
    try {
        # O protocolo transmite JSON UTF-8; Console.In depende da code page do host Windows.
        $reader = [System.IO.StreamReader]::new([Console]::OpenStandardInput(), [System.Text.Encoding]::UTF8, $true)
        return ConvertFrom-EditToolPayload -RawJson ($reader.ReadToEnd())
    } catch {
        [Console]::Error.WriteLine('EDIT-TOOL-PAYLOAD: invalid or unsupported edit input; guard inspection could not be completed. Check the tool envelope and patch format. Failure type: ' + $_.Exception.GetType().Name)
        exit 2
    } finally {
        if ($null -ne $reader) { $reader.Dispose() }
    }
}
