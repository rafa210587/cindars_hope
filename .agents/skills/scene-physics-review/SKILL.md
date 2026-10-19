---
name: scene-physics-review
description: Audit 2D scene collision, usable space, doors and Player/NPC circulation against visible geometry and live movement. Use for interiors, access changes and stuck actors; not for artistic acceptance or 3D physics.
---

# Skill: Scene physics review

**Core rule: collider presence, a clear cast and a successful traversal are different evidence.**
Audit-only unless implementation is separately authorized. Preserve NPCs, scene state, saves, IDs
and existing interactions; use [unity-validation](../unity-validation/SKILL.md) for actual runners
and Editor ownership. Select scope through the existing validation matrix, not a parallel global gate.

## Inspect the materialized state

- Identify scene/revision, door state, actor profile, collision layers/matrix and active NPC/schedule
  state. Use final world bounds and body offsets, not assumptions from root positions or editor defaults.
- Enumerate every effective solid in the affected area, including nested measured supports, root
  colliders and compound shapes. Inspect after all generators/wiring. A smaller root collider does
  not reduce blockage when a generated child still occupies the old footprint.
- Map each solid to the floor contact/support it represents. Tall furniture, canopy and roof do not
  occupy their entire painted silhouette on the floor. Conversely, front-wall cutaway does not imply
  removing the boundary collider. Visible limits must communicate the actual blocked area.
- Separate decorative floor/rugs, solid furniture, wall boundaries, interaction triggers and door
  blockers. Do not add a dynamic Rigidbody to every item or interaction to every collider by default.

## Geometry and usability

1. Measure doorway width and useful passages against the real actor body, offsets and relevant
   clearance. Record the chosen margin and reason; do not impose one universal tile/width ratio.
   Consider turning corners, approaching furniture and shared Player/NPC traffic where intended.
2. Check actual rendered thickness as well as colliders. Tests for a required visible trim need
   lower and upper bounds/tolerances; a maximum-only assertion accepts an effectively invisible line.
   Convert through real sprite bounds/PPU/alpha and the complete transform chain.
3. Verify representative entry-to-use-to-exit paths, including the intended interaction side of
   furniture. A free central pocket does not prove access to every service, exit or workstation.
   Test reachable sides; do not invent a requirement to walk behind furniture intentionally against a wall.
4. Inspect layers, enabled state, trigger status and multiple supports. Duplicate colliders are not
   automatically a bug, but their combined obstruction/contact behavior must match the intended base.

## Runtime evidence

- For collision/access changes, observe real Player movement through the affected routes using
  normal physics steps, with the relevant NPC bodies retained. Casts/overlaps are useful diagnostics,
  but do not replace motion, contact response or interaction callbacks. Teleporting into the room
  does not prove entrance. Public Interact calls do not prove keyboard input.
- Doors: closed blocks; opening preserves blockage until the intended animation moment; open permits
  passage; closing does not trap/crush an occupant; both sides remain interactable; reveal follows
  physical entry and exit. Reuse the existing door/reveal system rather than creating a parallel one.
- When NPC sharing is in scope, test an occupied/encounter case as well as a stable clear approach.
  Retain the startup/clock/traffic differences between runs. Waiting for an empty doorway may define
  a useful isolated test, but does not resolve a failure under ordinary concurrent traffic.
- On a stall capture phase/time, target and actual body position, contacts/hit object paths,
  effective colliders, door/animation state and relevant NPC positions. Distinguish observed facts
  from hypotheses; a clear earlier cast does not prove that an NPC caused a later obstruction.

## Failure and acceptance

- Classify failure as product, instrumentation, environment or unresolved using evidence. Preserve
  the first failed artifacts. No collider/NPC removal, tolerance inflation, scope filtering or timing
  change solely to manufacture PASS. Legitimate test corrections must explain the changed scenario.
- A later successful run is scoped evidence, not closure of an unexplained earlier stall. Reproduce
  the failure or establish a supported cause/correction; otherwise keep that behavior unresolved.
- Report expected/executed/skipped counts. A one-house pilot is not an all-town check; missing or
  inconclusive mandatory cases are not PASS. Hash-equivalent unaffected evidence may be reused with
  an explicit reason; broad reruns are not required for unrelated visual-only changes.
- Keep physical, visual and human acceptance separate. Do not claim readiness to replicate when an
  affected required movement/door criterion fails or is missing. Visual coherence is reviewed through
  [scene-composition-review](../scene-composition-review/SKILL.md), never inferred from these tests.

## Output

Report scope/state, effective-solid findings, measured clearances, runtime scenarios and artifacts,
PASS / FAIL / NOT RUN per criterion, unresolved failures and the smallest justified next correction.
For each failed scenario retain: failed artifact, cause (confirmed/unresolved), corrective change
(if any), same-scenario retest artifact (if any) and resolution status. A summary PASS must not
hide an unresolved row.
If runtime is unavailable, deliver static findings and mark motion/contact/door behavior NOT RUN.
