# SPEC_ORQUESTRADOR_CONSOLIDACAO_V2_CLEANUP_E_GATES.md

> **Spec ID:** `spec_orquestrador_consolidacao_v2_cleanup_e_gates`  
> **Status:** A implementar  
> **Branch alvo:** `dev`  
> **Tipo:** Tooling / Orquestração local / Limpeza técnica / Correção de gates  
> **Pasta canônica:** `orquestrador/`  
> **Objetivo:** Consolidar o orquestrador em um único sistema funcional, remover ou transformar duplicatas legadas em wrappers finos, corrigir gates de completude, corrigir logging/validação/fallback Claude→Codex e garantir que modo `spec` e modo `prompt` executem pastas inteiras sem avançar em caso de falha/parcial.  
> **Fora de escopo:** implementar gameplay, corrigir SPEC 10/12/13/14/15/16, alterar runtime Unity em `Assets/_Game/**`, rodar specs/prompts reais após a limpeza, criar LangGraph/Temporal, execução paralela.

---

## 1. Diagnóstico inicial conhecido

A pasta `orquestrador/` já existe e contém mistura de sistema novo e sistema antigo.

Arquivos vistos:

```text
orquestrador/
  logs/
  __init__.py
  .env.example
  agent.py
  ARCHITECTURE.md
  claude_api_integration.py
  CLEANUP_STATUS.md
  CONFIG_GUIDE.md
  config.json
  CONSOLIDATION_GUIDE.md
  IMPLEMENTATION_SUMMARY.md
  logger.py
  orquestrador_config.json
  queue.py
  QUICKSTART.md
  README_NEW.md
  README.md
  requirements.txt
  run_orchestrator.py
  run_orquestrador.py
  setup.py
  spec_operations.py
  spec_orchestrator.py
  test_imports.py
  validation.py
```

Problemas concretos identificados:

1. `run_orquestrador.py` é o runner novo, mas ainda convive com `run_orchestrator.py`, que é sistema legado completo.
2. `run_orchestrator.py` usa `SpecOrchestrator` e `ClaudeSpecExecutor`, ou seja, lógica paralela antiga usando API, não CLI.
3. `spec_orchestrator.py` é legado completo, com mapa hardcoded de specs antigo/incorreto.
4. Há pelo menos duas configs: `config.json` e `orquestrador_config.json`.
5. Há múltiplos documentos de README/guide/summary que podem apontar para comandos diferentes.
6. `queue.py` tem nome arriscado porque colide com o módulo padrão `queue` do Python.
7. `run_orquestrador.py` considera sucesso apenas com `validations.all_pass` + `AGENT_RESULT=SUCCESS`; falta exigir `SPEC_STATUS=COMPLETE`.
8. Em modo prompt, o script pode arquivar prompt mesmo quando não inferiu spec alvo ou quando `close_spec()` falhou.
9. `update_implementation_registries()` tenta extrair número por regex `spec_(\d+)_`, mas os arquivos atuais são do tipo `spec_player_combat_weapons...`, então registry pode não atualizar.
10. `parse_spec_execution_order()` procura `SPEC_XX`, mas `SPEC_EXECUTION_ORDER.md` usa linhas com coluna `Ordem` numérica e links para arquivos; isso quebra ordenação por spec.
11. `infer_target_spec()` não encontra specs reais quando o prompt é `SPEC_12...`, porque procura `spec_12_*.md` ou `*SPEC_12*.md`, mas o arquivo real é `spec_player_combat_weapons_spells_skill_actions_runtime.md`.
12. `run_unity_scan()` chama `ScanUnityLogs.ps1` sem passar `-LogFile`, embora o uso padrão do projeto exija `-LogFile`.
13. `run_repo_checks_spec()` espera chaves `SPEC_012`, mas `run_orquestrador.py` passa inteiro `12` ou `None`; checks específicos não rodam.
14. `agent.py` cria `temp_prompt_claude.txt`, mas não usa o arquivo.
15. `agent.py` passa prompt inteiro como argumento CLI; isso pode estourar limite de linha de comando no Windows para prompts longos.
16. `subprocess.TimeoutExpired` não é efetivo nos agentes, porque `Popen` + `stream_process()` não aplica timeout real.
17. Mensagens de instalação estão incorretas: Claude Code não é `pip install anthropic-cli`; Codex CLI não é `pip install codex-cli`.
18. `run_codex()` e `run_claude()` têm campos `primary_agent/fallback_agent` hardcoded de forma inconsistente.
19. `close_spec()` move arquivo antes de garantir atualização documental consistente; a operação não é transacional.
20. Não deve haver alterações em gameplay nesta spec.

---

## 2. Resultado final esperado

Ao final deve existir um único comando canônico:

```powershell
python .\orquestrador\run_orquestrador.py
```

E os comandos abaixo devem funcionar:

```powershell
python .\orquestrador\run_orquestrador.py `
  --mode spec `
  --input-dir ".\docs\specs\a_implementar" `
  --dry-run

python .\orquestrador\run_orquestrador.py `
  --mode prompt `
  --input-dir ".\docs\agent_prompts\a_executar" `
  --dry-run

python .\orquestrador\run_orquestrador.py `
  --mode spec `
  --input-file ".\docs\specs\a_implementar\spec_player_combat_weapons_spells_skill_actions_runtime.md" `
  --stop-after-one `
  --dry-run

python .\orquestrador\run_orquestrador.py `
  --mode prompt `
  --input-file ".\docs\agent_prompts\a_executar\SPEC_12_player-combat-spells-skill-actions_PROMPT.md" `
  --stop-after-one `
  --dry-run
```

---

## 3. Regra de completude obrigatória

O orquestrador só pode avançar ou fechar item se:

```text
AGENT_RESULT = SUCCESS
SPEC_STATUS = COMPLETE
docs_validation = PASS
unity_compile = PASS
repo_checks = PASS
target_spec_path existe quando mode=prompt
close_spec retornou sucesso quando fechamento de spec for necessário
```

Se qualquer condição falhar:

```text
- não mover spec para implementados;
- não arquivar prompt como executado;
- não avançar para o próximo item se --stop-on-failure estiver ativo;
- gerar summary.md e summary.json;
- registrar falha objetiva.
```

---

## 4. Proibição de alteração de gameplay

Esta spec não deve alterar:

```text
Assets/_Game/**
Assets/**.unity
Assets/**.prefab
Assets/**.asset
```

Se algum desses arquivos mudar, a execução deve ser considerada incorreta, salvo ajuste explícito e justificado em relatório. O foco é exclusivamente `orquestrador/`, docs de uso e `.gitignore`.

---

## 5. Arquivos obrigatórios para ler

Ler antes de alterar:

```text
AGENTS.md
CLAUDE.md
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/audits/SPECS_01_16_COMPLETENESS_AUDIT_20260524.md
memory/MEMORY.md
memory/project_skills_available.md
orquestrador/run_orquestrador.py
orquestrador/run_orchestrator.py
orquestrador/spec_orchestrator.py
orquestrador/agent.py
orquestrador/logger.py
orquestrador/queue.py
orquestrador/spec_operations.py
orquestrador/validation.py
orquestrador/orquestrador_config.json
orquestrador/config.json
orquestrador/README.md
orquestrador/README_NEW.md
orquestrador/CONFIG_GUIDE.md
orquestrador/QUICKSTART.md
```

Se algum arquivo não existir, registrar no audit e continuar.

---

## 6. Criar audit da limpeza

Criar:

```text
docs/audits/ORQUESTRADOR_CONSOLIDACAO_AUDIT_20260524.md
```

Conteúdo obrigatório:

```text
- arquivos encontrados em orquestrador/;
- arquivos legados;
- arquivos canônicos;
- duplicatas removidas;
- wrappers mantidos;
- configs consolidadas;
- templates/docs consolidados;
- riscos encontrados;
- arquivos de runtime alterados: deve ser zero;
- validações rodadas;
- resultado final.
```

---

## 7. Consolidar runners

### 7.1 Runner canônico

Manter como runner real:

```text
orquestrador/run_orquestrador.py
```

### 7.2 Runner legado

`orquestrador/run_orchestrator.py` não deve conter lógica própria.

Opção preferida: transformar em wrapper fino deprecated:

```python
#!/usr/bin/env python3
# Deprecated wrapper. Use:
# python .\orquestrador\run_orquestrador.py
from pathlib import Path
import runpy

target = Path(__file__).resolve().parent / "run_orquestrador.py"
runpy.run_path(str(target), run_name="__main__")
```

Ou remover se nenhuma documentação apontar para ele.

### 7.3 Remover/arquivar `spec_orchestrator.py`

`orquestrador/spec_orchestrator.py` é legado v1 e contém mapa hardcoded incorreto. Ele não deve continuar como implementação ativa.

Opções:

1. Remover se `run_orchestrator.py` virar wrapper.
2. Se quiser preservar histórico, mover para:

```text
orquestrador/legacy/spec_orchestrator_v1.py
```

e deixar claro que não é importado por nenhum comando canônico.

Preferência: remover para evitar sujeira.

### 7.4 `claude_api_integration.py`

Se esse arquivo implementa Anthropic API direta, ele não deve fazer parte do fluxo novo CLI.

Opções:

1. remover;
2. mover para `orquestrador/legacy/claude_api_integration_v1.py`;
3. manter somente se houver uso real documentado, mas fora do runner canônico.

Preferência: remover/mover para legacy.

---

## 8. Consolidar configs

Manter somente:

```text
orquestrador/orquestrador_config.json
```

Se `orquestrador/config.json` tiver campos úteis, migrar para `orquestrador_config.json`.

Depois:

- remover `config.json`, ou
- transformar em arquivo deprecated mínimo que aponta para `orquestrador_config.json`.

Preferência: remover `config.json`.

---

## 9. Renomear `queue.py`

`orquestrador/queue.py` deve ser renomeado para:

```text
orquestrador/execution_queue.py
```

Motivo: evitar colisão com módulo padrão Python `queue`.

Atualizar imports em `run_orquestrador.py` e onde mais for necessário.

---

## 10. Corrigir parser de ordem de specs

`parse_spec_execution_order()` deve parsear o formato real de `docs/specs/SPEC_EXECUTION_ORDER.md`.

Formato real esperado:

```md
| Ordem | Spec | Status | Depende de | Bloqueia | Risco se antecipar |
|---|---|---|---|---|---|
| 12 | [spec_player_combat_weapons_spells_skill_actions_runtime](a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md) | A implementar | ...
```

Criar retorno no mínimo:

```python
{
  12: {
    "order": 12,
    "filename": "spec_player_combat_weapons_spells_skill_actions_runtime.md",
    "path": "docs/specs/a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md",
    "status": "A implementar"
  }
}
```

Regras:

- ignorar linhas de header;
- parsear coluna 1 como número;
- parsear link markdown da coluna Spec;
- preservar status;
- usar ordem numérica como principal.

---

## 11. Corrigir fila de specs

`build_spec_queue()` deve:

1. listar `*.md` em `--input-dir`;
2. remover specs antigas explicitamente marcadas como não executáveis, se houver;
3. ordenar pelo mapa de `SPEC_EXECUTION_ORDER.md`;
4. se arquivo não estiver no execution order, colocar no final e registrar warning;
5. não selecionar SPEC 17 se 12–16 ainda estiverem em `a_implementar` ou status não completo.

Dry run deve imprimir:

```text
Mode: spec
Input dir: ...
Queue:
1. [06] spec_economy...
...
Warnings:
- X não encontrado em SPEC_EXECUTION_ORDER.md
```

---

## 12. Corrigir fila de prompts

`build_prompt_queue()` deve:

1. listar `*.md`;
2. ignorar:
   - `00_INDEX_ORDEM_USO.md` como item executável;
   - `00_PROMPT_MESTRE*`;
   - `00B_PROMPT_AUXILIAR*`;
   - `99_TEMPLATE*`;
3. usar `00_INDEX_ORDEM_USO.md` só para ordenação;
4. se o índice não tiver todos os arquivos, ordenar os faltantes por número `SPEC_XX`;
5. garantir `SPEC_17A` antes de `SPEC_17` quando ambos existirem, salvo índice diferente.

---

## 13. Corrigir inferência prompt -> spec

`infer_target_spec()` deve funcionar assim:

1. procurar caminho explícito no prompt:
   - `docs/specs/a_implementar/<arquivo>.md`;
   - `docs/specs/implementados/<arquivo>.md`;
2. extrair `SPEC_XX` do nome do prompt;
3. consultar `SPEC_EXECUTION_ORDER.md` para mapear número -> path real;
4. confirmar que o arquivo existe;
5. se não existir em `a_implementar` mas existir em `implementados`, registrar que já está implementada e não fechar de novo;
6. se não conseguir inferir, permitir execução apenas como prompt solto, mas:
   - não fechar spec;
   - não arquivar como sucesso de spec;
   - summary deve dizer `target_spec_path = null`.

---

## 14. Corrigir sucesso e fechamento em `run_orquestrador.py`

Substituir regra atual por:

```python
agent_success = agent_output.get("agent_result") == "SUCCESS"
spec_complete = agent_output.get("spec_status") == "COMPLETE"
validations_pass = validations.all_pass

success = agent_success and spec_complete and validations_pass
```

Em modo prompt:

```python
if args.mode == "prompt":
    success = success and target_spec is not None
```

Fechamento:

```python
closed_spec = False

if success and target_spec:
    closed_spec = close_spec(target_spec, config, repo_root)

if args.mode == "prompt":
    if success and closed_spec:
        moved_prompt = archive_prompt(...)
    elif not success and args.archive_on_failure:
        moved_prompt = archive_prompt(...bloqueados...)
```

Não arquivar prompt como executado se `close_spec()` falhou.

---

## 15. Corrigir fechamento de spec

`close_spec()` deve ser mais seguro.

Regras:

1. não mover se `spec_path` não existir;
2. não mover se já estiver em `implementados`;
3. copiar para `implementados`;
4. atualizar registries/status/log;
5. se update documental passar, remover original;
6. se update documental falhar, manter original e remover cópia;
7. retornar `True` somente se tudo passar.

`update_implementation_registries()` deve conseguir atualizar specs sem depender de regex `spec_(\d+)_`.

Usar:

- mapa de `SPEC_EXECUTION_ORDER.md`;
- ou número recebido explicitamente;
- ou inferir por match de filename.

---

## 16. Corrigir validação Unity scan

`run_unity_scan()` deve receber o mesmo log gerado por `RunUnityCompileValidation.ps1`.

Comando correto:

```powershell
.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```

No orquestrador, salvar também cópia no log do item:

```text
unity_compile_validation.log
unity_scan.log
```

---

## 17. Corrigir spec checks

Em `run_orquestrador.py`, converter número para label:

```python
spec_num = extract_spec_number(...)
spec_id = f"SPEC_{spec_num:03d}" if spec_num else None
```

Ou padronizar para `SPEC_12`, mas precisa bater com `validation.py`.

Recomendação: aceitar os dois formatos em `run_repo_checks_spec()`:

```python
normalized = normalize_spec_id(spec_id)
```

Suportar:

```text
12
"12"
"SPEC_12"
"SPEC_012"
```

---

## 18. Corrigir agentes

### 18.1 Timeout real

`logger.stream_process()` deve aceitar timeout.

Implementar loop com timeout ou monitorar tempo enquanto lê stream.

Se timeout:

- matar processo;
- salvar logs parciais;
- retornar exit code 124;
- fallback se aplicável.

### 18.2 Prompt longo

Evitar prompt gigante como argumento se possível.

Pelo menos:

- salvar prompt em `rendered_prompt.md`;
- registrar tamanho do prompt;
- se `len(prompt) > 30000`, retornar erro claro antes de chamar agente no Windows ou usar estratégia segura documentada;
- não gerar erro silencioso.

### 18.3 Mensagens de instalação

Corrigir:

```text
Claude CLI not found
```

para:

```text
Claude Code CLI not found in PATH. Verify with:
claude --version
claude auth status --text
```

Corrigir Codex:

```text
Codex CLI not found in PATH. Install with:
npm install -g @openai/codex
codex --version
```

### 18.4 Primary/fallback metadata

`run_claude()` e `run_codex()` não devem hardcodar `primary_agent="claude"` quando o primário real é outro.

Opção simples:

- `run_claude()` retorna apenas `agent_used`;
- `run_with_fallback()` preenche metadata final.

Ou passar `primary_agent` e `fallback_agent` como parâmetros.

---

## 19. Corrigir docs

Consolidar documentação.

Manter:

```text
orquestrador/README.md
```

Remover ou arquivar:

```text
README_NEW.md
CONFIG_GUIDE.md
CONSOLIDATION_GUIDE.md
IMPLEMENTATION_SUMMARY.md
CLEANUP_STATUS.md
ARCHITECTURE.md
```

Se o conteúdo for útil, migrar para `README.md`.

`README.md` deve conter:

- comando canônico;
- diferença entre `--mode spec` e `--mode prompt`;
- uso de `--input-dir`;
- uso de `--input-file`;
- dry run;
- logs;
- fallback Claude -> Codex;
- critérios de fechamento;
- regra de que prompts não substituem specs;
- troubleshooting básico.

Atualizar também:

```text
AGENTS.md
CLAUDE.md
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
```

Apenas para apontar para o comando canônico, sem duplicar documentação longa.

---

## 20. Logs e gitignore

Garantir `.gitignore`:

```gitignore
# Orquestrador runtime outputs
orquestrador/logs/**
!orquestrador/logs/.gitkeep
orquestrador/state/**
!orquestrador/state/.gitkeep
orquestrador/orchestrator.log
orquestrador/*_prompt_*.txt
orquestrador/temp_*.txt
```

Se logs reais já estiverem versionados, remover do git mantendo `.gitkeep`.

---

## 21. Validações obrigatórias

Executar:

```powershell
python .\orquestrador\run_orquestrador.py --mode spec --input-dir ".\docs\specs\a_implementar" --dry-run

python .\orquestrador\run_orquestrador.py --mode prompt --input-dir ".\docs\agent_prompts\a_executar" --dry-run
```

Se `docs/agent_prompts/a_executar` não existir:

- criar estrutura vazia;
- dry run deve retornar fila vazia ou erro amigável, nunca stack trace.

Executar docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Executar Unity compile:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"

.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```

Executar proteção de runtime:

```powershell
git diff --name-only | Select-String "Assets/_Game|Assets/.*\.unity|Assets/.*\.prefab|Assets/.*\.asset"
```

Esse comando não deve retornar arquivos.

---

## 22. Critérios de aceite

Esta spec só estará completa quando:

- `orquestrador/run_orquestrador.py` for o único runner real;
- `run_orchestrator.py` for removido ou wrapper fino;
- `spec_orchestrator.py` legado for removido ou movido para `legacy` sem import ativo;
- `claude_api_integration.py` API direto for removido/movido para `legacy` se não for usado;
- existir uma única config canônica `orquestrador/orquestrador_config.json`;
- `queue.py` for renomeado para `execution_queue.py`;
- imports estiverem corrigidos;
- dry run de `--mode spec --input-dir` funcionar;
- dry run de `--mode prompt --input-dir` funcionar ou falhar amigavelmente se pasta não existir;
- `SPEC_EXECUTION_ORDER.md` for parseado corretamente;
- prompt -> spec inferir SPEC_12 para `spec_player_combat_weapons_spells_skill_actions_runtime.md`;
- sucesso exigir `AGENT_RESULT=SUCCESS` e `SPEC_STATUS=COMPLETE`;
- prompt só for arquivado como executado se spec alvo fechar;
- `ScanUnityLogs.ps1` receber `-LogFile`;
- checks específicos por spec aceitarem `12`, `SPEC_12` e `SPEC_012`;
- timeout real existir nos subprocessos;
- logs reais não estiverem versionados;
- docs validation passar;
- Unity compile validation passar;
- nenhum runtime do jogo for alterado;
- audit de consolidação for criado.

---

## 23. Commit esperado

Commit principal:

```text
chore: consolidar orquestrador e corrigir gates de execução
```

Se houver documentação separada:

```text
docs: atualizar runbook do orquestrador
```

---

## 24. Resposta final esperada

Responder com:

```text
- branch usada;
- runner canônico;
- duplicatas encontradas;
- duplicatas removidas;
- wrappers mantidos;
- config canônica;
- comandos dry run executados;
- resultado dry run spec;
- resultado dry run prompt;
- resultado docs validation;
- resultado Unity compile validation;
- confirmação de que Assets/_Game não foi alterado;
- arquivos alterados;
- commit criado;
- pendências reais.
```
