# FASE 9E — Item Examples and Variations v1.0

> Complementa `docs/FASE9E_ITEM_TAXONOMY_IDS_SPEC_v1.0.md`.

---

## 1. Seeds/crops/foods — 6 tipos

| Seed ID | Nome | Raridade futura | BuyPrice | SellPrice | GrowthDays | Crop gerado | Receita/Food alvo | Papel no jogo |
|---|---|---:|---:|---:|---:|---|---|---|
| `item_seed_wheat` | Wheat Seed | Common | 5 | 2 | 2 | `item_crop_wheat` | `item_consumable_food_bread` | entrada barata, farming inicial |
| `item_seed_carrot` | Carrot Seed | Common | 8 | 3 | 3 | `item_crop_carrot` | `item_consumable_food_carrot_stew` | comida inicial melhor que pão |
| `item_seed_moonbean` | Moonbean Seed | Uncommon | 18 | 7 | 4 | `item_crop_moonbean` | `item_consumable_food_moonbean_soup` | crop mágico leve, stamina |
| `item_seed_sunpepper` | Sunpepper Seed | Uncommon | 24 | 10 | 4 | `item_crop_sunpepper` | `item_consumable_food_spicy_sunpepper` | comida com buff temporário |
| `item_seed_crystal_berry` | Crystal Berry Seed | Rare | 45 | 18 | 5 | `item_crop_crystal_berry` | `item_consumable_food_crystal_jam` | crop caro, alimento forte |
| `item_seed_starroot` | Starroot Seed | Rare/Epic futuro | 75 | 30 | 6 | `item_crop_starroot` | `item_consumable_food_starroot_pie` | alimento raro, alto valor |

Regras:

- quanto mais rara a seed, maior o preço;
- quanto mais rara a seed, melhor o alimento/receita produzida;
- seed é equipável para escolher plantio;
- crop é usado principalmente para receitas;
- food resultante pode recuperar HP/stamina ou dar efeito temporário.

---

## 2. Crops detalhados

| Crop ID | Nome | Origem | Uso principal | Consumo direto? | Stack |
|---|---|---|---|---:|---:|
| `item_crop_wheat` | Wheat | Wheat Seed | receita de Bread | Não no MVP | 99 |
| `item_crop_carrot` | Carrot | Carrot Seed | receita de Carrot Stew | Não no MVP | 99 |
| `item_crop_moonbean` | Moonbean | Moonbean Seed | receita de Moonbean Soup | Não no MVP | 99 |
| `item_crop_sunpepper` | Sunpepper | Sunpepper Seed | receita de Spicy Sunpepper | Não no MVP | 99 |
| `item_crop_crystal_berry` | Crystal Berry | Crystal Berry Seed | receita de Crystal Jam | Não no MVP | 99 |
| `item_crop_starroot` | Starroot | Starroot Seed | receita de Starroot Pie | Não no MVP | 99 |

---

## 3. Foods/Consumables detalhados

| Item ID | Nome | Subtipo | Ingredientes principais | Efeito MVP | Uso em combate? | Stack |
|---|---|---|---|---|---:|---:|
| `item_consumable_potion_hp_small` | Small HP Potion | Potion | craft/shop | +10 HP | Sim | 99 |
| `item_consumable_food_bread` | Bread | Food | Wheat | +8 stamina | Sim | 99 |
| `item_consumable_food_carrot_stew` | Carrot Stew | Food | Carrot | +6 HP, +10 stamina | Sim | 99 |
| `item_consumable_food_moonbean_soup` | Moonbean Soup | BuffFood | Moonbean | +15 stamina, pequeno buff futuro | Sim | 99 |
| `item_consumable_food_spicy_sunpepper` | Spicy Sunpepper | BuffFood | Sunpepper | +20 stamina, buff de velocidade futuro | Sim | 99 |
| `item_consumable_food_crystal_jam` | Crystal Jam | Food | Crystal Berry | +18 HP, +18 stamina | Sim | 99 |
| `item_consumable_food_starroot_pie` | Starroot Pie | BuffFood | Starroot | +30 HP, +30 stamina, buff futuro | Sim | 99 |

---

## 4. Tool material tiers

Materiais/tier de ferramenta:

```text
Wood -> Bronze -> Iron -> Gold -> Diamond
```

| Material | Tier | Ideia de bônus para tools | Observação |
|---|---:|---|---|
| Wood | 1 | libera ação básica | fraco, barato |
| Bronze | 2 | reduz custo/tempo levemente | primeiro upgrade real |
| Iron | 3 | aumenta ActionPower ou yield | médio |
| Gold | 4 | ação mais rápida e melhor yield | caro, eficiente |
| Diamond | 5 | melhor eficiência/área/bônus especial | top tier MVP/futuro |

### 4.1 Tool families

| Tool family | IDs |
|---|---|
| Hoe | `item_tool_hoe_wood`, `item_tool_hoe_bronze`, `item_tool_hoe_iron`, `item_tool_hoe_gold`, `item_tool_hoe_diamond` |
| Axe | `item_tool_axe_wood`, `item_tool_axe_bronze`, `item_tool_axe_iron`, `item_tool_axe_gold`, `item_tool_axe_diamond` |
| Sickle | `item_tool_sickle_wood`, `item_tool_sickle_bronze`, `item_tool_sickle_iron`, `item_tool_sickle_gold`, `item_tool_sickle_diamond` |
| Pickaxe | `item_tool_pickaxe_wood`, `item_tool_pickaxe_bronze`, `item_tool_pickaxe_iron`, `item_tool_pickaxe_gold`, `item_tool_pickaxe_diamond` |
| FishingRod | `item_tool_fishing_rod_wood`, `item_tool_fishing_rod_bronze`, `item_tool_fishing_rod_iron`, `item_tool_fishing_rod_gold`, `item_tool_fishing_rod_diamond` |

### 4.2 Hoe bônus sugerido

| ID | Bônus sugerido |
|---|---|
| `item_tool_hoe_wood` | planta/irriga seed básica normalmente |
| `item_tool_hoe_bronze` | pequena chance de reduzir 1 dia de crescimento |
| `item_tool_hoe_iron` | reduz 1 dia de crescimento em seeds comuns/incomuns |
| `item_tool_hoe_gold` | reduz 1 dia e melhora chance de yield extra |
| `item_tool_hoe_diamond` | reduz crescimento e pode afetar área/linha futuramente |

Ferramentas podem causar dano, mas inferior ao de armas equivalentes.

---

## 5. Weapons por material e função

Materiais/tier de arma:

```text
Wood -> Bronze -> Iron -> Gold -> Diamond
```

| Material | Tier | Efeito geral em armas |
|---|---:|---|
| Wood | 1 | básico, baixo dano |
| Bronze | 2 | dano levemente maior |
| Iron | 3 | bom dano base |
| Gold | 4 | rápido/eficiente, caro |
| Diamond | 5 | alto dano/eficiência |

### 5.1 Weapon families

| Weapon family | IDs |
|---|---|
| Sword | `item_weapon_sword_wood`, `item_weapon_sword_bronze`, `item_weapon_sword_iron`, `item_weapon_sword_gold`, `item_weapon_sword_diamond` |
| Dagger | `item_weapon_dagger_wood`, `item_weapon_dagger_bronze`, `item_weapon_dagger_iron`, `item_weapon_dagger_gold`, `item_weapon_dagger_diamond` |
| Hammer | `item_weapon_hammer_wood`, `item_weapon_hammer_bronze`, `item_weapon_hammer_iron`, `item_weapon_hammer_gold`, `item_weapon_hammer_diamond` |
| Bow | `item_weapon_bow_wood`, `item_weapon_bow_bronze`, `item_weapon_bow_iron`, `item_weapon_bow_gold`, `item_weapon_bow_diamond` |

Regras:

- armas usam os mesmos materiais das ferramentas;
- armas corpo a corpo podem ter dano, velocidade de ataque, durabilidade e peso em spec futura;
- arcos aumentam dano, velocidade da flecha e velocidade de ataque;
- elemento de ataque de arco vem da flecha, não do arco.

---

## 6. Ammo/flechas

| ID | Nome | Elemento | Status | Papel |
|---|---|---|---|---|
| `item_ammo_arrow_basic` | Basic Arrow | Physical | nenhum | munição comum |
| `item_ammo_arrow_fire` | Fire Arrow | Fire | Burn | flecha elemental de fogo |
| `item_ammo_arrow_lightning` | Lightning Arrow | Lightning | Paralyze | controle curto |
| `item_ammo_arrow_poison` | Poison Arrow | Poison | Poison | DoT venenoso |
| `item_ammo_arrow_ice` | Ice Arrow | Ice | Slow/Root futuro | controle de movimento |
| `item_ammo_arrow_light` | Light Arrow | Light | Blind | reduz precisão/percepção |
| `item_ammo_arrow_shadow` | Shadow Arrow | Shadow | Poison/Weakness futuro | dano sombrio/debuff |

---

## 7. Magic item variations

Magic é item equipável no MVP. Cada pergaminho, cetro ou varinha representa uma magia.

| ID | Forma | Elemento | Status | Papel |
|---|---|---|---|---|
| `item_magic_scroll_fire_spark` | Pergaminho | Fire | Burn | magia inicial descartável ou fraca |
| `item_magic_wand_fire_spark` | Varinha | Fire | Burn | cast repetível no MVP |
| `item_magic_wand_lightning_jolt` | Varinha | Lightning | Paralyze | controle curto |
| `item_magic_scroll_wind_gust` | Pergaminho | Wind | Knockdown | empurrão/derrubar |
| `item_magic_wand_earth_bind` | Varinha | Earth | Slow | controle de movimento |
| `item_magic_scroll_shadow_mist` | Pergaminho | Shadow | Poison | veneno/sombra |
| `item_magic_wand_light_flash` | Varinha | Light | Blind | cegar/inibir ataque |

---

## 8. Materials/ores/drops

```text
item_material_wood
item_material_stone
item_material_fiber
item_ore_copper
item_ore_iron
item_ore_gold
item_gem_diamond
item_drop_slime_gel
```
