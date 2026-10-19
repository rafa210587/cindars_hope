# SPEC — SOLID, contexto de AI e contratos verificáveis do harness

> **Spec ID:** `spec_solid_ai_harness_hardening_v1`
> **Status:** CODE_COMPLETE_WITH_GLOBAL_GATES_FAILING — execução autorizada pelo pedido humano desta conversa
> **Wave:** SOLID_AI — Qualidade estrutural e ferramentas
> **Priority:** P1
> **Type:** Tooling / Governance
> **Domain:** Core / Save
> **Parallelizable:** CONDITIONAL
> **Parallel group:** Harness
> **Can run with:** auditoria/refatoração runtime que não edite os arquivos desta spec
> **Must not run with:** edição dos mesmos hooks, gerador e strict validator
> **Repo lock scope:** arquivos exatos em §18; nenhuma geração de saídas Codex durante trabalho concorrente
> **Depends on:** AGENTS.md, docs/project/CURRENT_STATE.md, auditoria Phase 0 desta sessão
> **Blocks:** confiança nos gates de refatoração SOLID
> **Scope:** corrigir instruções perigosas, contratos de hooks e resultados de validação; integrar regras SOLID ao harness existente
> **Out of scope:** runtime, schema/save real, arte, scenes, migração de contexto histórico, cabeçalho universal por classe
> **Validation level alvo:** testes contratuais PowerShell; BUILD_VALIDATED somente com strict exit 0
> **Executor:** Claude ou Codex
> **Ordem de execucao:** contrato do harness antes da validação de refatorações
> **Depende de:** auditoria Phase 0 desta spec; sem dependência de implementação externa
> **Bloqueia:** claims de validação estrita de refatorações que dependam destes gates

required_adrs: []
required_game_rules: []

# /speckit.specify

## 5. Contexto

O humano pediu auditoria ampla, regras/skills/agents para clean code, modularização,
desacoplamento, facilidade de uso de AI, avaliação crítica de frontmatter e refatoração,
incluindo SOLID. Esse pedido autoriza criar e executar esta fatia de tooling/governança.
Foi adotada a recomendação de XML útil e índice consultável, sem impor documentação
mecânica universal no runtime. O índice pertence à fatia coordenada pelo orquestrador.

## 6. Problema

Os guards reconhecem `Edit`/`Write`, mas o matcher Codex também anuncia `apply_patch`;
payloads de patch são ignorados. O gerador hardcoda hooks e omite a sincronização de Stop.
O strict mistura stdout com resultado numérico e chama toda falha de docs de legado.
A skill de save reatribui IDs pela posição e contradiz a regra de estabilidade de ID.

## 7. Objetivo

Produzir contratos testados de normalização, geração e validação, mantendo SOLID
orientado a coesão/contratos reais e patterns justificados por necessidade observada.

## 8. Fontes obrigatórias lidas

- `AGENTS.md`, `docs/project/CURRENT_STATE.md`, `CLAUDE.md`.
- `.claude/HARNESS_AUTHORING_STANDARD.md`, skills `spec-authoring`, `harness-authoring`,
  `harness-audit`, `monobehaviour-decomposition`, `save-load-pattern`, `non-regression-review`.
- Rules `unity-architecture`, `id-stability`, `validation-truth`, `code-minimalism-ladder`.
- Quatro guards e gerador/strict em §18; `.claude/settings.json`.
- `Assets/_Game/Scripts/Save/ISaveSectionProvider.cs`, `SaveManager.cs`,
  `Save/Providers/HotbarSectionProvider.cs`, `Save/Migrations/SaveMigrationRegistry.cs`,
  `Inventory/InventorySaveData.cs` (somente leitura, caminhos relativos a Scripts).

## 9. Estado atual do repo — Phase 0 auditada

```text
Contagem Get-ChildItem: 66 skills, 10 agents, 22 arquivos de rules.
Get-Content(...).Count: CURRENT_STATE=545; CLAUDE=244; AGENTS=269.
Smoke explícito runtime-code-guard: apply_patch contendo namespace proibido -> exit 0;
controle Edit com mesmo conteúdo -> exit 2. Nenhum arquivo foi escrito pelo smoke.
Generate-CodexHarness.ps1: SettingsPath declarado, mas hooks hardcoded; Stop sync ausente.
run_strict_validation.ps1: stdout devolvido junto com número; docs nonzero mascarado.
Test-ArchitectureRatchet.ps1 já existe, mas não é chamado pelo strict.
save-load-pattern: OnValidate escreve items[i].Id = i + 1; inventário real usa string ItemId.
Working tree contém alterações de outras sessões (rules/keyart/farm/arte/scenes).
```

Reconfirmar o diff dos arquivos permitidos antes de editar. Falhas globais de docs já
conhecidas não autorizam aumentar baseline, ignorar falhas ou declarar BUILD_VALIDATED.

## 13. Regras de não duplicação

Reusar `architecture-reviewer`, `non-regression-auditor`, `spec-implementer`, ratchet e
strict existentes. Um normalizador compartilhado atende os quatro guards; não criar
parser por guard. Reusar `ISaveSectionProvider`, `SaveProviderRegistry` e migrations atuais.
Seleção de patterns permanece ancorada em `non-regression-review` e `system-reuse-audit`.

# /speckit.plan

## 15. Arquitetura alvo — CRIAR vs MODIFICAR

CRIAR: rule SOLID/contexto; skill de refatoração; normalizador de entradas de hooks;
três suítes de testes contratuais PowerShell; execution report.
MODIFICAR: agentes existentes, orientação save/arquitetura, índices, quatro guards,
gerador de hooks a partir de settings e execução do strict em processos isolados.
Nenhuma classe C# criada ou modificada nesta spec.

## 16. Contratos de tooling

```powershell
ConvertFrom-EditToolPayload -RawJson <string>
# Retorna operações com Path, PreviousPath, Operation, AddedContent, IsFullWrite.
# Write/Edit/write_file e apply_patch: input string, objeto ou JSON serializado.
# Patch suporta múltiplos arquivos, Add/Update/Delete/Move; malformed -> throw.

Generate-CodexHarness.ps1 [-WhatIf]
# Lê settings.json antes de mutar; preserva eventos, opções, comandos e ordem.
# Traduz matchers de edição/shell; origem inválida -> exit != 0, sem reset das saídas.
# Config existente preservado byte-exato; scaffold root-level somente se ausente.

run_strict_validation.ps1 [-ProjectRoot <string>]
# Sequência: corrupção, docs, build, ratchet, diff, qualidade.
# Executa scripts em PowerShell filho; encaminha logs fora do valor de retorno.
# Success ruidoso -> exit 0; nonzero/throw/arquivo ausente -> exit 1.
```

Guards: exit 0 somente se entrada inspecionada e sem problema; exit 2 com diagnóstico
sem reproduzir secrets quando entrada inválida ou violação. PostToolUse não desfaz edits.
Runtime/events/save/UI contracts: N/A, tooling e instruções apenas.

## 17. Sistemas afetados

Governança de arquitetura; protocolos de hooks; paridade Claude/Codex; strict validation.

## 18. Arquivos permitidos

```text
.specs/a_implementar/spec_solid_ai_harness_hardening_v1.md
.claude/rules/solid-and-ai-context.md
.claude/rules/unity-architecture.md
.claude/rules/RULES.md (preservar todas as alterações existentes)
.claude/skills/solid-refactoring/SKILL.md
.claude/skills/save-load-pattern/SKILL.md
.claude/agents/architecture-reviewer.md
.claude/agents/non-regression-auditor.md
.claude/agents/spec-implementer.md
CLAUDE.md
.claude/hooks/edit-tool-payload.ps1
.claude/hooks/runtime-code-guard.ps1
.claude/hooks/protected-path-guard.ps1
.claude/hooks/guard-secrets.ps1
.claude/hooks/guard-large-files.ps1
tools/codex/Generate-CodexHarness.ps1
tools/codex/README.md
tools/codex/Test-EditToolGuards.ps1
tools/codex/Test-CodexHarnessGeneration.ps1
tools/docs/run_strict_validation.ps1
tools/docs/Test-StrictValidation.ps1
docs/validation/spec_solid_ai_harness_hardening_v1_execution_report.md
```

## 19. Arquivos proibidos

`Assets/**`, `Packages/**`, `ProjectSettings/**`, `docs_old/**`, saves reais,
`tools/architecture/**`, `CURRENT_STATE.md`, `PROJECT_LOG.md`, `IMPLEMENTATION_STATUS.md`,
saídas geradas `.codex/**`, `.agents/**`, `AGENTS.md` antes da coordenação final.
Emenda de execução: orquestrador autorizou geração final após backup/hash das saídas;
preservar `.codex/config.toml` byte-exato e inspecionar divergências antes de gerar.
Sem commit/push.

# /speckit.tasks

## 20. Estratégia de implementação

1. Regra/skill: SOLID sem quotas de LOC/interfaces, intenção em XML, consultas por escopo;
   conectar nos agentes e índices. Skill `harness-authoring`; nenhuma proliferação de agentes.
2. Save: substituir exemplos fictícios por contratos/paths reais, IDs string estáveis,
   registro tipado e migração explícita; remover toda atribuição por posição.
3. Guards: normalizar envelope -> operações -> checar paths e conteúdo adicionado;
   erros de parse causam diagnóstico + exit 2. Move inspeciona origem e destino.
4. Gerador: parse settings antes de reset, traduzir apenas matchers conhecidos,
   preservar propriedades/eventos, validar targets absolutos antes de delete recursivo.
5. Strict: chamar PowerShell filho para cada script, emitir stdout como log, retornar
   somente exit code; falhar imediatamente; acrescentar ratchet existente.
6. Testes em temporários: fixtures de scripts/repos; nenhuma alteração no runtime.
7. Registrar os testes e falhas reais globais; geração final coordenada pelo orquestrador.

## 21. Ordem segura de execução

Spec -> regra/skill -> adapters de payload -> gerador -> strict -> testes -> report.

## 14. Critérios de aceite e Definition of Done

- `Test-EditToolGuards.ps1` imprime `EDIT_TOOL_GUARDS_TESTS: PASS`, exit 0.
  Casos: Write/Edit/write_file; patch multi-file/string/objeto; add/update/delete/move;
  malformed; forbidden namespace/path; conteúdo removido não bloqueia; >1 MB bloqueia.
- `Test-CodexHarnessGeneration.ps1` imprime `CODEX_HARNESS_TESTS: PASS`, exit 0.
  Asserts: Stop sync preservado, custom hook/options/order preservados, matcher
  traduzido, -WhatIf sem escrita, geração duas vezes idempotente, origem inválida falha;
  config existente byte-exato e scaffold novo com propriedades no nível raiz.
- `Test-StrictValidation.ps1` imprime `STRICT_VALIDATION_TESTS: PASS`, exit 0.
  Asserts: noisy success exit 0; nonzero/throw/docs error/missing script exit 1;
  ratchet chamado; fail-fast não executa passos posteriores.
- Parse PowerShell de todos os scripts novos/alterados retorna zero erros.
- Rule <=70 linhas, skill nova <=150; índices contêm `solid-refactoring` e rule.
- Nenhuma alteração nos arquivos proibidos desta fatia; preservar dirty pré-existente.
- `run_strict_validation.ps1` real retorna exit code honesto; falha global bloqueia
  BUILD_VALIDATED, mesmo quando testes contratuais específicos passam.

## 23. Edge cases / falhas

- Envelope desconhecido/malformed: recusar com diagnóstico de contrato, sem expor entrada.
- Patch com move e adições: aplicar guards ao destino e origem; não ignorar segundo arquivo.
- Paths com `..`/absolutos: normalizar antes de checar diretórios proibidos.
- Hook PostToolUse detecta violação após escrita: pede correção, não afirma rollback.
- Settings inválido: validar antes de qualquer reset; saída gerada anterior permanece.
- Test cleanup: confirmar target absoluto dentro do diretório temporário exclusivo antes de remover.
- Docs baseline vermelho: manter FAIL explícito e restante NOT RUN por fail-fast.

## 22. Validação e gates

Executar três suítes contratuais, parse scripts, docs e strict reais. Não executar build
ou Unity paralelo. Unity/Play Mode: NOT RUN, sem runtime alterado nesta spec.
Report com Scope, Changes, Validation Results, Testing Quality Gate, Residual Risks,
Commands/exit codes e Phase status. Sem promoção para implementados durante esta fatia.
