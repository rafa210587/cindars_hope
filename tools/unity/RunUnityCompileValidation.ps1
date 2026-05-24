param(
    [string]$UnityEditorPath = "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe",
    [string]$ProjectPath = ".",
    [string]$LogFile = ".\Logs\unity-compile-validation.log",
    [int]$TimeoutSeconds = 900
)

$ErrorActionPreference = "Stop"

function Resolve-FullPath {
    param([string]$Path)

    $expanded = [Environment]::ExpandEnvironmentVariables($Path)
    if ([System.IO.Path]::IsPathRooted($expanded)) {
        return [System.IO.Path]::GetFullPath($expanded)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $expanded))
}

$unityPath = Resolve-FullPath $UnityEditorPath
$projectFullPath = Resolve-FullPath $ProjectPath
$logFullPath = Resolve-FullPath $LogFile
$logDirectory = Split-Path -Parent $logFullPath

if (-not (Test-Path -LiteralPath $unityPath -PathType Leaf)) {
    Write-Host "Unity validation NOT RUN."
    Write-Host "Reason: UnityEditorPath not found."
    Write-Host "Path: $UnityEditorPath"
    exit 1
}

if (-not (Test-Path -LiteralPath $projectFullPath -PathType Container)) {
    Write-Host "Unity validation NOT RUN."
    Write-Host "Reason: ProjectPath not found."
    Write-Host "Path: $ProjectPath"
    exit 1
}

New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null

if (Test-Path -LiteralPath $logFullPath) {
    Remove-Item -LiteralPath $logFullPath -Force
}

$arguments = @(
    "-batchmode",
    "-quit",
    "-nographics",
    "-projectPath", $projectFullPath,
    "-logFile", $logFullPath
)

$process = Start-Process `
    -FilePath $unityPath `
    -ArgumentList $arguments `
    -NoNewWindow `
    -PassThru `
    -Wait:$false

$finished = $process.WaitForExit($TimeoutSeconds * 1000)

if (-not $finished) {
    try {
        $process.Kill()
        $process.WaitForExit()
    } catch {
        Write-Host "Unity validation FAILED."
        Write-Host "Reason: Unity batchmode timeout; process kill also failed."
        Write-Host "TimeoutSeconds: $TimeoutSeconds"
        Write-Host "Error: $($_.Exception.Message)"
        exit 1
    }

    Write-Host "Unity validation FAILED."
    Write-Host "Reason: Unity batchmode timeout."
    Write-Host "TimeoutSeconds: $TimeoutSeconds"
    exit 1
}

$process.Refresh()
$exitCode = $process.ExitCode
if ($null -eq $exitCode) {
    $exitCode = 1
}

if ($exitCode -ne 0) {
    Write-Host "Unity validation FAILED."
    Write-Host "Reason: Unity process returned non-zero exit code."
    Write-Host "ExitCode: $exitCode"
    Write-Host "Log: $LogFile"
    exit $exitCode
}

Write-Host "Unity compile validation finished."
Write-Host "Log: $LogFile"
Write-Host "ExitCode: 0"
exit 0
