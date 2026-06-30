# SPEC — Save/Load: Fechamento de Débitos (Cave Enemy HP, Daily Goals, Cave Run no SaveManager)

> **Spec ID:** `fable_13_spec_save_debt_closure_runtime`
> **Status:** BUILD_VALIDATED (executada — E04; evidência: docs/validation/fable_13_spec_save_debt_closure_runtime_execution_report.md)
> **Wave:** FABLE — Gap Closure Bloco F (fundação)
> **Priority:** P1
> **Type:** Save / Runtime
> **Domain:** Save / Cave / Farm
> **Parallelizable:** NO
> **Parallel group:** fable_bloco_F
> **Can run with:** N/A
> **Must not run with:** QUALQUER spec que adicione estado persistido (fable_07, fable_12)
> **Repo lock scope:** `SaveManager.cs`, `GameSaveData`, `CaveRunManager`, `FarmDailyGoalService`
> **Depends on:**
> - WI-16 (cave run cache), WI-18 (padrão de seção), WI-24 (daily goals)
> **Blocks:** N/A
> **Scope:** persistir os 3 débitos de save documentados sem reescrever SaveManager.
> **Out of scope:** snapshot completo de nível visitado no save (16_spec futura), backup/escrita segura nova.

required_adrs: [ADR-0005]
required_game_rules: [save_rules.md, cave_rules.md]

---

# /speckit.specify

## Contexto

`SAVE_LOAD_FULL_STATE_DIRECTION.md` exige que todo estado de gameplay relevante persista com
seção declarada, owner e restore order. Três débitos estão formalmente documentados nos
reports: **CAVE_ENEMY_HP_SAVE_DEBT** (WI-18: HP de inimigos da caverna não persiste — matar
metade de um nível e salvar/recarregar restaura inimigos cheios),
**SAVE_LOAD_DAILY_GOAL_DEBT** (WI-24: `FarmDailyGoalService.CaptureSaveData/RestoreFromSaveData`
existem mas o SaveManager não os chama), e **CAVE_RUN_SAVE_LOAD_DEBT** (WI-16: estado da run
vive em `GameBootstrap.SetCachedCaveRunState` — sobrevive a troca de cena mas NÃO a fechar o
jogo: save no meio de uma run perde a run).

## Problema

Os três débitos quebram a promessa central de um jogo de save: fechar o jogo no nível 8 da
caverna com a run boa = perder a run; metas diárias resetam em reload; dano causado em
inimigos evapora. São perdas reais de progresso do jogador, não detalhes.

## Objetivo

Ao final desta spec, `GameSaveData` deve ganhar duas seções aditivas (`CaveRun`,
`FarmDailyGoals` — esta só wiring, DTO já existe) e o snapshot do nível corrente deve
incluir HP corrente por `EnemyInstanceId`, restaurado na rematerialização — tudo no padrão
WI-18 (DTOs simples, defaults para saves antigos, sem migration de schema, idempotente).

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
docs/validation/WAVE_INTEGRATION_18_SAVE_LOAD_GAP_REPORT.md
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
.claude/rules/save-dto-simple-types-only.md
.claude/skills/save-load-pattern/SKILL.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- SaveManager (orquestrador; SchemaVersion 5; padrão WI-18 de Capture/Restore + pending data)
- GameSaveData (Quests/FarmDailyGoalsSaveData field JÁ EXISTE — WI-24 criou DTO sem wiring)
- CaveRunManager.State (CaveWorldSeed/CaveRunSeed/level/boss states/checkpoints) +
  GameBootstrap.SetCachedCaveRunState (cache cross-scene)
- VisitedLevelSnapshot (enemy plan + resource states + [F09] chests)
- CaveRuntimeMaterializer (ponto de restauração de inimigos)
- FarmDailyGoalService.CaptureSaveData/RestoreFromSaveData (prontos, não chamados)
Não existe:
- CaveRun como seção do GameSaveData; HP por instância no snapshot; wiring de daily goals
Auditar Fase 0:
- shape exato de CaveRuntimeState (serializável? DTOs simples?) e de VisitedLevelSnapshot
- ordem de restore atual do SaveManager (matriz WAVE 01.03)
```

## Engineering stories

```text
Como jogador, quero fechar o jogo no nível 8 e retomar a run no mesmo nível com checkpoints/bosses.
Como jogador, quero que inimigos feridos continuem feridos ao salvar/recarregar no mesmo nível.
Como fazendeiro, quero metas diárias preservadas em reload no meio do dia.
Como SaveManager, quero seções aditivas com defaults — save antigo carrega sem migration.
```

## Escopo

```text
Inclui:
- CaveRunSaveData (NOVO DTO simples): WorldSeed, RunSeed, CurrentLevel, BossDefeatRecords
  (List<{GateId, IsDefeated}>), CheckpointRecords, HasActiveRun — espelho 1:1 do
  CaveRuntimeState via mapper testável;
- SaveManager.CaptureCaveRunSaveData/RestoreCaveRunSaveData (padrão WI-18 com pending data:
  CaveRunManager pode não existir na cena no momento do load — restaurar via
  GameBootstrap.SetCachedCaveRunState);
- HP por instância: VisitedLevelSnapshot ganha List<EnemyHpRecord {EnemyInstanceId,
  CurrentHp}> (aditivo); materializer grava no snapshot ao sair do nível e aplica HP ao
  rematerializar (0 HP = não materializar — morto permanece morto na run);
- inclusão do snapshot do NÍVEL CORRENTE no CaveRunSaveData (apenas o corrente — snapshot
  completo de todos os níveis visitados fica para 16_spec futura; documentar limite);
- wiring FarmDailyGoals: SaveManager chama Capture/Restore existentes (campo do
  GameSaveData já existe — só ligação);
- restore order documentado: CaveRun após World/Player, antes de cena de caverna materializar;
- EditMode tests: mappers round-trip, HP records, load legado (save v5 sem seções → defaults),
  morto-permanece-morto, daily goals round-trip.
```

## Fora de escopo

```text
Não inclui: snapshot de TODOS os níveis visitados no save (16_spec futura); mudança de
SchemaVersion; migration; política de save dentro da caverna (permitir/bloquear — mantém atual);
backup/escrita segura (existente).
```

## Regras de não duplicação

```text
Não reescrever SaveManager — adicionar seções no padrão existente.
Não criar segundo cache de run — GameBootstrap cache continua o canal cena↔save.
Não criar novo snapshot — estender VisitedLevelSnapshot (aditivo).
```

## Critérios de aceite

### CA-1 Run sobrevive a fechar o jogo
- Save no nível N → load → entrar na caverna retoma nível N com mesma RunSeed/bosses/checkpoints.
- Evidência: testes do mapper + roteiro humano (fechar Editor entre save/load).

### CA-2 HP persistente no nível corrente
- Inimigo com 3/10 HP após save/load rematerializa com 3 HP; morto não rematerializa.
- Evidência: testes de EnemyHpRecord + aplicação no materializer.

### CA-3 Daily goals wired
- Progresso de meta sobrevive a reload no mesmo dia; reset por DayStarted continua.
- Evidência: round-trip + teste de reset.

### CA-4 Compatibilidade
- Save v5 sem as seções carrega com defaults (sem run ativa, goals frescos).
- Evidência: teste de load legado.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Runtime/CaveRunSaveData.cs       (NOVO — DTOs + mapper)
Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs  (EnemyHpRecords aditivo)
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs (gravar/aplicar HP)
Assets/_Game/Scripts/Save/SaveManager.cs                   (2 wirings de seção)
Assets/_Game/Scripts/Save/GameSaveData.cs                  (campo CaveRun aditivo)
Assets/_Game/Tests/EditMode/Core/SaveDebtClosureTests.cs
```

## Contratos

### Save contracts
Seção `CaveRun`: owner CaveRunManager (via bootstrap cache); restore após Player/World.
Seção `FarmDailyGoals`: owner FarmDailyGoalService; restore após Time.
Defaults: HasActiveRun=false / goals vazios. Sem migration. IDs/ints/bools apenas.
### Runtime contracts — mapper estático `CaveRunSaveMapper.ToSaveData/FromSaveData`.
### Event contracts — N/A.
### UI contracts — N/A.

## Sistemas afetados

```text
Save/load, Cave run lifecycle, Farm daily goals, Materializer
```

## Arquivos permitidos

```text
Arquivos da arquitetura ; Assets/_Game/Tests/EditMode/Core/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity/*.prefab/*.asset; Packages/ProjectSettings; SchemaVersion (não incrementar);
seções existentes do GameSaveData (não alterar shape)
```

## Estratégia de implementação

```md
### Fase 0 — Auditar CaveRuntimeState/snapshot/restore order (matriz 01.03).
### Fase 1 — DTOs + mapper + testes round-trip.
### Fase 2 — Wiring SaveManager (CaveRun + FarmDailyGoals) com pending data.
### Fase 3 — EnemyHpRecords no snapshot + materializer (morto permanece morto).
### Fase 4 — Load legado + validação estrita + report.
```

## Paralelização

- Parallelizable: NO
- Reason: lock de save schema — nenhuma outra spec de seção pode rodar junto.

## Impacto em save/load

```text
Does this change save schema? YES (2 campos aditivos com default; SEM bump de SchemaVersion —
critério: campos novos com default são compatíveis no JsonUtility; validar na Fase 0)
Does this add a save section? YES (CaveRun; FarmDailyGoals é wiring de campo existente)
Does this require migration? NO
Does this persist Unity references? NO
```

## Impacto em eventos

```text
Adds events: NO | Changes existing: NO
```

## Impacto em UI/Unity

```text
Changes UI/scenes/prefabs/assets: NO
Requires Play Mode final validation: YES (fechar/reabrir Editor)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: restaurar run com seed antiga + código novo de geração divergir layout.
Mitigação: geração é determinística por seed (F09 valida replay); registrar versão de
geração no save para diagnóstico (string, não gate).
Risco: HP aplicado antes do Configure do inimigo. Mitigação: aplicar após Configure no
materializer; teste de ordem.
```

## Rollback

```text
Remover wirings/campos aditivos — saves gravados com seções extras continuam carregáveis
(JsonUtility ignora campos desconhecidos ao ler com DTO antigo).
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar estado/snapshot/restore order.
- [ ] T002 — CaveRunSaveData + mapper + round-trip tests.
- [ ] T003 — Wiring SaveManager CaveRun (pending data pattern).
- [ ] T004 — Wiring FarmDailyGoals (Capture/Restore existentes).
- [ ] T005 — EnemyHpRecords + materializer + morto-permanece-morto.
- [ ] T006 — Load legado v5; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES
- Requires EditMode tests: YES (round-trip é mandatório para save)
- Requires PlayMode automated or final human scenario: YES (fechar/reabrir)
- Requires regression test: YES (seções existentes intactas; load legado)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: round-trips + cenário humano save-no-nível-8

## Definition of Done

```text
3 débitos fechados com testes; saves antigos compatíveis; sem migration; builds 0E; report
atualizando o backlog de débitos (WAVE 22).
```

## Anti-regressão

```text
Seções existentes byte-compatíveis. Sem refs Unity. Stable run intacto (seeds persistidos
são os mesmos do cache). Restore order documentado e testado.
```

## Notas para execução posterior

```text
Snapshot multi-nível no save: 16_spec_cave_snapshot_save_provider_restore_order_future.
Política de save em caverna (bloquear no boss?): 16_spec_cave_final_save_restriction_policy_future.
```
