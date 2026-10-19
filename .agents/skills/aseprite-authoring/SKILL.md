---
name: aseprite-authoring
description: Edit existing pixel art in Aseprite as layered candidates using CLI and Lua, preserving source, animation and import contracts. Use for authorized sprite cleanup, palette corrections or pose edits; route generation and Unity integration separately.
---

# Skill: Aseprite authoring

Choose a concrete visual correction before choosing an operation. More pixels, colors or frames do not establish better art. Preserve the approved identity and judge the result at its actual display size.

## Essential checklist
- [ ] Authorized source, intended correction and output ownership identified; prior edits preserved.
- [ ] Source visually inspected and its dimensions, color mode, frames/timing, layers, tags, slices and pivot contract recorded.
- [ ] Original remains intact; edits live in a separate, editable `.aseprite` candidate with meaningful layers.
- [ ] CLI/Lua capability checked against installed Aseprite; source hash unchanged after execution.
- [ ] Candidate reopened; transparency, metadata and native/game-scale appearance compared with baseline.

## Procedure
1. Read only the target art and relevant contract. For unclear style or scale, use [pixel-art-direction](../pixel-art-direction/SKILL.md). Do not infer a resolution or frame count from an example.
2. Locate Aseprite and inspect its version/help. Prefer existing CLI and Lua for repeatable edits. One owner writes each document/candidate; preserve unsaved human edits. Native UI automation may be unavailable: do not claim UI inspection. Use an optional MCP only for an identified operation or demonstrated workflow benefit, following [adapter contracts](references/adapters.md); do not install infrastructure speculatively.
3. For batch execution and pixel edits, read [CLI and Lua](references/cli-lua.md). For a bounded handoff or multi-step edit, fill the [work order](assets/templates/work-order.md); replace placeholders and remove inapplicable fields.
4. Make a small candidate first. Preserve canvas bounds, alpha semantics, layer structure, frame order/durations, tags, slice keys, pivots and external import metadata unless the authorized correction specifically changes them. A flat PNG supplies no lost layers: keep its baseline layer and put new changes on named layers without claiming reconstructed provenance.
5. Read [acceptance checks](references/acceptance.md) before accepting a candidate. Use [visual-asset-review](../visual-asset-review/SKILL.md) for static fidelity and [sprite-animation-review](../sprite-animation-review/SKILL.md) when motion changes.
6. Deliver candidate and previews with observed improvements, preserved contracts and unresolved limitations. Continue already authorized edits; do not add an approval gate. Route authorized Unity import work through [sprite-scene-integration](../sprite-scene-integration/SKILL.md).

## Boundaries
This skill does not authorize runtime, scene, prefab, importer or `.meta` changes. Image generation belongs to its generation workflow. Do not flatten the editable deliverable, replace source art implicitly, auto-trim, repack, rescale or manufacture extra animation frames as generic enhancement.
