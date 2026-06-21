# Execution Report — fable_14 UI Canvas Screens Integration (Runtime)

> Spec: `.specs/a_implementar/fable/fable_14_spec_ui_canvas_screens_integration_runtime.md`
> Date: 2026-06-21
> Branch: `dev`
> **Status: BUILD_VALIDATED_WITH_WARNINGS** (deterministic core shipped + EditMode tests;
> Canvas screen views / ShopCanvas refactor / live pause wiring DEFERRED_TO_FINAL_VALIDATION —
> require Unity Play Mode, explicitly out of bounds for this session).
> Human scenario: [docs/validation/playmode/fable_14_human_test_scenario.md](playmode/fable_14_human_test_scenario.md)

validated_adrs: [ADR-0013-input-keyboard-mouse-only-v1.md, ADR-0014-single-difficulty-v1.md]
validated_game_rules: [ui_modal_rules.md]

---

## Scope decision (honest)

fable_14 is a large UI/Canvas spec whose acceptance ultimately requires Unity Play Mode
(spec: "Requires Play Mode final validation: YES (obrigatório para UI)"). This session runs
under an explicit constraint: no Unity Editor, no Play Mode, target status
BUILD_VALIDATED / _WITH_WARNINGS.

Therefore this slice delivers the **deterministic, EditMode-testable core** of the spec —
the parts that are pure C# logic and the genuinely durable contracts — and honestly defers
the Canvas MonoBehaviour views, the `RuntimeUiBuilder`/ShopCanvas refactor, and the live
`ModalManager → GameTimeManager` pause wiring to the final Unity validation pass. No status
inflation: those deferred items are NOT claimed as done.

Shipped this session (pure C#, no UnityEngine dependency, EditMode-tested):

- `UiFocusController` — deterministic linear keyboard focus order (arrows/Tab/Enter/Esc),
  wrap-around, visual-outline query (`IsFocused`). CA-2 focus order core.
- `ModalPauseGate` — single pause gate (EMENDA V3 decision 4.1): freeze the day clock while
  any modal is active, resume on empty; emits rising/falling edges only.
- `GameplayScreenTab` + `GameplayScreensPanelModel` — the binding 9-tab panel state (tab
  order, Tab cycle, direct-shortcut→tab mapping reusing the existing `ModalType`s,
  placeholder classification, canonical empty-state strings). CA-1 routing + CA-4 core.
- `SkillNodePurchaseFlow` — CA-2 canonical rule: a skill point can never be spent without
  detail-then-confirm.
- `QuestLogSpoilerProjection` — CA-2 / anti-regression: quest log never reveals hidden quest
  detail; reuses existing `QuestLogViewModel`.

Deferred (build-only, see human scenario): the 4 Canvas screen views, `RuntimeUiBuilder`
extraction + ShopCanvas refactor, `GameplayScreensCanvasBootstrap` MonoBehaviour, IMGUI
`LegacyImguiPanelsEnabled` flag flip, and the live runtime adapter that drives
`ModalPauseGate` from `ModalManager.HasActiveModal` into `GameTimeManager`.

## Acceptance criteria extracted

| CA | Requirement | This session | Evidence |
|----|-------------|--------------|----------|
| CA-1 | 4 screens open via I/K/U/J, Esc closes, gameplay blocked, IMGUI off | PARTIAL — tab/open/close/routing model + ModalType mapping shipped & tested; Canvas views + IMGUI-off flag DEFERRED | `GameplayScreensPanelModelTests`; human scenario §1-3 |
| CA-2 | Focus order; skill detail-before-buy; compare never equips on focus; drop confirmation; quest log hides secret | PARTIAL — focus order, skill purchase flow, quest spoiler shipped & tested; equipment-compare view + drop-confirm view DEFERRED | `UiFocusControllerTests`, `SkillNodePurchaseFlowTests`, `QuestLogSpoilerProjectionTests`; scenario §4-9 |
| CA-3 | RuntimeUiBuilder; ShopCanvas identical after refactor | DEFERRED — builder extraction not done this session (requires Unity hierarchy equivalence test) | scenario §11 |
| CA-4 | Empty states (inventory/quests/skills) | PARTIAL — canonical message constants + classification shipped & tested; Canvas rendering DEFERRED | `GameplayScreensPanelModel` constants; scenario §8 |
| EMENDA V3 4.1 | Single pause gate freezes day clock with UI open | PARTIAL — gate logic shipped & tested; live ModalManager→GameTimeManager wiring DEFERRED | `ModalPauseGateTests`; scenario §10 |
| EMENDA 9-tab | 9-tab panel, Tab cycles, placeholders | DONE (model) — order + cycle + placeholder classification shipped & tested | `GameplayScreensPanelModelTests` |

## Existing systems audit

System-reuse audit (Phase 0) performed before writing any new type:

- `ModalManager` + `ModalType` enum (`Assets/_Game/Scripts/UI/Modal/ModalManager.cs`) —
  REUSED, not duplicated. New model maps to existing `ModalType.Inventory/CharacterEquipment/
  SkillTree/QuestLog`; no second modal stack created.
- `UIFocusRouter` / `ModalStackRouter` / `UIFocusState`
  (`Assets/_Game/Scripts/UI/Input/InputFocusModalRoutingModel.cs`) — the input-blocking and
  focus-state contract already exists and is REUSED conceptually. `UiFocusController` is a
  distinct concern (per-screen element navigation order), not a duplicate of the modal stack
  router (which manages focus *states*). No overlap of responsibility.
- `QuestLogViewModel` / `QuestLogEntryViewModel` (`Assets/_Game/Scripts/UI/Quest/`) — REUSED
  as the input to `QuestLogSpoilerProjection`; the existing `IsHidden` flag drives masking.
- `GameplayHudBootstrap` (`Assets/_Game/Scripts/UI/HUD/`) — bootstrap pattern noted as the
  template for the (deferred) `GameplayScreensCanvasBootstrap`.
- `CreateMvpTownScene.CreateShopUi` (Editor) — the canonical programmatic Canvas builder
  noted as the source to extract `RuntimeUiBuilder` from (deferred refactor).
- IMGUI controllers (`InventoryPanelController`, `CharacterEquipmentPanelController`,
  `SkillTreeGameplayPanelController`, `QuestLogPanelController`) — AUDITED as the source of
  business logic to consume; not modified this session (IMGUI-off flag deferred).

No new manager/service/singleton created. No second modal/EventSystem/pause mechanism.

## Spec Compliance Matrix

| Requirement | Implementation | Status |
|-------------|----------------|--------|
| Deterministic focus order navigable by keyboard | `UiFocusController` (linear, wrap, outline query) | OK |
| Single pause gate (no parallel pause path) | `ModalPauseGate` (one boolean from modal-active) | OK (logic) |
| 9-tab panel, Tab cycle, direct-shortcut → tab | `GameplayScreensPanelModel` + `GameplayScreenTab` | OK |
| Same keys/ModalTypes preserved | `TryGetTabForModalType` / `GetModalTypeForTab` reuse existing enum | OK |
| Skill point never spent without detail+confirm | `SkillNodePurchaseFlow` (Browsing→Detail→Confirm) | OK |
| Quest log never reveals hidden quest | `QuestLogSpoilerProjection` (masked name + objective) | OK |
| Canonical empty-state strings | constants on `GameplayScreensPanelModel` | OK |
| Placeholder tabs ("em breve") | `IsPlaceholderTab` | OK |
| 4 Canvas screen views | DEFERRED (Play Mode) | DEFERRED_UI_VISUAL |
| RuntimeUiBuilder + ShopCanvas refactor | DEFERRED (Play Mode hierarchy equivalence) | DEFERRED_UI_VISUAL |
| Live ModalManager→GameTimeManager pause wiring | DEFERRED (Play Mode) | DEFERRED_UI_VISUAL |
| IMGUI behind LegacyImguiPanelsEnabled flag | DEFERRED (paired with Canvas views) | DEFERRED_UI_VISUAL |
| No `.unity`/`.prefab`/`.asset` edits | none made | OK |
| No GameObject.Find/FindObjectOfType in new code | new types are pure C# | OK |

## Validation

Validation method: per-spec scoped gates + `run_strict_validation.ps1`.

- Docs validation (`validate_docs.ps1`): **PASS** (exit 0).
- `dotnet build Assembly-CSharp.csproj --no-restore`: **PASS** — 0 Erros (1 pre-existing
  warning in `CombatTelemetrySession.cs`, unrelated).
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`: **PASS** — 0 Erros (3
  pre-existing warnings, unrelated).
- `check_spec_diff_completeness.ps1`: **PASS** (scoped gate).
- `run_strict_validation.ps1`: **exit 1 — ENVIRONMENTAL, not this spec.** Cause: (a) 3
  `.unity` scenes (`CaveScene`, `FarmScene`, `TownScene`) pre-modified in the working tree
  before this session (not touched here), flagged as "forbidden files altered"; (b)
  pre-existing report-section WARNs across many historical reports. None of the failing
  items belong to fable_14. The four fable_14-relevant sub-gates (docs, Assembly-CSharp,
  Assembly-CSharp-Editor, diff completeness) all PASS.

Automated tests: `Assets/_Game/Tests/EditMode/UI/Runtime/GameplayScreensRuntimeTests.cs`
(5 fixtures: focus controller, pause gate, panel model, skill purchase flow, quest spoiler).
Tests compile into Assembly-CSharp (build PASS). Unity Test Runner execution DEFERRED (no
Unity available this session).

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (focus order, pause gate, tab routing, purchase flow, spoiler)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES
Automated tests command: Unity Test Runner EditMode (NOT RUN — no Unity; tests compile via dotnet build PASS)
Manual Play Mode scenario: docs/validation/playmode/fable_14_human_test_scenario.md
Justification if no automated tests: N/A (tests added)
Residual risk: Canvas views, ShopCanvas refactor, live pause wiring and IMGUI-off flag are
  build-only this session; their runtime behavior is unverified until the human Play Mode
  scenario runs. EditMode tests are compiled but not executed in a runner this session.
```

## Honest status rationale

Status is **BUILD_VALIDATED_WITH_WARNINGS**, not BUILD_VALIDATED and not any ACCEPTED/
PLAYMODE status, because:

- The deterministic core compiles (0E) and is covered by new EditMode tests, satisfying the
  build + automated-test signal for the logic shipped.
- The spec's UI acceptance explicitly requires Play Mode, which was not run (out of bounds).
- Material parts of the spec (4 Canvas views, ShopCanvas refactor, live pause wiring, IMGUI
  flag) are DEFERRED and reported as such — not claimed done.
- `run_strict_validation.ps1` exit 1 is environmental (pre-modified scenes + historical
  report WARNs), as anticipated by the spec's own per-spec gate note.

This spec is NOT promoted to `implementados/` (deferred UI + Play Mode pending).

## Remaining work

- Build `RuntimeUiBuilder` (extract from `CreateMvpTownScene.CreateShopUi`) + ShopCanvas
  hierarchy-equivalence editor test (CA-3).
- Build `GameplayScreensCanvasBootstrap` + 4 Canvas screen views consuming WAVE 04/11
  viewmodels and the models shipped here.
- Wire the live adapter: `ModalManager.HasActiveModal` → `ModalPauseGate` → `GameTimeManager`.
- Flip IMGUI controllers behind `LegacyImguiPanelsEnabled = false`.
- Run Unity Test Runner (EditMode) and the human Play Mode scenario.

## Files changed

- `Assets/_Game/Scripts/UI/Runtime/UiFocusController.cs` (new)
- `Assets/_Game/Scripts/UI/Runtime/ModalPauseGate.cs` (new)
- `Assets/_Game/Scripts/UI/Runtime/GameplayScreenTab.cs` (new)
- `Assets/_Game/Scripts/UI/Runtime/GameplayScreensPanelModel.cs` (new)
- `Assets/_Game/Scripts/UI/Runtime/QuestLogSpoilerProjection.cs` (new)
- `Assets/_Game/Scripts/UI/Runtime/SkillNodePurchaseFlow.cs` (new)
- `Assets/_Game/Tests/EditMode/UI/Runtime/GameplayScreensRuntimeTests.cs` (new)
- `docs/validation/playmode/fable_14_human_test_scenario.md` (new)
- `docs/validation/fable_14_ui_canvas_screens_integration_execution_report.md` (this file)
- `Assembly-CSharp.csproj` (Compile includes; gitignored — not committed)
```
