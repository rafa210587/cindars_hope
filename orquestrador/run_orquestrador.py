#!/usr/bin/env python3
r"""
Run Orquestrador - Main orchestration entry point for spec/prompt queue execution
Canonical command: python .\orquestrador\run_orquestrador.py
"""

import argparse
import json
import sys
from datetime import datetime
from pathlib import Path

from agent import run_with_fallback, build_agent_prompt, get_agent_rules, AgentResult
from logger import ItemLogger, ItemSummary
from execution_queue import build_spec_queue, build_prompt_queue, print_dry_run_report, extract_spec_number, infer_target_spec
from spec_operations import (
    git_status, git_diff_stat, git_diff, git_commit, close_spec, archive_prompt,
    build_repair_prompt, generate_final_human_validation_checklist
)
from validation import run_all_validations, parse_agent_result_block, normalize_spec_id


def load_config(config_path: Path) -> dict:
    """Load orchestrator configuration"""
    if not config_path.exists():
        print(f"ERROR: Config file not found: {config_path}")
        print(f"Expected at: {config_path.absolute()}")
        sys.exit(1)

    with open(config_path, "r", encoding="utf-8") as f:
        return json.load(f)


def main():
    """Main orchestrator entry point"""
    parser = argparse.ArgumentParser(
        description="Spec/Prompt Queue Orchestrator for Cindar's Hope",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # List queue without executing
  python run_orquestrador.py --mode spec --input-dir docs/specs/a_implementar --dry-run

  # Execute all prompts in folder
  python run_orquestrador.py --mode prompt --input-dir docs/agent_prompts/a_executar --stop-on-failure

  # Execute single spec
  python run_orquestrador.py --mode spec --input-file docs/specs/a_implementar/spec_player_combat_*.md
        """
    )

    # Required arguments
    parser.add_argument("--mode", required=True, choices=["spec", "prompt"],
                        help="Execution mode: spec or prompt")

    # Input selection
    input_group = parser.add_mutually_exclusive_group()
    input_group.add_argument("--input-dir", type=Path,
                            help="Directory containing .md files to execute")
    input_group.add_argument("--input-file", type=Path,
                            help="Single .md file to execute")

    # Configuration
    parser.add_argument("--config", type=Path, default=Path("orquestrador/orquestrador_config.json"),
                        help="Path to orchestrator config")

    # Agent configuration
    parser.add_argument("--primary-agent", choices=["claude", "codex"], default="claude",
                        help="Primary agent to use")
    parser.add_argument("--fallback-agent", choices=["claude", "codex", "none"], default="codex",
                        help="Fallback agent if primary fails")
    parser.add_argument("--max-repair-attempts", type=int, default=3,
                        help="Max repair loop iterations")

    # Execution control
    parser.add_argument("--stop-after-one", action="store_true",
                        help="Execute only one item and stop")
    parser.add_argument("--stop-on-failure", action="store_true", default=True,
                        help="Stop on first failure (default: True)")
    parser.add_argument("--continue-on-partial", action="store_true",
                        help="Continue even if item is partial")

    # Dry run and no-execution modes
    parser.add_argument("--dry-run", action="store_true",
                        help="List queue without executing")

    # Git control
    parser.add_argument("--no-auto-commit", action="store_true",
                        help="Don't auto-commit after item success")
    parser.add_argument("--commit-partial", action="store_true",
                        help="Commit even on partial failure")
    parser.add_argument("--allow-dirty", action="store_true",
                        help="Allow execution with dirty working tree")

    # Archiving
    parser.add_argument("--archive-on-success", action="store_true",
                        help="Archive prompt on success")
    parser.add_argument("--archive-on-failure", action="store_true",
                        help="Archive prompt on failure")

    args = parser.parse_args()

    # Validate arguments
    if not args.input_dir and not args.input_file:
        print("ERROR: Either --input-dir or --input-file required")
        sys.exit(1)

    # Load configuration
    config = load_config(args.config)
    config["primary_agent"] = args.primary_agent
    config["fallback_agent"] = args.fallback_agent
    config["max_repair_attempts"] = args.max_repair_attempts

    repo_root = Path(config.get("repo_root", "."))

    # Build queue
    if args.mode == "spec":
        input_path = args.input_dir or args.input_file
        queue = build_spec_queue(
            input_path if args.input_dir else input_path.parent,
            Path(config.get("spec_execution_order_path", "docs/specs/SPEC_EXECUTION_ORDER.md"))
        )
        if args.input_file:
            queue = [args.input_file]  # Override with single file
    else:  # prompt mode
        input_path = args.input_dir or args.input_file
        queue = build_prompt_queue(
            input_path if args.input_dir else input_path.parent,
            config.get("ignored_prompt_patterns", []),
            Path(config.get("prompt_input_dir", "docs/agent_prompts/a_executar")) / "00_INDEX_ORDEM_USO.md"
        )
        if args.input_file:
            queue = [args.input_file]

    # Dry run: just print queue
    if args.dry_run:
        print_dry_run_report(queue, args.mode, config.get("ignored_prompt_patterns", []))
        return

    # Main execution loop
    executed_items = []
    executed_specs = []
    executed_prompts = []

    for item_path in queue:
        item_id = item_path.stem
        print(f"\n{'='*70}")
        print(f"Processing: {item_id}")
        print(f"{'='*70}\n")

        # Create logger for this item
        logger = ItemLogger.create(item_id)

        # 1. Git status check
        clean, git_status_output = git_status(repo_root)
        logger.write_file("git_status_before.log", git_status_output)

        if not clean and not args.allow_dirty:
            print(f"ERROR: Working tree is dirty. Use --allow-dirty to continue.")
            print(git_status_output)
            return

        # 2. Read item content and build prompt
        item_content = item_path.read_text(encoding="utf-8")
        logger.write_file("input_item.md", item_content)

        # Infer target spec
        target_spec = infer_target_spec(
            item_path,
            item_content,
            Path(config.get("spec_execution_order_path", "docs/specs/SPEC_EXECUTION_ORDER.md"))
        ) if args.mode == "prompt" else item_path

        # Build agent prompt
        agent_rules = get_agent_rules()
        prompt = build_agent_prompt(item_content, item_path, target_spec, agent_rules)
        logger.write_file("rendered_prompt.md", prompt)

        # 3. Run agent with fallback
        start_time = datetime.now().isoformat()
        agent_result = run_with_fallback(prompt, config, repo_root, logger)
        end_time = datetime.now().isoformat()

        # 4. Parse agent result block
        agent_output = parse_agent_result_block(agent_result.combined)

        # 5. Run validations
        spec_num = extract_spec_number(item_path.name) if args.mode == "spec" else None
        spec_id = normalize_spec_id(spec_num) if spec_num else None
        validations = run_all_validations(repo_root, config, logger.get_log_dir(), spec_id)

        print(f"\nValidation Results:")
        print(f"  Docs: {validations.docs}")
        print(f"  Unity Compile: {validations.unity_compile}")
        print(f"  Repo Checks: {validations.repo_checks}")

        # 6. Repair loop
        repair_attempt = 0
        while not validations.all_pass and repair_attempt < config["max_repair_attempts"]:
            repair_attempt += 1
            print(f"\n[REPAIR ATTEMPT {repair_attempt}]")

            repair_prompt = build_repair_prompt(
                item_path,
                target_spec,
                {
                    "docs": validations.docs,
                    "unity_compile": validations.unity_compile,
                    "repo_checks": validations.repo_checks
                },
                logger.get_log_dir()
            )

            agent_result = run_with_fallback(repair_prompt, config, repo_root, logger)
            agent_output = parse_agent_result_block(agent_result.combined)
            validations = run_all_validations(repo_root, config, logger.get_log_dir(), spec_id)

        # 7. Determine success (correct gate per spec section 14)
        agent_success = agent_output.get("agent_result") == "SUCCESS"
        spec_complete = agent_output.get("spec_status") == "COMPLETE"
        validations_pass = validations.all_pass
        success = agent_success and spec_complete and validations_pass

        # In prompt mode, also require target_spec to be found
        if args.mode == "prompt":
            success = success and target_spec is not None

        print(f"\n[SUCCESS GATE]")
        print(f"  AGENT_RESULT=SUCCESS: {agent_success}")
        print(f"  SPEC_STATUS=COMPLETE: {spec_complete}")
        print(f"  Validations all pass: {validations_pass}")
        if args.mode == "prompt":
            print(f"  Target spec found: {target_spec is not None}")
        print(f"  FINAL SUCCESS: {success}\n")

        # 8. Close spec or archive prompt (transactional per spec section 14)
        closed_spec = False
        moved_prompt = False

        if success and target_spec:
            closed_spec = close_spec(target_spec, config, repo_root)
            if closed_spec:
                executed_specs.append(f"SPEC_{extract_spec_number(target_spec.name):02d}" if target_spec else "UNKNOWN")

        # Only archive prompt if spec closed successfully (or no spec in prompt mode)
        if args.mode == "prompt":
            if success and closed_spec:
                # Archive as executed
                moved_prompt = archive_prompt(item_path, Path(config.get("prompt_executed_dir", "docs/agent_prompts/executados")))
                executed_prompts.append(item_path.name)
            elif not success and args.archive_on_failure:
                # Archive as blocked
                moved_prompt = archive_prompt(item_path, Path(config.get("prompt_blocked_dir", "docs/agent_prompts/bloqueados")))

        # 9. Create summary and commit
        diff_stat = git_diff_stat(repo_root)
        diff_patch = git_diff(repo_root)
        logger.write_file("git_diff_stat.log", diff_stat)
        logger.write_file("git_diff.patch", diff_patch)

        commit_sha = None
        if success and not args.no_auto_commit:
            commit_sha = git_commit(repo_root, f"feat: concluir {item_id}")
        elif not success and args.commit_partial:
            commit_sha = git_commit(repo_root, f"chore: registrar parcial {item_id}")

        # Write summary
        summary = ItemSummary(
            mode=args.mode,
            item_path=str(item_path),
            target_spec_path=str(target_spec) if target_spec else None,
            primary_agent=config["primary_agent"],
            fallback_agent=config["fallback_agent"],
            used_fallback=agent_result.used_fallback,
            fallback_reason=agent_result.fallback_reason,
            agent_result=agent_output.get("agent_result", "UNKNOWN"),
            spec_status=agent_output.get("spec_status", "UNKNOWN"),
            docs_validation=validations.docs,
            unity_compile=validations.unity_compile,
            repo_checks=validations.repo_checks,
            closed_spec=closed_spec,
            moved_prompt=moved_prompt,
            commit_sha=commit_sha or "",
            started_at=start_time,
            finished_at=end_time,
            changed_files=agent_output.get("changed_files", []),
            errors=[],
            residual_risks=[]
        )

        logger.write_summary_json(summary)
        logger.write_summary_md(summary)

        executed_items.append(item_id)

        # 10. Stop conditions
        if args.stop_after_one:
            break

        if args.stop_on_failure and not success:
            print(f"Stopping: {item_id} failed or partial")
            break

    # Generate final checklist
    timestamp_dir = datetime.now().strftime("%Y%m%d_%H%M%S")
    log_root = Path("orquestrador/logs")
    checklist_path = log_root / timestamp_dir / "FINAL_HUMAN_VALIDATION_CHECKLIST.md"
    generate_final_human_validation_checklist(
        executed_items,
        executed_specs,
        executed_prompts,
        log_root / timestamp_dir,
        checklist_path
    )

    print(f"\n{'='*70}")
    print(f"Orchestration Complete")
    print(f"{'='*70}")
    print(f"Executed: {len(executed_items)} items")
    print(f"Specs closed: {len(executed_specs)}")
    print(f"Prompts executed: {len(executed_prompts)}")
    print(f"Checklist: {checklist_path}")


if __name__ == "__main__":
    main()
