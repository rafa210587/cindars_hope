# Execution Report — fable_77: SceneFadeOverlay

**Spec:** `fable_77_spec_scene_fade_transition`
**Status:** BUILD_VALIDATED
**Date:** 2026-06-21

## Acceptance Criteria

| Critério | Evidência |
|----------|-----------|
| SceneFadeOverlayBootstrap cria canvas DontDestroyOnLoad | SceneFadeOverlayBootstrap.cs — RuntimeInitializeOnLoadMethod(AfterSceneLoad) cria go + DontDestroyOnLoad |
| CanvasGroup.alpha = 0 no estado inicial | SceneFadeOverlayController.Awake() — `_overlay.alpha = 0f` |
| Fade-in ao SceneTransitionStartedEvent | OnEnable/Subscribe + FadeToBlackRoutine |
| Fade-out ao SceneTransitionCompletedEvent | OnEnable/Subscribe + FadeFromBlackRoutine |
| FADE_LOADING_DEFERRED_WITH_REASON removido | SceneTransitionRouter.cs — comentário atualizado |
| Assembly-CSharp 0E/0W | EXIT 0 |
| Assembly-CSharp-Editor 0E | EXIT 0 |
| validate_docs exit 0 | PASS |

## Arquivos criados/modificados

| Arquivo | Ação |
|---------|------|
| `Assets/_Game/Scripts/World/Scenes/SceneFadeOverlayController.cs` | NOVO |
| `Assets/_Game/Scripts/World/Scenes/SceneFadeOverlayBootstrap.cs` | NOVO |
| `Assets/_Game/Scripts/World/Scenes/SceneTransitionRouter.cs` | EDIT (remoção do guard FADE_LOADING_DEFERRED) |
| `Assets/_Game/Tests/EditMode/World/SceneFadeOverlayTests.cs` | NOVO |

## Sistemas auditados

- `SceneTransitionRouter` — static, publica `SceneTransitionStartedEvent`, não cria canvas
- `PlayerSpawnResolver` — publica `SceneTransitionCompletedEvent` em `ApplySpawn` ✅
- `GameplayHudBootstrap` — padrão DontDestroyOnLoad de referência (copiado)
- `GameEventBus` — Subscribe/Unsubscribe padrão OnEnable/OnDisable ✅

## Design canônico

- Overlay Canvas `sortingOrder = 9999` (acima de ModalManager e DebugHud)
- `Time.unscaledDeltaTime` — funciona com TimeScale = 0
- `GraphicRaycaster.enabled = false` quando alpha = 0 — sem bloqueio de input em estado normal
- Timeout de 5s em FadeToBlackRoutine — auto-fade-out se `SceneTransitionCompletedEvent` não chegar

## Testing Quality Gate

```
Changed runtime code:           YES
Changed deterministic logic:    NO
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES — SceneFadeOverlayTests.cs (estrutural)
Automated tests command:        dotnet test / Unity Test Runner
Manual Play Mode scenario:      Play Mode — navegar Farm → Town → Cave; confirmar fade preto
Justification if no tests:      N/A
Residual risk:                  timing do fade pode precisar ajuste em Play Mode real;
                                PlayerSpawnResolver precisa estar na cena para publicar
                                SceneTransitionCompletedEvent (debt WAVE13 wiring)
```
