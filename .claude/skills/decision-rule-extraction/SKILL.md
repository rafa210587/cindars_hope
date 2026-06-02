---
name: decision-rule-extraction
description: Extract durable decisions and current rules from specs, amendments, refinements, and validation evidence into ADRs and game_rules.
version: 1.0
when_to_use:
  - when creating or changing architectural/gameplay decisions
  - when migrating amendments
  - when a spec introduces a new canonical rule
  - when a rule changes and needs a superseding ADR
---

# Skill: Decision Rule Extraction

## Use When

- A document contains a durable design or architecture decision.
- A gameplay/system rule must become canonical.
- A spec changes existing behavior in a way that affects future specs.
- An amendment/spec/refinement is being retired.
- A rule changes and requires a superseding ADR.

## Required Reads

- `docs/project/CURRENT_STATE.md`
- `docs/project/DECISION_LOG.md`
- `docs/game_rules/GAME_RULES_INDEX.md`
- relevant ADRs only (cited by source document)
- relevant game_rules only (cited by source document)
- source document being migrated

## Do Not Read By Default

- all ADRs
- all game_rules
- PROJECT_LOG.md
- all validation reports
- all specs (except source document)

## Procedure

1. **Identify content type:**
   - decision rationale → create/update ADR
   - current operational/gameplay rule → create/update game_rules
   - evidence only → stays in validation report
   - obsolete → mark as delete candidate / archive

2. **Create or update ADR:**
   - Follow `docs/decisions/_templates/ADR_TEMPLATE.md`
   - Document decision rationale and consequences
   - Reference source document
   - Use numbering: ADR-0001, ADR-0002, etc.

3. **Create or update game_rule:**
   - Follow `docs/game_rules/_templates/GAME_RULE_TEMPLATE.md`
   - Document current behavior (not desired future state)
   - Reference source ADRs and documents
   - Use snake_case naming (e.g., `cave_rules.md`)

4. **Update indexes:**
   - Update `docs/project/DECISION_LOG.md` if new ADR
   - Update `docs/game_rules/GAME_RULES_INDEX.md` if new game_rule

5. **Update references:**
   - Update any specs or docs that previously cited the old source
   - Point them to new ADR/game_rule instead
   - Mark amendments as "archived" in README if content migrated

6. **If source was an amendment:**
   - Verify ALL content is migrated to ADR/game_rules
   - Archive or delete amendment per governance
   - Keep migration note in `docs/amendments/README.md`
   - Update `docs/project/DOCUMENT_INDEX.md` if amendment referenced

7. **Validate:**
   - Run `tools/docs/validate_docs.ps1`
   - Verify no broken references
   - Confirm ADR/game_rule are properly indexed

## Stop Conditions

Stop and report to human if:

- Source contains Batch 2 rule not yet Phase 2-3 validated (reserve for Batch 2)
- No canonical target exists (needs specification first)
- Migration would lose a unique decision (document separately)
- Existing ADR conflicts with new rule (reconcile first)
- Existing game_rule conflicts with current code and no reconciliation documented (report conflict)

## Output

Upon completion, provide:

1. **Created/updated ADRs:**
   - File paths
   - ADR IDs
   - Decision summaries

2. **Created/updated game_rules:**
   - File paths
   - Domain
   - Rule count

3. **Source documents resolved:**
   - Status (migrated, archived, deleted)
   - Migration completeness (%)

4. **Validation result:**
   - tools/docs/validate_docs.ps1 PASS/FAIL
   - Any warnings or errors

---

*Skill created: 2026-06-01 (SPEC_DOCS_39)*
