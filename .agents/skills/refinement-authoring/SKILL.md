---
name: refinement-authoring
description: Refines a gameplay or product idea into evidence-backed decisions before specification. Use for design review, capability proposals, balance reasoning or turning an ambiguous request into scoped requirements; does not implement gameplay.
---

# Skill: Refinement authoring

**Resolve why a change is needed before prescribing how to build it.**

Read the [SDD contract](../spec-authoring/references/sdd-workflow.md) when moving between stages.

1. Establish the intended player outcome, current behavior and relevant canonical decisions.
   Inspect the actual runtime path, not only catalogs or historical completion claims.
2. Separate observed facts, accepted rules, proposed changes and unresolved choices.
   For gameplay, relate the proposal to Vaalara and the Farm/Town/Cave loops.
3. Compare keeping, repairing and extending the existing mechanic. Recommend the smallest
   coherent change; explain rejected alternatives. A new node needs a distinct use scenario.
4. For balance, state formulas, legal build budgets, stacking, costs, duration, opportunity
   cost and counterplay. Test boundary cases and exploits. Analytical bounds are not playtest proof.
5. Produce a human-readable refinement in `docs/refinements/a_implementar/`, using its
   existing refinement metadata. Include source evidence, decisions with stable IDs,
   proposed scope, exclusions, open questions and candidate spec slices/dependencies.
6. Hand off resolved requirements to `spec-authoring`. Carry unresolved material decisions
   explicitly; do not quietly turn a proposal into approved canon or block independent work.

No runtime edits or automatic spec approval. Reuse `game-design-reviewer` when delegation
is authorized and independent design review is useful; model selection follows the user.

Output: refinement path, supported recommendation, uncertainties and next stage.
