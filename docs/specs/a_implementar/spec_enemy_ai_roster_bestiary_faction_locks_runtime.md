# SPEC 13 - Enemy AI, roster, bestiary e faction locks runtime

> Spec ID: spec_enemy_ai_roster_bestiary_faction_locks_runtime
> Status: Implementado parcial - escopo residual ativo
> Ordem de execucao: 13
> Tipo: Runtime/Data/UI minima/Bestiary/Cave Ecology
> Fonte de refinamento principal: `docs/refinements/a_implementar/pre_refinamentos/refinamento_spec13_bestiary_roster_enemy_data.md`
> Depende de: 00-12
> Bloqueia: 14, 15, 17
> Fora de escopo: boss fights finais, cave runtime completo, snapshot completo de inimigos por sala, animacoes/VFX finais, sprites finais, companion AI, balance final de XP, copiar statblocks/textos de D&D, Packages, ProjectSettings e docs_old.

---

# /speckit.specify

## O QUE

Completar a base data-driven de inimigos de Cindar's Hope para que a cave tenha criaturas coerentes com Vaalara, fantasia tabletop e progressao de 100 niveis.

Esta spec deve entregar:

1. Taxonomia canonica de inimigos de Vaalara.
2. Factions tecnicas revisadas.
3. Roster minimo de 40 `EnemyDataSO` oficiais.
4. 8 hooks opcionais para tiers altos, draconicos, drow, Ninrorin e boss/gate future work.
5. Movement profiles variados, exceto Flying.
6. Size profiles com escala, collider, footprint, pathing e restricao de sala.
7. Vulnerability windows diferentes por comportamento.
8. Enemy actions/action sets data-driven.
9. EnemyBrain MVP com state machine simples.
10. Telegraph por blink/cor durante wind-up.
11. Bestiary runtime/save.
12. Spawn resolver por band, faction, bioma, ambiente, boss gate progress, tamanho de sala e coexistencia.
13. XP e loot integrados sem duplicacao.

## POR QUE

A spec anterior tinha uma lista funcional, mas generica demais. Faltavam criaturas e povos de Vaalara, como goblins, kobolds, orcs, duergar, drows, gnomos, gnomorin, Ninrorin e criaturas draconicas.

A cave precisa parecer um ecossistema subterraneo do mundo, nao uma lista aleatoria de monstros.

## REGRAS DE DESIGN

- Criaturas devem ser de Vaalara e/ou arquetipos classicos de fantasia adaptados.
- Nao copiar statblocks, textos, nomes proprietarios ou habilidades exatas de D&D.
- Nao colocar todos os inimigos no mesmo nivel.
- Grupos de inimigos devem coexistir de forma plausivel por band/bioma/faction.
- Tamanho do inimigo deve impactar visual, collider, spawn e pathing.
- Todo inimigo deve ter movement profile, size profile e vulnerability profile.
- `Phase` nao e role. Blink/fase curta deve ser `MovementProfile = PhaseShortBlink`.
- `Poison`, `Bleed`, `Slow`, `Burn`, `Chill`, `Root` e similares sao status effects, nao `DamageType`.
- Boss/Huge nao podem nascer em corredor ou sala pequena.

---

# /speckit.clarify

## Decisoes fechadas

| Tema | Decisao |
|---|---|
| Roster base | 40 obrigatorios + 8 hooks opcionais |
| Identidade | Vaalara + fantasia tabletop adaptada |
| Goblins | Entram desde tiers baixos, com clãs/tags internas |
| Kobolds | Entram desde tiers baixos, conectados a draconicos futuros |
| Orcs | Entram em Nyx/floresta subterranea e Kaand/fogo |
| Duergar | Entram em gelo/patrulhas subterraneas |
| Drows | Entram em tiers altos/abismo |
| Gnomos/Gnomorin | Entram nas ruinas antigas |
| Ninrorin | Entram apenas nos niveis altos/corrompidos |
| Pseudodragao/Drake/Wyvern | Entram em high tier; Wyvern como hook/boss/arena |
| Flying | Fora do MVP |
| Phase | Movement profile, nao role |
| Bestiary | Deve ter texto de entrada, nao apenas contadores |

## Factions tecnicas revisadas

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

Tags internas recomendadas:

```text
orc: kaand, nyx, chama_viva
goblin: grashnaar, urudakh, zhakthul
gnome: gnome, gnomorin, gem_gnome
draconic: kobold_linked, pseudodragon, drake, wyvern
```

---

# /speckit.plan

## Arquitetura alvo

```text
EnemyDataSO
├── Identity/faction/roles/band/tags
├── SizeProfileId
├── MovementProfileId
├── ActionSetId
├── VulnerabilityProfileId
├── CombatResistanceProfileId
├── LootTableId
├── XPReward
└── BestiaryEntryId

EnemyActionSetSO
└── EnemyActionSO[]

EnemyBrain
├── State machine MVP
├── Movement profile runtime
├── Action selection
├── Telegraph controller
└── Vulnerability window trigger

BestiaryManager
├── FirstSeen
├── KillCount
├── DropsDiscovered
├── Weakness/Resistance discovery
├── Vulnerability discovery
└── Save/load por IDs

EnemySpawnResolver
├── Cave level range
├── Biome/environment tags
├── Faction locks
├── Boss gate progress
├── Room size
├── Size class
├── Packs/coexistence
└── Weighted spawn
```

## Contratos existentes a preservar

- `EnemyDataSO` continua sendo contrato principal de enemy data.
- `EnemyRole` continua limitado a roles oficiais.
- `DamageCalculator`, `DamageRequest`, `DamageType` e status da SPEC 11 continuam sendo a base de dano/status.
- Loot usa `LootTableSO`/contratos existentes da SPEC 10.
- Save usa IDs e DTOs simples.
- Nada serializa `ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody`.
- Nao usar `GameObject.Find`, `FindObjectOfType` ou buscas globais runtime novas.

## Roles oficiais

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

Nao adicionar `Phase` como role.

## Movement profiles

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

`Flying` fica fora do MVP.

## Size classes

| SizeClass | Uso | Restricao |
|---|---|---|
| Tiny | insetos, enxames, morcegos pequenos | qualquer sala valida |
| Small | goblins, kobolds, gnomos, wisps | qualquer sala valida |
| Medium | humanoides comuns, drows, cultistas, undead | sala pequena ou maior |
| Large | orcs grandes, duergar elite, beasts, sentinelas | sala media ou maior |
| Huge | hulks, drakes grandes, constructs pesados | sala grande somente |
| Boss | wyvern, colossos, gate guardians | arena/boss room somente |

Defaults iniciais de size ficam no refinamento.

## Vulnerability trigger modes

```text
AfterAttackRecover
DuringChargeWindup
AfterBurrowEmerges
AfterCast
AfterProjectileVolley
AfterShieldDrop
AfterBlinkArrival
AfterEnragePulse
AlwaysForTest somente debug
```

Todo inimigo deve apontar para um vulnerability profile.

---

# /speckit.roster

## Fonte de verdade do roster

A fonte detalhada do roster revisado e:

```text
docs/refinements/a_implementar/pre_refinamentos/refinamento_spec13_bestiary_roster_enemy_data.md
```

A implementacao deve seguir o roster revisado abaixo e o refinamento quando houver conflito com a versao antiga da SPEC 13.

## Roster obrigatorio de 40 inimigos

| # | EnemyId | Nome | Faixa | Faction | Roles | Movement | Size |
|---:|---|---|---|---|---|---|---|
| 1 | enemy_cave_mite | Acari da Fenda | 1-10 | beast | Swarm, Chaser | SwarmErratic | Tiny |
| 2 | enemy_stone_rat | Rato de Basalto | 1-10 | beast | Chaser | GroundChase | Small |
| 3 | enemy_cave_bat | Morcego de Fenda | 1-10 | beast | Swarm, Ranged | SwarmErratic | Tiny |
| 4 | enemy_goblin_grashnaar_scavenger | Saqueador Grash'naar | 1-10 | goblin | Chaser, Ranged | GroundPatrol | Small |
| 5 | enemy_kobold_scout | Batedor Kobold | 1-10 | kobold | Chaser, Ranged | KiteRanged | Small |
| 6 | enemy_mossling | Musguinho Errante | 1-10 | fungal | Guard, Tank | GroundPatrol | Small |
| 7 | enemy_cracked_bone | Osso Rachado | 1-10 | undead | Chaser | GroundChase | Medium |
| 8 | enemy_blackroot_sprout | Broto Raiz-Negra | 1-10 | fungal | Ranged, Guard | GuardStationary | Small |
| 9 | enemy_spore_imp | Diabrete de Esporo | 11-25 | fungal | Caster, Swarm | SwarmErratic | Small |
| 10 | enemy_rootsnare | Garra-Raiz | 11-25 | fungal | Guard, Burrower | BurrowAmbush | Medium |
| 11 | enemy_hollow_stagling | Cervino Oco | 11-25 | beast | Chaser, Elite | Leaper | Medium |
| 12 | enemy_goblin_urudakh_trapper | Armeiro Uru'dakh | 11-25 | goblin | Ranged, Guard | KiteRanged | Small |
| 13 | enemy_thorn_archer | Espinhador Sombrio | 11-25 | goblin | Ranged | KiteRanged | Medium |
| 14 | enemy_orc_nyx_stalker | Espreitador Orc de Nyx | 11-25 | orc | Chaser, Burrower | BurrowAmbush | Medium |
| 15 | enemy_mycobulwark | Baluarte Micelio | 11-25 | fungal | Tank, Guard | TankSlowPush | Large |
| 16 | enemy_nyx_moth | Mariposa de Nyx | 11-25 | abyssal | Caster, Swarm | SwarmErratic | Small |
| 17 | enemy_frost_gnawer | Roedor de Geada | 26-40 | beast | Chaser | GroundChase | Small |
| 18 | enemy_duergar_frostdelver | Escavador Duergar do Gelo | 26-40 | duergar | Guard, Tank | GroundPatrol | Medium |
| 19 | enemy_duergar_shieldbreaker | Quebra-Escudo Duergar | 26-40 | duergar | Tank, Elite | TankSlowPush | Large |
| 20 | enemy_icebound_sentinel | Sentinela Enregelado | 26-40 | construct | Guard, Tank | GuardStationary | Large |
| 21 | enemy_glassbone | Osso de Vidro | 26-40 | undead | Ranged, Chaser | GroundPatrol | Medium |
| 22 | enemy_cold_cult_acolyte | Acolito do Frio | 26-40 | cultist | Caster | CasterKeepAway | Medium |
| 23 | enemy_crystal_leaper | Saltador Cristalino | 26-40 | elemental | Chaser, Elite | Leaper | Medium |
| 24 | enemy_frost_wailer | Lamento Frio | 26-40 | undead | Caster, Elite | CasterKeepAway | Medium |
| 25 | enemy_ember_tick | Carrapato de Brasa | 41-55 | beast | Swarm, Chaser | SwarmErratic | Tiny |
| 26 | enemy_ash_crawler | Rastejante de Cinza | 41-55 | elemental | Chaser | GroundChase | Medium |
| 27 | enemy_orc_kaand_berserker | Berserker Orc de Kaand | 41-55 | orc | Chaser, Elite | GroundChase | Large |
| 28 | enemy_orc_kaand_ashcaller | Chamador de Cinzas de Kaand | 41-55 | orc | Caster, Ranged | CasterKeepAway | Medium |
| 29 | enemy_lava_bulwark | Baluarte de Lava | 41-55 | elemental | Tank, Guard | TankSlowPush | Large |
| 30 | enemy_cinder_spitter | Cuspidor de Cinza | 41-55 | beast | Ranged | KiteRanged | Medium |
| 31 | enemy_scorched_cultist | Cultista Chamuscado | 41-55 | cultist | Caster | CasterKeepAway | Medium |
| 32 | enemy_furnace_warden | Guardiao da Fornalha | 41-55 | construct | Guard, Elite | GuardStationary | Large |
| 33 | enemy_rune_shard | Lasca Runica | 56-70 | construct | Swarm, Ranged | SwarmErratic | Small |
| 34 | enemy_clockwork_guard | Guarda de Corda | 56-70 | construct | Guard, Tank | GuardStationary | Medium |
| 35 | enemy_gnome_gem_madcap | Gnomo de Gema Enlouquecido | 56-70 | gnome | Caster, Ranged | KiteRanged | Small |
| 36 | enemy_gnomorin_rune_tinker | Gnomorin Runa-Torta | 56-70 | gnome | Ranged, Guard | GroundPatrol | Small |
| 37 | enemy_sealed_knight | Cavaleiro Selado | 56-70 | undead | Tank, Elite | TankSlowPush | Large |
| 38 | enemy_mirror_adept | Adepto do Espelho | 56-70 | cultist | Caster, Elite | PhaseShortBlink | Medium |
| 39 | enemy_puzzle_golem | Golem de Enigma | 56-70 | construct | Tank, Guard | TankSlowPush | Large |
| 40 | enemy_oathless_shade | Sombra Sem-Juramento | 56-70 | undead | Caster, Elite | PhaseShortBlink | Medium |

## Hooks opcionais / high tier / boss future work

| # | EnemyId | Nome | Faixa | Faction | Roles | Movement | Size |
|---:|---|---|---|---|---|---|---|
| 41 | enemy_drow_shadowblade | Lamina Sombria Drow | 71-85 | drow | Chaser, Elite | PhaseShortBlink | Medium |
| 42 | enemy_drow_moon_caster | Conjurador Lunar Drow | 71-85 | drow | Caster, Elite | CasterKeepAway | Medium |
| 43 | enemy_drow_web_scout | Batedor de Teia Drow | 71-85 | drow | Ranged, Guard | KiteRanged | Medium |
| 44 | enemy_void_caster | Conjurador do Vazio | 71-85 | abyssal | Caster, Elite | CasterKeepAway | Medium |
| 45 | enemy_corrupt_hulk | Massa Corrompida | 71-99 | corrupted | Tank, Chaser | TankSlowPush | Huge |
| 46 | enemy_ninrorin_broken_oracle | Oraculo Ninrorin Quebrado | 86-99 | ninrorin | Caster, Elite | PhaseShortBlink | Medium |
| 47 | enemy_corrupted_pseudodragon | Pseudodragao Corrompido | 86-99 | draconic | Caster, Ranged | SwarmErratic | Small |
| 48 | enemy_blackstone_wyvern | Wyvern de Pedra Negra | 86-99 | draconic | Boss, Elite | Leaper | Boss |

## Packs/coexistencia obrigatorios

O spawn resolver deve suportar packs por band. Exemplos minimos:

```text
1-10:
- cave_mite x3 + stone_rat x1
- goblin_grashnaar_scavenger x2 + kobold_scout x1
- mossling x2 + blackroot_sprout x1

11-25:
- spore_imp x2 + mossling x2
- rootsnare x1 + blackroot_sprout x2
- goblin_urudakh_trapper x1 + thorn_archer x1 + goblin_grashnaar_scavenger x2
- orc_nyx_stalker x1 + nyx_moth x2

26-40:
- frost_gnawer x2 + glassbone x1
- duergar_frostdelver x2 + duergar_shieldbreaker x1
- icebound_sentinel x1 + cold_cult_acolyte x1
- crystal_leaper x1 + frost_wailer x1

41-55:
- ember_tick x4 + ash_crawler x1
- orc_kaand_berserker x1 + orc_kaand_ashcaller x1
- lava_bulwark x1 + cinder_spitter x2
- furnace_warden x1 + scorched_cultist x1

56-70:
- rune_shard x3 + clockwork_guard x1
- gnome_gem_madcap x1 + gnomorin_rune_tinker x2
- sealed_knight x1 + oathless_shade x1
- puzzle_golem x1 + mirror_adept x1
```

High tier packs ficam como hooks para SPEC 14/15 quando os niveis 71+ estiverem materializados com arenas/salas adequadas.

---

# /speckit.tasks

## SPEC 13A - Enemy taxonomy, profiles and contracts

- [ ] Revalidar `EnemyDataSO`, `EnemyRole`, `EnemyHealth`, `EnemyBrain`, XP/drop e eventos existentes.
- [ ] Atualizar/confirmar factions tecnicas revisadas.
- [ ] Criar/ajustar `EnemySizeProfileSO` com Tiny/Small/Medium/Large/Huge/Boss.
- [ ] Criar/ajustar `EnemyMovementProfileSO` com todos os profiles exceto Flying.
- [ ] Criar/ajustar `EnemyVulnerabilityProfileSO` com triggers aprovados.
- [ ] Garantir que `Phase` nao aparece como role.
- [ ] Criar validator para verificar role/movement/size/vulnerability por EnemyDataSO.

## SPEC 13B - Roster 40 EnemyDataSO

- [ ] Criar os 40 `EnemyDataSO` obrigatorios do roster revisado.
- [ ] Cada EnemyDataSO deve ter faction, roles, cave band, tags, size, movement, vulnerability, XP fallback e BestiaryEntryId.
- [ ] Criar hooks opcionais 41-48 se couber sem expandir escopo; caso contrario registrar backlog.
- [ ] Garantir que nomes/descricoes sao originais de Vaalara e nao copiados de D&D.

## SPEC 13C - Enemy actions/action sets

- [ ] Criar/ajustar `EnemyActionSO` e `EnemyActionSetSO`.
- [ ] Criar action principal para cada um dos 40 inimigos.
- [ ] Separar `DamageType` de `StatusEffectIds`.
- [ ] Actions que causam dano usam `DamageRequest`/`DamageCalculator`.
- [ ] Toda action ofensiva deve ter wind-up, recover e telegraph profile.

## SPEC 13D - EnemyBrain runtime MVP

- [ ] Implementar state machine MVP sem busca cara por frame.
- [ ] Implementar decision tick configuravel.
- [ ] Implementar pelo menos 8 inimigos testaveis em runtime.
- [ ] Implementar pelo menos 5 roles funcionais: Chaser, Guard, Ranged, Caster, Tank.
- [ ] Implementar movement profiles MVP: GroundChase, GuardStationary, KiteRanged, CasterKeepAway, TankSlowPush, SwarmErratic.
- [ ] Implementar BurrowAmbush, PhaseShortBlink e Leaper se couber; caso contrario registrar como residual da SPEC 13.
- [ ] Implementar telegraph blink/color e restauracao da cor original.
- [ ] Implementar vulnerability windows integradas ao pipeline da SPEC 11.

## SPEC 13E - Bestiary runtime/save

- [ ] Criar/ajustar `EnemyBestiaryEntrySO`.
- [ ] Criar Bestiary entries com texto minimo: description, habitat, behavior hint, vulnerability hint, drops hint, faction text.
- [ ] Implementar `BestiarySaveData` e `BestiaryEntrySaveData` por IDs.
- [ ] Registrar FirstSeen.
- [ ] Registrar KillCount.
- [ ] Registrar DropsDiscovered.
- [ ] Registrar Weaknesses/Resistances quando o pipeline da SPEC 11 expuser resultado.
- [ ] Registrar VulnerabilityWindowDiscovered quando player acertar durante janela.

## SPEC 13F - Spawn resolver/ecology/faction locks

- [ ] Criar/ajustar `EnemySpawnProfileSO`.
- [ ] Criar `EnemySpawnResolver`, `EnemySpawnRequest`, `EnemySpawnResult`.
- [ ] Resolver spawn por cave level, biome tags, environment tags, faction lock, boss gate progress, room size e size class.
- [ ] Implementar packs/coexistencia por band.
- [ ] Impedir Huge/Boss em corredor/sala pequena.
- [ ] Preparar contrato para snapshot da SPEC 14 sem marcar snapshot como completo nesta spec.
- [ ] Integrar XP/loot sem duplicar recompensa por morte.

---

# /speckit.implement

## Ordem recomendada

```text
13A - taxonomy/profiles/contracts
13B - roster 40 data
13C - actions/action sets
13D - runtime brain/telegraph/vulnerability
13E - bestiary runtime/save
13F - spawn resolver/ecology/faction locks
```

## Prompt base para agente executor

```md
Leia primeiro:
- CLAUDE.md
- AGENTS.md
- docs/IMPLEMENTATION_STATUS.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_spec13_bestiary_roster_enemy_data.md
- docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md
- docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md
- docs/specs/implementados/spec_player_combat_weapons_spells_skill_actions_runtime.md

Implemente somente o recorte:
<SPEC 13A/13B/13C/13D/13E/13F>

Regras obrigatorias:
- Nao copiar statblocks/textos de D&D.
- Nao criar roles fora dos oficiais.
- Phase e movement profile, nao role.
- Separar DamageType de StatusEffectIds.
- Todo EnemyDataSO precisa de faction, roles, movement, size, vulnerability e bestiary id.
- Nao criar sistema paralelo de dano, status, loot, save ou event bus.
- Nao usar GameObject.Find/FindObjectOfType/FindObjectsByType em runtime.
- Nao serializar referencias Unity em save.

Ao final, entregue:
- arquivos alterados;
- validacoes executadas;
- riscos residuais;
- checklist Play Mode pendente.
```

---

# Definition of Done

- Roster de 40 inimigos obrigatorios existe em dados oficiais.
- Roster usa criaturas de Vaalara e arquetipos fantasy adaptados.
- Goblins, kobolds, orcs, duergar, gnomos/gnomorin, undead, elementais, constructs, cultistas e criaturas de Nyx/Pedra Negra aparecem nas faixas corretas.
- Hooks de drow, Ninrorin, pseudodragao e wyvern ficam registrados para high tier.
- Cada inimigo tem faction, roles, movement profile, size profile, vulnerability profile, action set, XP fallback e bestiary entry.
- Packs/coexistencia por band existem.
- Spawn resolver respeita cave band, faction, room size, size class e boss gate progress.
- EnemyBrain MVP funciona para pelo menos 8 inimigos e 5 roles.
- Telegraph blink/cor funciona e restaura cor original.
- Vulnerability windows integram com o pipeline de dano.
- Bestiary persiste FirstSeen, KillCount, DropsDiscovered e discoveries.
- XP e loot nao duplicam.
- Nenhuma regressao nas SPECs 10, 11 e 12.

---

# Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

1. Validar que 40 EnemyDataSO existem e carregam sem Missing Script.
2. Spawnar/testar pelo menos 8 inimigos diferentes.
3. Validar roles oficiais em dados.
4. Validar pelo menos 5 roles funcionando em runtime.
5. Validar size profiles Tiny/Small/Medium/Large/Huge/Boss em dados e collider/scale coerentes nos testaveis.
6. Validar movement profiles principais.
7. Confirmar que Flying nao foi implementado como runtime ativo.
8. Validar telegraph blink/cor no AttackWindup e restauracao de cor depois.
9. Validar vulnerability window por monstro.
10. Matar inimigo e validar XP uma vez, loot e Bestiary KillCount.
11. Causar dano Weak/Resistant/Immune e validar Bestiary discovery quando suportado.
12. Validar spawn resolver por cave band/faction/biome/environment/boss gate/size.
13. Validar que Huge/Boss nao spawnam em sala pequena.
14. Salvar/carregar Bestiary.

---

# Riscos residuais aceitos

- Balance numerico de XP/dano pode ficar provisório.
- Sprites finais podem ficar placeholder.
- Hooks 41-48 podem ficar como backlog se o recorte exigir exatamente 40 inimigos.
- Burrow/Phase/Leaper podem iniciar como runtime simplificado se o validator documentar residual.
- High tier completo depende da SPEC 14 cave runtime e futuras boss specs.
