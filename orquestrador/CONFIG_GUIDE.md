# Orchestrator Configuration Guide

## Two Config Files, One Clear Purpose

This directory contains TWO configuration files. They coexist without conflict because they serve different orchestrator implementations:

### 1. `config.json` (Legacy - Backward Compatible)

**Used by**: `run_orchestrator.py` (legacy orchestrator, API-based)  
**Type**: Simple, API-focused configuration  
**Status**: Preserved for backward compatibility  
**Do not use with**: `run_orquestrador.py`

```json
{
  "project_name": "Cindar's Hope",
  "api_config": { ... },
  "validation": { ... },
  "output": { ... }
}
```

### 2. `orquestrador_config.json` (NEW - Queue-Based)

**Used by**: `run_orquestrador.py` (new orchestrator, queue-based)  
**Type**: Comprehensive, per-spec section 6 of SPEC_ORQUESTRADOR_MODO_SPEC_PROMPT_QUEUE_v1  
**Status**: Production - use this for all new work  
**Do use with**: `run_orquestrador.py` only

```json
{
  "repo_root": ".",
  "spec_input_dir": "docs/specs/a_implementar",
  "prompt_input_dir": "docs/agent_prompts/a_executar",
  "primary_agent": "claude",
  "fallback_agent": "codex",
  "credit_error_patterns": [ ... ],
  ...
}
```

## Which One to Use?

### For spec/prompt queue-based orchestration (NEW RECOMMENDED)

```bash
python run_orquestrador.py --mode spec --input-dir docs/specs/a_implementar --dry-run
```

This uses `orquestrador_config.json` (loaded automatically).

### For legacy API-based orchestration (DEPRECATED)

```bash
python run_orchestrator.py --command status
```

This uses `config.json` (loaded automatically).

## Migration Path

1. **Old workflow** → `run_orchestrator.py` + `config.json`
2. **New workflow** → `run_orquestrador.py` + `orquestrador_config.json`
3. **Transition**: Both can run simultaneously without issues

## Configuration Customization

### To customize the NEW config (recommended):

Edit `orquestrador_config.json`:
- Update `unity_editor_path` to match your Unity installation
- Adjust `max_repair_attempts` if needed
- Customize `credit_error_patterns` for your agent API
- Set `primary_agent` and `fallback_agent`

### To customize the OLD config (if still using legacy):

Edit `config.json`:
- Update `api_config.model` if needed
- Adjust `validation.validation_timeout_seconds`

## Canonical Entry Points

```
Canonical NEW command:  python .\orquestrador\run_orquestrador.py
Legacy OLD command:    python .\orquestrador\run_orchestrator.py
```

## Cleanup

Both config files are intentionally kept separate:
- ✅ `config.json` - Can be safely deleted if you never use `run_orchestrator.py` again
- ✅ `orquestrador_config.json` - Required for `run_orquestrador.py`

If you want to remove the legacy system:

```bash
rm orquestrador/config.json
rm orquestrador/run_orchestrator.py
rm orquestrador/spec_orchestrator.py
rm orquestrador/claude_api_integration.py
```

But keeping them doesn't hurt and provides a smooth migration path.
