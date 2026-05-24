"""
Spec Operations - Spec closing, prompt archiving, registry updates, and git operations
"""

import shutil
import subprocess
from pathlib import Path
from typing import Optional, Tuple


def git_status(repo_root: Path) -> Tuple[bool, str]:
    """
    Check git status.
    Returns (is_clean: bool, output: str)
    """
    try:
        result = subprocess.run(
            ["git", "status", "--porcelain"],
            cwd=str(repo_root),
            capture_output=True,
            text=True,
            timeout=10
        )

        output = result.stdout.strip()
        is_clean = len(output) == 0

        return is_clean, output

    except Exception as e:
        return False, f"Error checking git status: {str(e)}"


def git_diff_stat(repo_root: Path) -> str:
    """Get git diff statistics"""
    try:
        result = subprocess.run(
            ["git", "diff", "--stat"],
            cwd=str(repo_root),
            capture_output=True,
            text=True,
            timeout=10
        )
        return result.stdout

    except Exception as e:
        return f"Error getting diff stat: {str(e)}"


def git_diff(repo_root: Path) -> str:
    """Get git diff patch"""
    try:
        result = subprocess.run(
            ["git", "diff"],
            cwd=str(repo_root),
            capture_output=True,
            text=True,
            timeout=10
        )
        return result.stdout

    except Exception as e:
        return f"Error getting diff: {str(e)}"


def git_add_all(repo_root: Path) -> bool:
    """
    Stage all changes.
    Returns success status.
    """
    try:
        result = subprocess.run(
            ["git", "add", "."],
            cwd=str(repo_root),
            capture_output=True,
            timeout=10
        )
        return result.returncode == 0

    except Exception as e:
        print(f"Error staging changes: {str(e)}")
        return False


def git_commit(repo_root: Path, message: str) -> Optional[str]:
    """
    Create a commit.
    Returns commit SHA or None on failure.
    """
    try:
        # Stage changes
        if not git_add_all(repo_root):
            print("Failed to stage changes")
            return None

        result = subprocess.run(
            ["git", "commit", "-m", message],
            cwd=str(repo_root),
            capture_output=True,
            text=True,
            timeout=10
        )

        if result.returncode == 0:
            # Extract SHA from output (git commit outputs something like [dev abc123def])
            import re
            match = re.search(r"\[.*?\s+([a-f0-9]{7})", result.stdout)
            if match:
                return match.group(1)
            return "unknown"
        else:
            print(f"Commit failed: {result.stderr}")
            return None

    except Exception as e:
        print(f"Error committing: {str(e)}")
        return None


def close_spec(
    spec_path: Path,
    config: dict,
    repo_root: Path
) -> bool:
    """
    Close a spec by moving it from a_implementar to implementados.
    Updates related registries.
    Returns success status.
    """
    if not spec_path.exists():
        print(f"Spec file not found: {spec_path}")
        return False

    try:
        # Determine target directory
        impl_dir = Path(config.get("implemented_specs_dir", "docs/specs/implementados"))
        impl_dir.mkdir(parents=True, exist_ok=True)

        # Move spec file
        target_path = impl_dir / spec_path.name
        shutil.move(str(spec_path), str(target_path))
        print(f"Moved spec: {spec_path.name} → implementados/")

        # Update registries
        update_implementation_registries(spec_path.name, config, repo_root)

        return True

    except Exception as e:
        print(f"Error closing spec: {str(e)}")
        return False


def archive_prompt(
    prompt_path: Path,
    archive_dir: Path
) -> bool:
    """
    Archive a prompt by moving it to executados or bloqueados directory.
    """
    if not prompt_path.exists():
        print(f"Prompt file not found: {prompt_path}")
        return False

    try:
        archive_dir.mkdir(parents=True, exist_ok=True)
        target_path = archive_dir / prompt_path.name
        shutil.move(str(prompt_path), str(target_path))
        print(f"Archived prompt: {prompt_path.name} → {archive_dir.name}/")
        return True

    except Exception as e:
        print(f"Error archiving prompt: {str(e)}")
        return False


def update_implementation_registries(
    spec_filename: str,
    config: dict,
    repo_root: Path
) -> bool:
    """
    Update SPEC_REGISTRY_IMPLEMENTED.md and SPEC_REGISTRY_TO_IMPLEMENT.md
    """
    try:
        # Extract spec number from filename
        import re
        match = re.search(r"spec_(\d+)_", spec_filename)
        spec_num = int(match.group(1)) if match else None

        if spec_num is None:
            print(f"Could not extract spec number from {spec_filename}")
            return False

        spec_label = f"SPEC_{spec_num:02d}"

        # Update SPEC_REGISTRY_IMPLEMENTED.md
        impl_reg_path = Path(config.get("implemented_registry_path", "docs/specs/SPEC_REGISTRY_IMPLEMENTED.md"))
        if impl_reg_path.exists():
            content = impl_reg_path.read_text(encoding="utf-8")
            if spec_label not in content:
                # Add to list
                lines = content.split("\n")
                for i, line in enumerate(lines):
                    if "- " in line and spec_label not in line:
                        lines.insert(i, f"- {spec_label}: {spec_filename}")
                        break
                content = "\n".join(lines)
                impl_reg_path.write_text(content, encoding="utf-8")
                print(f"Updated {impl_reg_path.name}")

        # Update SPEC_REGISTRY_TO_IMPLEMENT.md
        to_impl_reg_path = Path(config.get("to_implement_registry_path", "docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md"))
        if to_impl_reg_path.exists():
            content = to_impl_reg_path.read_text(encoding="utf-8")
            # Remove from to_implement list
            lines = [l for l in content.split("\n") if spec_label not in l or ":" not in l]
            content = "\n".join(lines)
            to_impl_reg_path.write_text(content, encoding="utf-8")
            print(f"Updated {to_impl_reg_path.name}")

        # Update IMPLEMENTATION_STATUS.md if it exists
        impl_status_path = Path(config.get("implementation_status_path", "docs/IMPLEMENTATION_STATUS.md"))
        if impl_status_path.exists():
            content = impl_status_path.read_text(encoding="utf-8")
            # Try to update status for this spec (simplified approach)
            content = content.replace(
                f"{spec_label}: [In Progress]",
                f"{spec_label}: [✅ Complete]"
            )
            impl_status_path.write_text(content, encoding="utf-8")

        return True

    except Exception as e:
        print(f"Error updating registries: {str(e)}")
        return False


def build_repair_prompt(
    item_path: Path,
    target_spec_path: Optional[Path],
    validation_results: dict,
    log_dir: Path
) -> str:
    """
    Build a repair prompt for failed validations.
    """
    failures = []
    for val_type, status in validation_results.items():
        if status != "PASS" and status != "NOT_RUN":
            failures.append(f"- {val_type}: {status}")

    failure_summary = "\n".join(failures) if failures else "Unknown failure"

    prompt = f"""
A execução anterior de {item_path.name} falhou nas validações.

Item:
{item_path}

Spec alvo:
{target_spec_path.name if target_spec_path else 'UNKNOWN'}

Falhas encontradas:
{failure_summary}

Logs de validação estão em:
- {log_dir}/docs_validation.log
- {log_dir}/unity_compile_validation.log
- {log_dir}/repo_checks.log

Corrija APENAS o necessário para passar nas validações.
Não avance para outro item.
Não refaça tudo do zero.
Rode as validações novamente.
Finalize com o BLOCO FINAL OBRIGATÓRIO.
"""
    return prompt


def generate_final_human_validation_checklist(
    executed_items: list,
    executed_specs: list,
    executed_prompts: list,
    log_dir: Path,
    output_path: Path
) -> Path:
    """
    Generate final human validation checklist.
    """
    checklist = f"""# FINAL_HUMAN_VALIDATION_CHECKLIST

Generated from orchestrator execution on {log_dir.name}

## Resumo da Execução

- Total de items: {len(executed_items)}
- Specs completadas: {len(executed_specs)}
- Prompts executados: {len(executed_prompts)}

## Specs Completadas
{chr(10).join(f"- [ ] {s}" for s in executed_specs)}

## Prompts Executados
{chr(10).join(f"- [ ] {p}" for p in executed_prompts)}

## Funcionalidades com Play Mode Manual Obrigatório
- [ ] SPEC_12: Player combat (Q, E, Space, R/T/Y/G)
- [ ] SPEC_013: Enemy AI and combat roles
- [ ] SPEC_014: Skill trees and progression
- [ ] SPEC_015: Cave generation and stable run
- [ ] SPEC_016: Loot and crafting economy
- [ ] SPEC_017: Full integration and polish

## Passos Manuais Consolidados
1. Abra a cena principal no editor
2. Entre em Play Mode
3. Teste cada funcionalidade descrita
4. Verifique visualmente se a integração está correta
5. Procure por regressions em sistemas anteriores

## Riscos Residuais
- Validações automáticas podem não cobrir 100% dos cenários
- Play Mode manual é essencial para UX/feel validation
- Performance não é validada automaticamente

## Ordem Sugerida de Validação Manual
1. Testar cada SPEC completada individualmente
2. Testar integrações entre SPECs
3. Testar casos edge
4. Verificar performance
5. Confirmar balance

## Próximos Passos
- [ ] Executar todos os testes manuais
- [ ] Registrar issues encontradas
- [ ] Abrir PRs para fixes se necessário
- [ ] Mergear branches para prod quando pronto
"""

    output_path.write_text(checklist, encoding="utf-8")
    print(f"Generated: {output_path}")
    return output_path
