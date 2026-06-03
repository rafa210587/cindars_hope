# Cindar's Hope — Cave Monster Roster Direction

> **Status:** documento canônico de roster, descrição e direção de criaturas da caverna  
> **Local:** `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md`  
> - `docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> **Função:** detalhar criaturas, bosses, packs, comportamento, ataques, XP, recursos, tesouros e função de gameplay.  
> **Não é spec implementável.** Specs futuras devem converter estes dados em `EnemyDataSO`, `EnemyActionSO`, `EnemySpawnProfileSO`, `LootTableSO` e bestiary entries.

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

Regra principal:

```text
Não copiar statblocks, textos ou nomes proprietários de D&D.
Usar fantasia tabletop como inspiração estrutural, mas adaptar para Vaalara e para os sistemas do jogo.
```

---

# PARTE A — Contratos de design

## 1. Campos conceituais de cada criatura

Cada criatura deve ter:

```text
EnemyId
Nome PT-BR
Faixa de níveis
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
XP base
Drops/recursos
Tesouros raros
Função de gameplay
Bestiary lore curta
```

## 2. Escala de XP conceitual

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

# PARTE B — Factions e funções

## 3. Factions oficiais da caverna

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
```

## 4. Roles oficiais

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
```

## 5. Movement profiles oficiais

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
```

## 6. Size classes oficiais

```text
Tiny
Small
Medium
Large
Huge
Boss
```

---

# PARTE C — Roster comum e elite

## 7. Níveis 1-10 — Caverna de Pedra

| EnemyId | Nome | Faction | Roles | Size | XP | Drops principais |
|---|---|---|---|---|---:|---|
| enemy_cave_mite | Ácaro da Fenda | beast | Swarm, Chaser | Tiny | 8 | quitina pequena, pedra miúda |
| enemy_stone_rat | Rato de Basalto | beast | Chaser | Small | 10 | pele áspera, carvão baixo |
| enemy_cave_bat | Morcego de Fenda | beast | Swarm, Ranged | Tiny | 12 | asa fina, eco mineral |
| enemy_goblin_grashnaar_scavenger | Saqueador Grash'naar | goblin | Chaser, Ranged | Small | 16 | sucata, cobre baixo, moeda |
| enemy_kobold_scout | Batedor Kobold | kobold | Chaser, Ranged | Small | 16 | garra, pedra polida |
| enemy_mossling | Musguinho Errante | fungal | Guard, Tank | Small | 14 | esporo verde, fibra úmida |
| enemy_cracked_bone | Osso Rachado | undead | Chaser | Medium | 18 | osso seco, pó mineral |
| enemy_blackroot_sprout | Broto Raiz-Negra | fungal | Ranged, Guard | Small | 18 | raiz negra fraca, seiva escura |

### Descrições rápidas

```text
enemy_cave_mite
Pequeno inseto mineralizado. Anda em grupo, cerca o jogador e morre rápido. Serve para ensinar swarm e controle de área.

 enemy_stone_rat
Rato de pedra com dorso basáltico. Corre em linha curta, recua pouco e pressiona corredores.

 enemy_cave_bat
Morcego pequeno que solta ondas curtas de eco. Não voa mecanicamente no MVP; usa movimento errático no chão/baixa altura visual.

 enemy_goblin_grashnaar_scavenger
Goblin saqueador dos túneis baixos. Alterna investida curta com arremesso simples.

 enemy_kobold_scout
Kobold cauteloso ligado a túneis dracônicos antigos. Mantém distância e tenta puxar o jogador para packs.

 enemy_mossling
Criatura fúngica defensiva. Lenta, resistente, protege recursos simples.

 enemy_cracked_bone
Morto-vivo simples, restos animados por energia subterrânea. Pressiona em linha reta e ensina dano físico.

 enemy_blackroot_sprout
Broto fixo de raiz escura. Ataca à distância curta e protege salas de fungos.
```

### Packs 1-10

```text
pack_stone_swarm_low: cave_mite x5-8 + stone_rat x1-2
pack_goblin_kobold_low: goblin_grashnaar_scavenger x3-5 + kobold_scout x1-2
pack_fungal_low: mossling x2-3 + blackroot_sprout x2-3
pack_undead_low: cracked_bone x3-5 + cave_bat x2-4
```

---

## 8. Níveis 11-25 — Floresta Subterrânea

| EnemyId | Nome | Faction | Roles | Size | XP | Drops principais |
|---|---|---|---|---|---:|---|
| enemy_spore_imp | Diabrete de Esporo | fungal | Caster, Swarm | Small | 22 | esporo irritante, fungo arcano |
| enemy_rootsnare | Garra-Raiz | fungal | Guard, Burrower | Medium | 30 | fibra de raiz, seiva grossa |
| enemy_hollow_stagling | Cervino Oco | beast | Chaser, Elite | Medium | 55 | chifre oco, couro sombrio |
| enemy_goblin_urudakh_trapper | Armeiro Uru'dakh | goblin | Ranged, Guard | Small | 28 | armadilha simples, sucata |
| enemy_thorn_archer | Espinhador Sombrio | goblin | Ranged | Medium | 32 | espinho escuro, madeira dura |
| enemy_orc_nyx_stalker | Espreitador Orc de Nyx | orc | Chaser, Burrower | Medium | 48 | osso ritual, couro negro |
| enemy_mycobulwark | Baluarte Micélio | fungal | Tank, Guard | Large | 75 | placa fúngica, esporo raro |
| enemy_nyx_moth | Mariposa de Nyx | abyssal | Caster, Swarm | Small | 38 | pó lunar escuro, asa macia |

### Comportamentos e ataques

```text
enemy_spore_imp
Atira nuvem de esporos curta. Pode aplicar Slow/Poison leve. Vulnerável depois de conjurar.

 enemy_rootsnare
Fica semi-enterrado e emerge quando o jogador se aproxima. Pode aplicar Root curto. Vulnerável após emergir.

 enemy_hollow_stagling
Avança em saltos curtos. Elite de mobilidade. Vulnerável após errar investida.

 enemy_goblin_urudakh_trapper
Prepara zonas pequenas de armadilha. Mantém distância. Bom para salas largas.

 enemy_thorn_archer
Atirador de espinhos. Kiting simples. Fraco se encurralado.

 enemy_orc_nyx_stalker
Orc silencioso ligado a Nyx. Usa emboscada e aproximação lateral. Mais agressivo em salas escuras.

 enemy_mycobulwark
Tank fúngico grande. Protege outros fungos e bloqueia passagem. Fraco depois de ataque pesado.

 enemy_nyx_moth
Criatura associada à lua oculta. Ataca com pulsos curtos e confusão visual leve. Sem controle mental pesado no MVP.
```

### Packs 11-25

```text
pack_spore_grove: spore_imp x4-6 + mossling x3-5
pack_rootsnare_patch: rootsnare x2-3 + blackroot_sprout x3-5
pack_urudakh_ambush: goblin_urudakh_trapper x2-3 + thorn_archer x2-4 + goblin_grashnaar_scavenger x2-4
pack_nyx_stalkers: orc_nyx_stalker x1-3 + nyx_moth x3-6
pack_fungal_elite: mycobulwark x1-2 + spore_imp x3-5 + rootsnare x1-2
```

---

## 9. Níveis 26-40 — Caverna de Gelo

| EnemyId | Nome | Faction | Roles | Size | XP | Drops principais |
|---|---|---|---|---|---:|---|
| enemy_frost_gnawer | Roedor de Geada | beast | Chaser | Small | 36 | presa fria, cristal baixo |
| enemy_duergar_frostdelver | Escavador Duergar do Gelo | duergar | Guard, Tank | Medium | 48 | minério frio, ferramenta quebrada |
| enemy_duergar_shieldbreaker | Quebra-Escudo Duergar | duergar | Tank, Elite | Large | 105 | fragmento de escudo, ferro frio |
| enemy_icebound_sentinel | Sentinela Enregelado | construct | Guard, Tank | Large | 95 | placa congelada, núcleo fraco |
| enemy_glassbone | Osso de Vidro | undead | Ranged, Chaser | Medium | 52 | osso cristalino, pó de gelo |
| enemy_cold_cult_acolyte | Acólito do Frio | cultist | Caster | Medium | 58 | tecido frio, símbolo quebrado |
| enemy_crystal_leaper | Saltador Cristalino | elemental | Chaser, Elite | Medium | 110 | lasca cristalina, gema fria |
| enemy_frost_wailer | Lamento Frio | undead | Caster, Elite | Medium | 125 | eco gelado, essência fria |

### Comportamentos e ataques

```text
enemy_frost_gnawer
Pressiona em grupo. Pode aplicar Chill leve por contato.

 enemy_duergar_frostdelver
Patrulha salas de mineração. Usa ataque curto com ferramenta pesada.

 enemy_duergar_shieldbreaker
Elite lento, resistente, quebra postura. Vulnerável após golpe pesado.

 enemy_icebound_sentinel
Constructo guardião. Fica parado até aproximação. Bom para proteger baús/minérios.

 enemy_glassbone
Morto-vivo cristalino. Atira lascas frágeis e se aproxima quando isolado.

 enemy_cold_cult_acolyte
Conjurador humanoide. Usa zona fria pequena e recua.

 enemy_crystal_leaper
Salta em linha curta. Pode punir jogador parado.

 enemy_frost_wailer
Elite caster. Usa pulso frio e invoca pressão de área curta.
```

### Packs 26-40

```text
pack_frost_beasts: frost_gnawer x5-8 + glassbone x1-3
pack_duergar_patrol: duergar_frostdelver x3-5 + duergar_shieldbreaker x1-2
pack_frozen_guard: icebound_sentinel x1-2 + cold_cult_acolyte x2-3
pack_crystal_hunt: crystal_leaper x2-4 + frost_wailer x1 + frost_gnawer x2-4
```

---

## 10. Níveis 41-55 — Caverna de Fogo

| EnemyId | Nome | Faction | Roles | Size | XP | Drops principais |
|---|---|---|---|---|---:|---|
| enemy_ember_tick | Carrapato de Brasa | beast | Swarm, Chaser | Tiny | 62 | brasa pequena, quitina quente |
| enemy_ash_crawler | Rastejante de Cinza | elemental | Chaser | Medium | 76 | cinza mineral, carvão alto |
| enemy_orc_kaand_berserker | Berserker Orc de Kaand | orc | Chaser, Elite | Large | 170 | couro queimado, símbolo de Kaand |
| enemy_orc_kaand_ashcaller | Chamador de Cinzas de Kaand | orc | Caster, Ranged | Medium | 135 | cinza ritual, osso marcado |
| enemy_lava_bulwark | Baluarte de Lava | elemental | Tank, Guard | Large | 150 | pedra ígnea, núcleo quente |
| enemy_cinder_spitter | Cuspidor de Cinza | beast | Ranged | Medium | 92 | glândula de cinza, brasa |
| enemy_scorched_cultist | Cultista Chamuscado | cultist | Caster | Medium | 110 | pano chamuscado, reagente ígneo |
| enemy_furnace_warden | Guardião da Fornalha | construct | Guard, Elite | Large | 210 | peça de fornalha, engrenagem quente |

### Comportamentos e ataques

```text
enemy_ember_tick
Enxame rápido. Explode visualmente em brasa baixa sem dano massivo. Serve para pressão.

 enemy_ash_crawler
Avança e deixa cinza curta no chão. Pode aplicar Burn leve.

 enemy_orc_kaand_berserker
Elite agressivo. Entra em fúria curta quando com HP baixo.

 enemy_orc_kaand_ashcaller
Caster de suporte. Aplica área de brasa e fortalece orcs próximos.

 enemy_lava_bulwark
Tank elemental. Lento, bloqueia passagem, resistente a fogo.

 enemy_cinder_spitter
Atirador de cinza. Ataca em arcos curtos e recua.

 enemy_scorched_cultist
Conjurador instável. Usa fogo e status Burn.

 enemy_furnace_warden
Constructo de elite. Guarda salas de forja antiga e tesouros.
```

### Packs 41-55

```text
pack_ember_swarm: ember_tick x6-10 + ash_crawler x2-4
pack_kaand_warband: orc_kaand_berserker x1-2 + orc_kaand_ashcaller x1-2 + ash_crawler x2-4
pack_lava_guard: lava_bulwark x1-3 + cinder_spitter x2-4
pack_furnace_room: furnace_warden x1-2 + scorched_cultist x2-3 + ember_tick x4-6
```

---

## 11. Níveis 56-70 — Ruínas Antigas

| EnemyId | Nome | Faction | Roles | Size | XP | Drops principais |
|---|---|---|---|---|---:|---|
| enemy_rune_shard | Lasca Rúnica | construct | Swarm, Ranged | Small | 98 | lasca rúnica, pó arcano |
| enemy_clockwork_guard | Guarda de Corda | construct | Guard, Tank | Medium | 130 | engrenagem, mola antiga |
| enemy_gnome_gem_madcap | Gnomo de Gema Enlouquecido | gnome | Caster, Ranged | Small | 145 | gema trincada, ferramenta fina |
| enemy_gnomorin_rune_tinker | Gnomorin Runa-Torta | gnome | Ranged, Guard | Small | 150 | peça bromeciana, runa falha |
| enemy_sealed_knight | Cavaleiro Selado | undead | Tank, Elite | Large | 280 | placa antiga, juramento quebrado |
| enemy_mirror_adept | Adepto do Espelho | cultist | Caster, Elite | Medium | 260 | fragmento refletivo, símbolo oculto |
| enemy_puzzle_golem | Golem de Enigma | construct | Tank, Guard | Large | 320 | núcleo lógico, pedra trabalhada |
| enemy_oathless_shade | Sombra Sem-Juramento | undead | Caster, Elite | Medium | 300 | eco de juramento, tecido antigo |

### Comportamentos e ataques

```text
enemy_rune_shard
Pequeno constructo quebrado. Dispara energia curta em grupo.

 enemy_clockwork_guard
Guarda patrulheiro. Movimento previsível, resistente, bom para corredores largos.

 enemy_gnome_gem_madcap
Gnomo afetado por gemas instáveis. Atira projéteis irregulares.

 enemy_gnomorin_rune_tinker
Gnomorin técnico corrompido. Usa armadilhas rúnicas e recua.

 enemy_sealed_knight
Guardião antigo. Lento, forte, vulnerável após ataque pesado.

 enemy_mirror_adept
Caster de elite. Usa blink curto e ilusões simples de posição.

 enemy_puzzle_golem
Tank de sala especial. Pode exigir janela de vulnerabilidade clara.

 enemy_oathless_shade
Sombra ligada a juramentos quebrados. Usa fase curta e ataque arcano.
```

### Packs 56-70

```text
pack_rune_swarm: rune_shard x5-8 + clockwork_guard x1-3
pack_gnome_ruin_team: gnome_gem_madcap x2-3 + gnomorin_rune_tinker x2-3 + rune_shard x2-4
pack_sealed_hall: sealed_knight x1-2 + oathless_shade x1-2
pack_puzzle_guard: puzzle_golem x1 + mirror_adept x1-2 + clockwork_guard x2-4
```

---

## 12. Níveis 71-85 — Abismo Sombrio

| EnemyId | Nome | Faction | Roles | Size | XP | Drops principais |
|---|---|---|---|---|---:|---|
| enemy_drow_shadowblade | Lâmina Sombria Drow | drow | Chaser, Elite | Medium | 390 | lâmina escura, tecido drow |
| enemy_drow_moon_caster | Conjurador Lunar Drow | drow | Caster, Elite | Medium | 430 | pó lunar, foco lunar |
| enemy_drow_web_scout | Batedor de Teia Drow | drow | Ranged, Guard | Medium | 360 | fio escuro, seta fina |
| enemy_void_caster | Conjurador do Vazio | abyssal | Caster, Elite | Medium | 520 | essência vazia, fragmento escuro |
| enemy_abyss_wisp | Fagulha do Abismo | abyssal | Swarm, Caster | Small | 240 | eco sombrio, partícula lunar |
| enemy_moonless_hound | Cão Sem-Lua | beast | Chaser, Leaper | Medium | 310 | presa escura, couro frio |
| enemy_black_lantern_cultist | Cultista da Lanterna Negra | cultist | Caster, Guard | Medium | 340 | lanterna quebrada, óleo escuro |
| enemy_oath_eater | Devorador de Juramento | abyssal | Tank, Elite | Large | 560 | fragmento de voto, essência abissal |

### Comportamentos e ataques

```text
enemy_drow_shadowblade
Atacante rápido com fase curta. Pressiona flancos.

 enemy_drow_moon_caster
Caster lunar. Usa área escura e recua.

 enemy_drow_web_scout
Ranged/controlador. Aplica Slow curto com fio escuro.

 enemy_void_caster
Elite caster. Janela de vulnerabilidade após conjuração.

 enemy_abyss_wisp
Pequeno inimigo mágico em grupo. Serve para encher salas escuras sem virar boss.

 enemy_moonless_hound
Criatura de perseguição. Salta e tenta isolar jogador.

 enemy_black_lantern_cultist
Guardião de pontos de lore de Nyx. Usa luz escura como aviso visual.

 enemy_oath_eater
Elite grande. Lento, resistente, ataca em pulsos curtos.
```

### Packs 71-85

```text
pack_drow_patrol: drow_shadowblade x2-3 + drow_web_scout x2-4 + abyss_wisp x3-5
pack_moon_cult: drow_moon_caster x1-2 + black_lantern_cultist x2-3 + moonless_hound x2-4
pack_void_elite: void_caster x1-2 + oath_eater x1 + abyss_wisp x4-6
pack_abyss_hunt: moonless_hound x4-6 + drow_shadowblade x1-2
```

---

## 13. Níveis 86-99 — Núcleo Corrompido

| EnemyId | Nome | Faction | Roles | Size | XP | Drops principais |
|---|---|---|---|---|---:|---|
| enemy_corrupt_hulk | Massa Corrompida | corrupted | Tank, Chaser | Huge | 720 | carne mineral, essência corrompida |
| enemy_ninrorin_broken_oracle | Oráculo Ninrorin Quebrado | ninrorin | Caster, Elite | Medium | 760 | fragmento profético, tecido antigo |
| enemy_corrupted_pseudodragon | Pseudodragão Corrompido | draconic | Caster, Ranged | Small | 620 | escama pequena, sopro instável |
| enemy_blackstone_wyvern | Wyvern de Pedra Negra | draconic | Boss, Elite | Boss | boss/miniboss | escama negra, núcleo dracônico |
| enemy_blackstone_cult_paragon | Paragon da Pedra Negra | cultist | Caster, Elite | Medium | 680 | símbolo negro, fragmento instável |
| enemy_core_mirror | Reflexo do Núcleo | corrupted | Phase, Caster | Medium | 610 | vidro escuro, eco arcano |
| enemy_mana_warped_beast | Fera Distorcida por Mana | beast | Chaser, Elite | Large | 650 | pelo arcano, carne instável |
| enemy_anya_silent_echo | Eco Silencioso de Anya | corrupted | Guard, Caster | Medium | 0 ou especial | fragmento de lore, não farmável |

### Comportamentos e ataques

```text
enemy_corrupt_hulk
Huge tank. Só deve nascer em salas grandes. Pressiona lentamente com dano alto.

 enemy_ninrorin_broken_oracle
Caster de alto tier. Usa previsões quebradas como telegraph visual. Vulnerável após cast.

 enemy_corrupted_pseudodragon
Pequeno dracônico corrompido. Ataca à distância e tenta manter espaço.

 enemy_blackstone_wyvern
Dracônico boss/miniboss. Deve ser usado com arena adequada, não em corredor comum.

 enemy_blackstone_cult_paragon
Cultista avançado da Pedra Negra. Suporte e dano mágico.

 enemy_core_mirror
Reflexo corrompido. Usa blink curto e projéteis simples.

 enemy_mana_warped_beast
Fera alterada por Mana instável. Rápida, forte e imprevisível.

 enemy_anya_silent_echo
Não é inimigo comum farmável. Deve funcionar como encontro de lore, teste ou proteção fragmentada. Evitar matar/recompensar como monstro comum.
```

### Packs 86-99

```text
pack_corrupted_core: corrupt_hulk x1-2 + blackstone_cult_paragon x2-3 + core_mirror x2-4
pack_ninrorin_vision: ninrorin_broken_oracle x1-2 + core_mirror x3-5
pack_draconic_corruption: corrupted_pseudodragon x3-5 + mana_warped_beast x1-2
pack_blackstone_warden: corrupt_hulk x1 + blackstone_cult_paragon x2 + corrupted_pseudodragon x2-4
pack_anya_echo_event: anya_silent_echo x1 + no normal loot + lore trigger
```

---

# PARTE D — Bosses de gate

## 14. Boss gates 15/30/45/60/75/90/100

| Gate | BossId | Nome | Função | XP | Recompensa única sugerida |
|---:|---|---|---|---:|---|
| 15 | boss_blackroot_matriarch | Matriarca Raiz-Negra | fecha floresta subterrânea inicial | 400 | Semente de Raiz Mineral |
| 30 | boss_duergar_frost_captain | Capitão Duergar do Gelo | chefe de patrulha congelada | 750 | Martelo Frio Quebrado |
| 45 | boss_kaand_ember_champion | Campeão de Brasa de Kaand | prova agressiva do fogo | 1200 | Cinza de Juramento de Kaand |
| 60 | boss_bromecian_puzzle_colossus | Colosso-Enigma Bromeciano | guardião de ruínas | 1900 | Núcleo Lógico Antigo |
| 75 | boss_moonless_drow_hierophant | Hierofante Drow Sem-Lua | marco de Nyx/abismo | 3000 | Lanterna Sem-Lua |
| 90 | boss_blackstone_wyvern | Wyvern de Pedra Negra | início do núcleo final | 4800 | Escama de Pedra Negra Estabilizada |
| 100 | boss_core_oathbreaker | Quebra-Juramento do Núcleo | gate final para nível 101 | 8000 | Chave da Câmara de Anya |

## 15. Bosses do nível 101

| Ordem | BossId | Nome | Função | Recompensa |
|---:|---|---|---|---|
| 101-A | boss_blackstone_warden_prime | Guardião Primário de Pedra Negra | teste de resistência | Fragmento Selado I |
| 101-B | boss_elyndor_oath_construct | Constructo de Juramento de Elyndor | teste de mecanismo/lore | Fragmento Selado II |
| 101-C | boss_nyx_broken_herald | Arauto Quebrado de Nyx | teste de sombra/verdade parcial | Fragmento Selado III |
| 101-D | boss_draconic_core_remnant | Remanescente Dracônico do Núcleo | teste final de combate | Núcleo de Libertação Parcial |
| 101-Final | boss_anya_bound_echo | Eco Acorrentado de Anya | encontro final narrativo, não farmável | Libertação parcial do poder de Anya |

Regra:

```text
O boss final 101-Final não deve ser tratado como monstro comum.
Ele é encontro de lore e progressão, com recompensa única e alteração de estado de mundo.
```

---

# PARTE E — Ataques e poderes por categoria

## 16. Ataques comuns

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
SummonMinorPack futuro/limitado
```

## 17. Status permitidos na caverna

```text
Burn
Poison
Bleed
Slow
Stun
Chill
Root
VulnerabilityWindow
```

Regra:

```text
Status não devem virar DamageType novo sem spec de damage/status.
```

## 18. Vulnerability windows

Usar modos existentes:

```text
AfterAttackRecover
DuringChargeWindup
AfterBurrowEmerges
AfterCast
AfterProjectileVolley
AfterShieldDrop
AfterBlinkArrival
AfterEnragePulse
AlwaysForTest apenas debug
```

---

# PARTE F — Drops e tesouros

## 19. Categorias de drop

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

## 20. Regra de drop

```text
Drops comuns podem repetir.
Drops raros devem ter chance controlada.
Drops únicos de boss/gate só podem ser obtidos uma vez por save/progresso permanente.
Itens de lore não devem ser vendidos como lixo sem decisão explícita.
```

---

# PARTE G — Bestiary entries

## 21. Conteúdo mínimo de bestiário

Cada entrada de bestiário deve ter:

```text
Nome
Faction
Bioma/faixa
Descrição física curta
Comportamento observado
Drops descobertos
Fraquezas/resistências descobertas
KillCount
FirstSeenLevel
LoreTagline
```

## 22. Exemplos de lore tagline

```text
Ácaro da Fenda: "Onde há pedra quebrada, eles chegam primeiro."
Saqueador Grash'naar: "Os Grash'naar não mineram; eles esperam alguém minerar por eles."
Espreitador Orc de Nyx: "Não ora em voz alta, mas carrega a noite na pele."
Capitão Duergar do Gelo: "Patrulha túneis que já não pertencem a ninguém."
Guardião da Fornalha: "Ainda protege uma forja que esqueceu seu mestre."
Gnomorin Runa-Torta: "A máquina que tentou consertar também o quebrou."
Wyvern de Pedra Negra: "Não nasceu da pedra. Foi convencida por ela."
Eco Silencioso de Anya: "Não ataca por ódio. Protege o que restou."
```

---

# PARTE H — Specs futuras derivadas

```text
spec_cave_monster_roster_data_expansion.md
spec_cave_enemy_packs_density_rebalance.md
spec_cave_enemy_actions_vulnerability_profiles.md
spec_cave_boss_gate_roster_rewards.md
spec_cave_level_101_boss_gauntlet_roster.md
spec_cave_bestiary_entries_lore_rewards.md
spec_cave_loot_tables_by_biome_and_faction.md
```

---

# PARTE I — Decisões fechadas

```text
O roster base deixa de ser só 40 inimigos e passa a ter 48+ criaturas, incluindo high tier.
Níveis 71-99 deixam de ser apenas hooks vagos e passam a ter direção de criaturas própria.
Nível 100 tem boss gate final.
Nível 101 tem bosses próprios e encontro final de Anya.
Cada criatura precisa de descrição física, comportamento, ataques, XP, drops e função de gameplay.
Bosses têm recompensas únicas controladas por save.
O Eco Silencioso de Anya e o encontro final de Anya não devem ser farmáveis como monstros comuns.
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
