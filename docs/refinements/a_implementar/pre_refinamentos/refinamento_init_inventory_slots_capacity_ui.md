# refinamento_init_inventory_slots_capacity_ui

> **Status:** Refinamento inicial a implementar
> **Origem:** validaÃ§Ã£o das specs implementadas/parciais
> **Spec futura sugerida:** `spec_inventory_slots_capacity_ui_final.md`
> **Objetivo:** evoluir o inventory MVP por ID/stack agregada para um inventÃ¡rio final com slots, mÃºltiplas stacks, capacidade e UI.

---

## 1. Estado atual

O sistema atual usa `InventoryManager` com `Dictionary<string, int>`, ou seja, uma quantidade agregada por `itemId`.

Isso Ã© suficiente para o MVP, mas nÃ£o representa inventÃ¡rio final.

EvidÃªncia principal:

```text
Assets/_Game/Scripts/Inventory/InventoryManager.cs
Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs
Assets/_Game/Scripts/Inventory/Data/ItemDatabaseSO.cs
docs/specs/implementados/spec_inventory_001_inventario_itens_gold_e_stacks.md
```

---

## 2. Problemas/gaps

- NÃ£o hÃ¡ slots reais.
- NÃ£o hÃ¡ mÃºltiplas stacks do mesmo item.
- `MaxStack` limita a quantidade total por item, nÃ£o por stack.
- NÃ£o hÃ¡ capacidade mÃ¡xima de inventory.
- NÃ£o hÃ¡ drag/drop, split stack, merge stack ou swap.
- UI final de inventÃ¡rio nÃ£o existe.
- Save atual salva `ItemId + Amount`, nÃ£o slots.

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

- cada slot guarda no mÃ¡ximo 1 item stackÃ¡vel;
- `MaxStack` passa a limitar cada slot;
- itens iguais podem ocupar mÃºltiplos slots;
- AddItem distribui entre stacks existentes e slots vazios;
- RemoveItem consome das stacks corretamente;
- operaÃ§Ãµes retornam sobra quando nÃ£o houver espaÃ§o.

### UI

Criar UI final mÃ­nima:

- grid de slots;
- tooltip simples;
- seleÃ§Ã£o de item;
- highlight de slot selecionado;
- indicaÃ§Ã£o de quantidade;
- suporte futuro a drag/drop.

### Save/load

Migrar save antigo:

```text
InventorySaveData.Items -> InventorySlotSaveData[]
```

Migration segura:

- se save antigo tiver Items, converter para slots sequenciais;
- preservar compatibilidade com saves antigos enquanto schema migration nÃ£o estiver completa.

---

## 4. Arquivos provÃ¡veis

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

- [ ] Inventory suporta N slots configurÃ¡veis.
- [ ] Mesmo item pode ocupar mÃºltiplas stacks.
- [ ] `MaxStack` Ã© por slot.
- [ ] Save/load preserva slots.
- [ ] Save antigo por `ItemId + Amount` migra sem perda.
- [ ] UI mostra slots e quantidades.
- [ ] Add/remove nÃ£o perde item silenciosamente.
- [ ] `spec_inventory_001` Ã© atualizada com estado real.

---

## 7. ValidaÃ§Ã£o

1. Adicionar item stackÃ¡vel acima do `MaxStack` e verificar mÃºltiplos slots.
2. Remover quantidade parcial e validar decremento correto.
3. Salvar/carregar inventory com mÃºltiplos slots.
4. Testar inventory cheio e validar sobra/rejeiÃ§Ã£o sem perda.
5. Validar UI no Play Mode.
