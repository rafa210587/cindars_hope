# Execution Report — fable_36 — Main Quest Atos 2-4

> **Spec:** `.specs/a_implementar/fable/fable_36_spec_main_quest_acts_2_4.md`
> **Status:** BUILD_VALIDATED_WITH_WARNINGS (Phase 2-3 Unity/Play Mode DEFERRED, autorizado pelo dono)
> **Date:** 2026-06-20
> **Branch:** dev
> validated_adrs: []
> validated_game_rules: [quest_rules.md]

---

## Honest status rationale

Status **BUILD_VALIDATED_WITH_WARNINGS**: o núcleo determinístico dos Atos 2-4 (encadeamento, marcos
de ato, +1 skill point idempotente, fragmento via Fonte, ActCompletedEvent, gate do Nymirian, aviso
de profundidade) está implementado, compila com 0 erros nos dois assemblies, e tem cobertura EditMode
autorada. A execução dos EditMode tests via Unity Test Runner e a validação visual/Play Mode (placement
do interactable do Nymirian na cena da caverna, toast de marco, lore no detalhe da quest F14) ficam
**DEFERRED_TO_FINAL_VALIDATION** — o dono autorizou pular Play Mode/validação humana nesta sessão.
Nenhum claim de PLAYMODE_VALIDATED/ACCEPTED é feito.

---

## Acceptance criteria extracted

| CA | Critério (spec) | Implementação | Evidência | Status |
|----|-----------------|---------------|-----------|--------|
| CA-1 | Atos 2-4 (4-6 quests cada) encadeados por flag; oferta do ato N+1 só com act_N_done | `QuestRegistry.MainActs2To4.cs` (Act2=5, Act3=5, Act4=4 quests Main encadeadas por `PrerequisiteQuestIds`); Act2 entry gated em `mq_act1_05`; Act3 em `mq_act2_05`; Act4 em `mq_act3_05` | `MainActsTests`: `Act2_HasFiveChainedMainQuests`, `Act3_..._EntryGatedOnAct2Finale`, `Act4_..._EntryGatedOnAct3Finale`, `Act2EntryPrerequisite_BlockedUntilAct1Finale` | OK |
| CA-2 | Concluir ato seta act_N_done + +1 skill point idempotente (mesmo após reload) + lore | act-final grant de `act_N_done`/`flag_mq_actN_complete`/`flag_main_post_actN` (reward); `MainProgressionQuestBridge.CompleteAct` → `QuestService.TryAwardActSkillPoint` (idempotente via `RewardedMainActIds`) + `ActCompletedEvent(loreRecordId)` | `Bridge_CompleteAct_GrantsExactlyOneSkillPointPerAct_Idempotent`, `Bridge_SkillPoint_NotDuplicatedAcrossFreshServiceAfterReload`, `Bridge_CompleteAct_PublishesActCompletedEventOncePerAct`, `Act2Finale_GrantsActDoneFlag_OnTurnIn` | OK |
| CA-3 | Nymirian conversável via flag a partir do Ato 3 (interactable na caverna, sem schedule) | `NymirianAppearance` (gate flag-gated puro, depth 60) + `mq_act3_04` seta `flag_nymirian_available`; placement do interactable na cena = DEFERRED | `Nymirian_NotAvailableUntilFlagSet`, `Nymirian_FailsClosed_WithNullPredicate`, `Act3Act04_GrantsNymirianAvailableFlag` | OK (lógica) / DEFERRED (placement de cena) |
| CA-4 | Ato 1 auditado e completado a ≥4 quests | Auditoria Fase 0: Ato 1 já tem **5 quests** (fable_10), encadeadas, finale = Fragmento da Água. Nenhuma lacuna a fechar (≥4 satisfeito). Regressão preservada (IDs/flags/encadeamento intactos) | `Regression_Act1_StillFiveQuestsAndWaterFinale`, `Regression_SmokeTestQuests_Intact` | OK |

---

## Existing systems audit

Fase 0 — system-reuse audit (skill `system-reuse-audit`). Reutilizado, NÃO recriado:

| Sistema existente | Origem | Como foi reutilizado |
|---|---|---|
| `QuestRegistry` (in-memory quest catalog) | fable_10/WI-15 | Estendido via `partial class` + `RegisterMainQuestActs2To4()`. Mesmo padrão `Register(def, objectives, rewards, giver)` do Ato 1. Nenhum segundo registry. |
| `MainProgressionQuestBridge` | fable_10 | Generalizado de "só Water/Ato 1" para os 4 act-finales (mapa `QuestMainActsIds.Finales`). Mantém `FragmentGrantQuestId`/`TryGrantWaterFragment` para back-compat. |
| `FonteRuntimeService.IntegrateFragment` | WAVE 10 | Único ponto de integração de fragmento (Memory/Life). Hope é HINTADO (não integrado) — fronteira fable_43. |
| `QuestService.TryAwardActSkillPoint` + `RewardedMainActIds` | fable_34 | Hook de +1 skill point por ato já existia mas **sem caller runtime**. fable_36 liga o caller (a bridge). Idempotência persistida já estava pronta. |
| `QuestRewardType.QuestFlagGrant` + `QuestRewardApplicator` | quest core | Concede `act_N_done`/milestones via reward existente. Sem novo mecanismo de flag. |
| `QuestObjectiveType` (TalkToNpc/CollectItem/ReachCaveDepth/DefeatEnemy) | WI-26 | Apenas tipos com handler runtime real foram usados. |
| `ActCompletedEvent` | NOVO (mínimo) | Único tipo novo: struct de evento sem refs Unity, em `QuestRuntimeEvents.cs`. |
| `act_N_done` flags | fable_35/fable_70 | Side chains já LIAM `act_1_done`/`act_2_done`/`act_3_done` (`NpcQuestChainCatalog`) mas **nada os setava**. fable_36 fecha esse gap (os setamos no act-finale). |

Sistemas criados (novos, justificados): `ActCompletedEvent` (evento), `QuestMainActsIds` (constantes/IDs/lore/fragment-map), `NymirianAppearance` + `MainActDepthAdvisory` (lógica pura narrativa). Nenhum manager/service/SO paralelo.

### Fase 0 — auditoria de gates de boss (F33)

`CreateCaveBossAssets` cria gates **apenas** nos níveis 15/30/45/60/75/90, todos com boss
`enemy_meteor_ooze_king`. Os nomes do catálogo (Rimelock Colossus/gate 30, Draconic Guardian/gate 70,
Draconic Elder/gate 100) **não existem** como boss IDs canônicos, e **não há gate 70 nem 100** (o mais
fundo é 90). Adaptação (precedente fable_10, risco previsto na spec):

| Ato | Gate de design | Conteúdo real usado | Objetivo |
|---|---|---|---|
| Act 2 | gate 30 (Rimelock) | gate 30 REAL | `ReachCaveDepth 30` + `DefeatEnemy "any"` |
| Act 3 | gate 70 (Draconic Guardian) | clamped a 75 (gate real) | `ReachCaveDepth 75` + `DefeatEnemy "any"` |
| Act 4 | gate 100 (Draconic Elder) | clamped a 90 (gate mais fundo) | `ReachCaveDepth 90` + `DefeatEnemy "any"` |

`DefeatBoss` existe no enum mas **não tem handler runtime**; por isso usamos `DefeatEnemy "any"`
(handler real) — idêntico à decisão do guardião do nível 10 no fable_10. Documentado como residual risk.

---

## Spec Compliance Matrix

| Requisito (spec) | Implementação | Status |
|---|---|---|
| Gerador dos atos 2-4 (mq_act<N>_<n>) | `RegisterMainQuestActs2To4` (in-memory, mesmo padrão do Ato 1; NÃO um Editor asset-generator — coerente com o precedente fable_10 e sem dependência de Unity batchmode) | OK |
| Flags de marco act_N_done | Reward `QuestFlagGrant` no act-finale → `QuestMainActsIds.FlagActNDone` | OK |
| Revelação gradual (registro de lore por ato) | `ActCompletedEvent.LoreRecordId` + `QuestMainActsIds.LoreRecordActN`; texto derivado de flag (não persiste texto) | OK |
| Nymirian (roster + spawn por flag, interactable, sem schedule) | `NymirianAppearance` (gate por `flag_nymirian_available`, depth 60); placement de cena DEFERRED | OK (lógica) |
| Aviso de profundidade NARRATIVO (soft, 1×) | `MainActDepthAdvisory.ShouldAdvise` (informativo, nunca bloqueia; gate físico continua sendo os bosses) | OK |
| +1 skill point por ato (F34 hook, idempotente) | `MainProgressionQuestBridge.CompleteAct` → `QuestService.TryAwardActSkillPoint` | OK |
| ActCompletedEvent (publish no turn-in final; unsubscribe) | `QuestRuntimeEvents.ActCompletedEvent`; bridge publica; `Unsubscribe` em `QuestRuntimeBootstrap.OnDisable` | OK |
| Fronteira Ato 5/final choice → fable_43 | `mq_act4_05_final_choice` NÃO autorado; Hope HINTADO, não integrado | OK (`Act4_DoesNotAuthorFinalChoice_BoundaryToFable43`) |
| Sem save section nova / sem migração | Quests/flags/skill points usam seções existentes | OK |
| Anti-softlock (Main nunca expira) | `QuestCategory.Main` em todas; `CanExpire()`=false | OK (`AllActsQuests_AreMainAndNeverExpire`) |

---

## Validation

```
Validation method: dotnet build (per-assembly) + validate_docs.ps1 + check_spec_diff_completeness.ps1
Assembly-CSharp:        PASS (exit 0, 0 erros)
Assembly-CSharp-Editor: PASS (exit 0, 0 erros)
Docs validation:        PASS (validate_docs.ps1 exit 0)
Diff completeness:      PASS (check_spec_diff_completeness.ps1 exit 0)
run_strict_validation.ps1: exit 1 ESPERADO/AMBIENTAL — 3 cenas .unity (CaveScene/FarmScene/TownScene)
  já modificadas no working tree ANTES desta spec; fora do escopo do fable_36 (nenhuma cena tocada).
  Os builds e checks escopados (acima) passam exit 0.
Unity batchmode / Play Mode: NOT RUN (DEFERRED_TO_FINAL_VALIDATION — autorizado pelo dono)
  Reason: approval (sessão sem Play Mode); Residual risk abaixo.
```

### Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (gates de ato, idempotência de skill point, gate do Nymirian, aviso de profundidade)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Quests/MainActsTests.cs)
Automated tests command: Unity Test Runner (EditMode) — NOT RUN (Unity batchmode deferred)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (placement do Nymirian, toast de marco, lore no detalhe F14)
Justification if no automated tests: N/A (tests authored; só a EXECUÇÃO é deferida ao Unity Test Runner)
Residual risk:
  - EditMode tests compilam (parte do Assembly-CSharp, build exit 0) mas não foram EXECUTADOS nesta sessão (sem Unity).
  - Boss de gate: Act 3/Act 4 usam ReachCaveDepth clamped (75/90) + DefeatEnemy "any" porque gates 70/100 não existem no conteúdo F33 real.
  - Propagação cross-system das flags act_N_done para os QuestFlagService de F28/F25/side-chains depende do wiring de runtime (instâncias separadas por subsistema) — verificável só em Play Mode.
  - Placement do interactable do Nymirian na CaveScene e exibição da lore no detalhe da quest (F14) ficam para a validação visual final.
```

---

## Files changed

```
Assets/_Game/Scripts/Core/Events/QuestRuntimeEvents.cs        (+ ActCompletedEvent)
Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs          (partial + chama RegisterMainQuestActs2To4)
Assets/_Game/Scripts/Quests/Runtime/QuestMainActsCatalog.cs   (NOVO — IDs/flags/lore/fragment-map)
Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.MainActs2To4.cs (NOVO — Atos 2-4 data-driven)
Assets/_Game/Scripts/Quests/Runtime/MainProgressionQuestBridge.cs (generalizado p/ 4 atos + skill point + ActCompletedEvent)
Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs  (injeta QuestService na bridge)
Assets/_Game/Scripts/Cave/Runtime/MainActCaveNarrative.cs     (NOVO — Nymirian gate + aviso de profundidade)
Assets/_Game/Tests/EditMode/Quests/MainActsTests.cs           (NOVO — cobertura CA-1..CA-4 + anti-softlock)
Assembly-CSharp.csproj                                        (4 novos Compile Include)
docs/validation/fable_36_spec_main_quest_acts_2_4_execution_report.md (este report)
```

Nenhum arquivo proibido alterado: sem `.unity`/`.prefab`/`.asset`, sem `Packages/`/`ProjectSettings/`,
sem alterar o contrato do `QuestFlagService`, sem tocar dados do Ato 1 (só auditoria), sem alterar
bosses F33 (só referência de IDs/depth), sem `SaveManager`/seções de save.

---

## Dependency Chain

```
Original target: fable_36
Dependency chain: F34 (BUILD_VALIDATED), F35/F70 (BUILD_VALIDATED), F10 (BUILD_VALIDATED), F33 (assets gerados) — todas já implementadas (consumidas, não re-executadas)
Forbidden dependencies: none
Resolved depth: 0 (sem dependência same-wave pendente)
Can continue original target: YES
```

---

## Remaining work (DEFERRED, não bloqueante)

- Executar `MainActsTests` no Unity Test Runner (EditMode) e anexar resultado.
- Placement do interactable do Nymirian na CaveScene (nível 60) via editor script + evidência.
- Exibir o registro de lore no detalhe da quest (F14) e o toast de marco de ato (consumidor de `ActCompletedEvent`).
- Cenário humano: completar 1 quest de ato com marco visível (Testing Quality Gate da spec).
- fable_43: Ato 5 / escolha final / integração do fragmento Esperança (fora de escopo, fronteira preservada).
```
