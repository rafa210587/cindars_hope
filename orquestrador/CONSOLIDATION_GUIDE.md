# Orchestrator Consolidation & File Organization Guide

## Problem Identified

Naming confusion from having two orchestrator systems side-by-side:
- `run_orchestrator.py` (OLD, v1.0) vs `run_orquestrador.py` (NEW, v2.0)
- `spec_orchestrator.py` (OLD state mgmt) vs `spec_operations.py` (NEW file ops)
- `claude_api_integration.py` (OLD API) vs `agent.py` (NEW subprocess CLI)

## Solution: Clear Deprecation Markers

All OLD files now have `[DEPRECATED - LEGACY SYSTEM v1.0]` header comments to make it absolutely clear they are outdated. The new files are unmarked (implicitly current).

## File Organization

### OLD SYSTEM (v1.0 - Deprecated, but still functional)

**API-based: Uses Anthropic Python SDK directly**

```
orquestrador/
├── run_orchestrator.py          [DEPRECATED] Entry point
├── spec_orchestrator.py         [DEPRECATED] State management
├── claude_api_integration.py    [DEPRECATED] API client
├── config.json                  [DEPRECATED] Simple config
├── README.md                    [OLD DOCS]
├── QUICKSTART.md                [OLD DOCS]
├── ARCHITECTURE.md              [OLD DOCS]
├── setup.py                     [OLD SETUP]
└── requirements.txt             [SHARED DEPS]
```

**How it works:**
1. User calls `python .\orquestrador\run_orchestrator.py --command status`
2. Loads `spec_orchestrator.py` (hardcoded 17 specs + state management)
3. Calls Anthropic API via `claude_api_integration.py`
4. Uses `config.json` (simple API settings)

**Status:**
- ✅ Still works
- ⚠️ Deprecated
- 📖 Documentation exists (README.md, QUICKSTART.md, ARCHITECTURE.md)

### NEW SYSTEM (v2.0 - Queue-based, Recommended)

**CLI-based: Invokes Claude/Codex via subprocess**

```
orquestrador/
├── run_orquestrador.py          Entry point (PRIMARY COMMAND)
├── queue.py                     Queue building & ordering
├── agent.py                     Agent invocation (subprocess)
├── validation.py                Validators (docs, compile, repo)
├── logger.py                    Structured logging
├── spec_operations.py           File ops, git integration
├── __init__.py                  Package init
├── orquestrador_config.json     Comprehensive config
├── README_NEW.md                Complete v2.0 documentation
├── CONFIG_GUIDE.md              Config comparison & guide
├── CLEANUP_STATUS.md            Config duplication explained
├── IMPLEMENTATION_SUMMARY.md    Full implementation overview
├── CONSOLIDATION_GUIDE.md       This file
└── test_imports.py              Import verification
```

**How it works:**
1. User calls `python .\orquestrador\run_orquestrador.py --mode spec --input-dir ...`
2. Builds queue from directory using `queue.py`
3. For each item:
   - Streams via `agent.py` (subprocess CLI)
   - Validates via `validation.py`
   - Logs via `logger.py`
   - Operates files via `spec_operations.py`
4. Uses `orquestrador_config.json` (comprehensive per-spec config)

**Status:**
- ✅ Complete and tested
- ✅ Recommended for new work
- 📖 Full documentation (README_NEW.md, CONFIG_GUIDE.md, etc.)

## Directory Structure (Created)

```
docs/agent_prompts/
  a_ejecutar/        ← New prompts to execute
  ejecutados/        ← Completed prompts (auto-moved)
  bloqueados/        ← Failed prompts (auto-moved)

orquestrador/
  logs/              ← Per-run execution logs (auto-created)
```

## No Duplicate Code

The files are NOT duplicates - they're completely different implementations:

| Aspect | OLD (v1.0) | NEW (v2.0) |
|---|---|---|
| **Entry point** | `run_orchestrator.py` | `run_orquestrador.py` |
| **State management** | `spec_orchestrator.py` (hardcoded) | `queue.py` (dynamic directories) |
| **Agent communication** | `claude_api_integration.py` (Anthropic SDK) | `agent.py` (subprocess CLI) |
| **Configuration** | `config.json` (simple) | `orquestrador_config.json` (comprehensive) |
| **Logging** | Single `orchestrator.log` | Per-item structured logs |
| **Validation** | Built-in to code | Separate `validation.py` module |
| **Queue management** | State-based tracking | File-system based (queue.py) |
| **Git integration** | None | Full (`spec_operations.py`) |

## No Regressions

✅ OLD system (v1.0) preserved and functional  
✅ All old files marked with `[DEPRECATED]` headers  
✅ OLD docs preserved (README.md, QUICKSTART.md)  
✅ NEW system fully independent (can run simultaneously)  
✅ Different config files - no overlap  
✅ Different entry points - no confusion  

## Migration Path

### For existing OLD system users:
```bash
# This still works
python .\orquestrador\run_orchestrator.py --command status

# But add this note to your scripts:
# [DEPRECATED] Consider migrating to v2.0:
#   python .\orquestrador\run_orquestrador.py --mode spec --dry-run
```

### For new work (RECOMMENDED):
```bash
# Use new canonical command
python .\orquestrador\run_orquestrador.py --mode spec --input-dir .\docs\specs\a_implementar --dry-run
```

### For gradual migration:
```bash
# Both can run simultaneously
./run_orchestrator.py       # OLD (uses config.json)
./run_orquestrador.py       # NEW (uses orquestrador_config.json)
```

## Cleanup Options

### Option A: Keep Everything (Current state - Recommended)

```bash
# Both systems work in parallel
# Clear deprecation headers guide users to v2.0
# Zero breaking changes
# Smooth migration path
```

**Pros:**
- Backward compatible
- Users can migrate at their own pace
- Easy rollback if needed

**Cons:**
- More files in directory

### Option B: Remove OLD System (When fully migrated)

```bash
# After 100% migration to v2.0, delete:
rm run_orchestrator.py
rm spec_orchestrator.py
rm claude_api_integration.py
rm config.json
rm README.md (old docs)
rm setup.py
```

Result: Clean directory with only v2.0  
Risk: Low (only if 100% migrated)

### Option C: Move OLD System to subdirectory

```bash
# Organize old files:
mkdir legacy
mv run_orchestrator.py legacy/
mv spec_orchestrator.py legacy/
mv claude_api_integration.py legacy/
mv config.json legacy/
mv README.md legacy/
```

Result: Physically separated  
Risk: Low (just moves files)

## Current Recommendation

**Keep Option A (Everything, for now)**

Reason:
1. ✅ Deprecation headers make direction clear
2. ✅ Zero breaking changes
3. ✅ Both systems work independently
4. ✅ Users migrate when ready
5. ✅ Easy cleanup later (just delete marked files)

**No immediate action needed.** The consolidation is conceptual (clear labeling) not structural (file reorganization).

## File Checklist

### OLD SYSTEM (all marked [DEPRECATED])
- ✅ `run_orchestrator.py` - Header updated
- ✅ `spec_orchestrator.py` - Header updated
- ✅ `claude_api_integration.py` - Header updated
- ✅ `config.json` - Still present, noted in CONFIG_GUIDE
- ✅ `README.md`, `QUICKSTART.md`, `ARCHITECTURE.md` - Preserved

### NEW SYSTEM (all marked v2.0, no "deprecated" labels)
- ✅ `run_orquestrador.py` - Clean, current
- ✅ `queue.py` - Clean, current
- ✅ `agent.py` - Clean, current
- ✅ `validation.py` - Clean, current
- ✅ `logger.py` - Clean, current
- ✅ `spec_operations.py` - Clean, current
- ✅ `orquestrador_config.json` - Clean, current

### DOCUMENTATION (Clear & comprehensive)
- ✅ `README_NEW.md` - v2.0 guide
- ✅ `CONFIG_GUIDE.md` - Which config to use
- ✅ `CLEANUP_STATUS.md` - Config explanation
- ✅ `CONSOLIDATION_GUIDE.md` - This file
- ✅ `IMPLEMENTATION_SUMMARY.md` - Overview
- ✅ Deprecation headers in old files

## The "Mess" is Resolved

Before consolidation:
- ❌ Two `run_*` files - confusing which is new
- ❌ Two `spec_*` files - overlapping names
- ❌ Two config systems - which to use?
- ❌ Unclear migration path

After consolidation:
- ✅ Old files clearly marked [DEPRECATED]
- ✅ New files unmarked (implicitly current)
- ✅ Documentation explains both systems
- ✅ CONFIG_GUIDE.md clarifies config choice
- ✅ Clear migration path documented

## Verification

```bash
# Check deprecation markers exist
grep -r "\[DEPRECATED\]" orquestrador/

# Check new system works
python .\orquestrador\test_imports.py

# Check old system still works
python .\orquestrador\run_orchestrator.py --command status

# Check new system dry run
python .\orquestrador\run_orquestrador.py --mode spec --input-dir .\docs\specs\a_implementar --dry-run
```

## Summary

The "mess" was naming confusion from having two systems. Resolution:
1. ✅ Added clear [DEPRECATED] markers to old files
2. ✅ New system unmarked (implicitly current)
3. ✅ Comprehensive documentation for both
4. ✅ Clear migration path
5. ✅ No code duplication (completely different implementations)
6. ✅ No regressions (both systems work)
7. ✅ No breaking changes (backward compatible)

**Result**: Clean, organized, well-documented, ready for use.

---

**Canonical command**: `python .\orquestrador\run_orquestrador.py`  
**Legacy command** (still works): `python .\orquestrador\run_orchestrator.py`  
**Status**: Consolidated and clarified.
