---
paths:
  - ".specs/**"
---
# Rule: Spec Quality Gate

> Selection, evidence and status: [canonical matrix](../../.specs/SPEC_VALIDATION_MATRIX_MASTER.md).
> Procedure: [spec-execution](../skills/spec-execution/SKILL.md).

Each spec declares exactly one blocking acceptance gate: the existing test, command or
executed scenario that proves its acceptance criteria. Include every applicable save,
migration, stable-ID, secrets, architecture invariants and data-integrity check in that gate; none may be waived
as informational. Other checks are informational and cannot independently block acceptance.
A failed or unexecuted acceptance gate prevents acceptance; compile alone does not prove
behavior. Evidence format follows the matrix; a separate report is not always required.
