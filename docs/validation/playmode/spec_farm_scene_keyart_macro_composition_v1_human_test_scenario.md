# FarmScene Macro Composition — Human Test Scenario

Status: `NOT RUN` — Unity Editor is unavailable locally.

## Feature Summary

Verify that the regenerated FarmScene keeps the collision/navigation contract while reading as six keyart macro masses: north cliff, west forest, central agriculture, northeast homestead, south animal row, and southeast lake.

## Scene and Initial State

- Open the project in a Unity Editor that can import the current scripts.
- Use a new or disposable save; this scenario does not require save-state changes.
- Run `CindarsHope/Inicializar Projeto` once to regenerate FarmScene.

## Scenario 1 — Regeneration and composition evidence

1. Run `CindarsHope/Validar Composicao FarmScene`.
2. Confirm the Console contains exactly `ValidateFarmSceneComposition: 6/6 groups valid` and `ValidateFarmSceneComposition: Scale outliers: 0`.
3. Run `CindarsHope/Dev/Capturar FarmScene (PNG)`.
4. Confirm three lines beginning `[FarmSceneCapture] salvo:` were emitted and that the full-map, homestead, and animal-row PNG files were updated.
5. Inspect the full capture: north cliff, west forest, central farm area, northeast homestead, south buildings, and southeast lake must each read as distinct masses. No final terrain/detail fidelity is asserted in this spec.

## Scenario 2 — Physical regression check

1. Run `CindarsHope/Validar Navegacao FarmScene`.
2. Confirm `Reachable required landmarks: 10/10`.
3. Enter Play Mode and walk from the homestead through the bridge corridor to the central field, then visit the cave approach, town exit, lake approach, and south animal row.
4. Confirm the player does not pass through mountain, river, or lake collision, and that the bridge corridor remains passable even though the bridge visual is smaller.

## Expected Results

- The six composition groups and approved scale ranges validate with zero errors/outliers.
- Capture produces all three required views.
- Navigation retains all ten required approaches; no object root, interaction ID, or collider was moved by visual-scale changes.

## Console Expectations

- Expected: the validator and three capture success lines above.
- Forbidden: `ERROR`, `Exception`, missing-anchor, scale-outlier, or navigation-reachability errors.

## Pass/Fail Checklist

- [ ] FarmScene regenerates without compile errors.
- [ ] Composition validator reports 6/6 groups and zero scale outliers.
- [ ] Capture emits three save lines and updates all views.
- [ ] Navigation validator reports 10/10 landmarks.
- [ ] Basic movement preserves water/mountain blocking and bridge passage.

Overall: `NOT RUN`.
