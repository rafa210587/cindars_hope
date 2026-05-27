# refinamento_spec13_bestiary_roster_enemy_data

> Status: Refinamento detalhado a implementar
> Spec relacionada: `docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md`
> Ordem de execucao sugerida: 13
> Tipo: Enemy Design / Bestiary / Data-driven Runtime / Cave Ecology
> Objetivo: substituir/refinar o roster generico da SPEC 13 por um bestiario mais canonico de Vaalara, misturando criaturas classicas de fantasia com povos, subgrupos e criaturas proprias do mundo, organizadas por ecossistemas subterraneos, grupos que coexistem na cave, tamanho, movimento, vulnerabilidades, status, roles e progressao por faixas de nivel.

---

## 1. Contexto

A SPEC 13 atual ja define uma base correta para o runtime de inimigos:

- `EnemyDataSO` como contrato principal;
- roles oficiais;
- movement profiles;
- factions;
- bestiary persistente;
- spawn resolver por cave band, faction, biome, environment e boss gate progress;
- vulnerabilidades e telegraph;
- tamanho visual/collider/pathing;
- roster minimo de 40 inimigos.

O problema principal nao e a arquitetura. O problema esta no roster: ele esta funcional, mas generico demais e pouco conectado a Vaalara.

Este refinamento ajusta o bestiario para incluir:

- criaturas naturais de caverna;
- goblins de Vaalara;
- kobolds e draconicos menores;
- orcs e meio-orcs de clãs/linhagens ligados a Kaand, Nyx e Chama Viva;
- duergar/anões profundos;
- drows subterraneos;
- gnomos, gnomorin e gnomos das gemas corrompidos;
- elfos/Ninrorin perdidos ou quebrados;
- mortos-vivos;
- elementais;
- constructs runicos;
- cultistas e criaturas tocadas por Pedra Negra;
- pseudodragões, drakes, wyverns e abominacoes nos niveis altos.

Nao copiar statblocks, textos, nomes proprietarios ou habilidades exatas de D&D. Usar apenas arquetipos de fantasia amplamente reconheciveis, adaptados para Vaalara.

---

## 2. Decisoes de design

### 2.1 Bestiary deve ser de Vaalara

A cave nao deve parecer uma lista aleatoria de monstros. Ela deve parecer um ecossistema subterraneo de Vaalara.

Cada inimigo deve pertencer a pelo menos um destes eixos:

```text
Natural/Subterraneo
Vaalara povo/subraca/faccao
Pedra Negra/corrupcao
Deuses/luas/energia planar
Ruinas antigas/constructs/runas
Draconico profundo
Boss gate/cave progression
```

### 2.2 D&D-like, mas sem copiar D&D

Permitido:

- goblin saqueador;
- kobold batedor;
- orc berserker;
- duergar sentinela;
- drow lâmina sombria;
- gnomo artifice enlouquecido;
- pseudodragao corrompido;
- wyvern de pedra negra;
- slime, morcego, aranha, morto-vivo, elemental, construct.

Proibido:

- copiar statblock;
- copiar texto de habilidades;
- copiar nomes especificos de criaturas protegidas;
- usar balance numerico de livros como fonte.

### 2.3 Nao colocar tudo no mesmo nivel

A cave tem progressao por faixas. O roster deve se distribuir por bandas de profundidade, bioma e gates.

Modelo macro:

```text
1-10: Caverna de pedra / tutorial subterraneo
11-25: Floresta subterranea / fungos / goblins / Nyx menor
26-40: Gelo / duergar / mortos-vivos frios / constructs gelidos
41-55: Fogo / Kaand / elementais / fornalhas antigas
56-70: Ruinas antigas / gnomos / gnomorin / constructs / sombras de juramento
71-85: Abismo sombrio / drows / Nyx / undead fortes / cultistas profundos
86-99: Nucleo corrompido / Ninrorin quebrados / draconicos / Pedra Negra
100: boss/lore futura fora do MVP desta spec
```

### 2.4 Grupos de coexistencia

Inimigos nao aparecem isolados de forma aleatoria. Eles devem formar grupos que fazem sentido.

Exemplos:

```text
Pack Goblin: batedor + trapper + arqueiro + brute pequeno
Ninho Kobold: scout + thrower + drake-tender + pseudodragon corrupto futuro
Colonia Fungica: mossling + spore imp + mycobulwark + rootsnare
Patrulha Duergar: delver + shield guard + rune miner + frost sentry
Corte Drow: shadowblade + moon caster + web scout + oathless shade
Ruina Gnomorin: rune tinker + gem madcap + puzzle golem + archive wisp
Culto da Pedra Negra: acolyte + scorched cultist + meteor spawn + corrupt hulk
Covil Draconico: kobold elder + deep drake + pseudodragon corrupted + blackstone wyvern
```

### 2.5 Tamanho importa

Cada inimigo precisa ter `EnemySizeProfileSO` com impacto real em:

- escala visual;
- collider;
- area ocupada/footprint;
- pathing radius;
- targeting offset;
- damage number offset;
- knockback multiplier;
- restricao de spawn por tamanho de sala.

Classes:

```text
Tiny: insetos, pequenos enxames, criaturas fragilimas
Small: goblins, kobolds, pequenos fungos, pequenos wisps
Medium: humanoides, drows, orcs menores, cultistas, esqueletos
Large: orcs grandes, duergar elite, sentinelas, beasts grandes
Huge: hulks, drakes grandes, constructs pesados
Boss: colossos, wyverns boss, guardioes de gate
```

### 2.6 Movimento precisa variar

Usar movement profiles existentes/previstos:

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

`Flying` continua fora do MVP.

Criaturas com asas ou levitacao usam, por enquanto:

```text
SwarmErratic
CasterKeepAway
PhaseShortBlink
```

sem implementar voo real.

### 2.7 Vulnerabilidades nao podem ser iguais para todos

Todo monstro precisa ter vulnerability window. A janela deve combinar com seu comportamento:

```text
AfterAttackRecover: melee/tank/guard
DuringChargeWindup: chaser/leaper/brute
AfterBurrowEmerges: burrower
AfterCast: caster
AfterProjectileVolley: ranged
AfterShieldDrop: guard/tank/duergar/construct
AfterBlinkArrival: phase/drow/ninrorin
AfterEnragePulse: corrupted/abyssal
AlwaysForTest: somente debug
```

---

## 3. Factions revisadas

Factions tecnicas recomendadas:

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

Observacoes:

- `faction_orc` pode carregar tags internas: `kaand`, `nyx`, `chama_viva`.
- `faction_goblin` pode carregar tags internas: `grashnaar`, `urudakh`, `zhakthul`.
- `faction_gnome` inclui gnomos, gnomorin e gnomos das gemas quando hostis/corrompidos.
- `faction_ninrorin` deve aparecer apenas nos niveis altos e/ou em eventos especificos.
- `faction_draconic` entra em tiers medios-altos e altos.

---

## 4. Grupos de coexistencia por faixa

### 4.1 Niveis 1-10 — Caverna de pedra / tutorial

Objetivo: ensinar movimento, ataque basico, ranged simples, enxame, armadilha leve e resistencia basica.

Grupos que coexistem:

```text
Pequena fauna subterranea:
- acaros
- ratos
- morcegos

Primeiros saqueadores:
- goblin grashnaar scavenger
- kobold scout

Fungos fracos:
- mossling
- blackroot sprout
```

Nao usar ainda:

- drow;
- duergar forte;
- orc elite;
- drakes/wyverns;
- Ninrorin.

### 4.2 Niveis 11-25 — Floresta subterranea

Objetivo: introduzir ecologia organica, veneno, slow, armadilhas, goblins organizados e primeiro orc de Nyx.

Grupos que coexistem:

```text
Colonia fungica:
- spore imp
- rootsnare
- mycobulwark

Goblins de toca e armadilha:
- urudakh trapper
- thorn archer

Predadores naturais:
- hollow stagling
- burrow beetle

Nyx menor:
- nyx moth
- orc nyx stalker
```

### 4.3 Niveis 26-40 — Gelo

Objetivo: controle, slow/chill, inimigos defensivos e primeira pressao duergar.

Grupos que coexistem:

```text
Fauna congelada:
- frost gnawer
- hoarfang

Duergar/anões profundos:
- duergar frostdelver
- duergar shieldbreaker

Constructs gelidos:
- icebound sentinel

Undead frios:
- glassbone
- frost wailer

Culto do frio:
- cold cult acolyte
```

### 4.4 Niveis 41-55 — Fogo

Objetivo: dano por fogo/burn, area denial, inimigos agressivos e orcs de Kaand.

Grupos que coexistem:

```text
Fauna de brasa:
- ember tick
- ash crawler
- cinder spitter

Orcs de Kaand:
- orc kaand berserker
- orc kaand ashcaller

Elementais/fornalha:
- lava bulwark
- furnace warden

Cultistas queimados:
- scorched cultist
- flame imp
```

### 4.5 Niveis 56-70 — Ruinas antigas

Objetivo: constructs, runas, puzzles simples, gnomos/gnomorin corrompidos e inimigos com blink curto.

Grupos que coexistem:

```text
Ruinas runicas:
- rune shard
- clockwork guard
- puzzle golem

Gnomos e gnomorin:
- gnome gem madcap
- gnomorin rune tinker

Mortos-vivos juramentados:
- sealed knight
- oathless shade

Saqueadores de reliquia:
- relic thief
- archive wisp
```

### 4.6 Niveis 71-85 — Abismo sombrio

Objetivo: drows, sombras, Nyx, casters fortes, elite, ambush e pressure.

Grupos que coexistem:

```text
Corte drow subterranea:
- drow shadowblade
- drow moon caster
- drow web scout

Nyx/abismo:
- nyxling pack
- void caster

Undead/eco:
- anya echo
- oathless shade upgraded variant ou reuso com tier alto

Corrupcao inicial:
- black meteor spawn
```

### 4.7 Niveis 86-99 — Nucleo corrompido

Objetivo: high tier, Ninrorin quebrados, draconicos, Pedra Negra, elites e hooks de boss.

Grupos que coexistem:

```text
Ninrorin quebrados:
- ninrorin broken oracle
- ninrorin planar echo

Draconicos profundos:
- corrupted pseudodragon
- deep drake
- blackstone wyvern

Pedra Negra/corrupcao:
- corrupt hulk
- meteorborn abomination
- black meteor spawn elite variant

Gate guardians:
- gate colossus variants
```

---

## 5. Roster revisado — 40 obrigatorios + 8 hooks opcionais

### 5.1 Os 40 obrigatorios

Estes devem ser criados primeiro como `EnemyDataSO` oficiais.

| # | EnemyId | Nome | Faixa | Grupo | Faction | Roles | Movement | Size | DamageType | StatusEffectIds | VulnerabilityTrigger | XP fallback |
|---:|---|---|---|---|---|---|---|---|---|---|---|---:|
| 1 | enemy_cave_mite | Acari da Fenda | 1-10 | Fauna subterranea | beast | Swarm, Chaser | SwarmErratic | Tiny | Physical | none | AfterAttackRecover | 4 |
| 2 | enemy_stone_rat | Rato de Basalto | 1-10 | Fauna subterranea | beast | Chaser | GroundChase | Small | Physical | none | DuringChargeWindup | 5 |
| 3 | enemy_cave_bat | Morcego de Fenda | 1-10 | Fauna subterranea | beast | Swarm, Ranged | SwarmErratic | Tiny | Physical | status_bleed_minor | AfterProjectileVolley | 6 |
| 4 | enemy_goblin_grashnaar_scavenger | Saqueador Grash'naar | 1-10 | Goblins | goblin | Chaser, Ranged | GroundPatrol | Small | Physical | none | AfterAttackRecover | 7 |
| 5 | enemy_kobold_scout | Batedor Kobold | 1-10 | Kobolds | kobold | Chaser, Ranged | KiteRanged | Small | Physical | none | AfterProjectileVolley | 8 |
| 6 | enemy_mossling | Musguinho Errante | 1-10 | Fungos | fungal | Guard, Tank | GroundPatrol | Small | Toxic | status_poison_minor | AfterAttackRecover | 6 |
| 7 | enemy_cracked_bone | Osso Rachado | 1-10 | Undead menor | undead | Chaser | GroundChase | Medium | Physical | none | DuringChargeWindup | 8 |
| 8 | enemy_blackroot_sprout | Broto Raiz-Negra | 1-10 | Fungos | fungal | Ranged, Guard | GuardStationary | Small | Toxic | status_slow_minor | AfterProjectileVolley | 8 |
| 9 | enemy_spore_imp | Diabrete de Esporo | 11-25 | Colonia fungica | fungal | Caster, Swarm | SwarmErratic | Small | Toxic | status_poison | AfterCast | 16 |
| 10 | enemy_rootsnare | Garra-Raiz | 11-25 | Colonia fungica | fungal | Guard, Burrower | BurrowAmbush | Medium | Physical | status_root_minor | AfterBurrowEmerges | 18 |
| 11 | enemy_hollow_stagling | Cervino Oco | 11-25 | Predador natural | beast | Chaser, Elite | Leaper | Medium | Physical | none | DuringChargeWindup | 20 |
| 12 | enemy_goblin_urudakh_trapper | Armeiro Uru'dakh | 11-25 | Goblins | goblin | Ranged, Guard | KiteRanged | Small | Physical | status_slow_minor | AfterProjectileVolley | 21 |
| 13 | enemy_thorn_archer | Espinhador Sombrio | 11-25 | Goblins/Cultistas | goblin | Ranged | KiteRanged | Medium | Physical | status_bleed | AfterProjectileVolley | 22 |
| 14 | enemy_orc_nyx_stalker | Espreitador Orc de Nyx | 11-25 | Orcs de Nyx | orc | Chaser, Burrower | BurrowAmbush | Medium | Arcane | status_slow_minor | AfterBurrowEmerges | 28 |
| 15 | enemy_mycobulwark | Baluarte Micelio | 11-25 | Colonia fungica | fungal | Tank, Guard | TankSlowPush | Large | Toxic | status_poison | AfterShieldDrop | 26 |
| 16 | enemy_nyx_moth | Mariposa de Nyx | 11-25 | Nyx menor | abyssal | Caster, Swarm | SwarmErratic | Small | Arcane | status_slow | AfterCast | 28 |
| 17 | enemy_frost_gnawer | Roedor de Geada | 26-40 | Fauna congelada | beast | Chaser | GroundChase | Small | Ice | status_chill_minor | DuringChargeWindup | 34 |
| 18 | enemy_duergar_frostdelver | Escavador Duergar do Gelo | 26-40 | Patrulha duergar | duergar | Guard, Tank | GroundPatrol | Medium | Physical | status_chill_minor | AfterShieldDrop | 40 |
| 19 | enemy_duergar_shieldbreaker | Quebra-Escudo Duergar | 26-40 | Patrulha duergar | duergar | Tank, Elite | TankSlowPush | Large | Physical | none | AfterAttackRecover | 48 |
| 20 | enemy_icebound_sentinel | Sentinela Enregelado | 26-40 | Construct gelido | construct | Guard, Tank | GuardStationary | Large | Ice | status_chill | AfterShieldDrop | 42 |
| 21 | enemy_glassbone | Osso de Vidro | 26-40 | Undead frio | undead | Ranged, Chaser | GroundPatrol | Medium | Ice | status_bleed_minor | AfterProjectileVolley | 38 |
| 22 | enemy_cold_cult_acolyte | Acolito do Frio | 26-40 | Culto do frio | cultist | Caster | CasterKeepAway | Medium | Ice | status_chill | AfterCast | 44 |
| 23 | enemy_crystal_leaper | Saltador Cristalino | 26-40 | Elemental cristalino | elemental | Chaser, Elite | Leaper | Medium | Ice | status_chill_minor | DuringChargeWindup | 46 |
| 24 | enemy_frost_wailer | Lamento Frio | 26-40 | Undead frio | undead | Caster, Elite | CasterKeepAway | Medium | Ice | status_slow | AfterCast | 52 |
| 25 | enemy_ember_tick | Carrapato de Brasa | 41-55 | Fauna de brasa | beast | Swarm, Chaser | SwarmErratic | Tiny | Fire | status_burn_minor | AfterAttackRecover | 54 |
| 26 | enemy_ash_crawler | Rastejante de Cinza | 41-55 | Fauna de brasa | elemental | Chaser | GroundChase | Medium | Fire | status_burn_minor | DuringChargeWindup | 60 |
| 27 | enemy_orc_kaand_berserker | Berserker Orc de Kaand | 41-55 | Orcs de Kaand | orc | Chaser, Elite | GroundChase | Large | Physical | status_bleed | DuringChargeWindup | 74 |
| 28 | enemy_orc_kaand_ashcaller | Chamador de Cinzas de Kaand | 41-55 | Orcs de Kaand | orc | Caster, Ranged | CasterKeepAway | Medium | Fire | status_burn | AfterCast | 76 |
| 29 | enemy_lava_bulwark | Baluarte de Lava | 41-55 | Elemental/guardiao | elemental | Tank, Guard | TankSlowPush | Large | Fire | status_burn | AfterShieldDrop | 72 |
| 30 | enemy_cinder_spitter | Cuspidor de Cinza | 41-55 | Fauna de brasa | beast | Ranged | KiteRanged | Medium | Fire | status_burn_minor | AfterProjectileVolley | 66 |
| 31 | enemy_scorched_cultist | Cultista Chamuscado | 41-55 | Culto de fogo | cultist | Caster | CasterKeepAway | Medium | Fire | status_burn | AfterCast | 70 |
| 32 | enemy_furnace_warden | Guardiao da Fornalha | 41-55 | Construct/fornalha | construct | Guard, Elite | GuardStationary | Large | Fire | status_burn | AfterShieldDrop | 90 |
| 33 | enemy_rune_shard | Lasca Runica | 56-70 | Ruinas runicas | construct | Swarm, Ranged | SwarmErratic | Small | Arcane | none | AfterProjectileVolley | 82 |
| 34 | enemy_clockwork_guard | Guarda de Corda | 56-70 | Ruinas runicas | construct | Guard, Tank | GuardStationary | Medium | Physical | none | AfterShieldDrop | 94 |
| 35 | enemy_gnome_gem_madcap | Gnomo de Gema Enlouquecido | 56-70 | Gnomos/Gemas | gnome | Caster, Ranged | KiteRanged | Small | Arcane | status_confuse_minor | AfterCast | 98 |
| 36 | enemy_gnomorin_rune_tinker | Gnomorin Runa-Torta | 56-70 | Gnomorin/ruinas | gnome | Ranged, Guard | GroundPatrol | Small | Arcane | status_slow_minor | AfterProjectileVolley | 102 |
| 37 | enemy_sealed_knight | Cavaleiro Selado | 56-70 | Undead juramentado | undead | Tank, Elite | TankSlowPush | Large | Physical | status_bleed | AfterAttackRecover | 110 |
| 38 | enemy_mirror_adept | Adepto do Espelho | 56-70 | Cultista/ruinas | cultist | Caster, Elite | PhaseShortBlink | Medium | Arcane | status_slow | AfterBlinkArrival | 108 |
| 39 | enemy_puzzle_golem | Golem de Enigma | 56-70 | Construct runico | construct | Tank, Guard | TankSlowPush | Large | Physical | none | AfterShieldDrop | 120 |
| 40 | enemy_oathless_shade | Sombra Sem-Juramento | 56-70 | Undead/ruinas | undead | Caster, Elite | PhaseShortBlink | Medium | Arcane | status_slow | AfterBlinkArrival | 126 |

### 5.2 Hooks opcionais para tiers altos e bosses

Estes podem entrar como extras na SPEC 13 se couber, ou ficar como backlog para SPEC 14/15/boss gates.

| # | EnemyId | Nome | Faixa | Grupo | Faction | Roles | Movement | Size | DamageType | StatusEffectIds | VulnerabilityTrigger | XP fallback |
|---:|---|---|---|---|---|---|---|---|---|---|---|---:|
| 41 | enemy_drow_shadowblade | Lamina Sombria Drow | 71-85 | Corte drow | drow | Chaser, Elite | PhaseShortBlink | Medium | Physical | status_bleed | AfterBlinkArrival | 150 |
| 42 | enemy_drow_moon_caster | Conjurador Lunar Drow | 71-85 | Corte drow | drow | Caster, Elite | CasterKeepAway | Medium | Arcane | status_slow | AfterCast | 165 |
| 43 | enemy_drow_web_scout | Batedor de Teia Drow | 71-85 | Corte drow | drow | Ranged, Guard | KiteRanged | Medium | Physical | status_root_minor | AfterProjectileVolley | 145 |
| 44 | enemy_void_caster | Conjurador do Vazio | 71-85 | Abismo/Nyx | abyssal | Caster, Elite | CasterKeepAway | Medium | Arcane | status_slow | AfterCast | 160 |
| 45 | enemy_corrupt_hulk | Massa Corrompida | 71-99 | Pedra Negra | corrupted | Tank, Chaser | TankSlowPush | Huge | Toxic | status_poison | AfterEnragePulse | 180 |
| 46 | enemy_ninrorin_broken_oracle | Oraculo Ninrorin Quebrado | 86-99 | Ninrorin quebrados | ninrorin | Caster, Elite | PhaseShortBlink | Medium | Arcane | status_slow | AfterCast | 220 |
| 47 | enemy_corrupted_pseudodragon | Pseudodragao Corrompido | 86-99 | Draconico profundo | draconic | Caster, Ranged | SwarmErratic | Small | Arcane | status_burn_minor | AfterProjectileVolley | 230 |
| 48 | enemy_blackstone_wyvern | Wyvern de Pedra Negra | 86-99 | Draconico corrompido | draconic | Boss, Elite | Leaper | Boss | Toxic | status_poison, status_bleed | DuringChargeWindup | 420 |

Observacao:

- Drow e Ninrorin foram movidos para niveis altos para preservar impacto narrativo.
- Draconicos aparecem apenas em high tier.
- Wyvern nao deve aparecer como inimigo comum em salas pequenas.
- Pseudodragao pode ser pequeno, rapido e irritante, mas ainda high tier por ser corrompido/arcano.

---

## 6. Regras de spawn por ecossistema

### 6.1 Packs recomendados por sala

#### Tutorial / pedra

```text
Pack A: cave_mite x3 + stone_rat x1
Pack B: goblin_grashnaar_scavenger x2 + kobold_scout x1
Pack C: mossling x2 + blackroot_sprout x1
Pack D: cracked_bone x1 + stone_rat x2
```

#### Floresta subterranea

```text
Pack A: spore_imp x2 + mossling x2
Pack B: rootsnare x1 + blackroot_sprout x2
Pack C: goblin_urudakh_trapper x1 + thorn_archer x1 + goblin_grashnaar_scavenger x2
Pack D: orc_nyx_stalker x1 + nyx_moth x2
```

#### Gelo

```text
Pack A: frost_gnawer x2 + glassbone x1
Pack B: duergar_frostdelver x2 + duergar_shieldbreaker x1
Pack C: icebound_sentinel x1 + cold_cult_acolyte x1
Pack D: crystal_leaper x1 + frost_wailer x1
```

#### Fogo

```text
Pack A: ember_tick x4 + ash_crawler x1
Pack B: orc_kaand_berserker x1 + orc_kaand_ashcaller x1
Pack C: lava_bulwark x1 + cinder_spitter x2
Pack D: furnace_warden x1 + scorched_cultist x1
```

#### Ruinas

```text
Pack A: rune_shard x3 + clockwork_guard x1
Pack B: gnome_gem_madcap x1 + gnomorin_rune_tinker x2
Pack C: sealed_knight x1 + oathless_shade x1
Pack D: puzzle_golem x1 + mirror_adept x1
```

#### Abismo

```text
Pack A: drow_shadowblade x1 + drow_web_scout x1
Pack B: drow_moon_caster x1 + oathless_shade x1
Pack C: void_caster x1 + nyx_moth/nyxling future x2
Pack D: black_meteor_spawn future + corrupt_hulk x1
```

#### Nucleo corrompido

```text
Pack A: ninrorin_broken_oracle x1 + corrupt_hulk x1
Pack B: corrupted_pseudodragon x2 + deep_drake future x1
Pack C: blackstone_wyvern x1 somente arena/sala grande/boss hook
Pack D: meteorborn_abomination future + black_meteor_spawn future
```

### 6.2 Restricoes de tamanho por sala

```text
Tiny: qualquer sala valida
Small: qualquer sala valida
Medium: sala pequena ou maior
Large: sala media ou maior
Huge: sala grande somente
Boss: arena/boss room somente
```

Regras:

- Nunca spawnar Huge/Boss em corredor.
- Large precisa de `MinimumRoomSizeForSizeClass >= Medium`.
- Wyvern, drake e colossus exigem arena ou sala grande.
- Swarm pode usar mais unidades, mas cada unidade Tiny/Small deve respeitar limite de quantidade por sala.

---

## 7. Movement profiles detalhados

| MovementProfileId | Uso | Regras |
|---|---|---|
| GroundChase | ratos, esqueletos, orcs agressivos | anda ate o player, respeita stop distance |
| GroundPatrol | goblins, duergar, patrulhas | patrulha entre pontos e persegue se detectar |
| GuardStationary | sentinelas, sprouts, golems | guarda posicao; persegue pouco ou nada |
| KiteRanged | arqueiros, kobolds, drows ranged | mantem distancia preferida e recua se player aproxima |
| CasterKeepAway | casters | tenta manter distancia, castar e reposicionar |
| BurrowAmbush | rootsnare, beetle, orc nyx | some/reaparece em safe anchor perto do player |
| SwarmErratic | insetos, wisps, pseudodragao corrompido MVP | movimento curto irregular, baixo custo |
| TankSlowPush | bulwarks, hulks, guardioes | avanca lento, dificil de empurrar |
| PhaseShortBlink | drow/ninrorin/sombras | blink curto validado contra colisao/nav area |
| Leaper | stagling, crystal leaper, wyvern MVP | salto curto com windup e landing recovery |

---

## 8. Vulnerability profiles recomendados

| VulnerabilityProfileId | Trigger | Duração | Multiplier | Uso |
|---|---|---:|---:|---|
| vuln_swarm_after_bite | AfterAttackRecover | 0.45s | 1.5 | Tiny/swarm |
| vuln_chaser_charge | DuringChargeWindup | 0.55s | 1.5 | chasers simples |
| vuln_ranged_after_volley | AfterProjectileVolley | 0.70s | 1.5 | ranged/kite |
| vuln_caster_after_cast | AfterCast | 0.80s | 1.5 | casters |
| vuln_burrow_emerge | AfterBurrowEmerges | 0.90s | 1.6 | burrowers |
| vuln_guard_shield_drop | AfterShieldDrop | 0.75s | 1.4 | guard/tank |
| vuln_tank_recover | AfterAttackRecover | 0.65s | 1.35 | tanks pesados |
| vuln_phase_arrival | AfterBlinkArrival | 0.50s | 1.6 | drow/sombras/ninrorin |
| vuln_leaper_landing | AfterAttackRecover | 0.70s | 1.6 | leapers |
| vuln_corrupted_enrage_pulse | AfterEnragePulse | 0.60s | 1.7 | corrupted/hulks |

Regras:

- Todo EnemyDataSO deve apontar para um profile.
- Bestiary registra discovery quando o player acerta durante a janela.
- Bosses podem ter janela menor, mas nao zero.

---

## 9. DamageType vs StatusEffectIds

Nao misturar status com tipo de dano.

### 9.1 DamageType permitido

Usar o que existir no codigo. Se o enum atual tiver menos tipos, adaptar para os existentes sem criar enum paralelo.

Tipos esperados/aceitos:

```text
Physical
Fire
Ice
Arcane
Lightning
Toxic
TrueDamage opcional
```

### 9.2 StatusEffectIds sugeridos

```text
status_poison_minor
status_poison
status_bleed_minor
status_bleed
status_slow_minor
status_slow
status_chill_minor
status_chill
status_burn_minor
status_burn
status_root_minor
status_confuse_minor
```

Se algum status nao existir, criar como data se o sistema suportar. Se o sistema de status ainda for parcial, criar hook/placeholder data-driven e registrar pendencia sem criar manager paralelo.

---

## 10. Size profiles numericos iniciais

| SizeClass | SpriteScale | ColliderRadius | FootprintCells | PathingRadius | DamageNumberOffsetY | KnockbackMultiplier |
|---|---:|---:|---:|---:|---:|---:|
| Tiny | 0.65 | 0.22 | 1 | 0.20 | 0.45 | 1.30 |
| Small | 0.85 | 0.32 | 1 | 0.30 | 0.65 | 1.10 |
| Medium | 1.00 | 0.45 | 1 | 0.45 | 0.90 | 1.00 |
| Large | 1.35 | 0.65 | 2 | 0.65 | 1.25 | 0.75 |
| Huge | 1.80 | 0.95 | 3 | 0.95 | 1.70 | 0.50 |
| Boss | 2.20 | 1.20 | 4 | 1.20 | 2.10 | 0.35 |

Regras:

- Nao alterar PPU/import para representar tamanho.
- Usar visual child scale + collider ajustado.
- Spawn resolver deve validar sala/corredor antes de instanciar Large/Huge/Boss.

---

## 11. Bestiary text minimo por inimigo

Cada `EnemyBestiaryEntrySO` deve conter:

```text
BestiaryEntryId
EnemyId
DisplayName
ShortDescription
HabitatText
BehaviorHint
VulnerabilityHintLocked
VulnerabilityHintDiscovered
KnownDropsHint
FactionText
FirstSeenUnlockMode
```

Exemplo:

```text
EnemyId: enemy_orc_kaand_berserker
DisplayName: Berserker Orc de Kaand
ShortDescription: Um guerreiro exilado ou corrompido pela fúria subterranea, usando calor, pedra e sangue como linguagem de combate.
HabitatText: Aparece em cavernas de fogo e zonas de fornalha antiga.
BehaviorHint: Avança em linha agressiva e pune jogadores parados.
VulnerabilityHintLocked: Sua abertura aparece antes do impacto mais pesado.
VulnerabilityHintDiscovered: Fica vulneravel durante a investida, antes do golpe terminar.
KnownDropsHint: Fragmentos de cinza, metal aquecido, possivel componente de arma.
FactionText: Orcs de Kaand / faccao_orc.
```

---

## 12. Implementacao em etapas pequenas

### SPEC 13A — Enemy taxonomy, profiles and contracts

Escopo:

- factions revisadas;
- size profiles;
- movement profiles;
- vulnerability profiles;
- corrigir `Phase` para movement profile, nao role;
- validar `EnemyDataSO` contra os campos minimos.

### SPEC 13B — Roster 40 EnemyDataSO

Escopo:

- criar os 40 obrigatorios;
- cada um com faction, roles, size, movement, vulnerability, XP fallback;
- sem exigir IA completa ainda.

### SPEC 13C — Enemy actions/action sets

Escopo:

- `EnemyActionSO`;
- `EnemyActionSetSO`;
- pelo menos action principal para cada um dos 40;
- status e DamageType separados.

### SPEC 13D — EnemyBrain runtime MVP

Escopo:

- state machine;
- movement profiles funcionais exceto Flying;
- 8 inimigos testaveis;
- 5 roles funcionais;
- telegraph blink/color;
- vulnerability windows.

### SPEC 13E — Bestiary runtime/save

Escopo:

- FirstSeen;
- KillCount;
- DropsDiscovered;
- Weakness/Resistance/Vulnerability discovery;
- save/load por IDs.

### SPEC 13F — Spawn resolver/ecology/faction locks

Escopo:

- spawn resolver data-driven;
- packs/coexistencia;
- room size constraints;
- faction locks por boss gate/story flag;
- integração futura com SPEC 14 cave snapshot.

---

## 13. Critérios de pronto do refinamento

A SPEC 13 so deve ser considerada pronta para execução depois que:

- a spec principal substituir ou referenciar este roster revisado;
- os 40 obrigatorios estiverem separados dos 8 hooks;
- `Phase` deixar de ser role;
- DamageType e StatusEffectIds forem separados;
- cada inimigo tiver size profile, movement profile e vulnerability profile;
- cada band tiver grupos de coexistencia;
- spawn resolver respeitar sala, tamanho, faction e band;
- Bestiary tiver texto de entrada, nao apenas telemetria.
