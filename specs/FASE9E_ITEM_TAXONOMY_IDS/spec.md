# SpecKit — FASE9E Item Taxonomy, IDs e Regras de Item

> **Feature:** FASE9E_ITEM_TAXONOMY_IDS  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs/FASE9E_ITEM_TAXONOMY_IDS_SPEC_v1.0.md`  
> **Exemplos:** `docs/FASE9E_ITEM_EXAMPLES_VARIATIONS_v1.0.md`

---

## 1. User story

Como dev/designer, quero uma convenção única para categorias, IDs, stack, venda, uso, equip e persistência de itens para criar conteúdo novo sem quebrar Inventory, Save/Load, Shop, Crafting, Hotbar e Equipment.

---

## 2. Objetivos funcionais

### O1 — Categorias oficiais

Definir categorias oficiais para todos os tipos de item do MVP e próximas fases.

### O2 — Prefixos oficiais

Definir prefixos por categoria.

### O3 — Flags de item

Padronizar stack, economy, equip, use, drop, removable, quest, debug, hotbar e combat flags.

### O4 — Stack/split/drop

Definir stack, split e drop com escolha de quantidade.

### O5 — Equipment/hotbar compatibility

Definir categorias permitidas por slot.

### O6 — Exemplos iniciais

Guardar exemplos de seeds, crops, foods, tools, weapons, ammo, magic items, materials, ores e drops.

---

## 3. Non-goals

Fora de escopo:

- UI final;
- balanceamento final;
- receitas completas;
- todos os itens finais do jogo;
- ItemRarity implementado;
- economia final;
- ícones finais;
- durabilidade/peso avançado.

---

## 4. Regras de negócio

### R1 — IDs estáveis

Save usa apenas IDs estáveis. ID publicado não deve mudar.

### R2 — Consumable único

Food, Potion, Drink, BuffFood e Utility ficam em `ItemCategory.Consumable` com `ConsumableSubtype`.

### R3 — Seeds equipáveis

Seeds são equipáveis para escolher qual plantar.

### R4 — Crops para receita

Crops não são consumidos diretamente no MVP; são usados para receitas.

### R5 — Food em combate

Consumables do subtipo Food podem ser equipados e usados em combate.

### R6 — Drop com quantidade

Drop de stack deve permitir escolher quantidade.

### R7 — Tools/Weapons básicas

Ferramentas e armas básicas podem ser vendidas, dropadas e removidas se flags permitirem.

### R8 — Quest/Key protection

Quest/Key items não podem ser vendidos, dropados ou removidos por padrão sem override explícito.

### R9 — Materials/tier

Tools e weapons usam materiais: Wood, Bronze, Iron, Gold, Diamond.

---

## 5. Entidades funcionais

- `ItemCategory`
- `ConsumableSubtype`
- `ItemDataSO vNext`
- `ItemPrefixValidator`
- `ItemStackRules`
- `ItemSlotRules`

---

## 6. Critérios de aceite

### CA1 — Categorias

ItemCategory contém Seed, Crop, Consumable, Material, Tool, Weapon, Magic, Ammo, Fish, Ore, Gem, MonsterDrop, Quest, KeyItem, Furniture e Misc.

### CA2 — ConsumableSubtype

ConsumableSubtype contém Food, Potion, Drink, BuffFood e Utility.

### CA3 — Prefixos

Todo item novo segue prefixo compatível com categoria.

### CA4 — Flags

ItemDataSO suporta flags necessárias.

### CA5 — Stack

MaxStack e IsStackable são validados.

### CA6 — Drop quantidade

Drop de stack permite escolher quantidade ou registra fallback temporário explícito.

### CA7 — Hotbar/slots

Slot rules permitem Seed, Consumable, Tool, Magic, Ammo, Weapon e Quest usável conforme definido.

### CA8 — Exemplos

Seeds, foods, tools, weapons, ammo e magic items iniciais estão documentados.

### CA9 — Future TODO

ItemRarity fica registrado em `docs/FUTURE_IDEAS_TODO_v1.0.md`.

---

## 7. Dependências

- `InventoryManager`
- `ItemDataSO`
- `ItemDatabaseSO`
- `EquipmentManager`
- `HotbarSelectionData`
- `ItemPickup`
- `Shop/SellAllPoint`
- `CraftingManager`
- `SaveManager`
- `CindarsHopeDataValidator`

---

## 8. Pronto para Plan quando

- Esta spec estiver aprovada.
- `docs/FASE9E_ITEM_TAXONOMY_IDS_SPEC_v1.0.md` estiver lida.
- `docs/FASE9E_ITEM_EXAMPLES_VARIATIONS_v1.0.md` estiver lido.
