# SPEC - Unity compile validation protocol and scripts

> Spec ID: spec_unity_compile_validation_protocol_and_scripts
> Status: Implementado parcial
> Ordem de execucao: 01
> Depende de: spec_docs_001_single_source_specs_refinements_reconciliation_parcial
> Bloqueia: execucao segura das specs runtime 02-17
> Tipo: Tooling / Operacao / Validacao
> Fonte: docs/specs/a_implementar/spec_scene_unity_validation_missing_scripts_prefabs.md; docs/refinements/a_implementar/pre_refinamentos/refinamento_init_scene_unity_validation_missing_scripts_prefabs.md
> Escopo: validacao Unity minima por PowerShell e regra permanente para agentes.
> Fora de escopo: MissingScriptScanner C#, SceneReferenceValidator C#, DataIdValidator C#, Play Mode automatizado completo e alteracoes de gameplay runtime.

---

# /speckit.specify

## Contexto

O projeto ja possui validacao documental por `tools/docs/validate_docs.ps1`. Esta spec adiciona a camada minima reutilizavel para validar compilacao Unity em batchmode antes de avancar para specs runtime.

## Problema

Sem validacao Unity obrigatoria, agentes podem alterar C#, assets, cenas, prefabs, Packages ou ProjectSettings e concluir specs sobre uma base que nao compila no Unity.

## Objetivo

Criar:

- `tools/unity/RunUnityCompileValidation.ps1`
- `tools/unity/ScanUnityLogs.ps1`

E tornar a execucao desses scripts regra operacional para tarefas runtime/Unity.

## User stories / engineering stories

- Como agente, quero um comando padrao de compile validation para nao depender de prompt manual.
- Como maintainer, quero erro claro quando Unity nao existir, quando houver timeout ou quando o processo retornar exit code diferente de zero.
- Como maintainer, quero scanner de log que falhe em erros criticos de compilacao e reporte warnings sem bloquear o MVP.

## Criterios de aceite

- `RunUnityCompileValidation.ps1` usa por padrao `C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe`.
- O log padrao e `Logs/unity-compile-validation.log`.
- Unity ausente, projeto ausente, timeout e exit code nao zero retornam falha clara.
- `ScanUnityLogs.ps1` detecta `error CS`, compilation failures, exceptions criticas e referencias de assembly/tipo ausentes.
- AGENTS, CLAUDE e Agent Execution Protocol mandam rodar a validacao em tarefas runtime/Unity.

---

# /speckit.plan

## Arquitetura

Tooling PowerShell em `tools/unity/`, sem dependencias de codigo C# e sem alterar assets Unity.

## Sistemas afetados

- Tooling local.
- Protocolo operacional de agentes.
- Registries e maps de specs/refinements.

## Fluxos

1. Agente altera runtime/Unity.
2. Agente roda `tools/docs/validate_docs.ps1`.
3. Agente roda `tools/unity/RunUnityCompileValidation.ps1`.
4. Agente roda `tools/unity/ScanUnityLogs.ps1`.
5. Se falhar, corrige causa raiz e repete validacao.
6. Se Unity nao puder rodar, registra motivo, comando tentado e risco residual.

## Dados / DTOs / IDs

Nao aplicavel.

## Eventos

Nao aplicavel.

## Save/load

Nao aplicavel.

## UI, se aplicavel

Nao aplicavel.

## Riscos de regressao

O script cobre compile/log scan minimo. Missing Script, SceneReferenceValidator e DataIdValidator ainda ficam pendentes como validadores futuros.

---

# /speckit.tasks

## Tasks

- [x] Criar `tools/unity/RunUnityCompileValidation.ps1`.
- [x] Criar `tools/unity/ScanUnityLogs.ps1`.
- [x] Atualizar `AGENTS.md`, `CLAUDE.md` e `docs/operations/AGENT_EXECUTION_PROTOCOL.md`.
- [x] Reclassificar a spec 01 como implementada parcial.
- [x] Reclassificar o refinement 01 como implementado parcial.
- [x] Atualizar registries, maps, status e log.

## Arquivos permitidos

- `tools/unity/**`
- `*.md`
- `docs/**/*.md`

## Arquivos proibidos

- `Assets/_Game/Scripts/**/*.cs`
- `Assets/**/*.unity`
- `Assets/**/*.prefab`
- `Assets/**/*.asset`
- `Packages/**`
- `ProjectSettings/**`
- `docs_old/**`

## Definition of Done

- Unity compile validation minima criada.
- Agentes instruidos a rodar validacao automaticamente ao final de specs runtime.
- Scanners avancados ficam para futuro.
- Nenhum runtime gameplay alterado.

## Validacao

- `.\tools\docs\validate_docs.ps1`
- `.\tools\unity\RunUnityCompileValidation.ps1 -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"`
- `.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"`

## Resultado de validacao nesta entrega

- `tools/docs/validate_docs.ps1`: falhou em parse antes de validar (`Future spec missing $marker:` em `tools/docs/validate_docs.ps1`).
- `RunUnityCompileValidation.ps1`: executado, mas Unity batchmode foi bloqueado por outra instancia do Unity aberta no mesmo projeto.
- `ScanUnityLogs.ps1`: executado e falhou corretamente ao detectar `Application will terminate with return code 1`.

Unity validation: NOT RUN
Reason: Unity batchmode bloqueado por outra instancia do Unity aberta no mesmo projeto.
Command attempted: `.\tools\unity\RunUnityCompileValidation.ps1 -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"`
Residual risk: Unity compile not validated locally
