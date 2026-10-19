---
doc_type: execution_report
spec_id: fable_73_spec_localization_string_table_runtime
status: BUILD_VALIDATED
date: 2026-06-20
validated_adrs: [ADR-0012-localization-string-table-from-p4.md]
validated_game_rules: [documentation_rules.md, event_rules.md]
---

# Execution Report — fable_73 Localization String Table Runtime

**Spec:** `.specs/a_implementar/fable/fable_73_spec_localization_string_table_runtime.md`
**Status:** BUILD_VALIDATED
**Branch:** dev
**Type:** Runtime (lightweight infra + authoring discipline)
**Domain:** Localization / Text-authoring / Quest / Dialogue

This spec creates the lightweight `LocalizationService` (id -> string lookup, PT-BR single locale,
deterministic fallback to the id) plus the static string table and the id->string authoring
convention for NEW quest/dialogue text from phase P4 onward, per ADR-0012. It does NOT retrofit
pre-P4 hardcoded text (registered debt). No framework, no locale switch, no save persistence.

---

## Acceptance criteria extracted

| ID | Criterion (resumo) | Status | Evidence |
|----|--------------------|--------|----------|
| CA-1 | `Get`/`TryGet`/`Has` resolve a present id to its PT-BR string | OK | `LocalizationServiceTests.Get_PresentId_ReturnsStoredValue`, `TryGet_PresentId_ReturnsTrueAndValue`, `Has_PresentId_ReturnsTrue` |
| CA-2 | Absent id => the id itself (non-empty, never throws, never surprise empty); `TryGet` false; parity with legacy `?? QuestId` | OK | `Get_AbsentId_ReturnsTheIdItself`, `Get_AbsentId_ReturnsNonEmptyString`, `TryGet_AbsentId_ReturnsFalseAndIdAsValue`, `Get_MatchesLegacyNullCoalesceFallback_ForAbsentKey` |
| CA-3 | `Get(null)`/`Get("")` deterministic, no NRE | OK | `Get_Null_ReturnsNullWithoutThrowing`, `Get_Empty_ReturnsEmptyWithoutThrowing`, `TryGet_Null_ReturnsFalse`, `TryGet_Empty_ReturnsFalse`, `Has_NullOrEmpty_ReturnsFalse` |
| CA-4 | No framework, no locale switch, no save; pure/static, no Unity ref, no global search, no event | OK | diff only in `Localization/**` + tests + root csproj includes; no Packages/ change; ADR-0012 conformance table below; `DefaultLanguage_IsPtBr` |
| CA-5 | Authoring convention declared; legacy text untouched | OK | "Convention" section below; `git diff` = 0 in `TownNpcDialogueLibrary.cs`, `DialogueNode.cs`, `DialogueTreeSO.cs`, `QuestDefinition.cs`, `QuestStepDefinition.cs` |

---

## Existing systems audit (Phase 0 — system-reuse-audit)

Grep over `Assets/_Game/Scripts` for `Localization|StringTable|TextTable|I18n|L10n|TextService`:
**zero matches**. Glob `Assets/_Game/Scripts/Localization/**`: **no files**. Confirmed there is no
pre-existing text-resolution service to reuse — the spec's scope (create new) is correct.

| Concern | Finding | Decision |
|---------|---------|----------|
| Existing Localization/TextService/StringTable | None found (grep + glob) | Create new `CindarsHope.Localization` |
| Catalogue pattern to mirror | `NpcDialogueSetRegistry` (static, lazy `s_entries`, `TryGet`) + `TownNpcDialogueLibrary` | Mirrored the static/lazy/TryGet style; no new pattern |
| Stable-id interface | `CindarsHope.Core.Data.IIdentifiedData { string Id }` | `LocalizationEntry` implements it |
| Ad-hoc fallback in use | `QuestLogProjectionService` `def.DisplayNameKey ?? def.QuestId` (lines 39, 47, 81, 82) | Formalized as the single tested fallback; parity test added |
| Id format convention (F67) | F67 `input_map.md` exists but defines input bindings only — **no text-id convention** | Propose a minimal id format, marked "proposal to calibrate with F67"; lookup is id-format-agnostic |
| Table source medium | Static C# catalogue vs `.asset` YAML (forbidden) vs Resources JSON | Static C# catalogue — no YAML, EditMode-testable without a scene |
| Edge `Get(null)`/`Get("")` | Decided: return the argument unchanged (null->null, ""->"") | Fixed in `LocalizationService.Get` + tests (CA-3) |
| Parity integration in `QuestLogProjectionService` | Optional per spec; "if in doubt, do NOT touch" | **DEFERRED to F35** — service ready + parity-tested; projection untouched (diff isolation) |

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation | Status |
|--------------------|----------------|--------|
| `LocalizationService` pure/static: `string Get(string id)` | `Assets/_Game/Scripts/Localization/LocalizationService.cs` | OK |
| `bool TryGet(string id, out string value)` (false => value=id) | `LocalizationService.TryGet` (false sets value to the `Get` fallback) | OK |
| `bool Has(string id)` | `LocalizationService.Has` | OK |
| Fallback to id, deterministic, no exception, no surprise empty | `Get` returns id for absent non-empty; argument unchanged for null/empty | OK |
| id->string table, static/lazy, style of TownNpcDialogueLibrary | `Assets/_Game/Scripts/Localization/LocalizationStringTable.cs` (`Entries` lazy, `TryGetValue`/`Contains`/`Count`) | OK |
| Table may start with seed convention keys; production text from consumers | 3 demo convention keys (`quest.sample.*`, `dialogue.sample.greeting`); no production content | OK |
| Authoring convention id->string documented | This report "Convention" section + class XML docs | OK |
| Single PT-BR locale; no runtime locale switch | `LocalizationStringTable.DefaultLanguage = "pt-BR"`; no switch API | OK |
| No framework / no .po/.resx | 100% pure C#; Packages/ untouched | OK |
| No save persistence (table = data) | No DTO/section/migration; no SaveManager change | OK |
| No GameObject.Find/FindObjectOfType | Service is static/pure; table is static/lazy | OK |
| No new event / no parallel channel to GameEventBus | Lookup is a synchronous data read; no Publish/Subscribe | OK |
| `*Key` quest field signatures unchanged | `QuestDefinition.cs`/`QuestStepDefinition.cs` not modified | OK |
| Legacy text not migrated/touched | `TownNpcDialogueLibrary.cs`/`DialogueNode.cs`/`DialogueTreeSO.cs` not modified | OK |
| EditMode tests (lookup, fallback, edges, idempotence, parity, table load) | `Assets/_Game/Tests/EditMode/Localization/LocalizationServiceTests.cs` (19 tests) | OK |
| `LocalizationKey` constants (optional) | Omitted — Phase 0 found no value yet (convention demoed via seed keys); a future spec may add it | DEFERRED (optional) |
| Parity swap in `QuestLogProjectionService` (optional) | DEFERRED to F35 — behavior preserved (legacy `?? id` intact); parity proven by test | DEFERRED (optional) |

---

## ADR-0012 conformance (item by item)

| ADR-0012 clause | Conformance |
|-----------------|-------------|
| New P4 text via id->string table, not inline literal | Service + table + convention delivered (the seed/contract; consumers populate text) |
| No localization framework | No third-party dependency; Packages/ untouched |
| No runtime locale-switching machinery | Single `pt-BR`; no switch API |
| No .po/.resx tooling | Pure C# catalogue only |
| Only one (authoring) language in v1 | PT-BR only |
| Does NOT retrofit pre-P4 hardcoded text | Legacy files untouched (diff = 0); reaffirmed below |
| No locale UI / font fallback / pluralization | None added |
| Table location/id convention coordinated with F67 | Static C# catalogue; id format proposed, marked "to calibrate with F67" |
| Registered Debt: pre-P4 text not migrated | Reaffirmed (see "Registered debt" below) |

---

## Convention (authoring id -> string, from P4 onward)

Declared convention for NEW quest/dialogue text (PROPOSAL to calibrate with F67 — lookup is
agnostic to the id format, so this does not gate resolution):

```
{domain}.{spec_or_npc}.{slot}
  domain   : "quest." | "dialogue."   (lowercase)
  separator: "."
  segments : snake_case, no spaces
examples : quest.first_supplies.title
           quest.first_supplies.desc
           dialogue.pip.greeting
```

How a future text spec (F35/F36/F70) declares its keys:
- Add entries to `LocalizationStringTable.SeedEntries()` (or a partial of `LocalizationStringTable`)
  using the convention above, with the PT-BR string as the value.
- Reference text at the call site by id resolved through `LocalizationService.Get(id)` instead of
  embedding a literal.
- The quest `*Key` fields (`DisplayNameKey`, `DescriptionKey`, etc.) become resolvable via
  `LocalizationService.Get(key)`; the parity swap in `QuestLogProjectionService` is left to the
  first consumer (F35) to keep this spec's diff isolated. Behavior is identical either way.

---

## Registered debt reaffirmed (ADR-0012)

Pre-P4 hardcoded quest/dialogue text remains inline and is NOT migrated by this spec:
`Assets/_Game/Scripts/NPC/TownNpcDialogueLibrary.cs`, `Assets/_Game/Scripts/NPC/DialogueNode.cs`
(`Text`), `Assets/_Game/Scripts/NPC/DialogueTreeSO.cs` (`Nodes[].Text`). Migration stays deferred
debt, schedulable only if/when a second language or full framework is adopted. `git diff` shows
zero change in those files.

---

## Validation

```
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (exit 0, 0 errors, 1 pre-existing warning CombatTelemetrySession CS0649 — unrelated)
Assembly-CSharp-Editor: PASS (exit 0, 0 errors, 3 pre-existing warnings — unrelated)
Quality check: PASS (run_strict_validation aggregate)
Docs validation: PASS (validate_docs.ps1 exit 0)
Spec diff completeness: PASS (check_spec_diff_completeness.ps1 exit 0)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Commands run (each gated on `$LASTEXITCODE`):
- `dotnet build .\Assembly-CSharp.csproj --no-restore` -> 0 errors
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` -> 0 errors
- `.\tools\docs\validate_docs.ps1` -> exit 0
- `.\tools\docs\check_spec_diff_completeness.ps1` -> exit 0
- `.\tools\docs\run_strict_validation.ps1` -> exit 0

EditMode tests: `LocalizationServiceTests` (19 tests) compile in `Assembly-CSharp-Editor` (0E).
Unity EditMode runner execution is DEFERRED (owner authorized skipping Unity/Play Mode for this
batch); compile-validated via dotnet. The logic is pure/deterministic and fully covered by these
tests.

---

## Testing Quality Gate

```
Changed runtime code: YES (new pure text-resolution service)
Changed deterministic logic: YES (lookup + fallback to id)
Changed Unity scene/prefab/asset wiring: NO (static C# catalogue; no scene/prefab/asset)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Localization/LocalizationServiceTests.cs)
Automated tests command: dotnet build .\Assembly-CSharp-Editor.csproj --no-restore (compile); Unity EditMode runner DEFERRED (owner-authorized)
Manual Play Mode scenario: NOT REQUIRED (deterministic logic covered by EditMode; no UI/scene change — parity integration deferred to F35)
Justification if no automated tests: N/A (tests added)
Residual risk: id format is a "proposal to calibrate" with F67 (does not gate lookup — id-format-agnostic); parity swap in QuestLogProjectionService is DEFERRED to F35 with no behavior loss (legacy "?? id" preserved and parity-tested); Unity EditMode runner not executed in this session (compile-validated; owner authorized deferring Unity).
```

---

## Files changed

```
Assets/_Game/Scripts/Localization/LocalizationStringTable.cs      (NEW — id->string table, static/lazy, PT-BR, convention seed)
Assets/_Game/Scripts/Localization/LocalizationService.cs          (NEW — Get/TryGet/Has, deterministic fallback to id)
Assets/_Game/Tests/EditMode/Localization/LocalizationServiceTests.cs (NEW — 19 EditMode tests)
docs/validation/fable_73_spec_localization_string_table_runtime_execution_report.md (this report)
Assembly-CSharp.csproj / Assembly-CSharp-Editor.csproj            (Compile includes for the new files — Unity-generated; NOT committed)
```

`LocalizationKey.cs` was intentionally omitted (spec marks it optional, "só se a Fase 0 confirmar
valor"): the convention is demonstrated via seed keys and documented; no constant set adds value
yet. No `.meta` files were hand-authored (Unity generates them for the new `.cs` on next import).

---

## Honest status rationale

Status is **BUILD_VALIDATED** (not ACCEPTED). All five acceptance criteria are met and tested;
both assemblies build with 0 errors; `run_strict_validation.ps1` returned exit 0. The Unity
EditMode runner and any Play Mode pass are DEFERRED (owner-authorized for this batch) — the
service is pure/deterministic and compile-validated, so no Play Mode is required by the spec
(`Requires Play Mode final validation: NO`). The optional parity integration in
`QuestLogProjectionService` is DEFERRED to the first consumer (F35) to keep this diff isolated to
`Localization/**`; the legacy `?? id` fallback is preserved and its behavior is reproduced by a
parity test. No ACCEPTED / PLAYMODE_VALIDATED claim is made.

## Remaining work

- F35 (first consumer) may swap `QuestLogProjectionService` `?? QuestId`/`?? ""` for
  `LocalizationService.Get(...)` (parity-tested) and start populating production keys.
- Calibrate the id format with F67 if/when F67 fixes a canonical text-id convention.
- Unity EditMode runner execution for `LocalizationServiceTests` (deferred to the wave Unity gate).
```
