# WAVE_INTEGRATION_19 — Modal/Input Guard Matrix

**Date:** 2026-06-10

Auditoria de cada modal: push, pop, e bloqueio de dash/dodge/block.

---

## Como Funciona o Guard

`PlayerDashController`, `PlayerDodgeController`, `PlayerBlockController`, e `DirectionalDoubleTapDetector` verificam:

```csharp
if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true) return;
```

Isso bloqueia qualquer ação de movimento/combate enquanto qualquer modal estiver na stack.

---

## Matriz por Modal

| Modal | Classe | Opens? | Pushes ModalManager | Pop on Close | Blocks Dash | Blocks Dodge | Blocks Block | Blocks DoubleTap | Closes on Esc |
|---|---|---|---|---|---|---|---|---|---|
| Dialogue | DialogueModal | SIM | ModalType.Dialogue | SIM (TryPopModal) | SIM (via HasActiveModal) | SIM | SIM | SIM | SIM (Escape key) |
| QuestOffer | QuestOfferPanelController | SIM | ModalType.QuestOffer | SIM (TryPopModal) | SIM | SIM | SIM | SIM | SIM |
| QuestLog | QuestLogPanelController | SIM | ModalType.QuestLog (WAVE19 delta) | SIM (TryPopModal) | SIM | SIM | SIM | SIM | SIM (Escape key) |
| ShopMenu | ShopMenuModal | SIM | ModalType.ShopMenu | SIM (TryPopIfCurrent) | SIM | SIM | SIM | SIM | SIM |
| Buy | BuyPanel via ShopMenuModal | SIM | ModalType.Buy | SIM | SIM | SIM | SIM | SIM | SIM |
| Sell | SellPanel via ShopMenuModal | SIM | ModalType.Sell | SIM | SIM | SIM | SIM | SIM | SIM |
| Inventory | InventoryPanelController | SIM | ModalType.Inventory | SIM (TryPopModal) | SIM | SIM | SIM | SIM | SIM (Escape) |
| SkillTree | SkillTreeGameplayPanelController | SIM | ModalType.SkillTree | SIM (TryPopModal) | SIM | SIM | SIM | SIM | SIM |
| CharacterEquipment | CharacterEquipmentPanelController | SIM | ModalType.CharacterEquipment | SIM (TryPopModal) | SIM | SIM | SIM | SIM | SIM |

---

## Evidências de Código

### PlayerDashController (linha 39)

```csharp
if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true) return;
```

### PlayerDodgeController (linha 39)

```csharp
if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true) return;
```

### PlayerBlockController (linhas 37-41)

```csharp
if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
{
    if (_isBlocking) StopBlock();
    return;
}
```

### DirectionalDoubleTapDetector (linhas 37-43)

```csharp
if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
{
    for (var i = 0; i < _lastTapTime.Length; i++)
        _lastTapTime[i] = float.MinValue;
    return null;
}
```

---

## Delta WAVE19

| Item | Ação | Status |
|---|---|---|
| ModalType.QuestLog | Adicionado ao enum | BUILD_VALIDATED |
| QuestLogPanelController.Open() | Adicionado PushModal(ModalType.QuestLog) | BUILD_VALIDATED |
| QuestLogPanelController.Close() | Adicionado TryPopModal(ModalType.QuestLog) | BUILD_VALIDATED |

---

## Resultado

Todos os modais bloqueiam dash, dodge, block e double-tap via `HasActiveModal`.
QuestLog agora faz parte da stack modal (WAVE19 delta).
Nenhum vazamento de input identificado em código-fonte.

**Human validation pendente**: confirmar no Play Mode que nenhum input vaza durante modais abertos.
