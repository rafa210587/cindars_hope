param(
    [string]$UnityEditorPath = "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe",
    [string]$ProjectPath = ".",
    [string]$LogFile = ".\Logs\unity-standalone-build.log",
    [int]$TimeoutSeconds = 3600,
    [string]$ExecuteMethod = "CindarsHope.EditorTools.Build.StandaloneBuildPipeline.BuildBatch",
    [string]$OutputExe = ".\Builds\Windows\CindarsHope.exe",
    [switch]$Smoke,
    [int]$SmokeBootSeconds = 12,
    [string]$CompanyName = "Cindar's Hope",
    [string]$ProductName = "Cindar's Hope"
)

# F61 — Pipeline de build standalone Windows.
# Esqueleto derivado de RunUnityCompileValidation.ps1 (mesmo Editor 6000.4.7f1, Start-Process,
# timeout, log dedicado, exit code honesto). NAO infere sucesso de output filtrado: valida
# exit code do processo + existencia do .exe (artefato ausente = FAIL mesmo com exit 0).
# Builds sao SEQUENCIAIS (pre-bash-guard bloqueia batchmode Unity paralelo).
#
# -Smoke: inicia o .exe, sobrevive ao boot por N segundos, encerra o processo e escaneia o
# Player.log por Exception/Error de boot (mesmo padrao do ScanUnityLogs.ps1).

$ErrorActionPreference = "Stop"

function Resolve-FullPath {
    param([string]$Path)

    $expanded = [Environment]::ExpandEnvironmentVariables($Path)
    if ([System.IO.Path]::IsPathRooted($expanded)) {
        return [System.IO.Path]::GetFullPath($expanded)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $expanded))
}

function Scan-PlayerLogForBootErrors {
    param([string]$PlayerLogPath)

    if (-not (Test-Path -LiteralPath $PlayerLogPath -PathType Leaf)) {
        Write-Host "SMOKE_FAIL"
        Write-Host "Reason: Player.log not found (executable may not have booted)."
        Write-Host "PlayerLog: $PlayerLogPath"
        return $false
    }

    $criticalPatterns = @(
        "Unhandled exception",
        "Exception:",
        "NullReferenceException",
        "MissingReferenceException",
        "Fatal error",
        "Crash!!!",
        "Scene '*' couldn't be loaded",
        "is not added to the build settings",
        "Failed to load",
        "Application will terminate with return code 1"
    )

    $lines = Get-Content -LiteralPath $PlayerLogPath
    $matches = New-Object System.Collections.Generic.List[string]

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        foreach ($pattern in $criticalPatterns) {
            if ($line -like "*$pattern*") {
                $matches.Add(("{0}: {1}" -f ($i + 1), $line.Trim()))
                break
            }
        }
    }

    if ($matches.Count -gt 0) {
        Write-Host "SMOKE_FAIL"
        Write-Host "Boot exceptions/errors found in Player.log:"
        $matches | Select-Object -First 40 | ForEach-Object { Write-Host $_ }
        Write-Host "PlayerLog: $PlayerLogPath"
        return $false
    }

    Write-Host "SMOKE_PASS"
    Write-Host "No boot exceptions/errors in Player.log."
    Write-Host "PlayerLog: $PlayerLogPath"
    return $true
}

$unityPath = Resolve-FullPath $UnityEditorPath
$projectFullPath = Resolve-FullPath $ProjectPath
$logFullPath = Resolve-FullPath $LogFile
$logDirectory = Split-Path -Parent $logFullPath
$outputExeFull = Resolve-FullPath $OutputExe

if (-not (Test-Path -LiteralPath $unityPath -PathType Leaf)) {
    Write-Host "Standalone build NOT RUN."
    Write-Host "Reason: UnityEditorPath not found."
    Write-Host "Path: $UnityEditorPath"
    exit 1
}

if (-not (Test-Path -LiteralPath $projectFullPath -PathType Container)) {
    Write-Host "Standalone build NOT RUN."
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
    "-logFile", $logFullPath,
    "-executeMethod", $ExecuteMethod
)

Write-Host "Standalone build STARTING."
Write-Host "Editor: $unityPath"
Write-Host "Method: $ExecuteMethod"
Write-Host "Log: $LogFile"
Write-Host "TimeoutSeconds: $TimeoutSeconds"

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
        Write-Host "Standalone build FAILED."
        Write-Host "Reason: Unity batchmode timeout; process kill also failed."
        Write-Host "TimeoutSeconds: $TimeoutSeconds"
        Write-Host "Error: $($_.Exception.Message)"
        exit 1
    }

    Write-Host "Standalone build FAILED."
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
    Write-Host "Standalone build FAILED."
    Write-Host "Reason: Unity process returned non-zero exit code (BuildReport != Succeeded or scene/identity error)."
    Write-Host "ExitCode: $exitCode"
    Write-Host "Log: $LogFile"
    exit $exitCode
}

# Validacao de artefato: exit 0 nao basta — o .exe TEM que existir (validation-truth).
if (-not (Test-Path -LiteralPath $outputExeFull -PathType Leaf)) {
    Write-Host "Standalone build FAILED."
    Write-Host "Reason: exit code 0 but build artifact (.exe) is missing."
    Write-Host "Expected: $OutputExe"
    Write-Host "Log: $LogFile"
    exit 1
}

$exeInfo = Get-Item -LiteralPath $outputExeFull
Write-Host "Standalone build SUCCEEDED."
Write-Host "Artifact: $OutputExe"
Write-Host "ArtifactSizeBytes: $($exeInfo.Length)"
Write-Host "Log: $LogFile"
Write-Host "ExitCode: 0"

if (-not $Smoke) {
    exit 0
}

# ---- Smoke check: boot + Player.log scan ----
Write-Host ""
Write-Host "Smoke check STARTING."
Write-Host "BootSeconds: $SmokeBootSeconds"

$playerLog = Resolve-FullPath (Join-Path $env:USERPROFILE ("AppData\LocalLow\{0}\{1}\Player.log" -f $CompanyName, $ProductName))
if (Test-Path -LiteralPath $playerLog -PathType Leaf) {
    Remove-Item -LiteralPath $playerLog -Force
}

$smokeProcess = $null
try {
    $smokeProcess = Start-Process -FilePath $outputExeFull -PassThru
} catch {
    Write-Host "SMOKE_FAIL"
    Write-Host "Reason: failed to start executable."
    Write-Host "Error: $($_.Exception.Message)"
    exit 1
}

$booted = $smokeProcess.WaitForExit($SmokeBootSeconds * 1000)

if (-not $booted) {
    # Processo sobreviveu ao boot (esperado para um jogo) — encerra limpo.
    try {
        $smokeProcess.CloseMainWindow() | Out-Null
        Start-Sleep -Milliseconds 1500
        $smokeProcess.Refresh()
        if (-not $smokeProcess.HasExited) {
            $smokeProcess.Kill()
        }
        $smokeProcess.WaitForExit()
    } catch {
        Write-Host "Smoke note: process termination raised: $($_.Exception.Message)"
    }
    Write-Host "Smoke note: executable survived boot window and was terminated cleanly."
} else {
    # Saiu sozinho durante o boot — so e OK se exit code 0.
    $smokeProcess.Refresh()
    $smokeExit = $smokeProcess.ExitCode
    if ($smokeExit -ne 0) {
        Write-Host "SMOKE_FAIL"
        Write-Host "Reason: executable exited during boot with non-zero code."
        Write-Host "SmokeExitCode: $smokeExit"
        Scan-PlayerLogForBootErrors -PlayerLogPath $playerLog | Out-Null
        exit 1
    }
    Write-Host "Smoke note: executable exited cleanly during boot window (exit 0)."
}

$smokePass = Scan-PlayerLogForBootErrors -PlayerLogPath $playerLog
if (-not $smokePass) {
    exit 1
}

Write-Host "Smoke check SUCCEEDED."
exit 0
