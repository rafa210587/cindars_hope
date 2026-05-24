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
from logger import ItemLogger, ItemSummary, RunSummary
from execution_queue import build_spec_queue, build_prompt_queue, build_agnostic_queue, print_dry_run_report, extract_spec_number, infer_target_spec
from spec_operations import (
    git_status, git_diff_stat, git_diff, git_commit, close_spec, archive_prompt, move_item_to_subdir,
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

    # Agnostic mode control
    parser.add_argument("--sort", choices=["natural", "alpha", "index"], default="natural",
                        help="Sort mode for queue: natural (default), alpha, or index")
    parser.add_argument("--completed-subdir", default="implementado",
                        help="Subdirectory name for completed items (relative to input folder)")
    parser.add_argument("--blocked-subdir", default="bloqueado",
                        help="Subdirectory name for blocked items (relative to input folder)")
    parser.add_argument("--include-all-md", action="store_true",
                        help="Include all .md files (ignore default patterns)")

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

    # Single run-level log root. All item logs, run summary and final checklist
    # must use the same run_id.
    run_id = datetime.now().strftime("%Y%m%d_%H%M%S")
    run_log_root = Path("orquestrador/logs") / run_id
    run_log_root.mkdir(parents=True, exist_ok=True)

    # Build queue (agnostic mode)
    if args.input_file:
        input_path = args.input_file
        base_dir = input_path.parent
        queue = [input_path]
        skipped = []
    else:
        input_path = args.input_dir
        base_dir = input_path

        queue, skipped = build_agnostic_queue(
            base_dir,
            ignored_patterns=config.get("ignored_prompt_patterns", []),
            sort_mode=args.sort,
            include_all_md=args.include_all_md
        )

    # Dry run: just print queue
    if args.dry_run:
        print_dry_run_report(queue, args.mode, ignored_patterns=config.get("ignored_prompt_patterns", []), skipped_items=skipped)
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
        logger = ItemLogger.create(item_id, run_log_root=run_log_root)

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

        # 7. Determine success (gate: agent SUCCESS AND spec COMPLETE AND validations)
        agent_success = agent_output.get("agent_result") == "SUCCESS"
        spec_complete = agent_output.get("spec_status") == "COMPLETE"
        validations_pass = validations.all_pass
        success = agent_success and spec_complete and validations_pass

        # In prompt mode, target_spec is inferred for logging only (not a gate)
        target_spec_inferred = target_spec is not None

        print(f"\n[SUCCESS GATE]")
        print(f"  AGENT_RESULT=SUCCESS: {agent_success}")
        print(f"  SPEC_STATUS=COMPLETE: {spec_complete}")
        print(f"  Validations all pass: {validations_pass}")
        if args.mode == "prompt":
            print(f"  Target spec inferred: {target_spec_inferred}")
        print(f"  FINAL SUCCESS: {success}\n")

        # 8. Move item to appropriate subdir (agnostic mode)
        item_moved_to_implemented = False
        item_moved_to_blocked = False
        completed_item_path = None
        blocked_item_path = None

        if success:
            dest = move_item_to_subdir(item_path, args.completed_subdir)
            if dest:
                item_moved_to_implemented = True
                completed_item_path = str(dest)
                executed_items.append(item_id)
        else:
            if args.archive_on_failure:
                dest = move_item_to_subdir(item_path, args.blocked_subdir)
                if dest:
                    item_moved_to_blocked = True
                    blocked_item_path = str(dest)

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
            commit_sha=commit_sha or "",
            started_at=start_time,
            finished_at=end_time,
            changed_files=agent_output.get("changed_files", []),
            errors=[],
            residual_risks=[],
            # New agnostic fields
            completed_item_path=completed_item_path,
            blocked_item_path=blocked_item_path,
            target_spec_inferred=target_spec_inferred,
            item_moved_to_implemented=item_moved_to_implemented,
            item_moved_to_blocked=item_moved_to_blocked,
        )

        logger.write_summary_json(summary)
        logger.write_summary_md(summary)

        # 10. Stop conditions
        if args.stop_after_one:
            break

        if args.stop_on_failure and not success:
            print(f"Stopping: {item_id} failed or partial")
            break

    # Generate run summary in the same run directory used by item logs
    log_root = run_log_root

    # Determine stats
    total_found = len(queue) + len(skipped)
    items_impl = [item.stem for item in queue if any(Path(item.parent / args.completed_subdir / item.name).exists() for _ in [None])]
    items_blocked = [item.stem for item in queue if any(Path(item.parent / args.blocked_subdir / item.name).exists() for _ in [None])]
    items_kept = [item.stem for item in queue if item.stem not in items_impl and item.stem not in items_blocked]

    run_summary = RunSummary(
        run_id=timestamp_dir,
        input_dir=str(base_dir),
        mode=args.mode,
        sort_mode=args.sort,
        total_found=total_found,
        total_ignored=len(skipped),
        total_executed=len(executed_items),
        total_success=len([item for item in executed_items if item]),
        total_failure=len(executed_items) - len([item for item in executed_items if item]),
        total_blocked=len(items_blocked),
        items_moved_to_implemented=items_impl,
        items_kept_in_origin=items_kept,
        items_moved_to_blocked=items_blocked,
        next_suggested=queue[len(executed_items)].name if len(executed_items) < len(queue) else None,
        stopped_on_failure=args.stop_on_failure and not success if 'success' in locals() else False
    )

    ItemLogger.write_run_summary(log_root, run_summary)

    # Generate final checklist (legacy)
    checklist_path = log_root / "FINAL_HUMAN_VALIDATION_CHECKLIST.md"
    generate_final_human_validation_checklist(
        executed_items,
        executed_specs,
        executed_prompts,
        log_root,
        checklist_path
    )

    print(f"\n{'='*70}")
    print(f"Orchestration Complete")
    print(f"{'='*70}")
    print(f"Total encontrado: {total_found}")
    print(f"Total ignorado: {len(skipped)}")
    print(f"Total executado: {len(executed_items)}")
    print(f"Movidos para {args.completed_subdir}/: {len(items_impl)}")
    print(f"Movidos para {args.blocked_subdir}/: {len(items_blocked)}")
    print(f"Run summary: {log_root}/RUN_SUMMARY.md")


if __name__ == "__main__":
    main()
