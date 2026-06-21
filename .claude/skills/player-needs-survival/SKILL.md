---
name: player-needs-survival
description: Stamina, fome/hunger, sono/fatigue e consumo de comida — consumo por ação, regen, thresholds, penalidades, dormir (avança dia), comer (FoodConsumer), save dessas necessidades. Usar em specs de player condition, fatigue, sleep, hunger, stamina ou qualquer ação que gaste/restaure essas necessidades.
---

# Skill: Player Needs & Survival

O sistema de necessidades do player (fable_16, fable_18, WAVE_INTEGRATION_11) usa `StaminaManager`, `HungerManager` e `FoodConsumer` — todos MonoBehaviours no player prefab, coordenados pelo `GameBootstrap`. As necessidades são recursos com regen automática, thresholds e eventos de gameplay quando chegam a estados críticos.

## Quando usar

- Spec adiciona ação que consome stamina (`TrySpendStamina`).
- Spec altera taxa de regen, drain ou thresholds de fome/stamina.
- Spec implementa comer, dormir ou colapso por exaustão.
- Spec de save/load que inclua `currentHunger`, `currentStamina`, `currentFatigue`.
- Checklist de closeout: confirmar que ação que pode falhar por falta de stamina retorna `FailureReason` correto (não exceção).

## Por que existe

Verificar pré-condição de stamina/fome espalhada pelo código de gameplay cria duplicação. O padrão canônico é: a ação tenta gastar stamina via `TrySpendStamina` → false = ação recusada com `FailureReason` → a view exibe o motivo via `LocalizationService.Get(reason)` ou via HUD.

## Sistemas existentes (reusar, não duplicar)

| Classe | Papel |
|---|---|
| `StaminaManager` (`CindarsHope.Player`) | Stamina atual/máxima; `TrySpendStamina(amount)` → bool; `AddStamina`; `FullRecover`; regen automática por `_regenRate`; `SetMaxStamina(newMax, preserveRatio)` (F18); `ExternalRegenBonus` (passivas); publica `StaminaChangedEvent`; reseta no `DayStartedEvent` |
| `HungerManager` | Fome atual/máxima; `RestoreHunger(amount)`; drain por distância (`PlayerStepEvent`) e por dia (`DayStartedEvent`); threshold crítico (20% max) → `HungerCriticalEvent`; fome zero → `HungerEmptyEvent` + dano de HP; `DrainMultiplier` (F18); `RestoreFromSaveData` |
| `FoodConsumer` | Monobehaviour; tecla H (configurável); prioridade de alimento (`item_crop_carrot`, `item_crop_wheat`, `item_fish_common`); consome via `InventoryManager.RemoveItem`; chama `HungerManager.RestoreHunger` + `StaminaManager.AddStamina` + `StatusEffectManager.TryAddEffect`; aplica `AccessoryEffectRouter.ApplyFoodEffect` (F23 — Charm de Thandra +15%) |
| `PlayerNeedsBalanceSO` | SO de balanceamento: `BaseStaminaRegenRate`, `ZeroHungerRegenRate`, `GetStaminaRegenModifier(hunger)`, `ZeroHungerDamagePerSecond`, `IsZeroHunger` |
| `ConsumableManager` | Gerencia consumíveis avançados além do FoodConsumer |
| `ManaManager` | Análogo ao StaminaManager para mana (skills mágicas) |
| `PlayerVitalsApplier` | Aplica derived stats (F18): MaxHP/Stamina/Mana escalados, `CraftTimeReductionSource`, `ExperienceGainMultiplier` |
| `DerivedStatsCalculator` | Calcula MaxHP/Stamina/Mana derivados de Strength/Endurance/Intelligence |
| `PlayerManager` | HP atual/máximo; `DamageHP(amount)` chamado pelo `HungerManager` ao fome zerar |

## Procedimento

### Ação que consome stamina

```csharp
// Padrão canônico: tenta gastar → false = recusa com reason
if (!_staminaManager.TrySpendStamina(cost))
{
    failureReason = "not_enough_stamina"; // ou FailureReason.NoStamina
    GameEventBus.Publish(new PlayerActionFeedbackEvent(LocalizationService.Get(failureReason)));
    return false;
}
// executa a ação
```

Nunca verificar `CurrentStamina < cost` manualmente — `TrySpendStamina` já é atômico.

### Guardar stamina e fome no save

As managers já têm `RestoreFromSaveData` e expõem `CurrentStamina`/`CurrentHunger`/`MaxStamina`/`MaxHunger`. O save DTO (simples types):

```csharp
[Serializable]
public class PlayerNeedsData
{
    public int CurrentStamina;
    public int MaxStamina;
    public int CurrentHunger;
    public int MaxHunger;
}
```

Restore:
```csharp
_staminaManager.SetMaxStamina(data.MaxStamina);
_staminaManager.AddStamina(data.CurrentStamina - _staminaManager.CurrentStamina);
_hungerManager.RestoreFromSaveData(data.CurrentHunger, data.MaxHunger);
```

### Dormir (avança dia)

Dormir publica `DayStartedEvent` via `WorldTimeManager.AdvanceDay()`. `StaminaManager` assina `DayStartedEvent` e chama `FullRecover()` automaticamente. `HungerManager` assina e perde `_hungerLossPerDay`. Não é necessário restaurar stamina manualmente após dormir.

### Comer (FoodConsumer)

`FoodConsumer` já está no player prefab. Para adicionar alimento à priority list:

```csharp
private static readonly string[] FoodPriority = { "item_crop_carrot", "item_crop_wheat", "item_fish_common", "meu_alimento_novo" };
```

Para forçar consumo programático (NPC servindo refeição, estalagem):

```csharp
_hungerManager.RestoreHunger(amount);
_staminaManager.AddStamina(amount);
```

### Colapso por exaustão (fable_16)

Stamina zero não provoca colapso diretamente — impede ações. O colapso às 02:00 (fatigue) foi implementado em fable_16 como `FatigueSystem` com limiar de hora e `PlayerCollapseEvent`. Reutilize o `FatigueSystem`; não reimplemente o timer.

## Testes

`StaminaManager` e `HungerManager` têm lógica determinística → EditMode com `Initialize`:

```csharp
var stamina = go.AddComponent<StaminaManager>();
stamina.Initialize(100, 50);
Assert.IsFalse(stamina.TrySpendStamina(60));  // 50 < 60
Assert.IsTrue(stamina.TrySpendStamina(30));    // 50 >= 30
Assert.AreEqual(20, stamina.CurrentStamina);
```

`HungerManager.RestoreFromSaveData` e thresholds críticos também são testáveis fora de Play Mode.

## Regras

- Ação com stamina insuficiente: retorna false + publica `PlayerActionFeedbackEvent` — **nunca** exceção nem skip silencioso.
- Não verificar `CurrentStamina` manualmente para "guardar" o valor — use sempre `TrySpendStamina` (atômico).
- Regen de stamina é automaticamente reduzida por `ZeroHungerRegenRate` quando fome = 0 (via `PlayerNeedsBalanceSO`) — não duplicar essa lógica.
- `DrainMultiplier` de fome (F18) é aplicado dentro de `HungerManager.LoseHunger` — não escalar manualmente.
- Sem refs Unity em save DTOs das necessidades (tipos simples: int, float, bool).

## Relacionados

- `(skill: time-calendar-weather)` — dormir = `DayStartedEvent`; stamina/fome reagem ao dia
- `(skill: game-feel-checklist)` — feedback de stamina baixa / bloqueio de ação via HUD
- `(skill: save-load-pattern)` — `PlayerNeedsData` DTO, `RestoreFromSaveData`
- `(skill: editmode-test-authoring)` — consumo/regen/threshold são lógica determinística
- `(rule: unity-architecture)` — save DTOs sem refs Unity; comunicação via GameEventBus
