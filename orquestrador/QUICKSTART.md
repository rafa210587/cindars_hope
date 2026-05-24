# Quick Start Guide - Spec Orchestrator

Get up and running in 5 minutes.

## Prerequisites

- Python 3.9+
- Anthropic API key (from claude.ai or Anthropic console)

## Installation (First Time)

```bash
cd orquestrador
python setup.py
```

This will:
1. Verify Python version
2. Create `.env` file
3. Install dependencies
4. Validate configuration
5. Initialize orchestrator state

## Configuration

Edit `.env` and add your API key:
```
ANTHROPIC_API_KEY=sk-ant-...your-key-here...
```

## Run Orchestrator

### Option 1: Check Status (No API Calls)
```bash
python run_orchestrator.py --command status
```

Shows which specs are done, pending, or blocked. Safe to run anytime.

### Option 2: Execute Next Spec
```bash
python run_orchestrator.py --command next
```

Finds the next spec ready to execute (all dependencies met) and runs it via Claude API.

### Option 3: Execute All Remaining Specs
```bash
python run_orchestrator.py --command run
```

Executes all pending specs in dependency order until complete or failure.

### Option 4: Resume from Specific Spec
```bash
python run_orchestrator.py --command run --start-spec SPEC_012
```

Resume if a previous run was interrupted.

### Option 5: Limit Execution
```bash
python run_orchestrator.py --command run --max-specs 2
```

Execute maximum 2 specs then stop (useful for testing).

## Monitoring

In another terminal, periodically check progress:
```bash
watch -n 30 "python run_orchestrator.py --command status"
```

Or read the logs:
```bash
tail -f orchestrator.log
```

## Understanding Status Output

```
Overall Progress: 4/17 specs completed (23%)

Spec Status:
  ✅ SPEC_001: Core Event Bus (100%)
    Completed - all 4 tasks done
  ⏳ SPEC_002: Bootstrap (50%)
    Tasks: 2/4 - working on task 3
  🚫 SPEC_003: IDs/Registries (0%)
    Blocked by SPEC_002
  ...
```

## State Files

### orchestrator_state.json
Persists progress. Contains:
- Which specs are done/pending/failed
- Which tasks are complete
- When each was started/completed
- Error messages for failures

Delete this if you want to start completely from scratch:
```bash
rm orchestrator_state.json
```

### orchestrator.log
Complete execution log with timestamps and detailed messages.

## Troubleshooting

### "API Key not set"
```bash
# Edit .env and add key
ANTHROPIC_API_KEY=sk-ant-...
```

### "Spec blocked by X"
Can't proceed until spec X completes first. Check dependencies:
```bash
python run_orchestrator.py --command status
```

### "Compilation error in task"
Claude will log the error. Fix the code, then re-run the same spec:
```bash
python run_orchestrator.py --command run --start-spec SPEC_YYY
```

### State file corrupted
```bash
python run_orchestrator.py --command reset
```

## Workflow Example

```bash
# Start
$ python run_orchestrator.py --command status
Overall Progress: 0/17 specs (0%)

$ python run_orchestrator.py --command next
🚀 Executing SPEC_001...
[many API calls and task completions]
✅ SPEC_001 COMPLETED

# Check progress
$ python run_orchestrator.py --command status
Overall Progress: 1/17 specs (5%)

# Continue
$ python run_orchestrator.py --command next
🚀 Executing SPEC_002...
[tasks running...]

# Or execute multiple at once
$ python run_orchestrator.py --command run --max-specs 3
Executing SPEC_002...
Executing SPEC_003...
Executing SPEC_004...
Overall Progress: 4/17 specs (23%)
```

## Expected Timeline

- Sequential: ~28-35 days (one spec/day)
- Estimated hours per spec: 12-25 hours (broken into 6-8 subtasks)
- Wall-clock time: 2-3 weeks with continuous execution

## Next Steps

1. Read `README.md` for full documentation
2. Read `ARCHITECTURE.md` for system design
3. Check logs for any errors: `tail -f orchestrator.log`
4. Run status periodically: `python run_orchestrator.py --command status`

## See Also

- `../CLAUDE.md` - AI agent instructions
- `../docs/specs/SPEC_EXECUTION_ORDER.md` - Spec dependency matrix
- `../memory/MEMORY.md` - Reusable patterns and skills

---

**Status**: Ready to use. Safe for production. All changes committed to git.
