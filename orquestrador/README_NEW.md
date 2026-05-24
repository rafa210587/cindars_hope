# SPEC Orchestrator v2.0 - Queue-Based Execution

**New entry point**: `python .\orquestrador\run_orquestrador.py`  
**Previous system**: See `README.md` and `spec_orchestrator.py` (deprecated but functional)

## Overview

The orchestrator v2.0 automates execution of SPECS 1-17 via a queue-based system that:

- **Runs files from directories** - Executes all `.md` files from a folder, one at a time
- **Two modes**: `--mode spec` for specifications, `--mode prompt` for execution prompts
- **Claude/Codex CLI invocation** - Calls agents via subprocess (not API)
- **Structured logging** - Per-item logs with stdout/stderr/validation/git tracking
- **Automatic repair loop** - Retries on validation failures up to `max_repair_attempts`
- **Fallback agents** - Automatically uses Codex if Claude fails on credit error
- **Git integration** - Checks status, diffs, commits after each item
- **Validation** - Docs, Unity compile, repo checks (automatic gates)
- **Dry run mode** - Preview queue without executing

## Quick Start

### 1. Dry run (preview queue)

```bash
python .\orquestrador\run_orquestrador.py `
  --mode spec `
  --input-dir ".\docs\specs\a_implementar" `
  --dry-run
```

Output shows:
- Queue of specs in execution order
- Ignored patterns (if any)
- Which item would run first

### 2. Execute next spec

```bash
python .\orquestrador\run_orquestrador.py `
  --mode spec `
  --input-dir ".\docs\specs\a_implementar" `
  --stop-after-one
```

This runs ONE spec only, validates it, closes it, and commits.

### 3. Execute all specs

```bash
python .\orquestrador\run_orquestrador.py `
  --mode spec `
  --input-dir ".\docs\specs\a_implementar" `
  --stop-on-failure
```

Runs specs in order until completion or failure.

### 4. Execute prompts

```bash
python .\orquestrador\run_orquestrador.py `
  --mode prompt `
  --input-dir ".\docs\agent_prompts\a_executar" `
  --stop-on-failure
```

Prompts are executed in index order (or `SPEC_XX` number fallback).

## Command-Line Parameters

```
--mode spec|prompt              (REQUIRED) What to execute
--input-dir PATH               Directory with .md files
--input-file PATH              Single .md file
--config PATH                  Config JSON (default: orquestrador_config.json)
--primary-agent claude|codex   Primary agent (default: claude)
--fallback-agent claude|codex|none  Fallback (default: codex)
--max-repair-attempts INT      Max repair loops (default: 3)
--stop-after-one               Execute one item only
--stop-on-failure              Stop at first failure (default: True)
--continue-on-partial          Continue despite partial failures
--dry-run                      List queue, don't execute
--no-auto-commit               Skip auto-commit after success
--commit-partial               Commit even if partial
--archive-on-success           Archive prompt on success
--archive-on-failure           Archive prompt on failure
--allow-dirty                  Run with uncommitted changes
```

## Execution Flow

For each item in queue:

```
1. Git status check (fail if dirty, unless --allow-dirty)
2. Read .md content
3. Build prompt with rules + content + agent result block
4. Run primary agent (Claude)
   - Stream stdout/stderr in real-time
   - Save to agent_primary_*.log
   - Check for credit errors
5. If failed, try fallback (Codex)
   - Save to agent_fallback_*.log
6. Parse AGENT_RESULT block from output
7. Run validations:
   - docs_validation.log
   - unity_compile_validation.log
   - repo_checks.log
8. If validations fail, repair loop:
   - Build repair prompt with failure summary
   - Re-run agent
   - Re-run validations
   - Repeat up to max_repair_attempts
9. If all pass:
   - Move spec to implementados/
   - Archive prompt to executados/
   - git add . && git commit
10. Generate summary.json + summary.md
11. Next item
```

## Output Structure

Per execution, logs stored in:

```
orquestrador/logs/<TIMESTAMP>/<ITEM_ID>/
  input_item.md                        # Original .md content
  rendered_prompt.md                   # Full prompt sent to agent
  agent_primary_stdout.log             # Primary agent stdout
  agent_primary_stderr.log             # Primary agent stderr
  agent_primary_combined.log           # Combined stdout+stderr
  agent_fallback_stdout.log            # (if fallback used)
  agent_fallback_stderr.log
  agent_fallback_combined.log
  docs_validation.log                  # Docs validation output
  unity_compile_validation.log         # Unity compile check
  unity_scan.log                       # Unity log scan
  repo_checks.log                      # Repo checks (grep results)
  git_status_before.log                # Git status at start
  git_status_after.log                 # Git status at end
  git_diff_stat.log                    # Git diff --stat
  git_diff.patch                       # Full git diff
  summary.json                         # Machine-readable summary
  summary.md                           # Human-readable summary
```

At the END of all executions:

```
orquestrador/logs/<TIMESTAMP>/
  FINAL_HUMAN_VALIDATION_CHECKLIST.md  # Manual validation checklist
```

## Agent Prompt Structure

Every prompt sent to agents follows this structure:

```
===REGRAS INVIOLÁVEIS DO ORQUESTRADOR===
[mandatory rules from spec]

===CONTEÚDO DO ITEM===
[raw .md content from the item]

===BLOCO FINAL OBRIGATÓRIO===
Ao finalizar, responda EXATAMENTE neste formato:
AGENT_RESULT: SUCCESS | PARTIAL | BLOCKED | FAILED
SPEC_STATUS: COMPLETE | PARTIAL | BLOCKED
CHANGED_FILES:
- path
VALIDATIONS:
- docs: PASS | FAIL | NOT_RUN
- unity_compile: PASS | FAIL | NOT_RUN
- repo_checks: PASS | FAIL | NOT_RUN
NEXT_ACTION: close_spec | repair | human_manual_validation | stop
```

The orchestrator **parses this block** to determine:
- Whether to retry (repair loop)
- Whether to close the spec
- What validation state was reached

## Validation Rules

All three validations must PASS for an item to be marked complete:

- **Docs validation** (`tools/docs/validate_docs.ps1`)
  - Checks documentation integrity
  - Runs crossreference checks
  - PASS if exit code 0

- **Unity compile** (`tools/unity/RunUnityCompileValidation.ps1`)
  - Compiles C# code
  - Catches syntax errors
  - Catches missing references
  - PASS if exit code 0

- **Repo checks** (PowerShell `Select-String`)
  - Grep for forbidden patterns
  - Check for GameObject.Find()
  - Check for unsafe serialization
  - PASS if no forbidden patterns found

## Repair Loop

If any validation fails:

1. Build repair prompt with failure summary
2. Re-run same agent
3. Run validations again
4. If still failing, retry up to `max_repair_attempts` times
5. If all repairs fail, mark item as BLOCKED

Example repair loop:

```
Item: spec_player_combat_weapons_spells_skill_actions_runtime.md
Validation 1: unity_compile FAIL (CompilationError in PlayerAttackController.cs)
  → Repair attempt 1: Agent fixes error
  → Re-validate: PASS
  → Item marked SUCCESS
```

## Fallback Agent Logic

If Claude returns error text matching any `credit_error_patterns`:

- Automatically switch to Codex
- Run with same prompt
- Log fallback_reason
- Mark `used_fallback=True` in summary

Patterns that trigger fallback:

- "Credit balance is too low"
- "insufficient credits"
- "rate limit"
- "quota"
- "sem créditos" (Portuguese)
- etc. (configurable in orquestrador_config.json)

## Directory Structure

```
docs/
  agent_prompts/
    a_executar/             ← New prompts to execute
    executados/             ← Completed prompts (moved here)
    bloqueados/             ← Failed prompts (moved here)
  specs/
    a_implementar/          ← Specs to implement
    implementados/          ← Completed specs (auto-moved here)

orquestrador/
  run_orquestrador.py       ← Entry point (NEW)
  orquestrador_config.json  ← Config (NEW)
  queue.py                  ← Queue management
  agent.py                  ← Agent invocation (Claude/Codex)
  validation.py             ← Validation runners
  logger.py                 ← Structured logging
  spec_operations.py        ← Spec/prompt operations + git
  logs/                     ← Per-run execution logs
    <timestamp>/
      <item_id>/
        [all log files]
```

## Troubleshooting

### "Claude CLI not found in PATH"

Install Claude CLI:
```bash
pip install anthropic-cli
```

### "Dirty working tree"

Either:
1. Commit your changes: `git add . && git commit -m "..."` 
2. Or use: `python run_orquestrador.py ... --allow-dirty`

### Validation failing after multiple repairs

Check the failure logs:
```bash
cat "orquestrador/logs/<timestamp>/<item_id>/unity_compile_validation.log"
```

Fix the code manually, then re-run the orchestrator with the same item.

### Agent timeout

Increase `agent_timeout_minutes` in `orquestrador_config.json`:
```json
"agent_timeout_minutes": 180
```

## Configuration

See `CONFIG_GUIDE.md` for detailed configuration options.

Default config: `orquestrador_config.json`

Critical settings:
- `unity_editor_path` - Must match your Unity installation
- `primary_agent` - "claude" or "codex"
- `fallback_agent` - "codex", "claude", or "none"
- `max_repair_attempts` - 3 is default, increase if needed

## See Also

- `orquestrador_config.json` - Full configuration
- `CONFIG_GUIDE.md` - When to use which config
- `README.md` - Legacy API-based orchestrator (v1.0)
- `CLAUDE.md` - AI agent rules (applies to both systems)
- `docs/specs/SPEC_EXECUTION_ORDER.md` - Spec dependency matrix

---

**Status**: Production ready. All files created, logs functional, validations working.
