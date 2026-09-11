---
name: hud-canvas-binding
description: Materialize or repair a gameplay Canvas widget using the existing HUD composition and projections. Check layout, event wiring, modal focus and visible runtime behavior.
---

# Skill: Canvas and HUD binding

A tested projection needs a bound View and observed behavior. Inspect the actual HUD owner
and hierarchy before applying defaults from an external UI skill.

## Workflow
1. Read the target spec and current Canvas/controller/composition code. Reuse the existing HUD,
   EventSystem and input module; do not assume a historical bootstrap class owns new widgets.
2. Keep the View a thin binding adapter. Domain decisions remain in projections/services;
   gameplay communication follows GameEventBus with matching unsubscribe.
3. Create or repair the hierarchy through its existing generator or Editor API. Preserve
   reference identity. When using a live bridge, apply
   [unity-mcp-operations](../unity-mcp-operations/SKILL.md).
4. Diagnose who controls each RectTransform axis: anchors/offsets, parent LayoutGroup or
   ContentSizeFitter. Avoid two controllers fighting over the same dimension. For ScrollRect,
   verify viewport/mask/content references and overflow behavior.
5. Preserve the project's pixel-art scaling contract. Do not impose a reference resolution,
   CanvasScaler mode, font migration or extra root Canvas from a generic example.
6. Verify visible bounds, alpha, clipping, ordering and raycast blockers. Decorative graphics
   must not intercept intended input. Reuse localized string keys; check long text, accents,
   wrapping and missing-key behavior in the actual View.
7. Bind interactive controls to their intended action/projection. Exercise open/use/close,
   empty/disabled/error states and modal input blocking as applicable. Visual appearance alone
   does not establish a functioning button or correct gameplay state.
8. Update state from relevant events; avoid assigning unchanged text/layout each frame.
   Profile rebuild costs before splitting canvases as an optimization.

## Evidence
Use [unity-validation](../unity-validation/SKILL.md) to select the applicable gates. Record
hierarchy/wiring, viewport and observed action/result. An offscreen camera render may exclude
screen-space overlay UI; capture a surface that includes the actual Canvas.
Keep compile, automated interaction, agent visual review and required human acceptance separate.
Report each result with evidence or NOT RUN; never prefill a checklist with successful answers.

## Conditional routing
- [ui-projection-pattern](../ui-projection-pattern/SKILL.md): state/projection implementation.
- [ui-modal-stack](../ui-modal-stack/SKILL.md): modal lifecycle and focus.
- [localization-authoring](../localization-authoring/SKILL.md): new player-facing text.
- [gameplay-test-scenario](../gameplay-test-scenario/SKILL.md): required human scenario.
- [Unity auto layout](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/UIAutoLayout.html):
  layout mechanics; verify the installed package version before relying on APIs.

## Boundaries and delivery
No manual YAML, duplicate HUD root or unrelated UI rewrite. Deliver changed binding/creator,
actual interaction and visual evidence, and remaining limitations. Unity unavailable means
in-game observation NOT RUN, not that the projection is unusable.
