# Behavioral seam plan — {{RESPONSIBILITY}}

Copy for an authorized refactoring that needs an explicit contract and risk. It does not require a new interface or class.

- Spec and allowed files: {{VERIFIED_CONTRACT_AND_PATHS}}
- Concrete problem: {{MIXED_REASON_TO_CHANGE}}
- State and current owner: {{STATE_AND_OWNER}}
- Actual consumers: {{CONFIRMED_CONSUMERS}}
- Reuse: {{EXISTING_SEAM_OR_EXTRACTION_REASON}}
- Smaller local solution considered: {{ALTERNATIVE_AND_DECISION}}
- Before → after: {{MOVING_RESPONSIBILITY_AND_REMAINING_FACADE}}

| Observable case | Input/state | Result and effects that must remain | Relevant evidence |
|---|---|---|---|
| {{CASE}} | {{INPUT}} | {{CONTRACT}} | {{EXISTING_OR_PLANNED_TEST}} |

- Preserve: {{APPLICABLE_API_SERIALIZATION_IDS_EVENTS_LIFECYCLE}}
- Minimal sequence: {{EDITS_AND_INTEGRATION}}
- Gates: {{MATRIX_SELECTION_AND_REUSABLE_EVIDENCE}}
- Main risk: {{BEHAVIOR_AT_RISK}}
- Scope/stop boundary: {{BOUNDARY}}
- Actual result after execution: {{RESULTS_AND_EXISTING_PATHS_OR_PENDING_ITEMS}}.
