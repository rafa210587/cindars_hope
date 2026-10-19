---
name: event-bus-pattern
description: Define or review cross-system gameplay requests, facts and subscriptions through GameEventBus, with explicit mutation ownership and lifecycle.
---

# Skill: Event bus pattern

**Cross-system gameplay uses `GameEventBus.Publish` / `Subscribe`; one owner commits state.**
Local helpers/policies and injected queries inside the owner boundary remain explicit calls.
Bootstrap wiring and Unity callbacks are not gameplay messages.

## Request, commit, fact

- Discover existing events and subscribers before adding a type. Use pure DTO payloads with
  stable IDs/simple values, no Unity references; preserve existing IDs and APIs.
- A request expresses intent; its authoritative owner validates, commits or rejects it.
  A completed fact describes state already committed. Never mutate damage in response to
  `DamageAppliedEvent`: that can double-apply it and make correctness depend on listener order.
- Conceptual flow (names illustrate semantics, not a request to add duplicate event types):
  `DamageRequestedEvent` → damage owner validates/applies → `DamageAppliedEvent` → HUD/audio.
  Rejected requests produce the existing failure feedback and no completed fact.
- Commit all related state before publishing facts. Transactions keep inventory/gold/job
  changes atomic; splitting their mutation among subscribers is not a transaction.
- Check actual dispatch semantics. A synchronous bus is not a queue: callbacks may reenter
  the owner before Publish returns. Do not assume listener order or automatic deduplication.
  Prevent request/fact feedback loops; use existing operation IDs when retries need idempotency.

## Lifetime

- Unity adapters pair Subscribe in OnEnable with Unsubscribe in OnDisable (or the existing
  matching OnDestroy contract). Pure subscribers have explicit ownership/disposal.
- Use the same delegate instance; avoid unremovable anonymous handlers. Rebinding must not
  duplicate subscriptions. Filter targeted messages by ID and keep callbacks bounded.
- Tests clean static subscriptions in `finally`/TearDown even when assertions fail.
  When static state or boot changes, cover repeated entry/exit using the project's actual
  domain-reload setting. Reuse existing reset/composition paths; do not add auto-bootstraps.

## Evidence

Test the owner's observable commit/failure behavior and affected subscribers, not just DTO
field assignment: wrong target ignored; valid mutation once; rejection leaves state intact;
completed fact observes committed state; disable/re-enable does not multiply callbacks.
Add reentrancy/retry/scene-transition cases when those contracts change. Use the canonical
validation matrix; EditMode alone does not certify scene lifetime or UI wiring.

## References, only when relevant

- [Event queue tradeoffs](https://gameprogrammingpatterns.com/event-queue.html): global state
  and feedback loops; the local GameEventBus is not assumed to implement this queue pattern.
- [Unity domain reload](https://docs.unity.cn/Manual/DomainReloading.html): static event cleanup.
- Local rules: `unity-architecture`; skills: `action-feedback-pipeline`, `audio-event-wiring`,
  `editmode-test-authoring`. External examples do not override project invariants.
