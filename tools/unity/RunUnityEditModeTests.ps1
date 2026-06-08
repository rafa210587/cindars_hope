param([string]$ProjectPath = ".", [string]$UnityPath = "", [string]$ResultsPath = "")
$ProjectPath = (Resolve-Path $ProjectPath).Path
$LogDir = Join-Path $ProjectPath "Logs"
if (-not (Test-Path $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }
if ([string]::IsNullOrEmpty($ResultsPath)) { $ResultsPath = Join-Path $LogDir "unity-editmode-results.xml" }
$LogFile = Join-Path $LogDir "unity-editmode-tests.log"
if ([string]::IsNullOrEmpty($UnityPath)) { $UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" }
Write-Host "Running tests: $ProjectPath"
& $UnityPath -batchmode -projectPath $ProjectPath -runTests -testPlatform EditMode -testResults $ResultsPath -logFile $LogFile -quit
Write-Host "Exit code: $LASTEXITCODE"
if (Test-Path $ResultsPath) { Write-Host "Results: $ResultsPath"; [xml]$xml = Get-Content $ResultsPath; $run = $xml.SelectSingleNode("//test-run"); if ($run) { Write-Host "Total: $($run.GetAttribute('total')), Passed: $($run.GetAttribute('passed')), Failed: $($run.GetAttribute('failed'))" } }
if (Test-Path $LogFile) { Write-Host "Log: $LogFile" }
