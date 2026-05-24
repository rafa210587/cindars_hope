"""
[DEPRECATED - LEGACY SYSTEM v1.0]

Spec Orchestrator - Automates SPECS 1-17 execution for Cindar's Hope project.
Handles dependency tracking, state persistence, validation, and Claude API integration.

⚠️  THIS IS THE OLD SYSTEM. Used by run_orchestrator.py only.
This system uses Anthropic API directly (not CLI subprocess).

For new work, use the queue-based system (v2.0):
- run_orquestrador.py (entry point)
- queue.py, agent.py, validation.py, logger.py, spec_operations.py (modules)

See CONFIG_GUIDE.md for system comparison.
"""

import json
import subprocess
import sys
from datetime import datetime
from enum import Enum
from pathlib import Path
from typing import Optional, List, Dict, Any

from pydantic import BaseModel, Field


class TaskStatus(str, Enum):
    """Possible states for a task"""
    PENDING = "pending"
    IN_PROGRESS = "in_progress"
    COMPLETED = "completed"
    FAILED = "failed"
    BLOCKED = "blocked"


class SpecTask(BaseModel):
    """Individual subtask within a spec"""
    task_id: str
    name: str
    description: str
    estimated_hours: float = 1.5
    status: TaskStatus = TaskStatus.PENDING
    completed_at: Optional[str] = None
    error_message: Optional[str] = None


class Spec(BaseModel):
    """A specification that needs implementation"""
    spec_id: str
    name: str
    description: str
    file_path: str
    blocks: List[str] = Field(default_factory=list)
    blocked_by: List[str] = Field(default_factory=list)
    status: TaskStatus = TaskStatus.PENDING
    completion_percentage: int = 0
    tasks: List[SpecTask] = Field(default_factory=list)
    started_at: Optional[str] = None
    completed_at: Optional[str] = None


class OrchestratorState(BaseModel):
    """Complete state of the orchestration process"""
    project_root: str
    specs: Dict[str, Spec] = Field(default_factory=dict)
    execution_order: List[str] = Field(default_factory=list)
    last_updated: str = Field(default_factory=lambda: datetime.now().isoformat())
    total_estimated_hours: float = 0.0
    total_completed_hours: float = 0.0
    start_time: Optional[str] = None
    end_time: Optional[str] = None


class SpecOrchestrator:
    """
    Main orchestrator that manages spec execution with dependency tracking,
    state persistence, and validation
    """

    def __init__(self, project_root: str, state_file: str = "orchestrator_state.json"):
        self.project_root = Path(project_root)
        self.state_file = self.project_root / state_file
        self.state = self._load_state() or self._initialize_state()
        self._define_specs()

    def _load_state(self) -> Optional[OrchestratorState]:
        """Load orchestrator state from JSON file"""
        if not self.state_file.exists():
            return None
        try:
            with open(self.state_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
                return OrchestratorState(**data)
        except Exception as e:
            print(f"Error loading state: {e}")
            return None

    def _save_state(self):
        """Persist orchestrator state to JSON file"""
        self.state.last_updated = datetime.now().isoformat()
        with open(self.state_file, 'w', encoding='utf-8') as f:
            json.dump(self.state.model_dump(), f, indent=2)

    def _initialize_state(self) -> OrchestratorState:
        """Create fresh orchestrator state"""
        return OrchestratorState(project_root=str(self.project_root))

    def _define_specs(self):
        """Define all 17 specs with dependencies and task breakdowns"""
        specs_definition = {
            "SPEC_001": {
                "name": "Core Event Bus & Event System",
                "description": "Event-driven architecture using GameEventBus",
                "file_path": "docs/specs/implementados/spec_core_001_event_bus_e_eventos_base.md",
                "blocks": ["SPEC_002", "SPEC_003", "SPEC_004", "SPEC_009"],
            },
            "SPEC_002": {
                "name": "Bootstrap & Manager Initialization",
                "description": "GameBootstrap, core manager wiring, lifecycle",
                "file_path": "docs/specs/implementados/spec_core_002_bootstrap_managers_e_runtime_references.md",
                "blocks": ["SPEC_005", "SPEC_006", "SPEC_010"],
                "blocked_by": ["SPEC_001"],
            },
            "SPEC_003": {
                "name": "IDs, Registries & ScriptableObjects",
                "description": "Data-driven content via scriptable objects and registries",
                "file_path": "docs/specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md",
                "blocks": ["SPEC_005", "SPEC_006", "SPEC_007"],
                "blocked_by": ["SPEC_001"],
            },
            "SPEC_004": {
                "name": "Damage Formula MVP",
                "description": "Unified damage calculation pipeline (DamageCalculator, DamageRequest)",
                "file_path": "docs/specs/implementados/spec_damage_001_damage_formula_mvp.md",
                "blocks": ["SPEC_012", "SPEC_013"],
                "blocked_by": ["SPEC_001"],
            },
            "SPEC_005": {
                "name": "Farm Scene & Movement",
                "description": "Basic farm scene, player movement, tile-based interaction",
                "file_path": "docs/specs/implementados/spec_farm_001_farm_scene_movimento_interacao.md",
                "blocks": ["SPEC_006"],
                "blocked_by": ["SPEC_002", "SPEC_003"],
            },
            "SPEC_006": {
                "name": "Equipment & Durability",
                "description": "Equipment system, slots, durability tracking, stat modifiers",
                "file_path": "docs/specs/a_implementar/spec_equipment_durability_environment_loot_runtime.md",
                "blocks": ["SPEC_012", "SPEC_013"],
                "blocked_by": ["SPEC_003", "SPEC_005"],
            },
            "SPEC_007": {
                "name": "Hunger & Stamina",
                "description": "Stamina regeneration, hunger system, balance parameters",
                "file_path": "docs/specs/a_implementar/spec_hunger_stamina_status_balance.md",
                "blocks": ["SPEC_012"],
                "blocked_by": ["SPEC_003"],
            },
            "SPEC_008": {
                "name": "Status Effects & Resistances",
                "description": "Poison, burn, freeze, resistance modifiers, status tick system",
                "file_path": "docs/specs/a_implementar/spec_damage_status_elements_resistances_runtime.md",
                "blocks": ["SPEC_013", "SPEC_014"],
                "blocked_by": ["SPEC_004"],
            },
            "SPEC_009": {
                "name": "GameTime Manager (Pause-Aware)",
                "description": "Real-time tracking with pause awareness (10min day/5min night)",
                "file_path": "docs/specs/a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md",
                "blocks": ["SPEC_012"],
                "blocked_by": ["SPEC_001"],
            },
            "SPEC_010": {
                "name": "SaveManager & Data Persistence",
                "description": "Save/load system using SaveData v3 schema (IDs+primitives)",
                "file_path": "docs/specs/a_implementar/spec_docs_single_source_specs_refinements_reconciliation_v1.md",
                "blocks": ["SPEC_012", "SPEC_013"],
                "blocked_by": ["SPEC_002"],
            },
            "SPEC_011": {
                "name": "UI Framework & HUD",
                "description": "Basic HUD framework, mana/stamina bars, inventory UI foundation",
                "file_path": "docs/specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md",
                "blocks": ["SPEC_012", "SPEC_013"],
                "blocked_by": ["SPEC_010"],
            },
            "SPEC_012": {
                "name": "Player Combat - Weapons, Spells, Skills",
                "description": "Q=LeftHand, E=IInteractable|RightHand, Space=Dodge, R/T/Y/G=Skills, ManaManager, ArcaneBolt",
                "file_path": "docs/specs/a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md",
                "blocks": ["SPEC_015"],
                "blocked_by": ["SPEC_004", "SPEC_006", "SPEC_007", "SPEC_009", "SPEC_010", "SPEC_011"],
            },
            "SPEC_013": {
                "name": "Enemy AI & Combat Roles",
                "description": "Enemy stat scaling, combat behavior trees, faction system, drops",
                "file_path": "docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md",
                "blocks": ["SPEC_014"],
                "blocked_by": ["SPEC_004", "SPEC_006", "SPEC_008", "SPEC_010"],
            },
            "SPEC_014": {
                "name": "Skill Trees & Active Slots",
                "description": "Unlock nodes, respec, active/passive differentiation, stat scaling",
                "file_path": "docs/specs/a_implementar/spec_skill_trees_active_slots_respec_anya_runtime.md",
                "blocks": ["SPEC_016"],
                "blocked_by": ["SPEC_008", "SPEC_013"],
            },
            "SPEC_015": {
                "name": "Cave Generation & Checkpoints",
                "description": "Procedural generation, snapshot-based replay, stable run handoff",
                "file_path": "docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md",
                "blocks": ["SPEC_016"],
                "blocked_by": ["SPEC_012"],
            },
            "SPEC_016": {
                "name": "Loot, Crafting & Economy",
                "description": "Drops, crafting queue, shop pricing, resource loops",
                "file_path": "docs/specs/a_implementar/spec_crafting_queue_workstations_recipes_ui.md",
                "blocks": ["SPEC_017"],
                "blocked_by": ["SPEC_014", "SPEC_015"],
            },
            "SPEC_017": {
                "name": "End-to-End Integration & Polish",
                "description": "Full feature integration, balance tuning, visual polish, final validation",
                "file_path": "docs/specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md",
                "blocked_by": ["SPEC_016"],
            },
        }

        for spec_id, spec_data in specs_definition.items():
            if spec_id not in self.state.specs:
                spec = Spec(
                    spec_id=spec_id,
                    name=spec_data["name"],
                    description=spec_data["description"],
                    file_path=spec_data["file_path"],
                    blocks=spec_data.get("blocks", []),
                    blocked_by=spec_data.get("blocked_by", []),
                )
                self._add_spec_tasks(spec)
                self.state.specs[spec_id] = spec

        self._calculate_execution_order()

    def _add_spec_tasks(self, spec: Spec):
        """Break down spec into granular subtasks"""
        # Task breakdown varies by spec
        task_templates = {
            "SPEC_001": ["Define event types and base classes", "Implement GameEventBus", "Add subscriber registry", "Validate event system"],
            "SPEC_002": ["Create GameBootstrap with Initialize/Shutdown", "Wire core managers", "Test lifecycle", "Validate initialization"],
            "SPEC_003": ["Define IIdentifiedData interface", "Create registries", "Setup SO templates", "Validate data loading"],
            "SPEC_004": ["Define DamageRequest/DamageResult", "Implement DamageCalculator", "Apply resistances/vulnerabilities", "Validate pipeline"],
            "SPEC_005": ["Create farm scene structure", "Implement player movement", "Add interaction system", "Test gameplay"],
            "SPEC_006": ["Define equipment slots", "Implement EquipmentManager", "Add durability tracking", "Test equipment swapping"],
            "SPEC_007": ["Implement StaminaManager", "Add stamina regen", "Balance consumption", "Test integration"],
            "SPEC_008": ["Define status effect types", "Implement StatusEffectManager", "Apply resistance modifiers", "Test status tick"],
            "SPEC_009": ["Create GameTimeManager", "Add pause awareness", "Implement tick event", "Validate time tracking"],
            "SPEC_010": ["Define SaveData v3 schema", "Implement SaveManager", "Add serialization", "Test save/load"],
            "SPEC_011": ["Create UI framework", "Implement HUD elements", "Add bar visualizations", "Test UI integration"],
            "SPEC_012": [
                "Wire ManaManager to GameBootstrap",
                "Integrate ManaManager into SaveManager",
                "Implement E key IInteractable logic",
                "Create ArcaneBolt spell asset",
                "Create sample weapons and equipment",
                "Implement R/T/Y/G skill slot activation",
                "Create Mana bar and skill UI",
                "Run Play Mode validation"
            ],
            "SPEC_013": ["Define enemy stat scaling", "Implement AI behavior trees", "Add faction system", "Test combat encounters"],
            "SPEC_014": ["Define skill tree nodes", "Implement unlock system", "Add respec logic", "Test progression"],
            "SPEC_015": ["Implement procedural generation", "Add snapshot replay", "Implement stable run", "Test cave generation"],
            "SPEC_016": ["Implement loot drops", "Add crafting queue", "Implement shop system", "Test economy loops"],
            "SPEC_017": ["Integrate all features", "Balance gameplay", "Polish visuals", "Final validation"],
        }

        for i, task_name in enumerate(task_templates.get(spec.spec_id, ["Implement spec functionality", "Validate and test"]), 1):
            task = SpecTask(
                task_id=f"{spec.spec_id}_TASK_{i}",
                name=task_name,
                description=f"{task_name} for {spec.name}",
                estimated_hours=2.0 if spec.spec_id == "SPEC_012" else 1.5,
            )
            spec.tasks.append(task)
            spec.completion_percentage = 0

    def _calculate_execution_order(self):
        """Topological sort to determine execution order respecting dependencies"""
        completed = set()
        order = []

        while len(completed) < len(self.state.specs):
            progress = False
            for spec_id, spec in self.state.specs.items():
                if spec_id in completed:
                    continue
                # Check if all blockers are complete
                if all(blocker in completed for blocker in spec.blocked_by):
                    order.append(spec_id)
                    completed.add(spec_id)
                    progress = True

            if not progress:
                break

        self.state.execution_order = order

    def can_execute(self, spec_id: str) -> tuple[bool, str]:
        """Check if a spec can be executed (all dependencies met)"""
        spec = self.state.specs.get(spec_id)
        if not spec:
            return False, f"Spec {spec_id} not found"

        for blocker_id in spec.blocked_by:
            blocker_spec = self.state.specs.get(blocker_id)
            if not blocker_spec or blocker_spec.status != TaskStatus.COMPLETED:
                return False, f"Blocked by {blocker_id}"

        return True, "Ready to execute"

    def get_next_executable(self) -> Optional[str]:
        """Get the next spec that's ready to execute"""
        for spec_id in self.state.execution_order:
            spec = self.state.specs[spec_id]
            if spec.status == TaskStatus.PENDING:
                can_exec, _ = self.can_execute(spec_id)
                if can_exec:
                    return spec_id
        return None

    def start_spec(self, spec_id: str) -> bool:
        """Mark a spec as in progress"""
        spec = self.state.specs.get(spec_id)
        if not spec:
            return False
        spec.status = TaskStatus.IN_PROGRESS
        spec.started_at = datetime.now().isoformat()
        self._save_state()
        return True

    def complete_task(self, spec_id: str, task_id: str):
        """Mark a specific task as complete"""
        spec = self.state.specs.get(spec_id)
        if not spec:
            return

        for task in spec.tasks:
            if task.task_id == task_id:
                task.status = TaskStatus.COMPLETED
                task.completed_at = datetime.now().isoformat()
                break

        completed_tasks = sum(1 for t in spec.tasks if t.status == TaskStatus.COMPLETED)
        spec.completion_percentage = int((completed_tasks / len(spec.tasks)) * 100) if spec.tasks else 0
        self._save_state()

    def complete_spec(self, spec_id: str):
        """Mark an entire spec as complete"""
        spec = self.state.specs.get(spec_id)
        if not spec:
            return

        spec.status = TaskStatus.COMPLETED
        spec.completion_percentage = 100
        spec.completed_at = datetime.now().isoformat()

        # Mark all tasks as complete
        for task in spec.tasks:
            if task.status != TaskStatus.COMPLETED:
                task.status = TaskStatus.COMPLETED
                task.completed_at = datetime.now().isoformat()

        self._save_state()

    def fail_spec(self, spec_id: str, error_message: str):
        """Mark a spec as failed"""
        spec = self.state.specs.get(spec_id)
        if not spec:
            return
        spec.status = TaskStatus.FAILED
        spec.error_message = error_message
        self._save_state()

    def get_status_report(self) -> str:
        """Generate a status report"""
        completed = sum(1 for s in self.state.specs.values() if s.status == TaskStatus.COMPLETED)
        total = len(self.state.specs)

        report = [
            "=" * 70,
            f"SPEC ORCHESTRATOR STATUS REPORT",
            f"Generated: {datetime.now().isoformat()}",
            "=" * 70,
            f"\nOverall Progress: {completed}/{total} specs completed ({int(completed/total*100)}%)",
            f"\nExecution Order: {' → '.join(self.state.execution_order)}",
            "\nSpec Status:",
        ]

        for spec_id in self.state.execution_order:
            spec = self.state.specs[spec_id]
            status_icon = {
                TaskStatus.PENDING: "⏳",
                TaskStatus.IN_PROGRESS: "🔄",
                TaskStatus.COMPLETED: "✅",
                TaskStatus.FAILED: "❌",
                TaskStatus.BLOCKED: "🚫",
            }.get(spec.status, "❓")

            report.append(f"  {status_icon} {spec.spec_id}: {spec.name} ({spec.completion_percentage}%)")

            if spec.tasks:
                completed_tasks = sum(1 for t in spec.tasks if t.status == TaskStatus.COMPLETED)
                report.append(f"      Tasks: {completed_tasks}/{len(spec.tasks)}")

        report.append("\n" + "=" * 70)
        return "\n".join(report)

    def validate_unity_compilation(self) -> bool:
        """Validate Unity compilation using tools"""
        try:
            # Try to run the validation script
            result = subprocess.run(
                ["powershell", "-ExecutionPolicy", "Bypass", "-File",
                 str(self.project_root / "tools" / "unity" / "RunUnityCompileValidation.ps1")],
                capture_output=True,
                text=True,
                timeout=300
            )
            return result.returncode == 0
        except Exception as e:
            print(f"Validation error: {e}")
            return False


def main():
    """CLI entry point"""
    project_root = Path.cwd()
    orchestrator = SpecOrchestrator(str(project_root))

    if len(sys.argv) > 1:
        command = sys.argv[1]

        if command == "status":
            print(orchestrator.get_status_report())

        elif command == "next":
            spec_id = orchestrator.get_next_executable()
            if spec_id:
                print(f"Next executable spec: {spec_id}")
            else:
                print("No more specs to execute")

        elif command == "start" and len(sys.argv) > 2:
            spec_id = sys.argv[2]
            if orchestrator.start_spec(spec_id):
                print(f"Started {spec_id}")
            else:
                print(f"Failed to start {spec_id}")

        elif command == "complete" and len(sys.argv) > 2:
            spec_id = sys.argv[2]
            orchestrator.complete_spec(spec_id)
            print(f"Completed {spec_id}")

        else:
            print("Usage: python spec_orchestrator.py [status|next|start <SPEC_ID>|complete <SPEC_ID>]")
    else:
        print(orchestrator.get_status_report())


if __name__ == "__main__":
    main()
