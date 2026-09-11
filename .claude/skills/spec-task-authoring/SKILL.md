---
name: spec-task-authoring
description: Converts a reviewed specification and technical plan into ordered, traceable execution tasks and checks cross-artifact consistency. Use to prepare or regenerate tasks before execution, not to invent requirements or implement the feature.
---

# Skill: Spec task authoring

**Every task has a requirement; every acceptance criterion has an execution or verification task.**

Read the [SDD contract](../spec-authoring/references/sdd-workflow.md) when establishing a package.

1. Confirm the spec and plan revisions match. Flag changed requirements/contracts instead
   of producing tasks against stale inputs. Never infer approval from document existence.
2. Write stable task IDs, unchecked completion boxes, owning files/methods, concrete edits,
   prerequisites, acceptance IDs and evidence expected at completion.
3. Order foundational contracts before consumers, migration before dependent restoration,
   generated assets after their authoring tools, and acceptance after scene integration.
4. Mark parallel work only for disjoint ownership and settled shared contracts. Provide
   a serial default when tasks touch the same file. Test work may stay with the implementer.
5. Include meaningful failure/edge-case verification and closeout through existing workflows.
   Do not create a test per class, placeholder task quotas or implementation-mirroring tests.
6. Review consistency: no orphan requirement/task, no dependency cycle, no unresolved
   design choice disguised as an implementation detail, and no assertion of unrun validation.
   Return each inconsistency to its owning stage, then regenerate only affected tasks.

Write `Tasks` in the canonical spec by default; use a compact table when clearer. For a
complex task list use the [task format](assets/templates/tasks.md). The format is optional.
Only mark completion with inspected artifacts and evidence. Drafting tasks never runs them.

Output: ordered tasks, requirement coverage, parallel boundaries and readiness assessment.
Execution routes to `spec-execution` / `/execute-spec-strict`; closeout to `/finish-spec`.
