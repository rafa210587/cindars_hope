# SPEC Orchestrator - Cindar's Hope

Automated orchestration system for executing SPECS 1-17 in the Cindar's Hope project with dependency tracking, state persistence, and Claude API integration.

## Overview

The orchestrator solves the core problem that individual Claude sessions have context limits and cannot reliably complete large, multi-stage implementations. It:

- **Tracks spec completion state** across sessions in persistent JSON
- **Understands dependencies** - respects SPEC_EXECUTION_ORDER_MATRIX.md blocking relationships
- **Breaks large specs into subtasks** (~1.5-2.5 hours each, completable in single Claude session)
- **Calls Claude API** for each subtask with complete context
- **Validates after each spec** using Unity compile checks and doc validation
- **Resumes from last checkpoint** if interrupted
- **Guarantees completude** - ensures all 17 specs are fully implemented

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                   run_orchestrator.py                        │
│              (Main orchestration workflow)                   │
└─────────────────────────────────────────────────────────────┘
                    ↓                 ↓
        ┌───────────────────────┬─────────────────┐
        ↓                       ↓                 ↓
┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐
│ spec_orchestrator│  │ claude_api_      │  │   config.    │
│     .py          │  │ integration.py   │  │    json      │
└──────────────────┘  └──────────────────┘  └──────────────┘
  - State mgmt         - Claude API calls    - Settings
  - Dependencies       - Prompt building     - Validation
  - Task tracking      - Response parsing    - API config
```

## Installation

```bash
cd orquestrador
pip install -r requirements.txt
```

## Configuration

Edit `config.json` to configure:

- **API Model**: Default `claude-opus-4-7` (most capable)
- **Validation**: Enable/disable Unity compile checks
- **Parallelization**: Which specs can run in parallel (currently safe: SPECS 005-007)
- **Output**: Log files and state persistence locations

## Usage

### 1. Check Status
```bash
python run_orchestrator.py --command status
```
Shows which specs are done, in progress, pending, or blocked.

### 2. Execute Next Pending Spec
```bash
python run_orchestrator.py --command next
```
Automatically finds the next spec that's ready (all dependencies met) and executes it.

### 3. Execute All Specs (Full Run)
```bash
python run_orchestrator.py --command run
```
Executes all pending specs in dependency order until complete or failure.

### 4. Execute from Specific Spec
```bash
python run_orchestrator.py --command run --start-spec SPEC_012
```
Resume from SPEC_012 if previous run was interrupted.

### 5. Limit Execution
```bash
python run_orchestrator.py --command run --max-specs 3
```
Execute maximum 3 specs then stop (useful for testing).

### 6. Reset State
```bash
python run_orchestrator.py --command reset
```
Clears `orchestrator_state.json` and starts from scratch.

## Spec Execution Flow

For each spec:

1. **Dependency Check**: Verify all blocking specs are completed
2. **Task Breakdown**: Identify subtasks (e.g., SPEC_012 → 8 tasks)
3. **Claude Invocation**: Call API with detailed task prompt
4. **Task Execution**: Claude implements feature in project
5. **Validation**: Run Unity compilation + doc checks
6. **State Update**: Mark task complete, recalculate completion %
7. **Commit**: Claude commits changes with Portuguese message
8. **Move to Next**: Loop until spec complete

## Spec Definitions

All 17 specs defined with:
- **Dependencies**: What blocks this spec, what it blocks
- **Tasks**: Granular subtasks (~2 hours each max)
- **Validation**: How to verify completion

### SPEC Dependency Graph

```
SPEC_001 (Event Bus) → blocks 002,003,004,009
    ↓
SPEC_002 (Bootstrap) → blocks 005,006,010
SPEC_003 (IDs/Registries) → blocks 005,006,007
SPEC_004 (Damage) → blocks 012,013
    ↓
SPEC_005 (Farm Movement)
SPEC_006 (Equipment) ────────┐
SPEC_007 (Stamina) ──────────┼─→ SPEC_012 (Player Combat)
SPEC_008 (Status Effects) ──────→ SPEC_013 (Enemy AI)
SPEC_009 (GameTime) ──────────┤
SPEC_010 (SaveManager) ───────┤
SPEC_011 (UI Framework) ──────┘
    ↓
SPEC_012 → blocks SPEC_015
SPEC_013 → blocks SPEC_014
SPEC_014 → blocks SPEC_016
SPEC_015 → blocks SPEC_016
SPEC_016 → blocks SPEC_017
    ↓
SPEC_017 (Integration & Polish)
```

## SPEC_012 Implementation (Example)

SPEC_012 breaks down into 8 tasks:
1. Wire ManaManager to GameBootstrap
2. Integrate ManaManager into SaveManager
3. Implement E key IInteractable priority logic
4. Create ArcaneBolt spell asset (real instance)
5. Create sample weapon/equipment assets
6. Implement R/T/Y/G skill slot activation (real, not placeholder)
7. Create Mana bar + skill visualization UI
8. Run Play Mode validation testing

Each task is sent to Claude with full context of what was done before, so Claude can:
- Reference previous work ("in task 2 we added ManaManager to SaveManager...")
- Build incrementally ("now in task 3, we'll use the E key to check...")
- Test continually
- Commit after each task

## State Persistence

Orchestrator state saved to `orchestrator_state.json`:

```json
{
  "project_root": "...",
  "specs": {
    "SPEC_001": {
      "spec_id": "SPEC_001",
      "status": "completed",
      "completion_percentage": 100,
      "tasks": [
        {"task_id": "SPEC_001_TASK_1", "status": "completed", ...},
        ...
      ],
      ...
    },
    ...
  },
  "execution_order": ["SPEC_001", "SPEC_002", ...],
  "last_updated": "2026-05-24T14:30:00.000000"
}
```

This allows:
- **Resumption**: Restart orchestrator, picks up where it left off
- **Visibility**: See exactly which tasks completed when
- **Debugging**: Track which specs/tasks failed and why
- **Reporting**: Generate progress reports from state

## Validation

After each spec completes, orchestrator runs:

1. **Unity Compilation Check**
   ```powershell
   tools/unity/RunUnityCompileValidation.ps1
   ```
   Verifies no C# compile errors

2. **Documentation Validation**
   ```powershell
   tools/docs/validate_docs.ps1
   ```
   Checks doc integrity and crossreferences

3. **Play Mode Testing** (SPEC_012+)
   - Start Unity in Play Mode
   - Verify feature works as expected
   - Check for regressions
   - Exit Play Mode, commit

## Logging

All activity logged to `orchestrator.log`:
```
[2026-05-24T14:30:00.000000] Starting orchestrated spec execution from beginning
[2026-05-24T14:30:05.000000] 🚀 Executing SPEC_001: Core Event Bus & Event System
[2026-05-24T14:30:08.000000]   [1/4] Define event types and base classes
[2026-05-24T14:30:08.000000]     Calling Claude API...
[2026-05-24T14:31:45.000000]     ✅ Task completed
...
```

## Monitoring

Monitor real-time execution:

```bash
# In one terminal
python run_orchestrator.py --command run

# In another terminal, periodically check status
watch -n 30 "python run_orchestrator.py --command status"
```

## Error Handling

If a task fails:

1. **Task Failure**: Orchestrator marks task as FAILED, logs error
2. **Spec Retry**: Manually fix issue in code, rerun:
   ```bash
   python run_orchestrator.py --command run --start-spec SPEC_XXX
   ```
3. **Compilation Error**: Read `UnityCompileValidation.log`, fix code, retry
4. **API Error**: Check API key in `.env`, rate limits, then retry

## Performance Estimates

- **Sequential execution**: 28-35 days (one spec per day)
- **With parallelization**: 18-22 days (run SPECS 005-007 together, etc.)
- **Actual wall-clock time**: ~2-3 weeks with dedicated runner

Bottleneck: Most specs require human validation (Play Mode testing) that can't fully automate.

## Future Enhancements

- [ ] Parallel execution wrapper for safe-to-parallelize specs
- [ ] Webhook integration for Slack notifications on completion
- [ ] Dashboard/web UI for monitoring orchestration progress
- [ ] Automatic spec decomposition based on file size
- [ ] Git integration to auto-push completed specs
- [ ] Spec-specific validation templates (beyond generic compile check)

## Troubleshooting

### "Spec blocked by X"
Check dependency: does SPEC_X need to complete first? Can't proceed until it's done.

### API rate limit
Reduce `max_tokens` in config.json or add `--max-specs 1` to execute one at a time.

### State file corrupted
Delete `orchestrator_state.json` and run `--command reset` to start fresh.

### Unity validation hanging
Set `validation_timeout_seconds` higher in config.json (default 300s).

## See Also

- `CLAUDE.md` - AI agent instructions
- `AGENTS.md` - Codex/Claude integration guide
- `docs/specs/SPEC_EXECUTION_ORDER.md` - Dependency matrix
- `memory/MEMORY.md` - Reusable patterns and skills

---

**Status**: Fully functional for SPECS 1-17 orchestration. Ready for deployment.
