# Retro-Spec 06 — WI-23 Gameplay HUD Canvas (headless, event-driven)

> **Spec ID:** `spec_retro_06_wi23_gameplay_hud_canvas`
> **Status:** RETRO_DOCUMENTED (código implementado em 2026-06; spec escrita a posteriori para reconstrutibilidade)
> **Tipo:** Retro-spec
> **Domínio:** UI / HUD
> **Código que documenta:**
> - `Assets/_Game/Scripts/UI/HUD/GameplayHudBootstrap.cs`
> - `Assets/_Game/Scripts/UI/HUD/GameplayHudCanvasController.cs`
> - `Assets/_Game/Scripts/UI/HUD/GameplayHudRuntimeBinder.cs`
> - `Assets/_Game/Scripts/UI/HUD/GameplayFeedbackService.cs`
> - `Assets/_Game/Scripts/UI/HUD/GameplayFeedbackMessage.cs`
> - `Assets/_Game/Scripts/UI/HUD/HudVisibilityController.cs`
> - `Assets/_Game/Scripts/UI/HUD/Views/{StatusBarsHudView, QuestTrackerHudView, ActiveSkillSlotsHudView, InteractionPromptHudView, FeedbackToastHudView, ModalBlockerHudView}.cs`
> - `Assets/_Game/Scripts/Core/Events/HudEvents.cs` (HudFeedbackUpdatedEvent, HudVisibilityChangedEvent)
> **Evidência de execução:** `docs/validation/WAVE_INTEGRATION_23_UI_HUD_CANVAS_REPORT.md` (BUILD_VALIDATED_HUD_CANVAS_READY_PENDING_HUMAN_PLAYMODE; validador `ValidateWave23UiHudCanvasFinalization` com 27 checks; matrizes DECISION/DATA_BINDING/FEEDBACK_COVERAGE/MODAL_VISIBILITY)
> **Supersedida/complementada por:** `fable_14` (UI Canvas screens integration — dará corpo visual uGUI aos views headless; esta wave é pré-requisito documental), `fable_20` (calendar/clock HUD).

---

# /speckit.specify

## Contexto

Até a WAVE22 o único HUD era o `DebugHud` (IMGUI, ~750 linhas), apropriado para debug mas não para player. A WAVE_INTEGRATION_23 criou a camada de HUD de gameplay **headless** (lógica/binding/eventos completos, sem visual uGUI ainda): bootstrap sem edição de cena, viewmodel alimentado por eventos do GameEventBus, serviço de feedback com fila e prioridade, visibilidade reativa a modais e 6 views componentizados. O `DebugHud` foi mantido intacto (complementado, não substituído). O visual definitivo chega via fable_14.

## Comportamento implementado

### 1. Bootstrap (`GameplayHudBootstrap`)

- `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`: se `GameplayHudCanvasController.Instance == null`, cria GameObject `GameplayHudCanvas` `DontDestroyOnLoad` com os 4 componentes (controller, binder, feedback service, visibility controller), instancia um `GameplayHudViewModel` novo e chama `controller.Initialize(...)`. Zero edição de cena/prefab.

### 2. Controller (`GameplayHudCanvasController`)

- Singleton (`Instance`); duplicatas se autodestroem.
- `Initialize(viewModel, binder, feedbackService, visibilityController)`: injeta o viewmodel no binder e anexa os 6 views como componentes do mesmo GameObject (`AttachViews`), inicializando cada um com o viewmodel (toast recebe o feedback service). `IsInitialized = true`.

### 3. Binder (`GameplayHudRuntimeBinder`) — eventos → viewmodel

Subscreve 9 eventos no OnEnable (com Unsubscribe simétrico):

| Evento | Efeito no viewmodel |
|---|---|
| `HPChangedEvent` | `Hp`, `MaxHp` |
| `StaminaChangedEvent` | `Stamina`, `MaxStamina` |
| `ManaChangedEvent` | `Mp`, `MaxMp`, `ShowMp = MaxMana > 0` |
| `HungerChangedEvent` | `HungerCompact = Current/Max` (1 se Max <= 0) |
| `InteractionPromptChangedEvent` | `ContextPrompt { IsVisible = HasCandidate, DescriptionKey = Prompt, ActionKey = "E" }` |
| `QuestAcceptedEvent` | `QuestPrompt = "Quest ativa: {id}"` |
| `QuestObjectiveProgressedEvent` | `QuestPrompt = "{id}: {atual}/{requerido}"` |
| `QuestReadyToCompleteEvent` | `QuestPrompt = "Entregar: {id}"` |
| `QuestCompletedEvent` | `QuestPrompt = ""` |

- `Update()`: polling dos 4 slots ativos no `SkillTreeManager.State` — reconstrói `ActiveSkillSlots` com labels de tecla `R, T, Y, G` e `IsEquipped`.

### 4. Feedback service (`GameplayFeedbackService`) — fila com prioridade

Subscreve 10 eventos:

| Evento | Mensagem | Duração | Prioridade |
|---|---|---|---|
| `PlayerActionFeedbackEvent` | `evt.Message` | `evt.DurationSeconds` | Normal |
| `GameSavedEvent` | "Jogo salvo." / falha | 3s | Normal |
| `GameLoadedEvent` | "Jogo carregado." / falha | 3s | Normal |
| `QuestAcceptedEvent` | "Quest aceita: {id}" | 4s | **Important** |
| `QuestCompletedEvent` | "Quest concluída: {id}" | 4s | **Important** |
| `QuestRewardClaimedEvent` | "Recompensa: {gold}g" / genérica | 3s | Normal |
| `EnemyKilledEvent` | "Inimigo derrotado." / "Loot: {item} x{n}" | 2s | Low |
| `CaveLevelEnteredEvent` | "Caverna — nível {n}" | 3s | Normal |
| `CaveExitedEvent` | "Retornando à superfície..." | 3s | Normal |
| `NotificationToastRequestedEvent` | `evt.Message` | 3s | Normal |

- Política da fila: sem mensagem ativa → ativa imediatamente; `Important` → interrompe a ativa; demais → enfileiram. Ao expirar (`Time.time >= activeUntil`), avança a fila.
- Cada ativação publica `HudFeedbackUpdatedEvent(text, duration, priority)` — o view de toast é desacoplado do serviço.

### 5. Visibilidade (`HudVisibilityController`)

- Polling por frame de `ModalManager.HasActiveModal` (via `GameBootstrap.Instance`); na transição de estado publica `HudVisibilityChangedEvent(visible)`. `IsHudVisible` exposto. HUD some quando qualquer modal está aberto.

### 6. Views (6, headless — namespace `CindarsHope.UI.HUD.Views`)

- `StatusBarsHudView` — barras HP/Stamina/Mana do viewmodel; subscreve `HudVisibilityChangedEvent`.
- `QuestTrackerHudView` — exibe `QuestPrompt`.
- `ActiveSkillSlotsHudView` — slots ativos; integra `FinalHudGuardValidator` (guard pré-existente, reusado).
- `InteractionPromptHudView` — prompt contextual "E".
- `FeedbackToastHudView` — subscreve `HudFeedbackUpdatedEvent`; loga no Console enquanto headless.
- `ModalBlockerHudView` — gate de visibilidade (`IsBlocking`).

Todos anexados pelo controller; nenhum cria Canvas/uGUI ainda (DEFERRED para fable_14).

## Critérios de aceite (verificáveis no código atual)

1. Entrar em Play Mode em qualquer cena cria `GameplayHudCanvas` DontDestroyOnLoad sem nenhuma edição de cena, com controller+binder+feedback+visibility+6 views.
2. Os 9 eventos do binder atualizam o viewmodel conforme a tabela; slots ativos refletem `SkillTreeManager.State` com teclas R/T/Y/G.
3. Feedback respeita a política de fila (Important interrompe; Normal/Low enfileiram) e publica `HudFeedbackUpdatedEvent` a cada ativação.
4. Abrir/fechar modal publica `HudVisibilityChangedEvent` exatamente nas transições.
5. `DebugHud`, `ModalManager`, `GameBootstrap` e `GameplayHudViewModel` pré-existentes não foram modificados (audit WI-23).

---

# /speckit.plan

## Arquitetura real

| Arquivo | Responsabilidade |
|---|---|
| `GameplayHudBootstrap.cs` | Auto-criação runtime (padrão *RuntimeBootstrap) |
| `GameplayHudCanvasController.cs` | Singleton coordenador; composição dos views |
| `GameplayHudRuntimeBinder.cs` | 9 eventos → `GameplayHudViewModel`; polling de skill slots |
| `GameplayFeedbackService.cs` + `GameplayFeedbackMessage.cs` | Fila de toasts com 3 prioridades (Low/Normal/Important) |
| `HudVisibilityController.cs` | Modal → `HudVisibilityChangedEvent` |
| `Views/*.cs` (6) | Apresentação headless por preocupação |
| `Core/Events/HudEvents.cs` | `HudFeedbackUpdatedEvent`, `HudVisibilityChangedEvent` |

## Contratos

- `GameplayHudCanvasController.Instance` / `IsInitialized` / `ViewModel` — superfície para fable_14 e validadores.
- `GameplayFeedbackService.CurrentMessage : GameplayFeedbackMessage` — leitura do toast ativo.
- `HudVisibilityController.IsHudVisible : bool`.
- Eventos novos: `HudFeedbackUpdatedEvent(text, duration, priority)`, `HudVisibilityChangedEvent(visible)`.
- Consumo exclusivo via GameEventBus + `GameBootstrap.Instance` (ModalManager/SkillTreeManager) — sem global search, sem refs de cena.

## Decisões e invariantes

- **Headless-first**: lógica/binding validados por build antes do visual; fable_14 pluga uGUI nos mesmos views/viewmodel.
- **Complementar, não substituir**: `DebugHud` IMGUI permanece para debug.
- **View desacoplado do serviço**: toast consome `HudFeedbackUpdatedEvent`, nunca o serviço diretamente.
- **Prioridade mínima viável**: 3 níveis; Important interrompe (quests), Low é descartável visualmente (kills).
- **Sem edição de cena/prefab/assets** (gate WI-23: scene edits NONE).

---

# /speckit.tasks

## Reconstrução (passos para refazer do zero)

1. Criar `HudEvents.cs` com os 2 eventos.
2. Criar `GameplayFeedbackMessage` (text, duration, priority enum Low/Normal/Important) e `GameplayFeedbackService` com a tabela de 10 eventos e a política de fila.
3. Criar `GameplayHudRuntimeBinder` com as 9 subscriptions e o polling de slots (R/T/Y/G).
4. Criar `HudVisibilityController` (polling de modal, publish na transição).
5. Criar os 6 views headless e o `GameplayHudCanvasController` que os anexa via `AddComponent` + `Initialize(viewModel)`.
6. Criar `GameplayHudBootstrap` com `RuntimeInitializeOnLoadMethod` e guarda de singleton.
7. Validar com `ValidateWave23UiHudCanvasFinalization` (27 checks) e o checklist humano WI-23.

## Débitos conhecidos

- Views são headless: nenhum Canvas/uGUI renderiza para o player ainda (fable_14 fecha; toasts visíveis hoje apenas via Console/DebugHud).
- `GameplayHudRuntimeBinder` faz polling por frame dos skill slots (sem evento de mudança de slot — otimização futura).
- `HudVisibilityController` faz polling do ModalManager (sem evento de push/pop de modal exposto).
- Sem testes automatizados (lifecycle Unity); cobertura via checklist humano `WAVE_INTEGRATION_23_HUMAN_PLAYMODE_CHECKLIST.md` (27 passos).
