# Cindar's Hope — Item Catalog Direction v1.1

> **Status:** documento canônico de catálogo nominal de itens
> **Local:** `docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md`
> **Complementa:** `LOOT_CRAFTING_ECONOMY_DIRECTION.md` e `ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
> **Depende de:**
> - `docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md` (decisões humanas)
> - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md` (stats de gear)
> - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md` (estações/luas)
> **Função:** ser a lista nominal e canônica de todos os itens do jogo (~118 base + variantes
> de qualidade), com BaseValue, fonte, uso e ligações entre sistemas.
> **Não é spec implementável.** Geradores de dados (ItemDataInitializer e afins) derivam daqui.

---

## 0. Regra anti-duplicação

```text
Este catálogo é a fonte da LISTA (quais itens existem, com que BaseValue e fonte).
Preço final, restock, anti-arbitragem: ECONOMY_PRICING vence.
Stats mecânicos de armas/armaduras/escudos/munição: EQUIPMENT_MECHANICAL_BASELINES vence.
Em conflito de LISTA/nome/BV, ESTE catálogo vence.
```

---

# PARTE A — Regras do catálogo

## 1. Identidade e governança

```text
ID: item_<categoria>_<slug>, categoria ∈ {seed, crop, consumable, ammo, material, essence,
  tool, weapon, armor, shield, acc, relic, animal, key}. ID sem categoria é ERRO de validator.
Todo item vendável tem BaseValue > 0. Todo item declara FONTE (loja/craft/drop/quest/evento)
  e USO. Item órfão (sem fonte E sem uso) é ERRO de validator (spec F30).
Tier ≠ Quality ≠ Rarity (regra canônica da economia).
```

## 2. Qualidade (decisão humana Q3.2)

A qualidade de crops e produtos animais usa **itens separados por sufixo** — simples,
legível e compatível com o save atual:

```text
item_crop_carrot           Normal   BV ×1.0
item_crop_carrot_silver    Prata    BV ×1.5
item_crop_carrot_gold      Ouro     BV ×2.0
Fatores de qualidade na colheita: regas completas + fertilizante + estação correta + skill
(CropQualityResolver, spec F15). Receitas aceitam qualquer qualidade; vender é onde a
qualidade brilha. Presentes de qualidade alta valem mais na amizade futura.
```

---

# PARTE B — Fazenda: sementes, crops e comidas

## 3. A mesa de Cindar's Hope

A vila come do que o jogador planta — essa é a promessa pastoral do jogo. As seis sementes
originais cobrem o feijão-com-trigo do vale; as seis novas trazem Vaalara para o canteiro:
uma chora sob Alihana, uma arde com Senya, uma cresce onde nada deveria crescer.

## 4. Sementes e crops (12 pares) [com variantes de qualidade]

| Semente (BV) → Crop (BV) | Estação | Dias | Identidade |
|---|---|---|---|
| item_seed_wheat (5) → item_crop_wheat (12) | Primavera/Verão | 4 | a base do pão e da rotina |
| item_seed_carrot (6) → item_crop_carrot (14) | Primavera | 3 | rápida, primeira lição |
| item_seed_moonbean (10) → item_crop_moonbean (24) | Primavera | 5 | feijão que incha à noite |
| item_seed_sunpepper (12) → item_crop_sunpepper (30) | Verão | 5 | pimenta que estala ao sol |
| item_seed_crystal_berry (18) → item_crop_crystal_berry (48) | Verão | 7 | doce premium do vale |
| item_seed_starroot (15) → item_crop_starroot (38) | Outono | 6 | raiz que lembra céu |
| item_seed_alihana_tear (25) → item_crop_alihana_tear (70) | **noturna** — cresce só de noite; pico de Alihana = +1 qualidade | 8 | a lágrima branca da Guardiã |
| item_seed_senya_pepper (22) → item_crop_senya_pepper (60) | Verão — pico de Senya: 10% muta (BV ×3) | 6 | caos comestível |
| item_seed_shadowroot (20) → item_crop_shadowroot (55) | Outono **ou subsolo** (plantável em níveis de caverna com solo) | 7 | a raiz que prefere o escuro |
| item_seed_thandra_wheat (14) → item_crop_thandra_wheat (32) | Qualquer | 5 | a bênção cotidiana de Thandra |
| item_seed_vale_pumpkin (16) → item_crop_vale_pumpkin (44) | Outono | 8 | orgulho de festival |
| item_seed_brigandini_grape (20) → item_crop_brigandini_grape (52) | Verão | 7 | a uva que virou vinho e diplomacia |

Fontes: Sylveth (loja, por estação); sementes novas das luas exigem a cadeia da Sylveth
(sq_sylveth_2/3) ou eventos.

## 5. Comidas e receitas (20)

Receitas (`recipe_<slug>`) aprendidas por: padrão inicial (5), lojas/NPCs (8), quests (4),
first-kill de boss (3). Cozinhar na cozinha da casa (CraftingStation).

| Item (BV) | Ingredientes | Efeito | Receita via |
|---|---|---|---|
| item_consumable_food_bread (20) | wheat ×2 | +30 fome | inicial |
| item_consumable_food_carrot_stew (35) | carrot ×2 + água | +40 fome, +10 stamina | inicial |
| item_consumable_food_grilled_fish (40) | fish_common | +35 fome | inicial |
| item_consumable_food_moonbean_soup (50) | moonbean ×2 | +45 fome, +20 MP | inicial |
| item_consumable_food_miners_ration (50) | bread + grub_meat | +50 fome, +10 stamina — o rango barato de run | inicial |
| item_consumable_food_spicy_sunpepper (55) | sunpepper ×2 | +35 fome, +10% dano 4h | Gruta |
| item_consumable_food_pumpkin_soup (60) | vale_pumpkin + água | +55 fome, +ColdRes 1 dia | Gruta |
| item_consumable_food_grape_juice (45) | grape ×3 | +20 fome, +15 MP | Orlan |
| item_consumable_food_egg_breakfast (45) | egg ×2 | +40 fome, +5% XP 4h | Orlan |
| item_consumable_food_goat_cheese (65) | goat_milk ×2 (queijaria) | +35 fome, +10 HP máx 1 dia | Eiran |
| item_consumable_food_starroot_pie (70) | starroot ×2 + wheat | +60 fome, -15% fadiga | Gruta |
| item_consumable_food_shadow_salad (75) | shadowroot + carrot | +40 fome, -20% fadiga noturna | Yael |
| item_consumable_food_crystal_jam (80) | crystal_berry ×2 | +50 fome, +15 stamina máx 1 dia | Sylveth |
| item_consumable_food_hearty_omelette (85) | egg ×2 + cheese | +60 fome, +10 stamina máx 1 dia | Eiran q2 |
| item_consumable_food_mirrorfin_sashimi (90) | mirrorfin | +40 fome, +10% crit 4h | Yael (cadeia) |
| item_consumable_food_thandra_loaf (95) | thandra_wheat ×3 | +70 fome, remove 1 status leve | quest Corvus |
| item_consumable_food_tear_tonic (110) | alihana_tear + água | +30 MP máx 1 dia | quest Sylveth |
| item_consumable_food_vale_wine (120) | grape ×5 (barril, 2 dias) | presente amado; social futuro | Orlan q3 |
| item_consumable_food_pepper_feast (130) | senya_pepper ×2 + carne | +15% dano mágico 1 dia | first-kill Patriarch |
| item_consumable_food_festival_cake (150) | wheat+egg+milk+berry | só em festival; +5% tudo 1 dia | festival |

---

# PARTE C — Alquimia e preparo de run

## 6. O balcão da Ozzra

A regra do preparo (Combat Core, Parte N) vive nestes frascos: nenhum trivializa boss,
todos mudam a conta de uma run longa.

## 7. Poções (8)

| Item (BV) | Efeito | Nota |
|---|---|---|
| item_consumable_potion_hp_small (40) | +35% HP | comum |
| item_consumable_potion_hp_medium (90) | +60% HP | banda 26+ |
| item_consumable_potion_mp_small (45) | +40% MP | comum |
| item_consumable_potion_mp_medium (95) | +70% MP | banda 26+ |
| item_consumable_potion_antidote (35) | remove Poison/Bleed | obrigatória p/ fungal |
| item_consumable_potion_fire_resist (60) | +50% FireRes 5min | banda fire |
| item_consumable_potion_ice_resist (60) | +50% Ice/ColdRes 5min | banda ice |
| item_consumable_potion_stamina_draught (70) | stamina cheia + regen ×1.5 por 60s | cooldown interno 5min |

## 8. Óleos de arma (4 — canônicos) e flechas (6)

```text
item_consumable_oil_fire / _frost / _shock / _poison (BV 50)
  Aplicam a tag canônica do óleo a UMA arma por 3 minutos. Craft na Ozzra; o counterplay
  segue o adapter (bônus só contra vulnerabilidade declarada).
item_ammo_arrow_wood (2) · _iron (4) · _steel (6) · _silver (12) · _fire (8) · _frost (8)
  Stats e tags da tabela canônica de arrows (EQUIPMENT_MECHANICAL_BASELINES §19).
```

---

# PARTE D — Materiais, essências e especiais

## 9. Materiais (10)

| Item (BV) | Fonte | Uso principal |
|---|---|---|
| item_material_wood (3) | árvores | construção/craft |
| item_material_stone (3) | rochas | construção |
| item_material_copper_ore (8) | mineração 1-10 | ferramentas early |
| item_material_iron_ore (15) | mineração 11-25 | gear ferro |
| item_material_leather (15) | drops beast | armor leve |
| item_material_silver_ore (30) | mineração 26-40 | counter espiritual |
| item_material_arcane_crystal (60) | mineração 41+/meteoro | focos/robe |
| item_material_mithril_ore (80) | mineração 56+ | gear leve/durável |
| item_material_bromecian_alloy (90) | drops construct/ruínas | gear técnico |
| item_material_star_iron (120) | Starfall Remnant/void | endgame |

## 10. Essências elementais (6 — sistema de Têmpera)

```text
item_essence_fire (60) · _ice (60) · _toxic (45) · _lightning (75) · _arcane (75) · _void (90)
Drop: 8% nas criaturas da banda correspondente; 100% nos minibosses da banda; elites +25%.
Uso: Têmpera de Essência na forja do Brumdar (versão PERMANENTE dos óleos — fable_22).
```

## 11. Especiais e drops de criatura

```text
Especiais (existentes, raros por canon): item_fruto_mana, item_agua_viva,
item_pedra_negra_estabilizada, item_blackstone_corrupted_shard.
Drops de criatura (BV 5-150 conforme banda): chitin, chitin_plate, fiber, spores, glowcap,
slime, grub_meat, rot_gland, gold_pouch, sinew, hide, white_pelt, fang, ember_fang, tusk,
bone, grave_dust, veil_cloth, spark_dust, mycel_thread, mycel_heart, frost_core, stone_core,
greater_core, gears, turret_core, warden_core, bromecian engravings, phantom_essence,
memory_shard, night_essence, moth_dust, shade_ash, abyssal_fang, lurker_eye, void_ichor,
wyrmling_scale, guardian_scale, dragon_ember_scale, elder_scale, choir_mask, ashwing_feather,
angler_lamp (upgrade!), glacier_hide, magma_chitin, fish_pale, mirrorfin.
A tabela peso/weight por família é autorada na execução da spec F06 usando esta lista.
```

---

# PARTE E — Gear: armas, armaduras, escudos e itens mágicos

## 12. Armas (16 nominais de loja/craft + únicas de drop)

```text
Loja/craft (stats da matriz canônica por tipo+material):
item_weapon_sword_iron (120) / _sword_steel (300) · _axe_iron (130) / _axe_steel (320)
item_weapon_hammer_iron (140) / _hammer_steel (340) · _spear_iron (125) / _spear_steel (310)
item_weapon_dagger_copper (90) / _dagger_steel (260) · _bow_wood (110) / _bow_steel (330)
item_weapon_staff_apprentice (150) / _staff_oak (360) · item_weapon_tool_shovel (60)
item_weapon_sword_silver (420 — counter espiritual, Brumdar pós-cadeia)
Tiers Mithril/Bromeciana/Pedra Negra/Meteórica: SOMENTE craft/têmpera (não loja).
Únicas de drop: warlord_cleaver, vask_hammer (de peças), master_blade (de peças),
elder_scale set (gate 100).
```

## 13. Armaduras (6) e escudos (3)

```text
item_armor_light_leather (100) · _light_studded (240) · _medium_iron (220) ·
_medium_steel (420) · _heavy_steel (520) · _robe_arcane (180)
item_shield_buckler (90) · _shield_round_iron (200) · _shield_tower_steel (450)
Stats por ArmorType/ShieldType da direction de baselines (ArmorFlat, mods de dash/dodge etc.).
```

## 14. Itens mágicos (wands 3 · scrolls 4)

```text
item_weapon_wand_simple (150) · _wand_fire (280) · _wand_frost (280) — cargas canônicas.
item_consumable_scroll_cast_fireburst (60) · _scroll_cast_barrier (70)
item_consumable_scroll_learn_fire_spark (350) · _scroll_learn_minor_heal (400)
LearnableScrolls aparecem na Ozzra APÓS a cadeia dela (sq_ozzra_3) — regra de conexão de quests.
```

---

# PARTE F — Acessórios e relíquias

## 15. Os três bolsos do aventureiro

Decisão humana: **3 slots** — Ring, Amulet e Charm (tipos canônicos). Acessório nunca dá
dano direto; dá a *vida ao redor do dano*: economia, sustain, resistência, conforto.

## 16. Catálogo (12)

| Item (BV) | Slot | Efeito | Fonte |
|---|---|---|---|
| item_acc_ring_thoren (400) | Ring | +15% durabilidade de ferramentas | Brumdar |
| item_acc_ring_finan (600) | Ring | +5% ouro em vendas | Renko |
| item_acc_ring_alihana (—) | Ring | +10% chance de 1 roll extra de loot raro | tesouro 20+ |
| item_acc_ring_swiftcurrent (500) | Ring | -10% custo de stamina de dash/dodge | Zrix |
| item_acc_ring_rootguard (450) | Ring | imune a Root; Chill -50% duração | Savra |
| item_acc_ring_emberward (—) | Ring | +resistência Fire/Heat | tesouro fire band |
| item_acc_amulet_anya (—) | Amulet | +25% de cura recebida | quest Ato 1 |
| item_acc_amulet_kanthor (700) | Amulet | +10% Block Stability; posture recebida -20% | templo |
| item_acc_amulet_senya (650) | Amulet | +10% dano mágico; +10% custo de MP | Ozzra |
| item_acc_amulet_nyx (—) | Amulet | -30% fadiga noturna; detecção de trap futura | mercador errante |
| item_acc_charm_thandra (600) | Charm | +15% efeito de comida; animais +10% produto | Eiran |
| item_acc_charm_stoneheart (350) | Charm | -50% knockback recebido; -5% move | Dagna |

## 17. Relíquias divinas (4 no v1 — raras, 1 por deus, drop de boss/quest)

```text
item_relic_kanthor — "Julgamento": perfect block cura 2% HP máx.
item_relic_kaand — "Fúria": crítico estende janelas de vulnerabilidade +0.5s.
item_relic_anya — "Esperança": Água Viva +1/dia (com a função desbloqueada).
item_relic_alihana — "Véu": 1 sonho/mês revela um segredo do calendário.
Regra: relíquia ocupa o slot do tipo correspondente (amulet/charm); só 1 relíquia equipada.
```

---

# PARTE G — Produtos animais, ferramentas e chaves

## 18. Produtos animais (4) [com qualidade]

```text
item_animal_egg (12) · item_animal_goat_milk (28) · item_animal_cow_milk (35) ·
item_animal_wool (40) — variantes _silver/_gold; produção via fable_12.
```

## 19. Ferramentas e utilitários

```text
Existentes: hoe, watering_can, pickaxe, axe, fishing_rod (+ tiers por upgrade do Brumdar).
item_tool_lantern (120) — alcance de luz padrão; UPGRADE: angler_lamp (drop do Deep Angler,
+50% raio e revela tells de mimics/anglers).
item_consumable_repair_kit_basic/standard/superior (existentes).
```

## 20. Chaves e documentos (key — não vendáveis)

```text
item_key_market_license (licença do Tovin, F19) · item_key_contract_farm_registry (Mara, F19)
item_key_lot_deed_north / _east / _south (alvarás de lote da fazenda — HUD_LAYOUT_SCENES §3)
item_key_warchief_crest · item_key_orc_warbanner · item_key_nymirian_engraving (quests)
```

---

# PARTE H — Decisões fechadas

```text
~118 itens base + ~24 variantes de qualidade (crops/produtos ×_silver/_gold).
Qualidade = itens separados (decisão Q3.2 resolvida).
6 sementes novas com identidade de lua/deusa (nomes aprovados).
Acessórios: 3 slots (Ring/Amulet/Charm), 12 itens, sem dano direto. Relíquias: 4 no v1.
Essências: 6, por banda, para a Têmpera permanente (fable_22).
LearnableScrolls gated pela cadeia da Ozzra; tiers altos de arma só por craft.
Todo item: ID com categoria + BV + fonte + uso; órfão = erro de validator (F30).
```

# PARTE I — Pendências

```text
Tabelas weight/raridade por família de drop (execução F06).
BV fino dos drops de banda 71+ (validar com curva de ouro do playtest).
Receitas de processamento (queijaria/barril) — números na spec de farm animals/crafting.
Itens de festival adicionais (1 por festival) — definir com o catálogo de eventos.
```

---

# EMENDA 2026-06-13-V3 (Refinamento v3)

> **Origem:** `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` — BLOCO 2 (ITENS), decisões
> 2.1, 2.2, 2.3, 2.5, 2.6, 2.8, 2.9, 2.10, 2.11.
> **Natureza:** emenda aditiva ao catálogo. Não substitui PARTES A-I acima; **detalha,
> corrige e fixa números** que estavam em aberto ou citados de forma imprecisa.
> **Precedência:** as regras de anti-duplicação da seção 0 continuam valendo — em conflito de
> LISTA/nome/BV este catálogo (incluindo esta emenda) vence; preço final/restock = ECONOMY_PRICING;
> stats mecânicos = EQUIPMENT_MECHANICAL_BASELINES.
> **Convenção de validação de receita:** sempre que esta emenda fixa um BV de item craftado,
> ele respeita a fórmula da economia §4/§6: para poções `inputs ×1.50-3.00`, óleos/processados
> `inputs ×1.20-3.00`, comidas `inputs ×1.10-1.80`. "inputs" = soma de `BV(ingrediente) ×
> quantidade` lendo os BVs das PARTES B-G acima.

## E2.1 Correção da regra de qualidade (decisão 2.1)

Confirma e **canoniza** a seção 2 deste documento contra a F32:

```text
Qualidade de crops e produtos animais = 3 NÍVEIS, itens separados por sufixo:
  Normal (sem sufixo)  BV ×1.0
  _silver (Prata)      BV ×1.5
  _gold   (Ouro)       BV ×2.0   ← CANÔNICO

Correção: a F32 (gerador) usava Gold ×2.2 — está ERRADO. Gold = ×2.0.
A tabela Q0-Q4 da ECONOMY_PRICING (§11: Q4 ×2.20) é de um sistema de qualidade FUTURO
(instâncias mais granulares) e NÃO se aplica a crops/produtos animais no v1.
Crops/produtos animais usam exclusivamente os 3 níveis acima.
```

## E2.5 Correção do número de essências (decisão 2.5)

```text
Essências elementais = 6 (seis), conforme a seção 10 deste catálogo:
  item_essence_fire · _ice · _toxic · _lightning · _arcane · _void.
Correção: qualquer citação de "8 essências" (inclusive a F32) está ERRADA. São 6.
Uso/drop permanecem como na seção 10 (Têmpera permanente — fable_22).
```

## E2.2 Receitas canônicas de poção e óleo (decisão 2.2)

Fixa os **ingredientes** das poções (seção 7) e óleos (seção 8), todos com ingredientes de
**drops/crops EXISTENTES** (PARTES B/D/G e §11). BV validado pela fórmula da economia
(poção `inputs ×1.5-3.0`; óleo tratado como processado alquímico `inputs ×1.2-3.0`).
Craft no balcão da Ozzra (alquimia).

| Item (BV) | Ingredientes canônicos | inputs (soma BV) | múltiplo efetivo | banda |
|---|---|---:|---:|---|
| item_consumable_potion_hp_small (40) | glowcap ×2 + fiber ×1 | ~16 | ×2.5 | comum |
| item_consumable_potion_hp_medium (90) | glowcap ×3 + sinew ×1 + crystal_berry ×1 | ~40 | ×2.25 | banda 26+ |
| item_consumable_potion_mp_small (45) | spores ×2 + mycel_thread ×1 | ~18 | ×2.5 | comum |
| item_consumable_potion_mp_medium (95) | spores ×3 + arcane_crystal_dust(mycel_heart) ×1 | ~42 | ×2.26 | banda 26+ |
| item_consumable_potion_antidote (35) | rot_gland ×1 + glowcap ×1 | ~14 | ×2.5 | obrigatória p/ fungal |
| item_consumable_potion_fire_resist (60) | frost_core ×1 + glowcap ×1 | ~24 | ×2.5 | banda fire |
| item_consumable_potion_ice_resist (60) | ember_fang ×1 + glowcap ×1 | ~24 | ×2.5 | banda ice |
| item_consumable_potion_stamina_draught (70) | sunpepper ×1 + moonbean ×1 + fiber ×1 | ~24 | ×2.9 | cooldown 5min |

Notas: `mycel_heart` e `frost_core` são drops de §11; `glowcap`/`spores`/`fiber` abundantes
na banda 1-25. As poções de resistência usam um drop de banda OPOSTA como reagente temático
(o "antídoto elemental"), sem trivializar boss.

```text
Óleos de arma (4 canônicos, BV 50 cada) — receitas:
  item_consumable_oil_fire   = ember_fang ×1 + fiber ×1   (inputs ~24, ×2.08)
  item_consumable_oil_frost  = frost_core ×1 + fiber ×1   (inputs ~24, ×2.08)
  item_consumable_oil_shock  = spark_dust ×2 + fiber ×1   (inputs ~24, ×2.08)
  item_consumable_oil_poison = rot_gland ×1 + spores ×2   (inputs ~24, ×2.08)
Aplicam a tag canônica do óleo a UMA arma por 3 min (counterplay via adapter, seção 8).
A versão PERMANENTE é a Têmpera de Essência (essências da seção 10 / E2.5; fable_22),
NÃO os óleos consumíveis.
```

## E2.3 Itens mágicos achados na caverna (decisão 2.3) — SEÇÃO PRÓPRIA

> **Correção de citação:** a fable_31 cita "ITEM_CATALOG §19" para os itens mágicos
> não-identificados. **§19 é "Ferramentas e utilitários"** — a citação está errada. A seção
> canônica dos itens mágicos da caverna passa a ser **esta E2.3** (e a F31 deve referenciar
> "ITEM_CATALOG E2.3 (EMENDA 2026-06-13-V3)", não "§19").

Os **8 itens mágicos** da F31 chegam NÃO-identificados (par de itens: `unidentified_trinket_N`
↔ item real; identificar = swap 1:1, sem metadata de instância). Fonte: baús raros nível 10+
e rotação semanal determinística da Veska (F25); identificação por serviço `IdentifyItem`
(Veska 120g — F25) ou pelo pergaminho `scroll_identify`. BV magic item = baseline Focus/Relic
da economia (§4: 250-3000+; consumíveis de uso 1× ficam no piso, passivos contínuos mais altos).

| Item (BV) | Raridade | Efeito | Tipo |
|---|---|---|---|
| item_magic_pendant_of_echoes (400) | Rare | mostra HP de inimigos (flag HUD) | passivo (ItemPassiveTracker) |
| item_magic_lantern_of_true_sight (450) | Rare | revela mimics/ambush em raio 6 (flag p/ F24) | passivo |
| item_magic_pouch_of_holding (600) | Epic | +6 slots de inventário | passivo |
| item_magic_candle_of_the_depths (350) | Rare | luz não consumível, raio maior | passivo |
| item_magic_whetstone_eternal (500) | Epic | 1 reparo grátis por dia | passivo (uso diário) |
| item_magic_bell_of_warding (220) | Uncommon | 1 uso: inimigos em raio 8 fogem 5s | consumível 1× |
| item_magic_mirror_of_return (260) | Uncommon | 1 uso: teleporta à ENTRADA do mesmo nível (stable-run intacto) | consumível 1× |
| item_magic_hourglass_of_dawn (300) | Rare | 1 uso: adianta para 6h (consome DayService) | consumível 1× |

Par de identificação e custo de revelação:

```text
item_unidentified_trinket_N  — categoria consumable/relic genérica, tooltip "???", BV 120
  (≈ custo do serviço da Veska; vendável por uma fração — não é arbitragem).
  N varre os 8 mágicos acima (par 1:1). Identificar = swap pelo item real.
item_consumable_scroll_identify (140)  — Uncommon; consome no uso; revela 1 trinket.
  Receita/fonte: vendido pela Ozzra após a cadeia dela e em baús; alternativa ao serviço da Veska.
Regra de não-arbitragem: o trinket não-identificado NÃO pode ser vendido pelo BV do item real
  (o jogador escolhe entre gastar p/ identificar OU vender barato o mistério).
```

## E2.6 Gear de tier alto craftável (decisão 2.6) — LISTA NOMINAL

Complementa a seção 12 ("Tiers Mithril/Bromeciana/Pedra Negra/Meteórica: SOMENTE craft/têmpera,
não loja"). **~14 itens nominais**, todos craft/têmpera na forja do Brumdar (gated). Stats vêm
de EQUIPMENT_MECHANICAL_BASELINES §16 (armas) / §26 (armaduras); aqui só a LISTA + BV.

BV pela economia §7 (Material + Recipe + Tier + StatBudget + Rarity): material base lido da
seção 9 (mithril_ore 80, bromecian_alloy 90, star_iron 120, pedra_negra_estabilizada raro),
escalonado acima do tier aço (sword_steel 300, heavy_steel 520). Bandas: Mithril ≈ Tier 5
(56+); Bromeciana ≈ Tier 5 (ruínas 56-70); Meteórica/Pedra Negra ≈ Tier 6 (endgame 71+).

| Item (BV) | Material | Classe | Banda |
|---|---|---|---|
| item_weapon_sword_mithril (640) | Mithril | arma leve/eficiente | Tier 5 |
| item_weapon_dagger_mithril (560) | Mithril | arma leve | Tier 5 |
| item_armor_light_mithril (700) | Mithril | armadura leve/móvel | Tier 5 |
| item_weapon_hammer_bromecian (760) | Liga bromeciana | arma técnica | Tier 5 |
| item_weapon_spear_bromecian (720) | Liga bromeciana | arma técnica | Tier 5 |
| item_armor_medium_bromecian (820) | Liga bromeciana | armadura técnica | Tier 5 |
| item_shield_bromecian_kite (700) | Liga bromeciana | escudo técnico | Tier 5 |
| item_weapon_sword_blackstone (1200) | Pedra Negra estabilizada | arma late/perigosa | Tier 6 |
| item_weapon_axe_blackstone (1240) | Pedra Negra estabilizada | arma late/perigosa | Tier 6 |
| item_armor_heavy_blackstone (1400) | Pedra Negra estabilizada | armadura late/risco | Tier 6 |
| item_weapon_sword_meteoric (1300) | Meteórico/Mana | arma endgame (físico+mágico) | Tier 6 |
| item_weapon_staff_meteoric (1350) | Meteórico/Mana | foco mágico endgame | Tier 6 |
| item_weapon_bow_meteoric (1280) | Meteórico/Mana | arco endgame | Tier 6 |
| item_armor_robe_meteoric (1320) | Meteórico/Mana | robe arcano endgame | Tier 6 |

```text
Regra: nenhum destes aparece em loja (CanBuy de loja = false). Apenas craft/têmpera no Brumdar,
gated por cadeia (Mithril Work, Engenharia Bromeciana, Pedra Negra estabilizada — ver
EQUIPMENT_WEAPONS_ARMOR_MATERIALS §24/§25/§27). Pedra Negra exige processo/lore/risco.
```

## E2.8 Drops órfãos do bestiário (decisão 2.8) — entram na seção 11

A seção 11 lista drops "soltos". Esta emenda **fixa BV por banda, nomes EN-only e uso declarado**
para ~20 drops órfãos do CAVE_BESTIARY_CATALOG, eliminando órfãos (regra F30). BV por banda:
1-25 baixo (8-30); 26-55 médio (30-80); 56-70 técnico (40-120); 71-101 alto (80-200).

> **Resolução de duplicata "stabilized blackstone" × "pedra_negra_estabilizada":** o nome
> canônico EN-only passa a ser **`item_material_stabilized_blackstone`**. O ID legado
> `item_pedra_negra_estabilizada` (citado em PT na seção 11) é **DEPRECADO** e mapeia para
> `item_material_stabilized_blackstone`. O `item_blackstone_corrupted_shard` (forma bruta/
> corrompida) permanece distinto: é a forma BRUTA, perigosa; estabilizá-la (processo do Brumdar)
> produz `item_material_stabilized_blackstone`.

| Item EN-only (BV) | Banda | Uso declarado |
|---|---|---|
| item_material_chitin (10) | 1-25 | armadura leve / craft early |
| item_material_chitin_plate (22) | 1-25 | reforço de armadura média |
| item_material_glowcap (14) | 1-25 | reagente de poção (E2.2) |
| item_material_spores (12) | 1-25 | reagente de poção MP/poison (E2.2) |
| item_material_grub_meat (16) | 1-25 | carne de comida (miners_ration, E2.9) |
| item_material_rot_gland (18) | 1-25 | reagente de antídoto/óleo poison (E2.2) |
| item_material_sinew (20) | 1-25 | craft de arco/corda |
| item_material_white_pelt (28) | 26-40 | armor leve _silver / presente |
| item_material_frost_core (45) | 26-40 | reagente fire_resist + têmpera ice (E2.2) |
| item_material_ember_fang (45) | 41-55 | reagente ice_resist + óleo fire (E2.2) |
| item_material_magma_chitin (60) | 41-55 | armadura resistente a fogo |
| item_material_shade_ash (50) | 41-55 | reagente arcano / craft sombra |
| item_material_spark_dust (40) | 11-55 | reagente óleo shock (E2.2) |
| item_material_mycel_thread (24) | 11-25 | reagente poção MP (E2.2) |
| item_material_mycel_heart (70) | 11-25 | reagente raro poção MP média (E2.2) |
| item_material_gears (60) | 56-70 | componente técnico bromeciano |
| item_material_turret_core (90) | 56-70 | componente técnico / armadilha |
| item_material_warden_core (120) | 56-70 | componente técnico raro |
| item_material_phantom_essence (110) | 56-70 | reagente arcano / craft espectral |
| item_material_night_essence (130) | 56-70 | reagente sombra / craft noturno |
| item_material_void_ichor (160) | 71-101 | reagente void endgame / têmpera void |
| item_material_abyssal_fang (150) | 71-101 | arma/craft de profundeza |
| item_material_wyrmling_scale (140) | 71-101 | armadura escamada |
| item_material_lurker_eye (150) | 71-101 | foco/craft de visão verdadeira |
| item_material_stabilized_blackstone (200) | 71-101 | gear Pedra Negra (E2.6) / anti-corrupção |

```text
Os demais drops já nomeados na seção 11 (slime, fiber, bone, grave_dust, veil_cloth, hide,
fang, tusk, gold_pouch, stone_core, greater_core, glacier_hide, fish_pale, mirrorfin,
angler_lamp, choir_mask, ashwing_feather, memory_shard, moth_dust, guardian_scale,
dragon_ember_scale, elder_scale) permanecem; quando usados como reagente/peça aqui ganham o
mesmo padrão EN-only item_material_<slug>. Peso/raridade por família continua pendência F06.
```

## E2.9 Água, carne e leite (decisão 2.9 — OVERRIDE)

**OVERRIDE das receitas das seções 5/8 que usavam "água"/"carne"/"milk" genéricos.**

```text
ÁGUA VIRA ITEM COLETÁVEL (NÃO recurso infinito de estação de cozinha):
  item_tool_bucket (60) — ferramenta; coleta água em ponto de coleta (poço da fazenda / lago).
  item_material_water (1) — material coletável (BV mínimo; não vendável de forma lucrativa).
    Fonte: usar o balde no poço da fazenda OU em lago (fazenda/caverna). NÃO é gerado do nada
    na cozinha — exige o balde e o ponto de coleta. Refresh do ponto = ECONOMY_PRICING §25-26
    (resource node). Sem balde, sem água.
  → Receitas que pediam "+ água" agora pedem "+ item_material_water" explicitamente:
     carrot_stew, pumpkin_soup, tear_tonic (e qualquer outra com "água" nas seções 5/7).

CARNE = grub_meat (drop EXISTENTE, E2.8). Não criar item de carne genérico.
  → miners_ration usa item_material_grub_meat; pepper_feast usa item_material_grub_meat
    no lugar de "+ carne".

LEITE: festival_cake exige COW_MILK ESPECIFICAMENTE (não "qualquer leite"):
  → item_consumable_food_festival_cake = wheat + egg + item_animal_cow_milk + crystal_berry
    (a versão da seção 5 dizia "milk" genérico; cow_milk é obrigatório; goat_milk NÃO serve).
```

## E2.10 Roster de peixes expandido (decisão 2.10) — ~10 peixes

Substitui o uso genérico de `fish_common`/`mirrorfin` por um roster nominal de **10 peixes**:
4 sazonais do açude da fazenda (1-2 por estação), 5 de caverna (2-3 por banda, [AQUÁTICA] —
lagos subterrâneos, ver CAVE_BESTIARY §3/§19 e Lake Lurker/Mirrorfin Shoal) e 1 do Moonless
Pool (águas mais profundas da banda VOID/Sem-Lua). BV pela economia §4 (Fish comum 12-160).

| Item (BV) | Local / fonte | Estação / banda | Raridade |
|---|---|---|---|
| item_fish_river_perch (14) | açude da fazenda | Primavera | Common |
| item_fish_sun_bass (22) | açude da fazenda | Verão | Common |
| item_fish_amber_trout (30) | açude da fazenda | Outono | Uncommon |
| item_fish_frostfin (38) | açude da fazenda | Inverno | Uncommon |
| item_fish_pale (24) | lago de caverna (Lake Lurker) | banda 1-25 | Common |
| item_fish_cave_eel (45) | lago de caverna | banda 26-55 | Uncommon |
| item_fish_mirrorfin (55) | lago de caverna (Mirrorfin Shoal) | banda 26-55 | Uncommon |
| item_fish_emberfish (70) | lago de caverna | banda 41-70 | Rare |
| item_fish_ruin_lamprey (90) | lago de caverna | banda 56-70 | Rare |
| item_fish_void_angler (160) | **Moonless Pool** (águas profundas) | banda 86-101 (VOID/Sem-Lua) | Epic |

```text
fish_common (citado nas receitas da seção 5) passa a ser item_fish_river_perch como peixe
base/inicial; grilled_fish aceita qualquer peixe (qualquer item_fish_*).
mirrorfin já existia → canoniza como item_fish_mirrorfin (mantém mirrorfin_sashimi).
fish_pale (drop do Lake Lurker, §11) → canoniza como item_fish_pale.
O Moonless Pool é o lago raro garantido na banda VOID (geração: ECONOMY/CAVE — pendência de
geração de lago na banda 86-101). 1 peixe icônico (void_angler).
```

## E2.11 Tabela derivada de durabilidade e custo de upgrade (decisão 2.11)

Tabela DERIVADA (não redefine stats; aplica os DurabilityModifier de
EQUIPMENT_MECHANICAL_BASELINES §16/§26 às bases). **Bases:** arma 80 / armadura 150
(escudo usa a base de armadura: 150). DurabilityMax = round(base × (1 + DurabilityModifier)).

| Material | Modifier (§16/§26) | DurabilityMax arma (base 80) | DurabilityMax armadura/escudo (base 150) |
|---|---:|---:|---:|
| Madeira / Couro | -50% / baixo (-40%) | 40 | 90 |
| Pedra | -30% | 56 | 105 |
| Cobre | -25% | 60 | — |
| Ferro | +0% | 80 | 150 |
| Aço | +15% | 92 | 173 |
| Aço refinado | +25% | 100 | 188 |
| Prata | -5% | 76 | 143 |
| Mithril | +35% | 108 | 203 |
| Liga bromeciana | +40% | 112 | 210 |
| Cristal arcano | -10% | 72 | 135 |
| Pedra Negra estabilizada | +20% | 96 | 180 |
| Meteórico/Mana | +30% | 104 | 195 |

```text
Notas:
- Couro usa "baixo" → tratado como -40% (90 de DurabilityMax de armadura), entre Madeira e Pedra.
- Cobre só existe como arma early (sem armadura de cobre no catálogo) → célula armadura vazia.
- Escudo compartilha a base de armadura (150) com o modifier do material correspondente.
- DurabilityMax fino por ITEM pode subir com upgrade do Brumdar (tiers de ferramenta/arma).
```

Custo de upgrade **+N** (forja do Brumdar; serviço, ECONOMY_PRICING §1 ServicePrice):

```text
UpgradeCost(+N) = (2N × MaterialBandValue) + (BV × 0.5N) ouro

  N                = nível de upgrade alvo (+1, +2, ...).
  MaterialBandValue= BV do material da banda do item (seção 9; ex.: iron_ore 15, steel≈ via
                     ingot, silver_ore 30, mithril_ore 80, bromecian_alloy 90, star_iron 120).
  BV               = BaseValue do próprio item sendo melhorado.

Exemplos:
  sword_iron (BV 120, material iron_ore 15) +1 = (2×15) + (120×0.5) = 30 + 60 = 90 ouro.
  sword_iron +2                              = (4×15) + (120×1.0) = 60 + 120 = 180 ouro.
  sword_mithril (BV 640, mithril_ore 80) +1  = (2×80) + (640×0.5) = 160 + 320 = 480 ouro.
  armor_heavy_steel (BV 520, steel band ~25) +1 = (2×25) + (520×0.5) = 50 + 260 = 310 ouro.

Regra: o custo cresce com N e com o BV do item — upgrades de gear caro/late são caros (sink
econômico), respeitando ServicePrice da ECONOMY_PRICING. Material consumido por upgrade fica
para a spec de forja (não redefinido aqui).
```

## Resumo da emenda (decisões fechadas — adendo à PARTE H)

```text
Qualidade: 3 níveis, Silver ×1.5 / Gold ×2.0 (Gold ×2.2 da F32 = ERRO corrigido). [2.1]
Essências: 6 (correção do "8" da F32). [2.5]
Poções (8) e óleos (4): receitas canônicas de drops/crops existentes, BV pela fórmula. [2.2]
Itens mágicos da caverna: 8 + unidentified_trinket_N + scroll_identify, seção E2.3 própria;
  a citação "§19" da F31 está errada (§19 = ferramentas) → usar E2.3. [2.3]
Gear tier alto: ~14 itens nominais craft/têmpera (Mithril/Bromeciana/Pedra Negra/Meteórica). [2.6]
Drops órfãos: ~20 com BV por banda, EN-only; pedra_negra_estabilizada (PT) DEPRECADO →
  item_material_stabilized_blackstone; corrupted_shard = forma bruta distinta. [2.8]
Água = item coletável (item_tool_bucket + item_material_water, poço/lago — NÃO infinito);
  carne = grub_meat; festival_cake exige cow_milk. [2.9]
Peixes: ~10 (4 açude sazonal + 5 caverna por banda + 1 Moonless Pool), BVs fixados. [2.10]
Durabilidade: DurabilityMax por classe×material (bases 80 arma / 150 armadura × §16/§26);
  upgrade +N = (2N × material da banda) + (BV × 0.5N) ouro. [2.11]
```
