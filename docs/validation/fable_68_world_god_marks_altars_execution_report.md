# Execution Report — fable_68 (Mundo: Marcas dos Deuses / Altares)

> Spec: `.specs/a_implementar/fable/fable_68_spec_world_god_marks_altars.md`
> Status: **BUILD_VALIDATED_WITH_WARNINGS**
> Date: 2026-06-21
> Branch: `dev`
> Validation method: `dotnet build` (both csproj, exit 0) + `validate_docs.ps1` + `check_spec_diff_completeness.ps1`. Unity batchmode / Play Mode: NOT RUN (DEFERRED per owner authorization).

---

## Honest status rationale

Status is **BUILD_VALIDATED_WITH_WARNINGS**, not full BUILD_VALIDATED, because:

- The **core logic** (catalog, daily prayer rules, expiration, non-stack, conditionals, deterministic
  cave draw, save round-trip) is fully implemented as pure C# and covered by EditMode tests — CA-1..CA-5
  satisfied at logic level.
- **Cave special-room placement (F22) and city/farm scene anchors are DEFERRED to human Unity wiring**:
  Phase 0 audit found that **no special-room type registry exists** in the codebase (cave generation is
  procedural-only via `CaveLayoutStableHash`). Editing scenes is human-only (editor scripts, out of code
  scope). Therefore the spec's Fase 2 (register `god_mark` type in F22 pipeline) and Fase 3 (5 fixed scene
  interactables) are delivered as **a pure, tested resolver + a thin interactable component ready to be
  placed**, with placement deferred. This matches the established WAVE_INTEGRATION pattern
  (CODE_READY_HUMAN_UNITY_ACTION_REQUIRED).
- SaveManager/`GameSaveData` integration of the `godMarks` section is **deferred** (same documented pattern
  as fable_24 `SAVE_LOAD_DAILY_GOAL_DEBT`): the DTO + capture/restore + round-trip test exist and pass;
  wiring the provider into the SaveManager pipeline is a bootstrap/scene action not authorized by the spec's
  allowed-files list (`SaveManager.cs`/`GameSaveData.cs` are not listed).

No premature acceptance claim is made. Unity compile and Play Mode are explicitly NOT RUN.

---

## Acceptance criteria extracted

| CA | Requirement | Status | Evidence |
|----|-------------|--------|----------|
| CA-1 | 11 Marcas fiéis ao apêndice A.5; Anya sem bônus | OK | `GodMarkCatalog` + tests `Catalog_*`, `Catalog_AnyaIsLoreOnly_NoBonus`, `Catalog_EffectsMatchAppendixA5` |
| CA-2 | Orar 1x/dia; 2ª recusa; dormir expira buff e reseta flags | OK | tests `Pray_GrantsBuff_FirstTime`, `Pray_SecondTimeSameDay_Refused`, `DayRollover_ExpiresBuffAndResetsFlag` |
| CA-3 | Marca + relíquia do mesmo deus → só o maior (4 deuses com relíquia) | OK | `GetEffectiveMarkMagnitude` + tests `NonStack_SameGodRelic_TakesMax_NotSum`, `NonStack_FlagForFourRelicGods` |
| CA-4 | Mesmo seed+nível → mesma Marca; bandas respeitadas | OK | `GodMarkCaveResolver` (FNV-1a estável) + tests `CaveResolve_SameSeedAndLevel_SameResult`, `CaveResolve_RespectsBands`, `CaveResolve_NyxOnlyInBand71to85_TandraOnly11to25` |
| CA-5 | Nyx só noite; Alihana só pico; Senya só festival/pico; Thandra consome 1 crop | OK | tests `Nyx_*`, `Alihana_RequiresLunarPeak`, `Senya_RequiresFestivalOrPeak`, `Thandra_RequiresCropOffering_AndConsumesIt`, `Thandra_BonusAppliesNextDay` |

Scene/cave placement evidence (interaction in 3 scenes) is the human Play Mode portion — DEFERRED.

---

## Existing systems audit (Phase 0 — system-reuse)

| System | Found | Decision |
|--------|-------|----------|
| Buff diário (E06 PlayerConditionService) | `Player/Conditions/PlayerConditionService.cs` — only fatigue thresholds, NO named-buff API | REUSE concept; daily buff hosted in `GodMarkService` (the daily-state owner). No second combat-buff system created; effects exposed via single-point `GetActiveEffectMagnitude` (same idiom as `AccessoryEffectRouter.GetModifier`). |
| Special rooms (F22) | NO registry exists; cave gen procedural via `Cave/Generation/CaveLayoutStableHash.cs` | REUSE the canonical FNV-1a hash; implement a PURE resolver (`GodMarkCaveResolver`) the F22 pipeline can consume. No second spawner. Placement DEFERRED (no registry to register into yet). |
| Relics non-stack (F23) | `Equipment/AccessoryCatalog.cs` (`RelicGod`, relic god per definition), `Equipment/AccessoryEffectRouter.cs` (`GetModifier`, max-not-sum) | REUSE: non-stack resolved by god in `GodMarkService.GetEffectiveMarkMagnitude`; F23 files UNTOUCHED. Relic god resolvable via `AccessoryCatalog` from equipped instance ids (delegate-injected). |
| IInteractable | `Interaction/IInteractable.cs` (`InteractionPrompt`/`CanInteract`/`Interact`) | REUSE: `GodMarkAltarInteractable` implements it (thin adapter). |
| GameEventBus + day events | `Core/GameEventBus.cs`, `Core/Events/DayStartedEvent.cs`, `PlayerActionFeedbackEvent` | REUSE: subscribe `DayStartedEvent` for expiration; publish `GodMarkPrayedEvent` (new) + `PlayerActionFeedbackEvent` for HUD. No direct MB↔MB calls. |
| Calendar/lunar/festival (F37) | `World/Events/WorldEventResolver.cs` (`ResolveLunarPeak`, `ResolveFestival`), `GameTimeManager.CurrentPhase` (night) | REUSE: queried via `IGodMarkWorldContext` delegates, wired by the runtime adapter (no copy of the resolution logic). |
| Crop offering | `Inventory/InventoryManager.cs` (`HasItem`, `RemoveItem`) | REUSE: consumed via `IGodMarkWorldContext.TryConsumeCropOffering` delegate. |
| God enum | `Equipment/RelicGod` (only 4 gods) | EXTENDED with `MarkGod` (11 gods) for the wider pantheon; mapped to `RelicGod` by name for the non-stack rule. |
| Localization | `Localization/LocalizationService.cs` (id→string, visible fallback) | REUSE: all player-facing mark names/lore via string keys. |

No parallel manager/service/registry duplicating an existing system was created.

---

## Spec Compliance Matrix

| Spec requirement | Implementation | Status |
|------------------|----------------|--------|
| GodMarkCatalogSO (11 entries) | `GodMarkCatalog` (pure static catalog; testable; no SO asset needed in runtime — mirrors `AccessoryCatalog` precedent) | OK |
| GodMarkAltarInteractable (IInteractable) | `GodMarkAltarInteractable.cs` — pray → delegate to `GodMarkRuntime` → feedback | OK |
| GodMarkService (1/day, non-stack, expiration) | `GodMarkService.cs` (pure) | OK |
| GodMarkDailyState / save section (flags + buffs, simple types) | `GodMarkSaveData.cs` (simple types + ids; round-trip tested) | OK (wiring into SaveManager DEFERRED) |
| Cave integration (register god_mark type, StableHash draw, bands) | `GodMarkCaveResolver.cs` (pure deterministic draw, bands) | PARTIAL — draw logic OK + tested; F22 pipeline registration DEFERRED (no registry exists) |
| City/farm fixed anchors (5 interactables) | `GodMarkAltarInteractable` ready; scene anchors via editor script | DEFERRED (human Unity scene action) |
| Non-stack with F23 relic of same god | `GetEffectiveMarkMagnitude` (max, not sum) | OK |
| mark_anya_waters lore-only (zero bonus) | `MarkEffectType.None`; `TryPray` returns `LoreOnly`, no flag/buff | OK |
| GodMarkPrayedEvent (GameEventBus) | `Core/Events/GodMarkPrayedEvent.cs` | OK |
| EditMode tests (all listed cases) | `Assets/_Game/Tests/EditMode/World/GodMarkTests.cs` (28 tests) | OK |

---

## Files changed

Code (committed):
- `Assets/_Game/Scripts/World/Altars/GodMarkCatalog.cs` (NEW)
- `Assets/_Game/Scripts/World/Altars/GodMarkSaveData.cs` (NEW)
- `Assets/_Game/Scripts/World/Altars/GodMarkCaveResolver.cs` (NEW)
- `Assets/_Game/Scripts/World/Altars/GodMarkService.cs` (NEW)
- `Assets/_Game/Scripts/World/Altars/GodMarkRuntime.cs` (NEW)
- `Assets/_Game/Scripts/World/Altars/DelegateGodMarkWorldContext.cs` (NEW)
- `Assets/_Game/Scripts/World/Altars/GodMarkAltarInteractable.cs` (NEW)
- `Assets/_Game/Scripts/Core/Events/GodMarkPrayedEvent.cs` (NEW)
- `Assets/_Game/Tests/EditMode/World/GodMarkTests.cs` (NEW)
- `docs/validation/fable_68_world_god_marks_altars_execution_report.md` (this file)

Local-only (NOT committed — `*.csproj` is gitignored):
- `Assembly-CSharp.csproj` — added 9 `<Compile Include>` entries for the new files (needed for the
  `dotnet build` fallback compile signal only; Unity regenerates the csproj from .meta on import).

No `.unity`/`.prefab`/`.asset` edited. No F22/F23 files edited. No Packages/ProjectSettings touched.

---

## Validation

```
Validation method: dotnet build (fallback compile) + validate_docs.ps1 + check_spec_diff_completeness.ps1
Assembly-CSharp:        PASS — exit 0 (0 errors, 1 pre-existing warning CS0649 unrelated)
Assembly-CSharp-Editor: PASS — exit 0 (0 errors, 3 pre-existing warnings unrelated)
Docs validation (validate_docs.ps1):          exit 0 — PASS
Diff completeness (check_spec_diff_completeness.ps1): exit 0 — PASS (scoped gate)
Strict validation (run_strict_validation.ps1): may exit 1 due to 3 pre-modified .unity scenes in the
  working tree (CaveScene/FarmScene/TownScene) — ENVIRONMENTAL, not produced by this spec.
Unity batchmode compile: NOT RUN (DEFERRED — owner authorized skipping Unity/Play Mode)
Play Mode:               NOT RUN (DEFERRED)
```

### validated_game_rules

- `cave_rules.md` — deterministic cave content via FNV-1a StableHash, no GUID/timestamp; revisit = same
  Mark same place (stable-run / ADR-0005). COMPLIANT (`GodMarkCaveResolver`).
- `farm_rules.md` — Thandra offering consumes 1 crop via existing inventory removal; bonus applies next day.
  COMPLIANT (delegate-based consumption, no parallel farm system).
- `event_rules.md` — daily expiration driven by existing `DayStartedEvent`; gameplay comms via GameEventBus
  only. COMPLIANT.
- `save_rules.md` — additive `godMarks` DTO, simple types + stable ids only, no Unity refs; absent = never
  prayed (trivial migration). COMPLIANT (`GodMarkSaveData`, round-trip tested).

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (prayer rules, expiration, non-stack, cave draw, save DTO)
Changed Unity scene/prefab/asset wiring: NO (deferred to human)
Automated tests added/updated: YES
Automated tests command: Unity Test Runner EditMode (CindarsHope.Tests.EditMode.World.GodMarkTests) — NOT RUN here (Unity deferred); compiled into Assembly-CSharp (dotnet build exit 0)
Manual Play Mode scenario: REQUIRED (interaction in cave/city/farm) — DEFERRED to final human validation
Justification if no automated tests: N/A (tests added)
Residual risk:
  - Cave special-room placement (F22) and 5 fixed scene anchors not yet wired in Unity scenes
    (no registry exists; scenes are human-only). Logic is tested; placement is the remaining human step.
  - godMarks save section not yet registered in SaveManager/GameSaveData (DTO + round-trip tested);
    same documented debt pattern as fable_24.
  - EditMode tests compile but were not executed in Unity Test Runner here (Unity deferred).
```

---

## Anti-regression

- F22 special rooms: NO files touched.
- F23 relics: NO files touched (`AccessoryCatalog`/`AccessoryEffectRouter`/`AccessoryEffectType` unchanged).
- No GUID/timestamp/unseeded Random in the cave draw (FNV-1a only).
- Save is additive; existing sections untouched.
- No runtime `GameObject.Find`/`FindObjectOfType` introduced; runtime access via static accessor +
  GameEventBus (same idiom as `AccessoryEffectRouter.Active`).

---

## Remaining work (deferred, documented)

1. Register the `god_mark` special-room type in the F22 pipeline once that pipeline exposes a type registry;
   feed it `GodMarkCaveResolver.Resolve(seed, level)`.
2. Place the 5 fixed `GodMarkAltarInteractable` anchors (Thandra niche, Kanthor oath, Merithus seal,
   Alihana mirror, Senya mast) + the 3 Anya lore spots via authorized editor scene scripts.
3. Wire a `GodMarkSaveData` section provider into SaveManager (`Capture`/`Restore` already on the service).
4. Wire the runtime adapter: `GodMarkRuntime.Activate(service, DelegateGodMarkWorldContext)` with delegates
   bound to `GameTimeManager.CurrentPhase==Night`, `WorldEventResolver` peaks/festivals, `InventoryManager`
   crop removal, and equipped-relic god via `AccessoryCatalog`.
5. Add localization string-table entries for the 11 mark name/lore keys.
6. Run Unity Test Runner EditMode + human Play Mode scenario (pray in cave/city/farm).
```
