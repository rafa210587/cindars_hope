# REF FUTURO — FASE9E item taxonomy IDs

> Origem histórica: $Source
> Status: Refinamento futuro preservado
> Spec futura relacionada: $Spec

---

## Decisões preservadas

Conteúdo histórico preservado abaixo para evitar perda operacional de decisões, escopo e pendências.

---

# FASE 9E — Item Taxonomy, IDs e Regras de Item Spec v1.0

> **Status:** spec aprovada para orientar próximas waves.  
> **Feature:** FASE9E_ITEM_TAXONOMY_IDS  
> **Base:** FASE9C Tools/Farm/Combat + Player Equipment + UI/Hotbar + Damage/Status.  
> **Objetivo:** definir uma taxonomia única de itens, prefixos de IDs, categorias, flags, stacks e regras de item para evitar drift entre Inventory, Equipment, Shop, Crafting, Save/Load, Hotbar e ItemPickup.

---

## 1. Problema

O projeto já usa itens para sementes, crops, materiais, ferramentas, peixes, crafting, venda, pickups persistentes, armas, magia, flechas/ammo, consumíveis, drops de monstros e recursos de caverna.

Com a expansão para ferramentas, armas, flechas elementais, magia, poções, foods, minérios e quest items, precisamos de uma taxonomia única. Sem isso, cada sistema tende a criar IDs, categorias e flags próprias.

---

## 2. Decisões fechadas

| Pergunta | Decisão |
|---|---|
| ItemRarity no MVP? | Não; registrar TODO em ideias futuras |
| Food e Consumable separados? | Não; categoria única `Consumable` com subtipo |
| Magic físico/equipável ou spell aprendido? | MVP trata como item: pergaminho, cetro ou varinha |
| Ferramentas básicas vendem/dropam/removem? | Sim |
| Armas básicas vendem/dropam/removem? | Sim |
| Quest item aparece na hotbar se usável? | Sim |
| MaxStack Fish | 99 |
| MaxStack Food/Consumable | 99 |
| Drop de stack | Permite escolher quantidade |
| Seeds equipáveis? | Sim, para escolher qual plantar |
| Crop é comido direto? | Não no MVP; crop é usado para receita |
| Food equipável? | Sim, pode ser usado em combate |
| RightHand pode ter weapon? | Sim |
| Ferramentas podem atacar? | Sim, mas com dano inferior a armas |
| Tools e weapons usam materiais? | Sim: Wood/Bronze/Iron/Gold/Diamond |
| Raridade visual futura | Pode aparecer pela cor do ícone/borda |

---

## 3. Categorias oficiais

```csharp
public enum ItemCategory
{
    None,
    Seed,
    Crop,
    Consumable,
    Material,
    Tool,
    Weapon,
    Magic,
    Ammo,
    Fish,
    Ore,
    Gem,
    MonsterDrop,
    Quest,
    KeyItem,
    Furniture,
    Misc
}
```

Food e consumíveis ficam em uma categoria única, `Consumable`, com subtipo:

```csharp
public enum ConsumableSubtype
{
    None,
    Food,
    Potion,
    Drink,
    BuffFood,
    Utility
}
```

---

## 4. Prefixos oficiais

| Categoria | Prefixo | Exemplo |
|---|---|---|
| Seed | `item_seed_` | `item_seed_wheat` |
| Crop | `item_crop_` | `item_crop_wheat` |
| Consumable | `item_consumable_` | `item_consumable_food_bread` |
| Material | `item_material_` | `item_material_wood` |
| Tool | `item_tool_` | `item_tool_axe_wood` |
| Weapon | `item_weapon_` | `item_weapon_sword_bronze` |
| Magic | `item_magic_` | `item_magic_scroll_fire_spark` |
| Ammo | `item_ammo_` | `item_ammo_arrow_basic` |
| Fish | `item_fish_` | `item_fish_carp` |
| Ore | `item_ore_` | `item_ore_copper` |
| Gem | `item_gem_` | `item_gem_ruby` |
| MonsterDrop | `item_drop_` | `item_drop_slime_gel` |
| Quest | `item_quest_` | `item_quest_lost_ring` |
| KeyItem | `item_key_` | `item_key_cave_gate` |
| Furniture | `item_furniture_` | `item_furniture_chair_wood` |
| Misc | `item_misc_` | `item_misc_debug_token` |

Dados relacionados usam prefixos como `tool_`, `weapon_`, `spell_`, `consumable_`, `projectile_`, `recipe_`, `enemy_`, `enemy_action_` e `status_`.

---

## 5. ItemDataSO vNext

```csharp
public class ItemDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public string Description;
    public Sprite Icon;
    public ItemCategory Category;
    public ConsumableSubtype ConsumableSubtype;

    public bool IsStackable = true;
    public int MaxStack = 99;

    public bool IsSellable = true;
    public int SellPrice = 0;
    public bool IsBuyable = false;
    public int BuyPrice = 0;

    public bool IsEquippable = false;
    public bool IsUsable = false;
    public bool IsDroppable = true;
    public bool IsRemovable = true;

    public bool IsQuestItem = false;
    public bool IsDebugOnly = false;
    public bool CanBeAssignedToHotbar = true;
    public bool CanBeUsedInCombat = false;
}
```

Regras:

- `Id` nunca muda depois de publicado.
- Save usa apenas `Id`.
- `MaxStack <= 1` implica `IsStackable = false`.
- Quest/Key items geralmente não são sellable, droppable nem removable, salvo override explícito.
- DebugOnly não deve aparecer em loot/shop normal.

---

## 6. Stack e drop

| Categoria | MaxStack sugerido |
|---|---:|
| Seed | 99 |
| Crop | 99 |
| Consumable | 99 |
| Material | 99 |
| Ore | 99 |
| Gem | 99 |
| Fish | 99 |
| MonsterDrop | 99 |
| Ammo | 99 |
| Tool | 1 |
| Weapon | 1 |
| Magic | 1 |
| Quest | 1 ou específico |
| KeyItem | 1 |

Split divide em duas pilhas iguais ou quase iguais. Drop deve permitir escolher quantidade. Se não houver seletor no MVP inicial, o default temporário pode ser pilha inteira, mas a spec alvo é escolher quantidade.

---

## 7. Equip/Hotbar rules

| Slot | Categorias permitidas |
|---|---|
| Hotbar | Tool, Seed, Consumable, Magic, Ammo, Quest usável, Material opcional |
| LeftHand | Tool, Seed, Weapon, Magic, Consumable, Ammo contextual, Quest usável |
| RightHand | Tool, Seed, Weapon, Ammo, Consumable, Magic contextual, Quest usável |
| PrimaryWeapon | Weapon |
| Magic | Magic ou WeaponDataSO RangedMagic no MVP |
| Ammo | Ammo |
| Tool | Tool |
| Consumable | Consumable com subtipo Food/Potion/Drink/BuffFood/Utility |

Regras especiais:

- Seed fica na hotbar.
- RightHand pode receber Weapon quando fizer sentido.
- Seeds são equipáveis e usadas para escolher plantio.
- Hoe irriga/ativa a seed e acelera crescimento conforme tier/material.
- Arco fica no Q/LeftHand.
- Flecha fica no E/RightHand.
- Magia é removida automaticamente ao equipar arco.

---

## 8. Critérios de aceite

- ItemCategory inclui categorias necessárias para farm, tools, weapons, magic, ammo, consumables, ores, drops, quest e key items.
- ConsumableSubtype existe para Food/Potion/Drink/BuffFood/Utility.
- Todo item novo segue prefixo compatível com categoria.
- ItemDataSO suporta stack, economy, equip, use, drop, removable, quest, debug, hotbar e combat flags.
- Inventory, equipment, hotbar, pickup e save/load usam IDs estáveis.
- Stack rules são compatíveis com split e drop com quantidade escolhida.
- Quest/Key items não podem ser vendidos, dropados ou removidos por padrão sem override explícito.
- Validator detecta inconsistências principais.

---

## 9. Referências complementares

Exemplos iniciais e variações estão em:

- `docs/FASE9E_ITEM_EXAMPLES_VARIATIONS_v1.0.md`

TODOs futuros estão em:

- `docs/FUTURE_IDEAS_TODO_v1.0.md`


## Itens que devem virar implementação

- [ ] Refinar em spec executável antes de código, quando aplicável.
- [ ] Validar dependências contra specs implementadas atuais.

## Fora de escopo / cuidado

- Não tratar este refinement como autorização automática de implementação.
- Não sobrescrever specs implementadas sem amendment/correction explícito.


