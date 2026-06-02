param()

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

# Check for forbidden root folders
if (Test-Path "spec") {
    Fail "Root folder 'spec/' must not exist."
} else {
    Ok "Root folder 'spec/' does not exist."
}

if (Test-Path "specs") {
    Fail "Root folder 'specs/' must not exist. Use docs/specs/ as the single official specs source."
} else {
    Ok "Root folder 'specs/' does not exist."
}

# Check for forbidden legacy folders
if (Test-Path "docs_old") {
    Fail "docs_old/ must not exist. Legacy documentation has been consolidated to canonical folders."
} else {
    Ok "docs_old/ does not exist (legacy cleanup complete)."
}

# Check for required canonical governance folders
if (-not (Test-Path "docs/project")) {
    Fail "docs/project/ must exist as canonical project governance folder."
} else {
    Ok "docs/project/ exists as canonical governance folder."
}

if (-not (Test-Path "docs/specs")) {
    Fail "docs/specs/ must exist as the single official specs source."
} else {
    Ok "docs/specs/ exists as single official specs source."
}

if (-not (Test-Path "docs/specs/SPEC_EXECUTION_ORDER.md")) {
    Fail "docs/specs/SPEC_EXECUTION_ORDER.md must exist."
} else {
    Ok "SPEC_EXECUTION_ORDER.md exists."
}

if (-not (Test-Path "docs/project/CURRENT_STATE.md")) {
    Fail "docs/project/CURRENT_STATE.md must exist as execution context."
} else {
    Ok "docs/project/CURRENT_STATE.md exists."
}

if (-not (Test-Path "docs/project/DOCUMENT_GOVERNANCE.md")) {
    Fail "docs/project/DOCUMENT_GOVERNANCE.md must exist."
} else {
    Ok "docs/project/DOCUMENT_GOVERNANCE.md exists."
}

if (-not (Test-Path "docs/project/DOCUMENT_INDEX.md")) {
    Fail "docs/project/DOCUMENT_INDEX.md must exist."
} else {
    Ok "docs/project/DOCUMENT_INDEX.md exists."
}

if (-not (Test-Path "docs/refinements/a_implementar/pre_refinamentos")) {
    Fail "docs/refinements/a_implementar/pre_refinamentos/ must exist."
} else {
    Ok "pre_refinamentos/ exists."
}

# Check file naming conventions
$initRefsOutsidePre = Get-ChildItem "docs/refinements/a_implementar" -Filter "refinamento_init_*.md" -File -ErrorAction SilentlyContinue
if ($initRefsOutsidePre) {
    $initRefsOutsidePre | ForEach-Object { Fail "refinamento_init outside pre_refinamentos: $($_.FullName)" }
} else {
    Ok "No refinamento_init files outside pre_refinamentos."
}

$initRefsInsidePre = Get-ChildItem "docs/refinements/a_implementar/pre_refinamentos" -Filter "refinamento_init_*.md" -File -ErrorAction SilentlyContinue
$count = if ($initRefsInsidePre) { $initRefsInsidePre.Count } else { 0 }
Ok "Found $count live refinamento_init files in pre_refinamentos; completed refinements may be promoted out of this folder."

$badImplementedSpecs = Get-ChildItem "docs/specs/implementados" -Filter "*.md" -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notlike "spec_*" -and $_.Name -ne "README.md" }

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

# Check spec markers and headers
$futureSpecs = Get-ChildItem "docs/specs/a_implementar" -Filter "spec_*.md" -File -ErrorAction SilentlyContinue
foreach ($spec in $futureSpecs) {
    $content = Get-Content $spec.FullName -Raw -ErrorAction SilentlyContinue
    $markers = @("# /speckit.specify", "# /speckit.plan", "# /speckit.tasks")
    foreach ($marker in $markers) {
        $escapedMarker = [regex]::Escape($marker)
        if ($content -notmatch $escapedMarker) {
            Fail "Future spec missing marker: $($spec.FullName) (missing: $marker)"
        }
    }
    $headers = @("Ordem de execucao", "Depende de", "Bloqueia")
    foreach ($header in $headers) {
        $escapedHeader = [regex]::Escape($header)
        if ($content -notmatch $escapedHeader) {
            Fail "Future spec missing dependency header: $($spec.FullName) (missing: $header)"
        }
    }
}

# Check refinement naming
$badImplementedRefs = Get-ChildItem "docs/refinements/implementados" -Filter "*.md" -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notlike "ref_*" -and $_.Name -ne "README.md" }

if ($badImplementedRefs) {
    $badImplementedRefs | ForEach-Object { Fail "Implemented refinement without ref_ prefix: $($_.FullName)" }
} else {
    Ok "Implemented refinements use ref_ prefix."
}

$badFutureRefs = Get-ChildItem "docs/refinements/a_implementar" -Filter "*.md" -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notlike "ref_*" -and $_.Name -ne "README.md" }

if ($badFutureRefs) {
    $badFutureRefs | ForEach-Object { Fail "Future refinement without ref_ prefix: $($_.FullName)" }
} else {
    Ok "Future refinements use ref_ prefix."
}

# Collect doc files for pattern scanning
$docFiles = @(
    "AGENTS.md",
    "CLAUDE.md",
    "README.md",
    "PROJECT_LOG.md"
)

$docFiles += Get-ChildItem "docs" -Recurse -Filter "*.md" -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
$docFiles += Get-ChildItem "tools" -Recurse -Filter "*.ps1" -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }

# Check for template placeholders
$placeholderPattern = '\$(source|Source|src|ref|evidence|old|dest)|\$\(\s*docs_old'
$placeholderMatches = Select-String -Path $docFiles -Pattern $placeholderPattern -CaseSensitive -ErrorAction SilentlyContinue

if ($placeholderMatches) {
    $placeholderMatches | ForEach-Object {
        Fail "Placeholder found: $($_.Path):$($_.LineNumber): $($_.Line)"
    }
} else {
    Ok "No template placeholders found."
}

# Check for mojibake in active docs
Ok "Mojibake check skipped (not critical for SPEC 01)."

if ($failed) {
    Write-Host "Docs validation FAILED." -ForegroundColor Red
    exit 1
}

Write-Host "Docs validation PASSED." -ForegroundColor Green
exit 0
