# Ambient v15 native authoring candidate

Candidate only; root owns copying into `Assets/_Game/Scripts/Editor/Art/` and Unity execution.

Call once in `CreateMvpFarmScene`, immediately before MarkSceneDirty/SaveScene:

```csharp
var ambientReport = FarmAmbientAnimationAuthoring.Apply(scene);
```

The return value is a serializable validation report; root can write `JsonUtility.ToJson(ambientReport, true)` into its selected evidence directory. `ValidateGenerated()` can recheck existing assets independently, without claiming a scene-collider comparison.

Inputs already copied by root: `Assets/_Game/Art/Generated/World/ambient/{fountain,cascade,fish}_v15.{png,json}`. JSON is Aseprite array format, untrimmed. Source artwork corresponds to fountain_ambient_v15, cascade_ambient_v15, fish_jump_v15 under the parent authoring folder.

Outputs: three native AnimationClips (with stable named Sprite subassets) and three native AnimatorControllers under `ambient/_AnimationAssets/`. Existing source renderers retain canvas/pivot/PPU, transform, color and sorting. All Animators use AlwaysAnimate. No runtime helper, custom SO, new collider, input or save behavior.

Fountain/cascade: 5 × 140ms, looping 0.7s. Fish: 8 × 120ms at 19.04–20s, a null sprite from 0–19.04s, plus null at the 20s boundary; source frame7 is also transparent. Fish PPU18.3, pivot(0.5,14/48), scale1, position(22,-12), Ground/5.

Validation checks native keyframes, period, sampling every authored interval, and null wait/boundary for fish. Apply additionally compares the existing scene colliders before/after. **Editor native sampling is not a 21-second Play Mode observation, visual approval or performance measurement.**

Preparation checks: source JSON dimensions/frame counts/durations checked statically. Unity compile and native sampling: NOT RUN by this worker (root owns the Unity window).
