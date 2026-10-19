# Artifact formats

Consult only the type being created or reviewed. Sections are tools for clarity,
not a requirement to insert empty headings or reach a line count.

## Skill
Minimal frontmatter:
```yaml
---
name: example-skill
description: Describe the action and concrete trigger in English.
---
```
The ID must match the folder and remain kebab-case. The description participates in discovery
before the body is read. Avoid `version` and `when_to_use`; requirements belong in the description/body.

Entry: `# Skill: ...` title, actual context, essential rule, procedure and output.
Use a checklist for execution criteria; state exclusions when they prevent misrouting.
For distinct modes, use links such as "for playback, read references/playback.md".
Do not add glossaries/manuals/copies of available rules unless the mode requires them.

## Agent
```yaml
---
name: example-reviewer
description: Reviews the target contract without implementing corrections.
tools: Read, Glob, Grep, Bash
---
```
Preserve existing agents' functional tools unless an explicit capability change is in scope.
Bash is a source-format tool name, not authorization for Unix; this project uses PowerShell.
Document Role, minimal input, ownership, allowed/forbidden edits, evidence and output.
An auditor reports findings; a validator may write evidence if its role allows, without fixing gameplay.
A model name/alias proves neither image access nor price. Check session capabilities;
if the tool cannot inspect the artifact, mark the corresponding gate as not executed.
Skills use canonical IDs and are loaded only when applicable.

## Command
No frontmatter; `# /nome`, purpose, `$ARGUMENTS`, procedure and reviewable output.
Call the canonical skill instead of repeating its rules/tables. Audit-only is the default for
an audit request; explicitly authorized corrections need no invented additional confirmation.
Do not preset PASS/FAIL/NOT RUN in execution instructions; results depend on evidence.

## Rule
`# Rule: ...` title, concrete invariant, scope and actual enforcement.
Do not turn a local example into a universal prohibition or repeat an entire skill tutorial.
For a stub rule, preserve the invariant and a working detail link.

## Hook
Preserve stdin/stdout, schema, flags, exit codes and settings integration.
When changing behavior, test relevant scenarios: success, violation, missing/invalid input
where supported, and repetition/idempotence when state exists. Parsing proves syntax only.
Do not incidentally change regex or logic during translation. Do not weaken safety
to obtain PASS. Never write invented status or suppress stderr/actual failures.

## Shared closeout
Register the artifact in HARNESS_INDEX; transitive references must exist in source and copy
if the generator publishes them. Generate only after owners freeze sources; check the diff and a second run.
One test per file is not required: choose cases that detect regression in the affected contract.
