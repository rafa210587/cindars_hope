$ErrorActionPreference = "Stop"

Write-Host "== Cindar's Hope docs validation =="

$failed = $false

function Fail($message) {
    Write-Host "ERROR: $message" -ForegroundColor Red
    $script:failed = $true
}

function Ok($message) {
    Write-Host "OK: $message" -ForegroundColor Green
}

if (Test-Path "spec") {
    Fail "Root folder 'spec/' must not exist."
} else {
    Ok "Root folder 'spec/' does not exist."
}

if (-not (Test-Path "docs_old")) {
    Fail "docs_old/ must exist."
} else {
    Ok "docs_old/ exists."
}

if (-not (Test-Path "specs")) {
    Fail "specs/ must exist as operational SpecKit folder."
} else {
    Ok "specs/ exists."
}

$badImplementedSpecs = Get-ChildItem "docs/specs/implementados" -Filter "*.md" -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notlike "spec_*" }

if ($badImplementedSpecs) {
    $badImplementedSpecs | ForEach-Object { Fail "Implemented spec without spec_ prefix: $($_.FullName)" }
} else {
    Ok "Implemented specs use spec_ prefix."
}

$badFutureSpecs = Get-ChildItem "docs/specs/a_implementar" -Filter "*.md" -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notlike "spec_*" -and $_.Name -ne "README.md" }

if ($badFutureSpecs) {
    $badFutureSpecs | ForEach-Object { Fail "Future spec without spec_ prefix: $($_.FullName)" }
} else {
    Ok "Future specs use spec_ prefix."
}

$badImplementedRefs = Get-ChildItem "docs/refinements/implementados" -Filter "*.md" -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notlike "ref_*" }

if ($badImplementedRefs) {
    $badImplementedRefs | ForEach-Object { Fail "Implemented refinement without ref_ prefix: $($_.FullName)" }
} else {
    Ok "Implemented refinements use ref_ prefix."
}

$badFutureRefs = Get-ChildItem "docs/refinements/a_implementar" -Filter "*.md" -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notlike "ref_*" }

if ($badFutureRefs) {
    $badFutureRefs | ForEach-Object { Fail "Future refinement without ref_ prefix: $($_.FullName)" }
} else {
    Ok "Future refinements use ref_ prefix."
}

$docFiles = @(
    "AGENTS.md",
    "CLAUDE.md",
    "README.md",
    "PROJECT_LOG.md"
)

$docFiles += Get-ChildItem "docs" -Recurse -Filter "*.md" -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
$docFiles += Get-ChildItem "specs" -Recurse -Filter "*.md" -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }

$placeholderPattern = '\$(source|Source|src|ref|evidence|old|dest)|\$\(\s*docs_old'
$placeholderMatches = Select-String -Path $docFiles -Pattern $placeholderPattern -CaseSensitive -ErrorAction SilentlyContinue

if ($placeholderMatches) {
    $placeholderMatches | ForEach-Object {
        Fail "Placeholder found: $($_.Path):$($_.LineNumber): $($_.Line)"
    }
} else {
    Ok "No template placeholders found."
}

$oldPathScanFiles = $docFiles | Where-Object {
    $_ -notlike "*PROJECT_LOG.md" -and
    $_ -notlike "*DOCS_OLD_TO_ACTIVE_CROSSWALK.md" -and
    $_ -notlike "*DOCS_REORGANIZATION_HANDOFF.md"
}

$oldPathPattern = 'docs/GDD|docs/ARCH(?![A-Za-z])|docs/audits|spec/implementado|spec/preparado'
$oldPathMatches = Select-String -Path $oldPathScanFiles -Pattern $oldPathPattern -ErrorAction SilentlyContinue

if ($oldPathMatches) {
    $oldPathMatches | ForEach-Object {
        Fail "Old path reference found: $($_.Path):$($_.LineNumber): $($_.Line)"
    }
} else {
    Ok "No critical old path references found."
}

$codeChanged = git diff --name-only origin/dev...HEAD | Select-String -Pattern '^(Assets|Packages|ProjectSettings)/' -ErrorAction SilentlyContinue

if ($codeChanged) {
    $codeChanged | ForEach-Object { Fail "Unexpected code/project change: $($_.Line)" }
} else {
    Ok "No code/project changes detected against origin/dev."
}

if ($failed) {
    Write-Host "Docs validation FAILED." -ForegroundColor Red
    exit 1
}

Write-Host "Docs validation PASSED." -ForegroundColor Green
exit 0
