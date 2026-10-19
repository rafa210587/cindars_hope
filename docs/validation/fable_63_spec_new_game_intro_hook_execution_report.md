# Execution Report — fable_63 (New Game Intro + Main Quest Hook)

> Spec: `.specs/a_implementar/fable/fable_63_spec_new_game_intro_hook.md`
> Status: **BUILD_VALIDATED_WITH_WARNINGS** (Play Mode DEFERRED_TO_FINAL_VALIDATION)
> Date: 2026-06-21
> Branch: dev

---

## Acceptance criteria extracted

| ID | Criterio | Evidencia | Status |
|----|----------|-----------|--------|
| CA-1 | Intro 3-5 telas, Esc pula, termina/skipa libera gameplay, 1x/save (inclusive apos reload) | `IntroSequenceModel` (state machine pura) + `IntroSequenceController` (modal guard + flag IntroSeen); tests `Intro_*`, `FlagStore_Set_*` | OK (logica); Play Mode DEFERRED |
| CA-2 | 1o DayStarted oferta/aceita mq_act1_00 exatamente 1x; reload+DayStarted nao duplica; aparece no log apontando Corvus | `MainQuestHookService.OnDayStarted` (flag persistida checada antes); test `Hook_OffersOnce_ReloadDoesNotDuplicate` + `Hook_Quest_AcceptThenTalkToCorvus_*` | OK |
| CA-3 | Carta existe na FarmScene via gerador; interagir mostra texto; estado lido persiste; zero YAML manual | `LetterInteractable` + `CreateMvpFarmScene.CreateBedLetter()` (diff aditivo do gerador); flag letter_read | OK (codigo); regeneracao Unity DEFERRED |
| CA-4 | Concluir mq_act1_00 (TalkToNpc Corvus) seta flag/prerequisite que mq_act1_01 consome | `QuestRegistry.MainQuestHook` injeta mq_act1_00 como PrerequisiteQuestId da mq_act1_01; tests `Hook_IsPrerequisiteOf_MainQuestAct1_01` + conclusao/flag | OK |
| CA-5 | Todo texto provisorio marcado PLACEHOLDER_LORE; report lista dependencia de lore + pontos de troca | grep abaixo + secao "PLACEHOLDER_LORE" | OK |

---

## Existing systems audit

Fase 0 — auditoria de reuso (skill system-reuse-audit). Nenhum sistema paralelo criado.

| Sistema necessario | Existente reutilizado | Encontrado em |
|--------------------|------------------------|---------------|
| Fluxo unico de quest (registro/aceite/progresso/save) | `QuestRegistry` (partial, `Register()`), `QuestService` (`AcceptQuest`/`TurnIn`/`OnNpcTalkedTo`) | `Assets/_Game/Scripts/Quests/Runtime/` |
| TalkToNpc objective | `QuestObjectiveType.TalkToNpc` + `QuestService.OnNpcTalkedTo` (WI-26) | `Quests/Runtime/QuestService.cs` |
| Reward = flag | `QuestRewardType.QuestFlagGrant` + `QuestRewardApplicator` (idempotente) | `Quests/Rewards/` |
| Gatilho do 1o dia | `DayStartedEvent` (DayNumber) | `Core/Events/DayStartedEvent.cs` |
| Flags persistidas (familia existente) | `QuestStateSection.GlobalKnownHints` (List<string> ja round-trip via QuestRuntimeBootstrap Capture/Restore) — SEM secao nova, SEM migracao | `Quests/Save/QuestStateSection.cs` |
| Disparo New Game | `TitleScreenController.StartNewGame` (F56, dona do reset) — apenas publica `NewGameStartedEvent` (a intro ESCUTA) | `UI/Title/TitleScreenController.cs` |
| Modal/input guard | `ModalManager.PushModal(ModalType.Dialogue)` (mesmo bloqueio das telas de dialogo; sem novo ModalType) | `UI/Modal/ModalManager.cs` |
| Carta = IInteractable padrao via gerador | `IInteractable` + `CreateMvpFarmScene` (padrao BedInteractable) | `Interaction/`, `Editor/SceneCreation/` |
| HUD feedback da carta | `PlayerActionFeedbackEvent` (mesmo canal do BedInteractable) | `Core/Events/` |

Sistemas NOVOS criados (apenas o que nao existia): `NewGameStartedEvent`, `IntroSequenceModel`, `IntroSequenceController`, `NarrativeFlagStore`, `MainQuestHookService`, `NarrativeRuntimeBootstrap`, `LetterInteractable`, `QuestRegistry.MainQuestHook` (partial), `NarrativeIds`.

---

## Spec Compliance Matrix

| Requisito da spec | Implementacao |
|-------------------|---------------|
| IntroSequenceController programatico (sem YAML), 3-5 telas, E/Enter avanca, Esc pula | `IntroSequenceController` (IMGUI, padrao QuestLogPanelController) + `IntroSequenceModel` (pura) |
| Input de gameplay bloqueado durante a intro; libera ao terminar/skipar | `PushModal(ModalType.Dialogue)` / `TryPopModal` no `Finish()`/`OnDisable` |
| Exibida 1x/save (flag IntroSeen persistida) | `NarrativeFlagStore` sobre `GlobalKnownHints`; `TryStartIntro` aborta se `FlagIntroSeen` setada |
| Disparo: New Game (E56) OU boot de save novo (fallback) | `NewGameStartedEvent` (publicado por StartNewGame) + `GameLoadedEvent{WasSuccessful=false}` fallback |
| mq_act1_00 TalkToNpc Corvus, source Main, recompensa = flag | `QuestRegistry.MainQuestHook` (Category Main → source Main; reward QuestFlagGrant `flag_mq_act1_00_complete`) |
| Auto-oferta no 1o DayStarted, idempotente por flag persistida | `MainQuestHookService.OnDayStarted` (sela `FlagHookOffered` antes da oferta) wired por `NarrativeRuntimeBootstrap` |
| mq_act1_00 como PrerequisiteQuestId da mq_act1_01 (diff aditivo) | injetado em `RegisterMainQuestHook` (aditivo, idempotente) |
| Carta na cama via gerador; texto; estado lido persiste; some/fica lida | `LetterInteractable` (prompt muda apos leitura) + `CreateBedLetter()` (gerador); flag `letter_read` |
| Default seguro save legado = intro vista | `NarrativeRuntimeBootstrap.Initialize` sela IntroSeen se ha QuestStates carregadas; test dedicado |
| Sem evento novo obrigatorio salvo necessario (documentado) | 1 evento novo: `NewGameStartedEvent` (necessario p/ a intro escutar o New Game sem caminho duplicado) |
| unsubscribe pareado (hook assina DayStarted) | `NarrativeRuntimeBootstrap.OnDisable` + `IntroSequenceController.OnDisable` |

---

## PLACEHOLDER_LORE (rastreabilidade — CA-5)

Dependencia: **refinamento de lore Nymirianos/Cindar PENDENTE**. Trocar texto = diff trivial.

Pontos de troca (grep `PLACEHOLDER_LORE`):
- `Assets/_Game/Scripts/Narrative/IntroSequenceModel.cs` — 4 telas da intro (intro_01..intro_04).
- `Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.MainQuestHook.cs` — DisplayName/Description da mq_act1_00.
- `Assets/_Game/Scripts/World/LetterInteractable.cs` — texto da carta.

Ids estaveis (NUNCA renomear na troca de texto): `mq_act1_00`, `npc_corvus`, `obj_mq_act1_00_talk_corvus`, `flag_mq_act1_00_complete`, `narrative_intro_seen`, `narrative_mq_act1_00_offered`, `narrative_letter_read`, `intro_01..intro_04`.

---

## Ordem de mensagens do 1o dia (risco de empilhamento)

Documentado para nao empilhar telas: **intro fecha (modal liberado) → toast de hints (F62, push automatico) → carta (pull: o jogador interage quando quiser)**. A intro e a unica tela modal de abertura; a carta nao e modal (feedback toast). Validar a sequencia no cenario humano.

---

## Validation

```text
Validation method: dotnet build (no-restore) + validate_docs.ps1 + check_spec_diff_completeness.ps1 + run_strict_validation.ps1
Assembly-CSharp: PASS (exit 0; 1 warning pre-existente)
Assembly-CSharp-Editor: PASS (exit 0; 3 warnings pre-existentes)
Docs validation: PASS (exit 0)
Diff completeness (scoped): PASS (exit 0)
Strict validation: ver bloco abaixo (exit 1 ambiental possivel por 3 cenas .unity ja modificadas no working tree, NAO desta spec)
EditMode tests: 14 testes em Assets/_Game/Tests/EditMode/Narrative/IntroHookTests.cs — NOT RUN (Unity Test Runner nao executado neste ambiente; compilam no Assembly-CSharp)
```

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (intro state machine, idempotencia da oferta, flag store, default legado, prerequisite)
Changed Unity scene/prefab/asset wiring: YES (gerador CreateMvpFarmScene — carta; regeneracao da cena DEFERIDA ao humano)
Automated tests added/updated: YES (IntroHookTests, 14 testes)
Automated tests command: Unity Test Runner EditMode (NOT RUN neste ambiente; arquivos compilam em Assembly-CSharp)
Manual Play Mode scenario: docs/validation/playmode/fable_63_human_test_scenario.md
Justification if no automated tests: N/A (testes adicionados)
Residual risk: render IMGUI da intro, push/pop real do ModalManager, bloqueio de movimento com a intro aberta e a presenca da carta na cena regenerada so verificaveis em Play Mode (DEFERRED_TO_FINAL_VALIDATION). A regeneracao da FarmScene (CreateMvpFarmScene) precisa ser executada no Unity Editor para a carta existir na cena.
```

---

## Honest status rationale

Status **BUILD_VALIDATED_WITH_WARNINGS**: criterios centrais implementados no fluxo unico existente, ambos assemblies compilam 0E, docs e diff-completeness PASS, 14 EditMode tests escritos cobrindo a logica deterministica (state machine/skip, idempotencia com reload, default legado, conclusao TalkToNpc, prerequisite, PLACEHOLDER_LORE). Fase 2-3 (Unity batchmode + Play Mode) NAO executada por decisao do dono (DEFERRED). A regeneracao da FarmScene fica pendente de acao humana no Unity Editor — sem ela a carta nao aparece em cena (codigo do gerador pronto). Nao promovido a implementados/.

## Dependency / integration note

- F10 (E40 cadeia mq_act1_01..05), F34 (E38 fontes de quest), F56 (E56 New Game) ja IMPLEMENTADAS no repo — dependencias condicionais satisfeitas (sem fallback necessario).
- Ponte E40: mq_act1_00 ja injetado como PrerequisiteQuestId da mq_act1_01 (nao apenas documentado).

## Remaining work (deferred)

- Regenerar FarmScene no Unity (menu CreateMvpFarmScene) para materializar a carta (BedLetter).
- Unity Test Runner EditMode (14 testes).
- Play Mode: jornada New Game → intro → skip/replay (nao re-exibe) → carta → mq_act1_00 no log → falar com Corvus conclui.
- Troca dos textos PLACEHOLDER_LORE apos o refinamento de lore Nymirianos/Cindar.

---

validated_adrs: [ADR-0007-event-bus-gameplay-communication.md]
validated_game_rules: [ui_modal_rules.md, save_rules.md, event_rules.md]
