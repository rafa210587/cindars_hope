# Plan — FASE9E Item Taxonomy, IDs e Regras de Item

> **Feature:** FASE9E_ITEM_TAXONOMY_IDS  
> **Spec:** `spec.md`

---

## 1. Estratégia

Implementar em etapas pequenas:

1. contratos de categoria/subtipo;
2. expansão de ItemDataSO;
3. validação de prefixos;
4. validação de stack/split/drop;
5. regras de shop/crafting;
6. regras de equipment/hotbar;
7. exemplos iniciais de itens;
8. handoff.

---

## 2. Arquitetura proposta

### Tipos

- `ItemCategory`
- `ConsumableSubtype`
- `ItemStackRules`
- `ItemSlotRules`

### ItemDataSO vNext

Adicionar ou confirmar campos:

- `ConsumableSubtype`;
- `IsStackable`;
- `MaxStack`;
- `IsSellable`;
- `SellPrice`;
- `IsBuyable`;
- `BuyPrice`;
- `IsEquippable`;
- `IsUsable`;
- `IsDroppable`;
- `IsRemovable`;
- `IsQuestItem`;
- `IsDebugOnly`;
- `CanBeAssignedToHotbar`;
- `CanBeUsedInCombat`.

---

## 3. Integrações

### InventoryManager

Precisa respeitar stack, split, drop com quantidade e flags de remoção.

### EquipmentManager/Hotbar

Precisa validar categorias permitidas por slot.

### Shop

Precisa respeitar sellable/buyable, preço e proteção de Quest/Key items.

### Crafting

Precisa validar ingredientes/resultados por ID.

### ItemPickup

Drop deve criar pickup persistente com quantidade escolhida.

### Validator

Deve detectar prefixo/categoria/flags inconsistentes.

---

## 4. Riscos

| Risco | Mitigação |
|---|---|
| Renomear IDs quebrar saves | só migrar IDs antes de save público real; documentar alterações |
| Split exigir inventário por stacks reais | implementar fallback explícito ou evoluir InventoryManager |
| Muitos campos em ItemDataSO | adicionar por PR pequeno e validar defaults |
| Quest item removível por acidente | validator e defaults seguros |
| ItemRarity entrar cedo demais | manter em FUTURE_IDEAS_TODO |

---

## 5. Testes manuais mínimos

- Criar item de cada categoria.
- Validar prefixos.
- Stackar seed/ammo/consumable/fish até 99.
- Splitar stack par/ímpar.
- Dropar quantidade escolhida.
- Tentar vender Quest/Key item.
- Equipar seed.
- Equipar food/consumable.
- Equipar weapon no RightHand quando permitido.
- Confirmar validator sem erro vermelho.

---

## 6. Fora de escopo técnico

- ItemRarity implementado;
- balance final;
- ícones finais;
- receitas completas;
- durabilidade/peso;
- UI final de escolha de quantidade.

