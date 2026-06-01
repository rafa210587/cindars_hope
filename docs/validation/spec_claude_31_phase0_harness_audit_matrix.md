---
doc_type: validation
status: evidence
spec_id: SPEC_CLAUDE_31
validation_type: audit
result: COMPLETE
date: 2026-06-01
executor: Claude Code
source_of_truth: false
---

# Phase 0 Audit Matrix — SPEC_CLAUDE_31 Agent Runtime Governance

> Audit only. No files changed in this phase.

---

## 8.1 Inventory — `.claude/` Files

### Commands

| Path | Exists | Current Function | Problem | Proposed Function | Action | Alter This Spec? |
|------|--------|-----------------|---------|-------------------|--------|-----------------|
| `.claude/commands/start-spec.md` | YES | Prepare spec execution plan | Reads PROJECT_LOG.md + IMPLEMENTATION_STATUS.md + AGENT_EXECUTION_PROTOCOL.md as Camada 0 (expensive) | Read only CLAUDE.md + CURRENT_STATE.md + spec | Update: strip heavy Camada 0 reads | YES |
| `.claude/commands/implement-spec.md` | YES | Execute spec end-to-end | Phase 4 Closeout auto-moves spec to `implementados/` on PASS — no Phase 2-3 gate | Execute spec, generate report, phase-based status, NO auto-promote | Update: phase gates, no auto-promote | YES |
| `.claude/commands/finish-spec.md` | YES | Closeout checklist | Moves spec to implementados/ without distinguishing Phase 0-1 vs Phase 2-3 | Phase-aware closeout with status taxonomy | Update: phase distinction | YES |
| `.claude/commands/validate-unity.md` | YES | Unity compile flow | Name misleading (actually docs+compile), no NOT RUN record format | Separate validation types, explicit NOT RUN | Update: clarify scope + NOT RUN | YES |
| `.claude/commands/review-non-regression.md` | YES | Audit diff for violations | Good overall; minor context read update | No structural change needed | Minor update | NO |
| `.claude/commands/docs-health.md` | YES | Docs validation only | Good; can stay | No change | Keep | NO |
| `.claude/commands/audit-spec.md` | NO | — | Missing: no audit-only command exists | Phase 0 audit without implementation | Create | YES |
| `.claude/commands/validate-spec.md` | NO | — | Missing: validation separated from implementation | All 4 validation levels, explicit NOT RUN | Create | YES |
| `.claude/commands/reconcile-status.md` | NO | — | Missing: reconciliation mode needs its own command | Allowed to read PROJECT_LOG, reports | Create | YES |
| `.claude/commands/plan-wave.md` | NO | — | Missing: ROADMAP reader needs boundary | Only command allowed to read ROADMAP | Create | YES |
| `.claude/commands/bugfix.md` | NO | — | Missing: bug-scoped execution | Minimal context, no roadmap | Create | YES |

### Skills

| Path | Exists | Current Function | Problem | Proposed Function | Action | Alter This Spec? |
|------|--------|-----------------|---------|-------------------|--------|-----------------|
| `.claude/skills/spec-execution/SKILL.md` | YES | End-to-end spec execution | Step "Move spec to implementados/" in Phase 4 has no Phase 2-3 gate; reads SPEC_EXECUTION_ORDER whole | Phase-gated closeout, no auto-promote | Update | YES |
| `.claude/skills/unity-validation/SKILL.md` | YES | Unity compile flow | Good | Minor NOT RUN clarification | Minor update | NO |
| `.claude/skills/docs-migration/SKILL.md` | YES | Moving spec/refinement files | Good | Keep | Keep | NO |
| `.claude/skills/non-regression-review/SKILL.md` | YES | Audit diff for forbidden patterns | Good | Keep | Keep | NO |
| `.claude/skills/save-load-pattern/SKILL.md` | YES | Save/load data pattern | Good | Keep | Keep | NO |
| `.claude/skills/event-bus-pattern/SKILL.md` | YES | GameEventBus usage | Good | Keep | Keep | NO |
| `.claude/skills/implementation-closeout/SKILL.md` | YES | Final closeout checklist | Promotes spec without Phase 2-3 check | Add phase-aware status | Update | YES |
| `.claude/skills/unity-validation-triage/SKILL.md` | YES | Error triage | Good | Keep | Keep | NO |
| `.claude/skills/unity-asset-generation/SKILL.md` | YES | Asset generation evidence | Good | Keep | Keep | NO |
| `.claude/skills/cave-stable-run-guard/SKILL.md` | YES | Cave stable run checks | Good | Keep | Keep | NO |
| `.claude/skills/docs-governance/SKILL.md` | NO | — | Missing: governance tasks have no skill | Doc organize/archive/index tasks | Create | YES |
| `.claude/skills/bootstrap-wiring/SKILL.md` | NO | — | Missing: bootstrap integration not formalized | GameBootstrap wiring pattern | Create | YES |
| `.claude/skills/combat-data-wiring/SKILL.md` | NO | — | Missing: SPEC_09 pattern not formalized | StatusEffect/Spell/Weapon database wiring | Create | YES |
| `.claude/skills/ui-modal-stack/SKILL.md` | NO | — | Missing: modal stack has no skill | ModalManager, input blocking, Esc close | Create | YES |

### Agents

| Path | Exists | Current Role | Problem | Proposed Role | Action | Alter This Spec? |
|------|--------|-------------|---------|---------------|--------|-----------------|
| `.claude/agents/spec-implementer.md` | YES | Implement specs | Step 1 says "Read AGENTS.md → CLAUDE.md → PROJECT_LOG (top)"; moves spec without phase gate | Minimal context, phase-aware closeout | Update | YES |
| `.claude/agents/docs-curator.md` | YES | Doc governance | Good | Minor update for 00_PROJECT structure | Update | YES |
| `.claude/agents/unity-validator.md` | YES | Unity validation | Good | Keep | Keep | NO |
| `.claude/agents/non-regression-auditor.md` | YES | Regression audit | Good | Keep | Keep | NO |
| `.claude/agents/architecture-reviewer.md` | YES | Architecture review | Good | Keep | Keep | NO |
| `.claude/agents/bugfix-investigator.md` | NO | — | Missing | Bug-scoped, no roadmap read | Create | YES |
| `.claude/agents/asset-wiring-specialist.md` | NO | — | Missing | Unity data wiring, validator, prefab refs | Create | YES |

### Hooks

| Path | Exists | Trigger | Always On | Cost | Problem | Action | Alter This Spec? |
|------|--------|---------|-----------|------|---------|--------|-----------------|
| `.claude/hooks/pre-bash-guard.ps1` | YES | pre-bash | YES | Low | Good: blocks unsafe git+file ops | Keep | NO |
| `.claude/hooks/detect-change-scope.ps1` | YES | post-edit | YES | Low | Good: writes change-scope.json | Keep | NO |
| `.claude/hooks/stop-summary-check.ps1` | YES | stop | YES | Low | Still lists "Spec moved to implementados/" as if always required; reads change-scope.json | Minor: only check promotion if spec was in scope | YES |
| `.claude/hooks/run-required-validations.ps1` | YES | manual | NO | Medium | Good | Keep | NO |
| `.claude/hooks/post-edit-docs-validate.ps1` | YES | post-edit | NO (disabled) | Low | Good | Keep disabled | NO |
| `.claude/hooks/check-csproj-includes.ps1` | YES | manual | NO | Low | Good | Keep | NO |
| `.claude/hooks/check-runtime-forbidden-search.ps1` | YES | manual | NO | Low | Good | Keep | NO |
| `.claude/hooks/check-cave-stable-run-scope.ps1` | YES | manual | NO | Low | Good | Keep | NO |
| `.claude/hooks/context-policy-check.ps1` | NO | — | — | — | Missing: no context policy enforcement | Create (disabled) | YES |
| `.claude/hooks/docs-status-honesty-check.ps1` | NO | — | — | — | Missing: no honesty guard on docs | Create (disabled) | YES |
| `.claude/hooks/spec-promotion-guard.ps1` | NO | — | — | — | Missing: no guard on premature promotion | Create (disabled) | YES |
| `.claude/hooks/delete-guard.ps1` | NO | — | — | — | Missing: no guard on doc deletion | Create (disabled) | YES |
| `.claude/hooks/unity-yaml-edit-guard.ps1` | NO | — | — | — | Missing: no guard on YAML edits | Create (disabled) | YES |

### Settings

| Field | Current Value | Problem | Proposed | Action |
|-------|--------------|---------|----------|--------|
| `allowedTools.todovrite` | `"todovrite": true` | Typo: should be `todoWrite` (Claude Code tool name) | `"todoWrite": true` | Fix typo |
| `documentation.sourceOfTruth` | `"docs/specs/"` | Good | Keep | Keep |
| `documentation.implementationStatus` | `"docs/IMPLEMENTATION_STATUS.md"` | Not removed, just de-prioritized | Keep as reference | Keep |
| `documentation.projectLog` | `"PROJECT_LOG.md"` | Not removed, just de-prioritized | Keep + add policy link | Keep |
| `documentation.operationalProtocol` | `"docs/operations/AGENT_EXECUTION_PROTOCOL.md"` | Outdated reference; CURRENT_STATE.md is new default | Add `currentState` field | Update |
| `rules.*` | 8 flags existing | Missing new policy flags | Add 5 new flags | Update |
| `hooks.*` | 8 hooks existing | Missing 5 new hooks | Add as disabled | Update |

---

## 8.2 Root File Inventory

| File | Current Function | Proposed Function | Default Read? | Token Risk | Drift Risk | Action |
|------|-----------------|-------------------|--------------|-----------|-----------|--------|
| `CLAUDE.md` | Full manual: spec rules + context + skills list + code rules + git rules | Short router: project ID, minimal reads, conditional reads, command links, stop conditions | YES | HIGH — ~150 lines, mixes rules with routing | MEDIUM | Rewrite short |
| `AGENTS.md` | Full manual: project context + code rules + git rules + context policy (added SPEC_DOCS_30) | Multi-agent common rules: context policy + code rules + git | YES | HIGH — ~185 lines | MEDIUM | Reduce: strip duplicated routing/skill lists; keep rules |
| `PROJECT_LOG.md` | Historical execution log | Historical log only (governance header already added SPEC_DOCS_30) | NO (policy set SPEC_DOCS_30) | VERY HIGH | HIGH | Already fixed; header in place |
| `README.md` | Project readme | No change | NO | LOW | LOW | Keep |

---

## 8.3 Command Inventory

| Command | Exists | Reads PROJECT_LOG? | Can Edit Code? | Can Move Spec? | Can Read ROADMAP? | Status |
|---------|--------|-------------------|----------------|----------------|-------------------|--------|
| `/start-spec` | YES | YES (Camada 0) — PROBLEM | NO | NO | NO | Fix: remove PROJECT_LOG from Camada 0 |
| `/audit-spec` | NO | Should be: NO | NO | NO | NO | Create |
| `/implement-spec` | YES | YES (Camada 0) — PROBLEM | YES | YES (auto, no phase gate) — PROBLEM | NO | Fix: remove heavy reads, add phase gate |
| `/validate-spec` | NO | Should be: NO | NO | NO | NO | Create |
| `/finish-spec` | YES | NO | NO | YES (no phase gate) — PROBLEM | NO | Fix: phase gate |
| `/reconcile-status` | NO | Should be: YES (audit mode only) | NO | NO | NO | Create |
| `/docs-health` | YES | NO | NO | NO | NO | Keep |
| `/review-non-regression` | YES | NO | NO | NO | NO | Keep |
| `/validate-unity` | YES | NO | NO | NO | NO | Minor update |
| `/plan-wave` | NO | Should be: NO | NO | NO | YES (only command) | Create |
| `/bugfix` | NO | Should be: NO | YES | NO | NO | Create |

---

## 8.4 Skill Inventory

| Skill | Exists | Current Use | Proposed Use | Action |
|-------|--------|-------------|-------------|--------|
| `spec-execution` | YES | Full workflow; auto-move to implementados | Phase-gated workflow; phase status taxonomy | Update |
| `unity-validation` | YES | Compile flow | Keep | Keep |
| `docs-migration` | YES | Move files | Keep | Keep |
| `non-regression-review` | YES | Audit diff | Keep | Keep |
| `save-load-pattern` | YES | Save/load data | Keep | Keep |
| `event-bus-pattern` | YES | GameEventBus | Keep | Keep |
| `implementation-closeout` | YES | Final closeout | Add phase-aware status | Update |
| `unity-validation-triage` | YES | Error triage | Keep | Keep |
| `unity-asset-generation` | YES | Asset evidence | Keep | Keep |
| `cave-stable-run-guard` | YES | Cave checks | Keep | Keep |
| `docs-governance` | NO | — | Doc organize/move/archive/index | Create |
| `bootstrap-wiring` | NO | — | GameBootstrap integration | Create |
| `combat-data-wiring` | NO | — | StatusEffect/Spell/Weapon database | Create |
| `ui-modal-stack` | NO | — | ModalManager, input blocking | Create |

---

## 8.5 Agent Inventory

| Agent | Exists | Default Reads | Can Edit Code? | Can Edit Docs? | Can Run Validation? | Action |
|-------|--------|--------------|----------------|----------------|---------------------|--------|
| `spec-implementer` | YES | AGENTS.md, CLAUDE.md, PROJECT_LOG (top), spec | YES | YES | YES | Update: remove PROJECT_LOG; phase gate closeout |
| `docs-curator` | YES | AGENTS.md, relevant docs | NO | YES | YES | Update: add 00_PROJECT context awareness |
| `unity-validator` | YES | CLAUDE.md, spec, validators | NO | NO | YES | Keep |
| `non-regression-auditor` | YES | CLAUDE.md, diff, rules | NO | NO | YES | Keep |
| `architecture-reviewer` | YES | CLAUDE.md, arch, roadmap | NO | NO | NO | Keep |
| `bugfix-investigator` | NO | — | YES | NO | YES | Create |
| `asset-wiring-specialist` | NO | — | NO | NO | YES | Create |

---

## 8.6 Hook Inventory

| Hook | Exists | Trigger | Always On | Cost | Potential FP | Proposed Action |
|------|--------|---------|-----------|------|-------------|-----------------|
| `pre-bash-guard.ps1` | YES | pre-bash | YES | Low | Low | Keep enabled |
| `detect-change-scope.ps1` | YES | post-edit | YES | Low | Low | Keep enabled |
| `stop-summary-check.ps1` | YES | stop | YES | Low | Medium: spec-move checklist too aggressive | Update: conditional on spec scope |
| `run-required-validations.ps1` | YES | manual | NO | Medium | Low | Keep manual |
| `post-edit-docs-validate.ps1` | YES | post-edit | NO | Low | Low | Keep disabled |
| `check-csproj-includes.ps1` | YES | manual | NO | Low | Low | Keep manual |
| `check-runtime-forbidden-search.ps1` | YES | manual | NO | Low | Low | Keep manual |
| `check-cave-stable-run-scope.ps1` | YES | manual | NO | Low | Low | Keep manual |
| `context-policy-check.ps1` | NO | manual | NO | Low | Low | Create disabled |
| `docs-status-honesty-check.ps1` | NO | manual | NO | Low | Low | Create disabled |
| `spec-promotion-guard.ps1` | NO | manual | NO | Low | Low | Create disabled |
| `delete-guard.ps1` | NO | manual | NO | Low | Low | Create disabled |
| `unity-yaml-edit-guard.ps1` | NO | manual | NO | Low | Low | Create disabled |

---

## Summary

**Problems identified (require change):**
1. `CLAUDE.md` — too long (~150 lines), mixes routing with code rules, mandates heavy reads
2. `AGENTS.md` — still references PROJECT_LOG/IMPLEMENTATION_STATUS in Fluxo operacional "Leitura minima"
3. `/start-spec` and `/implement-spec` — read PROJECT_LOG.md + IMPLEMENTATION_STATUS.md + AGENT_EXECUTION_PROTOCOL.md as Camada 0
4. `/implement-spec` Phase 4 — auto-moves spec to `implementados/` without Phase 2-3 gate
5. `/finish-spec` — no phase distinction in closeout status
6. `spec-execution` skill + `implementation-closeout` skill — auto-move without phase gate
7. `spec-implementer` agent — reads PROJECT_LOG as default
8. `settings.json` — typo `todovrite`, missing CURRENT_STATE.md in documentation block
9. 5 commands missing: `audit-spec`, `validate-spec`, `reconcile-status`, `plan-wave`, `bugfix`
10. 4 skills missing: `docs-governance`, `bootstrap-wiring`, `combat-data-wiring`, `ui-modal-stack`
11. 2 agents missing: `bugfix-investigator`, `asset-wiring-specialist`
12. 5 hooks missing: context-policy-check, docs-status-honesty-check, spec-promotion-guard, delete-guard, unity-yaml-edit-guard
13. 6 rules missing: context-reading-policy, no-unsafe-git, event-bus-only-gameplay-communication, spec-promotion-requires-evidence, no-premature-acceptance-claims, no-doc-delete-without-candidate, unity-yaml-editing-policy

**No change needed:**
- `docs-health`, `review-non-regression`, `validate-unity` (minor only)
- Unity-related skills (unity-validation, triage, asset-generation, cave-stable-run-guard)
- save-load-pattern, event-bus-pattern, docs-migration, non-regression-review skills
- unity-validator, non-regression-auditor, architecture-reviewer agents
- pre-bash-guard, detect-change-scope, run-required-validations hooks
- Existing 8 rules in `.claude/rules/`

---

*Audit complete: 2026-06-01*
