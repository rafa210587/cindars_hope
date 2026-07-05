param(
    [string]$ProjectPath = ".",
    [string]$UnityPath = "",
    [string]$ResultsPath = "",
    [string]$TestFilter = "",
    [string]$LogFile = ""
)

$ErrorActionPreference = "Stop"
$ProjectPath = (Resolve-Path $ProjectPath).Path
$LogDir = Join-Path $ProjectPath "Logs"
if (-not (Test-Path $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }
if ([string]::IsNullOrEmpty($ResultsPath)) { $ResultsPath = Join-Path $LogDir "unity-editmode-results.xml" }
if ([string]::IsNullOrEmpty($LogFile)) { $LogFile = Join-Path $LogDir "unity-editmode-tests.log" }
if ([string]::IsNullOrEmpty($UnityPath)) { $UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" }

$ResultsPath = [System.IO.Path]::GetFullPath($ResultsPath)
$LogFile = [System.IO.Path]::GetFullPath($LogFile)
$arguments = @(
    "-batchmode",
    "-nographics",
    "-projectPath", $ProjectPath,
    "-runTests",
    "-testPlatform", "EditMode",
    "-testResults", $ResultsPath,
    "-logFile", $LogFile
)
if (-not [string]::IsNullOrWhiteSpace($TestFilter)) {
    $arguments += @("-testFilter", $TestFilter)
}

Write-Host "Running tests: $ProjectPath"
$process = Start-Process `
    -FilePath $UnityPath `
    -ArgumentList $arguments `
    -NoNewWindow `
    -PassThru `
    -Wait

$process.Refresh()
$exitCode = $process.ExitCode

if (-not (Test-Path -LiteralPath $ResultsPath -PathType Leaf)) {
    Write-Error "Unity exited without creating the EditMode test result: $ResultsPath"
    exit 2
}

[xml]$xml = Get-Content -LiteralPath $ResultsPath
$run = $xml.SelectSingleNode("//test-run")
if ($null -eq $run) {
    Write-Error "Unity test result does not contain a test-run node: $ResultsPath"
    exit 2
}

$failed = [int]$run.GetAttribute("failed")
Write-Host "Exit code: $exitCode"
Write-Host "Results: $ResultsPath"
Write-Host "Total: $($run.GetAttribute('total')), Passed: $($run.GetAttribute('passed')), Failed: $failed"
Write-Host "Log: $LogFile"

if ($failed -gt 0) { exit 1 }
if ($exitCode -ne 0) { exit $exitCode }
exit 0
