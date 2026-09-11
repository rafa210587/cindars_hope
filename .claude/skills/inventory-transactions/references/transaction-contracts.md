# Inventory transaction contracts

## Leitura mínima

1. `CLAUDE.md`
2. Spec alvo
3. `Assets/_Game/Scripts/Inventory/InventoryManager.cs` — `TryAddItem`, `RemoveItem`, `SplitSlot`, `CanAddItem`, `CaptureSaveData`/`RestoreFromSaveData`
4. `Assets/_Game/Scripts/Inventory/InventorySlot.cs`, `InventoryAddResult.cs`
5. `Assets/_Game/Scripts/Items/Runtime/InventorySwapAdapter.cs` — adapter de exemplo já existente

## Sistemas existentes (reusar, não duplicar)

| Preciso de… | Já existe | Não criar |
|---|---|---|
| Adicionar item | `InventoryManager.TryAddItem(itemId, amount) → InventoryAddResult` | um novo `Add` que mexa em slots por fora |
| Checar antes de adicionar | `CanAddItem(itemId, amount)` / `GetAvailableCapacityFor` | recálculo próprio de capacidade |
| Remover item | `RemoveItem(itemId, amount) → bool` | remoção que esvazie slot sem `RebuildAggregate` |
| Dividir stack | `SplitSlot(slotIndex)` | split manual sem `FindFirstEmptySlot` |
| Snapshot de leitura | `GetAllItems() → List<InventoryItemSnapshot>` | DTO paralelo de leitura |
| Save/load | `CaptureSaveData()` / `RestoreFromSaveData()` (`InventorySaveData`) | novo schema de save |
| Consumir inventory de outro sistema | adapter sobre uma interface (ver `InventorySwapAdapter` : `IItemSwapInventory`) | acesso direto aos `_slots` |

Antes de criar qualquer classe nova, rode a skill `system-reuse-audit`.

## Regras de stack / capacidade

- **Stack máximo é por item:** `ItemDataSO.MaxStack` (clampado por `Mathf.Max(1, MaxStack)`). Nunca hardcode o tamanho do stack.
- **`TryAddItem` preenche stacks parciais primeiro, depois slots vazios** — preserve essa ordem (merge antes de alocar slot novo) para não fragmentar o inventory.
- **Capacidade efetiva** considera slots vazios + espaço livre em stacks do mesmo item (`GetAvailableCapacityFor`). Um item com bônus de slots (ex.: Pouch) usa `PouchCapacityGuard.EffectiveCapacity` e a regra overflow-safe (`EvaluateDropPouch`) — perder o item NUNCA destrói itens.
- **`SplitSlot`** exige `Amount >= 2` e um slot vazio; falha (retorna `false`) sem mutar se não houver destino.

## Transação atômica (sem perder/duplicar)

Regra central: **valide a transação inteira ANTES de mutar.** `TryAddItem` já faz isso — calcula `available` e retorna `InventoryAddResult(false, …, addedAmount: 0)` sem tocar slots se `available < amount`.

Para operações compostas (ex.: transferir A→B, ou consumir N itens + creditar 1):
1. Cheque viabilidade de **todos** os lados primeiro (`CanAddItem` no destino, `HasItem` na origem).
2. Só então execute remove + add.
3. Se um lado falhar no meio, **reverta** o lado já aplicado (re-add do que saiu).

```csharp
// Transferência segura origem→destino (sem perder item em falha parcial)
public bool TryTransfer(InventoryManager from, InventoryManager to, string itemId, int amount, out string failureReason)
{
    failureReason = string.Empty;
    if (!from.HasItem(itemId, amount)) { failureReason = "NotEnoughItems"; return false; }
    if (!to.CanAddItem(itemId, amount)) { failureReason = "DestinationFull"; return false; }

    if (!from.RemoveItem(itemId, amount)) { failureReason = "RemoveFailed"; return false; }
    var added = to.TryAddItem(itemId, amount);
    if (!added.Success)
    {
        from.TryAddItem(itemId, amount); // rollback: devolve à origem
        failureReason = "AddFailedRolledBack";
        return false;
    }
    return true;
}
```

Uma transação inválida é **gameplay esperado** (rule `error-handling-resilience`, categoria 1): retorne `bool`/`out string failureReason`, nunca exception, nunca no-op silencioso. O `FailureReason` vira feedback de HUD via `GameEventBus` (skill `game-feel-checklist`).

## Adapters para outros sistemas

Quest, economy e crafting NÃO devem tocar `InventoryManager._slots` nem chamar métodos do manager direto. Exponha uma interface estreita e um adapter, como já faz `InventorySwapAdapter : IItemSwapInventory` (`GetAmount`/`RemoveItem`/`AddItem`):

```csharp
public interface IItemSink   // crafting/economy crédita itens
{
    bool TryAddItem(string itemId, int amount, out string failureReason);
}
public interface IItemSource // quest/economy consome itens
{
    int GetAmount(string itemId);
    bool RemoveItem(string itemId, int amount);
}
```

O adapter encapsula `InventoryManager` e converte resultados para o contrato do consumidor. Isso mantém a comunicação desacoplada (rule `unity-architecture` — sem chamadas MonoBehaviour→MonoBehaviour de gameplay) e deixa o consumidor testável com um fake da interface.
