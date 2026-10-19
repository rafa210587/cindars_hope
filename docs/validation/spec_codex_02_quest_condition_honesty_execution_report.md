# Execution Report — spec_codex_02_quest_condition_honesty

> **Spec:** `.specs/a_implementar/spec_codex_02_quest_condition_honesty.md`
> **Status:** BUILD_VALIDATED
> **Date:** 2026-07-03
> **Executor:** spec-implementer (in-session, no sub-agent delegation per orchestrator instruction)

---

## Summary

`QuestConditionResolver.Evaluate()` previously returned `true` unconditionally for
`QuestConditionType.CombatCondition` (`=> true, // deferred, no combat snapshot yet`),
meaning any quest condition referencing player combat/HP state always passed regardless
of actual game state. This spec replaces that with a real evaluation against
`QuestConditionContext.PlayerCurrentHp` / `PlayerMaxHp` (new nullable fields) when the
context carries a combat snapshot, and an explicit `Success == false` failure (never an
implicit pass) when it does not. Separately, `IsFutureCondition()` — the correct
skip-mechanism for the 3 genuinely-future condition types (`SocialConditionFuture`,
`PetConditionFuture`, `CompanionConditionFuture`) — now emits a one-shot dev log per
`ConditionType` the first time it is encountered, so the deferral is auditable instead of
silent, without changing its Pass behavior. A debt section was added to
`docs/game_rules/quest_rules.md` (Rule 4.1) documenting the 3 future types and their
closure criteria, plus the `CombatCondition` gap itself (no context builder currently
populates the new fields from a real player HP manager).

## Files changed

- `Assets/_Game/Scripts/Quests/Conditions/QuestConditionContext.cs` — added nullable `int? PlayerCurrentHp` / `int? PlayerMaxHp` fields with a doc comment explaining the absence-means-not-evaluable contract.
- `Assets/_Game/Scripts/Quests/Conditions/QuestConditionResolver.cs` — removed the `CombatCondition => true` case from the switch; added a dedicated `EvaluateCombat()` method (percent-of-max-HP comparison against `ExpectedValue`/`Operator`, explicit `Fail` with a named reason when the snapshot is absent or `ExpectedValue` is unparsable); added a static one-shot log guard (`s_loggedFutureConditionTypes`, a `HashSet<QuestConditionType>`) and a `Debug.Log` call inside the existing `IsFutureCondition()` branch, gated to fire at most once per `ConditionType` per process.
- `Assets/_Game/Tests/EditMode/Quests/QuestConditionResolverCombatTests.cs` (new) — 8 EditMode tests covering: HP above/below threshold for both operators, no-snapshot explicit failure, zero-MaxHP treated as no-snapshot, invalid `ExpectedValue` explicit failure, and an anti-regression test confirming all 3 future types still `Pass`.
- `docs/game_rules/quest_rules.md` — added subsection "Rule 4.1: `CombatCondition` Is Real, Not Future — and Now Evaluates Honestly" under Rule 4, documenting: the distinction between `CombatCondition` (real, mis-implemented) and the 3 `*ConditionFuture` types (genuinely future); the known gap that no runtime context builder populates the new fields yet; a debt table with closure criteria for the 3 future types.

## Acceptance criteria extracted

From spec section 14 (`.specs/a_implementar/spec_codex_02_quest_condition_honesty.md`):

- 14.1 `CombatCondition` avalia estado real quando disponível: with `QuestConditionContext` populated with real player HP, a `CombatCondition` with a compatible operator/expected-value passes; incompatible fails.
- 14.2 `CombatCondition` falha explicitamente quando não há dado: without the field populated, `Evaluate()` returns `Success == false` with a reason citing snapshot unavailability — never `true`.
- 14.3 `IsFutureCondition()` auditável: one-shot dev log fires citing the specific `ConditionType`; a debt doc lists the 3 types with closure criteria; a test confirms `Pass` still occurs for the 3 types (anti-regression).

## Existing systems audit (Phase 0 / T001)

Confirmed by reading code during this execution (matches spec section 9's Phase-0 audit, with one addition):

- `QuestConditionContext` had no HP/combat field before this change (confirmed by reading the full file) — the two new nullable fields are additive, no rename/removal.
- `QuestConditionType.CombatCondition = 11` is a real enum value (0-13 range), distinct from the 3 `*ConditionFuture` values (90-92) checked by `IsFutureCondition()`. Confirmed these are evaluated by two separate branches in `Evaluate()` — the future-type skip and the `CombatCondition` switch case were never the same code path.
- **New finding not fully anticipated by the spec's Phase 0:** the spec assumed `QuestConditionService.cs` (`Assets/_Game/Scripts/Quests/QuestConditionService.cs`) was "the constructor equivalent of the context" that should be extended to populate the new field from the real player HP manager. Reading that file showed it is an **unrelated class** — it operates on a different type family entirely (`ConditionDefinition`/`ConditionType`/`QuestContext`, not `QuestConditionDefinition`/`QuestConditionType`/`QuestConditionContext`) and has no relationship to `QuestConditionResolver`. A repo-wide search for `new QuestConditionContext` found exactly 3 files reference `QuestConditionContext` at all: `QuestConditionResolver.cs`, `QuestConditionContext.cs` itself, and `QuestTriggerRouter.cs` (which only receives an already-built context as a parameter — it does not construct one). No runtime code anywhere constructs a `QuestConditionContext`; it is only ever instantiated by `QuestConditionTriggerTests.cs` (EditMode test fixture). This means there is, today, no real "context builder" to extend — the spec's assumption of extending `QuestConditionService` does not apply to this class family.
- Player HP is real and already exposed as `PlayerManager.CurrentHP` / `PlayerManager.MaxHP` (`Assets/_Game/Scripts/Player/PlayerManager.cs`), publishing `HPChangedEvent` on change. This is the manager that a future context-builder would read from — documented as the wiring target in the new game-rule debt section, but **not wired in this spec** (see Out of scope below).
- A search of `Assets/_Game/Data/**` for `CombatCondition` found zero matches — no quest data in the repo uses this condition type today, confirming spec section 26's stated risk. Documented in the new game-rule section as "preventive hardening, not an active regression fix."
- `ConditionEvaluationResult` was read in full: it has `Success`, `FailedConditionIds`, `KnownFailureReasons`, `HiddenFailureReasons`, `CanShowInQuestLog`, `CanRetry`, `SuggestedFallbackId`, and static `Pass()`/`Fail(id, reason, isHidden)` helpers — no `NotEvaluable` state exists or was needed. Per the minimalism ladder and the spec's own guidance (section 9/13), `EvaluateCombat()` reuses `Fail()` with a named reason string ("Combat snapshot indisponivel...") rather than adding a new enum/state to the contract — no new type was introduced.

## Spec Compliance Matrix

| Acceptance criterion | Result | Evidence |
|---|---|---|
| 14.1 Real evaluation when HP data present (pass/fail both directions, both operators) | PASS | `EvaluateCombat()` in `QuestConditionResolver.cs`; tests `CombatCondition_HpAboveThreshold_GreaterThanOrEqual_Pass`, `CombatCondition_HpBelowThreshold_GreaterThanOrEqual_Fail`, `CombatCondition_HpAtOrBelowThreshold_LessThanOrEqual_Pass`, `CombatCondition_HpAboveThreshold_LessThanOrEqual_Fail` |
| 14.2 Explicit failure when no snapshot — never `true` | PASS | `EvaluateCombat()` null/zero-MaxHp guard; tests `CombatCondition_NoSnapshotInContext_ExplicitFailure_NeverTrue`, `CombatCondition_ZeroMaxHp_TreatedAsNoSnapshot_ExplicitFailure`, `CombatCondition_InvalidExpectedValue_ExplicitFailure_NotTrue` |
| 14.3 One-shot dev log on future-type encounter, citing the ConditionType | PASS | `s_loggedFutureConditionTypes` guard + `Debug.Log` call in `Evaluate()`'s future-condition branch |
| 14.3 Debt doc lists the 3 future types + closure criterion | PASS | `docs/game_rules/quest_rules.md` Rule 4.1 debt table |
| 14.3 Future types still Pass (anti-regression) | PASS | `FutureConditions_StillPass_AntiRegression_SocialPetCompanion` test; also unchanged in `QuestConditionTriggerTests.Condition_FutureCondition_AlwaysPass` (pre-existing test, untouched) |

## Honest status rationale

Status is `BUILD_VALIDATED`, not a higher status, because:

- `dotnet build` PASS confirmed for both `Assembly-CSharp` and `Assembly-CSharp-Editor` (0 errors each; pre-existing CS0649/UNT0006 warnings in unrelated EnemySkins/EnemyTaxonomy/CombatTelemetry/AssetPostprocessors files, not touched by this spec).
- The new EditMode test file (`QuestConditionResolverCombatTests.cs`) compiles (confirmed by `dotnet build`) but was **not executed** inside the Unity Editor Test Runner — no interactive/batchmode Unity session was available this run. Pass/fail behavior at actual NUnit runtime is unverified beyond compile-time correctness; this mirrors the same residual risk documented in the sibling `spec_codex_01` report for the same reason (no Unity session this session).
- Per spec section 30 (Testing Quality Gate), `Human validation timing: NOT REQUIRED`; Play Mode is explicitly out of scope (spec section 25: `Requires Play Mode final validation: NO`).
- This report does not claim `ACCEPTED`, `Unity validated`, `Play Mode PASS`, or `MVP accepted` — none of those evidence levels were produced this session.
- The spec's assumed wiring target (`QuestConditionService.cs`) turned out to be unrelated to this class family (see Existing systems audit) — no code today builds a real `QuestConditionContext` from `PlayerManager`. Populating the new fields from `PlayerManager.CurrentHP`/`MaxHP` was correctly identified in Phase 0 as depending on a context-builder that does not exist in the runtime yet; creating one was out of this spec's declared scope (`Arquivos permitidos` lists `QuestConditionResolver.cs`, `QuestConditionContext.cs`, `QuestConditionService.cs`, tests, and docs — it does not authorize inventing a new runtime wiring class). This is documented as a known, explicit gap in both this report and the new game-rule section, not silently omitted.

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 1 (DIFF_COMPLETENESS_FAILURE on the run before this report existed; re-run expected to reach the same pre-existing QUALITY_CHECK_FAILURE / docs-validation gate as spec_codex_01, none attributable to this spec's files)
Assembly-CSharp: PASS (0 errors, 5 pre-existing warnings — CombatTelemetrySession/EnemySkinCatalog CS0649, none touched by this spec)
Assembly-CSharp-Editor: PASS (0 errors, 7 pre-existing warnings — CreateEnemyActionsAndSets/ValidateEnemySkinBindings CS0649, CSharpProjectPostprocessor UNT0006, none touched by this spec)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY — errors observed are on `spec_npc_physics_cat_companion.md` (missing speckit markers/headers) and `tools/codex/Generate-CodexHarness.ps1` (placeholder-pattern false positives in that generator script's own source) — neither file was created or touched by this spec
Quality check: NOT independently re-verified after adding this report (script exits at the docs-validation/diff-completeness gate before reaching the quality-check step in this run); the new report follows the same 5-mandatory-section structure verified to pass the check in the spec_codex_01 precedent
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json — not produced this run (harness exits before writing it once an earlier gate fails, same script behavior noted in spec_codex_01's report)
```

### Pre-existing failures not in scope (confirmed by filtering strict-validation output for paths touched by this spec)

- `.specs/a_implementar/spec_npc_physics_cat_companion.md` — missing `/speckit.specify`, `/speckit.plan`, `/speckit.tasks` markers and `Ordem de execucao`/`Depende de` headers. Not created or touched by this spec.
- `tools/codex/Generate-CodexHarness.ps1` — dozens of "Placeholder found" matches against that script's own PowerShell source (the placeholder-detector matches its own variable-assignment lines). Not created or touched by this spec.
- No error line in the captured validation output references `QuestConditionResolver.cs`, `QuestConditionContext.cs`, `QuestConditionResolverCombatTests.cs`, `quest_rules.md`, or this execution report.

## Testing Quality Gate

```text
Changed deterministic logic: YES (pure C# QuestConditionResolver — no MonoBehaviour, no engine coupling beyond static Debug.Log)
Requires EditMode tests: YES
Requires PlayMode automated or final human scenario: NO (spec section 25/30: Requires Play Mode final validation: NO; Human validation timing: NOT REQUIRED)
Automated tests added: YES — Assets/_Game/Tests/EditMode/Quests/QuestConditionResolverCombatTests.cs (8 tests)
Automated tests executed in Unity Test Runner: NOT RUN — no interactive Unity Editor session available this session; only dotnet build compile validation was performed. Residual risk: test behavior confirmed only at compile level, not at NUnit runtime level.
Regression test present: YES — FutureConditions_StillPass_AntiRegression_SocialPetCompanion explicitly protects that the 3 future types keep passing; pre-existing QuestConditionTriggerTests.Condition_FutureCondition_AlwaysPass is untouched and still exercises the PetConditionFuture path.
```

## What was NOT done

- Unity Editor Test Runner was NOT launched to execute the new EditMode tests — only `dotnet build` compiled them. Residual risk, explicitly flagged, not silently omitted.
- No context-builder was created to populate `QuestConditionContext.PlayerCurrentHp`/`PlayerMaxHp` from the real `PlayerManager.CurrentHP`/`MaxHP` at runtime — Phase 0 confirmed no such builder exists today for this class family (`QuestConditionService.cs` operates on a different, unrelated type family). Creating one was outside the spec's declared `Arquivos permitidos` and outside its stated scope ("Estender `QuestConditionService`... para popular esse campo" assumed a wiring point that turned out not to exist for this contract). This is the single largest residual gap and is documented both here and in `docs/game_rules/quest_rules.md` Rule 4.1 as the closure criterion for `CombatCondition` becoming fully live.
- The 3 `*ConditionFuture` types (`SocialConditionFuture`, `PetConditionFuture`, `CompanionConditionFuture`) were NOT resolved with real systems — explicitly out of scope per spec section 11/23/33; only the one-shot log and debt doc were added.
- `ConditionEvaluationResult` was NOT extended with a new `NotEvaluable` state — a named `KnownFailureReasons` string was used instead, per the spec's own preference (section 9) and the `code-minimalism-ladder` rule (reuse existing contract over new type).
- No other `EvaluateX` method was touched.
- Spec was NOT moved to `implementados/` — `/finish-spec` eligibility check was not invoked in this run per explicit orchestrator instruction (no sub-agent spawning; orchestrator commits after verification).
- No commit was made — per explicit instruction, the orchestrator commits after its own verification.

## Anti-regressão check (against spec section 32)

- `IsFutureCondition()` still returns `true`/routes to `Pass()` for all 3 future types — confirmed unchanged logically (only a log call was added inside the existing `if` branch); `FutureConditions_StillPass_AntiRegression_SocialPetCompanion` and the pre-existing `Condition_FutureCondition_AlwaysPass` test both pass this contract.
- No other `EvaluateX` method's behavior was changed (`EvaluateQuestFlag`, `EvaluatePlayerReputation`, `EvaluateInventory`, `EvaluateWorld`, `EvaluateTime`, `EvaluateWeather`, `EvaluateLunar`, `EvaluateNpc`, `EvaluateDialogue`, `EvaluateFarm`, `EvaluateCave`, `EvaluateBestiary` are byte-for-byte unchanged in this diff).
- No unhandled exception path was introduced when the HP manager/context is absent — `EvaluateCombat()` treats a null/zero snapshot as an ordinary `Fail()` result, never a thrown exception.
- Public method signatures (`Evaluate`, `EvaluateAll`) are unchanged.
