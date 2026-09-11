# Helpers de processo e evidencia compartilhados pelos runners. PowerShell 5.1.
function Resolve-UnityValidationPath {
    param([string]$Path, [string]$BasePath)
    if ([string]::IsNullOrWhiteSpace($Path)) { throw 'Path must not be empty.' }
    $expanded = [Environment]::ExpandEnvironmentVariables($Path)
    if (-not [IO.Path]::IsPathRooted($expanded)) { $expanded = Join-Path $BasePath $expanded }
    return [IO.Path]::GetFullPath($expanded)
}

function Get-UnityValidationEditor {
    param([string]$ProjectPath, [string]$OverridePath)
    $versionFile = Join-Path $ProjectPath 'ProjectSettings/ProjectVersion.txt'
    if (-not (Test-Path -LiteralPath $versionFile -PathType Leaf)) { throw 'ProjectVersion.txt not found.' }
    if ((Get-Content -LiteralPath $versionFile -Raw) -notmatch '(?m)^m_EditorVersion:\s*(\S+)\s*$') { throw 'ProjectVersion.txt has no m_EditorVersion.' }
    $version = $Matches[1]
    $editorPath = $OverridePath
    if ([string]::IsNullOrWhiteSpace($editorPath)) {
        $editorPath = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe"
    }
    $editorPath = Resolve-UnityValidationPath $editorPath $ProjectPath
    if (-not (Test-Path -LiteralPath $editorPath -PathType Leaf)) {
        throw "Unity editor for project version $version not found: $editorPath. Supply the explicit editor path if installed elsewhere."
    }
    Write-Host "Unity project version: $version; editor: $editorPath"
    return $editorPath
}

function Assert-UnityProjectAvailable {
    param([string]$ProjectPath)
    $lockPath = Join-Path $ProjectPath 'Temp/UnityLockfile'
    if (Test-Path -LiteralPath $lockPath -PathType Leaf) {
        $lockStream = $null
        try { $lockStream = [IO.File]::Open($lockPath, 'Open', 'ReadWrite', 'None') }
        catch { throw "Unity project is locked: $ProjectPath. Use the existing editor or close it before batch validation." }
        finally { if ($null -ne $lockStream) { $lockStream.Dispose() } }
    }
    $processes = @(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" -ErrorAction Stop)
    foreach ($running in $processes) {
        if ($running.CommandLine -match '(?i)-projectPath\s+(?:"([^"]+)"|([^\s]+))') {
            $runningPath = $Matches[1]
            if (-not $runningPath) { $runningPath = $Matches[2] }
            if ([IO.Path]::IsPathRooted($runningPath) -and
                [IO.Path]::GetFullPath($runningPath).TrimEnd('\','/') -ieq $ProjectPath.TrimEnd('\','/')) {
                throw "Unity project is already open (PID $($running.ProcessId)): $ProjectPath"
            }
        }
    }
}

function ConvertTo-WindowsProcessArgument {
    param([AllowEmptyString()][string]$Value)
    return '"' + [regex]::Replace([regex]::Replace($Value, '(\\*)"', '$1$1\"'), '(\\+)$', '$1$1') + '"'
}

function Initialize-UnityValidationOutput {
    param([string]$Path, [string]$Extension)
    if ([IO.Path]::GetExtension($Path) -ine $Extension) { throw "Expected $Extension output path: $Path" }
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($Path)) | Out-Null
    if (Test-Path -LiteralPath $Path) { Remove-Item -LiteralPath $Path -Force -ErrorAction Stop }
}

function Invoke-UnityValidationProcess {
    param([string]$EditorPath, [string[]]$Arguments, [ValidateRange(1,86400)][int]$TimeoutSeconds)
    $argumentLine = ($Arguments | ForEach-Object { ConvertTo-WindowsProcessArgument $_ }) -join ' '
    $process = Start-Process -FilePath $EditorPath -ArgumentList $argumentLine -WindowStyle Hidden -PassThru
    try {
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            $process.Kill(); $process.WaitForExit()
            throw "Unity validation timed out after $TimeoutSeconds seconds; stopped only launched PID $($process.Id)."
        }
        $process.Refresh()
        if ($null -eq $process.ExitCode) { throw 'Unity process has no exit code.' }
        return [int]$process.ExitCode
    } finally { $process.Dispose() }
}

function Read-UnityTestResult {
    param([string]$ResultsPath, [string]$TestFilter = '')
    if (-not (Test-Path -LiteralPath $ResultsPath -PathType Leaf)) { throw "Test result not created: $ResultsPath" }
    $readerSettings = New-Object System.Xml.XmlReaderSettings
    $readerSettings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $readerSettings.XmlResolver = $null
    $reader = [System.Xml.XmlReader]::Create($ResultsPath, $readerSettings)
    try { $xml = New-Object System.Xml.XmlDocument; $xml.XmlResolver = $null; $xml.Load($reader) }
    finally { $reader.Dispose() }
    $run = $xml.SelectSingleNode('/test-run')
    if ($null -eq $run) { throw 'Result has no test-run root.' }
    $total = 0; $passed = 0; $failed = 0
    if (-not [int]::TryParse($run.GetAttribute('total'), [ref]$total) -or
        -not [int]::TryParse($run.GetAttribute('passed'), [ref]$passed) -or
        -not [int]::TryParse($run.GetAttribute('failed'), [ref]$failed) -or
        $total -le 0 -or $passed -le 0 -or $failed -ne 0 -or $passed -gt $total -or
        $run.GetAttribute('result') -cne 'Passed') { throw 'Test run did not pass with positive, consistent counts.' }
    $cases = @($run.SelectNodes('.//test-case'))
    $passedCases = @($cases | Where-Object { $_.GetAttribute('result') -ceq 'Passed' })
    if ($cases.Count -ne $total -or $passedCases.Count -ne $passed -or
        @($cases | Where-Object { $_.GetAttribute('result') -notin @('Passed','Skipped') }).Count -gt 0) {
        throw 'Test result case records disagree with aggregate counts/results.'
    }
    if (-not [string]::IsNullOrWhiteSpace($TestFilter)) {
        foreach ($filterPart in ($TestFilter -split ';')) {
            if ([string]::IsNullOrWhiteSpace($filterPart)) { throw 'Empty testFilter component.' }
            $filterRegex = New-Object System.Text.RegularExpressions.Regex($filterPart)
            $matching = @($passedCases | Where-Object {
                $filterRegex.IsMatch($_.GetAttribute('fullname')) -or $filterRegex.IsMatch($_.GetAttribute('name'))
            })
            if ($matching.Count -eq 0) { throw "No executed passing case matched filter: $filterPart" }
        }
    }
    return [pscustomobject]@{ Total = $total; Passed = $passed; Failed = $failed; Result = 'Passed' }
}
