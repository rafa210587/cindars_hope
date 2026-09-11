#requires -Version 7.0
$ErrorActionPreference = 'Stop'
$script = Join-Path $PSScriptRoot 'Get-ClassContext.ps1'
$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
$fixture = Join-Path $tempRoot ('cindars-class-context-' + [guid]::NewGuid().ToString('N'))
$checks = 0
function Assert-Contract([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "CLASS_CONTEXT_TEST_FAILURE: $Message" }
    $script:checks++
}
function Write-Fixture([string]$RelativePath, [string]$Content) {
    $path = Join-Path $fixture $RelativePath
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path)) | Out-Null
    [IO.File]::WriteAllText($path, $Content, [Text.UTF8Encoding]::new($false))
}
try {
    Write-Fixture 'Assets/_Game/Scripts/Runtime.asmdef' '{"name":"Fixture.Runtime"}'
    Write-Fixture 'Fixture.Runtime.csproj' '<Project><PropertyGroup><DefineConstants>UNITY_EDITOR;FEATURE_ON</DefineConstants></PropertyGroup></Project>'
    Write-Fixture 'Assets/_Game/Scripts/Inventory/First.cs' @'
namespace Fixture;
/// <summary>Owns items &amp; their identity.</summary>
/// <remarks>Preserve <see cref="Slot"/> identity.</remarks>
public partial class Inventory<T> : IStore {
    public int Count => 1;
    public void Add(int amount) { }
    public bool HasItem(string id, int amount = 1) => true;
    public string A = "secret initializer", B = "other initializer";
    public sealed class Nested { }
    private string text = "public class NotAType {}";
    // public class AlsoNotAType {}
}
public interface IStore { int Count { get; } bool HasItem(string id, int amount = 1); }
#if FEATURE_ON
public class Enabled { }
#else
public class Disabled { }
#endif
'@
    Write-Fixture 'Assets/_Game/Scripts/Inventory/Second.cs' 'namespace Fixture { public partial class Inventory<T> { } }'
    Write-Fixture 'Assets/_Game/Tests/EditMode/Tests.asmdef' '{"name":"Fixture.Tests"}'
    Write-Fixture 'Assets/_Game/Tests/EditMode/Sample.cs' 'namespace Fixture { public class Inventory<T> { } }'

    $summary = & $script -ProjectRoot $fixture -Summary | ConvertFrom-Json
    Assert-Contract ($summary.declarations -eq 5 -and $summary.uniqueTypes -eq 4) 'Partial declarations and nested types must be counted separately; strings/comments/inactive branches excluded.'
    $parts = @(& $script -ProjectRoot $fixture -Type Inventory -Members | ForEach-Object { $_ | ConvertFrom-Json })
    Assert-Contract ($parts.Count -eq 2 -and $parts[0].symbol -eq 'Fixture.Inventory`1') 'Generic partial symbols must retain both source locations.'
    Assert-Contract ($parts[0].assembly -eq 'Fixture.Runtime' -and $parts[0].line -eq 4) 'Assembly and source line must be exact.'
    Assert-Contract ($parts[0].summary -eq 'Owns items & their identity.' -and $parts[0].remarks -eq 'Preserve Slot identity.') 'XML documentation must preserve referenced types and entities.'
    Assert-Contract ($parts[0].members -contains 'public void Add(int amount)' -and $parts[0].members -contains 'public int Count') 'Opt-in members contain signatures, not bodies.'
    Assert-Contract ($parts[0].members -contains 'public bool HasItem(string id, int amount = 1)' -and $parts[0].members -contains 'public string A, B') 'Defaults in parameters are preserved; only field initializers are removed.'
    $contract = & $script -ProjectRoot $fixture -Type IStore -Members | ConvertFrom-Json
    Assert-Contract ($contract.members -contains 'int Count' -and $contract.members -contains 'bool HasItem(string id, int amount = 1)') 'Interface members are public without an explicit modifier.'
    $nested = & $script -ProjectRoot $fixture -Type Nested | ConvertFrom-Json
    Assert-Contract ($nested.symbol -eq 'Fixture.Inventory`1+Nested') 'Nested names include the declaring generic type.'
    $withTests = & $script -ProjectRoot $fixture -IncludeTests -Summary | ConvertFrom-Json
    Assert-Contract ($withTests.declarations -eq 6 -and $withTests.uniqueTypes -eq 5) 'Tests require IncludeTests; the same symbol in two assemblies is two distinct types.'
    $test = & $script -ProjectRoot $fixture -IncludeTests -Module 'Tests/*' | ConvertFrom-Json
    Assert-Contract ($test.assembly -eq 'Fixture.Tests') 'Nearest asmdef owns the type.'
    $output = Join-Path $fixture 'index.jsonl'
    & $script -ProjectRoot $fixture -OutputPath $output | Out-Null
    $firstHash = (Get-FileHash -LiteralPath $output).Hash
    & $script -ProjectRoot $fixture -OutputPath $output | Out-Null
    Assert-Contract ((Get-FileHash -LiteralPath $output).Hash -eq $firstHash) 'Repeated exports must be byte-identical.'
    & $script -ProjectRoot $fixture -OutputPath 'relative-index.jsonl' | Out-Null
    Assert-Contract (Test-Path -LiteralPath (Join-Path $fixture 'relative-index.jsonl')) 'Relative OutputPath resolves within ProjectRoot, independently of the invoking directory.'
    $limited = @(& $script -ProjectRoot $fixture -Module Inventory -MaxResults 1 -WarningAction SilentlyContinue)
    Assert-Contract ($limited.Count -eq 1) 'Console output must honor MaxResults.'
    $blocked = $false
    try { & $script -ProjectRoot $fixture -OutputPath (Join-Path $fixture 'Assets/index.jsonl') | Out-Null }
    catch { $blocked = $true }
    Assert-Contract $blocked 'Index exports must not write under Assets.'
    Write-Fixture 'Assets/_Game/Scripts/Inventory/Broken.cs' 'public class Broken {'
    $failed = $false
    try { & $script -ProjectRoot $fixture -OutputPath $output -WarningAction SilentlyContinue | Out-Null }
    catch { $failed = $true }
    Assert-Contract ($failed -and (Get-FileHash -LiteralPath $output).Hash -eq $firstHash) 'Syntax errors must fail without replacing a good index.'
    Write-Output "CLASS_CONTEXT_TESTS_PASS: $checks contracts"
}
finally {
    if (Test-Path -LiteralPath $fixture) {
        $resolved = (Resolve-Path -LiteralPath $fixture).Path
        if (-not $resolved.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -or
            [IO.Path]::GetFileName($resolved) -notlike 'cindars-class-context-*') {
            throw "Unsafe fixture cleanup target: $resolved"
        }
        Remove-Item -LiteralPath $resolved -Recurse -Force
    }
}
