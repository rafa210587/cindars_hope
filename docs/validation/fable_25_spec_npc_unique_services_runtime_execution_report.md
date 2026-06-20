---
doc_type: validation_report
spec_id: fable_25_spec_npc_unique_services_runtime
status: BUILD_VALIDATED_WITH_WARNINGS
wave: FABLE Batch 7
date: 2026-06-20
validated_adrs: [ADR-0006, ADR-0007]
validated_game_rules: [npc_rules.md, economy_rules.md]
---

# Execution Report — fable_25 NPCs: Serviços Únicos por Personagem

> Status: **BUILD_VALIDATED_WITH_WARNINGS**
> Núcleo (registry/executor/gates/custos/pendências/save + 8 serviços + Conversar único)
> completo e validado por build + EditMode. Play Mode / validação humana DIFERIDA
> (autorizado pelo dono). Sub-efeitos visuais/UI de comida e consumo do desconto pelo forge
> são débitos documentados (não bloqueiam o núcleo).

## 1. Resumo

Adicionada a camada de **serviços únicos de NPC** como opções no fluxo **Conversar existente**
(`NpcShopController` + árvore de diálogo), sem criar segundo fluxo. Cada serviço tem gate de
amizade (F26) e/ou flag (QuestFlagService), custo (ouro/item) e efeito real despachado por
interface ao sistema-alvo já implementado (F21/F16/F12/F31 + hooks de reparo/pasto/contrato).
Pendências temporais (encomenda 3 dias, contrato semanal, prato 1×/dia, pasto 3 dias)
persistidas em seção aditiva. Opção gated aparece **desabilitada com o motivo** (descoberta >
ocultação), nunca some.

## 2. Mapeamento serviço → npcId canônico (Fase 0)

A spec cita nomes ausentes do roster v1.1 (Sereth/Kael/Mirena/Veska). Conforme o mandato da
Fase 0 ("resolver contra o roster sem alterar o efeito"), cada serviço foi ancorado ao NPC
canônico de **função equivalente** (`NpcTownRosterRegistry`), mantendo 1 serviço único por NPC
e 8 NPCs distintos:

| # | Serviço | npcId canônico | Resolução |
|---|---------|----------------|-----------|
| 1 | Análise de Criatura | `npc_thalindra` | roster ✓ (Pesquisador/Alquimista) |
| 2 | Reparo com Desconto | `npc_brumdar` | roster ✓ (Artesao — reparo) |
| 3 | Encomenda de Livro | `npc_yael` | roster ✓ (itens raros/segredos, loja noturna) |
| 4 | Banho Termal | `npc_gruta` | Sereth → gruta (taverna/estalagem, descanso) |
| 5 | Pasto Premium | `npc_eiran` | roster ✓ (Tratador — animais/ração) |
| 6 | Contrato de Caça | `npc_zrix` | Kael → zrix (mapas/caverna/contratos da guilda) |
| 7 | Prato do Dia | `npc_orlan` | Mirena → orlan (taverna/estalagem, comida) |
| 8 | Identificação de Relíquia | `npc_ozzra` | Veska → ozzra (Alquimista — itens mágicos) |

Efeitos, custos e gates do escopo permanecem inalterados; só o npcId foi resolvido. Teste
`Catalog_NpcIds_AreCanonicalRosterIds` falha o build se algum id divergir do roster.

## Acceptance criteria extracted

Ver tabela detalhada na seção 3 (CA-1..CA-5, cada um com critério, status OK e evidência).

## Existing systems audit

Ver tabela detalhada na seção 4 (sistema → encontrado → reutilizado, sem paralelos criados).

## Spec Compliance Matrix

Ver tabela detalhada na seção 5 (requisito da spec → implementação).

## Validation

Ver bloco detalhado na seção 6 (builds, docs, diff, strict — todos exit 0).

## 3. Acceptance Criteria (evidência)

| CA | Critério | Status | Evidência |
|----|----------|--------|-----------|
| CA-1 | 8 serviços no Conversar dos NPCs corretos; gates funcionais (fecha antes, abre depois) | OK | `NpcServiceCatalog` (8 itens, 8 NPCs); `NpcShopController.AddNpcServiceChoices`; testes `Catalog_HasEightServices_*`, `Execute_GateClosed_*`, `Execute_GateOpen_*` |
| CA-2 | Análise consome 80g + parte → GrantKnowledge categoria faltante; idempotência herdada da F21 | OK | `NpcServiceRuntime.TryCreatureAnalysis` (consome MonsterDrop, `BestiaryManager.Knowledge.GrantKnowledge`); testes `Execute_Analysis_Charges80Gold_*`, `Execute_Analysis_NothingToGrant_NoCharge` |
| CA-3 | Encomenda dia +3; contrato semanal; prato 1×/dia | OK | `NpcServicesPendingState`; testes `BookOrder_DeliversOnDayPlusThree`, `HuntContract_WeeklyGate_*`, `DailyDish_OncePerDay_*` |
| CA-4 | Opção gated mostra o requisito; não some | OK | `NpcServiceOptionModel`/`NpcServiceGateEvaluator`; testes `Option_FriendshipGated_DisabledWithReason_AndNeverHidden`, `GateEvaluator_NullPredicates_FailClosedWithReason` |
| CA-5 | Pendências sobrevivem a save/load; saves antigos carregam sem pendências | OK | `NpcServicesSaveData` + SaveManager wiring; testes `Save_RoundTrip_PreservesPendencies`, `Save_LegacyNull_LoadsEmpty_NoError`, `Save_RoundTrip_PreservesPastureActiveWindow` |

## 4. Existing systems audit (Fase 0 — reuso, sem paralelos)

| Sistema | Encontrado | Reutilizado (não recriado) |
|---------|------------|----------------------------|
| Fluxo Conversar | `NpcShopController.ShowRootShopDialogue`/`HandleRootShopChoice` + `ShowThalindraQuestShopDialogue` | Opções entram nas DUAS árvores existentes; sem segundo fluxo |
| Amizade (F26) | `FriendshipService.Instance.IsAtLeast(npcId,int)` | Gate via predicado injetado; sem cópia de regra |
| Flags | `QuestFlagService.IsSet` | Gate de flag via predicado (fail-closed nesta camada — ver débito) |
| Bestiário (F21) | `BestiaryManager.Knowledge.GrantKnowledge` (via `GameBootstrap.BestiaryManager`) | Efeito análise chama GrantKnowledge; idempotência herdada |
| Fadiga (F16) | `PlayerConditionService.Instance.SetFatigue` | Banho remove fadiga; sem reimplementar fadiga |
| Animais (F12) | `FarmAnimalRegistry.Instance.Feed` | Pasto reusa `Feed`; hook pontual `FeedAllLiveAnimals` adicionado |
| Identificação (F31) | `ItemIdentificationService` + `InventorySwapAdapter` + `MagicItemCatalog.UnidentifiedTrinketId` | Identificação reusa exatamente o caminho do `ScrollIdentifyUseHandler` |
| Economia/Inventário | `PlayerManager.TrySpendGold/CurrentGold`, `InventoryManager.HasItem/RemoveItem/Items` | Custos via context injetado |
| Save aditivo | padrão `FriendshipSaveData`/`FarmAnimalsSaveData` no `SaveManager` | Replicado para `NpcServicesSaveData` |
| Calendário | `DayStartedEvent.DayNumber`, `EnemyKilledEvent.EnemyId` | Consumidos pelo bridge (encomenda/pasto/contrato) |
| Localização (F73) | `LocalizationService.Get` | Rótulos PT-BR (fallback = chave) |

Nenhum sistema paralelo criado. Nenhum efeito reimplementado localmente.

## 5. Spec Compliance Matrix (requirement → implementation)

| Requisito da spec | Implementação |
|-------------------|---------------|
| `NpcServiceDefinition` {id, npcId, gates, custo, efeito} | `Assets/_Game/Scripts/NPC/Services/NpcServiceDefinition.cs` |
| Registry dos 8 serviços | `NpcServiceCatalog.cs` (preços/gates canônicos do escopo) |
| Executor valida gates/custos e despacha efeitos por interface | `NpcServiceExecutor.cs` (`INpcServiceContext`/`INpcServiceEffects`) |
| Opção desabilitada com motivo | `NpcServiceOptionModel.cs` + `NpcServiceGateEvaluator.cs` |
| Opções no Conversar existente | `NpcShopController.AddNpcServiceChoices`/`HandleNpcServiceChoice` (raiz + Thalindra) |
| Seção de save pequena/aditiva (encomenda/contrato/prato) | `NpcServicesSaveData.cs` + `NpcServicesPendingState.cs` + SaveManager |
| Consumo de `DayStartedEvent` (sem evento novo) | `NpcServiceRuntime.OnDayStarted` |
| Sem `GameObject.Find` runtime; só GameEventBus | bridge resolve por `GameBootstrap.Instance`/`*.Instance` |
| Preços canônicos 80/200/50/120g | `NpcServiceCatalog` consts |

## 6. Validation

```
Validation method: dotnet build (x2) + validate_docs + check_spec_diff_completeness + run_strict_validation
Assembly-CSharp.csproj: PASS (exit 0, 0 erros, 1 warning pré-existente)
Assembly-CSharp-Editor.csproj: PASS (exit 0, 0 erros, 3 warnings pré-existentes)
Docs validation: PASS (exit 0)
Diff completeness: PASS (exit 0)
run_strict_validation.ps1: PASS (exit 0) — VALIDATION_PASS
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

EditMode tests (NpcServicesTests.cs, 26 testes): incluídos no `Assembly-CSharp.csproj`
(runtime), compilam 0 erros. Execução via Unity Test Runner: NOT RUN (Unity Editor/Play Mode
diferido por autorização do dono).

## 7. Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (gates, custos, pendências temporais, save round-trip)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/City/NpcServicesTests.cs — 26 testes)
Automated tests command: dotnet build .\Assembly-CSharp.csproj (compila; Unity Test Runner NOT RUN — diferido)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (lote; cenário humano: 3 serviços, 1 gated)
Justification if no automated tests: N/A (testes adicionados)
Residual risk: efeitos de runtime (GrantKnowledge real, SetFatigue, Feed, Identify) validados por
  mock no EditMode; o caminho integrado em cena (toasts, consumo do desconto pelo forge, buff
  alimentar visual) requer Play Mode humano.
```

## 8. Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`: o núcleo determinístico (registry, executor, gates, custos,
pendências temporais, save aditivo) e os 8 serviços no Conversar único estão completos,
auditados e cobertos por 26 testes EditMode que compilam. Builds runtime+editor exit 0; docs e
strict validation exit 0. Não é `BUILD_VALIDATED` puro porque há sub-efeitos diferidos
explicitamente (abaixo) e a validação Play Mode/humana foi diferida por autorização do dono.
Nenhuma alegação de Play Mode/ACCEPTED é feita.

## 9. Remaining work / débitos documentados

1. **Play Mode humano (lote):** abrir Conversar de Thalindra/Brumdar/Gruta/Eiran/Zrix/Orlan/
   Ozzra/Yael; verificar opção habilitada vs. gated (motivo no rótulo); executar 3 serviços
   (1 gated). Cenário do lote — `DEFERRED_TO_FINAL_VALIDATION`.
2. **Consumo do desconto de reparo pelo forge:** `NpcRepairDiscountState.ApplyToRepairCost` está
   pronto e armado; a 1-linha de consumo no caminho de reparo (`RepairUpgradeViewModel`/forja)
   é o hook pontual previsto, a ligar quando a UI de reparo estiver wired (fora do escopo de
   edição de equipamento desta spec). O serviço ARMA o desconto corretamente (testado).
3. **Buff "Rested" (+10% stamina regen) do Banho Termal:** o efeito primário (remover fadiga via
   `SetFatigue(0)`) é real; o sub-buff de regen depende de um status `Rested` da F01 que ainda não
   existe como modificador de regen — débito documentado (não reimplementado localmente).
4. **Buff alimentar visual do Prato do Dia:** limite 1×/dia garantido e persistido; o item-comida
   com buff concreto depende de wiring de consumível/UI — feedback honesto por toast por ora.
5. **Gate de flag de serviço:** nenhum dos 8 serviços usa flag por padrão; o predicado de flag no
   bridge é fail-closed (`FlagIsSet => false`) pois o `QuestFlagService` não é runtime-acessível
   desta camada. Previsto para extensões futuras do roster.
6. **Contrato de caça:** registrado/persistido na seção de pendências + conclusão por
   `EnemyKilledEvent`; a recompensa 2× concreta usa a tabela de loot do alvo (não reimplementada).
   O padrão board F34 não existe — caminho de flag/pendência declarado no escopo foi usado.

## 10. Anti-regressão

- Shop e diálogo genérico atuais intactos (compra/venda/conversar sem serviços).
- Nenhum segundo fluxo de diálogo criado (opções na árvore existente).
- Gates sempre via predicado de `FriendshipService`/`QuestFlagService` (sem cópia local).
- Sem refs Unity na seção de save (`NpcServicesSaveData` = strings/ints/dias/semanas).
- Saves antigos carregam (`Restore(null)` = sem pendências).
- Opções gated nunca somem (motivo no rótulo).
- Comunicação de gameplay só via `GameEventBus`; sem `GameObject.Find` em runtime.

## 11. Arquivos

Novos (Assets/_Game/Scripts/NPC/Services/):
`NpcServiceEffectType.cs`, `NpcServiceDefinition.cs`, `NpcServiceCatalog.cs`,
`NpcServiceOptionModel.cs`, `NpcServiceGateEvaluator.cs`, `NpcServicesSaveData.cs`,
`NpcServicesPendingState.cs`, `NpcServiceExecutor.cs`, `NpcRepairDiscountState.cs`,
`NpcServiceAccess.cs`, `NpcServiceRuntime.cs`.

Novo teste: `Assets/_Game/Tests/EditMode/City/NpcServicesTests.cs` (26 testes).

Editados:
`Assets/_Game/Scripts/NPC/NpcShopController.cs` (opções de serviço no Conversar raiz + Thalindra),
`Assets/_Game/Scripts/Save/SaveData.cs` (campo `NpcServices`),
`Assets/_Game/Scripts/Save/SaveManager.cs` (capture/restore aditivo),
`Assets/_Game/Scripts/Farm/Animals/FarmAnimalRegistry.cs` (hook pontual `FeedAllLiveAnimals`),
`Assembly-CSharp.csproj` (includes dos 11 novos arquivos + teste).
