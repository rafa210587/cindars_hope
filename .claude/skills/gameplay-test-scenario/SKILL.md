---
name: gameplay-test-scenario
description: Describe an executed or planned gameplay scenario when scene, input, lifecycle or human acceptance requires integration evidence. Not required for pure logic or a local correction covered by focused tests.
---

# Skill: Gameplay test scenario

Describe the observable behavior when integration or human acceptance needs a scenario.
A written scenario is a plan, never proof of PASS or human acceptance.

## Quando usar

Scene/lifecycle/input/UI, combat, save, cave, farm or shop integration needs behavior coverage
under the canonical `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` acceptance gate.

## Quando NÃO usar

Docs/harness use their diff; pure logic/local corrections use relevant focused evidence
and the exit code in the task result or authorized commit. Bugfixes affecting integration
still require that integration evidence; the label bugfix does not waive lifecycle/input/save risks.

## Procedimento

1. Read the changed criteria and existing test evidence; reuse a relevant wave scenario.
2. Put integration setup, action and expected result in one paragraph in the existing spec
   report. If a report is needed, use `docs/validation/<spec_id>/REPORT.md`, at most 60 lines.
3. Cover the happy path and affected failure boundaries. For save changes, retain real
   save/load, migration, unknown-ID and recovery checks; do not trade correctness for brevity.
4. State scene, initial items/stats/flags, exact actions, expected state/feedback and console
   expectations. Keep a scenario focused (1–5 minutes); share setup across related criteria.
5. Record PASS/FAIL/NOT RUN, actual tester and result. Console errors prevent that test's PASS.
6. Link this paragraph or the existing wave scenario from /finish-spec; no extra scenario file.

## Regras

- One folder per spec; overwrite reruns, no timestamp or run subfolders.
- Only scenes with approved keyart may produce one comparative evidence image.
- No .unity/.prefab/.asset snapshots/backups; cite commit hash. No default index.html.
- Keep raw test outputs ignored and available; record command, inputs, XML and exit code.
- Human acceptance remains separate when the selected acceptance gate requires it.

## Validação

Check that steps exercise the changed behavior and that results come from an actual run.
A deferred scenario stays NOT RUN; do not prefill successful checklist items.

## Quando parar e reportar

Unclear acceptance, scope expansion, or inaccessible required behavior: state the missing
input and residual risk. Debug retries follow the existing visual-iteration-budget rule.
