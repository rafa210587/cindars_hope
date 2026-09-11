---
name: spec-planning
description: Builds the technical implementation plan for an existing specification, including reuse, concrete contracts, file ownership and validation. Use after requirements are defined; macro wave selection belongs to plan-wave and task decomposition to spec-task-authoring.
---

# Skill: Spec planning

**Choose how to satisfy a requirement without silently changing it.**

Read the [SDD contract](../spec-authoring/references/sdd-workflow.md) for stage boundaries.
Read the target spec and only its relevant source files; inspect existing implementations.

1. Record the spec revision and evidence baseline. Map each requirement/acceptance ID to
   existing code, required delta and the chosen project pattern skill.
2. Define CREATE versus MODIFY ownership, real paths, existing versus proposed signatures,
   lifecycle, failure behavior, ordering and save compatibility. Do not invent an existing API.
3. Explain reuse decisions. Avoid parallel managers, duplicate catalogs and business logic in views.
4. Specify per-method edits and pseudocode for nontrivial transactions or formulas.
   Identify shared-file locks and dependent contracts before suggesting parallel execution.
5. Select meaningful tests, asset validation and observable Play Mode/visual scenarios
   through existing validation rules. Distinguish expected outcomes from executed evidence.
6. Record unresolved decisions and risks. A requirements change returns to spec-authoring;
   a lore/balance decision returns to refinement-authoring. Continue unaffected planning.

Write a clearly named `Plan` section in the canonical spec by default. Existing deep-spec
architecture/contracts/strategy sections already constitute a plan; do not duplicate them.
Use a separately linked companion only if a consumer requires it, outside the executable queue.
Keep the package self-contained and satisfy the spec-authoring Depth Gate before readiness.

Output: plan, covered acceptance IDs, ownership/dependency boundaries and remaining blockers.
Do not implement code or mark proposed contracts as already present.
