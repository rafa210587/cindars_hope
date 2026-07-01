---
name: action-feedback-pipeline
description: Pipeline completo de feedback de falha/sucesso de ação ao player — publicar PlayerActionFeedbackEvent, fila de HUD com prioridade, toast visual e SFX automático. Usar em specs que recusam ação (stamina, cooldown, item ausente) ou adicionam novo feedback de gameplay.
---

# Skill: Pipeline de Feedback de Ação

Quando uma ação de gameplay falha ou precisa de feedback ao player, o pipeline canônico é: publicar `PlayerActionFeedbackEvent` via `GameEventBus` → `GameplayFeedbackService` enfileira com prioridade → `FeedbackToastHudView` exibe o toast → `SfxEventBridge` dispara `SfxCategory.UiToast` automaticamente. Nenhum sistema de gameplay toca UI ou `AudioSource` diretamente.

## Quando usar

- Spec adiciona recusa de ação (stamina baixa, cooldown, sem item, posição inválida, pré-condição falhada).
- Spec de NPC, loja ou interactable que precisa exibir mensagem de falha/sucesso ao player.
- Spec adiciona novo tipo de toast/feedback ao HUD.
- Spec menciona "feedback", "toast", "mensagem de HUD", "player notification", "action denied".

## Fluxo end-to-end

```
Código de gameplay detecta falha
  └─ GameEventBus.Publish(new PlayerActionFeedbackEvent(LocalizationService.Get("chave.falha")))
          ↓
GameplayFeedbackService.OnPlayerFeedback()
  └─ Enfileira com FeedbackMessagePriority.Normal (ou .Important para preemptar)
  └─ Publica HudFeedbackUpdatedEvent quando ativa
          ↓
  ┌───────┴────────────┬────────────────────────────┐
  ↓                    ↓                            ↓
FeedbackToastHudView  NotificationToastController  SfxEventBridge
(toast visual)        (fallback IMGUI, até 4)      → SfxCategory.UiToast
                                                    (cooldown gate anti-spam)
```

## Publicar o evento (padrão canônico)

```csharp
// CORRETO: localização + evento via GameEventBus
if (!_staminaManager.TrySpendStamina(cost))
{
    GameEventBus.Publish(new PlayerActionFeedbackEvent(
        LocalizationService.Get("action.mine.no_stamina")));
    return false; // nunca fail silencioso
}

// ERRADO: string hardcoded
GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente para minerar."));

// ERRADO: chamar UI diretamente (viola rule unity-architecture)
_toastController.ShowMessage("Stamina insuficiente");

// ERRADO: chamar AudioManager diretamente para feedback de ação
AudioManager.Instance.PlaySfx(SfxCategory.UiToast);
```

## Adicionar nova mensagem de falha (3 passos)

**1. Registrar a chave em `LocalizationStringTable`** (skill: localization-authoring):

```csharp
{ "action.mine.no_stamina", "Stamina insuficiente para minerar." },
```

**2. Publicar no controller de ação:**

```csharp
if (!_staminaManager.TrySpendStamina(cost))
{
    GameEventBus.Publish(new PlayerActionFeedbackEvent(
        LocalizationService.Get("action.mine.no_stamina")));
    return false;
}
```

**3. SFX e toast são automáticos** — não configurar por ação.

## Prioridade

```csharp
// Normal (padrão 2s): enfileira após a mensagem ativa
new PlayerActionFeedbackEvent(message)

// Duração customizada
new PlayerActionFeedbackEvent(message, durationSeconds: 3.5f)
```

`GameplayFeedbackService` gerencia a fila internamente. Para preemptar a mensagem ativa com algo crítico (ex.: boss iniciado, missão falhada), publique via `HudFeedbackUpdatedEvent` com `Priority = (int)FeedbackMessagePriority.Important` — mas use com parcimônia.

## Sistemas existentes (não duplicar)

| Classe | Papel |
|---|---|
| `PlayerActionFeedbackEvent` | Struct imutável: `Message` + `DurationSeconds` (padrão 2s) — publicar este |
| `GameplayFeedbackService` | Fila de prioridade; assina o evento; publica `HudFeedbackUpdatedEvent` |
| `FeedbackToastHudView` | Visual do toast; assina `HudFeedbackUpdatedEvent` |
| `NotificationToastController` | Fallback IMGUI; até 4 toasts empilhados; duração 2.5s |
| `SfxEventBridge` | Assina `PlayerActionFeedbackEvent` → `SfxCategory.UiToast` com cooldown gate |
| `HudFeedbackUpdatedEvent` | Evento intermediário interno — não publicar manualmente |

## Regras

- **Toda recusa de ação** publica `PlayerActionFeedbackEvent` — nunca fail silencioso (rule: error-handling-resilience, Category 1).
- Mensagem sempre via `LocalizationService.Get(key)` — nunca string hardcoded no gameplay.
- Nunca chamar `FeedbackToastHudView`, `NotificationToastController` ou `AudioManager.PlaySfx` diretamente do gameplay.
- `HudFeedbackUpdatedEvent` é interno ao pipeline — não publicar manualmente fora de `GameplayFeedbackService`.

## Quando NÃO usar

- **Notificações de sistema** (save concluído, loading screen) → usar `HudFeedbackUpdatedEvent` diretamente com prioridade Normal; não é "falha de ação de gameplay".
- **Logs de debug** internos → nunca via evento de HUD; usar `Debug.Log` ou `UnityEngine.Debug`.
- **Notificações de quest** (quest completada, novo objetivo) → usar os eventos de quest próprios (`QuestCompletedEvent`, `QuestObjectiveUpdatedEvent`); o pipeline de quest já tem sua própria UI.
- **Boss announcements** → `BossPhaseLogic` publica eventos próprios; não usar `PlayerActionFeedbackEvent` para isso.

## Relacionados

- `(skill: localization-authoring)` — como criar a chave de mensagem
- `(skill: audio-event-wiring)` — como `SfxEventBridge` mapeia evento → som (SfxEventMap)
- `(skill: game-feel-checklist)` — checklist completo de feedback de gameplay (hit, recusa, sucesso, morte)
- `(rule: unity-architecture)` — publicar via GameEventBus, nunca chamar UI/audio diretamente
- `(rule: error-handling-resilience)` — Category 1: gameplay failure → bool + feedback via evento, nunca exceção
