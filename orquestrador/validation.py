"""
Validation - Automatic validations for docs, Unity compile, and repo checks
"""

import re
import subprocess
from dataclasses import dataclass
from pathlib import Path
from typing import Optional


@dataclass
class ValidationResult:
    """Results of all validations"""
    docs: str  # "PASS", "FAIL", "NOT_RUN"
    unity_compile: str
    repo_checks: str
    details: dict = None

    def __post_init__(self):
        if self.details is None:
            self.details = {}

    @property
    def all_pass(self) -> bool:
        return all(v == "PASS" for v in [self.docs, self.unity_compile, self.repo_checks])


def run_docs_validation(repo_root: Path, log_path: Path) -> tuple[str, str]:
    """
    Run docs validation via PowerShell script.
    Returns (status: str, output: str)
    """
    script_path = repo_root / "tools" / "docs" / "validate_docs.ps1"

    if not script_path.exists():
        output = f"Script not found: {script_path}"
        log_path.write_text(output, encoding="utf-8")
        return "NOT_RUN", output

    try:
        result = subprocess.run(
            ["powershell", "-ExecutionPolicy", "Bypass", "-File", str(script_path)],
            cwd=str(repo_root),
            capture_output=True,
            text=True,
            timeout=300
        )

        output = result.stdout + "\n" + result.stderr
        log_path.write_text(output, encoding="utf-8")

        status = "PASS" if result.returncode == 0 else "FAIL"
        return status, output

    except subprocess.TimeoutExpired:
        output = "Docs validation timeout (300s)"
        log_path.write_text(output, encoding="utf-8")
        return "FAIL", output
    except Exception as e:
        output = f"Docs validation error: {str(e)}"
        log_path.write_text(output, encoding="utf-8")
        return "FAIL", output


def run_unity_compile_validation(
    repo_root: Path,
    unity_editor_path: str,
    log_path: Path
) -> tuple[str, str]:
    """
    Run Unity compile validation via PowerShell script.
    Returns (status: str, output: str)
    """
    script_path = repo_root / "tools" / "unity" / "RunUnityCompileValidation.ps1"

    if not script_path.exists():
        output = f"Script not found: {script_path}"
        log_path.write_text(output, encoding="utf-8")
        return "NOT_RUN", output

    try:
        result = subprocess.run(
            [
                "powershell",
                "-ExecutionPolicy", "Bypass",
                "-File", str(script_path),
                "-UnityEditorPath", unity_editor_path,
                "-ProjectPath", str(repo_root),
                "-LogFile", str(log_path)
            ],
            cwd=str(repo_root),
            capture_output=True,
            text=True,
            timeout=1200  # 20 minutes
        )

        output = result.stdout + "\n" + result.stderr

        # Append to log if it has content from script
        existing = ""
        if log_path.exists():
            existing = log_path.read_text(encoding="utf-8")
        log_path.write_text(existing + "\n" + output, encoding="utf-8")

        status = "PASS" if result.returncode == 0 else "FAIL"
        return status, output

    except subprocess.TimeoutExpired:
        output = "Unity compile validation timeout (1200s)"
        log_path.write_text(output, encoding="utf-8")
        return "FAIL", output
    except Exception as e:
        output = f"Unity compile validation error: {str(e)}"
        log_path.write_text(output, encoding="utf-8")
        return "FAIL", output


def run_unity_scan(repo_root: Path, log_path: Path) -> tuple[str, str]:
    """
    Run Unity log scan via PowerShell script.
    Returns (status: str, output: str)
    """
    script_path = repo_root / "tools" / "unity" / "ScanUnityLogs.ps1"

    if not script_path.exists():
        output = f"Script not found: {script_path}"
        log_path.write_text(output, encoding="utf-8")
        return "NOT_RUN", output

    try:
        result = subprocess.run(
            [
                "powershell",
                "-ExecutionPolicy", "Bypass",
                "-File", str(script_path)
            ],
            cwd=str(repo_root),
            capture_output=True,
            text=True,
            timeout=300
        )

        output = result.stdout + "\n" + result.stderr
        log_path.write_text(output, encoding="utf-8")

        status = "PASS" if result.returncode == 0 else "FAIL"
        return status, output

    except subprocess.TimeoutExpired:
        output = "Unity scan timeout (300s)"
        log_path.write_text(output, encoding="utf-8")
        return "FAIL", output
    except Exception as e:
        output = f"Unity scan error: {str(e)}"
        log_path.write_text(output, encoding="utf-8")
        return "FAIL", output


def run_repo_checks_global(repo_root: Path, log_path: Path) -> tuple[str, list[str]]:
    """
    Run global repo checks via PowerShell Select-String.
    Returns (status: str, found_issues: list[str])
    """
    checks = [
        ("GameObject.Find", r"GameObject\.Find\("),
        ("FindObjectOfType", r"FindObjectOfType"),
        ("QuestManager", r"QuestManager"),
        ("Dictionary<string, DurabilityEntry>", r"Dictionary<string, DurabilityEntry>"),
    ]

    issues = []
    outputs = []

    try:
        for check_name, pattern in checks:
            result = subprocess.run(
                [
                    "powershell",
                    "-Command",
                    f'Select-String -Path "Assets/_Game/Scripts/**/*.cs" -Pattern "{pattern}" -Recurse | Select-Object -ExpandProperty Line | Measure-Object | Select-Object -ExpandProperty Count'
                ],
                cwd=str(repo_root),
                capture_output=True,
                text=True,
                timeout=60
            )

            count = result.stdout.strip()
            if count and int(count) > 0:
                issues.append(f"{check_name}: {count} occurrences found")
                outputs.append(f"❌ {check_name}: {count}")
            else:
                outputs.append(f"✅ {check_name}: OK")

        output = "\n".join(outputs)
        log_path.write_text(output, encoding="utf-8")

        status = "FAIL" if issues else "PASS"
        return status, issues

    except Exception as e:
        output = f"Repo checks error: {str(e)}"
        log_path.write_text(output, encoding="utf-8")
        return "FAIL", [f"Error: {str(e)}"]


def run_repo_checks_spec(repo_root: Path, spec_id: str, log_path: Path) -> tuple[str, list[str]]:
    """
    Run spec-specific repo checks.
    Returns (status: str, found_issues: list[str])
    """
    # Spec-specific checks
    spec_checks = {
        "SPEC_012": [
            ("SkillActionSO", r"SkillActionSO"),
            ("ManaManager", r"ManaManager"),
            ("ArcaneBolt", r"ArcaneBolt"),
            ("PlayerAttackController", r"PlayerAttackController"),
        ],
        "SPEC_016": [
            ("SkillTreeManager", r"SkillTreeManager"),
            ("SkillNodeDataSO", r"SkillNodeDataSO"),
            ("SkillTreeDataSO", r"SkillTreeDataSO"),
        ],
    }

    if spec_id not in spec_checks:
        output = f"No spec-specific checks for {spec_id}"
        log_path.write_text(output, encoding="utf-8")
        return "PASS", []

    checks = spec_checks[spec_id]
    issues = []
    outputs = []

    try:
        for check_name, pattern in checks:
            result = subprocess.run(
                [
                    "powershell",
                    "-Command",
                    f'Select-String -Path "Assets/_Game/Scripts/**/*.cs" -Pattern "{pattern}" -Recurse | Measure-Object | Select-Object -ExpandProperty Count'
                ],
                cwd=str(repo_root),
                capture_output=True,
                text=True,
                timeout=60
            )

            count = result.stdout.strip()
            if count and int(count) > 0:
                outputs.append(f"✅ {check_name}: {count} found")
            else:
                issues.append(f"{check_name}: Not found or zero occurrences")
                outputs.append(f"⚠️  {check_name}: Not found")

        output = "\n".join(outputs)
        log_path.write_text(output, encoding="utf-8")

        # For spec checks, we don't fail - they're informational
        return "PASS", issues

    except Exception as e:
        output = f"Spec checks error: {str(e)}"
        log_path.write_text(output, encoding="utf-8")
        return "PASS", [f"Error: {str(e)}"]


def run_all_validations(
    repo_root: Path,
    config: dict,
    log_dir: Path,
    spec_id: Optional[str] = None
) -> ValidationResult:
    """
    Run all validations and return results.
    """
    # Docs validation
    docs_status = "NOT_RUN"
    if config.get("require_docs_validation", True):
        docs_status, _ = run_docs_validation(repo_root, log_dir / "docs_validation.log")

    # Unity compile validation
    unity_status = "NOT_RUN"
    if config.get("require_unity_compile", True):
        unity_editor = config.get("unity_editor_path")
        unity_status, _ = run_unity_compile_validation(
            repo_root,
            unity_editor,
            log_dir / "unity_compile_validation.log"
        )

    # Run Unity scan (secondary)
    if unity_status == "PASS":
        run_unity_scan(repo_root, log_dir / "unity_scan.log")

    # Repo checks
    repo_status = "NOT_RUN"
    if config.get("require_repo_checks", True):
        repo_status, _ = run_repo_checks_global(repo_root, log_dir / "repo_checks.log")

        # Spec-specific checks (informational)
        if spec_id:
            run_repo_checks_spec(repo_root, spec_id, log_dir / f"repo_checks_{spec_id}.log")

    return ValidationResult(
        docs=docs_status,
        unity_compile=unity_status,
        repo_checks=repo_status,
        details={}
    )


def parse_agent_result_block(output: str) -> dict:
    """
    Parse the mandatory agent result block from output.
    Returns dict with AGENT_RESULT, SPEC_STATUS, CHANGED_FILES, VALIDATIONS, NEXT_ACTION
    """
    result = {
        "agent_result": "UNKNOWN",
        "spec_status": "UNKNOWN",
        "changed_files": [],
        "validations": {},
        "next_action": "stop",
    }

    # Parse AGENT_RESULT
    match = re.search(r"AGENT_RESULT:\s*(SUCCESS|PARTIAL|BLOCKED|FAILED)", output, re.IGNORECASE)
    if match:
        result["agent_result"] = match.group(1).upper()

    # Parse SPEC_STATUS
    match = re.search(r"SPEC_STATUS:\s*(COMPLETE|PARTIAL|BLOCKED)", output, re.IGNORECASE)
    if match:
        result["spec_status"] = match.group(1).upper()

    # Parse CHANGED_FILES
    files_match = re.search(r"CHANGED_FILES:\s*([\s\S]*?)(?=\n\w+:|$)", output, re.IGNORECASE)
    if files_match:
        files_text = files_match.group(1)
        files = [line.strip("- ").strip() for line in files_text.split("\n") if line.strip() and line.strip() != "-"]
        result["changed_files"] = files

    # Parse VALIDATIONS
    val_match = re.search(r"VALIDATIONS:\s*([\s\S]*?)(?=\n\w+:|$)", output, re.IGNORECASE)
    if val_match:
        val_text = val_match.group(1)
        for line in val_text.split("\n"):
            if ":" in line:
                key, val = line.split(":", 1)
                result["validations"][key.strip()] = val.strip()

    # Parse NEXT_ACTION
    match = re.search(r"NEXT_ACTION:\s*(\w+)", output, re.IGNORECASE)
    if match:
        result["next_action"] = match.group(1).lower()

    return result
