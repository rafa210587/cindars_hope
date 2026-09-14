# Negative test: a fresh file in the disposable benchmark snapshot must not be writable.
$ErrorActionPreference = 'Stop'
$probe = 'D:\Projetos\Cindars_Hope\.serena-runtime\comparison\3dd585d76c\source\readonly-boundary-' + [guid]::NewGuid().ToString('N') + '.txt'
try {
    [IO.File]::WriteAllText($probe, 'UNEXPECTED: read-only boundary did not hold')
    Write-Output "FAIL: unexpected write at $probe"
    exit 5
} catch {
    if ($_.Exception -is [UnauthorizedAccessException] -or $_.Exception.InnerException -is [UnauthorizedAccessException]) {
        Write-Output 'PASS: write denied by read-only boundary'
        exit 0
    }
    throw
}
