# Cindar's Hope — Cave Monster Roster Direction

> **Status:** documento canônico de roster, descrição, atributos, packs, scaling e direção de criaturas da caverna  
> **Local:** `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md`  
> - `docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> **Função:** detalhar criaturas, bosses, packs, comportamento, ataques, atributos, scaling, XP, recursos, tesouros e função de gameplay.  
> **Não é spec implementável.** Specs futuras devem converter estes dados em `EnemyDataSO`, `EnemyActionSO`, `EnemySpawnProfileSO`, `EnemySpawnPackSO`, `LootTableSO` e bestiary entries.

---

## 0. Regra de uso

Este documento é a fonte de design para monstros da caverna.

Specs que criarem/alterarem inimigos devem ler este documento antes de mexer em:

```text
EnemyDataSO
EnemyActionSO
EnemyActionSetSO
EnemyBrain
EnemySpawnProfileSO
EnemySpawnPackSO
EnemyFactionLockSO
EnemyBestiaryEntrySO
LootTableSO
XP rewards
boss gate data
```

Regra de referência externa:

```text
Usar D&D apenas como referência de arquétipos de dungeon fantasy.
Não copiar statblocks, textos, habilidades proprietárias, lore proprietária ou progressão oficial.
Criaturas icônicas devem ser adaptadas para Vaalara com nomes, função, comportamento e dados próprios.
```

Exemplos de adaptação permitida:

```text
Beholder-like -> Observador Tirano da Pedra Negra
Mimic-like -> Baú-Mordente
Gelatinous cube-like -> Cubo de Lodo Translúcido
Mind flayer-like -> Devora-Mentes Abissal
Rust monster-like -> Besouro Ferrugem
Displacer beast-like -> Pantera Distorcida
Umber hulk-like -> Titã Escavador
```

---

# PARTE A — Contratos de design

## 1. Campos conceituais de cada criatura

Cada criatura deve ter:

```text
EnemyId
Nome PT-BR
Faixa nativa de níveis
Pode aparecer fora da faixa?
Bioma principal
Faction
Roles
MovementProfile
SizeClass
Visual físico
Comportamento
Ataques principais
Poderes/status
Fraquezas/janelas de vulnerabilidade
Atributos base
Scaling por nível
XP base
Drops/recursos
Tesouros raros
Função de gameplay
Bestiary lore curta
```

## 2. Atributos usados por inimigos

Inimigos devem usar a mesma família de atributos do jogo:

```text
HP
MP
Stamina
Breath/Fôlego
Força
Constituição
Destreza
Inteligência
Vontade
Carisma
```

Escala dos atributos principais:

```text
1 = muito baixo
2 = baixo
3 = comum
4 = bom
5 = ótimo
6 = excepcional
7 = boss/elite extremo
8 = boss final/endgame
```

## 3. Size classes e footprint visual

| SizeClass | Sprite visual sugerido | Collider/footbox | Uso |
|---|---:|---:|---|
| Tiny | 16x16 a 24x24 px | 12x8 px | enxames, wisps, ácaros |
| Small | 24x32 a 32x40 px | 16x10 px | goblins, kobolds, diabretes |
| Medium | 32x48 px | 20x12 a 24x16 px | humanoides, mortos-vivos, cultistas |
| Large | 48x56 a 64x64 px | 32x20 px | orcs grandes, constructos, feras |
| Huge | 80x80 a 96x96 px | 48x28 px | hulks, colossos, monstros de sala |
| Boss | 96x96 a 160x160 px | custom | boss gate e level 101 |

Regra:

```text
O collider representa a base do inimigo.
Criaturas grandes exigem sala compatível e safe anchors próprios.
```

## 4. Scaling por nível da caverna

Cada inimigo possui uma faixa nativa. Quando aparece acima ou abaixo dela, deve sofrer scaling.

```text
NativeMinLevel
NativeMaxLevel
SpawnLevel
Delta = SpawnLevel - NativeMidLevel
```

Direção de scaling:

| Caso | Regra |
|---|---|
| Dentro da faixa nativa | usa atributos base + scaling leve por profundidade |
| Até 10 níveis acima | aumenta HP, dano, XP e chance de drop raro |
| 11+ níveis acima | vira variante reforçada, elite ou corrompida |
| Até 5 níveis abaixo | reduz HP/dano/XP e drop raro |
| 6+ níveis abaixo | só aparece como evento fraco, swarm ou não aparece |

Multiplicadores conceituais:

```text
HPScale = 1.0 + max(0, Delta) * 0.045
DamageScale = 1.0 + max(0, Delta) * 0.035
XPScale = 1.0 + max(0, Delta) * 0.05
DropRareScale = +0.5% por nível acima, com cap por spec

Se Delta negativo:
HPScale mínimo 0.65
DamageScale mínimo 0.70
XPScale mínimo 0.60
```

## 5. Variantes por profundidade

Quando uma criatura aparece acima da faixa nativa, ela pode receber sufixos/variantes:

| Variante | Condição | Efeito de design |
|---|---|---|
| Veterano | +6 níveis acima | mais HP, melhor XP |
| Elite | +10 níveis acima ou sala especial | nova ação ou status leve |
| Corrompido | 86+ ou Pedra Negra | dano/efeito escuro, drop corrompido |
| Lunar | Nyx/Alihana/evento | ação mágica/visão/luz/sombra |
| Bromeciano | ruínas 56+ | partes mecânicas, resistência física |
| Ígneo | fogo 41-55 | Burn/resistência a fogo |
| Gélido | gelo 26-40 | Chill/resistência a frio |
| Raiz-Negra | floresta/corrupção | Root/Poison leve |

## 6. XP conceitual

XP será balanceado em spec própria, mas a direção inicial é:

| Tier | Faixa | XP comum | XP elite | XP boss |
|---|---:|---:|---:|---:|
| T1 | 1-10 | 8-18 | 25-40 | — |
| T2 | 11-25 | 18-38 | 45-75 | 300-450 |
| T3 | 26-40 | 35-65 | 80-130 | 600-850 |
| T4 | 41-55 | 60-105 | 130-210 | 1000-1400 |
| T5 | 56-70 | 95-160 | 220-360 | 1600-2200 |
| T6 | 71-85 | 150-240 | 360-560 | 2600-3400 |
| T7 | 86-99 | 220-360 | 560-850 | 4000-5200 |
| Gate 100 | 100 | — | 800-1200 | 7000-9000 |
| Level 101 | 101 | — | — | 9000+ cada boss |

---

# PARTE B — Factions, roles e profiles

## 7. Factions oficiais da caverna

```text
faction_beast
faction_fungal
faction_goblin
faction_kobold
faction_orc
faction_duergar
faction_drow
faction_gnome
faction_ninrorin
faction_undead
faction_cultist
faction_elemental
faction_construct
faction_abyssal
faction_corrupted
faction_draconic
faction_aberrant
```

`faction_aberrant` cobre criaturas de olho, mente, distorção, lodo consciente e horrores de dungeon adaptados para Vaalara.

## 8. Roles oficiais

```text
Chaser
Guard
Ranged
Caster
Burrower
Swarm
Tank
Elite
MiniBoss
Boss
Controller
TreasureTrap
```

## 9. Movement profiles oficiais

```text
GroundChase
GroundPatrol
GuardStationary
KiteRanged
CasterKeepAway
BurrowAmbush
SwarmErratic
TankSlowPush
PhaseShortBlink
Leaper
FloatingSlow
FloatingOrbit
TreasureIdleAmbush
```

---

# PARTE C — Roster por faixa

## 10. Níveis 1-10 — Caverna de Pedra

| EnemyId | Nome | Faction | Roles | Size | Atributos fortes | XP | Drops principais |
|---|---|---|---|---|---|---:|---|
| enemy_cave_mite | Ácaro da Fenda | beast | Swarm, Chaser | Tiny | Destreza 4 | 8 | quitina pequena, pedra miúda |
| enemy_stone_rat | Rato de Basalto | beast | Chaser | Small | Destreza 4, Constituição 3 | 10 | pele áspera, carvão baixo |
| enemy_cave_bat | Morcego de Fenda | beast | Swarm, Ranged | Tiny | Destreza 5, Breath 4 | 12 | asa fina, eco mineral |
| enemy_goblin_grashnaar_scavenger | Saqueador Grash'naar | goblin | Chaser, Ranged | Small | Destreza 4, Carisma 3 | 16 | sucata, cobre baixo, moeda |
| enemy_kobold_scout | Batedor Kobold | kobold | Chaser, Ranged | Small | Destreza 4, Inteligência 3 | 16 | garra, pedra polida |
| enemy_mossling | Musguinho Errante | fungal | Guard, Tank | Small | Constituição 4 | 14 | esporo verde, fibra úmida |
| enemy_cracked_bone | Osso Rachado | undead | Chaser | Medium | Força 3, Constituição 3 | 18 | osso seco, pó mineral |
| enemy_blackroot_sprout | Broto Raiz-Negra | fungal | Ranged, Guard | Small | Vontade 3, Constituição 3 | 18 | raiz negra fraca, seiva escura |
| enemy_translucent_sludge_cube | Cubo de Lodo Translúcido | aberrant | Tank, TreasureTrap | Large | Constituição 5 | 35 | gel ácido, moeda corroída |
| enemy_rust_beetle | Besouro Ferrugem | beast | Chaser, Controller | Small | Destreza 4 | 22 | carapaça oxidada, pó ferrugem |
| enemy_chest_biter | Baú-Mordente | aberrant | TreasureTrap, Chaser | Medium | Força 4, Constituição 4 | 40 | madeira viva, dente de baú, loot guardado |

### Descrição de criaturas 1-10

```text
enemy_translucent_sludge_cube
Arquétipo de cubo gelatinoso adaptado. Massa translúcida que ocupa corredor e digere sucata. Lento, mas perigoso se o jogador ficar preso. Pode guardar moedas, ossos, pequenas gemas e restos de ferramentas.

 enemy_rust_beetle
Besouro atraído por metal e minério. Em vez de destruir equipamento de forma punitiva, aplica debuff temporário de DurabilityStress ou reduz eficiência de ferramenta até reparo/tempo, se esse sistema existir.

 enemy_chest_biter
Criatura disfarçada de baú quebrado. Deve ser rara nos andares baixos. Ensina que nem todo tesouro é seguro.
```

---

## 11. Níveis 11-25 — Floresta Subterrânea

| EnemyId | Nome | Faction | Roles | Size | Atributos fortes | XP | Drops principais |
|---|---|---|---|---|---|---:|---|
| enemy_spore_imp | Diabrete de Esporo | fungal | Caster, Swarm | Small | MP 4, Destreza 4 | 22 | esporo irritante, fungo arcano |
| enemy_rootsnare | Garra-Raiz | fungal | Guard, Burrower | Medium | Constituição 4, Vontade 3 | 30 | fibra de raiz, seiva grossa |
| enemy_hollow_stagling | Cervino Oco | beast | Chaser, Elite | Medium | Destreza 5, Breath 4 | 55 | chifre oco, couro sombrio |
| enemy_goblin_urudakh_trapper | Armeiro Uru'dakh | goblin | Ranged, Guard | Small | Inteligência 4, Destreza 4 | 28 | armadilha simples, sucata |
| enemy_thorn_archer | Espinhador Sombrio | goblin | Ranged | Medium | Destreza 5 | 32 | espinho escuro, madeira dura |
| enemy_orc_nyx_stalker | Espreitador Orc de Nyx | orc | Chaser, Burrower | Medium | Destreza 5, Vontade 4 | 48 | osso ritual, couro negro |
| enemy_mycobulwark | Baluarte Micélio | fungal | Tank, Guard | Large | Constituição 6 | 75 | placa fúngica, esporo raro |
| enemy_nyx_moth | Mariposa de Nyx | abyssal | Caster, Swarm | Small | MP 4, Vontade 4 | 38 | pó lunar escuro, asa macia |
| enemy_root_owlbear | Urso-Coruja Raiz-Oca | beast | Elite, Chaser, Tank | Large | Força 5, Constituição 5 | 90 | garra pesada, pena-raiz, couro |
| enemy_basilisk_lizard | Lagarto Basilisco de Musgo | beast | Controller, Guard | Medium | Constituição 4, Vontade 4 | 85 | olho opaco, escama musgosa |
| enemy_panther_distorted | Pantera Distorcida | aberrant | Elite, PhaseShortBlink | Medium | Destreza 6 | 95 | pelo distorcido, eco de sombra |

### Adaptações icônicas 11-25

```text
enemy_root_owlbear
Inspirado no arquétipo de besta híbrida de dungeon, mas adaptado para Vaalara. Corpo de urso, penas rígidas, olhos âmbar e raízes no dorso. Ataca com investida e garra pesada.

 enemy_basilisk_lizard
Baseado em criatura mitológica de petrificação, mas no jogo aplica Slow/StoneGaze buildup leve, não morte instantânea. Deve ter telegraph visual claro.

 enemy_panther_distorted
Arquétipo de fera deslocada. Não deve copiar mecânica proprietária. Usa distorção visual curta e reposicionamento lateral.
```

---

## 12. Níveis 26-40 — Caverna de Gelo

| EnemyId | Nome | Faction | Roles | Size | Atributos fortes | XP | Drops principais |
|---|---|---|---|---|---|---:|---|
| enemy_frost_gnawer | Roedor de Geada | beast | Chaser | Small | Destreza 4 | 36 | presa fria, cristal baixo |
| enemy_duergar_frostdelver | Escavador Duergar do Gelo | duergar | Guard, Tank | Medium | Força 4, Constituição 4 | 48 | minério frio, ferramenta quebrada |
| enemy_duergar_shieldbreaker | Quebra-Escudo Duergar | duergar | Tank, Elite | Large | Força 5, Constituição 5 | 105 | fragmento de escudo, ferro frio |
| enemy_icebound_sentinel | Sentinela Enregelado | construct | Guard, Tank | Large | Constituição 6 | 95 | placa congelada, núcleo fraco |
| enemy_glassbone | Osso de Vidro | undead | Ranged, Chaser | Medium | Destreza 4, Vontade 3 | 52 | osso cristalino, pó de gelo |
| enemy_cold_cult_acolyte | Acólito do Frio | cultist | Caster | Medium | MP 5, Vontade 4 | 58 | tecido frio, símbolo quebrado |
| enemy_crystal_leaper | Saltador Cristalino | elemental | Chaser, Elite | Medium | Destreza 6, Breath 5 | 110 | lasca cristalina, gema fria |
| enemy_frost_wailer | Lamento Frio | undead | Caster, Elite | Medium | MP 5, Vontade 5 | 125 | eco gelado, essência fria |
| enemy_hook_horror_ice | Horror-Gancho de Gelo | aberrant | Elite, Chaser | Large | Força 5, Constituição 5 | 135 | gancho de gelo, couro pálido |
| enemy_mind_eater_larva | Larva Devora-Mentes | aberrant | Controller, Caster | Small | MP 5, Inteligência 4 | 120 | tecido neural, muco psíquico |

### Adaptações icônicas 26-40

```text
enemy_hook_horror_ice
Criatura alta com braços em ganchos congelados. Usa ataques amplos e telegraph longo. Boa para salas médias, ruim para corredores estreitos.

 enemy_mind_eater_larva
Forma menor de horrores psíquicos do abismo. Aplica Confusion-lite ou AimDisrupt, sem controle total do jogador. Deve ser rara.
```

---

## 13. Níveis 41-55 — Caverna de Fogo

| EnemyId | Nome | Faction | Roles | Size | Atributos fortes | XP | Drops principais |
|---|---|---|---|---|---|---:|---|
| enemy_ember_tick | Carrapato de Brasa | beast | Swarm, Chaser | Tiny | Destreza 5 | 62 | brasa pequena, quitina quente |
| enemy_ash_crawler | Rastejante de Cinza | elemental | Chaser | Medium | Força 4, Breath 4 | 76 | cinza mineral, carvão alto |
| enemy_orc_kaand_berserker | Berserker Orc de Kaand | orc | Chaser, Elite | Large | Força 6, Constituição 5 | 170 | couro queimado, símbolo de Kaand |
| enemy_orc_kaand_ashcaller | Chamador de Cinzas de Kaand | orc | Caster, Ranged | Medium | MP 5, Vontade 4 | 135 | cinza ritual, osso marcado |
| enemy_lava_bulwark | Baluarte de Lava | elemental | Tank, Guard | Large | Constituição 6 | 150 | pedra ígnea, núcleo quente |
| enemy_cinder_spitter | Cuspidor de Cinza | beast | Ranged | Medium | Destreza 4, Breath 5 | 92 | glândula de cinza, brasa |
| enemy_scorched_cultist | Cultista Chamuscado | cultist | Caster | Medium | MP 5, Vontade 4 | 110 | pano chamuscado, reagente ígneo |
| enemy_furnace_warden | Guardião da Fornalha | construct | Guard, Elite | Large | Constituição 6, Força 5 | 210 | peça de fornalha, engrenagem quente |
| enemy_fire_basilisk | Basilisco de Brasa | beast | Controller, Ranged | Large | Constituição 5, MP 4 | 185 | escama quente, olho de brasa |
| enemy_stone_bulette | Tubarão de Pedra | beast | Burrower, Elite | Huge | Força 6, Constituição 6 | 260 | placa dorsal, dente de pedra |

### Adaptações icônicas 41-55

```text
enemy_fire_basilisk
Variante de basilisco adaptada ao calor. Seu olhar aplica HeatShock/Slow progressivo, nunca morte instantânea.

 enemy_stone_bulette
Arquétipo de predador subterrâneo. Move-se por burrow ambush, emerge com telegraph forte e deve ser usado em salas grandes.
```

---

## 14. Níveis 56-70 — Ruínas Antigas

| EnemyId | Nome | Faction | Roles | Size | Atributos fortes | XP | Drops principais |
|---|---|---|---|---|---|---:|---|
| enemy_rune_shard | Lasca Rúnica | construct | Swarm, Ranged | Small | MP 3, Destreza 4 | 98 | lasca rúnica, pó arcano |
| enemy_clockwork_guard | Guarda de Corda | construct | Guard, Tank | Medium | Constituição 5 | 130 | engrenagem, mola antiga |
| enemy_gnome_gem_madcap | Gnomo de Gema Enlouquecido | gnome | Caster, Ranged | Small | Inteligência 5, MP 4 | 145 | gema trincada, ferramenta fina |
| enemy_gnomorin_rune_tinker | Gnomorin Runa-Torta | gnome | Ranged, Guard | Small | Inteligência 5, Destreza 4 | 150 | peça bromeciana, runa falha |
| enemy_sealed_knight | Cavaleiro Selado | undead | Tank, Elite | Large | Força 5, Constituição 6 | 280 | placa antiga, juramento quebrado |
| enemy_mirror_adept | Adepto do Espelho | cultist | Caster, Elite | Medium | MP 6, Inteligência 5 | 260 | fragmento refletivo, símbolo oculto |
| enemy_puzzle_golem | Golem de Enigma | construct | Tank, Guard | Large | Constituição 7, Inteligência 4 | 320 | núcleo lógico, pedra trabalhada |
| enemy_oathless_shade | Sombra Sem-Juramento | undead | Caster, Elite | Medium | Vontade 6, MP 5 | 300 | eco de juramento, tecido antigo |
| enemy_beholder_kin_lesser | Observador Menor da Ruína | aberrant | Controller, Ranged, Elite | Large | MP 6, Inteligência 5 | 360 | lente ocular, nervo arcano |
| enemy_mimic_armory | Arsenal-Mordente | aberrant | TreasureTrap, Tank | Large | Força 5, Constituição 6 | 340 | madeira viva, metal mastigado, loot guardado |
| enemy_brain_jelly | Geleia-Memória | aberrant | Controller, Caster | Medium | MP 6, Vontade 5 | 310 | gel mental, memória quebrada |

### Adaptações icônicas 56-70

```text
enemy_beholder_kin_lesser
Criatura ocular inspirada no arquétipo de observador arcano, mas original de Vaalara. Corpo flutuante irregular, olho central rachado e olhos menores como cristais. Não usa raios proprietários; usa feixes elementais simples e telegraph claros.

 enemy_mimic_armory
Versão mais perigosa do Baú-Mordente. Imita armário, baú ou rack de armas. Deve proteger tesouros reais.

 enemy_brain_jelly
Lodo consciente que guarda ecos de memória. Aplica debuffs leves de mira/movimento e solta fragmentos de lore raro.
```

---

## 15. Níveis 71-85 — Abismo Sombrio

| EnemyId | Nome | Faction | Roles | Size | Atributos fortes | XP | Drops principais |
|---|---|---|---|---|---|---:|---|
| enemy_drow_shadowblade | Lâmina Sombria Drow | drow | Chaser, Elite | Medium | Destreza 6, Vontade 4 | 390 | lâmina escura, tecido drow |
| enemy_drow_moon_caster | Conjurador Lunar Drow | drow | Caster, Elite | Medium | MP 6, Vontade 5 | 430 | pó lunar, foco lunar |
| enemy_drow_web_scout | Batedor de Teia Drow | drow | Ranged, Guard | Medium | Destreza 5, Inteligência 4 | 360 | fio escuro, seta fina |
| enemy_void_caster | Conjurador do Vazio | abyssal | Caster, Elite | Medium | MP 7, Inteligência 5 | 520 | essência vazia, fragmento escuro |
| enemy_abyss_wisp | Fagulha do Abismo | abyssal | Swarm, Caster | Small | MP 4, Destreza 5 | 240 | eco sombrio, partícula lunar |
| enemy_moonless_hound | Cão Sem-Lua | beast | Chaser, Leaper | Medium | Destreza 6, Breath 5 | 310 | presa escura, couro frio |
| enemy_black_lantern_cultist | Cultista da Lanterna Negra | cultist | Caster, Guard | Medium | MP 5, Vontade 5 | 340 | lanterna quebrada, óleo escuro |
| enemy_oath_eater | Devorador de Juramento | abyssal | Tank, Elite | Large | Constituição 6, Vontade 6 | 560 | fragmento de voto, essência abissal |
| enemy_mind_eater_adult | Devora-Mentes Abissal | aberrant | Controller, Caster, Elite | Medium | MP 7, Inteligência 6 | 620 | tecido neural, cristal psíquico |
| enemy_eye_tyrant_blackmoon | Tirano Ocular da Lua Negra | aberrant | Controller, Caster, MiniBoss | Boss | MP 8, Inteligência 6 | 1600 | lente negra, olho lunar, fragmento de Nyx |

### Adaptações icônicas 71-85

```text
enemy_mind_eater_adult
Arquétipo de horror psíquico adaptado para Vaalara. Cabeça alongada, olhos leitosos, tentáculos curtos e manto orgânico. Ataca com pulso mental, zona de confusão leve e projéteis de dor arcana. Não controla o jogador de forma total.

 enemy_eye_tyrant_blackmoon
Criatura ocular grande. É o primeiro “beholder-like” real. Deve aparecer como miniboss raro ou sala especial, não como inimigo comum. Usa olho central que aplica pressão de zona e olhos menores com feixes variados.
```

---

## 16. Níveis 86-99 — Núcleo Corrompido

| EnemyId | Nome | Faction | Roles | Size | Atributos fortes | XP | Drops principais |
|---|---|---|---|---|---|---:|---|
| enemy_corrupt_hulk | Massa Corrompida | corrupted | Tank, Chaser | Huge | Força 7, Constituição 7 | 720 | carne mineral, essência corrompida |
| enemy_ninrorin_broken_oracle | Oráculo Ninrorin Quebrado | ninrorin | Caster, Elite | Medium | MP 7, Inteligência 6 | 760 | fragmento profético, tecido antigo |
| enemy_corrupted_pseudodragon | Pseudodragão Corrompido | draconic | Caster, Ranged | Small | Destreza 6, MP 5 | 620 | escama pequena, sopro instável |
| enemy_blackstone_wyvern | Wyvern de Pedra Negra | draconic | Boss, Elite | Boss | Força 7, Constituição 7 | boss/miniboss | escama negra, núcleo dracônico |
| enemy_blackstone_cult_paragon | Paragon da Pedra Negra | cultist | Caster, Elite | Medium | MP 7, Vontade 6 | 680 | símbolo negro, fragmento instável |
| enemy_core_mirror | Reflexo do Núcleo | corrupted | Phase, Caster | Medium | Destreza 6, MP 6 | 610 | vidro escuro, eco arcano |
| enemy_mana_warped_beast | Fera Distorcida por Mana | beast | Chaser, Elite | Large | Força 6, Destreza 6 | 650 | pelo arcano, carne instável |
| enemy_anya_silent_echo | Eco Silencioso de Anya | corrupted | Guard, Caster | Medium | Vontade 8, MP 7 | especial | fragmento de lore, não farmável |
| enemy_beholder_blackstone | Observador Tirano da Pedra Negra | aberrant | Controller, Caster, Boss | Boss | MP 8, Inteligência 7 | 2600 | olho tirano, lente de Pedra Negra |
| enemy_umber_hulk_corebreaker | Titã Escavador Quebra-Núcleo | aberrant | Burrower, Tank, Elite | Huge | Força 8, Constituição 7 | 950 | mandíbula de escavação, placa escura |
| enemy_core_devourer_slime | Lodo Devorador de Núcleo | aberrant | Tank, Controller | Huge | Constituição 8, MP 5 | 880 | gel corrompido, núcleo semi-digerido |

### Adaptações icônicas 86-99

```text
enemy_beholder_blackstone
Grande observador ocular de Pedra Negra. É a versão boss/elite do arquétipo de beholder, adaptada ao mundo. Não copia raios oficiais. Usa feixes próprios: Blackstone Beam, Fear Glare, Slow Gaze, Arcane Pierce e Corruption Pulse.

 enemy_umber_hulk_corebreaker
Arquétipo de monstro escavador bruto adaptado. Enorme, cego parcialmente, guiado por vibração. Abre caminho por rocha quebrável e pode proteger mining chambers raras.

 enemy_core_devourer_slime
Lodo colossal que absorveu Pedra Negra e peças bromecianas. Lento, ocupa espaço, cria zonas perigosas e pode guardar tesouros digeridos.
```

---

# PARTE D — Bosses de gate e nível 101

## 17. Boss gates 15/30/45/60/75/90/100

| Gate | BossId | Nome | Size | Atributos dominantes | XP | Recompensa única sugerida |
|---:|---|---|---|---|---:|---|
| 15 | boss_blackroot_matriarch | Matriarca Raiz-Negra | Boss 128x128 | CON 7, MP 5, VON 5 | 400 | Semente de Raiz Mineral |
| 30 | boss_duergar_frost_captain | Capitão Duergar do Gelo | Boss 128x128 | FOR 6, CON 7, VON 5 | 750 | Martelo Frio Quebrado |
| 45 | boss_kaand_ember_champion | Campeão de Brasa de Kaand | Boss 128x128 | FOR 7, CON 6, BREATH 6 | 1200 | Cinza de Juramento de Kaand |
| 60 | boss_bromecian_puzzle_colossus | Colosso-Enigma Bromeciano | Boss 160x160 | CON 8, INT 6, MP 5 | 1900 | Núcleo Lógico Antigo |
| 75 | boss_moonless_drow_hierophant | Hierofante Drow Sem-Lua | Boss 128x128 | MP 8, INT 6, VON 7 | 3000 | Lanterna Sem-Lua |
| 90 | boss_blackstone_wyvern | Wyvern de Pedra Negra | Boss 160x160 | FOR 8, CON 7, DES 6 | 4800 | Escama de Pedra Negra Estabilizada |
| 100 | boss_core_oathbreaker | Quebra-Juramento do Núcleo | Boss 160x160 | FOR 7, CON 8, VON 8 | 8000 | Chave da Câmara de Anya |

## 18. Bosses do nível 101

| Ordem | BossId | Nome | Size | Função | Recompensa |
|---:|---|---|---|---|---|
| 101-A | boss_blackstone_warden_prime | Guardião Primário de Pedra Negra | Boss 160x160 | resistência/posição | Fragmento Selado I |
| 101-B | boss_elyndor_oath_construct | Constructo de Juramento de Elyndor | Boss 160x160 | mecanismo/janelas | Fragmento Selado II |
| 101-C | boss_nyx_broken_herald | Arauto Quebrado de Nyx | Boss 128x160 | sombra/verdade | Fragmento Selado III |
| 101-D | boss_draconic_core_remnant | Remanescente Dracônico do Núcleo | Boss 192x160 | combate final pesado | Núcleo de Libertação Parcial |
| 101-Final | boss_anya_bound_echo | Eco Acorrentado de Anya | Boss especial | encontro narrativo | Libertação parcial do poder de Anya |

Regra:

```text
O boss final 101-Final não deve ser tratado como monstro comum.
Ele é encontro de lore e progressão, com recompensa única e alteração de estado de mundo.
```

---

# PARTE E — Packs diversificados

## 19. Regras de composição de packs

A caverna deve parecer habitada por ecologias/facções, não por spawn aleatório.

Cada nível deve misturar:

```text
2-4 packs principais do bioma
1-3 packs secundários
0-2 elite packs
0-2 treasure trap packs
0-1 special room pack
ambient enemies isolados
```

Packs devem respeitar:

```text
bioma
faixa de nível
faction locks
room size
safe anchors
size classes
boss gate progress
snapshot identity
```

## 20. Pack templates

```text
Swarm Pack
  6-12 Tiny/Small
  baixa recompensa individual
  bom para corredores e salas médias

Hunter Pack
  3-6 Chasers/Leapers
  pressiona movimento

Guarded Resource Pack
  1-2 Tanks/Guards + 3-6 suporte
  protege mining cluster

Treasure Trap Pack
  1 mimic/sludge/ocular trap + 2-5 suporte
  protege baú/reward

Faction Patrol
  4-8 humanoides da mesma facção
  usa ranged + melee + guard

Mixed Ecology Pack
  2 factions compatíveis
  exemplo: goblin + kobold, fungal + beast, construct + gnome

Elite Anchor Pack
  1 elite + 4-8 suporte
  usado em sala grande

Special Room Pack
  composição fixa por sala especial

Boss Antechamber Pack
  2-4 elites + 8-16 suporte, mas com espaço e pacing
```

## 21. Packs 1-10

```text
pack_stone_swarm_low
  cave_mite x6-10
  stone_rat x2-4
  Uso: sala comum/corredor largo

pack_goblin_kobold_low
  goblin_grashnaar_scavenger x4-6
  kobold_scout x2-4
  Uso: patrulha inteligente inicial

pack_fungal_low
  mossling x3-5
  blackroot_sprout x3-5
  cave_mite x2-4
  Uso: sala úmida/mineração leve

pack_undead_low
  cracked_bone x4-7
  cave_bat x3-5
  Uso: sala abandonada

pack_sludge_treasure_low
  translucent_sludge_cube x1
  stone_rat x2-3
  chest_biter x0-1
  Uso: tesouro baixo ou corredor bloqueado

pack_rust_miner_low
  rust_beetle x2-4
  cave_mite x4-6
  Uso: perto de cobre/carvão
```

## 22. Packs 11-25

```text
pack_spore_grove
  spore_imp x4-7
  mossling x4-6
  nyx_moth x1-3

pack_rootsnare_patch
  rootsnare x2-4
  blackroot_sprout x4-7
  hollow_stagling x0-1

pack_urudakh_ambush
  goblin_urudakh_trapper x2-4
  thorn_archer x3-5
  goblin_grashnaar_scavenger x3-5

pack_nyx_stalkers
  orc_nyx_stalker x2-4
  nyx_moth x4-8
  panther_distorted x0-1

pack_fungal_elite
  mycobulwark x1-2
  spore_imp x4-6
  rootsnare x2-3

pack_basilisk_grove
  basilisk_lizard x1
  rootsnare x2-3
  thorn_archer x2-4

pack_root_owlbear_den
  root_owlbear x1
  hollow_stagling x1-2
  spore_imp x2-4
```

## 23. Packs 26-40

```text
pack_frost_beasts
  frost_gnawer x6-10
  glassbone x2-4

pack_duergar_patrol
  duergar_frostdelver x4-6
  duergar_shieldbreaker x1-2
  frost_gnawer x2-3

pack_frozen_guard
  icebound_sentinel x1-3
  cold_cult_acolyte x2-4
  glassbone x2-4

pack_crystal_hunt
  crystal_leaper x2-5
  frost_wailer x1-2
  frost_gnawer x3-5

pack_hook_horror_cave
  hook_horror_ice x1-2
  frost_gnawer x4-6
  cold_cult_acolyte x1-2

pack_mind_larva_nest
  mind_eater_larva x2-4
  glassbone x2-4
  frost_wailer x0-1
```

## 24. Packs 41-55

```text
pack_ember_swarm
  ember_tick x8-12
  ash_crawler x3-5

pack_kaand_warband
  orc_kaand_berserker x2-3
  orc_kaand_ashcaller x1-2
  ash_crawler x3-5

pack_lava_guard
  lava_bulwark x1-3
  cinder_spitter x3-6
  ember_tick x3-6

pack_furnace_room
  furnace_warden x1-2
  scorched_cultist x2-4
  ember_tick x6-10

pack_fire_basilisk_lair
  fire_basilisk x1
  cinder_spitter x3-5
  scorched_cultist x1-2

pack_stone_bulette_ambush
  stone_bulette x1
  ash_crawler x3-5
  lava_bulwark x0-1
```

## 25. Packs 56-70

```text
pack_rune_swarm
  rune_shard x6-10
  clockwork_guard x2-4

pack_gnome_ruin_team
  gnome_gem_madcap x2-4
  gnomorin_rune_tinker x3-5
  rune_shard x3-5

pack_sealed_hall
  sealed_knight x1-3
  oathless_shade x1-3
  clockwork_guard x2-4

pack_puzzle_guard
  puzzle_golem x1
  mirror_adept x1-2
  clockwork_guard x3-5
  rune_shard x3-5

pack_observer_ruin_minor
  beholder_kin_lesser x1
  rune_shard x4-6
  mirror_adept x1

pack_mimic_armory_room
  mimic_armory x1-2
  clockwork_guard x2-4
  gnomorin_rune_tinker x1-2

pack_memory_jelly_archive
  brain_jelly x2-3
  mirror_adept x1-2
  rune_shard x3-5
```

## 26. Packs 71-85

```text
pack_drow_patrol
  drow_shadowblade x2-4
  drow_web_scout x3-5
  abyss_wisp x4-7

pack_moon_cult
  drow_moon_caster x1-3
  black_lantern_cultist x2-4
  moonless_hound x3-5

pack_void_elite
  void_caster x1-2
  oath_eater x1-2
  abyss_wisp x5-8

pack_abyss_hunt
  moonless_hound x5-8
  drow_shadowblade x1-3

pack_mind_eater_cell
  mind_eater_adult x1-2
  abyss_wisp x4-6
  black_lantern_cultist x1-2

pack_blackmoon_eye_room
  eye_tyrant_blackmoon x1
  drow_moon_caster x1-2
  abyss_wisp x5-8
```

## 27. Packs 86-99

```text
pack_corrupted_core
  corrupt_hulk x1-2
  blackstone_cult_paragon x2-4
  core_mirror x3-5

pack_ninrorin_vision
  ninrorin_broken_oracle x1-2
  core_mirror x4-6
  blackstone_cult_paragon x1-2

pack_draconic_corruption
  corrupted_pseudodragon x4-6
  mana_warped_beast x1-3
  core_mirror x1-3

pack_blackstone_warden
  corrupt_hulk x1
  blackstone_cult_paragon x3-5
  corrupted_pseudodragon x3-5

pack_beholder_blackstone_chamber
  beholder_blackstone x1
  core_mirror x3-5
  blackstone_cult_paragon x2-4

pack_corebreaker_mining_room
  umber_hulk_corebreaker x1-2
  corrupt_hulk x1
  mana_warped_beast x2-4

pack_core_devourer_treasure
  core_devourer_slime x1
  chest_biter x1-2 scaled
  core_mirror x2-4

pack_anya_echo_event
  anya_silent_echo x1
  no normal loot
  lore trigger
```

## 28. Boss antechamber packs

```text
gate15_antechamber_blackroot
  rootsnare x4-6
  mycobulwark x1
  spore_imp x4-6

 gate30_antechamber_frost
  duergar_shieldbreaker x2
  icebound_sentinel x1-2
  cold_cult_acolyte x3-5

 gate45_antechamber_kaand
  orc_kaand_berserker x2-3
  orc_kaand_ashcaller x2
  lava_bulwark x1

 gate60_antechamber_bromecia
  puzzle_golem x1
  clockwork_guard x4-6
  mirror_adept x2

 gate75_antechamber_moonless
  drow_shadowblade x3
  drow_moon_caster x2
  oath_eater x1

 gate90_antechamber_blackstone
  blackstone_cult_paragon x3
  corrupted_pseudodragon x4
  core_mirror x4

 gate100_antechamber_core
  corrupt_hulk x1-2
  beholder_blackstone x0-1
  ninrorin_broken_oracle x1
  blackstone_cult_paragon x3-5
```

---

# PARTE F — Ataques e poderes

## 29. Ataques comuns

```text
MeleeBite
MeleeClaw
MeleeHeavySmash
ShortDash
ShortLeap
RangedStoneThrow
RangedSporeShot
RangedShardShot
RangedCinderSpit
CasterPulse
CasterZoneSmall
GuardBlock
BurrowEmerge
PhaseShortBlink
FloatingRay
FloatingOrbitRay
TreasureAmbushBite
AcidContact
PsychicPulse
EyeBeamBlackstone
```

## 30. Status permitidos na caverna

```text
Burn
Poison
Bleed
Slow
Stun
Chill
Root
Fear
ConfusionLite
DurabilityStress
VulnerabilityWindow
```

Regra:

```text
Status não devem virar DamageType novo sem spec de damage/status.
ConfusionLite não deve tirar controle total do jogador.
DurabilityStress não deve destruir item permanentemente sem spec própria.
```

## 31. Poderes de observadores oculares

Observadores adaptados de Vaalara não copiam raios oficiais.

Poderes permitidos:

```text
Blackstone Beam — dano mágico/escuro em linha reta com telegraph.
Slow Gaze — aplica Slow curto se o jogador permanecer no cone.
Fear Glare — aplica Medo leve com cooldown alto.
Arcane Pierce — projétil único rápido, evitável.
Corruption Pulse — zona circular curta ao redor do boss.
Eye Shard Volley — vários projéteis fracos com intervalo.
```

Regra:

```text
Todo poder ocular precisa de telegraph visual claro.
Nenhum poder deve matar instantaneamente.
Nenhum poder deve petrificar/controlar permanentemente.
```

## 32. Vulnerability windows

```text
AfterAttackRecover
DuringChargeWindup
AfterBurrowEmerges
AfterCast
AfterProjectileVolley
AfterShieldDrop
AfterBlinkArrival
AfterEnragePulse
AfterEyeBeam
AfterTreasureReveal
AlwaysForTest apenas debug
```

---

# PARTE G — Drops e bestiário

## 33. Categorias de drop

```text
minério
pedra
carvão
gemas
componentes de constructo
partes de criatura
reagentes fúngicos
essências elementais
fragmentos de Pedra Negra
itens de Nyx
itens de Kaand
itens bromecianos
equipamentos
blueprints
chaves de gate
fragmentos de lore
```

## 34. Regra de drop

```text
Drops comuns podem repetir.
Drops raros devem ter chance controlada.
Drops únicos de boss/gate só podem ser obtidos uma vez por save/progresso permanente.
Itens de lore não devem ser vendidos como lixo sem decisão explícita.
```

## 35. Conteúdo mínimo de bestiário

Cada entrada de bestiário deve ter:

```text
Nome
Faction
Bioma/faixa
Descrição física curta
Tamanho/SizeClass
Atributos dominantes
Comportamento observado
Ataques conhecidos
Drops descobertos
Fraquezas/resistências descobertas
KillCount
FirstSeenLevel
LoreTagline
```

## 36. Lore taglines

```text
Ácaro da Fenda: "Onde há pedra quebrada, eles chegam primeiro."
Saqueador Grash'naar: "Os Grash'naar não mineram; esperam alguém minerar por eles."
Baú-Mordente: "Alguns tesouros aprenderam a morder primeiro."
Cubo de Lodo Translúcido: "A caverna também digere. Só faz isso devagar."
Urso-Coruja Raiz-Oca: "A floresta de baixo também sonha com predadores."
Besouro Ferrugem: "Ele fareja metal como fome."
Devora-Mentes Abissal: "Não quer seu corpo. Quer o espaço entre seus pensamentos."
Observador Tirano da Pedra Negra: "Cada olho viu uma versão diferente da queda."
Wyvern de Pedra Negra: "Não nasceu da pedra. Foi convencida por ela."
Eco Silencioso de Anya: "Não ataca por ódio. Protege o que restou."
```

---

# PARTE H — Specs futuras derivadas

```text
spec_cave_monster_roster_data_expansion.md
spec_cave_enemy_packs_density_rebalance.md
spec_cave_enemy_actions_vulnerability_profiles.md
spec_cave_iconic_dungeon_archetypes_vaalara_adaptation.md
spec_cave_boss_gate_roster_rewards.md
spec_cave_level_101_boss_gauntlet_roster.md
spec_cave_bestiary_entries_lore_rewards.md
spec_cave_loot_tables_by_biome_and_faction.md
spec_cave_enemy_level_scaling_rules.md
```

---

# PARTE I — Decisões fechadas

```text
O roster base passa a incluir criaturas inspiradas em arquétipos icônicos de dungeon fantasy, adaptadas para Vaalara.
Beholder-like entra como Observador Menor da Ruína, Tirano Ocular da Lua Negra e Observador Tirano da Pedra Negra.
Mimic-like entra como Baú-Mordente e Arsenal-Mordente.
Gelatinous cube-like entra como Cubo de Lodo Translúcido e Lodo Devorador de Núcleo.
Mind flayer-like entra como Larva Devora-Mentes e Devora-Mentes Abissal.
Rust monster-like entra como Besouro Ferrugem.
Displacer beast-like entra como Pantera Distorcida.
Owlbear-like entra como Urso-Coruja Raiz-Oca.
Bulette-like entra como Tubarão de Pedra.
Umber hulk-like entra como Titã Escavador Quebra-Núcleo.
Cada criatura deve ter atributos dominantes, size, comportamento, ataques, scaling, XP, drops e bestiary.
Packs passam a ser mais diversos, maiores e mais ecológicos.
Scaling por nível deve permitir variantes veteranas, elites, corrompidas, lunares, bromecianas, ígneas, gélidas e Raiz-Negra.
```

---

# PARTE J — Pendências

```text
Converter estes dados para EnemyDataSO.
Converter ataques para EnemyActionSO.
Converter packs para EnemySpawnPackSO.
Converter drops para LootTableSO.
Criar EnemyBestiaryEntrySO para cada criatura.
Balancear XP real contra progressão do jogador.
Validar performance com nova densidade.
Validar se materialização total ou active budget por proximidade será necessário.
Definir sprites finais.
Definir bosses finais em código e dados.
Definir quais poderes exigem MP de inimigo.
Definir se DurabilityStress entra antes ou depois do sistema completo de durabilidade.
