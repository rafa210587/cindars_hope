# CODEX: Spec Orchestration Prompt

**When to use:** User says "vamos para o próximo" OR explicitly names SPEC_XX  
**Agent:** Codex  
**Authority:** CLAUDE.md + CODEX_SPEC_EXECUTION_HARNESS.md  

---

## YOUR TASK

Execute the next unfinished SPEC in alphabetical order from `docs/agent_prompts/a_executar/` using the **10-Phase Completion Skill** defined in `CODEX_SPEC_EXECUTION_HARNESS.md`.

---

## STEP 1: Identify Next SPEC

1. Read `docs/specs/SPEC_EXECUTION_ORDER.md`
2. Find the next spec that:
   - Is in `docs/agent_prompts/a_executar/` (not `implementados/`)
   - Has all dependencies ready (check dependency matrix)
   - Is **not** marked as BLOCKED or WAITING
3. If tie, use alphabetical order: SPEC_02 < SPEC_03 < ... < SPEC_17B

**Example:** If SPEC_05 is done and SPEC_06 is ready, execute SPEC_06.

---

## STEP 2: Read the Harness

Open `docs/operations/CODEX_SPEC_EXECUTION_HARNESS.md` and follow **Execution Phases 1-10** in exact order.

Do NOT skip or reorder phases.

---

## STEP 3: Execute 10-Phase Skill

### Phase 1: Read Docs
- Read AGENTS.md, CLAUDE.md, PROJECT_LOG.md (top), IMPLEMENTATION_STATUS.md, AGENT_EXECUTION_PROTOCOL.md
- Read target SPEC_XX prompt from `docs/agent_prompts/a_executar/`
- Read target spec from `docs/specs/a_implementar/` (if it exists)
- Read pre-refinement from `docs/refinements/a_implementar/pre_refinamentos/` (if it exists)
- Read dependent specs from `docs/specs/implementados/` (cited in target)

### Phase 2: Implement
- Follow spec prompt exactly
- Follow CLAUDE.md immutable rules (10 rules: no GameObject.Find, no hardcode data, use GameEventBus, etc.)
- Alter ONLY in-scope files
- Do NOT expand scope
- Register blockers if you cannot execute

### Phase 3: Validate (MANDATORY)
```powershell
# A. Docs validation
.\tools\docs\validate_docs.ps1
# Expected: OK messages, ends with "Docs validation PASSED"

# B. Unity compile validation
.\tools\unity\RunUnityCompileValidation.ps1
# Expected: No C# errors, "Tundra build success" marker in log

# C. Log scan
grep "Tundra build success" .\Logs\unity-compile-validation.log
# Expected: Found at least once
```

- If docs FAIL → Fix → Rerun
- If Unity FAIL → Fix code → Rerun
- If "Tundra success" found → PASS (exit code 1 with warnings = OK)

### Phase 4: Move Files
```powershell
# Move SPEC prompt
Move-Item -Path "docs/agent_prompts/a_executar/SPEC_XX_*_PROMPT.md" `
          -Destination "docs/agent_prompts/implementados/"

# Move SPEC file (if in a_implementar)
Move-Item -Path "docs/specs/a_implementar/spec_XX_*.md" `
          -Destination "docs/specs/implementados/"
```

### Phase 5: Update Registries
1. **SPEC_REGISTRY_TO_IMPLEMENT.md** — Remove spec entry if present
2. **SPEC_REGISTRY_IMPLEMENTED.md** — Add/update with:
   - Status: "Implementado completo" or "Implementado parcial"
   - Component files (proof)
   - Pending items (if partial)
3. **IMPLEMENTATION_STATUS.md** — Add/update status row

### Phase 6: Update Logs
**PROJECT_LOG.md** — Prepend new session entry:
```
## Sessão YYYY-MM-DD (Nº)

**Foco:** SPEC_XX — [Name]
**Status:** Implementado completo | parcial

**Deliverables:**
- Component1.cs ✓
- Component2.cs ✓

**Validações:**
- Docs: PASS
- Unity: PASS (Tundra build success X.XXs)
- Registries: PASS

**Play Mode:** NOT RUN (sandbox) OR [Test case result]

**Próxima spec:** SPEC_YY (if known)
```

### Phase 7: Git Commit
```powershell
git add -A docs/ tools/
git status  # Verify staging
git commit -m "feat: validar e fechar spec XX - [short description]

- Deliverable 1
- Deliverable 2
- Validations: Docs PASS, Unity PASS

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"

git log --oneline -5  # Verify commit created
```

### Phase 8: Final Revalidation
```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
```

Both must PASS or be marked NOT_RUN with reason.

### Phase 9: Delivery Report

```
AGENT_RESULT: SUCCESS

SPEC_STATUS: IMPLEMENTADO COMPLETO

CHANGED_FILES:
- docs/agent_prompts/implementados/SPEC_XX_*
- docs/specs/implementados/spec_XX_*
- docs/IMPLEMENTATION_STATUS.md
- docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
- PROJECT_LOG.md

VALIDATIONS:
- docs: PASS
- unity_compile: PASS (Tundra build success)
- registries: PASS
- git_commit: CREATED

NEXT_SPEC: SPEC_YY (ready to execute)

READY_FOR_NEXT: YES
```

### Phase 10: Play Mode Checklist
Document for human testing (even if NOT RUN):
```
PLAY MODE: SPEC_XX — [Feature Name]
Scene: [Farm/Town/etc]
Test Steps:
1. [Action]
2. [Action]
Expected: [Result]
Status: NOT_RUN (sandbox) | PASSED | FAILED
Bugs: None | [List]
```

---

## IMPORTANT RULES

- **ONE SPEC PER SESSION:** Do not start next SPEC until current is 100% complete
- **ALWAYS FOLLOW PHASES:** Do not skip, reorder, or combine phases
- **PORTUGUESE COMMITS:** All commit messages in Portuguese
- **NEVER SKIP VALIDATION:** Phases 3 & 8 are non-negotiable
- **REGISTER BLOCKERS:** If stuck, document and ask user
- **RESPECT SCOPE:** Only modify files listed in spec scope
- **IMMUTABLE RULES:** Follow all 10 rules from CLAUDE.md without exception

---

## EDGE CASES

### If Spec Code Already Exists
- Audit existing code against spec requirements (Phase 1)
- If complete: note as "Implementado completo" with evidence
- If partial: complete missing pieces, note as "Implementado parcial"
- Still run validations (Phase 3) to confirm compilation

### If Validation Fails
- Do NOT commit
- Return to Phase 2, fix root cause
- Rerun Phase 3
- If still fails: register blocker, inform user

### If Dependency Not Ready
- Check SPEC_EXECUTION_ORDER.md
- If blocker: inform user, do NOT proceed
- If executable: continue

### If Docs Don't Match Code
- Use code as source of truth (it's newer)
- Update spec/registry to match reality
- Document discrepancy in commit message
- Flag to user in delivery report

---

## CHECKLIST

Before moving to Phase 4, confirm Phase 3:
- [ ] Docs validation PASS
- [ ] Unity compile PASS (or NOT_RUN with reason)
- [ ] No C# compilation errors
- [ ] "Tundra build success" found (if run)

Before committing (Phase 7), confirm Phase 6:
- [ ] PROJECT_LOG.md updated with session entry
- [ ] IMPLEMENTATION_STATUS.md updated
- [ ] SPEC_REGISTRY_IMPLEMENTED.md updated
- [ ] SPEC_REGISTRY_TO_IMPLEMENT.md cleaned (spec removed if was there)

Before delivery (Phase 9), confirm all phases:
- [ ] Phase 1-8 completed in order
- [ ] All files moved correctly
- [ ] All registries consistent
- [ ] Commit created with proper message
- [ ] Final validations PASS

---

## FAILURE = INFORM USER

If ANY phase fails and you cannot fix it:
1. Document the blocker clearly
2. Include: phase, error, root cause, what was attempted
3. Include: risk if unresolved, potential fix
4. Inform user and wait for guidance

Do NOT:
- Skip validation phases
- Commit with warnings
- Proceed to next SPEC before current is complete
- Guess at unclear requirements

---

**Last Updated:** 2026-05-24  
**Target Agent:** Codex  
**Context:** Cindar's Hope project, SPECS 02-17B orchestration
