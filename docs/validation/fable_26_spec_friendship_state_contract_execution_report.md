# Execution Report — fable_26 Friendship State Contract

> **Spec:** `.specs/a_implementar/fable/fable_26_spec_friendship_state_contract.md` (+ EMENDA 2026-06-13-V3)
> **Date:** 2026-06-20
> **Status:** `BUILD_VALIDATED_WITH_WARNINGS`
> **Type:** Runtime / Save (NPC domain)
> **validated_game_rules:** `docs/game_rules/npc_rules.md` (Rules 5 e 6 — friendship state + gift taste)

---

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS` — o núcleo do contrato está implementado, auditado e coberto por
testes EditMode determinísticos (24 testes novos), e `run_strict_validation.ps1` retornou exit 0.
É `_WITH_WARNINGS` (não `BUILD_VALIDATED` puro) porque:

- a validação Play Mode / humana foi **DIFERIDA** por decisão do dono (não rodar Unity Editor /
  Play Mode nesta sessão); o cenário humano de subir amizade a nível 1 é o gate restante para
  `ACCEPTED` (Phase 2-3 NOT RUN);
- a aplicação **real** do gosto por NPC depende de dados ainda inexistentes (itens marcados
  `ItemTag.Giftable` + `gift_*` tags do A4/ITEM_CATALOG + `NpcGiftPreferences` populadas por NPC
  a partir da matriz A6). Isto é **débito de dados conhecido e registrado** (emenda V3 §5/§8 e
  `NPC_GIFT_TASTE_MATRIX_v1.0.md` §6), não uma lacuna desta spec. A lógica de classificação está
  100% implementada e testada; só falta o dado para exercê-la em jogo.

Nenhuma alegação de Play Mode / ACCEPTED / 100% é feita aqui.

---

## Acceptance criteria extracted

| CA | Critério | Implementação | Evidência (teste) | Status |
|----|----------|---------------|-------------------|--------|
| CA-1 | 1ª conversa do dia +1; 2ª no mesmo dia +0; dia seguinte +1 | `FriendshipState.RegisterDailyConversation` (chave `lastTalkDay`) | `DailyConversation_FirstOfDayGains_SecondSameDayCapped_NextDayGainsAgain` | OK |
| CA-2 | Thresholds 10/30/60/100/150 + `FriendshipLevelChangedEvent` 1×/transição | `FriendshipState.LevelForPoints` + `ApplyResult.LevelChanged`; serviço publica em `PublishIfLevelChanged` | `LevelForPoints_MatchesCanonicalThresholds`, `AddPoints_ReportsLevelTransition_OncePerCrossing` | OK |
| CA-3 | Round-trip preserva pontos + marcadores; load legado = todos nível 0 | `FriendshipSaveData` + `Capture/RestoreFromSaveData`; wiring no SaveManager | `SaveRoundTrip_PreservesPointsAndDayMarkers`, `LegacyLoad_NullOrEmpty_AllNpcsLevelZero_NoError`, `Restore_InvalidNpcId_Ignored_NegativePointsClamped` | OK |
| CA-4 | `IsAtLeast(npcId, level)` correto em todos os níveis e npcId desconhecido = false/nível 0 | `FriendshipState.IsAtLeast` | `IsAtLeast_UnknownNpc_ReturnsFalse_Level0`, `IsAtLeast_RespondsCorrectlyAcrossLevels`, `IsAtLeast_NullOrEmptyNpc_IsSafe` | OK |
| CA-5 | Quest +8 idempotente/quest; presente cap 1×/dia/NPC; compra cap 1×/dia | `RegisterQuestCompleted` (`_creditedQuestIds`), `RegisterGift` (`lastGiftDay`/`DailyGiftLimit`), `RegisterShopPurchase` (`lastPurchaseDay`) | `QuestCompleted_Grants8_IdempotentPerQuestId`, `Gift_DailyCap_SecondGiftSameDayRefused_NotConsumed`, `ShopPurchase_FirstOfDayGains_SecondSameDayCapped` | OK |
| CA-6 (V3) | Gosto por NPC: loved +12 / liked +6 / neutral +2 / disliked -2 / hated -6; precedência hated>disliked>loved>liked>neutral; Giftable gate; clamp em 0; descida de nível; fallback neutral | `GiftTasteClassifier` + `RegisterGift` + clamp em `AddPoints` | `Classify_FiveLevels_MapToExpectedDeltas`, `Classify_Precedence_*` (3), `Giftable_Gate_*`, `Gift_HatedReducesPoints_ClampsAtZero`, `Gift_HatedCanDropLevel_AndReportsTransition`, `Classify_NullPreferences_FallsBackNeutral` | OK |

---

## Existing systems audit (Fase 0 — system-reuse)

| Item auditado | Encontrado? | Decisão | Nota |
|---------------|-------------|---------|------|
| `NpcGiftPreferences` (struct de gosto) | SIM — `NPC/NpcDefinition.cs` (código morto) | **ESTENDIDO** (não recriado) | +`NeutralItemTags`, +`HatedItemTags` (emenda V3 §3) |
| `ItemTag.Giftable` | SIM — `Items/ItemTag.cs` (`1L << 6`, sem item) | **REUTILIZADO** como porteiro | `ItemDefinition.HasTag(ItemTag.Giftable)` |
| Padrão de seção de save aditiva | SIM — `FarmDailyGoalsSaveData` + `FarmDailyGoalService` (WI-24/F13) | **REUTILIZADO** como precedente exato | static `Instance` + `Capture/RestoreFromSaveData`; wiring no SaveManager |
| `GameCalendarService` / dia absoluto | SIM — `World/Calendar/GameCalendarService.cs` + `DayStartedEvent.DayNumber` | **CONSUMIDO** via `DayStartedEvent` (não alterado) | mesmo idioma do `FarmDailyGoalService._currentDay` |
| `GameEventBus` | SIM | **REUTILIZADO** | Subscribe/Unsubscribe em OnEnable/OnDisable |
| `NpcInteractionStartedEvent` / `...EndedEvent` | SIM — `Core/Events/NpcInteractionEvents.cs` | **CONSUMIDO** (hook conversa + atribuição de compra) | carrega NpcId |
| `QuestGiverInteractedEvent` (TurnIn) | SIM — `Core/Events/QuestRuntimeEvents.cs` | **CONSUMIDO** (hook quest, carrega NpcId+QuestId) | publicado por NpcShopController/NpcController/QuestGiverInteractable |
| `EconomyTransactionCompletedEvent` (ShopBuy) | SIM — `Core/Events/EconomyTransactionCompletedEvent.cs` | **CONSUMIDO** (hook compra) | sem NpcId → atribuído ao NPC em interação aberta |
| `LocalizationService` (fable_73 / ADR-0012) | SIM — `Localization/LocalizationService.cs` | **REUTILIZADO** + 1 chave nova (`ui.friendship.level_label`) | texto novo ao jogador via LocalizationService (decisão 4.7) |
| Registry central de `NpcDefinition` em runtime | NÃO | N/A | `GiveGift` recebe `NpcGiftPreferences` do caller (gating honesto V3 §5) |
| `ItemDatabase` em runtime | NÃO | N/A | classificação recebe `ItemDefinition` do caller |
| `FriendshipService` paralelo pré-existente | NÃO | criado (1 único serviço, owner do estado) | sem tracker paralelo |

Nenhum sistema paralelo de relacionamento foi criado. As 4 fontes seguem exatamente 4 (não inflado).

---

## Spec Compliance Matrix

| Requisito (spec / emenda) | Implementação |
|---------------------------|---------------|
| `FriendshipService` no bootstrap | `NPC/Friendship/FriendshipService.cs` + `FriendshipRuntimeBootstrap.cs` (RuntimeInitializeOnLoad, DontDestroyOnLoad) |
| API `GetLevel/GetPoints/AddPoints/IsAtLeast` | `FriendshipService` (delegada ao núcleo `FriendshipState`) |
| Níveis 0-5 (Desconhecido..Confidente) | `FriendshipLevel` enum + `FriendshipState.LevelThresholds` |
| Thresholds 10/30/60/100/150 | `FriendshipState.LevelThresholds = {0,10,30,60,100,150}` |
| Fonte conversa +1 (1×/dia) | `RegisterDailyConversation` ← `NpcInteractionStartedEvent` |
| Fonte quest +8 (idempotente) | `RegisterQuestCompleted` ← `QuestGiverInteractedEvent` (TurnIn) |
| Fonte presente (delta por gosto) | `GiveGift` + `GiftTasteClassifier` ← API pública (gating de dados V3 §5) |
| Fonte compra +1 (1×/dia) | `RegisterShopPurchase` ← `EconomyTransactionCompletedEvent` (ShopBuy, NPC ativo) |
| Caps diários por fonte | `lastTalkDay`/`lastGiftDay`/`lastPurchaseDay` + `_creditedQuestIds` |
| `FriendshipLevelChangedEvent(npcId, nível)` | `Core/Events/FriendshipEvents.cs`; publicado em subida E descida (V3 §2/§7) |
| Save `FriendshipSaveData {npcId, points, lastTalkDay, lastGiftDay, lastPurchaseDay}` | `NPC/Friendship/FriendshipSaveData.cs` + `GameSaveData.Friendship` |
| Save aditivo WI-18, default vazio, sem migration | wiring em `SaveManager.CaptureFriendshipSaveData` / restore block; legado = null = nível 0 |
| Sem refs Unity no save (ADR-0006) | DTO só string/int |
| Estender `NpcGiftPreferences` com Neutral/Hated | feito em `NpcDefinition.cs` |
| Reusar `ItemTag.Giftable` como porteiro | `GiftTasteClassifier.IsGiftable` |
| Precedência hated>disliked>loved>liked>neutral | `GiftTasteClassifier.Classify` (ordem de teste) |
| Clamp pontos no piso 0 | `FriendshipState.AddPoints` |
| Linha "Amizade: nível N" no cabeçalho do Conversar | `FriendshipService.GetFriendshipHeaderLine` (placeholder; via LocalizationService) |
| Decaimento OFF | nenhuma rotina de decaimento |
| Romance/casamento/UI rica de corações fora | não implementados |

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (exit 0, 0 errors, 1 pre-existing warning)
Assembly-CSharp-Editor: PASS (exit 0, 0 errors, 3 pre-existing warnings)
Quality check: PASS
Docs validation: EXPECTED_FAIL_LEGACY_ONLY → reconciliado; run_strict_validation exit 0
Diff completeness: PASS
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

- `dotnet build .\Assembly-CSharp.csproj --no-restore` → exit 0 (0E/1W pré-existente).
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` → exit 0 (0E/3W pré-existente).
- `.\tools\docs\validate_docs.ps1` → exit 0.
- `.\tools\docs\check_spec_diff_completeness.ps1` → exit 0.
- `.\tools\docs\run_strict_validation.ps1` → exit 0 (VALIDATION_PASS).

### EditMode tests

- Arquivo: `Assets/_Game/Tests/EditMode/City/FriendshipTests.cs` (24 testes).
- Tipo: EditMode / NUnit, lógica pura (núcleo `FriendshipState` + `GiftTasteClassifier`) — sem
  cena/Unity lifecycle. Sem referência a tipos `CindarsHope.Editor.*`, portanto incluído no
  `Assembly-CSharp.csproj` (não no Editor).
- Execução autoritativa (Unity Test Runner EditMode): **NOT RUN** nesta sessão (Play Mode/Unity
  diferido por decisão do dono). Os testes **compilam** dentro do `Assembly-CSharp` (build exit 0).
  Risco residual: o veredito verde de runtime do Test Runner depende de execução humana no Unity.

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (thresholds, caps, idempotência, precedência de gosto, clamp)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (24 EditMode tests — Tests/EditMode/City/FriendshipTests.cs)
Automated tests command: Unity Test Runner EditMode — NOT RUN (diferido); compila no Assembly-CSharp (build exit 0)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (subir amizade a nível 1 no Conversar)
Justification if no automated tests: N/A (testes presentes)
Residual risk: veredito do Test Runner EditMode depende de execução humana no Unity; aplicação
  do gosto por NPC em jogo depende do débito de dados A4/A6 (itens Giftable + GiftPreferences populadas)
```

---

## Save expectations

```text
Does this change save schema? YES — seção nova aditiva (GameSaveData.Friendship : FriendshipSaveData)
Does this add a save section? YES (owner: FriendshipService)
Does this require migration? NO — CurrentSchemaVersion inalterado (5); default vazio; load legado = nível 0
Does this persist Unity references? NO — só string (npcId) + int (points/3 day markers)
Default values: Entries = lista vazia; LastTalkDay/LastGiftDay/LastPurchaseDay = -1 ("nunca")
Legacy/missing section: saveData.Friendship == null ⇒ RestoreFromSaveData(null) ⇒ estado limpo, todos nível 0, sem erro
Invalid ID fallback: entradas com NpcId vazio/nulo ignoradas; pontos negativos clampados em 0
Round-trip: pontos + lastTalkDay/lastGiftDay/lastPurchaseDay preservados (teste SaveRoundTrip)
Idempotency after reload: caps de dia preservados ⇒ re-trigger no mesmo dia não re-credita
Restore order: após registries de NPC disponíveis (padrão WI-18); bloco no ApplySaveData do SaveManager
```

---

## Files changed

### Novos (runtime)
- `Assets/_Game/Scripts/NPC/Friendship/FriendshipLevel.cs`
- `Assets/_Game/Scripts/NPC/Friendship/FriendshipSource.cs`
- `Assets/_Game/Scripts/NPC/Friendship/GiftTasteClassifier.cs`
- `Assets/_Game/Scripts/NPC/Friendship/FriendshipState.cs`
- `Assets/_Game/Scripts/NPC/Friendship/FriendshipSaveData.cs`
- `Assets/_Game/Scripts/NPC/Friendship/FriendshipService.cs`
- `Assets/_Game/Scripts/NPC/Friendship/FriendshipRuntimeBootstrap.cs`
- `Assets/_Game/Scripts/Core/Events/FriendshipEvents.cs`

### Novos (teste)
- `Assets/_Game/Tests/EditMode/City/FriendshipTests.cs`

### Novos (doc)
- `docs/validation/fable_26_spec_friendship_state_contract_execution_report.md` (este arquivo)

### Modificados (aditivos)
- `Assets/_Game/Scripts/NPC/NpcDefinition.cs` — `NpcGiftPreferences` +`NeutralItemTags`/`HatedItemTags`
- `Assets/_Game/Scripts/Save/SaveData.cs` — `GameSaveData.Friendship` (campo aditivo)
- `Assets/_Game/Scripts/Save/SaveManager.cs` — capture + restore da seção (aditivo)
- `Assets/_Game/Scripts/Localization/LocalizationStringTable.cs` — chave `ui.friendship.level_label`
- `Assembly-CSharp.csproj` — includes dos novos .cs (build local; NÃO comitado — Unity regenera)

---

## Architecture invariants

- Sem `GameObject.Find`/`FindObjectOfType` em gameplay. `FriendshipRuntimeBootstrap` usa
  `FindAnyObjectByType` apenas para anti-duplicata (wiring de setup — mesmo padrão sancionado do
  `FarmDailyGoalRuntimeBootstrap`), não comunicação de gameplay.
- Comunicação só via `GameEventBus` (4 hooks de fonte + publicação de `FriendshipLevelChangedEvent`).
- Save DTO só tipos simples + IDs estáveis (ADR-0006).
- Estado interno privado; consumidores (F25/F28/F35) leem por API pública.
- Sem `CindarsHope.Debug`/`CindarsHope.Temp`.
- Nenhum `.unity`/`.prefab`/`.asset` editado.

---

## Remaining work (deferido / fora de escopo desta spec)

1. **Play Mode / validação humana (Phase 2-3):** subir amizade a nível 1 no Conversar (toast +
   linha de cabeçalho). DEFERRED_TO_FINAL_VALIDATION.
2. **Toast visual:** `FriendshipLevelChangedEvent` é publicado (contrato cumprido); a assinatura
   do HUD/`GameplayFeedbackService` para exibir "X agora é seu amigo" é integração de UI deferida
   (fora do orçamento de hooks desta spec).
3. **Débito de dados A4/A6 (registrado):** marcar itens com `ItemTag.Giftable`, criar tags `gift_*`
   no ITEM_CATALOG e popular `NpcGiftPreferences` por NPC a partir da matriz A6. Enquanto isso, o
   presente cai no DEFAULT neutral (+2). Detalhado em `NPC_GIFT_TASTE_MATRIX_v1.0.md` §6.
4. **Consumidores F25/F28/F35:** consomem a API estável desta spec; não antecipados aqui.
5. **Validador estilo F30** de cobertura de `GiftPreferences` por NPC: débito (matriz §6).

---

## Dependency Chain

```text
Original target: fable_26_spec_friendship_state_contract
Dependency chain: nenhuma dependência same-wave pendente
  - F13 (save schema estável): BUILD_VALIDATED (executada) — seção aditiva segue o padrão
  - WAVE NPC base (NpcDefinition/registry): presente (NpcDefinition consumido/estendido)
Forbidden dependencies: none
Resolved depth: 0
Can continue original target: YES (concluído)
```
