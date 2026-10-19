# WAVE_INTEGRATION_11 — Human Unity Wiring Instructions

**Date:** 2026-06-08
**Status:** PENDING HUMAN ACTION

---

## Why Human Action Is Required

WAVE_INTEGRATION_11 added new MonoBehaviours and an extensible effect pipeline. The FarmScene scene file was NOT modified automatically (Unity YAML editing policy). The `CreateMvpFarmScene` editor generator must be re-run to wire the new components. Additionally, Unity must reimport the project to regenerate the `.csproj` files with the new source files.

---

## Prerequisites

- Unity 6000.4.7f1 (or current LTS) open with this project
- All compilation errors resolved (both assemblies should compile after reimport)
- FarmScene not currently loaded with unsaved changes

---

## Step 1: Open Unity and Wait for Reimport

After pulling this branch, Unity will automatically reimport. Wait for the reimport spinner to finish. The new files will be compiled into the runtime assembly.

---

## Step 2: Verify Compilation

- Check Unity Console: no `error CS` messages
- If there are compile errors in the new files, fix them before continuing

---

## Step 3: Regenerate FarmScene

Run:

**CindarsHope > Create Scenes > Farm Scene**

This will:
- Delete and recreate FarmScene
- Add `ActiveSkillExecutionController` to the scene (if Unity has compiled it)
- Add `PlayerDashController` and `PlayerMovementAbilityController` to the Player GameObject
- Wire Rigidbody2D and PlayerController references to the new controllers

Expected console output should include:
- "CreateMvpFarmScene: [component] added successfully" (or similar)
- No "type not found" warnings if Unity has completed reimport

---

## Step 4: Verify Component Wiring in Inspector

Open FarmScene, select the Player GameObject. Confirm:

| Component | Expected |
|-----------|----------|
| `PlayerDashController` | Present; _rigidbody and _playerController wired |
| `PlayerMovementAbilityController` | Present; _rigidbody and _playerController wired |

Find the `ActiveSkillExecutionController` GameObject (root level). Confirm:
- The `ActiveSkillExecutionController` component is present
- OR the scene runs and the RuntimeInitializeOnLoadMethod creates it automatically

---

## Step 5: Run Unity Validators

**CindarsHope > Validate > Validate Skill Effects Bridge**
- All checks should PASS
- If "FarmCropSkillEffectExecutor NOT resolved" → check that the runtime assembly compiled correctly

**CindarsHope > Validate > Validate Dash Dodge Movement**
- All checks should PASS
- Design direction values are documented inline

---

## Step 6: Play Mode Test

Run the WAVE_INTEGRATION_11 human Play Mode checklist:
`docs/validation/WAVE_INTEGRATION_11_HUMAN_PLAYMODE_CHECKLIST.md`

---

## Known Limitations After This Wave

1. The skill action IDs in the default catalog may not all have matching effect executors. Unmatched IDs will produce "Skill not yet implemented" feedback — this is expected behavior.

2. Pressing 1-4 when no skill is equipped in that slot produces "Nenhuma habilidade equipada no slot N" feedback — expected.

3. Block (Left Shift) is deferred. No component handles it. No feedback is produced.

4. Stamina is deducted for Dash/Dodge but NOT for skill slot effects (TODO_INTEGRATION_NOT_FINAL in `FarmCropSkillEffectExecutor`).

---

*Instructions created: 2026-06-08 (WAVE_INTEGRATION_11)*
