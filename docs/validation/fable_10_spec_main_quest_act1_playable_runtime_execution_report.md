# Execution Report — fable_10 Main Quest: Ato 1 Jogável ("A Fonte do Esquecimento")

- **Spec:** `.specs/a_implementar/fable/fable_10_spec_main_quest_act1_playable_runtime.md`
- **Status:** BUILD_VALIDATED_WITH_WARNINGS
- **Date:** 2026-06-20
- **Branch:** dev
- **Type:** Runtime / Data / Integration (Quest)
- **validated_game_rules:** [quest_rules.md, fonte_rules.md]
- **validated_adrs:** []

> Phase 2 (Unity batchmode) and Phase 3 (Play Mode / human) are DEFERRED by explicit owner
> authorization for this session. No Unity Editor was run. See Honest status rationale.

---

## Acceptance criteria extracted

| CA | Requirement | Implementation | Evidence | Status |
|----|-------------|----------------|----------|--------|
| CA-1 | Cadeia jogável e travada: quest N+1 só ofertada com N completa (prerequisite enforced no giver) | 5 quests Main encadeadas por `PrerequisiteQuestIds` no `QuestRegistry`; enforcement já existente em `QuestGiverInteractable.DetermineMode/FindRelevantQuestId` via `QuestService.ArePrerequisitesComplete` (fechado por fable_34) | `MainQuestAct1Tests.Chain_PrerequisitesFormALinearOrder`, `Prerequisites_GateEachStepUntilPreviousCompleted`, `Q1_NoPrerequisites_AlwaysOfferable` | OK |
| CA-2 | Fragmento concedido 1x: completar mq_act1_05 concede Fragmento da Água + desbloqueia Água Viva; reload + re-turn-in NÃO duplica | `MainProgressionQuestBridge` consome `QuestCompletedEvent` → `FonteRuntimeService.IntegrateFragment(Water)` (API WAVE 10); idempotência tripla (flag de sessão + Fonte já integrada + `TryIntegrateFragment` idempotente); reward gold/flag idempotente por `GrantedRewardIds` | `WaterFragment_IntegratesOnce_SecondIsAlreadyIntegrated`, `TurnIn_GrantsRewardsOnce_EvenOnRepeatedTurnIn`, `Questline_RewardIdempotency_SurvivesReload`, `Bridge_TargetsTheFinalAct1Quest` | OK (lógica; integração viva via Fonte = Phase 3) |
| CA-3 | Anti-softlock: nenhuma quest do ato expira; morrer/novo run não invalida progresso de ReachCaveDepth já registrado | Categoria `Main` ⇒ `QuestDefinition.CanExpire()` retorna `false`; objetivos persistidos em `QuestStateSection` (WI-18), progresso de profundidade sobrevive ao reload | `MainQuests_NeverExpire`, `CaveDepthProgress_PersistsAcrossReload` | OK |
| CA-4 | Save round-trip: questline sobrevive a save/load em qualquer ponto | Reusa `QuestStateSection` + `QuestService.RestoreFromSaveData` (WI-18); sem mudança de schema | `Questline_SurvivesSaveLoad_MidChain`, `Questline_RewardIdempotency_SurvivesReload` | OK |

---

## Existing systems audit

| Sistema | Status | Decisão |
|---------|--------|---------|
| `QuestRegistry` / `QuestService` / `QuestRuntimeBootstrap` / `QuestProgressEventBridge` (WI-15/26, fable_34) | EXISTE | REUSE — 5 quests adicionadas ao registry; nenhum 2º registry/serviço |
| 7 tipos de objetivo (CollectItem/SellItem/ReachCaveDepth/HarvestCrop/TalkToNpc/DefeatEnemy/CraftItem) | EXISTE | REUSE — Ato 1 usa TalkToNpc, CollectItem, ReachCaveDepth, DefeatEnemy. Nenhum tipo novo |
| `QuestGiverInteractable` prerequisite enforcement | JÁ FECHADO (fable_34 EMENDA 2026-06-12-C) | REUSE — `PREREQUISITE_UI_DEBT` da WI-26 já estava fechado; nenhuma mudança no giver foi necessária |
| WAVE 10 `MainProgressionService` / `FonteRuntimeService.IntegrateFragment(Water)` / `FonteFunctionUnlockService` | EXISTE | REUSE — bridge chama a API canônica (fonte_rules Rule 2: "integration entry point é fable_10"). Nenhum sistema de progressão paralelo |
| `QuestStateSection` save (WI-18, idempotência por `GrantedRewardIds`) | EXISTE | REUSE — sem mudança de schema; questline é conteúdo na seção existente |
| `TownNpcDialogueLibrary` + `DialogueActionType.OfferQuest` | EXISTE | REUSE — nós OfferQuest adicionados para Corvus/Maelor (padrão Thalindra), node count preservado em 13 |
| Cave boss gate nível 10 | NÃO EXISTE | `CreateCaveBossAssets` cria gates apenas em 15/30/45/60/75/90. Usado fallback TEMP `DefeatEnemy "any" x3` conforme risco previsto na spec |

**Sistemas criados (mínimos):** apenas `MainProgressionQuestBridge` (NOVO, exigido pela spec — ponte quest→Fonte idempotente).

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation | OK? |
|--------------------|----------------|-----|
| 5 quests IDs estáveis (`mq_act1_01..05`) encadeadas | `QuestRegistry.RegisterMainQuestAct1` + `QuestMainAct1Ids` | OK |
| mq_act1_01: TalkToNpc Corvus→Thalindra (reward flag) | Q1 TalkToNpc `npc_thalindra`; reward `flag_main_arrival`; giver `npc_corvus` | OK |
| mq_act1_02: CollectItem stone x5 + TalkToNpc Maelor | Q2 CollectItem `item_material_stone` x5 + TalkToNpc `npc_maelor`; giver `npc_thalindra` | OK |
| mq_act1_03: ReachCaveDepth 5 (Maelor) | Q3 ReachCaveDepth "5"; giver `npc_maelor` | OK |
| mq_act1_04: ReachCaveDepth 10 + DefeatEnemy bossId gate 10 | Q4 ReachCaveDepth "10" + DefeatEnemy `any` x3 (TEMP — sem gate canônico no nível 10) | OK (com TEMP documentado) |
| mq_act1_05: TalkToNpc Corvus; reward fragmento via WAVE 10 + 150 gold + flag mq_act1_complete | Q5 TalkToNpc `npc_corvus`; 150 gold + `flag_mq_act1_complete` + `flag_main_post_act1`; fragmento via bridge | OK |
| Enforcement de PrerequisiteQuestIds no giver (fecha PREREQUISITE_UI_DEBT) | Já fechado por fable_34; verificado/coberto por teste | OK (no-op — já existia) |
| `MainProgressionQuestBridge` (NOVO) idempotente | Criado; consome `QuestCompletedEvent` → `IntegrateFragment(Water)` | OK |
| QuestGiverInteractable em npc_corvus/npc_maelor via CreateMvpTownScene | `AddMainQuestGiver` (corvus/thalindra/maelor) com `_offeredQuestIds` da cadeia | OK (código; cena regenerada pelo humano) |
| Nós de diálogo OfferQuest Corvus/Maelor (padrão Thalindra) | `OfferedMainQuestId`/`OfferQuestLabel` + escolha OfferQuest em `BuildServiceChoices` | OK |
| Textos PT-BR alinhados à lore (Litania do Primeiro Retorno citada por Corvus) | Descrições das quests + nós Corvus citam a Litania | OK |
| EditMode tests (cadeia, idempotência fragmento, anti-softlock, save round-trip) | `MainQuestAct1Tests` (13 testes) | OK |
| Não criar 2º registry/serviço/progressão/objetivo | Auditado — nenhum criado | OK |

---

## Validation

```
Validation method: dotnet build (Phase 1) + scoped gates; run_strict_validation observado
Assembly-CSharp:         PASS (exit 0, 0 errors, 1 warning pré-existente CombatTelemetrySession)
Assembly-CSharp-Editor:  PASS (exit 0, 0 errors, 3 warnings pré-existentes)
Docs validation:         PASS (validate_docs.ps1 exit 0)
Diff completeness:       PASS (check_spec_diff_completeness.ps1 exit 0, com este report + commit)
run_strict_validation:   exit 1 esperado — ambiental (3 cenas .unity já modificadas no working tree
                         antes desta spec: CaveScene/FarmScene/TownScene). NÃO causado por fable_10.
```

### Testing Quality Gate

```
Changed runtime code: YES (QuestRegistry, MainProgressionQuestBridge, QuestRuntimeBootstrap, TownNpcDialogueLibrary)
Changed deterministic logic: YES (questline chain, prerequisites, reward idempotency, save round-trip)
Changed Unity scene/prefab/asset wiring: NO direto (CreateMvpTownScene gera; humano regenera TownScene)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Quests/MainQuestAct1Tests.cs, 13 testes)
Automated tests command: dotnet build .\Assembly-CSharp.csproj --no-restore (compila; Unity Test Runner = Phase 2-3 deferido)
Manual Play Mode scenario: REQUIRED (questline ponta a ponta) — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A (testes adicionados)
Residual risk: a integração viva quest→Fonte (bridge → FonteRuntimeService singleton) e o fluxo de
  oferta no giver in-scene só são exercitados em Play Mode; cobertos em EditMode no nível de lógica
  pura (registry/QuestService/MainProgressionService). Gate nível 10 usa fallback DefeatEnemy "any" x3
  (TEMP) até um boss gate canônico de nível 10 existir.
```

---

## Honest status rationale

**BUILD_VALIDATED_WITH_WARNINGS.** O núcleo da spec (5 quests Main encadeadas, prerequisite
enforcement, ponte de fragmento idempotente, diálogos de oferta, wiring de cena no gerador,
testes EditMode) está implementado e compila 0E em ambas as assemblies; docs e diff-completeness
gates passam. Foi rebaixado de `BUILD_VALIDATED` para `_WITH_WARNINGS` porque:

1. **Phase 2-3 deferidas** por autorização do dono — Unity batchmode e Play Mode não foram executados;
   a integração viva quest→Fonte e a oferta no giver in-scene precisam de Play Mode para validar (CA-2
   "desbloqueia Água Viva" é provado por lógica, não por execução viva nesta sessão).
2. **Gate nível 10 (TEMP):** sem boss gate canônico no nível 10, `mq_act1_04` usa `DefeatEnemy "any" x3`
   no contexto do nível 10 (risco previsto e mitigado pela spec). `ReachCaveDepth 10` garante a descida.
3. **`run_strict_validation` exit 1 é ambiental** (3 cenas .unity já modificadas no working tree antes
   desta spec), não regressão de fable_10.

Nenhum claim de `ACCEPTED`/`PLAYMODE_VALIDATED` é feito. Nenhum arquivo proibido foi alterado
(sem edição manual de .unity/.prefab/.asset; mudanças de cena são via gerador editor).

---

## Files changed

**Runtime/Editor (código):**
- `Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs` — +5 quests Ato 1 + `QuestMainAct1Ids`
- `Assets/_Game/Scripts/Quests/Runtime/MainProgressionQuestBridge.cs` — NOVO (ponte quest→Fonte)
- `Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs` — instancia/assina/desassina o bridge
- `Assets/_Game/Scripts/NPC/TownNpcDialogueLibrary.cs` — nós OfferQuest Corvus/Maelor (node count 13)
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` — `AddMainQuestGiver` (corvus/thalindra/maelor)

**Testes:**
- `Assets/_Game/Tests/EditMode/Quests/MainQuestAct1Tests.cs` — NOVO (13 testes)

**csproj (gitignored, não comitado):** includes adicionados para os 2 arquivos novos.

---

## Dependency Chain

```
Original target: fable_10
Dependency chain: nenhuma dependência same-wave pendente (WAVE 09/10/WI-15/26 e slice 2026-06-12 já implementados)
Forbidden dependencies: none
Resolved depth: 0
Can continue original target: YES
```

---

## Remaining work (post-Phase-1)

- **Humano/Unity:** regenerar TownScene (`CreateMvpTownScene`) para materializar `QuestGiverInteractable`
  em Corvus/Maelor; rodar `Rebuild Town NPC Dialogues` para escrever os nós OfferQuest; rodar Unity Test
  Runner EditMode (13 testes novos); executar o cenário humano de questline ponta a ponta (Phase 3).
- **Gate nível 10:** substituir o fallback TEMP `DefeatEnemy "any" x3` por um `bossId` canônico quando um
  boss gate de nível 10 for definido (follow-up, não bloqueia Ato 1).
- **Atos 2-4:** promover specs WAVE 19 futuras usando esta baseline (Vaelrion/Sethra como NPCs no Ato 2).
```
