---
name: non-regression-review
description: Review an implementation diff for architecture, scope, save, lifecycle and gameplay regression risks before closeout. Audit only; use after runtime or rule changes.
---

# Skill: Non-regression review

Audit the actual diff and evidence before closeout. Return findings; do not implement fixes.

## Essential decision
- Use for runtime/code changes, changed rules, save/cross-system work or when the spec requires an independent review.
- Skip for docs-only or asset-only work without code; use their applicable governance/review workflow.
- Establish scope and baseline first. Existing debt is a finding only when the change worsens it,
  relies on it unsafely or falsely claims to resolve it.
- Rank findings by user impact and concrete regression path. A pattern preference without a
  failing contract is not a blocking finding.

## Procedure
1. Read the target spec, changed files and applicable rules only.
2. For audit dimensions, evidence requirements and report shape, read
   [review procedure](references/review-procedure.md).
3. Read [C# and gameplay risk catalog](references/risk-catalog.md) only for affected paths
   involving failure handling, hot paths, design patterns or balance values.
4. Compare implementation and tests against every in-scope acceptance criterion. Green tests do
   not prove omitted behavior; stale evidence does not prove the current diff.
5. Return tight file/line findings with priority, impact and smallest plausible correction.

Use the canonical validation matrix for gates. Keep automated, visual and human acceptance
separate. Do not mutate runtime, assets, specs or reports during this audit.
