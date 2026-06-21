# Execution Report — fable_64 (Death Screen Canvas + Corpse Recovery Messaging)

- Spec: `.specs/a_implementar/fable/fable_64_spec_death_screen_canvas_corpse_messaging.md`
- Status: **BUILD_VALIDATED_WITH_WARNINGS** (PlayMode/human validation DEFERRED_TO_FINAL_VALIDATION per spec)
- Date: 2026-06-21
- Branch: dev
- Type: Runtime / UI / Integration
- validated_game_rules: [death_anya_corpse_rules.md (exhibited only, not changed), ui_modal_rules.md, event_rules.md]
- validated_adrs: [ADR-0007-event-bus-gameplay-communication.md]

---

## Acceptance criteria extracted

| ID | Criterion | Status | Evidence |
|----|-----------|--------|----------|
| CA-1 | Canvas screen replaces IMGUI; same triggers (PlayerDied/CavePlayerDefeated); DeathScreenOpened/Closed preserved | OK | `DeathScreenController` emptied to no-op shim (no OnGUI/GUI.Window anywhere in UI/Death); new `DeathScreenCanvasController` subscribes to both triggers and publishes Opened/Closed. EditMode tests cover action contract; build diff shows OnGUI removed. |
| CA-2 | Cave death shows death level, N items + gold, corpse level, recovery instruction, XP lost when present | OK | `DeathScreenViewModel.ForCaveDeath` + `ComposeBodyText`; tests `CaveDeath_WithCorpse_*`, `CaveDeath_WithXpLost_*`, `ComposeBodyText_CaveWithCorpse_*`. |
| CA-3 | Overworld death coherent (SceneName + empty state, no corpse/loss) | OK | `ForOverworldDeath`; tests `OverworldDeath_WithScene_*`, `OverworldDeath_NoScene_*`, `ComposeBodyText_OverworldNoCorpse_*`. |
| CA-4 | "Respawn na Fonte" fires real respawn path and closes; future slot disabled with honest label; no button without real effect | OK | Respawn already executes at death-time in `DeathSystemBootstrap.OnPlayerDied`; the button only DISMISSES (publishes Closed) — no second respawn path. Future action `Enabled=false`. Tests `Actions_RespawnEnabled_FutureDisabled`. |
| CA-5 | Zero diff in CorpseRecoveryManager/AnyaRespawnService/penalty rules; CaveDeathResolver at most additive | OK | No changes to those files (CaveDeathResolver not touched — required data already published via existing events). Diff is UI/Death only + csproj + tests + docs. |

---

## Existing systems audit

Phase 0 audit (system-reuse-audit). Findings:

- **Respawn path (authoritative):** `DeathSystemBootstrap.OnPlayerDied` already calls `_respawnService.RespawnAtAnyaFountain()` for cave deaths, and sets the active corpse via `CorpseRecoveryManager.SetActiveCorpse`. Respawn is NOT button-driven. Decision: the death screen is informational; the "Respawn" button only dismisses. **No second respawn path created.**
- **Corpse snapshot source:** `GameBootstrap.Instance.CorpseRecoveryManager.ActiveCorpse` is authoritative (`Corpse.GetTotalRecoverableItems()` / `GetTotalRecoverableGold()` / `CaveLevel`). Events used only as triggers / to cache cave level + XP lost. **Not re-derived from loose events.**
- **DeathScreenOpened/ClosedEvent consumers:** searched whole runtime — only `UIEvents.cs` (definitions) and the death controller itself. No external runtime consumer. Contract preserved by re-publishing at the same moments.
- **Cave level / XP lost:** `CavePlayerDeathResolvedEvent{CaveLevel}` and `XpResetToLevelStartEvent{XpLost}` cached on receipt (subscription order not guaranteed).
- **F14/E44 RuntimeUiBuilder:** NOT present in repo (`Glob **/RuntimeUiBuilder.cs` → none). **Fallback to WI-23 canvas pattern** (programmatic Canvas + `RuntimeInitializeOnLoadMethod` self-wiring, mirroring `GameplayHudBootstrap`) — documented as residual debt; migration to E44 builder is trivial later.
- **ModalManager:** `ModalType.Death` already exists; reused via `GameBootstrap.Instance.ModalManager.PushModal/TryPopIfCurrent`.
- **UiFocusController** (`CindarsHope.UI.Runtime`): reused for keyboard focus.

Created (not duplicated): `DeathScreenViewModel` (pure projection), `DeathScreenCanvasController` (thin MonoBehaviour adapter). Old `DeathScreenController` emptied to a deprecated no-op shim to preserve scene references without double-subscription.

---

## Spec Compliance Matrix

| Requirement | Implementation |
|-------------|----------------|
| DeathScreenCanvasController (canvas, triggers/events preserved) | `Assets/_Game/Scripts/UI/Death/DeathScreenCanvasController.cs` — subscribes PlayerDiedEvent/CavePlayerDefeatedEvent; publishes DeathScreenOpened/Closed; self-wired RuntimeInitializeOnLoad. |
| DeathScreenViewModel (pure projection) | `Assets/_Game/Scripts/UI/Death/DeathScreenViewModel.cs` — `ForCaveDeath`/`ForOverworldDeath`, simple types, no UnityEngine. |
| Keyboard navigable; Esc does NOT close | `Update()` handles arrows + Enter; Esc intentionally ignored (comment + behaviour). |
| Cave message (items/gold/level/instruction/XP) | `ForCaveDeath` + `ComposeBodyText`. |
| Overworld coherent empty state | `ForOverworldDeath`. |
| Real respawn action + honest disabled future slot | Respawn = dismiss (real respawn already at death-time); future action `Enabled=false`. |
| Single corpse source (manager authoritative) | `BuildCorpseSnapshot` reads `CorpseRecoveryManager.ActiveCorpse`. |
| No GameObject.Find; refs via GameBootstrap | `GameBootstrap.Instance` accessors only. |
| Unsubscribe paired | `OnEnable`/`OnDisable` symmetric. |
| IMGUI removed | `DeathScreenController` shim has no OnGUI/GUI.Window. |
| No new events / no save changes | None added. |

---

## Validation

| Check | Result |
|-------|--------|
| `dotnet build Assembly-CSharp.csproj --no-restore` | PASS — 0 errors, 1 pre-existing warning (CombatTelemetrySession CS0649) |
| `dotnet build Assembly-CSharp-Editor.csproj --no-restore` | PASS — 0 errors, 3 pre-existing warnings |
| `tools/docs/validate_docs.ps1` | PASS — exit 0 |
| `tools/docs/check_spec_diff_completeness.ps1` | PASS — exit 0 |
| `run_strict_validation.ps1` | exit 1 — ENVIRONMENTAL ONLY (3 pre-existing modified `.unity` scenes in working tree, not from this spec); builds + docs all PASS |
| EditMode tests | Compiled into Assembly-CSharp (0E); execution via Unity Test Runner DEFERRED (no Unity Editor per session policy) |

Validation method: individual dotnet builds + docs scripts. Exit codes checked via `$LASTEXITCODE`.

---

## Honest status rationale

Core criteria CA-1..CA-5 are implemented and build-validated (0E both assemblies). Death-screen logic is in a pure, EditMode-tested projection; the MonoBehaviour is a thin adapter. No death/penalty/corpse rule was changed — the screen only exhibits already-computed data and reuses the existing respawn path. Status is BUILD_VALIDATED_WITH_WARNINGS rather than ACCEPTED because:

- PlayMode / human validation is DEFERRED_TO_FINAL_VALIDATION (per spec and session policy — no Unity Editor run).
- WI-23 canvas fallback used (E44 RuntimeUiBuilder absent) — documented residual debt.
- EditMode tests compile but were not executed in Unity Test Runner this session.

`run_strict_validation.ps1` exit 1 is environmental (pre-existing `.unity` modifications in the working tree, unrelated to this spec); the scoped `check_spec_diff_completeness.ps1` gate passes.

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (death projection + body-text composition + action/event contract)
Changed Unity scene/prefab/asset wiring: NO (programmatic canvas; old controller shim keeps scene refs valid)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/UI/DeathScreenCanvasTests.cs — 16 tests)
Automated tests command: Unity Test Runner EditMode — NOT RUN this session (deferred; compiles 0E in Assembly-CSharp)
Manual Play Mode scenario: docs/validation/playmode/fable_64_human_test_scenario.md
Justification if no automated tests: N/A (tests added)
Residual risk: Canvas visual layout + keyboard focus + modal input-block only verifiable in Play Mode (deferred to final validation). WI-23 fallback canvas pending future E44 migration. Subscription-order edge for cave level/XP mitigated by caching, but only fully exercised live.
```

Anti-regression: death/respawn/penalty flow byte-identical in behaviour (surface only). CorpseRecoveryModal untouched. DeathScreenOpened/Closed published at same moments. No second respawn path. No new OnGUI in project. Events via GameEventBus, unsubscribe paired, zero GameObject.Find.

## Remaining work

- Execute EditMode tests in Unity Test Runner (final validation).
- Execute human Play Mode scenario (die in cave with items → read message → respawn → recover corpse; die in overworld).
- Optional: migrate WI-23 fallback canvas to E44 RuntimeUiBuilder when available.
- Optional: remove the `DeathScreenController` no-op shim from scenes after regeneration.

## Files changed

- `Assets/_Game/Scripts/UI/Death/DeathScreenViewModel.cs` (new)
- `Assets/_Game/Scripts/UI/Death/DeathScreenCanvasController.cs` (new)
- `Assets/_Game/Scripts/UI/Death/DeathScreenController.cs` (emptied → deprecated no-op shim; IMGUI removed)
- `Assets/_Game/Tests/EditMode/UI/DeathScreenCanvasTests.cs` (new)
- `docs/validation/fable_64_spec_death_screen_canvas_corpse_messaging_execution_report.md` (this report)
- `docs/validation/playmode/fable_64_human_test_scenario.md` (new)
- `Assembly-CSharp.csproj` (compile includes — gitignored, NOT committed)
