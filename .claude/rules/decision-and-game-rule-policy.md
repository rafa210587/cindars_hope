# Rule: Decision and Game Rule Policy

## Rule

Agents must read and follow Architecture Decision Records (ADRs) and game rules as canonical sources of truth. Amendments and historical documents are archived; only active ADRs and game_rules define decisions and behavior.

## Core Policy

1. **ADRs are canonical for decisions**
   - All architectural decisions live in `docs/decisions/ADR-NNNN-*.md`
   - ADRs explain the *why*; game_rules state the *what*
   - If an old amendment conflicts with an ADR, the ADR is correct

2. **Game rules are canonical for current behavior**
   - All current gameplay, operational, and architectural rules live in `docs/game_rules/*.md`
   - Game rules document what is true NOW, not what is desired in future
   - If code contradicts a game rule, stop and report (code or rule is stale)

3. **Amendments are archived**
   - All amendments (FASE9F, FASE9G, etc.) are migrated to ADRs/game_rules
   - `docs/amendments/` contains historical record only
   - Do NOT use amendments as canonical source; reference canonical ADRs/game_rules instead

4. **Reading policy**
   - Specs cite `required_adrs: [ADR-0001, ...]` and `required_game_rules: [cave_rules.md, ...]`
   - Read only cited ADRs/game_rules; do not read all ADRs/game_rules by default
   - Index documents (DECISION_LOG.md, GAME_RULES_INDEX.md) are for navigation, not implementation

5. **Conflict resolution**
   - **ADR vs. code:** ADR explains decision; if code violates ADR, stop and report
   - **Game rule vs. code:** Game rule is current; if code violates rule, stop and report (code or rule stale)
   - **ADR vs. game rule:** Game rule is current operational state; ADR explains history; update ADR or create superseding ADR if behavior changes

## Why

- **Single source of truth:** No fragmented decisions across amendments, refinements, and operational rules
- **Auditability:** All decisions have documented rationale in ADRs; easy to understand why
- **Stability:** Game rules document current behavior; agents know what is true without reading code
- **Clarity:** Agents read only what is relevant to their task (cited ADRs/game_rules, not all)

## Applies To

All agent-run spec execution, bug fix, and feature tasks.

## Validation / Detection

Hook `.claude/hooks/decision-rule-reference-guard.ps1` (when enabled) warns if:
- Spec references amendment as canonical (should reference ADR/game_rule instead)
- Spec lacks required_adrs/required_game_rules fields (if spec depends on decisions/rules)

---

*Created: 2026-06-01 (SPEC_DOCS_38 Phase 9)*  
*See: docs/project/DECISION_LOG.md, docs/game_rules/GAME_RULES_INDEX.md*
