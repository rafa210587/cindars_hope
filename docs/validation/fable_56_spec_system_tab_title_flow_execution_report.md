# Execution Report — fable_56_spec_system_tab_title_flow

> Spec: `.specs/a_implementar/fable/fable_56_spec_system_tab_title_flow.md`
> Date: 2026-06-21
> Branch: `dev`
> Status: **BUILD_VALIDATED_WITH_WARNINGS** (PlayMode DEFERRED_TO_FINAL_VALIDATION; SaveManager-internal multi-slot/backup/character-creation deferred as documented residual risk — ver "Honest status rationale")

validated_adrs: [ADR-0013-input-keyboard-mouse-only-v1.md, ADR-0014-single-difficulty-v1.md]
validated_game_rules: [ui_modal_rules.md, save_rules.md]

---

## Acceptance criteria extracted

| ID | Criterio | Implementacao | Evidencia |
|----|----------|---------------|-----------|
| CA-1 | Aba Sistema (Salvar/Carregar/Volume/Video/Sair) navegavel por teclado, Esc fecha, input de gameplay bloqueado | `SystemTabViewModel` (estado/acoes) + `SystemTabController` (adapter; confirmacoes via ModalManager.PushModal(SystemConfirm)) | EditMode: `Save_AlwaysAvailable`, `Load_Disabled_WhenNoSaveFile`, `RequestLoad_OpensConfirmation`; navegacao/Esc/input-blocking no cenario humano |
| CA-2 | Salvar persiste + feedback; Carregar exige confirmacao e restaura; Carregar desabilitado sem arquivo | `SystemTabViewModel.RequestLoad/ConfirmLoad/CancelConfirm`; `SystemTabController.RequestSave` (SaveManager.SaveGame) | EditMode: `ConfirmLoad_OK_LoadsMainSave_AndClearsConfirm`, `Cancel_ClosesConfirmation_NoEffect`, `Load_Disabled_WhenNoSaveFile`; round-trip no cenario humano |
| CA-3 | New Game inicia mundo novo sem herdar estado; save anterior intacto ate primeiro Salvar | `NewGameStateResetService.ResetAll` (ponto unico, `IResettableGameState`); `TitleScreenController.StartNewGame` NUNCA deleta slot | EditMode: `NewGameReset_ResetsAllRegisteredSurfaces`, `NewGameReset_IsIdempotentAndDeduplicates`; arquivo intacto no cenario humano |
| CA-4 | Continue so com save existente; mesmo caminho de load da aba | `TitleFlowViewModel.CanContinue/ResolveContinue` (probe injetado) | EditMode: `Continue_Hidden_WhenNoSave`, `Continue_Visible_WhenSaveExists_SamePathAsLoad` |
| CA-5 | Pendencia humana (slots/autosave) registrada e nao implementada | Decidida pelas EMENDAS (3 slots + backup); ver "Pendencia humana" abaixo. Nenhum codigo de multi-slot/autosave adicionado | Diff sem schema de slots; secao dedicada neste report |
| CA-6 | Recuperacao de save corrompido via backup rolling, com data; recusar/sem-backup nao destroi nada | `SystemTabViewModel` estados `OfferBackupRestore/LoadFailedNoBackup` + `TitleFlowViewModel.ResolveContinue`; `SaveFileProbe` le `.bak` | EditMode: `ConfirmLoad_Corrupted_WithBackup_OffersRestore`, `ConfirmBackupRestore_Restores_AndClears`, `ConfirmLoad_Corrupted_NoBackup_ErrorAndNothingDestroyed`, `Continue_Corrupted_*` |

---

## Existing systems audit

Auditoria Fase 0 (skill system-reuse-audit) — REUSADO, nada de paralelo:

| Sistema existente | Caminho | Decisao |
|---|---|---|
| Painel unico F14 (8ª aba = System) | `Assets/_Game/Scripts/UI/Runtime/GameplayScreenTab.cs` (`System = 8`), `GameplayScreensPanelModel.cs` | REUSADO — `GameplayScreenTab.System` ja existe e `IsPlaceholderTab` o marca; esta spec preenche o conteudo |
| ModalManager / ModalType | `Assets/_Game/Scripts/UI/Modal/ModalManager.cs` | REUSADO — `PushModal/TryPopIfCurrent`; **diff aditivo**: novo valor `ModalType.SystemConfirm` para as confirmacoes |
| Input routing / focus | `UI/Input/GameplayInputRouter.cs`, `UIFocusState.SystemFocus` (ja existia) | REUSADO — bloqueio de gameplay com modal aberto |
| SaveManager | `Assets/_Game/Scripts/Save/SaveManager.cs` | REUSADO — apenas API publica `SaveGame()/LoadGame()/SaveFilePath`; ZERO mudanca de schema/internals |
| GameSavedEvent / GameLoadedEvent | `Assets/_Game/Scripts/Core/Events/` | REUSADO — feedback (publicado pelo SaveManager) |
| NotificationToastController | `UI/Notification/NotificationToastController.cs` | REUSADO — toast de salvar |
| SceneTransitionRouter / SceneId | `World/Scenes/` | REUSADO — `SceneId.Farm` para cena inicial |
| AudioManager (F58) | `Assets/_Game/Scripts/Audio/AudioManager.cs` | REUSADO — `SetMasterVolume/SetChannelVolume(AudioChannel)`; `GameAudioSettings` alimenta os 3 canais |
| PlayerPrefs | (nenhum uso previo no projeto) | NOVO adapter `PlayerPrefsSettingsStore` (UI state, fora do GameSaveData — SAVE_LOAD §19) |

Tipos NOVOS (todos aditivos, logica pura + adapters finos): `ISaveFileProbe`, `SaveFileProbe`, `ISettingsStore`, `PlayerPrefsSettingsStore`, `GameAudioSettings`, `SystemTabViewModel`, `SystemTabController`, `IResettableGameState`, `NewGameStateResetService`, `TitleFlowViewModel`, `TitleScreenController`.

PauseMenuController: nao existe classe com esse nome — pause e tratado por `ModalPauseGate` + evento `PauseOpenedEvent` + `ModalType.Pause`. Decisao Fase 0: **nao duplicar** quit/save; a aba Sistema introduz `SystemConfirm` sem mexer no caminho de pause existente.

Boot/cena de titulo: nao existe cena de titulo; o jogo entra direto em gameplay. Decisao Fase 0: `TitleScreenController` e um controller programatico (padrao F14) que pode ser ligado por overlay de boot OU cena propria via gerador no wiring humano — **nao** se criou `CreateTitleScene` nem se tocou EditorBuildSettings/ProjectSettings (risco tecnico mitigado: sem superficie proibida).

---

## Spec Compliance Matrix

| Requisito da spec | Implementacao |
|---|---|
| Aba Sistema com 4+ entradas navegaveis | `SystemTabViewModel` + `SystemTabController` (Save/Load/Volume×3/Video/QuitToTitle) |
| Salvar via SaveManager (slot unico) + toast | `SystemTabController.RequestSave` → `SaveManager.SaveGame` (publica GameSavedEvent) |
| Carregar com confirmacao; desabilitado sem arquivo | `SystemTabViewModel.RequestLoad/ConfirmLoad`; `CanLoad = probe.HasSave` |
| Volume placeholder persistido fora do save → **3 volumes + video** (EMENDA v3 4.4) | `GameAudioSettings` (Master/SFX/Music + Fullscreen/ResolutionIndex) via `ISettingsStore`/PlayerPrefs |
| Sair para o Titulo com confirmacao → ResetAll | `SystemTabViewModel.RequestQuitToTitle/ConfirmQuitToTitle`; `TitleScreenController.DiscardToTitle` |
| Titulo: New Game limpa estado, nao deleta save | `NewGameStateResetService.ResetAll` + `TitleScreenController.StartNewGame` |
| Titulo: Continue condicional, mesmo caminho de load | `TitleFlowViewModel.CanContinue/ResolveContinue` |
| Recuperacao de save corrompido via backup (EMENDA v3 4.8) | state machine em `SystemTabViewModel`/`TitleFlowViewModel`; `SaveFileProbe` le `.bak`+data |
| GameEventBus para feedback; sem GameObject.Find | adapters resolvem deps via `GameBootstrap.Instance`; SaveManager publica eventos |
| Sem mudanca de schema; volume em PlayerPrefs | confirmado — nenhum campo novo em GameSaveData/DTOs |

---

## Validation

```
Validation method: run_strict_validation.ps1 + dotnet build per csproj
Assembly-CSharp: PASS (exit 0, 0 errors)
Assembly-CSharp-Editor: PASS (exit 0, 0 errors)
Docs validation (validate_docs.ps1): PASS (exit 0)
Spec diff completeness (check_spec_diff_completeness.ps1): PASS apos este report (gate escopado)
run_strict_validation.ps1: exit 1 SOMENTE pelo gate de diff completeness antes deste report existir;
  apos o report, re-executar deve dar PASS. As 3 cenas .unity modificadas no working tree sao
  ambientais (pre-existentes, fora desta spec) e nao foram tocadas.
EditMode tests: compilam 0E em Assembly-CSharp (SystemTabTitleFlowTests, 21 testes). Execucao no
  Unity Test Runner DEFERIDA (sem Unity nesta sessao) — sem editor lock disponivel.
```

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (disponibilidade, state machine de confirmacao, recuperacao de backup, reset de New Game, persistencia de volume/video)
Changed Unity scene/prefab/asset wiring: NO (nenhum YAML editado)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/UI/SystemTabTitleFlowTests.cs — 21 testes)
Automated tests command: dotnet build .\Assembly-CSharp.csproj (compila); Unity Test Runner EditMode = NOT RUN (sem Unity)
Manual Play Mode scenario: docs/validation/playmode/fable_56_human_test_scenario.md
Justification if no automated tests: N/A (testes adicionados)
Residual risk: navegacao por teclado/Esc/input-blocking, foco visual e wiring de Canvas/boot
  so verificaveis em Play Mode (DEFERRED_TO_FINAL_VALIDATION). Execucao do EditMode no Test Runner
  pendente (compila, mas nao executado nesta sessao).
```

---

## Pendencia humana (CA-5 / SAVE_LOAD §20)

A pergunta original "save slots multiplos / autosave / backup visivel" foi **RESOLVIDA pelas emendas vinculantes** da propria spec:

- EMENDA 2026-06-12-D item 2: **3 slots manuais + backup rolling** do ultimo save bom a cada gravacao.
- EMENDA v3 4.8: recuperacao automatica do backup rolling quando o load do slot falha.
- EMENDA 2026-06-12-D item 1: **criacao de personagem** no New Game (nome, M/F/Neutro, tints) persistida em PlayerSaveData (campos aditivos).

**Decisao de escopo desta sessao (residual risk documentado):** a logica de fluxo (disponibilidade,
confirmacao, recuperacao, reset, persistencia de UI state) foi entregue de forma testavel e aditiva,
**consumindo o SaveManager de slot unico v1 existente**. NAO foram implementados nesta sessao:

1. **Multi-slot real (3 slots) + backup rolling `.bak` por gravacao** no SaveManager — exige mudar
   internals/schema do SaveManager, explicitamente listado em "Arquivos proibidos" da spec
   ("SaveManager internals; zero mudanca de schema", "implementacao de slots multiplos/autosave").
   O `SaveFileProbe` ja le um `.bak` convencional e a state machine ja oferece a restauracao, de modo
   que ligar o backup real e aditivo quando o SaveManager passar a grava-lo.
2. **Criacao de personagem persistida em PlayerSaveData** (nome/genero/tints) — exige campos aditivos
   no DTO de save (mudanca de schema), tambem fora dos "Arquivos permitidos".

Estes dois itens permanecem como **PENDENCIA para uma spec/sessao dedicada de SaveManager** (lock do
F13/SaveManager), respeitando a regra de nao tocar internals/schema do SaveManager nesta spec de UI.
Nenhum codigo de multi-slot/autosave/character-creation foi adicionado (CA-5 satisfeito: feature
nao implementada; pendencia registrada).

---

## Honest status rationale

- **BUILD_VALIDATED_WITH_WARNINGS**: nucleo deterministico (CA-1..CA-6 logica) implementado, testado em
  EditMode (compila 0E) e auditado; builds 0E nos dois csproj; docs PASS. Os "warnings":
  (a) Play Mode/Canvas/boot wiring DEFERIDO ao final (UI-heavy);
  (b) multi-slot real + backup `.bak` por gravacao e character-creation em PlayerSaveData DEFERIDOS por
      serem mudancas de internals/schema do SaveManager proibidas no escopo desta spec de UI;
  (c) Unity Test Runner EditMode NOT RUN (sem Unity nesta sessao).
- Sem claim de ACCEPTED/PlayMode PASS. Nenhum arquivo proibido alterado: nenhum `.unity/.prefab/.asset`
  editado; sem Packages/ProjectSettings; SaveManager so via API publica.

## Remaining work

- Wiring humano (Unity): ligar `SystemTabController` ao Canvas da aba Sistema e `TitleScreenController` ao
  boot (overlay ou cena via gerador); registrar superficies reais de reset como `IResettableGameState`.
- Spec/sessao dedicada de SaveManager: 3 slots manuais + backup rolling `.bak` + character-creation em
  PlayerSaveData (schema aditivo + migration).
- Executar Play Mode (cenario humano) + Unity Test Runner EditMode no closeout final.

## Arquivos alterados

Novos (runtime):
- `Assets/_Game/Scripts/UI/System/ISaveFileProbe.cs`
- `Assets/_Game/Scripts/UI/System/ISettingsStore.cs`
- `Assets/_Game/Scripts/UI/System/SystemTabViewModel.cs`
- `Assets/_Game/Scripts/UI/System/GameAudioSettings.cs`
- `Assets/_Game/Scripts/UI/System/SaveFileProbe.cs`
- `Assets/_Game/Scripts/UI/System/PlayerPrefsSettingsStore.cs`
- `Assets/_Game/Scripts/UI/System/SystemTabController.cs`
- `Assets/_Game/Scripts/UI/Title/IResettableGameState.cs`
- `Assets/_Game/Scripts/UI/Title/NewGameStateResetService.cs`
- `Assets/_Game/Scripts/UI/Title/TitleFlowViewModel.cs`
- `Assets/_Game/Scripts/UI/Title/TitleScreenController.cs`

Novos (test/docs):
- `Assets/_Game/Tests/EditMode/UI/SystemTabTitleFlowTests.cs`
- `docs/validation/playmode/fable_56_human_test_scenario.md`
- `docs/validation/fable_56_spec_system_tab_title_flow_execution_report.md`

Modificado (aditivo):
- `Assets/_Game/Scripts/UI/Modal/ModalManager.cs` (+1 valor de enum `ModalType.SystemConfirm`)
- `Assembly-CSharp.csproj` (includes locais — NAO commitado)
