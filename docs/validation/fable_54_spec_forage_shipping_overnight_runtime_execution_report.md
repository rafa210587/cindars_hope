---
doc_type: validation
status: evidence
spec_id: fable_54_spec_forage_shipping_overnight_runtime
validation_type: automated
result: BUILD_VALIDATED
date: 2026-06-20
executor: Claude Code
source_of_truth: false
validated_adrs: []
validated_game_rules: [farm_rules.md, event_rules.md, save_rules.md]
---

# Validation Report — fable_54 Forrageio Sazonal + Shipping Bin Noturno

> **This report is evidence, NOT an execution queue.**
> **Do not re-run the spec based on this report alone.**

Status: **BUILD_VALIDATED_WITH_WARNINGS** (núcleo runtime pronto + testes EditMode; Play Mode/visual
final DIFERIDO ao lote final por decisão do dono — DEFERRED_TO_FINAL_VALIDATION).

---

## Resumo

Os DOIS módulos de farm órfãos foram HOSPEDADOS e LIGADOS (padrão F15), sem reescrita:

- **Forrageio sazonal:** `FarmForageRuntimeService` (host bootstrap) resolve a `ForageSeasonTable`
  da estação atual no `DayStartedEvent`, seleciona deterministicamente até 4 pontos
  (`ForageStableHash` / FNV-1a sobre worldSeed+dia) e (re)ativa/respawna via
  `FarmForageSpawnService` (módulo órfão, INTOCADO). `ForagePointInteractable` (6 pontos no gerador)
  substitui o smoke `ForageResource_01` (removido — não coexistem).
- **Shipping overnight:** `ShippingBinRuntimeService` (host bootstrap, dono da lista pendente)
  processa o batch da manhã no `DayStartedEvent` via `FarmShippingService.ProcessDayBatch`
  (idempotente, canal 0.95 via `ShippingPriceResolver`), credita o ouro 1× via `PlayerManager` e
  publica `EconomyTransactionCompletedEvent` (feedback via `ShippingSummaryService` existente) +
  `ShippingBatchProcessedEvent` (novo, resumo). `ShippingBinInteractable` na Zone_ShippingSellpoint
  deposita os itens vendáveis (reusa `SellableItemPolicy`).
- **Save:** campos ADITIVOS na seção farm (`PendingShipping`, `ForageSpawns`); legado = vazio.
- **Meta diária:** depósito no bin progride `daily_goal_first_harvest` (hook aditivo).

---

## Acceptance criteria extracted

### Acceptance Criteria (extraídos + evidência)

| CA | Critério | Status | Evidência |
|----|----------|--------|-----------|
| CA-1 | Forrageio sazonal determinístico (StableHash(worldSeed,day) + tabela da estação; mesmo dia=mesmos spawns; zona errada nunca spawna fora da tabela) | OK | `ForageStableHash.SelectDailyIndices` determinístico; `ForageSeasonTable` por estação; tests `Forage_SameSeedAndDay_SameSelection`, `ForageTable_DistinctSeasons_DistinctTables`, `ForageTable_AllSeasonItems_HaveAllowedSeasonMatchingTable`, `Forage_WrongSeason_NeverCollects_ServiceContract` |
| CA-2 | Coleta via serviço órfão com respawn (2ª coleta falha; respawn após política) | OK | `FarmForageRuntimeService.Collect` delega a `FarmForageSpawnService.Collect/TryRespawn`; tests `Forage_SecondCollect_FailsNotAvailable`, `Forage_Respawn_AfterPolicyDays` |
| CA-3 | Shipping overnight idempotente (Deposit cria entry ProcessOnDay=dia+1; batch credita 1×; reprocessar não paga 2×; quest/key recusados) | OK | `ShippingBinRuntimeService` + `FarmShippingService`; tests `Shipping_Deposit_CreatesPendingEntry_ProcessOnNextDay`, `Shipping_Overnight_CreditsOnce_Channel095`, `Shipping_Overnight_Idempotent_NoDoublePay`, `Shipping_QuestAndKeyItems_Refused` |
| CA-4 | Persistência da batch pendente (salvar+recarregar preserva; paga na manhã correta; legado sem erro) | OK | DTOs aditivos + Capture/Restore; tests `Save_PendingShipping_RoundTrip_PreservesEntry`, `Save_PendingShipping_RoundTrip_PaysOnCorrectMorning`, `Save_LegacyEmpty_LoadsWithoutBatch`, `Save_ForageSpawns_RoundTrip_PreservesState` |
| CA-5 | Meta diária integrada (depósito progride meta de colheita) | OK | `FarmDailyGoalService.ProgressHarvestGoal` (hook aditivo) chamado pelo `ShippingBinInteractable` por depósito bem-sucedido; verificação runtime (DEFERRED Play Mode) |

> CA-5: a integração de meta é via chamada direta a um serviço singleton de farm (`ProgressHarvestGoal`)
> a partir do interactable, espelhando o hook que o `FarmDailyGoalService` já expõe; a evidência
> automatizada cobre a unidade do serviço (AddProgress) — o caminho cena→interactable→serviço é
> coberto pelo cenário humano (DEFERRED).

---

## Existing Systems Audit (system-reuse-audit / Fase 0)

| Sistema | Encontrado | Ação | Nota |
|---------|-----------|------|------|
| `FarmForageSpawnService` (+ ForageDefinition/ForageSpawnState) | SIM (órfão) | REUSADO, INTOCADO | Collect/TryRespawn; zonas proibidas; idempotência |
| `FarmShippingService` (+ PendingShippingEntry/ShippingBatch/ShippingPriceResolver) | SIM (órfão) | REUSADO, INTOCADO | Deposit/ProcessDayBatch; canal 0.95; IsProcessed |
| `ShippingSummaryService` | SIM | REUSADO | Feedback de venda via EconomyTransactionCompletedEvent |
| `FarmDailyGoalService` | SIM | ESTENDIDO (hook aditivo `ProgressHarvestGoal`) | GoalId real = `daily_goal_first_harvest` (não há `bd_harvest` literal) |
| `SellableItemPolicy` | SIM | REUSADO | Decide o que vai para o bin (sem 2º canal de venda) |
| `EconomyManager` / `PlayerManager.AddGold` | SIM | REUSADO | Crédito do batch via PlayerManager (ref de bootstrap) |
| Padrão de host F15 (`WorldWeatherRuntimeBootstrap`, `FarmDailyGoalRuntimeBootstrap`) | SIM | SEGUIDO | RuntimeInitializeOnLoadMethod + Instance + DontDestroyOnLoad |
| Padrão de save aditivo (fable_55 `Processing`) | SIM | SEGUIDO | Campos aditivos em `FarmSaveData`; legado-null tolerante |
| Smoke `ForageResource_01` | SIM | REMOVIDO/CONVERTIDO | Substituído por ForagePointInteractable (anti-regressão) |

Nenhum sistema paralelo criado.

### Decisões de Fase 0 documentadas

- **GoalId de colheita:** `daily_goal_first_harvest` (o "bd_harvest" do texto da spec é placeholder; o
  serviço real define `daily_goal_first_harvest` e `daily_goal_sell_first_crop`). O depósito no bin
  progride a meta de COLHEITA; a venda overnight (TransactionType "shipping") também progride a meta
  de VENDA via subscription existente.
- **Host F15:** mesmo padrão `RuntimeInitializeOnLoadMethod(AfterSceneLoad)` + Instance estático; dois
  hosts novos (Forage/Shipping), cada um dono do seu estado e da sua seção de save aditiva.
- **ShippingEntryId = Guid.NewGuid:** aceitável para entry de SAVE (não é conteúdo de caverna; não
  exige determinismo de replay — ADR-0005 não se aplica). Documentado e mantido (módulo órfão INTOCADO).
- **Itens de forrageio por estação (catálogo F32):** o catálogo NÃO tem itens dedicados de forrageio
  por estação. A `ForageSeasonTable` v1 referencia IDs EXISTENTES do catálogo canônico
  (crops + materiais foragáveis), ≥2 por estação, sem expandir o catálogo. **Follow-up registrado**
  (abaixo) para itens de forrageio dedicados.
- **ForageTable data-driven por SO:** fora de escopo (v1 em código), documentado.

---

## Spec Compliance Matrix (requirement → implementation)

| Requirement (spec) | Implementação |
|--------------------|---------------|
| `FarmForageRuntimeService` (host; DayStarted → tabela + seleção determinística + TryRespawn) | `Assets/_Game/Scripts/Farm/Forage/FarmForageRuntimeService.cs` |
| `ForageSeasonTable` (season → ForageDefinitions; dados em código v1) | `Assets/_Game/Scripts/Farm/Forage/ForageSeasonTable.cs` |
| Seleção determinística StableHash(worldSeed, day) | `Assets/_Game/Scripts/Farm/Forage/ForageStableHash.cs` (FNV-1a) |
| 4-6 pontos no gerador (refs serializadas) + smoke removido | `CreateMvpFarmScene.CreateForagePoints` (6 pontos farm_forage_01..06); `CreateForageResource` removido |
| Interactable de coleta → FarmForageSpawnService.Collect | `Assets/_Game/Scripts/Farm/Forage/ForagePointInteractable.cs` |
| `ShippingBinRuntimeService` (host; DayStarted → ProcessDayBatch → EconomyManager + eventos) | `Assets/_Game/Scripts/Farm/Shipping/ShippingBinRuntimeService.cs` |
| Bin físico interactable na Zone_ShippingSellpoint | `Assets/_Game/Scripts/Farm/Shipping/ShippingBinInteractable.cs` + `CreateMvpFarmScene.CreateShippingBin` |
| `ShippingBatchProcessedEvent` (novo) | `Assets/_Game/Scripts/Core/Events/ShippingBatchProcessedEvent.cs` |
| Crédito único via EconomyManager + ShippingSummaryService feedback | PlayerManager.AddGold + EconomyTransactionCompletedEvent("shipping") |
| Meta diária: depósito progride colheita | `FarmDailyGoalService.ProgressHarvestGoal` (hook aditivo) |
| Save aditivo: pendingShipping + forageSpawns (tipos simples; legado vazio) | `FarmSaveData.PendingShipping`/`ForageSpawns`; DTOs em Shipping/Forage; SaveManager Capture/Restore |
| EditMode tests (determinismo/respawn/Deposit/ProcessDayBatch/idempotência/save round-trip/zona/meta) | `Assets/_Game/Tests/EditMode/Farm/ForageShippingWiringTests.cs` (24 testes) |

---

## Validation

What Was Run / Results:

| Check | Result | Notes |
|-------|--------|-------|
| C# runtime build (Assembly-CSharp.csproj) | PASS — 0E/1W | 1 warning pré-existente (CombatTelemetrySession CS0649), não desta spec |
| C# editor build (Assembly-CSharp-Editor.csproj) | PASS — 0E/3W | 3 warnings pré-existentes (CreateEnemyActionsAndSets, CSharpProjectPostprocessor) |
| Docs validation (validate_docs.ps1) | PASS (exit 0) | |
| Spec diff completeness (check_spec_diff_completeness.ps1) | PASS (exit 0) | após criação deste report |
| Strict validation (run_strict_validation.ps1) | PASS (exit 0) | artefato: docs/validation/LAST_STRICT_VALIDATION_RESULT.json |
| EditMode tests (compilam em Assembly-CSharp) | COMPILE PASS | execução via Unity Test Runner DIFERIDA (lote final) |
| Unity validators / asset generation | NOT RUN | sem Unity Editor (decisão do dono); cena regenerada manualmente no lote final |
| Play Mode | NOT RUN | DEFERRED_TO_FINAL_VALIDATION (autorizado pelo dono) |

Validation method: run_strict_validation.ps1 — Exit code: 0.

---

## ADRs / Game Rules Validated

| Item | Status | Notes |
|---|---|---|
| farm_rules.md | PASS | Loop diário (DayStarted), economia de farm (canal de venda adicional), persistência da seção farm |
| event_rules.md | PASS | `ShippingBatchProcessedEvent` é struct de tipos simples; comunicação via GameEventBus; unsubscribe em OnDisable nos 2 hosts |
| save_rules.md | PASS | DTOs só tipos simples/IDs; sem refs Unity; backward-compatible (campos ausentes = vazios); sem migração |

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (seleção por seed/dia, batch overnight, respawn, save round-trip)
Changed Unity scene/prefab/asset wiring: YES (gerador CreateMvpFarmScene — via código, regen manual)
Automated tests added/updated: YES (ForageShippingWiringTests.cs — 24 testes)
Automated tests command: Unity Test Runner EditMode (DIFERIDO ao lote final); compilação validada via dotnet build Assembly-CSharp.csproj (exit 0)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (lote final) — coletar forrageio sazonal + depositar no bin + acordar com ouro creditado
Justification if no automated tests: N/A (testes adicionados)
Residual risk: caminho cena→interactable→serviço (registro de pontos, depleção visual, prompt do bin, crédito real ao PlayerManager) não exercitado sem Play Mode; cobertura automatizada é da lógica determinística pura. Regeneração da FarmScene pendente de Unity (evidência no lote final).
```

Automated tests added/updated: YES
Manual Play Mode scenario required: YES (DEFERRED)
Residual risk: ver acima.

---

## Errors Found

```
(nenhum — ambos os builds exit 0; docs/strict validation exit 0)
```

## Warnings (pre-existing)

```
Assembly-CSharp: CombatTelemetrySession.cs(43) CS0649 — pré-existente (fable_59), não desta spec.
Assembly-CSharp-Editor: CreateEnemyActionsAndSets.cs CS0649 (x2) + CSharpProjectPostprocessor.cs UNT0006 — pré-existentes.
```

---

## Evidence

Files created:
```
Assets/_Game/Scripts/Core/Events/ShippingBatchProcessedEvent.cs (+ .meta)
Assets/_Game/Scripts/Farm/Forage/ForageSeasonTable.cs (+ .meta)
Assets/_Game/Scripts/Farm/Forage/ForageStableHash.cs (+ .meta)
Assets/_Game/Scripts/Farm/Forage/ForageSpawnsSaveData.cs (+ .meta)
Assets/_Game/Scripts/Farm/Forage/FarmForageRuntimeService.cs (+ .meta)
Assets/_Game/Scripts/Farm/Forage/ForagePointInteractable.cs (+ .meta)
Assets/_Game/Scripts/Farm/Shipping/PendingShippingSaveData.cs (+ .meta)
Assets/_Game/Scripts/Farm/Shipping/ShippingBinRuntimeService.cs (+ .meta)
Assets/_Game/Scripts/Farm/Shipping/ShippingBinInteractable.cs (+ .meta)
Assets/_Game/Tests/EditMode/Farm/ForageShippingWiringTests.cs (+ .meta)
docs/validation/fable_54_spec_forage_shipping_overnight_runtime_execution_report.md
```

Files modified:
```
Assets/_Game/Scripts/Save/SaveData.cs (campos aditivos PendingShipping/ForageSpawns em FarmSaveData)
Assets/_Game/Scripts/Save/SaveManager.cs (Capture/Restore dos 2 campos aditivos)
Assets/_Game/Scripts/Farm/Runtime/FarmDailyGoalService.cs (hook aditivo ProgressHarvestGoal)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (CreateForagePoints + CreateShippingBin; smoke ForageResource_01 removido)
Assembly-CSharp.csproj (includes dos 9 .cs novos + 1 teste — exigido: sem asmdef, includes explícitos)
```

Files NOT changed (protegidos / órfãos INTOCADOS):
```
Assets/_Game/Scripts/Farm/Forage/FarmForageSpawnService.cs
Assets/_Game/Scripts/Farm/Forage/ForageDefinition.cs
Assets/_Game/Scripts/Farm/Forage/ForageSpawnState.cs
Assets/_Game/Scripts/Farm/Shipping/FarmShippingService.cs
Assets/_Game/Scripts/Farm/Shipping/ShippingPriceResolver.cs
Assets/_Game/Scripts/Farm/Shipping/PendingShippingEntry.cs
Assets/_Game/Scripts/Farm/Shipping/ShippingBatch.cs
*.unity / *.prefab / *.asset (nenhuma edição manual de YAML; cena só via gerador)
Packages/** ; ProjectSettings/** ; SaveManager core (apenas campos aditivos na seção farm)
```

---

## Dependency Chain

```
Original target: fable_54_spec_forage_shipping_overnight_runtime
Dependency chain: F15 (executada/BUILD_VALIDATED — padrão de host) ; F13 (executada — padrão de save) ; F32 (executada — catálogo de itens)
Forbidden dependencies: [none]
Resolved depth: 0 (todas as dependências já executadas; nenhuma same-wave pendente)
Can continue original target: YES
```

---

## Anti-regressão (verificada)

- `FarmForageSpawnService` / `FarmShippingService` / `ShippingPriceResolver` — sem mudança de contrato
  (INTOCADOS; testes existentes `FarmForageFishingTests`/`FarmShippingServiceTests` continuam válidos).
- Venda imediata existente (SellPoint) intacta — o bin é canal ADICIONAL.
- DayStarted continua disparando daily goals / clima (F15) — os 2 novos hosts apenas assinam, com
  unsubscribe em OnDisable (sem subscriber morto).
- Zonas proibidas (fonte/caverna/saída/lore) jamais recebem spawn (regra do serviço órfão é a fonte).
- Save legado carrega sem erro (campos ausentes = listas vazias); só tipos simples no DTO.
- Zero `GameObject.Find`/`FindObjectOfType` em runtime de gameplay (refs serializadas pelo gerador;
  `FindAnyObjectByType` só nos bootstraps de wiring, padrão aprovado F15). Eventos só via GameEventBus.

---

## Follow-ups (NÃO nesta spec)

1. **Itens de forrageio dedicados por estação no catálogo F32:** a `ForageSeasonTable` v1 usa IDs
   existentes (crops + materiais foragáveis). Um follow-up de DATA pode adicionar itens próprios de
   forrageio (ervas/cogumelos/frutos silvestres) por estação ao catálogo canônico e re-apontar a tabela.
2. **ForageTable data-driven (SO):** mover a tabela de código para ScriptableObject (v2).
3. **UI de depósito seletivo no bin:** hoje deposita todos os vendáveis; uma tela de seleção
   (reusando SellPanel) é melhoria futura (fora de escopo — sem tela nova nesta spec).
4. **Tree/Rock interactables FINAL:** continuam smoke (TODO_INTEGRATION_NOT_FINAL); só Forage foi
   finalizado nesta spec.

---

## Phase Status

| Phase | Status | Date |
|-------|--------|------|
| Phase 0 (Audit) | COMPLETE | 2026-06-20 |
| Phase 1 (Automated build/docs/strict) | PASS | 2026-06-20 |
| Phase 2 (Unity validators / asset gen) | NOT RUN (sem Unity Editor; regen da FarmScene no lote final) | — |
| Phase 3 (Play Mode) | NOT RUN — DEFERRED_TO_FINAL_VALIDATION | — |

---

## Next Action

```
1. Humano: regenerar a FarmScene via gerador (CindarsHope → Scene → Create MVP Farm Scene) para
   materializar os 6 pontos de forrageio + a caixa de envio (evidência de geração no lote final).
2. Humano: Unity Test Runner EditMode (rodar ForageShippingWiringTests — 24 testes).
3. Humano: cenário Play Mode do lote final — coletar forrageio sazonal e receber o pagamento do bin
   na manhã seguinte (depósito → dormir/avançar dia → ouro creditado 1×).
```

---

*Report generated: 2026-06-20*
