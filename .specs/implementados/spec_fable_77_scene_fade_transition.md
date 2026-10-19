# SPEC — Fade de Transição de Cena (SceneFadeOverlay)

> **Spec ID:** `fable_77_spec_scene_fade_transition`
> **Status:** A implementar
> **Wave:** FABLE Batch 12
> **Priority:** P2
> **Type:** Runtime / UX
> **Domain:** Scene Management / UI
> **Parallelizable:** YES
> **Parallel group:** fable_bloco_ux
> **Can run with:** fable_74, fable_75, fable_76
> **Must not run with:** fable_13 (scene wiring WAVE13 — SceneTransitionRouter lock)
> **Repo lock scope:**
>   `Assets/_Game/Scripts/World/Scenes/SceneTransitionRouter.cs`,
>   `Assets/_Game/Scripts/World/Scenes/SceneFadeOverlayController.cs` (novo),
>   `Assets/_Game/Scripts/Core/Events/SceneTransitionStartedEvent.cs`,
>   `Assets/_Game/Scripts/Core/Events/SceneTransitionCompletedEvent.cs`
> **Depends on:**
>   - WAVE_INTEGRATION_13 (SceneTransitionRouter/Started/Completed events já implementados)
>   - WAVE_INTEGRATION_19 (GameplayHudBootstrap DontDestroyOnLoad — padrão de canvas persistente)
> **Blocks:** nenhum
> **Scope:** adicionar um overlay canvas preto que faz fade-in ao receber
> `SceneTransitionStartedEvent` e fade-out ao receber `SceneTransitionCompletedEvent`,
> cobrindo o hard cut entre cenas. Não reescreve nem modifica a lógica de roteamento.
> **Out of scope:** barra de progresso de loading, arte de loading screen, transições
> animadas por cena (cada cena tem sua animação), animação de câmera, fade em Play Mode
> do Unity Editor (aceitável ser mais lento).

required_adrs: [ADR-0007-event-bus-gameplay-communication.md]
required_game_rules: [ui_rules.md]

---

# /speckit.specify

## Contexto

`SceneTransitionRouter.Execute` chama `SceneManager.LoadScene` de forma síncrona sem qualquer
overlay de fade. O comentário explícito no arquivo diz:

```csharp
/// Does NOT do fading — FADE_LOADING_DEFERRED_WITH_REASON: no fade system.
```

Os eventos já existem:
- `SceneTransitionStartedEvent` — publicado ANTES de `LoadScene`; campos: `SourceSceneName`,
  `TargetSceneName`, `TargetSpawnId`
- `SceneTransitionCompletedEvent` — deve ser publicado após spawn completo no destino
  (checar `PlayerSpawnResolver` — é lá que `ClearTransitionGuard` é chamado)

O padrão de canvas persistente (`DontDestroyOnLoad`) já existe em `GameplayHudBootstrap`
(WAVE_INTEGRATION_23).

## Problema

A transição entre Fazenda, Cidade e Caverna é um hard cut instantâneo — o jogo parece
inacabado. Um fade to black + fade from black é o mínimo de polish para que a transição
pareça intencional.

## Objetivo

Ao final desta spec:

1. Um `Canvas` preto com `CanvasGroup` (alpha 0 por padrão) persiste via `DontDestroyOnLoad`.
2. Ao receber `SceneTransitionStartedEvent`: fade alpha 0 → 1 em `FadeOutDuration` segundos.
3. Ao receber `SceneTransitionCompletedEvent`: fade alpha 1 → 0 em `FadeInDuration` segundos.
4. O comentário `FADE_LOADING_DEFERRED_WITH_REASON` é removido de `SceneTransitionRouter.cs`.
5. Build PASS.

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/World/Scenes/SceneTransitionRouter.cs
Assets/_Game/Scripts/Core/Events/SceneTransitionStartedEvent.cs
Assets/_Game/Scripts/Core/Events/SceneTransitionCompletedEvent.cs  (verificar onde é publicado)
Assets/_Game/Scripts/Player/PlayerSpawnResolver.cs                  (ClearTransitionGuard call)
Assets/_Game/Scripts/UI/HUD/Runtime/GameplayHudBootstrap.cs         (padrão DontDestroyOnLoad)
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
SceneTransitionRouter.cs   — static; publica SceneTransitionStartedEvent; chama LoadScene síncrono
SceneTransitionStartedEvent — SourceSceneName, TargetSceneName, TargetSpawnId
SceneTransitionCompletedEvent — publicado por PlayerSpawnResolver após ClearTransitionGuard
GameplayHudBootstrap       — padrão: RuntimeInitializeOnLoadMethod(AfterSceneLoad) + DontDestroyOnLoad
ModalManager               — sortingOrder alto; o overlay precisa estar acima de tudo
```

## Tarefas

### T1 — SceneFadeOverlayController

- [ ] Criar `Assets/_Game/Scripts/World/Scenes/SceneFadeOverlayController.cs`:
  ```csharp
  namespace CindarsHope.World.Scenes
  {
      public sealed class SceneFadeOverlayController : MonoBehaviour
      {
          [SerializeField] private CanvasGroup _overlay;
          [SerializeField] private float _fadeOutDuration = 0.25f;  // Start → Black
          [SerializeField] private float _fadeInDuration  = 0.35f;  // Black → Clear
          // Subscribe SceneTransitionStartedEvent  → StartCoroutine(FadeToBlack)
          // Subscribe SceneTransitionCompletedEvent → StartCoroutine(FadeFromBlack)
      }
  }
  ```
- [ ] `FadeToBlack`: lerp alpha 0 → 1 ao longo de `_fadeOutDuration`; usa `Time.unscaledDeltaTime`
      (para funcionar mesmo com TimeScale 0)
- [ ] `FadeFromBlack`: lerp alpha 1 → 0 ao longo de `_fadeInDuration`
- [ ] Guard: se `SceneTransitionCompletedEvent` chegar enquanto `FadeToBlack` ainda está
      rodando, cancela a coroutine de fade-in até o próximo evento
- [ ] O canvas tem `sortingOrder = 9999` para ficar acima de ModalManager e DebugHud

### T2 — SceneFadeOverlayBootstrap

- [ ] Criar `SceneFadeOverlayBootstrap` com `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`
- [ ] Cria um `GameObject("SceneFadeOverlay")` com:
      - `Canvas` (ScreenSpace-Overlay, sortingOrder 9999)
      - `CanvasScaler`
      - `GraphicRaycaster` (bloqueio de input durante fade)
      - `CanvasGroup` com alpha=0
      - `Image` preenchendo o canvas inteiro, cor preta
      - `SceneFadeOverlayController` wired ao `CanvasGroup`
- [ ] `DontDestroyOnLoad(go)` + singleton guard (se já existe, destrói duplicata)

### T3 — Checar SceneTransitionCompletedEvent

- [ ] Verificar se `SceneTransitionCompletedEvent` é publicado por `PlayerSpawnResolver`
      após o spawn; se não, publicar em `PlayerSpawnResolver` logo após `ClearTransitionGuard()`
- [ ] Se já for publicado: apenas confirmar e documentar

### T4 — Remover guard do SceneTransitionRouter

- [ ] Remover o comentário `/// Does NOT do fading — FADE_LOADING_DEFERRED_WITH_REASON: no fade system.`
      de `SceneTransitionRouter.cs`

### T5 — EditMode test

- [ ] `SceneFadeOverlayBootstrapTests.cs` em `Assets/_Game/Tests/EditMode/`:
      - bootstrap inicializa sem NullReferenceException
      - `CanvasGroup.alpha` começa em 0

### T6 — Gate de build

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit 1 }
.\tools\docs\validate_docs.ps1
if ($LASTEXITCODE -ne 0) { exit 1 }
```

## Riscos de regressão

| Risco | Mitigação |
|-------|-----------|
| Overlay persiste visível se `SceneTransitionCompletedEvent` não for publicado | Timeout fallback: se alpha=1 por >5s, forçar fade-out |
| `GraphicRaycaster` bloqueia input permanentemente se alpha>0 | Desativar Raycaster quando alpha=0 (`_raycaster.enabled = alpha > 0.01f`) |
| Duplicate overlay se cena reloada (Unity Editor) | Singleton guard no Bootstrap destrói duplicata no `Awake` |

## Critérios de aceitação

1. `SceneFadeOverlayBootstrap` cria canvas preto DontDestroyOnLoad no boot
2. `CanvasGroup.alpha = 0` no estado inicial
3. Transição entre cenas cobre o hard cut com fade (visível em Play Mode)
4. Assembly-CSharp 0E/0W; validate_docs exit 0

## Stop conditions

- `SceneTransitionCompletedEvent` não existe no projeto → criar antes de prosseguir
- Build falha → parar

## Report obrigatório

Criar: `docs/validation/fable_77_scene_fade_transition_execution_report.md`

```text
Testing Quality Gate
────────────────────
Changed runtime code:           YES
Changed deterministic logic:    NO
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES (bootstrap init test)
Automated tests command:        dotnet test (EditMode)
Manual Play Mode scenario:      docs/validation/fable_77_playmode_scenario.md
Justification if no tests:      N/A
Residual risk:                  fade timing pode precisar ajuste em Play Mode real
```
