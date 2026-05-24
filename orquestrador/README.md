# Orchestrator v2.1 - Folder-Agnostic Spec/Prompt Queue Automation

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

### Execute a single item

```powershell
python .\orquestrador\run_orquestrador.py `
  --mode prompt `
  --input-file ".\docs\agent_prompts\a_executar\my_prompt.md" `
  --stop-after-one
```

---

## Folder-Agnostic Behavior

The orchestrator now works with **any folder structure**:

- Reads `.md` files from `--input-dir`
- Moves completed items to `<input-dir>/implementado/`
- Moves blocked items (on failure) to `<input-dir>/bloqueado/`
- No hardcoded paths, no `SPEC_EXECUTION_ORDER.md` dependency
- Works identically for specs, prompts, or any markdown documents

### Destination Folders

| Status | Destination | Custom Arg |
|---|---|---|
| Success | `<input-dir>/implementado/` | `--completed-subdir` |
| Failure (if `--archive-on-failure`) | `<input-dir>/bloqueado/` | `--blocked-subdir` |
| Failure (default) | Stays in `<input-dir>` | — |

---

## Modes

### `--mode spec`

Executes specs from a directory in sort order.

- Reads `.md` files from `--input-dir`
- Orders by `--sort` mode (default: natural)
- For each spec:
  - Builds prompt with mandatory rules and result block
  - Invokes Claude or Codex via subprocess
  - Fallback to Codex on credit errors or failures
  - Repair loop for failed validations (3 attempts by default)
  - Auto-moves to `implementado/` on success
  - Auto-commits on success

### `--mode prompt`

Executes custom prompts from a directory.

- Reads `.md` files from `--input-dir`
- Infers target spec from:
  - Explicit path in prompt content (optional)
  - `SPEC_XX` extracted from filename
  - Lookup in `SPEC_EXECUTION_ORDER.md` (if exists)
- Same execution as spec mode, but:
  - Archives to `implementado/` (success) or `bloqueado/` (failure, with `--archive-on-failure`)
  - Target spec is **optional** (for logging only, not a gate)

---

## Input Options

### `--input-dir PATH`

Execute all eligible `.md` files in directory. Files ordered by `--sort` mode.

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
- Ignored items and patterns

---

## Sorting

### `--sort natural` (default)

Natural sort: `SPEC_01 < SPEC_02 < SPEC_10`, not lexicographic.

Special handling: `SPEC_17A < SPEC_17` (letter suffix = prerequisite version).

### `--sort alpha`

Alphabetical by filename (case-insensitive).

### `--sort index`

Uses `00_INDEX_ORDEM_USO.md` in input directory for custom order.

Requires that index file to exist.

---

## File Filtering

### Default Ignored Patterns

```
00_INDEX_ORDEM_USO
00_PROMPT_MESTRE
00B_PROMPT_AUXILIAR
99_TEMPLATE
README
```

These are skipped unless `--include-all-md` is used.

### `--include-all-md`

Include all `.md` files, even those matching ignored patterns.

### Custom Ignored Patterns

Add to `orquestrador_config.json`:

```json
{
  "ignored_prompt_patterns": ["draft_", "archived_", "wip_"]
}
```

---

## Folder Customization

### `--completed-subdir NAME` (default: implementado)

Subdirectory name for successful items.

```powershell
python .\orquestrador\run_orquestrador.py `
  --mode prompt `
  --input-dir ".\my_folder" `
  --completed-subdir "done"
```

Result: `.\my_folder\done\item.md`

### `--blocked-subdir NAME` (default: bloqueado)

Subdirectory name for blocked items (with `--archive-on-failure`).

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

Archive prompt to `completado/` on success (default: true for prompts).

### `--archive-on-failure`

Archive prompt to `bloqueado/` on failure. Default: false.

---

## Success Criteria

An item is **successful** when ALL of these pass:

1. `AGENT_RESULT = SUCCESS` (from agent result block)
2. `SPEC_STATUS = COMPLETE` (from agent result block)
3. Docs validation: PASS
4. Unity compile validation: PASS
5. Repo checks: PASS

If ANY condition fails:
- Item is NOT moved to `implementado/`
- Item is NOT archived as executed
- If `--stop-on-failure`, execution stops
- Summary saved with failure details

**Note**: In prompt mode, target spec is optional (inferred for logging only, not a gate).

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
- Spec-specific patterns (if SPEC_EXECUTION_ORDER.md exists)

---

## Repair Loop

If validations fail:

1. Agent invoked again with failure details
2. Agent asked to fix issues
3. Validations re-run
4. Loop repeats up to `--max-repair-attempts` (default: 3)

If all repair attempts exhaust and validations still fail:
- Item marked partial
- Item NOT moved to `implementado/`
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

orquestrador/logs/<timestamp>/
  RUN_SUMMARY.json                 - Run-level statistics
  RUN_SUMMARY.md                   - Run-level summary
  FINAL_HUMAN_VALIDATION_CHECKLIST.md - Manual testing checklist
```

---

## Run Summary

After execution, `RUN_SUMMARY.md` shows:

- Total items found and executed
- Items moved to `implementado/`
- Items moved to `bloqueado/`
- Items kept in origin (failed, no archive)
- Next suggested item (if stopped early)
- Sorting mode used

---

## Configuration

Config file: `orquestrador/orquestrador_config.json`

Key settings:
- `repo_root`: Project root (default: ".")
- `spec_execution_order_path`: Path to SPEC_EXECUTION_ORDER.md (optional)
- `default_sort`: Default sort mode (default: "natural")
- `completed_subdir`: Default destination for completed items (default: "implementado")
- `blocked_subdir`: Default destination for blocked items (default: "bloqueado")
- `include_all_md`: Include all .md files by default (default: false)
- `primary_agent`, `fallback_agent`: Agent choices
- `agent_timeout_minutes`: Timeout for agent execution
- `require_docs_validation`, `require_unity_compile`, `require_repo_checks`: Enable/disable validators
- `ignored_prompt_patterns`: Patterns to ignore in prompt mode
- `credit_error_patterns`: Patterns that trigger fallback
- `close_target_spec`: Close target spec after prompt (legacy, default: false)
- `update_spec_registries`: Update registries after spec close (legacy, default: false)

---

## Troubleshooting

### "Claude Code CLI not found"

Verify installation:
```powershell
claude --version
claude auth status --text
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

Increase `--max-repair-attempts` or adjust config `agent_timeout_minutes`.

### Validation failures

Check logs:
```powershell
cat orquestrador/logs/<timestamp>/<item_id>/docs_validation.log
cat orquestrador/logs/<timestamp>/<item_id>/unity_compile_validation.log
```

### Items not moving to destination

- Check `--completed-subdir` / `--blocked-subdir` spelling
- Verify folder permissions
- Check logs for move errors

---

## Examples

### Execute all prompts in custom folder, natural sort, archive failures

```powershell
python .\orquestrador\run_orquestrador.py `
  --mode prompt `
  --input-dir ".\my_prompts" `
  --sort natural `
  --archive-on-failure
```

Result: Success items → `.\my_prompts\implementado\`, failed items → `.\my_prompts\bloqueado\`

### Execute specs with index order from 00_INDEX_ORDEM_USO.md

```powershell
python .\orquestrador\run_orquestrador.py `
  --mode spec `
  --input-dir ".\docs\specs\a_implementar" `
  --sort index
```

### Alphabetical sort, custom destination names

```powershell
python .\orquestrador\run_orquestrador.py `
  --mode prompt `
  --input-dir ".\requests" `
  --sort alpha `
  --completed-subdir "approved" `
  --blocked-subdir "rejected"
```

Result: `.\requests\approved\` and `.\requests\rejected\`

### Include index file and template files

```powershell
python .\orquestrador\run_orquestrador.py `
  --mode prompt `
  --input-dir ".\docs\agent_prompts\a_executar" `
  --include-all-md
```

---

## Migration from v2.0

Old hardcoded paths are now folder-agnostic:

| v2.0 | v2.1 |
|---|---|
| Specs must be in `docs/specs/a_implementar/` | Any folder with `--input-dir` |
| Completed → `docs/specs/implementados/` | Completed → `<input-dir>/implementado/` |
| Requires `SPEC_EXECUTION_ORDER.md` | Optional; use `--sort natural` (default) |
| Target spec required in prompt mode | Optional; inferred for logging only |

To migrate:

```powershell
# Old way
python .\orquestrador\run_orquestrador.py --mode spec --input-dir ".\docs\specs\a_implementar" --dry-run

# New way (same effect)
python .\orquestrador\run_orquestrador.py --mode spec --input-dir ".\docs\specs\a_implementar" --sort natural --dry-run

# New way (any folder)
python .\orquestrador\run_orquestrador.py --mode spec --input-dir ".\my_custom_folder" --sort natural --dry-run
```

---

## Real-Time Monitoring & Observability

### Quick Start: 5-Tab Auto-Monitoring

Auto-open 5 PowerShell tabs with all monitoring streams:

```powershell
.\orquestrador\run_with_monitoring.ps1 `
  -Mode prompt `
  -InputDir ".\docs\agent_prompts\a_executar"
```

Opens automatically:
1. **Tab 1** - Main orchestrator execution
2. **Tab 2** - Live monitor (status updates every 2s)
3. **Tab 3** - Claude raw output stream (80-line tail)
4. **Tab 4** - Git watch stream (30-line tail)
5. **Tab 5** - Ask mode (interactive guidance)

```powershell
# With options
.\orquestrador\run_with_monitoring.ps1 `
  -Mode prompt `
  -InputDir ".\docs\agent_prompts\a_executar" `
  -NoAutoCommit `
  -StopOnFailure
```

### Manual Setup (3+ Terminals)

If you prefer manual control:

```powershell
# Terminal 1: Main execution
python .\orquestrador\run_orquestrador.py `
  --mode prompt `
  --input-dir ".\docs\agent_prompts\a_executar" `
  --no-auto-commit

# Terminal 2: Monitor status
python .\orquestrador\run_orquestrador.py --monitor

# Terminal 3: Stream Claude output
$run = Get-ChildItem .\orquestrador\logs -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$item = Get-ChildItem $run.FullName -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Get-Content "$($item.FullName)\agent_primary_combined.log" -Wait -Tail 80

# Terminal 4 (optional): Watch git changes
Get-Content "$($item.FullName)\git_watch.log" -Wait -Tail 30
```

### Control Execution

Stop gracefully:
```powershell
New-Item .\orquestrador\state\STOP -ItemType File -Force
```

Pause after current item:
```powershell
New-Item .\orquestrador\state\PAUSE -ItemType File -Force
```

### Ask Mode - Send Questions Mid-Execution

**Already open in Tab 5!** When you run the 5-tab setup, Tab 5 is an interactive terminal where you can type questions/hints **WITHOUT stopping execution**:

```powershell
# In Tab 5, just type (no need for ask_agent.ps1):

> Usa GameEventBus para comunicação entre sistemas

> O que falta para passar nas validações?

> Verifica se estás usando padrão _SO para ScriptableObjects

> Diz [TRACE] dos próximos passos que vais fazer

> Nunca use GameObject.Find() - viola regra inviolável

> Como vamos resolver o erro X?
```

Tab 5 will:
1. Accept your input
2. Log it as event + command file
3. Display confirmation
4. Monitor for Claude's response
5. Stay open for next message

This is perfect for **real-time code guidance** - like a code review happening live without interrupting Claude!

#### Manual Ask (if not using 5-tab setup)

```powershell
# Manual - if you prefer to send individual questions
.\orquestrador\ask_agent.ps1 "Usa GameEventBus para comunicação"
```

### Log Files Generated

Each item generates real-time logs at `orquestrador/logs/<timestamp>/<item_id>/`:

- `agent_primary_stdout.log` - Agent output (tail -Wait)
- `agent_primary_combined.log` - Combined output with timestamps
- `events.jsonl` - JSON event stream (all events)
- `timeline.md` - Human-readable timeline
- `status.json` - Current status (read by monitor)
- `heartbeat.log` - Periodic heartbeat with elapsed time
- `git_watch.log` - Periodic git status/diff snapshot
- `changed_files_live.txt` - Changed file count

### Configuration

Enable streaming observability in `orquestrador_config.json`:

```json
{
  "enable_streaming_observability": true,
  "agent_no_output_timeout_minutes": 10,
  "heartbeat_interval_seconds": 5,
  "git_watch_interval_seconds": 10
}
```

---

## Next Steps

1. Review logs in `orquestrador/logs/<timestamp>/`
2. Check `RUN_SUMMARY.md` for execution statistics
3. Check `FINAL_HUMAN_VALIDATION_CHECKLIST.md` for manual testing
4. Test in Play Mode for gameplay feel
5. Review commits: `git log --oneline -<N>`
