# SPEC_AGENTIC_SPEC_ORCHESTRATOR_PYTHON_CLI.md

> **Spec ID:** `spec_agentic_spec_orchestrator_python_cli`  
> **Status:** A implementar  
> **Branch alvo:** `dev`  
> **Tipo:** Tooling / Orquestração local / Agent workflow  
> **Objetivo:** Criar um orquestrador local simples em Python para executar specs uma por uma, usando Claude Code CLI como agente primário quando disponível, Codex CLI como fallback, validação documental, validação Unity automática, repair loop, logs, relatório e fechamento controlado da spec.  
> **Fora de escopo:** LangGraph, Temporal, execução paralela, CI/CD remoto, automação pela extensão VS Code, validação visual humana automatizada, Play Mode completo sem testes automatizados.

---

## 1. Contexto

O projeto Cindar's Hope está sendo implementado por specs localizadas em:

```text
docs/specs/a_implementar/
```

O problema atual é que specs grandes demais fazem os agentes entregarem apenas fundação parcial, atualizarem documentação cedo demais ou confundirem "compilou" com "spec concluída".

Esta spec cria um orquestrador determinístico que:

1. seleciona dinamicamente a próxima spec;
2. monta o prompt;
3. chama Claude Code;
4. se Claude falhar por crédito/limite/timeout, chama Codex;
5. valida documentação;
6. valida compile Unity;
7. se falhar, manda repair;
8. fecha a spec somente se tudo passar;
9. move a spec para `implementados`;
10. gera relatório;
11. commita;
12. segue para a próxima spec.

---

## 2. Decisão técnica

Implementar o orquestrador em **Python 3.12+**.

Motivo:

- melhor controle de subprocessos;
- melhor parse de stdout/stderr;
- JSON nativo;
- logs estruturados;
- timeouts simples;
- portabilidade melhor que PowerShell;
- mais fácil de manter repair loop, fallback e estado;
- pode chamar PowerShell/Unity/Git normalmente via `subprocess`.

PowerShell continua sendo usado apenas para scripts existentes, como:

```text
tools/docs/validate_docs.ps1
tools/unity/RunUnityCompileValidation.ps1
tools/unity/ScanUnityLogs.ps1
```

---

## 3. CLIs suportados

### 3.1 Claude Code CLI

Teste local:

```powershell
claude --version
claude doctor
cd "D:\Projetos\Jogos\Cindars_hope\cindars_hope"
claude -p "Responda apenas OK" --output-format text
```

Observação importante:

- Não usar `--cwd`.
- A instalação atual retornou `unknown option '--cwd'`.
- O orquestrador deve usar `subprocess.run(..., cwd=repo_root)`.

Exemplo Python:

```python
subprocess.run(
    ["claude", "-p", prompt, "--output-format", "text"],
    cwd=repo_root,
    capture_output=True,
    text=True,
    timeout=timeout_seconds
)
```

Se Claude retornar:

```text
Credit balance is too low
```

ou mensagem equivalente de limite/crédito, o orquestrador deve chamar Codex automaticamente.

### 3.2 Codex CLI

Pré-requisito:

```powershell
node --version
npm --version
```

Se `npm` não existir:

```powershell
winget install OpenJS.NodeJS.LTS
```

Fechar e abrir o terminal novamente.

Instalar Codex CLI:

```powershell
npm install -g @openai/codex
```

Validar:

```powershell
codex --version
codex exec "Responda apenas OK"
```

Modo Python:

```python
subprocess.run(
    ["codex", "exec", prompt],
    cwd=repo_root,
    capture_output=True,
    text=True,
    timeout=timeout_seconds
)
```

Sandbox padrão:

```text
workspace-write
```

Se a versão instalada suportar argumento de sandbox, usar:

```powershell
codex exec --sandbox workspace-write "prompt"
```

O script deve permitir configurar isso. Se o argumento não for suportado na versão local, registrar no log e rodar sem sandbox customizado.

### 3.3 Unity

Usar scripts existentes do projeto:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"

.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```

O orquestrador Python deve chamar esses scripts com:

```python
subprocess.run(
    ["powershell", "-ExecutionPolicy", "Bypass", "-File", script_path, ...],
    cwd=repo_root,
    capture_output=True,
    text=True
)
```

---

## 4. Estrutura de arquivos a criar

```text
tools/spec_orchestrator/
  run_spec_orchestrator.py
  orchestrator_config.json
  requirements.txt
  README.md
  templates/
    spec_execution_prompt.md
    spec_repair_prompt.md
    spec_closure_prompt.md
  state/
    orchestrator_state.json
  logs/
    .gitkeep
```

Não criar múltiplos scripts antes da necessidade. Começar com um script Python único e modularizado em funções.

---

## 5. Configuração

Criar:

```text
tools/spec_orchestrator/orchestrator_config.json
```

Conteúdo inicial:

```json
{
  "repo_root": ".",
  "specs_to_implement_dir": "docs/specs/a_implementar",
  "implemented_specs_dir": "docs/specs/implementados",
  "refinements_to_implement_dir": "docs/refinements/a_implementar/pre_refinamentos",
  "implemented_refinements_dir": "docs/refinements/implementados",
  "spec_execution_order_path": "docs/specs/SPEC_EXECUTION_ORDER.md",
  "implemented_registry_path": "docs/specs/SPEC_REGISTRY_IMPLEMENTED.md",
  "to_implement_registry_path": "docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md",
  "implementation_status_path": "docs/IMPLEMENTATION_STATUS.md",
  "project_log_path": "PROJECT_LOG.md",
  "audit_path": "docs/audits/SPECS_01_16_COMPLETENESS_AUDIT_20260524.md",
  "unity_editor_path": "C:\\Program Files\\Unity\\Hub\\Editor\\6000.4.7f1\\Editor\\Unity.exe",
  "primary_agent": "claude",
  "fallback_agent": "codex",
  "max_repair_attempts": 3,
  "agent_timeout_minutes": 90,
  "unity_timeout_minutes": 20,
  "codex_sandbox": "workspace-write",
  "require_unity_compile": true,
  "require_docs_validation": true,
  "auto_commit": true,
  "auto_move_spec_to_implemented": true,
  "stop_after_one_spec": false,
  "credit_error_patterns": [
    "Credit balance is too low",
    "credit balance is too low",
    "insufficient credits",
    "usage limit",
    "rate limit",
    "quota",
    "out of credits",
    "authentication failed",
    "not authenticated",
    "limite de uso",
    "sem créditos",
    "acabou os créditos"
  ]
}
```

---

## 6. CLI do orquestrador

Executar todas as specs elegíveis:

```powershell
python .\tools\spec_orchestrator\run_spec_orchestrator.py
```

Executar uma spec específica:

```powershell
python .\tools\spec_orchestrator\run_spec_orchestrator.py `
  --spec ".\docs\specs\a_implementar\spec_player_combat_weapons_spells_skill_actions_runtime.md" `
  --stop-after-one-spec
```

Dry run:

```powershell
python .\tools\spec_orchestrator\run_spec_orchestrator.py --dry-run
```

Forçar Codex como primário:

```powershell
python .\tools\spec_orchestrator\run_spec_orchestrator.py `
  --primary-agent codex `
  --fallback-agent claude
```

Desativar commit automático:

```powershell
python .\tools\spec_orchestrator\run_spec_orchestrator.py --no-auto-commit
```

---

## 7. Seleção dinâmica da próxima spec

O script deve:

1. listar `docs/specs/a_implementar/*.md`;
2. ler `docs/specs/SPEC_EXECUTION_ORDER.md`;
3. ordenar specs conforme a ordem oficial;
4. ignorar specs bloqueadas;
5. nunca selecionar SPEC 17 se 12–16 ainda estiverem pendentes;
6. se não conseguir mapear a ordem, usar fallback alfabético e registrar risco no relatório.

---

## 8. Ciclo principal

Para cada spec:

```text
1. Criar pasta de execução em tools/spec_orchestrator/logs/<timestamp>/<spec_id>/
2. Ler spec.
3. Encontrar refinement relacionado.
4. Montar prompt de execução.
5. Chamar agente primário.
6. Detectar falha/crédito/limite/timeout.
7. Se falhou, chamar fallback.
8. Rodar docs validation.
9. Rodar Unity compile validation.
10. Rodar repo checks.
11. Se falhar, montar repair prompt.
12. Repetir repair até max_repair_attempts.
13. Se tudo passar, fechar spec.
14. Mover spec para implementados.
15. Mover refinement se absorvido.
16. Atualizar registries/status/log/audit.
17. Commitar.
18. Gerar relatório.
19. Seguir para próxima spec.
```

---

## 9. Detecção de falha do agente

O script deve considerar falha quando:

- exit code != 0;
- timeout;
- stdout vazio;
- output contém padrão de crédito/limite;
- agent não retornou bloco final obrigatório;
- agent declarou `PARTIAL`, `BLOCKED` ou `FAILED`;
- agent pediu decisão humana que não é bloqueio real.

Padrões mínimos:

```text
Credit balance is too low
credit balance is too low
insufficient credits
usage limit
rate limit
quota
out of credits
authentication failed
not authenticated
context length
maximum context
I cannot continue
não consigo continuar
acabou os créditos
sem créditos
limite de uso
```

---

## 10. Bloco final obrigatório do agente

Todo prompt deve exigir:

```text
AGENT_RESULT: SUCCESS | PARTIAL | BLOCKED | FAILED
SPEC_STATUS: COMPLETE | PARTIAL | BLOCKED
CHANGED_FILES:
- path
VALIDATIONS:
- docs: PASS | FAIL | NOT_RUN
- unity_compile: PASS | FAIL | NOT_RUN
- repo_checks: PASS | FAIL | NOT_RUN
NEXT_ACTION:
- close_spec | repair | human_manual_validation | stop
```

Regra:

```text
Somente AGENT_RESULT=SUCCESS + SPEC_STATUS=COMPLETE + validações PASS permitem fechar spec.
```

---

## 11. Prompt template de execução

Criar:

```text
tools/spec_orchestrator/templates/spec_execution_prompt.md
```

Conteúdo:

```md
Você está no projeto Cindar's Hope.

Execute UMA ÚNICA SPEC até completion real.

SPEC ALVO:
{{SPEC_PATH}}

REFINEMENT RELACIONADO:
{{REFINEMENT_PATH}}

ARQUIVOS DE CONTEXTO OBRIGATÓRIOS:
- AGENTS.md
- CLAUDE.md
- memory/MEMORY.md
- memory/project_skills_available.md
- docs/specs/SPEC_EXECUTION_ORDER.md
- docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
- docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
- docs/IMPLEMENTATION_STATUS.md
- PROJECT_LOG.md

REGRAS:
- Não avance para outra spec.
- Não implemente feature fora do escopo.
- Não marque parcial como completo.
- Não remova spec de a_implementar se não cumprir critérios.
- Não altere docs_old/**.
- Não use GameObject.Find() ou FindObjectOfType().
- Não serialize Unity refs em save.
- Não use Dictionary em DTO serializado por JsonUtility.
- Se precisar criar assets Unity, preferir Editor script idempotente.
- Se Unity validation não rodar por ambiente, registrar NOT_RUN com motivo real.

TAREFA:
1. Leia todos os arquivos obrigatórios.
2. Leia a spec alvo inteira.
3. Leia o refinement relacionado, se existir.
4. Audite rapidamente o código existente relacionado.
5. Implemente o escopo da spec.
6. Atualize audit/status/log/registries conforme necessário.
7. Rode validações.
8. Corrija erros até compilar.
9. Termine com o bloco AGENT_RESULT obrigatório.

BLOCO FINAL OBRIGATÓRIO:
AGENT_RESULT: SUCCESS | PARTIAL | BLOCKED | FAILED
SPEC_STATUS: COMPLETE | PARTIAL | BLOCKED
CHANGED_FILES:
- path
VALIDATIONS:
- docs: PASS | FAIL | NOT_RUN
- unity_compile: PASS | FAIL | NOT_RUN
- repo_checks: PASS | FAIL | NOT_RUN
NEXT_ACTION:
- close_spec | repair | human_manual_validation | stop
```

---

## 12. Validações automáticas

### 12.1 Docs

```powershell
.\tools\docs\validate_docs.ps1
```

### 12.2 Unity

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "<unity_editor_path>" `
  -ProjectPath "." `
  -LogFile "<log_path>"

.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile "<log_path>"
```

### 12.3 Repo checks globais

```powershell
Select-String -Path "Assets/_Game/Scripts/**/*.cs" -Pattern "GameObject.Find|FindObjectOfType" -Recurse
Select-String -Path "Assets/_Game/Scripts/**/*.cs" -Pattern "QuestManager|QuestDataSO|QuestStartedEvent|QuestCompletedEvent" -Recurse
Select-String -Path "Assets/_Game/Scripts/**/*.cs" -Pattern "Dictionary<string, DurabilityEntry>" -Recurse
```

Adicionar checks por spec:

- SPEC 12: `SkillActionSO|ActiveSkillSlot|ManaManager|ArcaneBolt`
- SPEC 16: `SkillTreeManager|SkillTreePanel|SkillNodeDataSO`

---

## 13. Fechamento automático da spec

Só fechar se:

```text
AGENT_RESULT = SUCCESS
SPEC_STATUS = COMPLETE
docs validation = PASS
unity compile = PASS
repo checks = PASS
```

Ações:

1. criar/copiar spec para `docs/specs/implementados/`;
2. remover spec original de `docs/specs/a_implementar/`;
3. mover refinement se existir e estiver absorvido;
4. atualizar `SPEC_EXECUTION_ORDER.md`;
5. atualizar `SPEC_REGISTRY_IMPLEMENTED.md`;
6. atualizar `SPEC_REGISTRY_TO_IMPLEMENT.md`;
7. atualizar `docs/IMPLEMENTATION_STATUS.md`;
8. atualizar `PROJECT_LOG.md`;
9. atualizar audit;
10. commitar.

Se qualquer validação falhar:
- não mover spec;
- gerar relatório `PARTIAL` ou `FAILED`;
- manter spec em `a_implementar`.

---

## 14. Relatório por spec

Criar:

```text
tools/spec_orchestrator/logs/<timestamp>/<spec_id>/summary.md
```

Formato:

```md
# Spec Execution Report

## Spec
- Path:
- Status:
- Agent primary:
- Agent fallback:
- Started at:
- Finished at:

## Result
- AGENT_RESULT:
- SPEC_STATUS:
- Closed spec: yes/no

## Changed files
- path

## Validations
- docs:
- unity_compile:
- repo_checks:

## Agent fallback
- Used fallback: yes/no
- Reason:

## Errors
- list

## Residual risks
- list

## Next action
- next spec path or blocked reason
```

---

## 15. Git

Commit por spec fechada:

```powershell
git add .
git commit -m "feat: implementar <spec_id>"
```

Commit de falha parcial só se autorizado por parâmetro:

```powershell
--commit-partial
```

---

## 16. Critérios de aceite

Esta spec estará completa quando:

- `tools/spec_orchestrator/run_spec_orchestrator.py` existir;
- config JSON existir;
- templates existirem;
- dry run funcionar;
- seleção de próxima spec funcionar;
- execução de spec específica funcionar;
- Claude for chamado via subprocess com `cwd=repo_root`, sem `--cwd`;
- falha `Credit balance is too low` disparar fallback;
- Codex for chamado via subprocess quando instalado;
- docs validator for chamado;
- Unity validation for chamada;
- repo checks forem executados;
- relatório `summary.md` for gerado;
- spec não for movida se validação falhar;
- spec for movida apenas quando validações passam.

---

## 17. Runbook local

### 17.1 Validar Python

```powershell
python --version
```

Se não houver Python 3.12+:

```powershell
winget install Python.Python.3.12
```

### 17.2 Validar Claude

```powershell
claude --version
claude doctor
cd "D:\Projetos\Jogos\Cindars_hope\cindars_hope"
claude -p "Responda apenas OK" --output-format text
```

Se retornar `Credit balance is too low`, Claude CLI está instalado, mas indisponível por crédito. O orquestrador deve usar fallback Codex.

### 17.3 Instalar Codex

```powershell
winget install OpenJS.NodeJS.LTS
```

Fechar e abrir terminal.

```powershell
node --version
npm --version
npm install -g @openai/codex
codex --version
codex exec "Responda apenas OK"
```

### 17.4 Validar Unity

```powershell
Test-Path "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"

.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"

.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```
