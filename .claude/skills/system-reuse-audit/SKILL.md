---
name: system-reuse-audit
description: Pre-creation audit — before creating any new manager, service, SO type or system, verify nothing equivalent already exists. Use at Phase 0 of every spec and whenever about to create a class whose name/concept may already exist.
---

# Skill: System Reuse Audit

## Why this skill exists (real incidents in this repo)

- `Scripts/Craft/` (CraftingManager, RecipeDataSO, RecipeDatabaseSO — 12 files) and `Scripts/Crafting/` (CraftingService, RecipeDefinition, RecipeType — 5 files) are TWO parallel crafting systems.
- `StatusEffectSO` exists twice: `Combat/StatusEffectSO.cs` and `Combat/StatusEffect/StatusEffectSO.cs`.
- `SkillActionSO` exists twice: `Combat/Skills/SkillActionSO.cs` and `Skills/SkillActionSO.cs`.

`spec_quality_gate` marks "criou sistema paralelo quando deveria reusar" as `NEEDS_REWORK`. The `runtime-code-guard` hook flags duplicate class names at Write time — this skill is the audit you run BEFORE writing.

## Audit procedure (5 minutes, before creating anything)

```powershell
# 1. Exact name and near-names
git grep -nE "class\s+(\w*)<CoreConcept>(\w*)" -- "Assets/_Game/Scripts/*.cs"

# 2. Concept synonyms (e.g., Craft|Crafting|Recipe|Workshop; Shop|Store|Vendor; Spawn|Materialize)
git grep -lE "<synonym1>|<synonym2>" -- "Assets/_Game/Scripts/*.cs"

# 3. Domain folder listing — does a folder for this domain already exist?
Get-ChildItem Assets\_Game\Scripts -Directory
```

## Decision matrix

| Finding | Action |
|---|---|
| Same concept, active system | **Reuse/extend it.** Wire your spec into it. |
| Same concept, two existing systems (e.g., Craft vs Crafting) | **STOP — report to human.** Do not pick one silently and do not add a third. |
| Similar name, different concept | Rename YOUR new type to remove ambiguity. |
| Nothing found | Create, following domain conventions (`<Thing>DataSO`, service in domain folder). |

## Required evidence in the execution report

```text
## Existing Systems Audit
Searched: <patterns/synonyms used>
Found: <types/folders, with verdict reuse|extend|new|conflict>
Created new: <list + one-line justification each>
Conflicts reported to human: <none | list>
```

A `BUILD_VALIDATED` claim without this section is invalid (spec_quality_gate checklist item 3).
