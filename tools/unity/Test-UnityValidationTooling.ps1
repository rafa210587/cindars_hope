param()
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$tempParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\','/')
$fixture = Join-Path $tempParent ('cindars-unity-contract-' + [guid]::NewGuid().ToString('N'))
$project = Join-Path $fixture 'Project with spaces'
$utf8 = New-Object Text.UTF8Encoding($false)
$script:checks = 0
$originalPath = $env:PATH
$originalCase = $env:CINDARS_VALIDATION_CASE
$originalTrace = $env:CINDARS_VALIDATION_TRACE
$originalVersion = $env:CINDARS_DOTNET_VERSION
function Assert-Contract([bool]$Condition, [string]$Name) {
    if (-not $Condition) { throw "Contract failed: $Name" }
    $script:checks++
}
function Write-Fixture([string]$Path, [string]$Content) {
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($Path)) | Out-Null
    [IO.File]::WriteAllText($Path, $Content, $utf8)
}
function Invoke-Fixture([string]$Script, [string[]]$Parameters) {
    $captured = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File (Join-Path $repo $Script) @Parameters 2>&1
    return [pscustomobject]@{ Exit = $LASTEXITCODE; Output = $captured -join "`n" }
}
try {
    Write-Fixture (Join-Path $project 'ProjectSettings/ProjectVersion.txt') "m_EditorVersion: 9000.1.2f3`n"
    $fakeEditor = Join-Path $fixture 'Fake editor/FakeUnity.exe'
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($fakeEditor)) | Out-Null
    # Processo real de fixture; nao referencia assemblies do jogo nem inicia Unity.
    $sourceText = @'
using System;
using System.IO;
using System.Threading;
public static class FakeUnityValidation {
    public static int Main(string[] args) {
        string trace = Environment.GetEnvironmentVariable("CINDARS_VALIDATION_TRACE");
        if (!String.IsNullOrEmpty(trace)) File.AppendAllText(trace, String.Join("\t",args)+"\n");
        string scenario = Environment.GetEnvironmentVariable("CINDARS_VALIDATION_CASE") ?? "pass";
        if (args.Length > 0 && args[0] == "--version") {
            Console.WriteLine(Environment.GetEnvironmentVariable("CINDARS_DOTNET_VERSION") ?? "10.0.100"); return 0;
        }
        if (args.Length > 0 && (args[0] == "build" || args[0] == "restore")) {
            Console.WriteLine("fake graph build"); return scenario == "nonzero" ? 7 : 0;
        }
        if (scenario == "slow") { Thread.Sleep(5000); return 0; }
        string log = null, result = null;
        for(int i=0;i<args.Length-1;i++) {
            if(args[i]=="-logFile") log=args[i+1];
            if(args[i]=="-testResults") result=args[i+1];
        }
        if(log != null) File.WriteAllText(log, scenario == "compile-error" ? "error CS1234: broken" : scenario == "expected-exception" ? "Exception: Test exception" : "Tundra build success");
        if(result != null && scenario != "missing") {
            string xml = "<test-run result='Passed' total='1' passed='1' failed='0'><test-suite><test-case name='Passes' fullname='Project.Tests.Passes' result='Passed'/></test-suite></test-run>";
            if(scenario=="zero") xml="<test-run result='Passed' total='0' passed='0' failed='0'/>";
            if(scenario=="failed") xml="<test-run result='Failed' total='1' passed='0' failed='1'><test-case name='Broken' result='Failed'/></test-run>";
            if(scenario=="inconsistent") xml="<test-run result='Passed' total='2' passed='2' failed='0'><test-case name='Passes' result='Passed'/></test-run>";
            if(scenario=="skipped") xml="<test-run result='Skipped' total='1' passed='0' failed='0'><test-case result='Skipped'/></test-run>";
            if(scenario=="malformed") xml="<broken";
            if(scenario=="dtd") xml="<!DOCTYPE test-run [<!ENTITY x SYSTEM 'file:///not-read'>]><test-run>&x;</test-run>";
            File.WriteAllText(result, xml);
        }
        return scenario=="nonzero" ? 4 : 0;
    }
}
'@
    Add-Type -TypeDefinition $sourceText -OutputAssembly $fakeEditor -OutputType ConsoleApplication
    $env:CINDARS_VALIDATION_TRACE = Join-Path $fixture 'arguments.txt'
    $resultPath = Join-Path $project 'Results with spaces/tests.xml'
    $logPath = Join-Path $project 'Results with spaces/tests.log'
    $parameters = @('-ProjectPath',$project,'-UnityPath',$fakeEditor,'-ResultsPath',$resultPath,'-LogFile',$logPath,'-TestFilter','Project.Tests')
    $env:CINDARS_VALIDATION_CASE = 'pass'
    $run = Invoke-Fixture 'tools/unity/RunUnityEditModeTests.ps1' $parameters
    Assert-Contract ($run.Exit -eq 0 -and $run.Output.Contains('UNITY_EDITMODE: PASS')) ('passing real fixture process: ' + $run.Output)
    $argv = (Get-Content -LiteralPath $env:CINDARS_VALIDATION_TRACE | Select-Object -First 1) -split "`t"
    Assert-Contract ($argv -contains $project -and $argv -contains $resultPath -and $argv -contains $logPath) 'space-containing paths remain one argument'
    foreach ($scenario in @('missing','zero','failed','inconsistent','skipped','malformed','dtd','nonzero','compile-error')) {
        # Um XML bom preexistente nunca pode mascarar a proxima tentativa.
        Write-Fixture $resultPath "<test-run result='Passed' total='1' passed='1' failed='0'><test-case name='Passes' result='Passed'/></test-run>"
        $env:CINDARS_VALIDATION_CASE = $scenario
        $run = Invoke-Fixture 'tools/unity/RunUnityEditModeTests.ps1' $parameters
        Assert-Contract ($run.Exit -eq 1 -and $run.Output.Contains('UNITY_EDITMODE: FAIL')) ("reject $scenario : " + $run.Output)
    }
    $env:CINDARS_VALIDATION_CASE = 'pass'
    $wrongFilter = @('-ProjectPath',$project,'-UnityPath',$fakeEditor,'-ResultsPath',$resultPath,'-LogFile',$logPath,'-TestFilter','Other.Tests')
    $run = Invoke-Fixture 'tools/unity/RunUnityEditModeTests.ps1' $wrongFilter
    Assert-Contract ($run.Exit -eq 1 -and $run.Output.Contains('No executed passing case matched')) 'filter must execute requested cases'
    $env:CINDARS_VALIDATION_CASE = 'expected-exception'
    $run = Invoke-Fixture 'tools/unity/RunUnityEditModeTests.ps1' $parameters
    Assert-Contract ($run.Exit -eq 0) 'Test Framework passing expected exception is not compile failure'
    $scan = Invoke-Fixture 'tools/unity/ScanUnityLogs.ps1' @('-LogFile',$logPath)
    Assert-Contract ($scan.Exit -eq 1) 'compile-only scanner still rejects generic exception'
    $scan = Invoke-Fixture 'tools/unity/ScanUnityLogs.ps1' @('-LogFile',$logPath,'-Context','Tests','-ResultsPath',(Join-Path $fixture 'absent.xml'))
    Assert-Contract ($scan.Exit -eq 1) 'test scanner requires positive result evidence'
    $env:CINDARS_VALIDATION_CASE = 'slow'
    $run = Invoke-Fixture 'tools/unity/RunUnityEditModeTests.ps1' ($parameters + @('-TimeoutSeconds','1'))
    Assert-Contract ($run.Exit -eq 1 -and $run.Output.Contains('timed out')) 'timeout fails and stops own process'
    $lockPath = Join-Path $project 'Temp/UnityLockfile'
    Write-Fixture $lockPath ''
    $lockStream = [IO.File]::Open($lockPath,'Open','ReadWrite','None')
    try {
        $run = Invoke-Fixture 'tools/unity/RunUnityEditModeTests.ps1' $parameters
        Assert-Contract ($run.Exit -eq 2 -and $run.Output.Contains('project is locked')) 'locked project not started'
    } finally { $lockStream.Dispose() }
    $run = Invoke-Fixture 'tools/unity/RunUnityEditModeTests.ps1' @('-ProjectPath',$project)
    Assert-Contract ($run.Exit -eq 2 -and $run.Output.Contains('9000.1.2f3')) 'default editor resolves project version'
    $env:CINDARS_VALIDATION_CASE = 'pass'
    $run = Invoke-Fixture 'tools/unity/RunUnityCompileValidation.ps1' @('-ProjectPath',$project,'-UnityEditorPath',$fakeEditor,'-LogFile',$logPath)
    Assert-Contract ($run.Exit -eq 0 -and $run.Output.Contains('Unity log scan PASSED')) 'compile wrapper includes scan once'
    $env:CINDARS_VALIDATION_CASE = 'expected-exception'
    $run = Invoke-Fixture 'tools/unity/RunUnityCompileValidation.ps1' @('-ProjectPath',$project,'-UnityEditorPath',$fakeEditor,'-LogFile',$logPath)
    Assert-Contract ($run.Exit -eq 1) 'compile wrapper propagates scanner failure'

    # Builder executa fake dotnet via PATH: nao compila o jogo.
    Copy-Item -LiteralPath $fakeEditor -Destination (Join-Path (Split-Path $fakeEditor) 'dotnet.exe')
    $env:PATH = (Split-Path $fakeEditor) + ';' + $originalPath
    $nodes = ''
    for ($index=1; $index -le 7; $index++) { Write-Fixture (Join-Path $project "P$index.csproj") '<Project/>'; $nodes += "<Project Path='P$index.csproj'/>" }
    Write-Fixture (Join-Path $project 'Game.slnx') "<Solution>$nodes</Solution>"
    $env:CINDARS_VALIDATION_CASE = 'pass'; $env:CINDARS_DOTNET_VERSION = '10.0.100'
    Write-Fixture $env:CINDARS_VALIDATION_TRACE ''
    $run = Invoke-Fixture 'tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1' @('-ProjectRoot',$project)
    $trace = @(Get-Content -LiteralPath $env:CINDARS_VALIDATION_TRACE)
    Assert-Contract ($run.Exit -eq 0 -and $trace.Count -eq 2 -and $trace[1].StartsWith("build`t") -and $trace[1].Contains('Game.slnx')) 'one solution build after version probe'
    Assert-Contract ($run.Output.Contains('7 project(s)')) 'all seven projects discovered'
    Write-Fixture $env:CINDARS_VALIDATION_TRACE ''
    $run = Invoke-Fixture 'tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1' @('-ProjectRoot',$project,'-SkipRestore')
    Assert-Contract ((Get-Content -LiteralPath $env:CINDARS_VALIDATION_TRACE -Raw).Contains('--no-restore')) 'skip restore forwarded to graph'
    $env:CINDARS_DOTNET_VERSION = '8.0.400'; Write-Fixture $env:CINDARS_VALIDATION_TRACE ''
    $run = Invoke-Fixture 'tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1' @('-ProjectRoot',$project)
    $trace = @(Get-Content -LiteralPath $env:CINDARS_VALIDATION_TRACE)
    Assert-Contract ($run.Exit -eq 0 -and @($trace | Where-Object { $_.StartsWith("build`t") }).Count -eq 7 -and @($trace | Where-Object { $_.StartsWith("restore`t") }).Count -eq 7) 'old SDK fallback preserves every project'
    $env:CINDARS_DOTNET_VERSION = '10.0.100'; $env:CINDARS_VALIDATION_CASE = 'nonzero'
    $run = Invoke-Fixture 'tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1' @('-ProjectRoot',$project)
    Assert-Contract ($run.Exit -eq 1) 'graph build nonzero fails'
    Write-Output "UNITY_VALIDATION_TOOLING_TESTS: PASS ($script:checks assertions; fake executable only)"
    exit 0
} finally {
    $env:PATH = $originalPath
    $env:CINDARS_VALIDATION_CASE = $originalCase
    $env:CINDARS_VALIDATION_TRACE = $originalTrace
    $env:CINDARS_DOTNET_VERSION = $originalVersion
    $resolved = [IO.Path]::GetFullPath($fixture)
    if (-not $resolved.StartsWith($tempParent + [IO.Path]::DirectorySeparatorChar + 'cindars-unity-contract-', [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup.' }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}
