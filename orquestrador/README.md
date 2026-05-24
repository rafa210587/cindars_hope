# Orchestrator v2.0 - Spec/Prompt Queue Automation

**Canonical command:**

```powershell
python .\orquestrador\run_orquestrador.py
```

---

## Quick Start

### Dry run - list specs without executing

```powershell
python .\orquestrador\run_orquestrador.py `
  --mode spec `
  --input-dir ".\docs\specs\a_implementar" `
  --dry-run
```

### Dry run - list prompts without executing

```powershell
python .\orquestrador\run_orquestrador.py `
  --mode prompt `
  --input-dir ".\docs\agent_prompts\a_executar" `
  --dry-run
```

### Execute a single spec

```powershell
python .\orquestrador\run_orquestrador.py `
  --mode spec `
  --input-file ".\docs\specs\a_implementar\spec_player_combat_weapons_spells_skill_actions_runtime.md" `
  --stop-after-one
```

---

## Modes

### `--mode spec`

Executes specs from a directory in dependency order (from `SPEC_EXECUTION_ORDER.md`).

- Reads `.md` files from `--input-dir`
- Orders by spec number from execution order file
- For each spec:
  - Builds prompt with mandatory rules and result block
  - Invokes Claude or Codex via subprocess
  - Fallback to Codex on credit errors or failures
  - Repair loop for failed validations (3 attempts by default)
  - Auto-closes spec (moves to `implementados/`) on success
  - Auto-commits on success

### `--mode prompt`

Executes custom prompts from a directory.

- Reads `.md` files from `--input-dir`
- Infers target spec from:
  - Explicit path in prompt content
  - `SPEC_XX` extracted from filename
  - Lookup in `SPEC_EXECUTION_ORDER.md`
- Same execution as spec mode, but:
  - Archives prompt to `executados/` (success) or `bloqueados/` (failure)
  - Spec closes only if explicitly required
  - Prompts don't auto-replace spec execution

---

## Input Options

### `--input-dir PATH`

Execute all `.md` files in directory. Files ordered by:
1. SPEC_EXECUTION_ORDER.md (for spec mode) or 00_INDEX_ORDEM_USO.md (for prompt mode)
2. Fallback: filename number extraction
3. Fallback: alphabetical

### `--input-file PATH`

Execute single file only. Useful for targeted execution.

---

## Execution Control

### `--stop-after-one`

Execute only first item, then stop. Useful for testing.

### `--stop-on-failure` (default: true)

Stop if any item fails or is partial. Use `--continue-on-partial` to ignore.

### `--continue-on-partial`

Continue to next item even if current is partial.

### `--dry-run`

List queue without executing. Shows:
- Queue order
- Item count
- Ignored patterns (prompt mode)

---

## Agent Control

### `--primary-agent claude|codex` (default: claude)

Primary agent to use.

### `--fallback-agent claude|codex|none` (default: codex)

Fallback agent on credit error or primary failure.

### `--max-repair-attempts INT` (default: 3)

Maximum repair loop iterations for failed validations.

---

## Git Control

### `--allow-dirty`

Allow execution even if working tree has uncommitted changes. Default: error if dirty.

### `--no-auto-commit`

Don't auto-commit on success. Useful for preview runs.

### `--commit-partial`

Commit even on partial success. Used with `--continue-on-partial`.

---

## Archive Control

### `--archive-on-success`

Archive prompt to `executados/` on success (default: true for prompts).

### `--archive-on-failure`

Archive prompt to `bloqueados/` on failure. Default: false.

---

## Success Criteria

An item is **successful** when ALL of these pass:

1. `AGENT_RESULT = SUCCESS` (from agent result block)
2. `SPEC_STATUS = COMPLETE` (from agent result block)
3. Docs validation: PASS
4. Unity compile validation: PASS
5. Repo checks: PASS
6. For prompt mode: target spec found and closed

If ANY condition fails:
- Spec is NOT moved to `implementados/`
- Prompt is NOT archived as executed
- If `--stop-on-failure`, execution stops
- Summary saved with failure details

---

## Validations

### Docs Validation

Runs `tools/docs/validate_docs.ps1`. Checks:
- Markdown syntax
- Missing references
- Broken links

### Unity Compile Validation

Runs `tools/unity/RunUnityCompileValidation.ps1`. Checks:
- C# syntax errors
- Missing assemblies
- Compilation success

### Repo Checks

Runs global and spec-specific checks:
- `GameObject.Find()` usage (forbidden)
- `FindObjectOfType()` usage (forbidden)
- `QuestManager` usage (legacy, forbidden)
- Spec-specific patterns (for SPEC_012, SPEC_016, etc.)

---

## Repair Loop

If validations fail:

1. Agent invoked again with failure details
2. Agent asked to fix issues
3. Validations re-run
4. Loop repeats up to `--max-repair-attempts` (default: 3)

If all repair attempts exhaust and validations still fail:
- Item marked partial
- Spec NOT closed
- Prompt NOT archived as success

---

## Fallback: Claude → Codex

If primary agent fails with:
- Credit/quota error
- CLI not found
- Timeout

Automatically retries with fallback agent (default: Codex).

Fallback behavior:
- Logged in summary
- Both stdout/stderr captured separately
- If fallback succeeds, item succeeds
- If fallback also fails, item fails

---

## Prompts vs Specs

**Specs** in `docs/specs/a_implementar/`:
- Official feature definitions
- Executable in dependency order
- Must close (move to `implementados/`)
- Multiple prompts can target same spec

**Prompts** in `docs/agent_prompts/a_executar/`:
- Custom instructions or repairs
- Infer target spec automatically
- Archive to `executados/` or `bloqueados/` on completion
- Prompts do NOT bypass spec execution

---

## Logs

Each item generates structured logs:

```
orquestrador/logs/<timestamp>/<item_id>/
  input_item.md                    - Original item
  rendered_prompt.md               - Full prompt sent to agent
  agent_primary_stdout.log         - Primary agent stdout
  agent_primary_stderr.log         - Primary agent stderr
  agent_primary_combined.log       - Combined output
  agent_fallback_stdout.log        - (if fallback used)
  agent_fallback_stderr.log        - (if fallback used)
  agent_fallback_combined.log      - (if fallback used)
  docs_validation.log              - Docs check output
  unity_compile_validation.log     - Unity compile output
  unity_scan.log                   - Unity error scan
  repo_checks.log                  - Global repo checks
  repo_checks_SPEC_012.log         - (spec-specific checks)
  git_status_before.log            - Working tree status
  git_status_after.log             - Working tree after execution
  git_diff_stat.log                - Statistics of changes
  git_diff.patch                   - Full diff
  summary.json                     - Structured summary
  summary.md                        - Human-readable summary
```

---

## Configuration

Config file: `orquestrador/orquestrador_config.json`

Key settings:
- `repo_root`: Project root (default: ".")
- `spec_execution_order_path`: Path to SPEC_EXECUTION_ORDER.md
- `implemented_specs_dir`: Where to move closed specs
- `primary_agent`, `fallback_agent`: Agent choices
- `agent_timeout_minutes`: Timeout for agent execution
- `require_docs_validation`, `require_unity_compile`, `require_repo_checks`: Enable/disable validators
- `ignored_prompt_patterns`: Patterns to ignore in prompt mode
- `credit_error_patterns`: Patterns that trigger fallback

---

## Troubleshooting

### "Claude Code CLI not found"

Verify installation:
```powershell
claude --version
claude auth status --text
```

If missing:
```powershell
# Verify CLI is in PATH or install if needed
```

### "Codex CLI not found"

Verify installation:
```powershell
npm install -g @openai/codex
codex --version
```

### "Working tree is dirty"

Commit or stash changes:
```powershell
git add .
git commit -m "..."
```

Or use `--allow-dirty` to force execution (not recommended).

### Timeout

Increase `--max-repair-attempts` or adjust config `agent_timeout_minutes` if legitimate long operations.

### Validation failures

Check logs:
```powershell
# View specific validation failure
cat orquestrador/logs/<timestamp>/<item_id>/docs_validation.log
cat orquestrador/logs/<timestamp>/<item_id>/unity_compile_validation.log
```

---

## Legacy Compatibility

Old command (deprecated):
```powershell
python .\orquestrador\run_orchestrator.py
```

This is now a wrapper that delegates to `run_orquestrador.py`. Do not use directly.

---

## Next Steps

1. Review logs in `orquestrador/logs/<timestamp>/`
2. Check `FINAL_HUMAN_VALIDATION_CHECKLIST.md` for manual testing
3. Test in Play Mode for gameplay feel
4. Review commits: `git log --oneline -<N>`
