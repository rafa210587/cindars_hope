# refinamento_init_inventory_slots_capacity_ui

> **Status:** Refinamento inicial a implementar  
> **Origem:** validação das specs implementadas/parciais  
> **Spec futura sugerida:** `spec_inventory_slots_capacity_ui_final.md`  
> **Objetivo:** evoluir o inventory MVP por ID/stack agregada para um inventário final com slots, múltiplas stacks, capacidade e UI.

---

## 1. Estado atual

O sistema atual usa `InventoryManager` com `Dictionary<string, int>`, ou seja, uma quantidade agregada por `itemId`.

Isso é suficiente para o MVP, mas não representa inventário final.

Evidência principal:

```text
Assets/_Game/Scripts/Inventory/InventoryManager.cs
Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs
Assets/_Game/Scripts/Inventory/Data/ItemDatabaseSO.cs
docs/specs/implementados/spec_inventory_001_inventario_itens_gold_e_stacks.md
```

---

## 2. Problemas/gaps

- Não há slots reais.
- Não há múltiplas stacks do mesmo item.
- `MaxStack` limita a quantidade total por item, não por stack.
- Não há capacidade máxima de inventory.
- Não há drag/drop, split stack, merge stack ou swap.
- UI final de inventário não existe.
- Save atual salva `ItemId + Amount`, não slots.

---

## 3. Escopo esperado

### Runtime

Criar modelo de inventory baseado em slots:

```text
InventorySlotSaveData
SlotIndex
ItemId
Amount
```

Regras:

- cada slot guarda no máximo 1 item stackável;
- `MaxStack` passa a limitar cada slot;
- itens iguais podem ocupar múltiplos slots;
- AddItem distribui entre stacks existentes e slots vazios;
- RemoveItem consome das stacks corretamente;
- operações retornam sobra quando não houver espaço.

### UI

Criar UI final mínima:

- grid de slots;
- tooltip simples;
- seleção de item;
- highlight de slot selecionado;
- indicação de quantidade;
- suporte futuro a drag/drop.

### Save/load

Migrar save antigo:

```text
InventorySaveData.Items -> InventorySlotSaveData[]
```

Migration segura:

- se save antigo tiver Items, converter para slots sequenciais;
- preservar compatibilidade com saves antigos enquanto schema migration não estiver completa.

---

## 4. Arquivos prováveis

```text
Assets/_Game/Scripts/Inventory/InventoryManager.cs
Assets/_Game/Scripts/Inventory/InventorySlot.cs
Assets/_Game/Scripts/Inventory/InventoryAddResult.cs
Assets/_Game/Scripts/Inventory/UI/InventoryPanelController.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Save/SaveManager.cs
docs/specs/implementados/spec_inventory_001_inventario_itens_gold_e_stacks.md
```

---

## 5. Fora de escopo

- Equipamento visual completo.
- Crafting UI final.
- Item rarity/affixes.
- Multiplayer/trading.

---

## 6. Definition of Done

- [ ] Inventory suporta N slots configuráveis.
- [ ] Mesmo item pode ocupar múltiplas stacks.
- [ ] `MaxStack` é por slot.
- [ ] Save/load preserva slots.
- [ ] Save antigo por `ItemId + Amount` migra sem perda.
- [ ] UI mostra slots e quantidades.
- [ ] Add/remove não perde item silenciosamente.
- [ ] `spec_inventory_001` é atualizada com estado real.

---

## 7. Validação

1. Adicionar item stackável acima do `MaxStack` e verificar múltiplos slots.
2. Remover quantidade parcial e validar decremento correto.
3. Salvar/carregar inventory com múltiplos slots.
4. Testar inventory cheio e validar sobra/rejeição sem perda.
5. Validar UI no Play Mode.
