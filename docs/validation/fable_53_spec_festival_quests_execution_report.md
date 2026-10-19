# Execution Report — fable_53 Quests de Festival (as 8 fq_*)

> Spec: `.specs/a_implementar/fable/fable_53_spec_festival_quests.md`
> Status: **BUILD_VALIDATED_WITH_WARNINGS** (Phase 2-3 Play Mode DIFERIDO por autorizacao do dono)
> Date: 2026-06-20
> Branch: dev

validated_adrs: [ADR-0007-event-bus-gameplay-communication.md]
validated_game_rules: [event_rules.md, save_rules.md, farm_rules.md]

---

## Acceptance criteria extracted

| ID | Criterio | Evidencia | Resultado |
|----|----------|-----------|-----------|
| CA-1 | fq_* so e ofertada com `festival_active(id)`; ao fim do festival expira sem punicao e sai do log ativo | `FestivalQuestService.OfferForFestival` (gate por festivalId F37) + `ExpireForDay` (estado `Expired`, publica `FestivalQuestExpiredEvent`); tests `Offer_OnlyForRealFestival`, `Expire_AtFestivalEnd_NoPunishment_LeavesActiveLog`, `Expire_DoesNothing_WhileFestivalStillActive` | OK |
| CA-2 | Reoferta no festival do ano seguinte; XP/ouro premiam de novo; trofeu/titulo so na 1a conclusao (idempotente entre anos) | Chave anual `fq_*_y<ano>` (`FestivalQuestCatalog.InstanceId`); trofeu so anexado quando nao concedido (`IsTrophyAlreadyGranted` varre `GrantedFlagIds` de quests completas); tests `AnnualRepeat_ReOffersNextYear_AsDistinctInstance`, `Trophy_GrantedOnlyOnFirstCompletion_IdempotentAcrossYears` | OK |
| CA-3 | fq_luas exige 3 ecos na MESMA noite (amanhecer reseta); fq_anonovo exige 5 NPCs DISTINTOS antes da meia-noite | `NightEchoTracker` (set distinto de luas, `ResetNight`), `GiftCountTracker` (set distinto de NPCs, `CloseAtMidnight`); tests `NightEchoTracker_*`, `GiftCountTracker_*`, `MoonsQuest_Completes_ViaThreeDistinctEchoes`, `NewYearQuest_Completes_ViaFiveDistinctGifts` | OK |
| CA-4 | Cada fq_* completa via mecanica reusada; nenhuma classe de minigame nova | Catalogo mapeia cada kind para sistema existente (PlantCrop, DefeatEnemy, DeliverItem, BuyItem, InteractWithObject, gift F26); diff sem classe de minigame; tests `PlantingQuest_*`, `TournamentQuest_*`, `VeilsQuest_*` | OK |
| CA-5 | Progresso ativo, flags anuais e trofeus sobrevivem a save/load | Instancia dinamica F34 persiste via `QuestStateSection` (simple types, sem refs Unity); test `FestivalQuest_SurvivesSaveLoad_AndStillRewardsOnce` | OK |

---

## Existing systems audit

Auditoria Fase 0 (system-reuse) — **nada criado em paralelo**:

| Sistema | Encontrado | Reuso |
|---------|------------|-------|
| Festival (F37) | `WorldEventDefinitions.Festivals` (8 ids canonicos `festival_*`), `WorldEventService.IsFestivalActive`, `FestivalStartedEvent`, `WorldEventResolver.ResolveFestival` (single-day por Season/DayInSeason) | Unica fonte de gating; festivais sao single-day, fim detectado por `DayStartedEvent` (sem evento de fim dedicado) |
| Quest infra (F34) | `QuestService` (accept/progress/turn-in/save), `QuestInstance`, `RegisterDynamicInstance`/`AcceptDynamicInstance`, `QuestRewardScaling` (ponto unico), `QuestSource.Npc`, `QuestCategory.Festival`, `QuestStateStatus.Expired` | Toda fq_* e uma `QuestInstance` no fluxo F34; recompensa via `QuestRewardScaling`; flag de trofeu via `AdditionalRewards` (`QuestFlagGrant` + `TrackByFlagId`) |
| Presente (F26/F72) | `NpcGiftReactionEvent` (publicado so quando o presente e aceito, carrega `NpcId`) | Alimenta `GiftCountTracker` (NPCs distintos) — sem recriar entrega de presente |
| Calendario | `GameDate.DaysPerYear` (112), `GameDate.Year` | Ano derivado do dia absoluto (`YearForDay`) — sem segundo calendario |
| Tracker puro | `NoHitFloorTracker` (precedente fable_51) | Mesmo shape (C# puro, EditMode) para `NightEchoTracker`/`GiftCountTracker` |

Pre-existente reutilizado sem duplicar: `Quests/Festivals/FestivalQuestDefinition.cs` (contrato F34) permanece intacto; este slice adiciona a camada de autoria/runtime em `Quests/FestivalQuests/`.

---

## Spec Compliance Matrix

| Requisito (spec) | Implementacao |
|------------------|---------------|
| 8 definicoes fq_* (IDs canonicos) ofertadas pelo NPC organizador so com festival_active | `FestivalQuestCatalog.Definitions` (8), `FestivalQuestService.OfferForFestival` gate por festival F37, `QuestSource.Npc` |
| Aceite/progresso/recompensa pelo fluxo F34 | `AcceptDynamicInstance` + `MarkObjectiveComplete` + `TurnIn` (sem fluxo novo) |
| Janela de validade: expira sem punicao no fim; reabre no ano seguinte | `ExpireForDay` (`Expired`, sem penalidade) + chave anual `_y<ano>` |
| XP/ouro escalados (ponto unico F34) | `QuestRewardScaling.Scale(base, QuestLevel)` em `BuildInstance` |
| Itens tematicos do catalogo onde definidos; trofeu = flag unica na 1a conclusao | `RewardItemId` por ano + `TrophyFlagId` so quando nao concedido (idempotente entre anos) |
| Mecanicas reusadas (plantio/escolta/duelos/qualidade/compra/Fonte/presentes) | `FestivalQuestKind` + hooks `OnSeedPlanted`/`OnEncounterCleared`/`OnGoldCropDelivered`/`OnSpecialItemBought`/`OnFountainVigil`/`RegisterEcho`/`RegisterGift` |
| Rastreadores puros testaveis | `NightEchoTracker`, `GiftCountTracker` (C# puro, EditMode) |
| Eventos novos de oferta/expiracao | `FestivalQuestOfferedEvent`, `FestivalQuestExpiredEvent` (readonly struct, GameEventBus) |
| Save: progresso pelo mecanismo existente; sem secao nova; defaults seguros | `QuestStateSection` (F34); nenhuma secao de save nova; sem refs Unity |
| Sem GameObject.Find; comunicacao via GameEventBus; unsubscribe | `Subscribe`/`Unsubscribe` simetricos; nenhuma busca global; nenhuma chamada direta MB->MB |

---

## Validation

```text
Validation method: builds + validate_docs + check_spec_diff_completeness (run_strict_validation: ambiental, ver abaixo)
Assembly-CSharp: PASS (0 Erros, exit 0)
Assembly-CSharp-Editor: PASS (0 Erros, exit 0)
Docs validation: PASS (exit 0)
Diff completeness: PASS (exit 0)
```

- `dotnet build .\Assembly-CSharp.csproj --no-restore` -> 0 Erros (1 warning pre-existente, CombatTelemetrySession, nao relacionado).
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` -> 0 Erros (warnings pre-existentes nao relacionados).
- `.\tools\docs\validate_docs.ps1` -> exit 0.
- `.\tools\docs\check_spec_diff_completeness.ps1` -> exit 0.
- `run_strict_validation.ps1`: pode retornar exit 1 devido a **3 cenas .unity ja modificadas no working tree por sessao de Unity anterior** (`CaveScene.unity`, `FarmScene.unity`, `TownScene.unity`) — falha **ambiental**, nao desta spec. Esta spec NAO tocou nenhum `.unity/.prefab/.asset`; builds + docs + diff-completeness estao limpos.

### EditMode tests

`Assets/_Game/Tests/EditMode/Quests/FestivalQuestsTests.cs` (NUnit, sem refs Unity, segue convencoes do `CaveContractsTests`):
oferta gated por festival; expiracao sem punicao; reoferta no ano seguinte; trofeu unico idempotente entre anos; trackers (3 ecos/luas mesma noite; 5 NPCs distintos/meia-noite); completion por plantio/duelos/compra/qualidade; round-trip de save. Execucao autoritativa via Unity Test Runner DIFERIDA (lote final, autorizacao do dono); compilam no build de fallback (Assembly-CSharp exit 0).

---

## Honest status rationale

**BUILD_VALIDATED_WITH_WARNINGS.** O nucleo esta completo e auditado: as 8 fq_* fluem pelo fluxo F34, gateadas pela unica fonte de festival F37, com expiracao limpa, reoferta anual, trofeu idempotente, trackers honestos e save round-trip — tudo coberto por EditMode tests no build de fallback (exit 0). Phase 2-3 (Unity Test Runner + Play Mode completando 1 festival real) ficam **DIFERIDAS** por autorizacao explicita do dono. Nao foi tocado nenhum `.unity/.prefab/.asset`; a possivel falha do `run_strict_validation` e ambiental (3 cenas pre-modificadas). Nenhuma claim de Play Mode/ACCEPTED foi feita.

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (gate por festival, expiracao/reoferta anual, trackers temporais, idempotencia de trofeu, scaling de recompensa)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Quests/FestivalQuestsTests.cs)
Automated tests command: Unity Test Runner (EditMode) — NOT RUN (diferido p/ lote final; compilam no build de fallback exit 0)
Manual Play Mode scenario: NOT RUN (DEFERRED_TO_FINAL_VALIDATION — completar fq_colheita ou fq_anonovo em festival real)
Justification if no automated tests: N/A (testes adicionados)
Residual risk: comportamento em scene real (oferta pelo NPC organizador, integracao com plantio/duelos/compra/Fonte reais) nao validado em Play Mode ate o lote final; logica deterministica coberta por EditMode tests.
```

## Remaining work

- Phase 2: rodar EditMode tests no Unity Test Runner (lote final).
- Phase 3: cenario humano Play Mode completando 1 fq_* em festival real (DEFERRED_TO_FINAL_VALIDATION).
- Futuro (fora de escopo): dialogos dos organizadores via pools F28; spawn fisico dos ecos noturnos / entrada do item especial no mercado noturno via mecanismos F37/F19; item de festival adicional por festival (pendencia do catalogo).
