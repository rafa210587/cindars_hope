---
name: wave-integration-slice
description: Protocol for WAVE_INTEGRATION specs — audit existing systems, decide reuse vs. new, implement, document, and produce human checklist
version: 1.0
when_to_use: Any WAVE_INTEGRATION spec (scene integration, runtime binding, NPC wiring, HUD binding, economy loop, skill effects, movement actions)
---

# Wave Integration Slice Skill

## Use When

Task is a `WAVE_INTEGRATION_*` spec covering:
- Scene runtime binding (HUD, inventory, equipment)
- Economy/shipping loop wiring
- NPC dialogue/shop runtime
- Skill effects / active slot binding
- Player movement/ability runtime
- Resource interactable wiring
- Any "make the code actually run in a scene" slice

## Do NOT Use When

- Spec is a pure WAVE (farm, cave, quest, calendar) with no scene integration
- Spec is docs-only or asset-only

---

## Phase 0 — Preflight

```powershell
Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'
git status --short | Select-Object -First 20
git branch --show-current   # must be dev
```

Confirm:
- Branch = `dev`
- Working tree clean (or only expected files)

---

## Phase 1 — Audit Existing Systems

Before writing a single line of code, audit what already exists.

Read:
1. The spec
2. `docs/project/CURRENT_STATE.md` (WAVE_INTEGRATION section)
3. The immediately prior WAVE_INTEGRATION report (if listed as dependency)

Create audit doc:
```
docs/validation/WAVE_INTEGRATION_<N>_<SLUG>_RUNTIME_AUDIT.md (optional if simple)
```

Answer these questions in the audit:
```
System X already exists?        YES / NO / PARTIAL
Controllers already attached?   YES / NO
RuntimeInitializeOnLoad used?   YES / NO
Modal guard exists?             YES / NO
Stamina/economy wired?          YES / NO
Bootstrap accessible?           YES / NO
Scene creator updated?          YES / NO
Debt from prior wave?           list it
```

**Reuse-first rule**: If the system already exists and works, extend it. Do NOT create a parallel system.

---

## Phase 2 — Decision Doc

Create:
```
docs/validation/WAVE_INTEGRATION_<N>_<SLUG>_DECISION.md
```

Structure:
```markdown
# WAVE_INTEGRATION_<N> — <Title> Decision

## Strategy
REUSE_EXISTING_<SYSTEM> / NEW_SYSTEM / EXTEND_EXISTING

## Rationale
<why this strategy>

## Reused systems
- <system>: <what is reused>

## Deferred
- <debt tag>: <reason deferred>

## Files in scope
- <list>
```

---

## Phase 3 — Implementation

### Runtime Bootstrap Pattern (when attaching to player/scene at runtime)

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
private static void EnsureInstance()
{
    if (_instance != null) return;
    var go = new GameObject("XRuntimeBootstrap");
    DontDestroyOnLoad(go);
    _instance = go.AddComponent<XRuntimeBootstrap>();
}
```

Re-bind on scene load:
```csharp
private void OnEnable() => SceneManager.sceneLoaded += HandleSceneLoaded;
private void OnDisable() => SceneManager.sceneLoaded -= HandleSceneLoaded;
```

### Modal Guard (always required for input-driven runtime)

```csharp
if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true) return;
```

### Graceful Optional Dependencies

```csharp
// Stamina — optional
var bootstrap = GameBootstrap.Instance;
if (bootstrap != null && _staminaManager == null)
    _staminaManager = bootstrap.StaminaManager;

// Use only if present
if (_staminaManager != null && !_staminaManager.TrySpendStamina(cost))
{
    GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente."));
    return;
}
```

### Debt Tags (use in code comments and reports)

```
TODO_INTEGRATION_NOT_FINAL
ACTIVE_SKILL_RUNTIME_DEFERRED_TO_WAVE_INTEGRATION_NN
HUD_FEEDBACK_CONSUMER_DEBT
BALANCE_FINAL_PENDING
BLOCK_DAMAGE_REDUCTION_DEFERRED_TO_COMBAT_RUNTIME
```

---

## Phase 4 — Docs

### Required docs (always)

| Doc | Path |
|-----|------|
| Report | `docs/validation/WAVE_INTEGRATION_<N>_<SLUG>_REPORT.md` |
| Human checklist | `docs/validation/WAVE_INTEGRATION_<N>_HUMAN_PLAYMODE_CHECKLIST.md` |

### Optional docs (when Unity wiring is required)

| Doc | Path |
|-----|------|
| Wiring instructions | `docs/validation/WAVE_INTEGRATION_<N>_HUMAN_UNITY_<SLUG>_WIRING_INSTRUCTIONS.md` |

### Report structure

```markdown
# WAVE_INTEGRATION_<N> — <Title> Report

## Status
BUILD_VALIDATED_WITH_UI_DEBT / BUILD_VALIDATED / BLOCKED

## Root cause / gap addressed
<what was missing>

## Strategy applied
REUSE_EXISTING_X

## Files created
- path

## Files changed
- path

## Debts
- DEBT_TAG: explanation

## Assembly-CSharp: PASS / FAIL
## Assembly-CSharp-Editor: PASS / FAIL
## Docs validation: PASS / EXPECTED_FAIL_LEGACY_ONLY

## Human Play Mode needed: YES
## Can continue to next WAVE_INTEGRATION: YES / NO
```

### Human checklist structure

```markdown
| Step | Expected result | Pass/Fail | Notes |
|------|----------------|-----------|-------|
| Open Scene | No red errors | | |
| Press Play | System initializes | | |
| ... | ... | | |
| Stop Play | Scene not corrupted | | |
```

---

## Phase 5 — Build Validation

```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED"; exit 1 }

dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "EDITOR BUILD FAILED"; exit 1 }
```

Expected: 0 errors. Pre-existing editor warnings are acceptable.

---

## Phase 6 — CURRENT_STATE Update

Update the WAVE_INTEGRATION section in `docs/project/CURRENT_STATE.md`:

```
- WAVE_INTEGRATION_<N>: <STATUS> (<date>) — <one-line summary>; human must execute checklist before ACCEPTED/WAVE_INTEGRATION_<N+1> continuation
```

---

## Status Taxonomy for Integration Work

| Status | Meaning |
|--------|---------|
| `BUILD_VALIDATED` | Code compiles, logic complete, no deferred debt |
| `BUILD_VALIDATED_WITH_UI_DEBT` | Core wired, UI/visual/PlayMode deferred |
| `BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE` | Code complete, human hasn't tested yet |
| `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED` | Needs Unity Editor scene action |
| `BLOCKED` | Cannot continue without scene/Packages/forbidden scope |

---

## Common Regressions

- Creating a new system when an existing one should be extended
- Not checking modal guard for input-driven actions
- Calling `MovePosition` from coroutine without `IsBeingDisplaced` flag (see `player-ability-runtime`)
- Committing scene files that Unity auto-modified
- Missing wiring instructions when human Unity action is required

## Stop Conditions

- Spec requires scene YAML edit → document as `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED`, stop
- Spec requires Packages/ or ProjectSettings/ → `BLOCKED`
- Reuse-first audit finds parallel system already being built → stop and report conflict
