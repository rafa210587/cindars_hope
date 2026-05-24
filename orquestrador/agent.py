"""
Agent - Claude and Codex subprocess invocation with fallback and streaming
"""

import json
import re
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Optional

from logger import ItemLogger


@dataclass
class AgentResult:
    """Result of an agent execution"""
    success: bool
    used_fallback: bool
    primary_agent: str
    fallback_agent: Optional[str]
    fallback_reason: str
    stdout: str
    stderr: str
    combined: str
    exit_code: int
    agent_used: str  # "claude" or "codex" - which one actually ran


def is_credit_error(output: str, credit_patterns: list) -> bool:
    """Check if output contains credit/quota error"""
    text = output.lower()
    for pattern in credit_patterns:
        if pattern.lower() in text:
            return True
    return False


def run_claude(
    prompt: str,
    repo_root: Path,
    timeout_minutes: int,
    logger: ItemLogger
) -> AgentResult:
    """
    Run Claude via CLI: claude -p "<prompt>" --output-format text
    """
    try:
        # Write prompt to temp file to avoid shell escaping issues
        temp_prompt_file = logger.get_log_dir() / "temp_prompt_claude.txt"
        temp_prompt_file.write_text(prompt, encoding="utf-8")

        cmd = [
            "claude",
            "-p", prompt,
            "--output-format", "text"
        ]

        print(f"[CLAUDE] Running: {' '.join(cmd[:3])}...")

        process = subprocess.Popen(
            cmd,
            cwd=str(repo_root),
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            bufsize=1
        )

        # Stream output
        stdout_text, stderr_text = logger.stream_process(
            process,
            "agent_primary_stdout.log",
            "agent_primary_stderr.log",
            "agent_primary_combined.log"
        )

        success = process.returncode == 0
        is_credit_fail = is_credit_error(stdout_text + stderr_text, [
            "credit", "quota", "limit", "insufficient"
        ])

        return AgentResult(
            success=success and not is_credit_fail,
            used_fallback=False,
            primary_agent="claude",
            fallback_agent=None,
            fallback_reason="" if success else ("Credit error" if is_credit_fail else "Execution error"),
            stdout=stdout_text,
            stderr=stderr_text,
            combined=stdout_text + "\n" + stderr_text,
            exit_code=process.returncode,
            agent_used="claude"
        )

    except FileNotFoundError:
        error = "Claude CLI not found in PATH. Install with: pip install anthropic-cli"
        print(f"[ERROR] {error}")
        return AgentResult(
            success=False,
            used_fallback=False,
            primary_agent="claude",
            fallback_agent=None,
            fallback_reason=error,
            stdout="",
            stderr=error,
            combined=error,
            exit_code=1,
            agent_used="none"
        )
    except subprocess.TimeoutExpired:
        error = f"Claude timeout ({timeout_minutes} minutes)"
        print(f"[ERROR] {error}")
        return AgentResult(
            success=False,
            used_fallback=False,
            primary_agent="claude",
            fallback_agent=None,
            fallback_reason=error,
            stdout="",
            stderr=error,
            combined=error,
            exit_code=124,
            agent_used="none"
        )
    except Exception as e:
        error = f"Claude error: {str(e)}"
        print(f"[ERROR] {error}")
        return AgentResult(
            success=False,
            used_fallback=False,
            primary_agent="claude",
            fallback_agent=None,
            fallback_reason=error,
            stdout="",
            stderr=error,
            combined=error,
            exit_code=1,
            agent_used="none"
        )


def run_codex(
    prompt: str,
    repo_root: Path,
    timeout_minutes: int,
    logger: ItemLogger
) -> AgentResult:
    """
    Run Codex via CLI: codex exec "<prompt>"
    """
    try:
        cmd = [
            "codex",
            "exec",
            prompt
        ]

        print(f"[CODEX] Running: {' '.join(cmd[:2])}...")

        process = subprocess.Popen(
            cmd,
            cwd=str(repo_root),
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            bufsize=1
        )

        # Stream output
        stdout_text, stderr_text = logger.stream_process(
            process,
            "agent_fallback_stdout.log",
            "agent_fallback_stderr.log",
            "agent_fallback_combined.log"
        )

        success = process.returncode == 0

        return AgentResult(
            success=success,
            used_fallback=True,
            primary_agent="claude",
            fallback_agent="codex",
            fallback_reason="Used after primary failed",
            stdout=stdout_text,
            stderr=stderr_text,
            combined=stdout_text + "\n" + stderr_text,
            exit_code=process.returncode,
            agent_used="codex"
        )

    except FileNotFoundError:
        error = "Codex CLI not found in PATH. Install with: pip install codex-cli"
        print(f"[ERROR] {error}")
        return AgentResult(
            success=False,
            used_fallback=True,
            primary_agent="claude",
            fallback_agent="codex",
            fallback_reason=error,
            stdout="",
            stderr=error,
            combined=error,
            exit_code=1,
            agent_used="none"
        )
    except subprocess.TimeoutExpired:
        error = f"Codex timeout ({timeout_minutes} minutes)"
        print(f"[ERROR] {error}")
        return AgentResult(
            success=False,
            used_fallback=True,
            primary_agent="claude",
            fallback_agent="codex",
            fallback_reason=error,
            stdout="",
            stderr=error,
            combined=error,
            exit_code=124,
            agent_used="none"
        )
    except Exception as e:
        error = f"Codex error: {str(e)}"
        print(f"[ERROR] {error}")
        return AgentResult(
            success=False,
            used_fallback=True,
            primary_agent="claude",
            fallback_agent="codex",
            fallback_reason=error,
            stdout="",
            stderr=error,
            combined=error,
            exit_code=1,
            agent_used="none"
        )


def run_with_fallback(
    prompt: str,
    config: dict,
    repo_root: Path,
    logger: ItemLogger
) -> AgentResult:
    """
    Run primary agent, fallback to secondary on credit error or failure.
    """
    primary = config.get("primary_agent", "claude")
    fallback = config.get("fallback_agent", "codex")
    timeout = config.get("agent_timeout_minutes", 90)

    print(f"\n[AGENT] Primary: {primary}, Fallback: {fallback}, Timeout: {timeout}m\n")

    # Run primary agent
    if primary == "claude":
        result = run_claude(prompt, repo_root, timeout, logger)
    else:
        result = run_codex(prompt, repo_root, timeout, logger)

    # Check if we should fallback
    should_fallback = (
        not result.success and
        fallback and
        fallback != "none" and
        (
            is_credit_error(result.combined, config.get("credit_error_patterns", [])) or
            "not found" in result.stderr.lower()
        )
    )

    if should_fallback:
        print(f"\n[FALLBACK] {result.fallback_reason}, trying {fallback}...\n")

        if fallback == "claude":
            result = run_claude(prompt, repo_root, timeout, logger)
        elif fallback == "codex":
            result = run_codex(prompt, repo_root, timeout, logger)

        result.used_fallback = True

    return result


def build_agent_prompt(
    item_content: str,
    item_path: Path,
    target_spec_path: Optional[Path],
    agent_rules: str
) -> str:
    """
    Build the final prompt to send to agent.
    Wraps raw content with rules and mandatory result block.
    """
    prompt = f"""
{agent_rules}

===CONTEÚDO DO ITEM===
File: {item_path.name}
Target Spec: {target_spec_path.name if target_spec_path else 'UNKNOWN'}

{item_content}

===BLOCO FINAL OBRIGATÓRIO===
Ao finalizar, responda EXATAMENTE neste formato (não modifique a estrutura):

AGENT_RESULT: SUCCESS | PARTIAL | BLOCKED | FAILED
SPEC_STATUS: COMPLETE | PARTIAL | BLOCKED
CHANGED_FILES:
- path1
- path2
VALIDATIONS:
- docs: PASS | FAIL | NOT_RUN
- unity_compile: PASS | FAIL | NOT_RUN
- repo_checks: PASS | FAIL | NOT_RUN
NEXT_ACTION: close_spec | repair | human_manual_validation | stop
"""
    return prompt


def get_agent_rules() -> str:
    """Return the mandatory inviolable rules for the agent"""
    rules = """
===REGRAS INVIOLÁVEIS DO ORQUESTRADOR===
A branch é controlada pelo orquestrador.
Não crie branch.
Não faça push.
Não faça merge.
Não altere arquivos fora do escopo do item.
Não avance para outro prompt/spec.
Não marque parcial como completo.
Não mova spec para implementados manualmente se validações não passaram.
Não apague logs do orquestrador.
Não edite docs_old/**.
Não recrie specs/ nem spec/.
Não use GameObject.Find() nem FindObjectOfType().
Não serialize Unity refs em DTOs de save.
Não use Dictionary em DTO serializado por JsonUtility.
Commits devem ser em português.
Use GameEventBus para comunicação (não direct calls).
ScriptableObjects devem usar sufixo _SO (Ex: SkillActionSO).
Eventos devem usar sufixo Event (Ex: DayStartedEvent).
IDs e tipos simples em SaveData, nunca refs Unity.
Mire nas patterns em memory/MEMORY.md se tarefa é similar.
===FIM DAS REGRAS===
"""
    return rules
