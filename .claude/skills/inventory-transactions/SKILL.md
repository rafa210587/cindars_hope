---
name: inventory-transactions
description: Regras de stack/split/move/merge, capacidade e transações seguras sobre o InventoryManager existente, com snapshot de save por id+quantidade e adapters para quest/economy/crafting consumirem o inventory via interface. Use em specs de inventory/storage/item (04_ui_inventory_items_tooltips, 04_ui_storage_chest_transfer, 06_item_definition_tags_quality_rarity) ou em qualquer mudança de add/remove/stack/move/capacity de itens.
---

# Skill: Transações de Inventory

O projeto já tem um `InventoryManager` (slot-based, `MaxCapacity = 30`) com `TryAddItem`/`RemoveItem`/`SplitSlot`/`DestroySlot` retornando `InventoryAddResult` ou `bool`, agregando por `RebuildAggregate()` e publicando `InventoryChangedEvent`. Esta skill garante que toda nova operação reuse esse manager (NÃO crie um inventory paralelo), preserve a atomicidade (nada perdido/duplicado em falha parcial) e persista só id+quantidade.

## Quando usar

A tarefa toca:
- Stack/split/move/merge de itens, ou capacidade/slots (`MaxStack`, `EnsureCapacity`, `GetAvailableCapacityFor`)
- `InventoryManager`, `InventorySlot`, `InventoryAddResult`, `InventoryItemSnapshot`
- Transferência de itens entre inventories (storage/chest)
- Outro sistema (quest, economy, crafting) precisa consumir/adicionar itens
- Specs: `04_ui_inventory_items_tooltips`, `04_ui_storage_chest_transfer`, `06_item_definition_tags_quality_rarity`

## Quando NÃO usar

- Persistência genérica sem regra de inventory → use a skill `save-load-pattern`.
- A tela/modal de inventory em si (open/close/Esc/input block) → use a skill `ui-modal-stack`.
- Catálogo em massa de itens (definir 60 itens como SO) → use a skill `data-catalog-authoring`.

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

## Interação com save

- O snapshot persiste **só id + quantidade por slot** (`InventorySlotSaveData`: `SlotIndex`, `ItemId`, `Amount`, `IsEquipped`, `EquipmentBindingId`). Nenhum `ItemDataSO`, `Sprite`, `GameObject` (rule `save-dto-simple-types-only`).
- `RestoreFromSaveData` resolve cada id pelo `ItemDatabaseSO` (`TryGetItemData`) e **descarta com warning** ids desconhecidos — fallback seguro, sem crash (rule `error-handling-resilience`, categoria 2).
- Save legado sem `Slots` cai no path `RestoreLegacyItems` (lista id+amount). Preserve esse fallback ao mudar o schema.
- Mudança de schema de inventory → seguir a skill `save-load-pattern` (version + migration) e documentar em `docs/validation/`.

## Testes

O `testing-quality-gate` exige EditMode tests para deterministic logic de inventory transactions (skill `editmode-test-authoring`). Cubra no mínimo:
- Stack: add além de `MaxStack` cria/preenche o número correto de slots; merge em stack parcial antes de slot novo.
- Capacidade cheia: `TryAddItem` retorna `Success=false` e `AddedAmount=0` **sem** mutar slots.
- Split: `SplitSlot` com `Amount>=2` e slot livre divide pela metade; sem slot livre retorna `false` sem mutar.
- Atomicidade: transferência com destino cheio devolve tudo à origem (contagem total conservada).
- Round-trip de save: `CaptureSaveData()` → `RestoreFromSaveData()` reproduz contagens e bindings; id desconhecido é ignorado sem perder os válidos.

## Regressões comuns

- Mutar `_slots` e esquecer `RebuildAggregate()` → `Items`/`GetAmount` ficam dessincronizados.
- Add/remove sem publicar `InventoryChangedEvent` → HUD e `EquipmentManager` (que faz subscribe) não atualizam.
- Mutar slots antes de validar capacidade → item duplicado ou perdido em falha parcial.
- Serializar `ItemDataSO`/`Sprite` no snapshot → viola `save-dto-simple-types-only`.
- Consumidor (quest/economy) chamando o `InventoryManager` direto em vez de via adapter/interface.

## Onde se aplica

- `04_ui_inventory_items_tooltips`
- `04_ui_storage_chest_transfer`
- `06_item_definition_tags_quality_rarity`

## Relacionados

- skill `save-load-pattern`, rule `save-dto-simple-types-only` — snapshot id+qty, sem Unity refs
- rule `error-handling-resilience` — transação inválida = `bool`+`FailureReason`, não exception
- skill `editmode-test-authoring`, rule `testing-quality-gate` — stack/split/move, atomicidade, round-trip
- skill `ui-modal-stack` — a tela/modal de inventory
- skill `system-reuse-audit` — antes de criar qualquer classe nova
