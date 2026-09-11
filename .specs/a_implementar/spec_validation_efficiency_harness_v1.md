# SPEC — Validação por escopo e runners com evidência confiável

> **Spec ID:** `spec_validation_efficiency_harness_v1`
> **Status:** CODE_COMPLETE — contratos de tooling PASS; integração real coordenada separadamente
> **Wave:** SOLID_AI — continuidade de qualidade e organização
> **Priority:** P1
> **Type:** Tooling / Governance / Validation
> **Domain:** Core
> **Parallelizable:** CONDITIONAL
> **Parallel group:** ValidationHarness
> **Can run with:** refatoração e auditoria de Assets por outros owners
> **Must not run with:** edição concorrente dos scripts/rules desta spec
> **Repo lock scope:** arquivos de §18; execução Unity/build real coordenada pelo orquestrador
> **Depends on:** docs/validation/TEST_VALIDATION_EFFICIENCY_REVIEW.md
> **Blocks:** redução confiável de execuções repetidas de validação
> **Scope:** matriz canônica, runners Unity/.NET e validação documental com contratos explícitos
> **Out of scope:** gameplay, assets, testes C#, farm, alteração de baseline para obter PASS
> **Validation level alvo:** gates contratuais de tooling; resultado global separado
> **Executor:** Claude ou Codex
> **Ordem de execucao:** contrato, runners/gates, instruções, contratos, evidência
> **Depende de:** revisão de eficiência já entregue nesta sessão
> **Bloqueia:** claims de reuso de evidência sem inputs verificáveis

required_adrs: []
required_game_rules: []

# /speckit.specify

## 5. Contexto

O humano autorizou executar as recomendações da revisão e continuar organização e
refatoração. Esta spec registra a aprovação de manutenção por sessão; não cria feature
de gameplay nem exige nova confirmação. O orquestrador coordena a única rodada real.

## 6. Problema

Builds são exigidos no worker, por fase e repetidos pelo revisor. O strict global mistura
gates não aplicáveis à mudança; docs confunde variáveis PowerShell com placeholders.
O runner EditMode aceita XML antigo/zero casos, não tem timeout e usa versão fixa.

## 7. Objetivo

Um gate aplicável pode fornecer evidência reutilizável sobre os mesmos inputs; o modo
global continua honesto. Os runners recusam resultado incompleto e preservam diagnóstico.

## 8. Fontes lidas

AGENTS.md; CURRENT_STATE; skills spec-authoring/harness-authoring; template profundo;
TEST_VALIDATION_EFFICIENCY_REVIEW; SPEC_VALIDATION_MATRIX_MASTER; arquivos de §18.

## 9. Estado atual auditado

Get-Content e rg confirmaram: strict tem seis gates incondicionais; builder restaura e
compila cada projeto em processos separados; RunUnityEditModeTests não remove XML anterior;
Assembly-CSharp-Editor.csproj está ausente. Nenhuma suíte foi executada nesta Phase 0.
Dirty anterior inclui harness, farm, arte, cenas e runtime; preservar todos os outros owners.

## 13. Não duplicação

Reusar strict, builder, scanner, matriz e skills existentes. Um helper de processo Unity
compartilha resolução de paths/versão, lock e timeout; nenhuma infraestrutura de cache.
Reusar testes contratuais strict, acrescentando fixtures para runners/docs. SOLID aqui
significa responsabilidade coesa e contrato verificável (skill solid-refactoring).

# /speckit.plan

## 15. Criar vs modificar

Criar helper Unity para processo e validação de resultado; testes contratuais de tooling.
Modificar runners compile/EditMode, scanner, builder; strict, docs/quality/diff;
matriz e instruções existentes. Nenhum tipo C# runtime/DTO/evento é criado.

## 16. Contratos

```powershell
RunUnityEditModeTests.ps1 [-ProjectPath path] [-UnityPath path] [-ResultsPath path]
    [-TestFilter regex] [-LogFile path] [-TimeoutSeconds 900]
# Resolver ProjectVersion quando UnityPath ausente; recusar projeto aberto; paths com espaços.
# Apagar somente o XML/log de saída explícito após checar paths; executar e exigir XML novo,
# total > 0, result Passed, failed=0, passed>0, casos executados coerentes com filtro.
# Exit 0=PASS; 1=FAIL; 2=NOT RUN/contrato inválido; timeout encerra somente processo criado.

ScanUnityLogs.ps1 [-LogFile path] [-Context Compile|Tests] [-ResultsPath path]
# Compile mantém exception genérica crítica; Tests exige XML válido e usa Test Framework
# para exceptions esperadas, mantendo compile/crash/unhandled como erros críticos.

run_strict_validation.ps1 [-ProjectRoot path] [-Gates string[]] [-ScopePath json]
# Omitir Gates = todos os seis gates e GLOBAL_PASS; Gates explícitos = SCOPED_PASS.
# Gates: corruption,docs,build,architecture,diff,quality. Desconhecido/vazio falha.
# Scope JSON: changedFiles (exatos), allowedPaths (exatos), optional reportPaths.
# Paths resolvidos dentro do repo; Scope exige Gates e não é autorização criada pelo runner.
# Nonzero/missing/throw continuam FAIL; logs não contaminam resultado numérico.

Invoke-UnityGeneratedProjectsBuild.ps1 [-ProjectRoot path] [-SkipRestore]
# .slnx + SDK compatível: um dotnet build da solução, restore implícito opcional.
# SDK antigo/sem solução: fallback explícito por projetos; nenhum projeto descartado.
```

Reuso de evidência é revisão de comando, versão/configuração, inputs, exit, XML/log e diff;
não basta HEAD, timestamp nem resumo de subagent. Runtime/save/UI contracts: N/A.

## 17. Sistemas afetados

Orquestração de validação, governança de evidência, processo Unity e feedback de tooling.

## 18. Arquivos permitidos

- Esta spec e docs/validation/spec_validation_efficiency_harness_v1_execution_report.md.
- .specs/SPEC_VALIDATION_MATRIX_MASTER.md.
- .claude/rules/validation-truth.md, subagent-results-not-evidence.md, spec_quality_gate.md.
- .claude/skills/delegated-execution, spec-execution, unity-validation,
  unity-validation-triage, implementation-closeout, solid-refactoring: SKILL.md.
- .claude/commands/validate-spec.md, validate-unity.md, run-editmode-tests.md,
  finish-spec.md, execute-spec-strict.md, loop-spec-batch-strict.md, review-non-regression.md.
- tools/unity/RunUnityCompileValidation.ps1, RunUnityEditModeTests.ps1,
  ScanUnityLogs.ps1, Invoke-UnityGeneratedProjectsBuild.ps1, UnityValidation.Common.ps1,
  Test-UnityValidationTooling.ps1.
- tools/docs/run_strict_validation.ps1, Test-StrictValidation.ps1, validate_docs.ps1,
  check_spec_quality.ps1, check_spec_diff_completeness.ps1, ValidationScope.ps1,
  Test-ValidationScope.ps1.
- Emenda autorizada pelo orquestrador: tools/architecture/Test-ArchitectureRatchet.ps1
  e Test-ArchitectureRatchetContracts.ps1; baselines TSV reais permanecem imutáveis.
- Emenda pós-audit: .claude/agents/unity-validator.md, asset-wiring-specialist.md,
  spec-implementer.md, bugfix-investigator.md, test-author.md; skills bootstrap-wiring,
  combat-data-wiring, editmode-test-authoring, npc-walk-animation,
  scene-interactable-wiring e tilemap-world-rendering (SKILL.md): referências de validação
  passam à matriz/runner canônico, preservando papel e escopo de cada agente/skill.

## 19. Arquivos proibidos

Assets/**, Packages/**, ProjectSettings/** (leitura permitida), docs_old/**, saves reais,
RunStandaloneBuild/GenerateSpawnEcologyAssets, logs/status globais, .codex/**, .agents/**,
AGENTS.md e geração de paridade até coordenação. Sem commits/push.

# /speckit.tasks

## 20. Plano por edição

1. Extrair resolução de versão/paths, quote Windows, lock e espera do processo Unity.
   Fluxo: validar -> preparar outputs -> iniciar -> timeout/exit -> parse XML -> diagnóstico.
2. Builder descobre projetos da solução, consulta SDK e escolhe grafo ou fallback completo.
3. Strict seleciona IDs explícitos, injeta ScopePath só em gates que o entendem e imprime
   GLOBAL/SCOPED distintos. Quality filtra por escopo declarado; docs escaneia placeholders
   somente em documentos. Diff aceita testes existentes evidenciados, sem exigir arquivo novo.
4. Matriz passa a ser fonte única de seleção. Instruções pedem evidência vigente, não
   comandos repetidos; removem assemblies antigas e proibições universais de testes.
5. Fixtures isoladas simulam processo Unity e dotnet; suites cobrem entradas adversas,
   exit não zero, versão, espaços, lock, timeout, XML inválido/antigo/zero/filtro, docs e scope.

## 21. Ordem segura

Spec -> scripts -> matriz/rules -> contratos -> relatório -> coordenação da rodada real.

## 14. Critérios e DoD

- Test-StrictValidation.ps1: `STRICT_VALIDATION_TESTS: PASS`, exit 0; global honesto,
  seleção explícita, scope propagado, unknown/empty gates FAIL e noisy success PASS.
- Test-UnityValidationTooling.ps1: `UNITY_VALIDATION_TOOLING_TESTS: PASS`, exit 0;
  fake executable observa argumentos exatos com espaços; stale/zero/failure XML recusados;
  lock/timeout e versões tratados; scanner Tests respeita XML sem ignorar compile errors;
  builder chama solução uma vez e fallback preserva projetos.
- Test-ValidationScope.ps1: `VALIDATION_SCOPE_TESTS: PASS`, exit 0; variável em .ps1
  não é placeholder; placeholder em .md continua falhando; scene permitida no scope passa,
  path não autorizado/traversal/malformed falha; modo global mantém proteção existente.
  Emenda: arrays com null simples/múltiplo/misto falham; reportPaths explícito ausente
  falha; report deletado/inexistente no diff não satisfaz evidência de runtime.
- Parser PowerShell 5.1 dos scripts alterados: zero erros.
- Test-ArchitectureRatchetContracts.ps1: `ARCHITECTURE_RATCHET_CONTRACTS: PASS`, exit 0;
  fonte com dívida nova, GUID alterado, assembly predefinida ou baseline/regra ausente,
  vazia, duplicada ou inválida falha. Scripts+Tests cobertos; testes de assemblies compiladas
  permanecem com o owner de runtime. Strict architecture é o owner dos três checks de fonte.
- Nenhum runner real Unity/build do jogo executado por este owner; evidência final coordenada.

## 23. Falhas e bordas

- XML velho/zero/NotRun/Failed/filtro sem casos: FAIL, sem promover status.
- Missing editor/projeto aberto: NOT RUN explícito, nenhum kill de processo preexistente.
- Timeout: matar somente child criado; resultado anterior não aproveitado.
- Escopo explícito não autoriza por si edição de assets; comparar autorização da tarefa.
- Dirty de terceiros: não usar todo git status como autoria; global permanece global.
- SDK incompatível: fallback explícito, sem reescrever solução ou perder projetos.

## 22. Validação e evidência

Somente contratos em temporários e parse nesta fatia. O relatório terá Acceptance criteria
extracted, Existing systems audit, Spec Compliance Matrix, Validation e Honest status rationale.
Escopo PASS nunca será apresentado como global PASS. Baseline/falhas reais ficam visíveis;
não promover spec automaticamente.
