# FarmScene Landmarks and Agriculture — Human Test Scenario

Status: `NOT RUN` — Unity Editor is unavailable locally.

## Feature Summary

Verify that the regenerated FarmScene reads as seven accessible landmarks while retaining the existing farming, animal, fountain, cave and fishing systems. The added crop rows and props are visual-only children; they must not replace gameplay roots.

## Scene and Initial State

- Open the project in a Unity Editor that can import the current scripts.
- Use a new or disposable save. No save migration is required.
- Run `CindarsHope/Inicializar Projeto` once to regenerate FarmScene.

## Scenario 1 — Landmark composition and field interaction

1. Run `CindarsHope/Validar Marcos FarmScene`.
2. Confirm `ValidateFarmSceneLandmarks: Landmarks valid: 7/7` and `CentralField: PASS (rows>=4, approaches>=4)` appear with zero errors.
3. Run `CindarsHope/Validar Navegacao FarmScene` and confirm `Reachable required landmarks: 10/10`.
4. Enter Play Mode. Walk to the central field from each side, till one valid tile, plant a seed and water it using the existing controls.
5. Confirm the four crop rows are visual-only: no decorative crop opens a `FarmPlot` interaction and the valid tilled tile remains usable.

## Scenario 2 — Existing functional landmarks

1. Walk to the homestead, Fonte de Anya, cave approach, south animal row and lake edge.
2. Interact with the existing fountain, an animal housing release point, and the `FishingSpot` using their normal runtime requirements.
3. Confirm the well, pen fence/hay/livestock, clearing foliage, ore and lake foliage are visible props only and do not block the required interaction approach.
4. Confirm mountain, river and lake stay blocked where expected and the bridge corridor stays passable.

## Expected Results

- All seven landmark inventories validate and the central field reports four rows/four approaches.
- Farming, animal release, fountain and fishing use their pre-existing components without duplicate systems.
- No unexpected Console error or exception occurs.

## Console Expectations

- Expected: the two landmark-validator success lines and navigation `10/10`.
- Forbidden: `ERROR`, `Exception`, missing functional-root, missing required-object, or approach-count errors.

## Pass/Fail Checklist

- [ ] FarmScene regenerates without compile errors.
- [ ] Landmark validator reports 7/7 and zero errors.
- [ ] Navigation validator reports 10/10.
- [ ] A central-field tile can be tilled, planted and watered.
- [ ] Fountain, animal release and fishing remain reachable and functional.
- [ ] Decorative children do not add colliders or gameplay interactions.

Overall: `NOT RUN`.
