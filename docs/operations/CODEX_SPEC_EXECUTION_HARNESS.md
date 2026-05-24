# CODEX SPEC Execution Harness

**Versão:** 2026-05-24  
**Target Agent:** Codex  
**Autoridade:** Claude/Haiku decisions, user approval only for new SPEC selection  

---

## Quick Start

```
User says: "vamos para o próximo"
↓
Codex reads this harness
↓
Codex checks SPEC_EXECUTION_ORDER.md for next executable spec
↓
Codex reads SPEC_XX prompt from docs/agent_prompts/a_executar/
↓
Codex executes Spec Completion Skill (10 fases)
↓
All validations PASS → User says "vamos para o próximo"
```

---

## 1. CONTEXT AND RULES

### Official Single Source of Specs

```
docs/specs/a_implementar/spec_*.md          ← SPECS to execute
docs/specs/implementados/spec_*.md          ← SPECS already done
docs/agent_prompts/a_executar/SPEC_XX_*_PROMPT.md   ← Agent prompts (move to implementados/ when done)
```

### Immutable Rules from CLAUDE.md

1. **NEVER** use `GameObject.Find()` or `FindObjectOfType()`
2. **NEVER** hardcode gameplay data in MonoBehaviour; use ScriptableObject in `Assets/_Game/Data/`
3. **ALWAYS** use `GameEventBus.Publish()` for gameplay communication (no direct calls)
4. **ALWAYS** unsubscribe in `OnDisable` or `OnDestroy`
5. **ALWAYS** prefix ScriptableObjects: `ItemDataSO`, `SeedDataSO`, `ToolDataSO`, etc.
6. **ALWAYS** prefix events: `DayStartedEvent`, `ItemCraftedEvent`, etc.
7. **ALWAYS** commit in Portuguese
8. Save must persist IDs and simple types, NEVER Unity references
9. Sprites: Filter Mode `Point`, Compression `None`, Generate Mip Maps `false`
10. **NEVER** create namespace called `Debug` inside `CindarsHope.*` — use `Runtime`, `DebugTools`, `Diagnostics`, or `Editor`

### Working Method (Sequential)

```
Read SPEC → Validate → Implement → Run Tests → Commit → Next
```

**Each SPEC must be 100% complete before starting next.**

---

## 2. EXECUTION PHASES (10-Phase Skill)

### Phase 1: Read and Reconciliation

```powershell
# Files to read in order:
1. AGENTS.md (project context)
2. CLAUDE.md (AI agent rules)
3. PROJECT_LOG.md (top section, recent activity)
4. docs/IMPLEMENTATION_STATUS.md (current state table)
5. docs/operations/AGENT_EXECUTION_PROTOCOL.md (operational procedures)
6. docs/specs/SPEC_EXECUTION_ORDER.md (dependency matrix)
7. docs/specs/a_implementar/spec_XX_*.md (target SPEC)
8. docs/refinements/a_implementar/pre_refinamentos/refinement_init_*.md (pre-refinement if exists)
9. docs/specs/implementados/spec_*.md (dependent specs cited in target)
```

**Goal:** Fully understand requirements, scope, dependencies.

### Phase 2: Implementation (As Per Spec Prompt)

- Follow spec prompts exactly
- Follow CLAUDE.md immutable rules
- Alter ONLY in-scope files
- Register blockers if cannot execute
- Do NOT mark partial as complete without evidence

### Phase 3: Validations (MANDATORY)

#### A. Docs Validation

```powershell
cd "D:\Projetos\Jogos\Cindars_hope\cindars_hope"
.\tools\docs\validate_docs.ps1
```

- **PASS:** Continue to next step
- **FAIL:** Correct docs, rerun until PASS

#### B. Unity Compile Validation

```powershell
cd "D:\Projetos\Jogos\Cindars_hope\cindars_hope"
.\tools\unity\RunUnityCompileValidation.ps1
```

- **Tundra build success:** = PASS (ignore exit code 1 if "Tundra build success" in log)
- **Exit code 0 + Tundra success:** = PASS
- **Errors in log:** = FAIL (fix and rerun)
- **NOT RUN (sandbox):** = Document formally with reason

#### C. Log Scan

```powershell
cd "D:\Projetos\Jogos\Cindars_hope\cindars_hope"
grep -E "Tundra build success|error|Error|FAILED" .\Logs\unity-compile-validation.log | head -20
```

- **Tundra success found:** = PASS
- **Business errors found:** = FAIL (fix Phase 2, rerun Phase 3)
- **Assembly warnings only:** = PASS (non-blocking)

#### D. Unity batchmode sequencing

- Execute a single Unity batchmode process at a time for the same project.
- Scene generation, content asset generation and validation must run sequentially; parallel Unity sessions compete for the project lock.
- If `ScanUnityLogs.ps1` flags only stale/invalid `Assembly-CSharp-firstpass.dll` assemblies while the same compile log contains `Tundra build success`, no `error CS` and Unity exits with return code `0`, register the scanner alert as residual tooling noise and treat compile as PASS.

### Phase 4: Move Documentation

```powershell
# Move SPEC prompt from a_executar/ to implementados/
Move-Item -Path "docs/agent_prompts/a_executar/SPEC_XX_*_PROMPT.md" `
          -Destination "docs/agent_prompts/implementados/"

# Move SPEC itself (if in a_implementar/)
Move-Item -Path "docs/specs/a_implementar/spec_XX_*.md" `
          -Destination "docs/specs/implementados/"
```

### Phase 5: Update Registries

#### 5A. SPEC_REGISTRY_TO_IMPLEMENT.md
Remove spec entry if it was there.

#### 5B. SPEC_REGISTRY_IMPLEMENTED.md
Add or update entry with:
- Spec name
- Status: "Implementado parcial" or "Implementado completo"
- Component files (evidence)
- Linked refinement
- Pending items (if partial)

#### 5C. IMPLEMENTATION_STATUS.md
Add/update line in status table with:
- Area | Status | Spec file

### Phase 6: Update Logs

#### 6A. PROJECT_LOG.md
Add session entry with:
```
## Sessão YYYY-MM-DD (Nº)

**Foco:** SPEC_XX — [Name]  
**Status:** Implementado completo | parcial  

**Deliverables:**
- [Component 1] ✓
- [Component 2] ✓

**Validações:**
- Docs: PASS
- Unity: PASS (Tundra build success X.XXs)
- Registries: PASS

**Play Mode:** [Checklist or NOT RUN reason]

**Próxima spec:** SPEC_YY (executável)
```

### Phase 7: Git Staging and Commit

```powershell
cd "D:\Projetos\Jogos\Cindars_hope\cindars_hope"

# Verify changes
git status

# Stage docs and operations files
git add -A docs/ tools/

# Verify staging
git status

# Commit in Portuguese
git commit -m "feat/fix/chore: (type) descrição breve

- Detalhe 1
- Detalhe 2

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"

# Confirm
git log --oneline -5
```

### Phase 8: Final Revalidation

```powershell
cd "D:\Projetos\Jogos\Cindars_hope\cindars_hope"
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
```

If FAIL: Fix → Re-commit → Revalidate

### Phase 9: Delivery Result

```
AGENT_RESULT: SUCCESS

SPEC_STATUS: IMPLEMENTADO COMPLETO

CHANGED_FILES:
- docs/specs/implementados/spec_XX_*.md
- docs/agent_prompts/implementados/SPEC_XX_*.md
- docs/IMPLEMENTATION_STATUS.md
- docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
- PROJECT_LOG.md

VALIDATIONS:
- docs: PASS
- unity_compile: PASS (Tundra build success)
- registries: PASS

NEXT_ACTION: Ready for next SPEC
```

### Phase 10: Play Mode Checklist

Document test case for human validation:

```
PLAY MODE TEST: SPEC XX — [Name]

Scene used: [Farm/Town/Cave/etc]
Steps: 
1. [Action]
2. [Action]
Expected: [Result]
Observed: [Result]
Bugs: None | [List]
Passed: YES | NO | NOT_RUN

Evidence: [code links or logs]
```

---

## 3. DECISION TREE

```
User says "vamos para o próximo"
  ↓
Codex reads this harness
  ↓
Is previous SPEC 100% complete?
  ├─ YES → Read SPEC_EXECUTION_ORDER.md
  │         Find next executable SPEC
  │         Read SPEC_XX_PROMPT from docs/agent_prompts/a_executar/
  │         Execute 10-Phase Skill
  │
  └─ NO  → STOP. Previous spec must be finished first.
```

---

## 4. SPEC SELECTION

**Check SPEC_EXECUTION_ORDER.md for:**
- Current executable specs (no blockers)
- Dependent specs that are ready
- Skip any with unfinished dependencies

**Alphabetical order when available:**
- SPEC_02, SPEC_03, SPEC_04, SPEC_05, SPEC_06, ...
- SPEC_XX_YY (e.g., SPEC_17B)
- SPEC_99 (if additional content)

---

## 5. FAILURE HANDLING

### If Phase 3 (Validations) FAILS:

1. **Docs validation FAIL:**
   - Fix documentation issues
   - Rerun `validate_docs.ps1`
   - If still FAIL: investigate root cause, DO NOT commit

2. **Unity compilation FAIL:**
   - Read full log: `.\Logs\unity-compile-validation.log`
   - Identify error (CS0246, CS0101, etc.)
   - Fix in Phase 2 code
   - Rerun Phase 3 validations
   - If still FAIL: register blocker, stop

3. **Log scan FAIL:**
   - Grep for errors: `error|Error|FAILED`
   - If business logic error: fix, rerun Phase 3
   - If assembly warning: non-blocking, continue

### If Phase 7 (Git) FAILS:

- Verify: `git status` shows expected files
- Check hooks: `git log --oneline -2` (verify pre-commit succeeded)
- If staging issue: `git restore --staged` unwanted files, re-stage
- If commit issue: investigate message encoding, retry with ASCII-safe message

### If ANY OTHER Phase BLOCKS:

Register in PROJECT_LOG.md:
```
**Blocker:** [Phase X] — [Issue]
**Cause:** [Root cause]
**Mitigation:** [What to do]
**Risk:** [Impact if unresolved]
```

User will advise next steps.

---

## 6. QUICK REFERENCE

| Phase | What | Reversible | Check |
|-------|------|-----------|-------|
| 1 | Read docs | No | `IMPLEMENTATION_STATUS.md` matches reality |
| 2 | Implement | Yes | Code compiles, follows CLAUDE.md rules |
| 3 | Validate | Partial | "Tundra build success" in logs, no business errors |
| 4 | Move files | Yes | `git status` shows moves correctly |
| 5 | Update registries | Yes | Registries match implemented specs |
| 6 | Update logs | Yes | SESSION entry present, dated today |
| 7 | Git commit | Conditional | `git log --oneline -1` shows commit |
| 8 | Revalidate | Partial | Both validations PASS |
| 9 | Deliver | No | Result format correct |
| 10 | Checklist | No | Test case documented |

---

## 7. CONTACT & ESCALATION

- **Questions about spec:** Check prompt file first, then ask user
- **Validation keeps failing:** Register blocker, ask user for guidance
- **Unclear scope:** Stop, ask user to clarify before proceeding
- **Dependency not ready:** Check SPEC_EXECUTION_ORDER.md, wait or ask user

---

**Last Updated:** 2026-05-24  
**Applies To:** Codex SPEC execution, all SPECS 02-17B  
**Author:** Claude/Haiku (orchestrated by user)
