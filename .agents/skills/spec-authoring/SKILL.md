---
name: spec-authoring
description: Authors executable specs with scope, contracts and verifiable criteria proportional to risk. Use to create/refine a spec or break down a wave; complex changes use a detailed blueprint, while cohesive maintenance may use a short format.
---

# Skill: Deep spec authoring

For staged SDD work, use the [project SDD contract](references/sdd-workflow.md).
`refinement-authoring` establishes evidence and design choices; this skill owns requirements
and acceptance; `spec-planning` owns technical design; `spec-task-authoring` owns execution
breakdown and consistency review. One canonical spec may contain all three artifacts.
The depth requirements below apply to the completed package, not to an unfinished first
requirements draft. Keep incomplete packages outside the executable queue and label them.

Precedent: strong specs (`spec_arch_*_residual_v1` batch) are blueprints with rich headers, named contracts, exact files and evidence-backed criteria. Weak ones (`spec_world_002`) are vague bullets. This skill raises the **floor** to the stronger level so Claude **and** Codex execute consistently.

**Core rule: a strong spec is an EXECUTABLE BLUEPRINT prescribing WHAT (signatures, classes to create vs. modify, EDITS per file, pseudocode when nontrivial, binary criteria with Definition of Done) and NAMING the pattern through a project skill; NEVER reteach the pattern. Complex specs pass the Depth Gate before entering `.specs/a_implementar/`; any failed item means they are not ready.**

## When to use
- Create a spec, refine/reject a shallow one; `/start-spec`; break down a wave; plan a batch for Claude or Codex.
- Whenever a criterion is prose ("works well", "polished") instead of binary with a command.

## Proportional selection before the blueprint

Small, cohesive maintenance may use a short spec with objective, scope/allowed files,
expected behavior, risk and verifiable evidence. Do not require already-existing signatures,
a method/class plan, artificial pattern, per-class test or deep template for this case.
The gates and deep template below apply to integration/feature specs or complex changes
that need that precision. Visual evidence may be compared captures/playback with observable
criteria; it need not become literal command output. Do not preset validation results to PASS
before execution. Preserve approval, safety, reuse and ownership in both formats.

## Depth by spec TYPE (complex blueprint)

| Type | Required depth | §16 Contracts |
|---|---|---|
| **Runtime / Data / UI** | signatures + classes to create/modify + EDITS per phase + named tests | required |
| **Validation / Docs** | EXACT deliverable structure (sections/fields) + evidence paths + "generated" DoD | justified N/A |
| **Tooling** | script interface (parameters/output) + idempotence + expected exit codes | partial |
| **Governance** | the decision + where it is recorded (ADR/game_rule) + what becomes invalid | N/A justified |

## The standard — prescribe vs. delegate

| PRESCRIBE (specific to this change) | DELEGATE (do not reteach) |
|---|---|
| Contracts with **actual signatures** (interface, method, DTO, save fields, ID `const`) | the generic pattern implementation → **name the skill** (`use registry-catalog-pattern, template X`) |
| Classes to **CREATE** (name + responsibility) vs. **MODIFY** (file + change) | pattern rationale/structure (belongs in the skill/rule) |
| Plan **per EDIT** (change in each named method/block) + pseudocode when nontrivial | C#/hot-path style → `csharp-style`/`non-regression-review` |
| **Binary criterion + DoD** (command + expected literal output) | testing conventions → `editmode-test-authoring` |

## Additional depth (complex blueprint)
1. **Per-edit step:** each phase states the concrete edit ("in `X.Metodo()`, after `<bloco>`, iterate `<def>` calling `<api>`"), not "edit X".
2. **DoD per criterion:** command + **literal expected output** (`ValidateEnemyAttackKits` prints `0 error(s)`, previously `43`). Before→after numbers.
3. **Named tests:** each test specifies its method name, assertion and expected value.
4. **Edge cases / failures:** section `## 23` — what can fail and how the spec handles it (ID collision, missing parent, destructive regeneration, public shape changes).
5. **Pseudocode:** include a skeleton for nontrivial algorithms (formula, order, condition).

## Depth Gate (complex blueprint; justify non-applicable items)
```
[ ] Complete header: Spec ID, Wave, Type, Domain, Priority, Parallelizable + Repo lock scope, Depends/Blocks, Scope/Out-of-scope, Validation level, Executor
[ ] §9 Repo state = ACTUAL Phase 0 (today's commands + results/counts), not "audit later"
[ ] TYPE depth met (see matrix): code→§16 with SIGNATURE; validation/docs→exact deliverable structure
[ ] Classes to CREATE (responsibility) and MODIFY (change) listed
[ ] §20 Phased strategy WITH edits per file/method + NAMED pattern + pseudocode where nontrivial
[ ] Every criterion is BINARY with DoD = command + expected LITERAL OUTPUT (before→after counts when applicable)
[ ] Named tests with assertion + expected value (when the type requires tests)
[ ] §23 Edge cases / failures present and nonempty
[ ] §18/19 Specific allowed/forbidden files; validation level through `validation-truth`
[ ] Non-duplication rule (§13) names existing systems that MUST NOT be recreated
[ ] Self-contained: executable by Claude AND Codex without this conversation
```

## Authoring procedure
1. **Actual Phase 0** — Grep/Read/validators; fill §9 with true state (files, lines, IDs, counts, commands).
2. **Choose TYPE** and apply the corresponding matrix column.
3. **Contracts first** (§16) — exact signatures before the plan (or deliverable structure for Validation/Docs).
4. **Per-EDIT plan** (§15/17/20) — CREATE vs. MODIFY; pattern-skill per system; pseudocode when nontrivial.
5. **Binary criteria + DoD** (§14) — command + literal output + before→after counts. Fill §23 edge cases.
6. **Parallelization/lock header** (§3/4); run the **Gate**; only then save in `.specs/a_implementar/`. The `sync-harness-and-tracing` hook regenerates `SPEC_INDEX` at Stop.

Complex blueprint foundation (use sections relevant to the contract):
- **Deep template:** `.specs/_templates/SPEC_DEEP_TEMPLATE.md`
- **Worked example:** `.specs/_templates/EXAMPLE_spec_content_enemy_status_kit_ids_v1.md`

## On-demand resources

When you need a reusable document, open the [template](assets/templates/maintenance-spec.md).
To calibrate its contents, consult the [hypothetical example](references/examples/maintenance-spec-example.md).
Do not load both by default; examples neither prove execution nor grant authorization.

## When NOT to use
- Execute/implement a ready spec → `spec-execution`.
- Create skill/rule/agent/command/hook → `harness-authoring`.
- Move a spec to `implementados/` with evidence → `docs-migration`.
- Plan the macro wave (which specs will exist) → `/plan-wave` + `SPEC_GENERATION_ROADMAP_MASTER.md`.

## When to stop and report
- Phase 0 reveals the system exists or differs from the request → report before writing (avoid specs that recreate it).
- Scope is too large for isolated execution → split into N specs with Depends/Blocks between them.

## Related
- `.specs/_templates/SPEC_DEEP_TEMPLATE.md` · `.specs/_templates/EXAMPLE_spec_content_enemy_status_kit_ids_v1.md`
- `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md` — canonical skeleton (the deep version is a superset).
- `(skill: spec-execution)` · `(skill: system-reuse-audit)` · `(rule: validation-truth)` · `(rule: testing-quality-gate)` · `(rule: code-minimalism-ladder)`
