"""
Logger - Structured per-item logging with real-time stdout/stderr streaming
"""

import json
import subprocess
import sys
import threading
import time
from dataclasses import dataclass, asdict, field
from datetime import datetime
from pathlib import Path
from typing import Optional, List


@dataclass
class AgentExecution:
    """Record of a single agent execution"""
    primary_agent: str
    fallback_agent: Optional[str]
    used_fallback: bool
    primary_exit_code: int
    fallback_exit_code: Optional[int] = None


@dataclass
class ValidationResult:
    """Results of all validations"""
    docs: str  # "PASS", "FAIL", "NOT_RUN"
    unity_compile: str
    repo_checks: str

    @property
    def all_pass(self) -> bool:
        return all(v == "PASS" for v in [self.docs, self.unity_compile, self.repo_checks])


@dataclass
class ItemSummary:
    """Summary of an orchestrator item execution"""
    mode: str
    item_path: str
    target_spec_path: Optional[str]
    primary_agent: str
    fallback_agent: Optional[str]
    used_fallback: bool
    fallback_reason: str
    agent_result: str  # SUCCESS, PARTIAL, BLOCKED, FAILED
    spec_status: str  # COMPLETE, PARTIAL, BLOCKED
    docs_validation: str
    unity_compile: str
    repo_checks: str
    closed_spec: bool = False  # Legacy field
    moved_prompt: bool = False  # Legacy field
    commit_sha: str = ""
    started_at: str = ""
    finished_at: str = ""
    changed_files: List[str] = field(default_factory=list)
    errors: List[str] = field(default_factory=list)
    residual_risks: List[str] = field(default_factory=list)
    # New agnostic fields
    completed_item_path: Optional[str] = None
    blocked_item_path: Optional[str] = None
    target_spec_inferred: bool = False
    item_moved_to_implemented: bool = False
    item_moved_to_blocked: bool = False


@dataclass
class RunSummary:
    """Run-level summary across all executed items"""
    run_id: str
    input_dir: str
    mode: str
    sort_mode: str
    total_found: int
    total_ignored: int
    total_executed: int
    total_success: int
    total_failure: int
    total_blocked: int
    items_moved_to_implemented: List[str] = field(default_factory=list)
    items_kept_in_origin: List[str] = field(default_factory=list)
    items_moved_to_blocked: List[str] = field(default_factory=list)
    next_suggested: Optional[str] = None
    stopped_on_failure: bool = False

    def to_dict(self) -> dict:
        """Convert to dictionary"""
        return asdict(self)

    def to_markdown(self) -> str:
        """Generate markdown representation"""
        return f"""# RUN_SUMMARY

**Run ID**: {self.run_id}
**Input Dir**: {self.input_dir}
**Mode**: {self.mode}
**Sort Mode**: {self.sort_mode}

## Estatísticas

| Métrica | Valor |
|---|---|
| Total encontrado | {self.total_found} |
| Total ignorado | {self.total_ignored} |
| Total executado | {self.total_executed} |
| Sucesso | {self.total_success} |
| Falha | {self.total_failure} |
| Bloqueado | {self.total_blocked} |

## Items Movidos para Implementado

{chr(10).join(f"- {item}" for item in self.items_moved_to_implemented) if self.items_moved_to_implemented else "Nenhum"}

## Items Mantidos na Origem (Falha)

{chr(10).join(f"- {item}" for item in self.items_kept_in_origin) if self.items_kept_in_origin else "Nenhum"}

## Items Movidos para Bloqueado

{chr(10).join(f"- {item}" for item in self.items_moved_to_blocked) if self.items_moved_to_blocked else "Nenhum"}

## Próximo Sugerido

{self.next_suggested if self.next_suggested else "Nenhum"}

## Status de Parada

Parou na falha: {self.stopped_on_failure}
"""


class ItemLogger:
    """Manages logging for a single orchestrator item execution"""

    def __init__(self, log_dir: Path):
        self.log_dir = log_dir
        self.log_dir.mkdir(parents=True, exist_ok=True)

    @classmethod
    def create(cls, item_id: str) -> "ItemLogger":
        """Create a new logger for an item with timestamp directory"""
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        log_dir = Path("orquestrador/logs") / timestamp / item_id
        return cls(log_dir)

    def write_file(self, filename: str, content: str) -> Path:
        """Write content to a log file"""
        file_path = self.log_dir / filename
        file_path.write_text(content, encoding="utf-8")
        return file_path

    def stream_process(
        self,
        process: subprocess.Popen,
        stdout_filename: str,
        stderr_filename: str,
        combined_filename: str,
        timeout_seconds: Optional[int] = None
    ) -> tuple[str, str]:
        """
        Stream stdout and stderr from process in real-time.
        Writes to files AND prints to console.
        Enforces timeout if specified.
        Returns (stdout_text, stderr_text) tuple.
        """
        stdout_path = self.log_dir / stdout_filename
        stderr_path = self.log_dir / stderr_filename
        combined_path = self.log_dir / combined_filename

        stdout_lines = []
        stderr_lines = []
        combined_lines = []

        def read_stdout():
            """Thread: read stdout"""
            try:
                for line in process.stdout:
                    stdout_lines.append(line)
                    combined_lines.append(f"[STDOUT] {line}")
                    sys.stdout.write(f"{line}")
                    sys.stdout.flush()
            except Exception:
                pass

        def read_stderr():
            """Thread: read stderr"""
            try:
                for line in process.stderr:
                    stderr_lines.append(line)
                    combined_lines.append(f"[STDERR] {line}")
                    sys.stderr.write(f"{line}")
                    sys.stderr.flush()
            except Exception:
                pass

        # Start reader threads
        t_out = threading.Thread(target=read_stdout, daemon=True)
        t_err = threading.Thread(target=read_stderr, daemon=True)
        t_out.start()
        t_err.start()

        # Wait for process with timeout
        try:
            if timeout_seconds is not None:
                process.wait(timeout=timeout_seconds)
            else:
                process.wait()
        except subprocess.TimeoutExpired:
            print(f"\n[TIMEOUT] Process exceeded {timeout_seconds}s, killing...\n")
            process.kill()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                # Force kill if needed
                process.terminate()
                process.wait()
            # Set exit code to timeout indicator
            process.returncode = 124

        # Wait for threads to finish reading any remaining output
        t_out.join(timeout=5)
        t_err.join(timeout=5)

        # Write accumulated output to files
        stdout_text = "".join(stdout_lines)
        stderr_text = "".join(stderr_lines)
        combined_text = "".join(combined_lines)

        stdout_path.write_text(stdout_text, encoding="utf-8")
        stderr_path.write_text(stderr_text, encoding="utf-8")
        combined_path.write_text(combined_text, encoding="utf-8")

        return stdout_text, stderr_text

    def write_summary_json(self, summary: ItemSummary) -> Path:
        """Write summary.json"""
        data = asdict(summary)
        file_path = self.log_dir / "summary.json"
        file_path.write_text(json.dumps(data, indent=2), encoding="utf-8")
        return file_path

    def write_summary_md(self, summary: ItemSummary) -> Path:
        """Write summary.md"""
        md = f"""# Summary - {summary.item_path}

## Execution Details
- **Mode**: {summary.mode}
- **Target Spec**: {summary.target_spec_path or "N/A"}
- **Started**: {summary.started_at}
- **Finished**: {summary.finished_at}

## Agent Execution
- **Primary Agent**: {summary.primary_agent}
- **Used Fallback**: {summary.used_fallback}
- **Fallback Reason**: {summary.fallback_reason or "N/A"}
- **Agent Result**: {summary.agent_result}

## Spec Status
- **Spec Status**: {summary.spec_status}
- **Spec Closed**: {summary.closed_spec}
- **Prompt Moved**: {summary.moved_prompt}

## Validations
- **Docs**: {summary.docs_validation}
- **Unity Compile**: {summary.unity_compile}
- **Repo Checks**: {summary.repo_checks}

## Git
- **Commit SHA**: {summary.commit_sha or "No commit"}

## Changed Files
{chr(10).join(f"- {f}" for f in summary.changed_files) if summary.changed_files else "None"}

## Errors
{chr(10).join(f"- {e}" for e in summary.errors) if summary.errors else "None"}

## Residual Risks
{chr(10).join(f"- {r}" for r in summary.residual_risks) if summary.residual_risks else "None"}
"""
        file_path = self.log_dir / "summary.md"
        file_path.write_text(md, encoding="utf-8")
        return file_path

    @staticmethod
    def write_run_summary(log_root: Path, run_summary: "RunSummary") -> Path:
        """Write run summary as JSON and Markdown"""
        run_summary_json = log_root / "RUN_SUMMARY.json"
        run_summary_md = log_root / "RUN_SUMMARY.md"

        run_summary_json.write_text(
            json.dumps(run_summary.to_dict(), indent=2, default=str),
            encoding="utf-8"
        )

        run_summary_md.write_text(
            run_summary.to_markdown(),
            encoding="utf-8"
        )

        print(f"Generated: {run_summary_json}")
        print(f"Generated: {run_summary_md}")

        return log_root

    def get_log_dir(self) -> Path:
        """Get the log directory path"""
        return self.log_dir
