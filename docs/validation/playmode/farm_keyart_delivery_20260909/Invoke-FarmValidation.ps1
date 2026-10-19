param(
    [Parameter(Mandatory=$true)][string]$Label,
    [Parameter(Mandatory=$true)][string]$Method,
    [switch]$AsyncPlayMode,
    [int]$TimeoutSeconds = 300
)
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$unityExe = 'C:/Program Files/Unity/Hub/Editor/6000.5.7f1/Editor/Unity.exe'
$active = @(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'")
if ($active.Count -gt 0) { throw 'Unity is already active; exclusive owner will not compete.' }
if ($Label -notmatch '^[a-zA-Z0-9_-]+$') { throw 'Invalid evidence label.' }
$out = Join-Path $PSScriptRoot $Label
if (Test-Path -LiteralPath $out) { throw 'Evidence folder already exists; use a new round label.' }
New-Item -ItemType Directory -Path $out | Out-Null
$log = Join-Path $out 'unity.log'
$scene = Join-Path $projectRoot 'Assets/_Game/Scenes/FarmScene.unity'
$sceneHashBefore = (Get-FileHash -LiteralPath $scene).Hash
$inputPaths = @(Get-ChildItem (Join-Path $projectRoot 'Assets/_Game/Scripts') -Filter *.cs -Recurse -File | Where-Object { $_.Name -match 'Farm|WorldTilemapGround|WorldSpriteLibrary|RoofRevealController' })
$inputs = @($inputPaths | ForEach-Object { [pscustomobject]@{ path=$_.FullName.Substring($projectRoot.Length+1).Replace('\','/'); sha256=(Get-FileHash $_.FullName).Hash } })
$arguments = @('-batchmode','-projectPath',$projectRoot,'-executeMethod',$Method,'-logFile',$log)
if (-not $AsyncPlayMode) { $arguments += '-quit' }
$quoted = @($arguments | ForEach-Object { '"' + $_ + '"' })
$started = (Get-Date).ToUniversalTime()
$process = Start-Process -FilePath $unityExe -ArgumentList $quoted -PassThru -WindowStyle Hidden
$timedOut = -not $process.WaitForExit($TimeoutSeconds * 1000)
if ($timedOut) { Stop-Process -Id $process.Id -Force; $process.WaitForExit(); $exitCode = -1 } else { $process.Refresh(); $exitCode = $process.ExitCode }
$finished = (Get-Date).ToUniversalTime()
[pscustomobject]@{
    startedUtc=$started.ToString('o'); finishedUtc=$finished.ToString('o'); exitCode=$exitCode; timedOut=$timedOut;
    command=$unityExe + ' ' + ($quoted -join ' '); unityVersion='6000.5.7f1'; mode='SCOPED';
    sceneBeforeSha256=$sceneHashBefore; sceneAfterSha256=(Get-FileHash -LiteralPath $scene).Hash; sourceInputs=$inputs
} | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $out 'process.json') -Encoding utf8
& powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File (Join-Path $projectRoot 'tools/unity/ScanUnityLogs.ps1') -LogFile $log -Context Compile *> (Join-Path $out 'log-scan.txt')
$scanExit = $LASTEXITCODE
[pscustomobject]@{ unityExit=$exitCode; scanExit=$scanExit; timedOut=$timedOut } | ConvertTo-Json | Set-Content (Join-Path $out 'runner-result.json') -Encoding utf8
if ($AsyncPlayMode) {
    $captureRoot = Join-Path $projectRoot 'docs/validation/playmode/farm_gameplay_review'
    Get-ChildItem -LiteralPath $captureRoot -File | Where-Object { $_.Extension -in '.png','.json' } | Copy-Item -Destination $out
} else {
    Get-ChildItem (Join-Path $projectRoot 'docs/validation/playmode') -Filter 'farm_capture*.png' -File | Where-Object { $_.LastWriteTimeUtc -ge $started } | Copy-Item -Destination $out
}
Write-Output "UnityExit=$exitCode ScanExit=$scanExit Evidence=$out"
if ($exitCode -ne 0 -or $scanExit -ne 0) { exit 1 }

