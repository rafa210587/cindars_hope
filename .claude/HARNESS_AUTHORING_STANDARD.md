# Harness Authoring Standard — Cindar's Hope

Canonical source for skills, commands, rules, hooks and agents in `.claude/`.
Codex copies are generated; never hand-edit `.agents/`, `.codex/` or the generated AGENTS block.

## Core principle
The entry point contains purpose, essential criteria and routing. Mode-specific detail lives
in conditional references. Read only what the current task needs; links are not a mandatory
recursive reading list.

## Language and identifiers
- Use English for new or migrated AI-facing prose, templates and examples. Keep one canonical
  version; do not include parallel translations in the loaded instructions. Legacy Portuguese
  files remain valid until migrated; language alone is not an execution blocker.
- Keep conversation, human reports, game localization and commits in their requested language.
  This project retains Portuguese commits. English instructions do not require English replies.
- Preserve `name` IDs, tool names, paths, commands, regex, schema keys and status tokens.
- Keep Unity, MonoBehaviour, ScriptableObject, event bus, save, DTO, seed, runtime, lifecycle,
  prefab, Scene, Play Mode, EditMode, wiring, frontmatter and commit terminology.
- Never translate canonical `skill`, `rule`, `agent` references, class/spec names, UI/menu
  literals or metadata markers consumed by existing validators.

## Progressive discovery
- Register artifacts in [HARNESS_INDEX.md](HARNESS_INDEX.md), reachable from CLAUDE.
  Do not repeat the full catalog in each consumer's entry point.
- Descriptions distinguish real triggers from neighboring workflows; avoid catchalls.
- `SKILL.md` presents a useful decision before details; use a compact checklist for execution criteria.
- Put references in `references/`, with relative links and explicit reading conditions.
  Do not add a support file unless it changes decisions in a concrete scenario.
- Line/byte counts are review signals, not standalone gates or token/cost measurements.
  Do not pad to a quota or move text merely to satisfy a size limit.

## Artifact formats
### Templates, examples and scripts
- `assets/templates/`: models copied/adapted into deliverables. Use `{{field}}` placeholders;
  explain how to fill them and remove inapplicable fields. Never preset successful results.
- `references/examples/`: filled cases illustrating decisions, tradeoffs or common mistakes.
  Label hypothetical cases; real evidence needs a verifiable source. Examples do not establish
  canonical resolution, frame counts, scale, architecture or balance values.
- `scripts/`: only necessary reusable automation, with relevant tests. Reuse existing tools;
  do not add scripts merely to complete a directory structure.
- Entry points state when each resource helps. Templates support production; examples explain
  decisions. Never load both automatically or duplicate the template inside the example.
- Check `.specs/_templates` and existing workflows before creating a model. Simple skills may
  remain self-contained; having every possible resource directory is not a gate.

### Contracts
- Skill: `name` and `description` frontmatter; no `version`/`when_to_use`; `# Skill: ...` title.
- Agent: functional `name`, `description`, `tools`; `# Agent: ...` title; role, limits,
  expected evidence and conditional skills. Model names do not establish visual capability or cost.
- Command: no frontmatter; `# /name` title, `$ARGUMENTS`, procedure and reviewable output.
- Rule: invariant and actual enforcement; justified constraint rather than duplicate tutorial.
- Hook: separate messages/instructions from logic. Behavioral changes need tests; translation
  does not authorize accidental changes to flags, regex, paths, exit codes or contracts.
- Type-specific models and criteria: [artifact reference](skills/harness-authoring/references/artifact-types.md),
  only when creating or reviewing that artifact type.

## Proportionate selection
- Complete small tasks directly when ownership and risk allow; delegate cohesive slices when
  they provide independent work or useful review. No agent-per-file/class requirement.
- Implementers may write small tests for their changes. Independent review follows regression
  risk, critical contracts or broad integration; auditors do not implement their own fixes.
- Reasoning effort does not prove a cheaper model, guaranteed quality or unavailable images.
  Check actual capabilities and report unobserved gates.
- Preserve behavior through seam, failure and side-effect tests; no test-per-class, interface
  quotas or manual headers/frontmatter on every class.

## Validation and authorization
- Check frontmatter, transitive references, catalog, generated copies and idempotence when affected.
- Exercise scenarios proportionate to the change; correct formatting does not prove good routing.
- Derive results from current evidence; never hardcode PASS/NOT RUN in an execution template.
- Agent summaries do not replace artifacts. Safety, save, scope and evidence remain binding.
- Audit-only requests return findings. Already authorized edits continue without renewed approval;
  skill instructions never expand scope or authorize external actions.
- Harness-only changes do not require Unity; test changed scripts and record the actual scope.
