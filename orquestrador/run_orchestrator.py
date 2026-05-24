#!/usr/bin/env python3
"""
[DEPRECATED - LEGACY SYSTEM v1.0]

Run Orchestrator - Main entry point for executing specs via Claude orchestration
(Uses Anthropic API, not CLI subprocess)

⚠️  THIS IS THE OLD SYSTEM. Use run_orquestrador.py instead (v2.0 - queue-based)

Canonical command: python .\orquestrador\run_orquestrador.py
Legacy command (still works): python .\orquestrador\run_orchestrator.py

See CONFIG_GUIDE.md for which config file to use.
"""

import argparse
import sys
from pathlib import Path
from datetime import datetime

from spec_orchestrator import SpecOrchestrator, TaskStatus
from claude_api_integration import ClaudeSpecExecutor, SpecExecutionPrompt


class OrchestratorRunner:
    """Executes spec orchestration workflow"""

    def __init__(self, project_root: str):
        self.project_root = Path(project_root)
        self.orchestrator = SpecOrchestrator(str(self.project_root))
        self.api_executor = ClaudeSpecExecutor("config.json")
        self.log_file = self.project_root / "orquestrador" / "orchestrator.log"

    def log(self, message: str):
        """Write to log file and stdout"""
        timestamp = datetime.now().isoformat()
        log_entry = f"[{timestamp}] {message}"
        print(log_entry)
        with open(self.log_file, 'a', encoding='utf-8') as f:
            f.write(log_entry + "\n")

    def run_spec_sequence(self, start_spec: str = None, max_specs: int = None):
        """Execute specs in dependency order"""
        self.log(f"Starting orchestrated spec execution from {start_spec or 'beginning'}")

        specs_executed = 0
        start_index = 0

        if start_spec:
            try:
                start_index = self.orchestrator.state.execution_order.index(start_spec)
            except ValueError:
                self.log(f"ERROR: Spec {start_spec} not found in execution order")
                return

        for i, spec_id in enumerate(self.orchestrator.state.execution_order[start_index:]):
            if max_specs and specs_executed >= max_specs:
                self.log(f"Reached max specs limit ({max_specs})")
                break

            spec = self.orchestrator.state.specs[spec_id]

            # Check if executable
            can_exec, reason = self.orchestrator.can_execute(spec_id)
            if not can_exec:
                self.log(f"⏸️  {spec_id} blocked: {reason}")
                continue

            if spec.status == TaskStatus.COMPLETED:
                self.log(f"✅ {spec_id} already completed")
                continue

            self.log(f"\n{'='*70}")
            self.log(f"🚀 Executing {spec_id}: {spec.name}")
            self.log(f"{'='*70}")

            self.orchestrator.start_spec(spec_id)

            for task_idx, task in enumerate(spec.tasks, 1):
                if task.status == TaskStatus.COMPLETED:
                    continue

                self.log(f"\n  [{task_idx}/{len(spec.tasks)}] {task.name}")

                # Build execution prompt
                exec_prompt = SpecExecutionPrompt(
                    spec_id=spec_id,
                    spec_name=spec.name,
                    task_name=task.name,
                    spec_file_path=spec.file_path,
                    task_description=task.description,
                    blockers_resolved=spec.blocked_by,
                    project_root=str(self.project_root),
                    validation_required=True
                )

                # Execute via Claude API
                self.log(f"    Calling Claude API...")
                success, response = self.api_executor.execute_spec_task(exec_prompt)

                if success:
                    self.log(f"    ✅ Task completed")
                    self.orchestrator.complete_task(spec_id, task.task_id)
                else:
                    self.log(f"    ❌ Task failed: {response[:200]}")
                    self.orchestrator.fail_spec(spec_id, response)
                    break

            # Check if all tasks completed
            all_completed = all(t.status == TaskStatus.COMPLETED for t in spec.tasks)
            if all_completed:
                self.orchestrator.complete_spec(spec_id)
                self.log(f"✅ {spec_id} COMPLETED")
                specs_executed += 1
            else:
                self.log(f"❌ {spec_id} FAILED - Not all tasks completed")

            self.log(f"Status:\n{self.orchestrator.get_status_report()}")

        self.log(f"\nOrchestration cycle complete. {specs_executed} spec(s) executed.")

    def run_next_spec(self):
        """Execute only the next pending spec"""
        spec_id = self.orchestrator.get_next_executable()
        if not spec_id:
            self.log("No specs ready to execute")
            return

        self.log(f"Executing next spec: {spec_id}")
        self.run_spec_sequence(start_spec=spec_id, max_specs=1)

    def show_status(self):
        """Display current orchestration status"""
        print(self.orchestrator.get_status_report())

    def reset_state(self):
        """Reset orchestration to initial state"""
        self.orchestrator = SpecOrchestrator(str(self.project_root))
        self.log("Orchestration state reset")


def main():
    parser = argparse.ArgumentParser(description="Spec Orchestrator for Cindar's Hope")
    parser.add_argument("--project-root", default=".", help="Project root directory")
    parser.add_argument("--command", choices=["run", "next", "status", "reset"], default="status",
                       help="Command to execute")
    parser.add_argument("--start-spec", help="Start from specific spec (for 'run' command)")
    parser.add_argument("--max-specs", type=int, help="Maximum specs to execute in one run")

    args = parser.parse_args()

    runner = OrchestratorRunner(args.project_root)

    if args.command == "run":
        runner.run_spec_sequence(start_spec=args.start_spec, max_specs=args.max_specs)
    elif args.command == "next":
        runner.run_next_spec()
    elif args.command == "status":
        runner.show_status()
    elif args.command == "reset":
        runner.reset_state()


if __name__ == "__main__":
    main()
