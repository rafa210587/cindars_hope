# Execution Report — fable_57 (Cidade Viva: Aniversários + Cama da Estalagem + ADR de Reputação)

> Spec: `.specs/a_implementar/fable/fable_57_spec_living_city_birthdays_inn_reputation_adr.md`
> Status: **BUILD_VALIDATED_WITH_WARNINGS** (Play Mode / human validation DEFERRED_TO_FINAL_VALIDATION, autorizado pelo dono)
> Date: 2026-06-20 · Branch: `dev`

---

## Honest status rationale

Núcleo determinístico (tabela de aniversário, multiplicador de presente ×2, resolver de
cobrança da diária, projection do calendário) implementado, auditado e coberto por EditMode
tests. Reusou os sistemas existentes (FriendshipService/F26, BedInteractable→PlayerConditionService/F16,
CalendarDayDetailModel/F37-F20, CityServiceAccess→PlayerManager) sem criar paralelos. Nenhum
sistema de reputação foi criado; o ADR-0017 documenta a absorção (status `proposed`, aval humano
pendente). A cama de hóspede entra na cena VIA GERADOR (`CreateMvpTownScene.cs`), portanto a
verificação visual/Play Mode (dormir na cidade, toast de aniversário) fica para o lote final —
`BUILD_VALIDATED_WITH_WARNINGS` é o teto honesto. Nenhum campo de save novo.

---

## Acceptance criteria extracted

| CA | Critério | Evidência | Status |
|----|----------|-----------|--------|
| CA-1 | 23 NPCs com data estável; projection expõe o aniversário do dia | `NpcBirthdayTable` (23/23, máx 1/dia, sem colisão com festival), `NpcBirthdayService`, `CalendarBirthdayProjectionBuilder` + campo `CalendarDayDetailModel.Birthdays`; testes `BirthdayTable_*`, `BirthdayService_*`, `CalendarProjection_*` | OK |
| CA-2 | Presente no aniversário ×2; cap 1/dia/NPC preservado; fora do aniversário normal | `FriendshipState.RegisterGift(..., pointsMultiplier)` (multiplicador APÓS o cap) + `FriendshipService.GiveGift` (ponto único); testes `RegisterGift_BirthdayMultiplier_DoublesDelta`, `..._DoesNotBreakDailyCap`, `GiftMultiplier_DoublesPointsOnlyOnBirthday`, `RegisterGift_NoMultiplier_KeepsBaselineF26Behavior` | OK |
| CA-3 | Cama cobra a diária 1× e dorme (fluxo F16); sem ouro recusa sem débito | `InnLodgingPaymentResolver` (puro) + `InnBedPaymentGate` (IInteractable → `PlayerConditionService.SleepInBed`); testes `InnPayment_WithEnoughGold_DebitsOnce`, `InnPayment_WithoutGold_RefusesAndDoesNotDebit`, `InnPayment_InvalidConfig_*`, `InnPayment_SpendFailureTreatedAsRefusal_*`; cenário humano (lote final) | OK (Play Mode deferido) |
| CA-4 | ADR de reputação aprovado, formato do projeto, sem código de reputação | `docs/decisions/ADR-0017-reputation-absorbed-by-friendship.md` (status `proposed`); diff sem `ReputationService`/`ReputationState`; teste `NoParallelReputationTracker_*` | OK (proposed, aval humano) |

---

## Existing systems audit (Fase 0)

| Sistema | Achado | Decisão |
|---------|--------|---------|
| FriendshipService / FriendshipState (F26) | `GiveGift`/`RegisterGift` = ponto único de ganho por presente; cap diário por `LastGiftDay`; `GiftGivingService` (F72) delega via `FriendshipGiftAdapter` | REUSADO — multiplicador ×2 aditivo dentro de `RegisterGift` (após o cap) |
| BedInteractable (F16) | dormir = `PlayerConditionService.Instance.SleepInBed()` (day transition + recuperação) | REUSADO — `InnBedPaymentGate` delega ao mesmo fluxo (NÃO modifica BedInteractable) |
| GameCalendarService / GameDate | ano 112 dias, 28/estação; `DayInSeason`, `CurrentSeason`, `DayInYear` | REUSADO — tabela usa o mesmo cálculo; serviço de aniversário é PURO (recebe `GameDate`) |
| Festivais (FestivalCalendar/FestivalRegistry) | dias-no-ano 14/56/98 | Tabela DESVIA desses dias (risco de colisão mitigado; teste de unicidade) |
| CalendarDayDetailModel (F37/F20) | DTO de projection com listas `Festivals`/`KnownEventsText` | REUSADO — campo aditivo `Birthdays` + builder puro |
| EconomyManager / PlayerManager | `TrySpendGold`/`CurrentGold`; sem singleton de PlayerManager | Cobrança via `CityServiceAccess.SpendGoldFunc/CurrentGoldFunc` (já ligados ao PlayerManager por `CityServiceRuntimeBootstrap`) — sem busca global |
| NpcTownRosterRegistry | 23 IDs canônicos | Fonte dos IDs da tabela + nomes de exibição |
| Reputação | NENHUM `ReputationService`/`ReputationState`/seção de save — só menções em directions | ADR-0017 aposenta o conceito; nada criado |
| Diária | proposta 50g (alinhada à régua) | `InnLodgingPaymentResolver.DefaultNightlyRate = 50` |
| Próximo ADR | último arquivo = ADR-0016 (índice parava em 0015) | Criado **ADR-0017** (prompt/spec citavam 0010, estava stale) |

---

## Spec Compliance Matrix

| Requisito (spec) | Implementação |
|------------------|---------------|
| NpcBirthdayTable (23, estática, sem random, ≤1/dia) | `Assets/_Game/Scripts/NPC/Social/NpcBirthdayTable.cs` |
| NpcBirthdayService.IsBirthdayToday | `Assets/_Game/Scripts/NPC/Social/NpcBirthdayService.cs` (puro, recebe GameDate) |
| Entrada de aniversário na projection do calendário | `CalendarDayDetailModel.Birthdays` + `CalendarBirthdayProjectionBuilder.cs` |
| Presente ×2 dentro do fluxo F26 (cap preservado) | `FriendshipState.RegisterGift(pointsMultiplier)` + `FriendshipService.GiveGift` |
| NpcBirthdayGiftEvent (toast) | `Assets/_Game/Scripts/Core/Events/NpcBirthdayGiftEvent.cs` (publicado no aceite, no aniversário) |
| InnBedPaymentGate (cobra → fluxo F16) | `Assets/_Game/Scripts/City/InnBedPaymentGate.cs` + `InnLodgingPaymentResolver.cs` |
| Cama de hóspede no gerador (refs serializadas) | `CreateMvpTownScene.cs` (`BuildMinimalInterior` → House_Inn) |
| ADR de reputação | `docs/decisions/ADR-0017-reputation-absorbed-by-friendship.md` (proposed) |
| Texto novo via LocalizationService (ADR-0012) | `LocalizationStringTable` (`ui.birthday.gift_toast`, `ui.inn.sleep_prompt`) |
| Sem campo de save novo | aniversário re-derivável; cap diário já persiste via F26; diária = transação |

---

## Files changed

Novos (runtime):
- `Assets/_Game/Scripts/NPC/Social/NpcBirthdayTable.cs`
- `Assets/_Game/Scripts/NPC/Social/NpcBirthdayService.cs`
- `Assets/_Game/Scripts/Core/Events/NpcBirthdayGiftEvent.cs`
- `Assets/_Game/Scripts/City/InnLodgingPaymentResolver.cs`
- `Assets/_Game/Scripts/City/InnBedPaymentGate.cs`
- `Assets/_Game/Scripts/UI/Calendar/CalendarBirthdayProjectionBuilder.cs`

Modificados (runtime, diff mínimo):
- `Assets/_Game/Scripts/NPC/Friendship/FriendshipState.cs` (param `pointsMultiplier` em `RegisterGift`)
- `Assets/_Game/Scripts/NPC/Friendship/FriendshipService.cs` (multiplicador + toast no `GiveGift`; using Social)
- `Assets/_Game/Scripts/UI/Calendar/CalendarDayDetailModel.cs` (campo aditivo `Birthdays` + `NpcBirthdayProjection`)
- `Assets/_Game/Scripts/Localization/LocalizationStringTable.cs` (2 chaves novas)

Modificados (editor / gerador):
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` (cama de hóspede no House_Inn; `CreateInteriorProp` retorna GameObject)

Testes:
- `Assets/_Game/Tests/EditMode/NPC/BirthdayInnTests.cs` (novo, 16 testes)

Docs:
- `docs/decisions/ADR-0017-reputation-absorbed-by-friendship.md` (novo, proposed)
- `docs/project/DECISION_LOG.md` (índice: +ADR-0016, +ADR-0017)
- `docs/validation/fable_57_..._execution_report.md` (este)

Projeto:
- `Assembly-CSharp.csproj` (+7 includes: 6 runtime + 1 teste)

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (exit 0; 1 warning pré-existente CombatTelemetrySession CS0649)
Assembly-CSharp-Editor: PASS (exit 0; 3 warnings pré-existentes, não-fable_57)
Docs validation (validate_docs.ps1): PASS (exit 0)
Spec diff completeness (check_spec_diff_completeness.ps1): PASS (exit 0)
Quality check (check_spec_quality.ps1 via strict): PASS
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Arquivos proibidos alterados: NENHUM (sem `.unity`/`.prefab`/`.asset` por YAML; sem
Packages/ProjectSettings; sem SaveManager/seção de save; sem `BedInteractable.cs`; sem
direction de cidade). Testes só em `Assets/_Game/Tests/EditMode/**`.

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (tabela/datas, multiplicador, cap, cobrança da diária, projection)
Changed Unity scene/prefab/asset wiring: YES (cama de hóspede via gerador; sem YAML manual)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/NPC/BirthdayInnTests.cs — 16 testes)
Automated tests command: dotnet build .\Assembly-CSharp.csproj (compila os EditMode; execução do runner = lote final)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION — dormir na estalagem (débito + day transition) e presentear no aniversário (toast + ×2) verificados no lote final pelo humano (dono autorizou pular Play Mode nesta sessão)
Justification if no automated tests: N/A (testes adicionados)
Residual risk: a entrada do calendário e o prompt/confirmação da cama dependem de cena/Canvas (não testados em runtime nesta sessão); o builder de projection NÃO está ligado a um construtor de detalhe de dia em runtime (não há um — o modelo é consumido pela UI da WAVE04), então a exposição visual do aniversário no calendário fica pendente do wiring de UI/Play Mode final.
```

Regressão: presente comum (F26) fora de aniversário continua +delta com cap 1/dia/NPC
(`RegisterGift_NoMultiplier_KeepsBaselineF26Behavior`); dormir na fazenda (F16) inalterado
(BedInteractable não tocado); nenhum tracker social paralelo (`NoParallelReputationTracker_*`).

---

## validated_adrs / validated_game_rules

```text
validated_adrs:
  - ADR-0007 (event-bus gameplay communication): NpcBirthdayGiftEvent e feedback da cama via GameEventBus; nenhuma chamada gameplay MonoBehaviour→MonoBehaviour direta. CONFORME.
  - ADR-0012 (localization string table from P4): texto novo voltado ao jogador declarado em LocalizationStringTable (ui.birthday.gift_toast, ui.inn.sleep_prompt). CONFORME.
  - ADR-0006 (save data simple DTOs): nenhuma seção de save nova; nenhum tipo Unity em DTO. CONFORME (sem mudança de save).
  - ADR-0017 (NOVO — reputation absorbed by friendship): criado por esta spec; status proposed.

validated_game_rules:
  - event_rules.md: NpcBirthdayGiftEvent publicado/consumido via GameEventBus; consumidores de toast usam unsubscribe (HUD existente). CONFORME.
  - save_rules.md: sem novo campo/seção; aniversário re-derivável de tabela+calendário; cap diário de presente já persistido por F26; diária = transação econômica normal. CONFORME (NO save schema change).
```

---

## Dependency Chain

```text
Original target: fable_57
Dependency chain: F26 (FriendshipService — BUILD_VALIDATED, executada), F37 (calendário/projection — executada), F16 (BedInteractable — executada), F11 (interiores da cidade — executada). Todas resolvidas; nenhuma pendente.
Forbidden dependencies: none
Resolved depth: 0 (todas as dependências já BUILD_VALIDATED no histórico)
Can continue original target: YES (executado)
```

---

## Remaining work

- Play Mode final (lote): presentear NPC no dia do aniversário (toast "Hoje é aniversário de X!" + ×2 confirmado nos pontos) e dormir na estalagem (débito de 50g + day transition).
- Regenerar TownScene no Unity (gerador atualizado com a cama de hóspede no House_Inn).
- Aval humano do ADR-0017 (proposed → accepted).
- Follow-up de docs (separado): substituir "reputação" nos directions de cidade por linguagem de amizade, citando ADR-0017.
- Wiring de UI: ligar `CalendarBirthdayProjectionBuilder` ao construtor de detalhe de dia do calendário quando a UI do calendário for finalizada (atualmente o modelo é consumido pela camada de UI da WAVE04).
```
