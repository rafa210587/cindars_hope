# SpecKit â€” FASE9E Item Taxonomy, IDs e Regras de Item

> **Feature:** FASE9E_ITEM_TAXONOMY_IDS  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9E_ITEM_TAXONOMY_IDS_SPEC_v1.0.md`  
> **Exemplos:** `docs_old/FASE9E_ITEM_EXAMPLES_VARIATIONS_v1.0.md`

---

## 1. User story

Como dev/designer, quero uma convenÃ§Ã£o Ãºnica para categorias, IDs, stack, venda, uso, equip e persistÃªncia de itens para criar conteÃºdo novo sem quebrar Inventory, Save/Load, Shop, Crafting, Hotbar e Equipment.

---

## 2. Objetivos funcionais

### O1 â€” Categorias oficiais

Definir categorias oficiais para todos os tipos de item do MVP e prÃ³ximas fases.

### O2 â€” Prefixos oficiais

Definir prefixos por categoria.

### O3 â€” Flags de item

Padronizar stack, economy, equip, use, drop, removable, quest, debug, hotbar e combat flags.

### O4 â€” Stack/split/drop

Definir stack, split e drop com escolha de quantidade.

### O5 â€” Equipment/hotbar compatibility

Definir categorias permitidas por slot.

### O6 â€” Exemplos iniciais

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
- Ã­cones finais;
- durabilidade/peso avanÃ§ado.

---

## 4. Regras de negÃ³cio

### R1 â€” IDs estÃ¡veis

Save usa apenas IDs estÃ¡veis. ID publicado nÃ£o deve mudar.

### R2 â€” Consumable Ãºnico

Food, Potion, Drink, BuffFood e Utility ficam em `ItemCategory.Consumable` com `ConsumableSubtype`.

### R3 â€” Seeds equipÃ¡veis

Seeds sÃ£o equipÃ¡veis para escolher qual plantar.

### R4 â€” Crops para receita

Crops nÃ£o sÃ£o consumidos diretamente no MVP; sÃ£o usados para receitas.

### R5 â€” Food em combate

Consumables do subtipo Food podem ser equipados e usados em combate.

### R6 â€” Drop com quantidade

Drop de stack deve permitir escolher quantidade.

### R7 â€” Tools/Weapons bÃ¡sicas

Ferramentas e armas bÃ¡sicas podem ser vendidas, dropadas e removidas se flags permitirem.

### R8 â€” Quest/Key protection

Quest/Key items nÃ£o podem ser vendidos, dropados ou removidos por padrÃ£o sem override explÃ­cito.

### R9 â€” Materials/tier

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

## 6. CritÃ©rios de aceite

### CA1 â€” Categorias

ItemCategory contÃ©m Seed, Crop, Consumable, Material, Tool, Weapon, Magic, Ammo, Fish, Ore, Gem, MonsterDrop, Quest, KeyItem, Furniture e Misc.

### CA2 â€” ConsumableSubtype

ConsumableSubtype contÃ©m Food, Potion, Drink, BuffFood e Utility.

### CA3 â€” Prefixos

Todo item novo segue prefixo compatÃ­vel com categoria.

### CA4 â€” Flags

ItemDataSO suporta flags necessÃ¡rias.

### CA5 â€” Stack

MaxStack e IsStackable sÃ£o validados.

### CA6 â€” Drop quantidade

Drop de stack permite escolher quantidade ou registra fallback temporÃ¡rio explÃ­cito.

### CA7 â€” Hotbar/slots

Slot rules permitem Seed, Consumable, Tool, Magic, Ammo, Weapon e Quest usÃ¡vel conforme definido.

### CA8 â€” Exemplos

Seeds, foods, tools, weapons, ammo e magic items iniciais estÃ£o documentados.

### CA9 â€” Future TODO

ItemRarity fica registrado em `docs_old/FUTURE_IDEAS_TODO_v1.0.md`.

---

## 7. DependÃªncias

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
- `docs_old/FASE9E_ITEM_TAXONOMY_IDS_SPEC_v1.0.md` estiver lida.
- `docs_old/FASE9E_ITEM_EXAMPLES_VARIATIONS_v1.0.md` estiver lido.

