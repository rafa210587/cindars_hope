param(
    [string]$UnityEditorPath = '',
    [string]$ProjectPath = '.',
    [string]$LogFile = 'Logs/unity-compile-validation.log',
    [ValidateRange(1,86400)][int]$TimeoutSeconds = 900
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'UnityValidation.Common.ps1')
try {
    $ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
    $editorPath = Get-UnityValidationEditor $ProjectPath $UnityEditorPath
    Assert-UnityProjectAvailable $ProjectPath
    $LogFile = Resolve-UnityValidationPath $LogFile $ProjectPath
} catch { Write-Host "UNITY_COMPILE: NOT RUN; $($_.Exception.Message)"; exit 2 }
try {
    Initialize-UnityValidationOutput $LogFile '.log'
    $processExit = Invoke-UnityValidationProcess $editorPath @('-batchmode','-quit','-nographics','-projectPath',$ProjectPath,'-logFile',$LogFile) $TimeoutSeconds
    if ($processExit -ne 0) { throw "Unity process exit $processExit. Log: $LogFile" }
    # Compile e scan compartilham a mesma evidencia; callers nao repetem o scanner.
    & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'ScanUnityLogs.ps1') -LogFile $LogFile
    if ($LASTEXITCODE -ne 0) { throw 'Unity compile log scan failed.' }
    Write-Host "UNITY_COMPILE: PASS; process exit: 0; Log: $LogFile"
    exit 0
} catch { Write-Host "UNITY_COMPILE: FAIL; $($_.Exception.Message)"; exit 1 }
