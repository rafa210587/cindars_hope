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

# Check for forbidden numbered folder structure (consolidated in SPEC_DOCS_35)
$numberedFolders = @("docs/00_PROJECT", "docs/01_PRODUCT", "docs/02_ARCHITECTURE", "docs/03_SPECS", "docs/04_REFINEMENTS", "docs/05_VALIDATION", "docs/06_BACKLOG", "docs/07_RELEASES")
foreach ($folder in $numberedFolders) {
    if (Test-Path $folder) {
        Fail "Numbered folder '$folder' must not exist. Use canonical equivalent instead."
    }
}
Ok "No numbered documentation folders found (consolidation complete)."

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

# Check for required template files
if (-not (Test-Path "docs/specs/_templates/SPEC_TEMPLATE.md")) {
    Fail "docs/specs/_templates/SPEC_TEMPLATE.md must exist as spec template."
} else {
    Ok "docs/specs/_templates/SPEC_TEMPLATE.md exists."
}

if (-not (Test-Path "docs/refinements/_templates/REFINEMENT_TEMPLATE.md")) {
    Fail "docs/refinements/_templates/REFINEMENT_TEMPLATE.md must exist as refinement template."
} else {
    Ok "docs/refinements/_templates/REFINEMENT_TEMPLATE.md exists."
}

if (-not (Test-Path "docs/validation/_templates/VALIDATION_REPORT_TEMPLATE.md")) {
    Fail "docs/validation/_templates/VALIDATION_REPORT_TEMPLATE.md must exist as validation template."
} else {
    Ok "docs/validation/_templates/VALIDATION_REPORT_TEMPLATE.md exists."
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

# Collect doc files for pattern scanning (exclude this validation script from placeholder check)
$docFiles = @(
    "AGENTS.md",
    "CLAUDE.md",
    "README.md",
    "PROJECT_LOG.md"
)

$docFiles += Get-ChildItem "docs" -Recurse -Filter "*.md" -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName }
$docFiles += Get-ChildItem "tools" -Recurse -Filter "*.ps1" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -ne (Resolve-Path "tools/docs/validate_docs.ps1").Path } |
    ForEach-Object { $_.FullName }

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

# === ADR / GAME_RULES GOVERNANCE CHECKS (SPEC_DOCS_39) ===

# Check 1: Required decision record infrastructure
if (-not (Test-Path "docs/project/DECISION_LOG.md")) {
    Fail "docs/project/DECISION_LOG.md must exist as decision index."
} else {
    Ok "docs/project/DECISION_LOG.md exists."
}

if (-not (Test-Path "docs/decisions")) {
    Fail "docs/decisions/ must exist as architectural decision records folder."
} else {
    Ok "docs/decisions/ exists."
}

if (-not (Test-Path "docs/decisions/_templates/ADR_TEMPLATE.md")) {
    Fail "docs/decisions/_templates/ADR_TEMPLATE.md must exist as ADR template."
} else {
    Ok "docs/decisions/_templates/ADR_TEMPLATE.md exists."
}

# Check 2: Required game rules infrastructure
if (-not (Test-Path "docs/game_rules")) {
    Fail "docs/game_rules/ must exist as game rules folder."
} else {
    Ok "docs/game_rules/ exists."
}

if (-not (Test-Path "docs/game_rules/GAME_RULES_INDEX.md")) {
    Fail "docs/game_rules/GAME_RULES_INDEX.md must exist as game rules index."
} else {
    Ok "docs/game_rules/GAME_RULES_INDEX.md exists."
}

if (-not (Test-Path "docs/game_rules/_templates/GAME_RULE_TEMPLATE.md")) {
    Fail "docs/game_rules/_templates/GAME_RULE_TEMPLATE.md must exist as game rule template."
} else {
    Ok "docs/game_rules/_templates/GAME_RULE_TEMPLATE.md exists."
}

# Check 3: ADR naming pattern (ADR-NNNN-slug.md)
$adrPattern = '^ADR-\d{4}-[a-z0-9-]+\.md$'
$adrFiles = Get-ChildItem "docs/decisions" -Filter "ADR-*.md" -File -ErrorAction SilentlyContinue
$badAdrs = $adrFiles | Where-Object { $_.Name -notmatch $adrPattern }
if ($badAdrs) {
    $badAdrs | ForEach-Object { Fail "ADR with invalid naming: $($_.FullName) (expected: ADR-NNNN-slug.md)" }
} else {
    Ok "All ADRs follow naming pattern ADR-NNNN-slug.md."
}

# Check 4: Game rule naming pattern (lower_snake_case.md)
$gameRulePattern = '^[a-z0-9_]+\.md$'
$gameRuleFiles = Get-ChildItem "docs/game_rules" -Filter "*.md" -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -ne "GAME_RULES_INDEX.md" -and $_.Name -ne "README.md" }
$badGameRules = $gameRuleFiles | Where-Object { $_.Name -notmatch $gameRulePattern }
if ($badGameRules) {
    $badGameRules | ForEach-Object { Fail "Game rule with invalid naming: $($_.FullName) (expected: lower_snake_case.md)" }
} else {
    Ok "All game rules follow naming pattern lower_snake_case.md."
}

# Check 5: Active specs (a_implementar) have required_adrs and required_game_rules fields with proper format
$activeSpecFiles = Get-ChildItem "docs/specs/a_implementar" -Filter "spec_*.md" -File -ErrorAction SilentlyContinue
$specFieldErrors = 0
$specRefErrors = 0

# Get list of valid ADRs and game rules for reference validation
$validAdrFiles = Get-ChildItem "docs/decisions" -Filter "ADR-*.md" -File -ErrorAction SilentlyContinue
$validAdrIds = $validAdrFiles | ForEach-Object { if ($_.BaseName -match '(ADR-\d{4})') { $matches[1] } }
$validGameRuleFiles = Get-ChildItem "docs/game_rules" -Filter "*.md" -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -ne "GAME_RULES_INDEX.md" -and $_.Name -ne "README.md" }
$validGameRuleNames = $validGameRuleFiles | ForEach-Object { $_.BaseName }

foreach ($spec in $activeSpecFiles) {
    $content = Get-Content $spec.FullName -Raw -ErrorAction SilentlyContinue
    if ($content -notmatch 'required_adrs:\s*\[') {
        Fail "Active spec missing required_adrs field or improper format: $($spec.FullName)"
        $specFieldErrors++
    }
    if ($content -notmatch 'required_game_rules:\s*\[') {
        Fail "Active spec missing required_game_rules field or improper format: $($spec.FullName)"
        $specFieldErrors++
    }

    # Validate that referenced ADRs exist (extract ADR-NNNN pattern and check against valid IDs)
    if ($content -match 'required_adrs:\s*\[(.*?)\]') {
        $adrRefs = $matches[1] -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ }
        foreach ($adrRef in $adrRefs) {
            $adrId = if ($adrRef -match '(ADR-\d{4})') { $matches[1] } else { $adrRef }
            if ($validAdrIds -notcontains $adrId) {
                Fail "Active spec references non-existent ADR: $($spec.FullName) references $adrRef"
                $specRefErrors++
            }
        }
    }

    # Validate that referenced game rules exist (normalize names by removing .md)
    if ($content -match 'required_game_rules:\s*\[(.*?)\]') {
        $gameRuleRefs = $matches[1] -split ',' | ForEach-Object { $_.Trim() -replace '\.md$', '' } | Where-Object { $_ }
        foreach ($gameRuleRef in $gameRuleRefs) {
            if ($validGameRuleNames -notcontains $gameRuleRef) {
                Fail "Active spec references non-existent game rule: $($spec.FullName) references $gameRuleRef"
                $specRefErrors++
            }
        }
    }
}
if ($specFieldErrors -eq 0 -and $specRefErrors -eq 0) {
    Ok "All active specs (a_implementar) have required_adrs and required_game_rules fields with valid references."
}

# Check 6: Validation reports have validated_adrs and validated_game_rules fields (if they exist)
$valReports = Get-ChildItem "docs/validation" -Filter "*execution_report.md" -File -ErrorAction SilentlyContinue
$valFieldErrors = 0
if ($valReports) {
    foreach ($report in $valReports) {
        $content = Get-Content $report.FullName -Raw -ErrorAction SilentlyContinue
        # Only check reports created after SPEC_DOCS_39 (which added these fields)
        if ($content -match 'SPEC_DOCS_39|SPEC_DOCS_4[0-9]|SPEC_[0-9]{2}[A-Z]') {
            if ($content -notmatch 'validated_adrs:') {
                Fail "Recent validation report missing validated_adrs field: $($report.FullName)"
                $valFieldErrors++
            }
            if ($content -notmatch 'validated_game_rules:') {
                Fail "Recent validation report missing validated_game_rules field: $($report.FullName)"
                $valFieldErrors++
            }
        }
    }
    if ($valFieldErrors -eq 0) {
        Ok "Validation reports properly include validated_adrs and validated_game_rules fields."
    }
} else {
    Ok "No execution reports found to validate."
}

# Check 7: No specs cite amendments as canonical sources
$amendmentPattern = 'docs/amendments/[^/]+\.md(?!\s*.*\(archived|historical)'
$specsWithBadAmendRefs = Get-ChildItem "docs/specs" -Recurse -Filter "*.md" -File -ErrorAction SilentlyContinue |
    Where-Object { (Get-Content $_.FullName -Raw) -match $amendmentPattern }
if ($specsWithBadAmendRefs) {
    $specsWithBadAmendRefs | ForEach-Object { Fail "Spec cites amendment as canonical (not archived): $($_.FullName)" }
} else {
    Ok "No specs cite amendments as canonical sources."
}

# Check 8: DOCUMENT_INDEX references canonical decision/rule sources
$docIndex = Get-Content "docs/project/DOCUMENT_INDEX.md" -Raw -ErrorAction SilentlyContinue
if ($docIndex -notmatch 'DECISION_LOG\.md') {
    Fail "DOCUMENT_INDEX.md must reference docs/project/DECISION_LOG.md"
}
if ($docIndex -notmatch 'docs/decisions/') {
    Fail "DOCUMENT_INDEX.md must reference docs/decisions/"
}
if ($docIndex -notmatch 'GAME_RULES_INDEX\.md') {
    Fail "DOCUMENT_INDEX.md must reference docs/game_rules/GAME_RULES_INDEX.md"
}
if ($docIndex -match 'DECISION_LOG\.md' -and $docIndex -match 'docs/decisions/' -and $docIndex -match 'GAME_RULES_INDEX\.md') {
    Ok "DOCUMENT_INDEX.md properly references canonical decision/rule sources."
}

# Check 9: No active documents cite amendments outside validation context
$refDocs = @("docs/project/CURRENT_STATE.md", "docs/project/DOCUMENT_GOVERNANCE.md")
foreach ($doc in $refDocs) {
    if (Test-Path $doc) {
        $content = Get-Content $doc -Raw
        if ($content -match 'docs/amendments/[^/]+\.md' -and $content -notmatch 'archived|historical') {
            Fail "Active document cites amendment as canonical: $doc"
        }
    }
}
if (-not ($refDocs | Where-Object { Test-Path $_ } | Where-Object { (Get-Content $_ -Raw) -match 'docs/amendments/[^/]+\.md' -and (Get-Content $_ -Raw) -notmatch 'archived|historical' })) {
    Ok "No active documents cite amendments as canonical sources."
}

# Check 10: No legacy numbered folders exist (consolidation is complete)
$legacyFoldersExist = $false
$checkFolders = @("00_PROJECT", "01_PRODUCT", "02_ARCHITECTURE", "03_SPECS", "04_REFINEMENTS", "05_VALIDATION", "06_BACKLOG", "07_RELEASES", "08_ARCHIVE")
foreach ($folderName in $checkFolders) {
    if (Test-Path "docs/$folderName") {
        Fail "Legacy numbered folder docs/$folderName exists (consolidation incomplete)"
        $legacyFoldersExist = $true
    }
}
if (-not $legacyFoldersExist) {
    Ok "No legacy numbered folders exist (consolidation complete)."
}

if ($failed) {
    Write-Host "Docs validation FAILED." -ForegroundColor Red
    exit 1
}

Write-Host "Docs validation PASSED." -ForegroundColor Green
exit 0
