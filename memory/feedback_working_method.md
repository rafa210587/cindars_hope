---
name: working_method_specs_execution
description: Proven method for executing large SPECS with sequential validation and iterative compilation fixes
metadata:
  type: feedback
---

## Method: Sequential SPEC Execution with Real-Time Validation

**Rule**: When executing multi-step SPECS (especially bootstrap/integration tasks), follow this proven pattern:
1. Read and understand SPEC requirements fully before starting
2. Implement in logical phases (bootstrap wiring → config → integration)
3. Run compilation validation after EACH phase (don't batch fixes)
4. Fix errors immediately with full namespace resolution (don't ignore conflicts)
5. Update memory and document patterns discovered
6. Commit after each validated phase

**Why**: Previous approach of "fix later" led to cascade failures where one namespace conflict masked others. Real-time validation caught 15+ hidden errors that would have compounded. Sequential commits preserve bisectability.

**How to apply**: 
- Always call PowerShell `RunUnityCompileValidation.ps1` before moving to next logical phase
- When compilation fails, read FULL error output (not just first error) - often 3-5 errors with root cause at bottom
- Use fully qualified namespaces (`CindarsHope.Core.EquipmentSaveData`) to disambiguate when multiple classes share names
- Commit after each validation passes, not after "all work is done"

---

## Method: Namespace Conflict Resolution

**Rule**: When code has duplicate class names in different namespaces:
1. Search codebase for ALL definitions of that class name
2. Identify which namespace is "source of truth" (usually: DTO namespace for Save, core namespace for runtime)
3. Consolidate: delete duplicates, keep authoritative version
4. Update imports in consuming code to use explicit `using` statements
5. Use fully qualified names (`CindarsHope.Save.EquipmentSaveData`) in method signatures when ambiguity persists

**Why**: Found 3 duplicate class definitions (EquipmentSaveData, StatusEffectSO) that caused type mismatch errors. Deletingduplicates eliminated entire category of errors.

**How to apply**: When you see "cannot implicitly convert type X to X", always search for duplicate class definitions before trying to cast or convert.

---

## Method: Error Triage from Large Log Files

**Rule**: When PowerShell validation outputs 50+ errors:
1. Read log from line ~966 (where compiler output starts) not from top
2. Look for error CATEGORIES: missing using, missing properties, namespace mismatches
3. Group errors by file and cause (all GameEventBus errors = missing using)
4. Fix by CATEGORY not by error line number (one fix often resolves 10+ errors)
5. Re-run validation and repeat

**Why**: Attempting to fix errors in reading order (top-to-bottom) led to false starts. Categorical grouping showed 15 errors were all the same root cause (missing `using CindarsHope.Core;`).

**How to apply**: Use Grep or PowerShell Select-String to count error patterns: `Select-String -Pattern "error CS"` and group by error code (CS0103 = missing name, CS1061 = missing property, etc).

---

## Method: Editor-Only Code Isolation

**Rule**: When editor scripts fail compilation but don't block SPEC:
1. Check if error is in code under `Assets/_Game/Scripts/Editor/`
2. If it's in initialization/validation code (not core functionality), simplify rather than expand
3. Fix only the minimum needed to unblock: remove dead code, comment problematic initialization
4. Don't try to "make it perfect" - preserve core runtime functionality first

**Why**: EquipmentDataInitializer had properties that don't exist on EquipmentDataSO. Instead of expanding EquipmentDataSO, I removed the assignments - cleaner and unblocked validation.

**How to apply**: Editor code often falls behind during refactors. It's okay to have simpler, reduced functionality in editor-only initialization if it means runtime compiles.

