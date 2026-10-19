param(
    [string]$ProjectPath = '.',
    [string]$UnityPath = '',
    [string]$ResultsPath = '',
    [string]$TestFilter = '',
    [string]$LogFile = '',
    [ValidateRange(1,86400)][int]$TimeoutSeconds = 900
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'UnityValidation.Common.ps1')
try {
    $ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
    $editorPath = Get-UnityValidationEditor $ProjectPath $UnityPath
    Assert-UnityProjectAvailable $ProjectPath
    if (-not $ResultsPath) { $ResultsPath = 'Logs/unity-editmode-results.xml' }
    if (-not $LogFile) { $LogFile = 'Logs/unity-editmode-tests.log' }
    $ResultsPath = Resolve-UnityValidationPath $ResultsPath $ProjectPath
    $LogFile = Resolve-UnityValidationPath $LogFile $ProjectPath
    if ($ResultsPath -ieq $LogFile) { throw 'Test result and log paths must differ.' }
} catch { Write-Host "UNITY_EDITMODE: NOT RUN; $($_.Exception.Message)"; exit 2 }
try {
    Initialize-UnityValidationOutput $ResultsPath '.xml'
    Initialize-UnityValidationOutput $LogFile '.log'
    $arguments = @('-batchmode','-nographics','-projectPath',$ProjectPath,
        '-runTests','-testPlatform','EditMode','-testResults',$ResultsPath,'-logFile',$LogFile)
    if ($TestFilter) { $arguments += @('-testFilter',$TestFilter) }
    $processExit = Invoke-UnityValidationProcess $editorPath $arguments $TimeoutSeconds
    if ($processExit -ne 0) { throw "Unity process exit $processExit. Log: $LogFile" }
    $result = Read-UnityTestResult $ResultsPath $TestFilter
    & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'ScanUnityLogs.ps1') -LogFile $LogFile -Context Tests -ResultsPath $ResultsPath
    if ($LASTEXITCODE -ne 0) { throw 'Unity test log contains compile/crash errors or invalid evidence.' }
    Write-Host "UNITY_EDITMODE: PASS; total=$($result.Total); passed=$($result.Passed); failed=0"
    Write-Host "Results: $ResultsPath; Log: $LogFile; process exit: $processExit"
    exit 0
} catch { Write-Host "UNITY_EDITMODE: FAIL; $($_.Exception.Message)"; exit 1 }
