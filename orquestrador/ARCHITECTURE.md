# Spec Orchestrator Architecture

## Problem Statement

The Cindar's Hope project requires implementation of 17 interdependent specifications. Each specification is too large (40-200 hours) for a single Claude session due to:

1. **Context Window Limits**: Session context filled before completion
2. **Scope Creep**: Underestimating subtasks leads to incomplete implementations
3. **Integration Complexity**: Multiple systems depend on each other; decisions made in SPEC_003 affect SPEC_012
4. **Validation Overhead**: Testing and validation happen at session end, not during implementation
5. **State Loss**: Interruptions require manual recovery of where work was

## Solution: Spec Orchestrator

A Python-based orchestration system that:

- **Breaks SPECS into micro-tasks** (1.5-2.5 hour subtasks, completable in single session)
- **Tracks state persistently** (JSON-based, survives interruptions)
- **Understands dependencies** (topological sort ensures correct execution order)
- **Validates after each spec** (Unity compile checks prevent regressions)
- **Calls Claude API** for each subtask with complete context
- **Commits work incrementally** (smaller, focused commits are easier to review)

## System Components

### 1. spec_orchestrator.py - Core State Management

**Responsibilities:**
- Define all 17 specs with dependencies and task breakdowns
- Track completion state (PENDING → IN_PROGRESS → COMPLETED/FAILED)
- Calculate execution order (topological sort)
- Persist state to JSON
- Answer "Can spec X execute now?"

**Key Classes:**
```
SpecOrchestrator
├── _define_specs()           # Define SPECS 001-017
├── _calculate_execution_order()  # Topological sort
├── can_execute(spec_id)      # Dependency check
├── start_spec(spec_id)       # Mark as in_progress
├── complete_spec(spec_id)    # Mark as done
└── fail_spec(spec_id, error)  # Mark as failed
```

**Data Model:**
```python
Spec:
  - spec_id: str              # "SPEC_012"
  - name: str                 # "Player Combat..."
  - status: TaskStatus        # PENDING|IN_PROGRESS|COMPLETED|FAILED
  - blocks: List[str]         # ["SPEC_013", "SPEC_014"]
  - blocked_by: List[str]     # ["SPEC_004", "SPEC_006"]
  - tasks: List[SpecTask]     # Granular subtasks
  - completion_percentage: int # 0-100
  - started_at: ISO timestamp
  - completed_at: ISO timestamp

SpecTask:
  - task_id: str              # "SPEC_012_TASK_3"
  - name: str                 # "Create ArcaneBolt spell asset"
  - estimated_hours: float    # 2.0
  - status: TaskStatus
```

**Execution Flow:**
```
┌─────────────────────────────────────┐
│ Load orchestrator_state.json         │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│ Define SPECS 001-017 (if not loaded)│
├─────────────────────────────────────┤
│ _define_specs():                    │
│  For each spec:                     │
│   - Create Spec object              │
│   - Add tasks via _add_spec_tasks()│
│   - Set dependencies               │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│ Calculate execution order           │
├─────────────────────────────────────┤
│ _calculate_execution_order():       │
│  Topological sort based on:         │
│   - spec.blocks                     │
│   - spec.blocked_by                 │
│  Result: execution_order list       │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│ Save state to orchestrator_state.json│
└─────────────────────────────────────┘
```

### 2. claude_api_integration.py - Claude Communication

**Responsibilities:**
- Build detailed task prompts with full context
- Call Claude API (Anthropic SDK)
- Parse responses for success/failure
- Generate spec summaries

**Key Classes:**
```
ClaudeSpecExecutor:
  - build_execution_prompt(prompt)     # Create detailed task prompt
  - execute_spec_task(prompt)          # Call Claude API
  - generate_spec_summary(task_results) # Summarize completion
```

**Prompt Structure:**
```
# SPEC EXECUTION REQUEST - SPEC_012

## Overview
You are Claude, executing a subtask in orchestrated implementation...

## Spec Details
- Spec ID: SPEC_012
- Task: "Wire ManaManager to GameBootstrap"
- Description: Specific implementation details
- Spec Document Location: docs/specs/a_implementar/...

## Context
- Project Root: /path/to/cindars_hope
- Blockers Resolved: [SPEC_004, SPEC_006, SPEC_007]

## Critical Instructions
1. **Scope**: Execute ONLY this task, not the whole spec
2. **Implementation**: Follow CLAUDE.md patterns
3. **Validation**: Run tools/unity/RunUnityCompileValidation.ps1
4. **Output**: Implement → Validate → Commit

## Success Criteria
- Code compiles without errors
- Validation passes
- Changes committed with Portuguese message
```

**API Call Flow:**
```
execute_spec_task(SpecExecutionPrompt)
    ↓
build_execution_prompt() → detailed prompt string
    ↓
client.messages.create(
    model="claude-opus-4-7",
    max_tokens=4096,
    messages=[{"role": "user", "content": prompt}]
)
    ↓
Parse response → (success: bool, response_text: str)
    ↓
Return to orchestrator
```

### 3. run_orchestrator.py - Orchestration Loop

**Responsibilities:**
- Main workflow controller
- Task-by-task execution
- State updates between tasks
- Logging and error handling

**Workflow:**
```
def run_spec_sequence():
  FOR each spec_id in execution_order:
    1. Check can_execute() - all dependencies met?
       NO? → SKIP (blocked)
       YES? → continue
    
    2. orchestrator.start_spec(spec_id)
    
    3. FOR each task in spec.tasks:
       a. Build SpecExecutionPrompt
       b. api_executor.execute_spec_task()
       c. Parse success/failure
       d. orchestrator.complete_task() or fail
       e. Log result
    
    4. Check if all tasks complete
       YES? → orchestrator.complete_spec()
       NO? → orchestrator.fail_spec() + break
    
    5. Log status report
    
    6. Next spec iteration
```

### 4. config.json - Configuration

**Structure:**
```json
{
  "project_name": "Cindar's Hope",
  "api_config": {
    "model": "claude-opus-4-7",
    "temperature": 0.7,
    "max_tokens": 4096
  },
  "validation": {
    "unity_compile_check": true,
    "validation_timeout_seconds": 300
  },
  "parallelization": {
    "specs_parallel_safe": [
      ["SPEC_005", "SPEC_006", "SPEC_007"],
      ["SPEC_013", "SPEC_014"]
    ]
  },
  "output": {
    "log_file": "orchestrator.log",
    "state_file": "orchestrator_state.json"
  }
}
```

## Data Flow

### Session 1: SPEC_001-003

```
User: python run_orchestrator.py --command run

orchestrator.state = load("orchestrator_state.json")  [not exists, so empty]
orchestrator._define_specs()  [creates all 17]
orchestrator._calculate_execution_order()
→ execution_order = [SPEC_001, SPEC_002, SPEC_003, ...]

FOR spec_id in [SPEC_001, SPEC_002, SPEC_003]:
  api_executor.execute_spec_task(prompt for task 1)
    → Claude implements task 1, validates, commits
    → Returns success=True
  orchestrator.complete_task(SPEC_XXX, TASK_1)
  
  api_executor.execute_spec_task(prompt for task 2)
    → Claude implements task 2, validates, commits
    → Returns success=True
  orchestrator.complete_task(SPEC_XXX, TASK_2)
  
  ... (all tasks)
  
  orchestrator.complete_spec(SPEC_XXX)

orchestrator._save_state()
→ Writes orchestrator_state.json with:
  {
    "specs": {
      "SPEC_001": {"status": "completed", "completion_percentage": 100, ...},
      "SPEC_002": {"status": "completed", "completion_percentage": 100, ...},
      "SPEC_003": {"status": "completed", "completion_percentage": 100, ...},
      "SPEC_004": {"status": "pending", ...}  [blocked by none]
      "SPEC_005": {"status": "pending", ...}  [blocked by SPEC_002, SPEC_003]
      ...
    }
  }
```

### Interruption: Session Resumes

```
User: python run_orchestrator.py --command next

orchestrator.state = load("orchestrator_state.json")  [FOUND!]
→ Loads SPECS 001-003 as completed, SPEC_004 as next
→ No need to re-execute 001-003

next_spec = orchestrator.get_next_executable()
→ SPEC_004  [no blockers]

api_executor.execute_spec_task(SPEC_004_TASK_1)
  → Claude implements (no context loss, task prompt is self-contained)
  → Validates, commits

orchestrator.complete_task(SPEC_004, TASK_1)
orchestrator._save_state()
→ Updates state with SPEC_004 progress
```

## Dependency Resolution

### Topological Sort Algorithm

```python
def _calculate_execution_order():
  completed = set()
  order = []
  
  while len(completed) < len(specs):
    for spec_id, spec in specs.items():
      if spec_id in completed:
        continue
      
      # Can this spec execute?
      blockers_met = all(blocker in completed for blocker in spec.blocked_by)
      if blockers_met:
        order.append(spec_id)
        completed.add(spec_id)

  return order
```

### Example: SPEC_012 Dependencies

```
SPEC_012 requires (blocked_by):
  - SPEC_004 (Damage calculator)
  - SPEC_006 (Equipment system)
  - SPEC_007 (Stamina system)
  - SPEC_009 (GameTime manager)
  - SPEC_010 (SaveManager)
  - SPEC_011 (UI framework)

SPEC_012 blocks:
  - SPEC_015 (Cave generation needs combat testing)

Topological sort ensures:
  Execute SPEC_001
  Execute SPEC_002
  Execute SPEC_003
  Execute SPEC_004  ✓ (no blockers)
  Execute SPEC_005  ✓ (SPEC_002, SPEC_003 done)
  Execute SPEC_006  ✓ (SPEC_003, SPEC_005 done)
  Execute SPEC_007  ✓ (SPEC_003 done)
  Execute SPEC_008  ✓ (SPEC_004 done)
  Execute SPEC_009  ✓ (SPEC_001 done)
  Execute SPEC_010  ✓ (SPEC_002 done)
  Execute SPEC_011  ✓ (SPEC_010 done)
  Execute SPEC_012  ✓ (all blockers met!)
  Skip SPEC_013   ✗ (blocked by SPEC_012)
  ...
```

## Task Decomposition Strategy

### Principle: 1.5-2.5 hours per task

Too small: Task doesn't accomplish anything meaningful
Too large: Single Claude session runs out of context

### SPEC_012 Example: 8 tasks, ~16 hours total

1. **Wire ManaManager to GameBootstrap** (2h)
   - Add field to GameBootstrap
   - Call Initialize in GameBootstrap.Initialize()
   - Call Shutdown in GameBootstrap.Shutdown()
   - Wire to SaveManager

2. **Integrate ManaManager into SaveManager** (1.5h)
   - Add mana to SaveData schema
   - Add CaptureManaManagerSaveData()
   - Add ApplyManaManagerSaveData()
   - Test save/load cycle

3. **Implement E key IInteractable logic** (2h)
   - Check for IInteractable in front of player
   - If found: interact (use/pickup)
   - If not found: use RightHand attack
   - Validate with test scenario

4. **Create ArcaneBolt spell asset** (1.5h)
   - Create SpellDataSO instance
   - Set damage, range, mana cost, cooldown
   - Create visual effect prefab
   - Test casting in Play Mode

5. **Create sample weapon assets** (1.5h)
   - Create sword WeaponDataSO
   - Create bow WeaponDataSO
   - Set stats for balance
   - Place in Resources/Weapons

6. **Implement skill slot activation** (2.5h)
   - R/T/Y/G → skill_manager.ActivateSlot(0-3)
   - Load active skill from EquipmentManager
   - Execute skill (damage + effects)
   - Validate with test scenario

7. **Create Mana bar + skill UI** (2h)
   - Create Canvas with Mana bar
   - Subscribe to ManaChangedEvent
   - Update bar visualization
   - Create skill slot icons

8. **Run Play Mode validation** (1.5h)
   - Start game in Play Mode
   - Test all features (attacks, dodge, spells, skills)
   - Check for regressions
   - Verify balance values
   - Commit final state

### Why This Works

- Each task has **clear acceptance criteria** (UI renders, API called, test passes)
- Each task can **reference previous work** ("In task 2 we wired ManaManager...")
- **No context overflow**: Each prompt fits in ~4k tokens
- **State persists**: If task fails, operator re-runs with same prompt
- **Atomic commits**: Each task = one commit, easier to review/revert

## Validation Strategy

### After Each Task

**Minimal validation** (10-30 seconds):
- C# compile check: `tsc` or Unity CLI
- If fails: Stop, log error, mark task FAILED

### After Each Spec

**Full validation** (2-5 minutes):
1. Unity compilation: `tools/unity/RunUnityCompileValidation.ps1`
2. Doc validation: `tools/docs/validate_docs.ps1`
3. Play Mode test: Start Unity, quick feature test, exit
4. If all pass: Commit and move to next spec

### Rationale

- **Catches regressions early** (don't discover SPEC_010 broke in SPEC_015)
- **Prevents cascading failures** (dead code doesn't propagate)
- **Maintains confidence** (status report reflects true state)

## Scalability & Future Work

### Current Implementation

- Sequential execution: 28-35 days
- Simple JSON state file
- Single API client (Anthropic)
- No UI dashboard

### Future: Parallel Execution

```
config.json specifies safe-to-parallelize specs:
"specs_parallel_safe": [
  ["SPEC_005", "SPEC_006", "SPEC_007"],  # All blocked by same deps
  ["SPEC_013", "SPEC_014"]               # Independent of 015-016
]

run_orchestrator --parallel would:
- Create ThreadPoolExecutor
- Fire independent specs simultaneously
- Merge results to state
- Result: ~25% time savings (18-22 days)
```

### Future: Web Dashboard

```
orchestrator_server.py:
  - Flask/FastAPI endpoint for status
  - WebSocket for real-time updates
  - Graph visualization of dependencies
  - Log viewer
  - Task history

Access at http://localhost:8000/orchestrator
```

## Security Considerations

### API Keys
- Never commit `.env` with ANTHROPIC_API_KEY
- Use `.env.example` template
- Add `.env` to `.gitignore`

### Prompt Injection
- Task prompts are generated by system (trusted)
- Claude's response is parsed, not executed directly
- Changes committed to git, reviewed manually

### State Integrity
- orchestrator_state.json is write-once per update
- Corrupted state can be reset: `--command reset`
- Backup before major operations

## Monitoring & Observability

### Status Report

```bash
python run_orchestrator.py --command status

SPEC ORCHESTRATOR STATUS REPORT
Generated: 2026-05-24T14:30:00.000000

Overall Progress: 4/17 specs completed (23%)

Execution Order: SPEC_001 → SPEC_002 → ... → SPEC_017

Spec Status:
  ✅ SPEC_001: Core Event Bus (100%)
  ✅ SPEC_002: Bootstrap (100%)
  ✅ SPEC_003: IDs/Registries (100%)
  ✅ SPEC_004: Damage Formula (100%)
  ⏳ SPEC_005: Farm Movement (0%)
  ⏳ SPEC_006: Equipment (0%)
  ...
```

### Logs

All actions logged to `orchestrator.log` with timestamps:
```
[2026-05-24T14:30:00] Starting orchestrated spec execution
[2026-05-24T14:30:05] 🚀 Executing SPEC_001: Core Event Bus
[2026-05-24T14:30:08]   [1/4] Define event types and base classes
[2026-05-24T14:30:08]     Calling Claude API...
[2026-05-24T14:31:45]     ✅ Task completed
[2026-05-24T14:31:46]   [2/4] Implement GameEventBus
...
```

---

**Version**: 1.0  
**Last Updated**: 2026-05-24  
**Status**: Production-Ready
