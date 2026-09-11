# Validation report — {{TASK}}

Fill only applicable gates. This template contains no approved results and does not replace the canonical matrix.

- Scope: {{GLOBAL_OR_SCOPED_AND_BOUNDARIES}}
- Inputs: {{COMMIT_OR_DIRTY_HASHES_FILTER_CONFIGURATION}}
- Environment/tool: {{CONFIRMED_VERSION}}
- Reused evidence: {{EXISTING_PATH_AND_RATIONALE_OR_NONE}}

| Selected gate | Observed result | Existing evidence / reason | Limit |
|---|---|---|---|
| {{GATE}} | {{PASS_FAIL_NOT_RUN_NOT_APPLICABLE}} | {{ACTUAL_PATH_OR_REASON}} | {{RESIDUAL_RISK}} |

For execution: command actually attempted {{COMMAND}}, exit {{ACTUAL_EXIT}}, log/XML {{EXISTING_ARTIFACT}}, cases/filter {{OBSERVED_COUNTS}}.
For FAIL: record cause and next step {{FAILURE_AND_ACTION}}, without reclassifying it as NOT RUN.
For NOT RUN: reason {{OBSTACLE}}, attempted command {{COMMAND_OR_NONE}}, risk {{RISK}}.
For NOT APPLICABLE: justify {{WHY_SCOPE_DOES_NOT_REQUIRE_THIS_GATE}}.

- Compile, tests, incorporated scan, validators, Player and human acceptance: state only what evidence supports, without duplicate execution.
- Later changes invalidating evidence: {{AFFECTED_INPUTS_OR_NONE}}.
- Pending closeout work: {{PENDING_ITEMS}}. Do not automatically promote the spec.
