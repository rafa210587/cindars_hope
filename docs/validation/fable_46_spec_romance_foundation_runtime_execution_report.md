# Execution Report — fable_46_spec_romance_foundation_runtime

> **Spec:** `.specs/a_implementar/fable/fable_46_spec_romance_foundation_runtime.md`
> **Status:** BUILD_VALIDATED_WITH_WARNINGS
> **Date:** 2026-06-21
> **Wave:** FABLE Batch 9 · Priority P3 · Type Runtime/Save · Domain NPC/Social

validated_adrs: []
validated_game_rules: [save_rules.md, event_rules.md]

---

## Honest status rationale

Status **BUILD_VALIDATED_WITH_WARNINGS** (não ACCEPTED, não PLAYMODE_VALIDATED).

A fundação de romance (estado, gates, limite de 2, progressão, eventos, save aditivo, bônus de
presente, condição de diálogo IsPartner) está implementada como C# puro testável e ambos os builds
passam (0 erros). O fluxo de confissão via diálogo REAL (entrada "Confessar" no NpcController e o
disparo de `RomanceService.Confess` a partir da UI de diálogo) é a camada de integração de Play
Mode, **DEFERIDA** por autorização do dono (validação humana/Play Mode pulada nesta sessão). Por
isso o status carrega `_WITH_WARNINGS`: a lógica está pronta e testada em EditMode; a amarração da
fala de confissão à interação viva fica para a validação final de Play Mode (CA-4 cenário humano).

Nenhum claim de aceitação/Play Mode é feito. Phase 2 (Unity batchmode) e Phase 3 (Play Mode) = NOT RUN.

---

## Acceptance criteria extracted

| CA | Critério | Implementação | Evidência |
|----|----------|---------------|-----------|
| CA-1 | Confissão gated por amizade 5 + cadeia F35 + eligibility; cada condição isolada; sucesso → Interesse + evento | `RomanceState.EvaluateConfession/Confess`; gates por probe injetada; `RomanceStageChangedEvent` no sucesso | Testes `Confess_*` (5 testes de condição isolada) |
| CA-2 | Limite de 2 parceiros (Namoro+); 3ª confissão recusada com diálogo dedicado; estado inalterado | `RomanceEligibilityTable.MaxSimultaneousPartners=2`; recusa `PartnerLimitReached`; `RomanceDialoguePool.RejectionKey` | `PartnerLimit_ThirdConfession_RejectedWithDignity`, `PartnerLimit_RejectionTextPresentInPool`, `MaxSimultaneousPartners_IsTwo_PerDecision13` |
| CA-3 | Maelor exige cadeia própria além do nível 5; Nymiriano não confessável sem `act_3_done` | `RomanceEligibilityTable` (Maelor/Nymiriano LateActGated); `ActGateFlagFor` | `Maelor_RequiresChainBeyondFriendship`, `Nymiriano_NotConfessableWithoutActFlag`, `Eligibility_TableHasElevenCandidatesPlusTwoLate` |
| CA-4 | IsPartner muda pools; presente de parceiro +50% (cap respeitado) | `DialogueLineCondition.RequiresPartner/MinRomanceStage`; `FriendshipState.RegisterGift(bonusMultiplier)`; `RomanceService.GiftPointMultiplierFor` | `PartnerLines_OnlyEligibleWhenPartner`, `PartnerGift_AppliesFiftyPercentBonus_AfterDailyCap`, `PartnerGiftMultiplier_IsFiftyPercent` + cenário humano (deferido) |
| CA-5 | Round-trip preserva estágio/progresso; save legado carrega tudo None | Campos aditivos em `FriendshipEntrySaveData`; `RomanceState.WriteInto/RestoreFrom` | `Save_RoundTrip_PreservesStageAndProgress`, `Save_LegacyData_LoadsAllNone`, `Save_NullData_NoThrow_AllNone` |

---

## Existing systems audit (Phase 0)

| Sistema | Achado | Decisão |
|---------|--------|---------|
| `FriendshipService` / `FriendshipState` (F26) | níveis 0-5, IsAtLeast, save WI-18, ponto único de presente `RegisterGift` | **REUSE** — romance hospedado no mesmo GameObject; bônus de presente entra como `bonusMultiplier` no ponto único |
| `FriendshipEntrySaveData` (F26) | DTO simple-types da seção de amizade | **EXTEND** — 4 campos aditivos (romanceStage/progress/gifts/confessedDay); sem nova seção; sem migration |
| `DialogueLineCondition` / `DialogueConditionContext` (F28) | condição extensível por flags/eixos; precedente `RequiredInferredTitleId` (F39) | **REUSE** — adicionados eixos `RequiresPartner` + `MinRomanceStage` (IsPartner registrado NO sistema F28, sem mecanismo paralelo) |
| `NpcQuestChainCatalog` (F35) | `DoneFlag(questId)` + `ForNpc` (passo final) | **READ-ONLY** — cadeia completa = done-flag do passo final via `QuestFlagService.IsSet` |
| `QuestFlagService` / `CityServiceRuntimeBootstrap.FlagService` | flags de ato (`act_3_done`) e de cadeia | **READ-ONLY** — gate de ato/cadeia |
| `NpcTownRosterRegistry` | IDs canônicos dos 23 NPCs | usado para derivar a tabela `RomanceEligibilityTable` (validável contra o registry) |
| `CindarsHope.NPC.RomanceEligibility` (enum pré-existente em `NpcRelationshipStatus.cs`) | enum de classificação em dados de NpcDefinition (NÃO um sistema runtime) | colisão de nome evitada: nova tabela chamada `RomanceEligibilityTable`; nenhum sistema runtime paralelo existia |

**Nenhum segundo tracker social, sistema de condição, ou seção de save foi criado.**

---

## Spec Compliance Matrix

| Requisito (Escopo) | Implementação | Status |
|--------------------|---------------|--------|
| enum `RomanceStage {None,Interesse,Namoro,Compromisso}` | `RomanceStage.cs` | OK |
| `RomanceService` API GetStage/CanConfess/Confess/Advance/Partners | `RomanceService.cs` (+ `RegisterPartnerInteraction`) | OK |
| eligibility canônica em dados (11 + Maelor tardio + Nymiriano act_3) | `RomanceEligibilityTable` | OK |
| gates de confissão (amizade 5 + cadeia F35 + eligibility) | `RomanceState.EvaluateConfession` | OK |
| progressão por marcos (N interações + 1 presente, constantes nomeadas) | `RomanceEligibilityTable.PartnerInteractionsPerStage/PartnerGiftsPerStage` | OK |
| limite 2 simultâneos + recusa falada | `MaxSimultaneousPartners`; `RomanceDialoguePool.RejectionKey` | OK |
| condição `IsPartner` registrada no F28 + pools por estágio | `DialogueLineCondition.RequiresPartner/MinRomanceStage`; `RomanceDialoguePool.PartnerLines` | OK |
| presente de parceiro +50% no ponto único F26 (após cap) | `FriendshipState.RegisterGift(bonusMultiplier)` + `FriendshipService.GiveGift` | OK |
| eventos `RomanceStageChangedEvent`, `RomanceConfessionRejectedEvent` | `RomanceEvents.cs` | OK |
| save aditivo (romanceStage/stageProgress/confessedDay) + legado None | `FriendshipEntrySaveData` + `RomanceState` | OK (campo extra `RomanceStageGifts` para precisão do marco) |
| sem checagem de gênero (bi por decisão §13) | nenhuma referência a gênero em todo o código | OK |
| entrada "Confessar" no diálogo vivo (NpcController) | chaves + condição prontas; amarração à interação viva | DEFERIDO (Play Mode) |

---

## Validation

```
Validation method: dotnet build (per-spec) + validate_docs.ps1 + check_spec_diff_completeness.ps1 + run_strict_validation.ps1
Assembly-CSharp:        PASS (exit 0, 0 erros; 1 warning pré-existente em CombatTelemetrySession)
Assembly-CSharp-Editor: PASS (exit 0, 0 erros; warnings pré-existentes alheios)
Docs validation:        PASS (exit 0)
Diff completeness:      PASS (exit 0) — gate escopado
Strict validation:      exit 1 AMBIENTAL — 3 .unity (CaveScene/FarmScene/TownScene) já modificadas no
                        working tree ANTES desta spec (não tocadas aqui); nenhum erro de build/docs novo
Unity batchmode:        NOT RUN (Play Mode deferido por autorização do dono)
Play Mode:              NOT RUN (deferido)
```

Arquivos proibidos: nenhum alterado (sem `.unity/.prefab/.asset`, Packages/, ProjectSettings/,
SaveManager core, QuestRegistry/cadeias F35, NpcShopController/F25). Testes apenas em
`Assets/_Game/Tests/EditMode/NPC/`.

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (gates, limite 2, progressão por marcos, multiplicador de presente)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES
Automated tests command: dotnet test (EditMode) — NOT RUN nesta sessão (Unity Test Runner indisponível;
  testes compilam em Assembly-CSharp e seguem o padrão NUnit do projeto). Validação de compile: dotnet build PASS.
Manual Play Mode scenario: docs/validation/playmode/fable_46_human_test_scenario.md (DEFERIDO — pendente Phase 3)
Justification if no automated tests: N/A (testes adicionados: RomanceFoundationTests.cs, 19 testes)
Residual risk: a amarração da entrada "Confessar" à interação de diálogo viva (NpcController) e os
  toasts dos eventos só serão exercidos em Play Mode; a lógica subjacente está coberta por EditMode.
  Regression F26: o multiplicador de presente passa por `bonusMultiplier=1f` por padrão, então caminhos
  existentes (conversa/quest/compra/presente normal/aniversário) ficam idênticos — coberto por
  `PartnerGift_AppliesFiftyPercentBonus_AfterDailyCap` (cap diário preservado).
```

---

## Files changed

Novos:
- `Assets/_Game/Scripts/NPC/Friendship/RomanceStage.cs`
- `Assets/_Game/Scripts/NPC/Friendship/RomanceEligibility.cs` (enum status + `RomanceEligibilityTable`)
- `Assets/_Game/Scripts/NPC/Friendship/RomanceState.cs`
- `Assets/_Game/Scripts/NPC/Friendship/RomanceService.cs`
- `Assets/_Game/Scripts/Core/Events/RomanceEvents.cs`
- `Assets/_Game/Scripts/NPC/Dialogue/RomanceDialoguePool.cs`
- `Assets/_Game/Tests/EditMode/NPC/RomanceFoundationTests.cs`

Estendidos:
- `Assets/_Game/Scripts/NPC/Friendship/FriendshipSaveData.cs` (+4 campos aditivos de romance)
- `Assets/_Game/Scripts/NPC/Friendship/FriendshipState.cs` (`RegisterGift` ganha `bonusMultiplier`)
- `Assets/_Game/Scripts/NPC/Friendship/FriendshipService.cs` (host do RomanceService; bônus de presente; save mirror; day sync)
- `Assets/_Game/Scripts/NPC/DialogueLineCondition.cs` (eixos `RequiresPartner` + `MinRomanceStage`)
- `Assets/_Game/Scripts/NPC/DialogueConditionContext.cs` (campo `RomanceStage` + leitura em `FromWorld`)

(`Assembly-CSharp.csproj` recebe os Compile includes; gitignored, não comitado.)

---

## Remaining work (deferido)

- Amarrar a entrada "Confessar" e os pools de parceiro/recusa ao NpcController/UI de diálogo vivo
  (`TownNpcDialogueLibrary`) e disparar `RomanceService.Confess` — Play Mode.
- Toasts dos eventos `RomanceStageChangedEvent`/`RomanceConfessionRejectedEvent` no HUD.
- Cenário humano de Play Mode: confessar a 1 NPC e ver a recusa no 3º com 2 parceiros ativos.
- Casamento/cerimônia, visitas à fazenda, parceiro-companion: specs futuras sobre esta fundação.
```
