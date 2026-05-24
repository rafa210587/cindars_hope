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

if (Test-Path "specs") {
    Fail "Root folder 'specs/' must not exist. Use docs/specs/ as the single official specs source."
} else {
    Ok "Root folder 'specs/' does not exist."
}

if (-not (Test-Path "docs_old")) {
    Fail "docs_old/ must exist."
} else {
    Ok "docs_old/ exists."
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

if (-not (Test-Path "docs/refinements/a_implementar/pre_refinamentos")) {
    Fail "docs/refinements/a_implementar/pre_refinamentos/ must exist."
} else {
    Ok "pre_refinamentos/ exists."
}

$initRefsOutsidePre = Get-ChildItem "docs/refinements/a_implementar" -Filter "refinamento_init_*.md" -File -ErrorAction SilentlyContinue
if ($initRefsOutsidePre) {
    $initRefsOutsidePre | ForEach-Object { Fail "refinamento_init outside pre_refinamentos: $($_.FullName)" }
} else {
    Ok "No refinamento_init files outside pre_refinamentos."
}

$initRefsInsidePre = Get-ChildItem "docs/refinements/a_implementar/pre_refinamentos" -Filter "refinamento_init_*.md" -File -ErrorAction SilentlyContinue
if (-not $initRefsInsidePre -or $initRefsInsidePre.Count -ne 18) {
    $count = if ($initRefsInsidePre) { $initRefsInsidePre.Count } else { 0 }
    Fail "Expected 18 refinamento_init files in pre_refinamentos; found $count."
} else {
    Ok "Found 18 refinamento_init files in pre_refinamentos."
}

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

$futureSpecs = Get-ChildItem "docs/specs/a_implementar" -Filter "spec_*.md" -File -ErrorAction SilentlyContinue
foreach ($spec in $futureSpecs) {
    $content = Get-Content $spec.FullName -Raw -ErrorAction SilentlyContinue
    foreach ($marker in @("# /speckit.specify", "# /speckit.plan", "# /speckit.tasks")) {
        if ($content -notmatch [regex]::Escape($marker)) {
            Fail "Future spec missing $marker: $($spec.FullName)"
        }
    }
    foreach ($header in @("Ordem de execucao", "Depende de", "Bloqueia")) {
        if ($content -notmatch [regex]::Escape($header)) {
            Fail "Future spec missing dependency header '$header': $($spec.FullName)"
        }
    }
}

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

$docFiles = @(
    "AGENTS.md",
    "CLAUDE.md",
    "README.md",
    "PROJECT_LOG.md"
)

$docFiles += Get-ChildItem "docs" -Recurse -Filter "*.md" -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
$docFiles += Get-ChildItem "tools" -Recurse -Filter "*.ps1" -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }

$placeholderPattern = '\$(source|Source|src|ref|evidence|old|dest)|\$\(\s*docs_old'
$placeholderMatches = Select-String -Path $docFiles -Pattern $placeholderPattern -CaseSensitive -ErrorAction SilentlyContinue

if ($placeholderMatches) {
    $placeholderMatches | ForEach-Object {
        Fail "Placeholder found: $($_.Path):$($_.LineNumber): $($_.Line)"
    }
} else {
    Ok "No template placeholders found."
}

$mojibakePattern = 'Ã|Â|â€™|â€œ|â€|â€“|â€”|Ãƒ|Ã¢'
$mojibakeMatches = Select-String -Path @("AGENTS.md", "CLAUDE.md", "README.md", "docs/README.md", "docs/specs/SPEC_SOURCE_OF_TRUTH.md") -Pattern $mojibakePattern -ErrorAction SilentlyContinue
if ($mojibakeMatches) {
    $mojibakeMatches | ForEach-Object {
        Fail "Mojibake found in active operational doc: $($_.Path):$($_.LineNumber): $($_.Line)"
    }
} else {
    Ok "No mojibake found in active operational docs."
}

$activeInstructionFiles = @(
    "AGENTS.md",
    "CLAUDE.md",
    "README.md",
    "docs/README.md",
    "docs/operations/AGENT_EXECUTION_PROTOCOL.md",
    "docs/specs/SPEC_SOURCE_OF_TRUTH.md",
    "docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md"
)

$rootSpecsReferenceMatches = Select-String -Path $activeInstructionFiles -Pattern 'specs/<|`specs/|\bspecs/' -ErrorAction SilentlyContinue |
    Where-Object { $_.Line -notmatch 'docs/specs' }

if ($rootSpecsReferenceMatches) {
    $rootSpecsReferenceMatches | ForEach-Object {
        Fail "Active instruction still references root specs/: $($_.Path):$($_.LineNumber): $($_.Line)"
    }
} else {
    Ok "No active instruction references root specs/."
}

$oldPathScanFiles = $docFiles | Where-Object {
    $_ -notlike "*PROJECT_LOG.md" -and
    $_ -notlike "*DOCS_OLD_TO_ACTIVE_CROSSWALK.md" -and
    $_ -notlike "*DOCS_REORGANIZATION_HANDOFF.md" -and
    $_ -notlike "*SPEC_MIGRATION_AUDIT.md" -and
    $_ -notlike "*REFINEMENT_MIGRATION_AUDIT.md"
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
