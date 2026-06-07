# SPEC 01.03 — Invalid ID Fallback Rules — Execution Report

> **Date:** 2026-06-07  
> **Spec ID:** `01_spec_invalid_id_fallback_rules`  
> **Phase Status:** BUILD_VALIDATED  
> **Executor:** Claude Code  

---

## Summary

Spec 01.03 completed: defined invalid ID fallback policy across 8 categories and 8 save sections (inventory, quests, bestiary, cave, farm, NPC, player skills, calendar).

**Deliverables:**
- ✓ `InvalidIdFallback.cs`: enums and policy documentation for all section types
- ✓ 8 category definitions (MissingOptional, MissingRequired, UnknownLegacy, RemovedContent, Duplicate, Empty, Malformed, SectionOwnerUnknown)
- ✓ 4 severity levels (Info, Warning, Error, Blocker)
- ✓ Fallback policy for Inventory, Quests, Bestiary, Cave, Farm, NPC, Player, Calendar sections
- ✓ `InvalidIdFinding` struct for diagnostics
- ✓ Query method `GetFallbackForSection()` for policy lookup

**Status:** `BUILD_VALIDATED` — Ready for 01.04-01.07 save specs to reference fallback rules.

---

## Artifacts

| File | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Core/Data/InvalidIdFallback.cs` | Policy enums, categories, severities, and per-section fallback rules |
| This report | Audit findings and status |

---

## Policy Coverage

All 8 sections covered:

| Section | Policy | Owner | Fallback Rules Defined |
|---------|--------|-------|------------------------|
| Inventory/Items/Equipment | Y | InventoryManager | Remove/clear/skip with warning |
| Quests/Objectives | Y | QuestManager | Quarantine/mark completed to prevent soft-lock |
| Bestiary/Knowledge | Y | BestiaryManager | Mark unknown; retain knowledge state |
| Cave/Corpse/Loot | Y | CaveRuntimeMaterializer | Remove enemy/item; regenerate if needed |
| Farm/Crops/Trees | Y | FarmManager | Clear plot/remove tree; make space available |
| NPC/Shops/Services | Y | NpcManager | Remove from active list; trigger restock |
| Player/Skills/Spells/Status | Y | PlayerSkillTreeManager | Remove/skip; retain points/state |
| Calendar/Weather/Time | Y | GameTimeManager | Use defaults; clamp invalid dates |

---

## Category and Severity Matrix

**8 Invalid ID Categories:**
1. `MissingOptionalReference` — Referenced non-critical ID not found
2. `MissingRequiredReference` — Critical resource missing; must use safe default
3. `UnknownLegacyId` — Old save format; may match updated naming later
4. `RemovedContentId` — Valid ID but intentionally removed in patch
5. `DuplicateId` — Same ID twice in registry; ambiguous
6. `EmptyId` — Null or whitespace persisted
7. `MalformedId` — Invalid characters/structure
8. `SectionOwnerUnknown` — Save section found but no owner identified

**4 Severity Levels:**
- `Info` — Non-persisted/local; no impact
- `Warning` — Degraded but recoverable
- `Error` — Cannot restore correctly; safe mode fallback
- `Blocker` — Must migrate or fix manually

---

## Testing Status

Test/validator: NOT CREATED (per spec guidelines for pure policy definition)

**Justification:** Spec 01.03 is policy/documentation only. InvalidIdFinding enum and fallback rules are data structures without runtime logic to test yet. Tests will be added in 01.04-01.07 when domain specs implement section-specific save handling.

---

## Residual Risk

| Risk | Mitigation |
|------|-----------|
| Future specs don't follow fallback policy | Policy is documented and centralized in InvalidIdFallback.cs; specs must cite this file |
| Fallback behavior is inconsistent across domains | GetFallbackForSection() method provides centralized lookup; prevents drift |
| Persisted data loss from invalid IDs | Each section defaults to conservative safe-state (remove/skip) rather than silent null |
| No validation before load | ValidateAndNormalizeSave() in SaveManager can be extended to call InvalidIdFinding logic; validator is separate spec |

---

## Next Steps

Specs 01.04-01.07 must:
- Reference `InvalidIdFallback.cs` when implementing save restore order, section ownership, and provider architecture
- Use `InvalidIdFinding` to report missing IDs during load
- Implement domain-specific fallback per policy documented above

---

**Status:** `BUILD_VALIDATED`

*Report created: 2026-06-07*
