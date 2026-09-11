# {{MAINTENANCE_TITLE}}

Short template for cohesive maintenance, with minimal registration markers. Replace fields and remove this guidance; use the existing deep blueprint if complexity requires it.

> Status: {{ACTUAL_STATUS}}
> Type: {{TYPE}}
> Domain: {{DOMAIN}}
> Authorization: {{ACTUAL_REQUEST_OR_APPROVAL}}
> Ownership: {{OWNER_AND_COORDINATION}}
> Ordem de execucao: {{MINIMAL_SEQUENCE}}
> Depende de: {{ACTUAL_DEPENDENCIES_OR_NONE}}
> Bloqueia: {{BLOCKED_CONSUMERS_OR_NONE}}

required_adrs: {{ACTUAL_ID_LIST_OR_EMPTY_ARRAY}}
required_game_rules: {{ACTUAL_ID_LIST_OR_EMPTY_ARRAY}}

# /speckit.specify

## Objective and confirmed state
{{OBSERVED_PROBLEM_AND_EXPECTED_RESULT}}. Observation source: {{INSPECTED_FILES_OR_ACTUAL_EVIDENCE}}.

# /speckit.plan

## Scope
May edit: {{EXACT_PATHS}}. Out of scope: {{BOUNDARIES}}. Preserve concurrent dirty changes.
Reuse/smallest change: {{EXISTING_ARTIFACT_AND_MINIMAL_EDIT}}.

# /speckit.tasks

## Criteria and execution
- [ ] {{OBSERVABLE_BEHAVIOR_AND_VERIFICATION}}
- [ ] {{RELEVANT_INVARIANT_OR_FAILURE}}
Minimal plan: {{EDITS_PER_FILE_IF_NEEDED}}.

## Validation and risk
Gates through the matrix: {{APPLICABLE_GATES_AND_RATIONALE}}.
Planned evidence: {{PLANNED_TYPE_AND_DESTINATION_NOT_OBTAINED_RESULT}}.
Risk/boundary: {{RISK_AND_STOP_CONDITION}}.
Actual results and closeout: {{FILL_AFTER_EXECUTION_WITHOUT_PRESET_PASS}}.
