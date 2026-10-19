# Refinement — Farm boat and animal motion v1

> Status: PROPOSED — refinement requested; no runtime implementation or spec approval
> Baseline: Farm v17 boat; existing FarmAnimalRuntime and AnimalReleaseHandler
> Scope: restrained ambient motion and readable owned-animal behavior

## Outcome and evidence

Make the farm feel alive without visible sliding, synchronized crowds or changes to its care economy.
- CreateMvpFarmScene selects boat_keyart_v17, anchors its support(35,24) at(17.4,-9.1); canvas76x68, effective18.3sourcepixels/worldunit. Ground/order5. Boat has no motion here.
- FarmAmbientAnimationAuthoring already builds native sprite clips/controllers and compares colliders; extend this pipeline rather than add an ambient manager.
- FarmAnimalRuntime already alternates pause/wander in FixedUpdate, sets velocity, uses global UnityEngine.Random and clamps the transform center relative to each spawn anchor. There is no obstacle-aware target selection or blocked recovery in that path.
- AnimalReleaseHandler.SpawnAnimalRuntime creates a SpriteRenderer with species color but no sprite assignment in this method, a trigger BoxCollider2D and a kinematic Rigidbody2D. Existing triggers do not prove obstacle blocking. Spawn offsets also shift the runtime anchor, so limits are not automatically shared housing limits.
- FarmAnimalRegistry, AnimalCareService, AnimalDailyProcessor and AnimalProductCollectionService own identity/care/products. Preserve them and FarmAnimalsSaveData.
- Static cow/sheep keyart exists; availability of complete directional animation sheets is NOT established. Static scenery animals must not be mistaken for owned instances.

## Decisions proposed

### B01 — Boat: quiet native sprite cycle
Use corrected v17 as immutable baseline. Aseprite layered candidate, fixed canvas/support and closed stern in every frame. Start with4distinct poses over4–6seconds, maximum1sourcepixel displacement from baseline in either axis; prefer vertical bob/contact-shadow variation. At current gameplay camera1sourcepixel is about1.54screenpixels, so do not call this1screenpixel. Hold timings/easing-shaped dwell create calm motion; do not add frames unless1x review shows stepping. No arbitrary rotation, scale pulsing or subpixel filtering. A subtle lake relationship does not require exact phase-lock to every ripple.
Animate only sprite pixels/native visual child. Anchor, dock, boat interaction/collision geometry and lake remain fixed. No accumulated transform drift. If content reaches canvas edge, reject the candidate or explicitly revise padding/pivot contract before integration. No wake suggesting propulsion.

### A01 — Reuse existing ownership and motion path
One visual actor per owned AnimalInstanceId; the same assembly path on release and restore. No free animals added to a new save, no duplicate decorative instance in the same spot. Stop/despawn visuals according to canonical dead/unavailable/removal behavior. Do not introduce a second registry or independent care system.

### A02 — Small state machine with cosmetic actions
Idle -> Walk -> Idle/Forage/Rest, with weighted durations per species. Feeding/collection briefly stops motion and keeps the actor reachable; then resumes if allowed. Dead/unavailable stop; hunger appearance remains canonical. Forage/pecking does NOT set FedToday, consume feed, heal or advance product readiness. Rest is a daytime presentation state, not a new sleep schedule.
Initial tuning hypotheses, not validated balance: chicken walk0.5–0.8u/s in0.4–1.2u bouts, idle2–5s, peck2–4s, rest4–8s. Cow/sheep use slower pace and longer pauses after pilot review. Store tunables in a presentation/motion SO under existing data conventions, not hardcoded MonoBehaviour content.

### A03 — Prevent sliding and false direction
Chicken pilot: idle2frames, walk4frames per authored direction, peck3frames, rest2frames as initial budgets. Require front/back/side coverage for actual travel; mirror lateral direction only if anatomy/light/accessories permit. Reuse identical cels; frame counts are not minimum quality quotas. Cow/sheep need species-specific foot-contact review before reuse of timings.
Walk phase follows actual distance travelled, not requested velocity. A blocked animal stops walk playback; feet remain anchored in idle/peck/rest. Keep consistent PPU, foot pivot and body scale; avoid translating a static sprite as the finished animation.

### A04 — Shared pen geometry and bounded recovery
Use housing/world-space pen bounds shrunk by the full body footprint, not each animal's shifted spawn center. Author boundaries against visible fence/door/feeding lane. Sweep intended motion against relevant static solids/water/fences; reject unreachable targets. Preserve separate interaction trigger. Decide exact Unity body implementation during spec against existing controller conventions; a trigger alone cannot be the blocking body.
Use a short blocked timeout and bounded target retries; on exhaustion idle and retry later. No visible teleport/clamp as normal locomotion. Validate spawn/restore position before activation. Animals avoid one another with local yielding/separation; they must not trap the player in narrow access lanes. Pilot recommendation: animal navigation respects solids while player/animal body blocking is disabled; interaction still works. Physical player pushing is excluded.

### A05 — Small responsibilities, no economy RNG coupling
Extract testable state/target decisions only as needed; retain thin Unity motion/presentation adapter and existing interaction path. Local movement/animation coordination need not publish every frame through GameEventBus. Gameplay care/collection continues through existing services/events.
Use a presentation-only per-instance random source seeded by stable ID (not randomized string.GetHashCode), independent from loot/economy RNG. Bounded checks on state changes; no pathfinding service, per-frame allocations or world scans. Movement pose/timer is transient: no save schema change. Reload may choose a fresh safe presentation pose while ownership/care persist.

## Comparison and scope

Keeping static art is cheapest but fails the intended movement. Pure bobbing of animals would look like sliding. Extending the existing wander path with proper sprite presentation and obstacle-aware motion is the recommended bounded solution. Full behavior trees/NavMesh/flocking, breeding, new hunger/feeding rules, schedules and new animal products are excluded.

## Candidate spec slices and dependencies

1. Boat: layered4pose candidate -> native clip wiring ->1x integrated comparison, using v17 art. Independent of animal code.
2. Chicken pilot: audit release/restore and pen geometry -> existing-controller decomposition only where necessary -> motion profile and authored directional clips -> focused tests and PlayMode capture.
3. Cow/sheep: reuse validated contracts, author species-specific clips and tune profile. Do not generate all sheets before pilot confirms perspective/contact.
Use aseprite-authoring, pixel-art-animator, sprite-animation-review, state-machine-design and sprite-scene-integration on demand. One art executor and one independent visual reviewer; one Unity owner. No new MCP/tool installation required.

## Validation and stopping criteria

Boat: inspect every pose for complete alpha silhouette; capture at least2cycles at gameplay scale; no snap at loop, no drift, no collider/anchor change. Static frame sampling alone does not prove smoothness.
Animals: meaningful deterministic tests for state transitions, health stop, full-body bounds, blocked timeout/retry limit and independent RNG. Reuse care/product/save tests for unchanged inputs; run affected release/restore tests if assembly changes. Do not create one test per sprite frame or rerun all farm tests for timing-only edits.
One reproducible PlayMode scenario: released chicken walks/pauses/pecks/rests; approaches fence, water, building and another animal; stops when blocked; remains interactable; save/restore shows one actor perID with care/product unchanged. Use seeded setup, movement/contact evidence and a short representative60–90s observation; include actual5–10s walks at1x rather than only stills. Verify pause/game speed respects existing world convention.
Compare before/after in one HTML. At most2candidate attempts without visible gain before changing the approach. Scoped tests are not human visual acceptance.

## Remaining decisions for spec

Confirm actual shared pen geometry and mapping of decorative vs owned animals; inspect available directional art before estimating asset count. Proposed nonblocking player/animal policy and timing ranges need explicit carry-forward in spec, not silent canon. No user answer is needed to finish this refinement. Next stage: spec/plan/tasks from these proposals, then implementation authorization according to session scope.

## Independent design review

Verdict:SOUND_WITH_AMENDMENTS. Incorporated concerns: cosmetic forage must not imply actual feeding; body-aware housing bounds and reachable interaction at full capacity; boat amplitude judged in screen space over two cycles. Added acceptance: visual-state traversal leaves FedToday/CareScore/products/inventory unchanged; hungry feeding prompt remains correct while pecking; full housing cannot permanently obstruct access/collection. These are requirements for the next spec, not executed tests.

Execution successor: .specs/a_implementar/spec_farm_boat_chicken_motion_v18.md, authorized and reviewed. Proposed-status header describes refinement stage only.
