param()
$ErrorActionPreference = 'Stop'
$tempParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\','/')
$fixture = Join-Path $tempParent ('cindars-ratchet-contract-' + [guid]::NewGuid().ToString('N'))
$utf8 = New-Object Text.UTF8Encoding($false)
$script:checks = 0
$fixtureScript = Join-Path $fixture 'tools/architecture/Test-ArchitectureRatchet.ps1'
function Write-Fixture([string]$RelativePath,[string]$Content) {
    $path = Join-Path $fixture $RelativePath
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path)) | Out-Null
    [IO.File]::WriteAllText($path,$Content,$utf8)
}
function Assert-Contract([bool]$Condition,[string]$Name) {
    if (-not $Condition) { throw "Ratchet contract failed: $Name" }
    $script:checks++
}
function Reset-Fixture {
    Write-Fixture 'tools/architecture/architecture-ratchet-rules.tsv' "Debt`tDebtToken`n"
    Write-Fixture 'tools/architecture/architecture-ratchet-baseline.tsv' "Debt`tAssets/_Game/Scripts/Example.cs`t1`n"
    Write-Fixture 'tools/architecture/serialized-guid-baseline.tsv' "fixture`tAssets/_Game/Scenes/Example.unity`t0123456789abcdef0123456789abcdef`n"
    Write-Fixture 'Assets/_Game/Scripts/Example.cs' 'class Example { /* DebtToken */ }'
    Write-Fixture 'Assets/_Game/Tests/ExampleTests.cs' 'class ExampleTests {}'
    Write-Fixture 'Assets/_Game/Scenes/Example.unity' 'fixture asset'
    Write-Fixture 'Assets/_Game/Scenes/Example.unity.meta' 'guid: 0123456789abcdef0123456789abcdef'
}
function Invoke-Fixture {
    $previous = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $captured = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $fixtureScript -ProjectRoot $fixture 2>&1
        return [pscustomobject]@{ Exit=$LASTEXITCODE; Output=$captured -join "`n" }
    } finally { $ErrorActionPreference = $previous }
}
try {
    Reset-Fixture
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Test-ArchitectureRatchet.ps1') -Destination $fixtureScript
    $result = Invoke-Fixture
    Assert-Contract ($result.Exit -eq 0 -and $result.Output.Contains('Architecture ratchet PASS') -and $result.Output.Contains('PredefinedAssemblyName: checked=2')) ('honest complete pass: '+$result.Output)
    Write-Fixture 'Assets/_Game/Scripts/Example.cs' 'DebtToken DebtToken'
    $result = Invoke-Fixture
    Assert-Contract ($result.Exit -ne 0 -and $result.Output.Contains('baseline allows 1')) 'new debt rejected'
    Reset-Fixture
    Write-Fixture 'Assets/_Game/Scenes/Example.unity.meta' 'guid: ffffffffffffffffffffffffffffffff'
    $result = Invoke-Fixture
    Assert-Contract ($result.Exit -ne 0 -and $result.Output.Contains('GUID changed')) 'serialized GUID change rejected'
    foreach ($sourcePath in @('Assets/_Game/Scripts/Example.cs','Assets/_Game/Tests/ExampleTests.cs')) {
        Reset-Fixture
        Write-Fixture $sourcePath ('class Example { string name = "Assembly'+'-CSharp"; }')
        $result = Invoke-Fixture
        Assert-Contract ($result.Exit -ne 0 -and $result.Output.Contains('PredefinedAssemblyName:')) ('predefined assembly rejected in '+$sourcePath)
    }
    foreach ($dataPath in @('tools/architecture/architecture-ratchet-rules.tsv','tools/architecture/architecture-ratchet-baseline.tsv','tools/architecture/serialized-guid-baseline.tsv')) {
        Reset-Fixture
        Write-Fixture $dataPath "# comments only`n"
        $result = Invoke-Fixture
        Assert-Contract ($result.Exit -ne 0 -and $result.Output.Contains('data empty')) ('empty data fails: '+$dataPath)
        Reset-Fixture
        Remove-Item -LiteralPath (Join-Path $fixture $dataPath)
        $result = Invoke-Fixture
        Assert-Contract ($result.Exit -ne 0 -and $result.Output.Contains('data missing')) ('missing data fails: '+$dataPath)
    }
    $invalidCases = @(
        @{Path='architecture-ratchet-rules.tsv';Body="Debt`t[`n"},
        @{Path='architecture-ratchet-rules.tsv';Body="Debt`tDebtToken`nDebt`tOther`n"},
        @{Path='architecture-ratchet-baseline.tsv';Body="Unknown`tAssets/_Game/Scripts/Example.cs`t1`n"},
        @{Path='architecture-ratchet-baseline.tsv';Body="Debt`tAssets/_Game/Scripts/Example.cs`t-1`n"},
        @{Path='architecture-ratchet-baseline.tsv';Body="Debt`t../outside.cs`t1`n"},
        @{Path='architecture-ratchet-baseline.tsv';Body="Debt`tAssets/_Game/Scripts/Example.cs`t1`nDebt`tAssets/_Game/Scripts/Example.cs`t2`n"}
    )
    foreach ($invalid in $invalidCases) {
        Reset-Fixture
        Write-Fixture ('tools/architecture/'+$invalid.Path) $invalid.Body
        $result = Invoke-Fixture
        Assert-Contract ($result.Exit -ne 0 -and -not $result.Output.Contains('Architecture ratchet PASS')) ('invalid data rejected: '+$invalid.Path)
    }
    # Exercise the production heuristic too; DebtToken only verifies ratchet accounting.
    $productionRules = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'architecture-ratchet-rules.tsv'))
    foreach ($call in @(
        'Object.FindFirstObjectByType<Player>()',
        'FindFirstObjectByType<Player>()',
        'FindAnyObjectByType<Player>()',
        'FindObjectsByType<Player>(sortMode)',
        'FindObjectOfType<Player>()',
        'FindObjectsOfType<Player>()',
        'GameObject . Find("Player")',
        "UnityEngine . Object`n . FindAnyObjectByType<Player>()"
    )) {
        Reset-Fixture
        Write-Fixture 'tools/architecture/architecture-ratchet-rules.tsv' $productionRules
        Write-Fixture 'tools/architecture/architecture-ratchet-baseline.tsv' "ForbiddenUnitySearch`tAssets/_Game/Scripts/Example.cs`t0`n"
        Write-Fixture 'Assets/_Game/Scripts/Example.cs' ('class Example { void Check() { '+$call+'; } }')
        $result = Invoke-Fixture
        Assert-Contract ($result.Exit -ne 0 -and $result.Output.Contains('ForbiddenUnitySearch:')) ('production search heuristic rejects '+$call)
    }
    Write-Fixture 'Assets/_Game/Scripts/Example.cs' 'class Example { void FindFirstObjectByTypeHelper() {} }'
    $result = Invoke-Fixture
    Assert-Contract ($result.Exit -eq 0) 'search method name prefix is not an invocation'
    foreach ($comment in @(
        '// FindObjectsByType<Player>()',
        '/* FindFirstObjectByType<Player>() */',
        "/* line one`nFindAnyObjectByType<Player>()`n*/"
    )) {
        Write-Fixture 'Assets/_Game/Scripts/Example.cs' $comment
        $result = Invoke-Fixture
        Assert-Contract ($result.Exit -eq 0) ('comment does not add architecture debt: '+$comment)
    }
    foreach ($prefix in @(
        'var url = "https://example.test";',
        'var text = "escaped\" // still string";',
        'var text = @"doubled"" // still string";',
        'var text = $@"doubled"" // still string";',
        'var text = $"{"//"}";',
        'var text = @$"{"//"}";',
        'var text = """literal " // " content""";',
        "var text = @`"line one`n// still string`n/* still string */`";",
        'var quote = ''\''''; var slash = ''/'';',
        '/* FindAnyObjectByType<Player>() */',
        "// FindAnyObjectByType<Player>()`n"
    )) {
        Write-Fixture 'Assets/_Game/Scripts/Example.cs' ($prefix+' FindAnyObjectByType<Player>();')
        $result = Invoke-Fixture
        Assert-Contract ($result.Exit -ne 0 -and $result.Output.Contains('ForbiddenUnitySearch:')) ('real search after literal/comment remains visible: '+$prefix)
    }
    Write-Output "ARCHITECTURE_RATCHET_CONTRACTS: PASS ($script:checks assertions; immutable real baselines)"
    exit 0
} finally {
    $resolved = [IO.Path]::GetFullPath($fixture)
    if (-not $resolved.StartsWith($tempParent+[IO.Path]::DirectorySeparatorChar+'cindars-ratchet-contract-',[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe fixture cleanup.' }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}
