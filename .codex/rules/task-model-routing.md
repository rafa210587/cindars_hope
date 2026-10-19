# Task model routing

Before executing a task/spec or delegating a cohesive slice, select the most economical
available model that can meet its quality and risk requirements. This project authorizes
per-task model selection and escalation without repeated approval. An explicit user model
choice and higher-priority host restrictions take precedence.

## Selection

- Assess requirement clarity, cross-system dependencies, regression impact and available
  validation from the context already required by the task. Keep triage brief; do not load
  project history or perform a separate research task for each selection.
- Select per cohesive task/slice, not necessarily for the whole spec. Do not split trivial
  edits or spawn agents solely to change models when coordination costs outweigh the gain.
- Use this initial Codex candidate mapping only when these IDs are available on the host.
  It is a starting hypothesis, not a project benchmark or a verified price ranking:

| Work profile | Initial candidate |
|---|---|
| Local documentation, catalog edits following exact examples, running established validators | `gpt-5.6-luna` |
| Isolated bug with known cause, bounded logic with behavior tests, established wiring | `gpt-5.6-terra` |
| Multi-component implementation, unknown-cause regression, gameplay integration | `gpt-5.6-sol` |
| Architecture decisions, save/schema changes, Cave determinism/snapshots, consequential ambiguity, critical review | `gpt-6-astra` |

- Terra is the initial candidate for ordinary bounded implementation. Risk overrides size:
  a one-line save contract change can require Astra. Running save tests alone does not.
- For other providers or unavailable IDs, use supported models and known capabilities;
  do not translate aliases or invent an equivalent. Without evidence for a safe economical
  alternative, retain the configured capable model and state the uncertainty briefly.
- Choose supported reasoning effort separately, preserving role-specific constraints.
  Lower effort does not establish a cheaper model or equivalent quality.

## Applying the decision and escalating

- Apply an explicit model override through supported execution/delegation controls when
  authorized and useful. Respect context inheritance and role restrictions of those tools.
  Do not claim the active model changed merely because this rule or a config file changed.
- If no supported control can apply the selection, continue authorized work on the current
  model and report the limitation once. Do not create a new user-owned task just to switch.
- Record chosen model (or inherited/unknown), short rationale and escalation trigger in the
  existing task/delegation note; a small edit needs at most one sentence, no separate report.
- Reassess immediately when new dependencies or critical contracts emerge. Escalate on
  contract violations, unresolved reasoning difficulties, or recurrence of the same
  implementation failure after one focused correction. Preserve useful work and hand off
  the relevant diff, failed evidence and remaining criteria rather than restarting blindly.
- Infrastructure failures, missing Unity connections and unrelated baseline failures need
  environment/validation triage, not automatic model escalation. User-input conflicts still
  follow the project's conflict rules; a stronger model cannot resolve missing authorization.

## Quality and evidence

Keep scope, architecture, independent risk-based review and validation gates unchanged.
Compilation alone does not establish gameplay or visual acceptance. Review actual artifacts
and evidence regardless of the implementer's model.

Use representative completed tasks to calibrate this mapping: first-pass acceptance,
corrections, review findings, total elapsed time and measured usage/cost when available.
Include coordination and retries; do not claim savings from model names, effort or file size.
No benchmark is required for each task, and no economy percentage is assumed.

Enforcement is instructional routing plus supported host model controls and existing quality
gates. This rule does not install an automatic model-switching hook.
