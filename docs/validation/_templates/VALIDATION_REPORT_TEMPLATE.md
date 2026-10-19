---
doc_type: validation
status: evidence
spec_id: SPEC_NN
validation_type: automated
result: NOT_RUN
date: YYYY-MM-DD
executor: Claude Code | Human
source_of_truth: false
validated_adrs: []
validated_game_rules: []
---

# Validation Report — SPEC_NN <Title>

> **This report is evidence, NOT an execution queue.**  
> **Do not re-run the spec based on this report alone.**

---

## What Was Run

- [ ] dotnet build Assembly-CSharp.csproj
- [ ] dotnet build Assembly-CSharp-Editor.csproj
- [ ] tools/docs/validate_docs.ps1
- [ ] Unity validators (specify which)
- [ ] Play Mode checklist

---

## What Was NOT Run

- [ ] <reason for not running>

---

## Results

| Check | Result | Notes |
|-------|--------|-------|
| C# runtime build | PASS/FAIL — NE/NW | Duration: Xs |
| C# editor build | PASS/FAIL — NE/NW | Duration: Xs |
| Docs validation | PASS/FAIL | tools/docs/validate_docs.ps1 |
| Unity validators | PASS/FAIL/NOT RUN | Which validators |
| Play Mode | PASS/FAIL/NOT RUN | |

---

## ADRs / Game Rules Validated

| Item | Status | Notes |
|---|---|---|
| ADR-XXXX | PASS/FAIL/NOT_RUN | |
| game_rule.md | PASS/FAIL/NOT_RUN | |

---

## Errors Found

```
(list compile errors, runtime errors, validation failures)
```

---

## Warnings (pre-existing)

```
(list pre-existing warnings; note if pre-existing or new)
```

---

## Evidence

Files changed:
```
(list files created or modified)
```

Files NOT changed (protected):
```
(list files that must not change per spec scope)
```

---

## Phase Status

| Phase | Status | Date |
|-------|--------|------|
| Phase 0 (Audit) | COMPLETE / NOT RUN | |
| Phase 1 (Automated) | PASS / FAIL / NOT RUN | |
| Phase 2 (Unity validators) | PASS / FAIL / NOT RUN | |
| Phase 3 (Play Mode) | PASS / FAIL / NOT RUN | |

---

## Next Action

```
(what needs to happen next — who needs to do it, where to find the checklist)
```

---

*Report generated: YYYY-MM-DD*
