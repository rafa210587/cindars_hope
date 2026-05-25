# /start-spec

Use when the user requests to start implementing or preparing a spec.

**Arguments expected:** `$ARGUMENTS` - spec number or filename (e.g., "spec_12" or "spec_12_player_combat")

## Objective

Prepare spec execution without implementing code yet.

## Execution Flow

1. Read Camada 0 (minimal):
   - `AGENTS.md` or `CLAUDE.md`
   - `PROJECT_LOG.md` (top/recent entries only)
   - `docs/IMPLEMENTATION_STATUS.md`
   - `docs/operations/AGENT_EXECUTION_PROTOCOL.md`

2. Read Camada 1 (spec-specific):
   - `docs/specs/SPEC_SOURCE_OF_TRUTH.md` (if exists)
   - `docs/specs/SPEC_EXECUTION_ORDER.md` (if exists)
   - Target spec in `docs/specs/a_implementar/spec_*.md`
   - Related pre-refinement in `docs/refinements/a_implementar/pre_refinamentos/` (if exists)
   - Related implemented specs in `docs/specs/implementados/` (only if cited)

3. Identify:
   - Objective and scope
   - Dependencies and blockers
   - Allowed files (in spec scope)
   - Forbidden files (docs_old, Assets, ProjectSettings, etc.)
   - Mandatory validations (docs, Unity compile, log scan)
   - Residual risks
   - Applicable skills from `.claude/skills/`
   - Recommended specialized agent (if complex)

4. Deliver execution plan:
   - Summary (1-2 sentences)
   - Scope boundaries
   - File changes expected
   - Validations required
   - Recommended approach
   - Risks

## Rules

- Do NOT implement code in this command
- Do NOT move spec to implementados/
- Do NOT update status as completed
- Do NOT read full docs_old/ or GDD
- Do NOT amplify scope beyond spec
- Do NOT merge multiple large specs into one task

## Example Output

```
Spec 12: Player Combat/Weapons/Spells
Objective: Implement basic player melee and ranged attacks with combat UI.

Dependencies: SPEC 11 (damage status resistances) - PASS
             SPEC 08 (town NPC dialogue) - independent

Scope:
  Files allowed: Assets/Scripts/Runtime/Combat/*
                 Assets/_Game/Data/Combat/*
                 Relevant Unity scenes

  Files forbidden: Assets/Gameplay/* (outside combat scope)
                   docs_old/**
                   ProjectSettings/**

Validations:
  Mandatory: docs validation, Unity compile validation
  Optional: Play Mode (sandboxed, cannot run)

Skills applicable:
  - SPEC Execution Pattern
  - Unity Validation Skill
  - Event Bus Pattern (weapon/spell events)
  - DamageRequest Construction (attack payload)

Recommended approach:
  1. Create WeaponDataSO for melee/ranged stats
  2. Implement PlayerCombatManager with attack logic
  3. Create DamageRequest in attack event
  4. Add weapon hotbar selection (if UI scope allows)
  5. Validate compilation after each phase

Residual risk: Play Mode features must be tested manually by user later.

Ready to proceed?
```

## Do NOT Continue Into Implementation

Stop here. Wait for user confirmation to advance to actual implementation task.
