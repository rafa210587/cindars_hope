# SPEC - Enemy AI, roster, bestiary e faction locks runtime

> Spec ID: spec_enemy_ai_roster_bestiary_faction_locks_runtime
> Status: Implementado parcial - escopo residual ativo
> Ordem de execucao: 13
> Depende de: 00-12
> Bloqueia: 14, 15, 17
> Tipo: Runtime/Data/UI minima
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar IA, roster 40+, bestiario, faction locks, ecologia, XP, telegraph, vulnerability windows, tamanhos de inimigos no mapa e pipeline data-driven para criar novos monstros.
> Fora de escopo: boss fights finais, cave runtime completo, snapshot completo de inimigos por sala, animações/VFX finais, sprites finais, companion AI, balance final de XP, copiar statblocks de D&D, Packages, ProjectSettings e docs_old.

Fontes absorvidas:
- specs/FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS/spec.md
- specs/FASE9G_ENEMY_COMBAT_ROLES_AI_STATUS/spec.md
- specs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_enemy_ai_roster_bestiary_faction_locks.md

---

# /speckit.specify

## Contexto

O modelo oficial estabilizado para inimigos e:

```text
Assets/_Game/Scripts/Combat/EnemyDataSO.cs
```

O projeto tambem possui skeletons/inicializadores de AI, bestiary, boss gates e cave. O runtime de IA ainda e parcial/MVP.

Specs anteriores relevantes:

```text
10 - Equipment/durability/environment/loot: LootTableSO, equipment instances, tamanhos/colisao a respeitar
11 - Damage/status/elements/resistances: DamageCalculator, DamageType, status, vulnerability, damage numbers
12 - Player combat/weapons/spells: player attacks, bow, spells, mana, active slots
```

Esta spec deve completar o runtime de inimigos sem invadir a spec 14, que sera dona do cave runtime/generation/checkpoints/boss gates final.

## Pre-condicoes

Implementar runtime somente depois de specs 02-12 estarem realmente implementadas:

```text
02 - save schema migration
03 - inventory slots/capacity/painel de itens
04 - farm irrigacao/solo/menu contextual
05 - world activities/fishing/trees/pickups/loot
06 - economy/shop/dialogue modal
07 - crafting/workstations/modal stack
08 - town/npc/dialogue
09 - hunger/stamina/status/time
10 - equipment/durability/environment/loot
11 - damage/status/elements/resistances
12 - player combat/weapons/spells/skill actions
```

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Combat/EnemyDataSO.cs
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Enemies/**
Assets/_Game/Data/Bestiary/**
Assets/_Game/Data/Loot/**
```

Se existirem listas antigas de inimigos, movimentacoes ou XP no repo, preservar e migrar. Se nao forem encontradas, aplicar o fallback definido nesta spec e registrar no log que o balance e provisório.

## Problema

Gaps atuais:

- nao ha roster completo de 40+ monstros em dados oficiais;
- nao ha fluxo simples para criar novos monstros sem duplicar codigo;
- EnemyBrain ainda e parcial;
- movement profiles por inimigo nao estao formalizados;
- roles de combate nao estao completos;
- monstros ainda nao possuem vulnerability window individual;
- habilidades dos monstros nao estao data-driven;
- telegraph ainda nao e padronizado;
- bestiary nao e event-driven completo;
- faction locks/ecologia nao controlam spawn real;
- tamanhos diferentes de monstros no mapa ainda nao estao formalizados;
- XP pode existir parcialmente, mas precisa ser resgatado/validado ou refeito como fallback.

## Objetivo

Implementar uma base data-driven de inimigos com:

- roster de 40 EnemyDataSO;
- abilities baseadas em Vaalara e em arquetipos de fantasia tabletop, sem copiar statblocks protegidos;
- todos os roles oficiais testaveis;
- multiplos roles por monstro;
- movement profiles, exceto flying por enquanto;
- vulnerability window por monstro;
- telegraph visual por piscada/cor do monstro;
- tamanhos diferentes no mapa;
- bestiary persistente;
- faction locks e spawn ecology;
- XPReward reaproveitado se ja existir ou fallback por band/role;
- pipeline simples para criar novos monstros no futuro.

## Decisoes aprovadas

- Criar sim os 40 monstros agora, como dados oficiais.
- Os 40 devem seguir o que ja discutimos: inspirados por Vaalara, por arquetipos de fantasia e por referencia de tabletop/D&D 5e em alto nivel, sem copiar nomes proprietarios, statblocks, textos ou habilidades exatas.
- Cada monstro precisa ter vulnerability window.
- Habilidades precisam ser coerentes com Vaalara e com arquetipos classicos de D&D/fantasia, mas implementadas como designs originais.
- Sistema deve ser facil de expandir com novos monstros no futuro.
- Roles antigos devem ser preservados, e monstros podem ter mais de um role.
- Aplicar todos os movement profiles previstos, exceto Flying.
- Telegraph de ataque: o monstro pisca/muda cor antes do ataque.
- XP existente deve ser resgatado se ja estiver definido no repo; se nao existir, usar fallback desta spec e marcar como balance provisório.
- Monstros devem ter tamanhos diferentes no mapa.

## Pipeline para criar novos monstros

Criar/usar dados modulares:

```text
EnemyDataSO
EnemyArchetypeSO opcional
EnemyActionSO
EnemyActionSetSO
EnemyMovementProfileSO
EnemySpawnProfileSO
EnemyFactionSO
EnemySizeProfileSO
EnemyVulnerabilityProfileSO
EnemyBestiaryEntrySO
```

Fluxo desejado:

```text
1. Criar EnemyFactionSO ou reutilizar faction existente.
2. Criar/reutilizar EnemyMovementProfileSO.
3. Criar/reutilizar EnemyActionSO para habilidades.
4. Agrupar habilidades em EnemyActionSetSO.
5. Criar EnemySizeProfileSO.
6. Criar EnemyVulnerabilityProfileSO.
7. Criar EnemyDataSO apontando para esses assets.
8. Adicionar EnemyDataSO a EnemyDatabaseSO/roster registry.
9. Adicionar spawn rules em EnemySpawnProfileSO.
10. Validar no test arena/cave level.
```

Regras:

- Criar novo monstro nao deve exigir novo script C# na maioria dos casos.
- Novas habilidades devem preferir `EnemyActionSO` parametrizado antes de criar classes novas.
- Balance, sprite, loot e spawn devem ficar em dados.
- Runtime deve resolver por IDs estaveis.

## EnemyDataSO minimo

Campos minimos:

```text
EnemyId
DisplayName
LoreTagline
FactionId
PrimaryRole
SecondaryRoles[]
CaveBand
BiomeTags[]
EnvironmentTags[]
SizeProfileId
MovementProfileId
ActionSetId
VulnerabilityProfileId
CombatResistanceProfileId
LootTableId
XPReward
DifficultyTier
IsElite
IsMiniBoss
IsBoss
BestiaryEntryId
SpriteId ou VisualProfileId opcional
```

Nao serializar ScriptableObject em save; save usa IDs.

## Roles oficiais

Roles que devem existir no sistema:

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

Regras:

- Todos os roles devem ser suportados em dados.
- Pelo menos um monstro de cada role deve existir no roster de 40.
- Pelo menos 8 inimigos devem ser testaveis em runtime MVP.
- Pelo menos 5 roles devem ter comportamento funcional em runtime logo no MVP.
- Monstros podem ter multiplos roles, por exemplo `Tank + Guard`, `Caster + Elite`, `Swarm + Chaser`.
- Boss/MiniBoss podem ficar como data/hook nesta spec se boss fight final depender da spec 14/15, mas precisam estar no roster.

## EnemyBrain

MVP usa state machine simples. Behavior tree fica futuro.

Estados minimos:

```text
Idle
Patrol
Alert
Chase
AttackWindup
AttackRecover
Stunned
Dead
```

Estados opcionais conforme role:

```text
GuardHold
Kite
Burrow
SwarmGroup
Retreat
CastPrepare
```

Regras:

- EnemyBrain escolhe acao por role, distancia, cooldown, linha de visao e estado do alvo.
- Nao fazer busca cara por frame.
- Usar decision tick configuravel, por exemplo 0.2s a 0.5s.
- Target principal MVP: player.
- Companions futuros ficam hook.
- Ao morrer, parar movimento, AI, status ticking se aplicavel, e publicar evento.

## Movement profiles

Aplicar todos exceto Flying.

Movement profiles:

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

Fora do MVP:

```text
Flying
```

Campos minimos de `EnemyMovementProfileSO`:

```text
MovementProfileId
MoveSpeed
Acceleration opcional
DetectionRange
LeashRange
AttackRange
PreferredDistance
WanderRadius
CanBurrow
CanPhaseShortBlink
CanLeap
CanFly = false no MVP
DecisionTickSeconds
```

Regras:

- Flying nao deve ser implementado agora.
- Burrow pode ser implementado como disappear/reappear dentro de nav area, sem pathfinding subterraneo completo.
- PhaseShortBlink e teleport curto validado contra colisao/nav area.
- Leaper move por salto curto sem atravessar bloqueios.
- SwarmErratic faz pequenas variacoes de direcao, sem custo alto.

## Enemy actions / habilidades

Criar/usar:

```text
EnemyActionSO
EnemyActionSetSO
EnemyActionRuntime
```

Tipos de action MVP:

```text
MeleeAttack
RangedProjectile
CastProjectile
AreaPulse
SelfBuff
SummonMinion hook futuro
BurrowStrike
LeapStrike
```

Campos minimos de `EnemyActionSO`:

```text
ActionId
DisplayName
ActionType
DamageType
BaseDamage
Range
CooldownSeconds
WindupSeconds
RecoverSeconds
ProjectileSpeed opcional
AreaRadius opcional
StatusApplicationRules[] opcional
VulnerabilityWindowTrigger opcional
TelegraphProfileId
```

Regras:

- Toda action que causa dano cria `DamageRequest` da spec 11.
- Status usa `StatusApplicationRules` da spec 11.
- Habilidades devem ser originais, inspiradas em Vaalara e arquetipos de fantasia/tabletop, sem copiar texto/estatistica de livros.
- Nenhuma action deve aplicar dano antes de terminar o windup.
- Se alvo sair do range/linha valida, action pode falhar ou aplicar conforme regra do action data.

## Telegraph visual

Telegraph MVP:

```text
O monstro pisca/muda cor durante AttackWindup.
```

Criar/usar:

```text
EnemyTelegraphProfileSO
EnemyTelegraphController
```

Campos:

```text
TelegraphProfileId
BlinkColor
BlinkFrequency
WindupSeconds
```

Regras:

- Telegraph nao deve exigir sprite novo.
- Usar material tint, SpriteRenderer color ou mecanismo equivalente.
- Ao entrar em `AttackWindup`, iniciar piscada.
- Ao resolver/cancelar ataque, parar piscada e restaurar cor original.
- Telegraph deve ser visivel, mas polish final fica para spec 17.

## Vulnerability window por monstro

Todo monstro deve ter `EnemyVulnerabilityProfileSO`.

Campos minimos:

```text
VulnerabilityProfileId
WindowDurationSeconds
TriggerMode
Multiplier default 1.5
CooldownSeconds
TelegraphCue opcional
```

Trigger modes MVP:

```text
AfterAttackRecover
DuringChargeWindup
AfterBurrowEmerges
AfterCast
AlwaysForTest opcional debug
```

Regras:

- Toda criatura tem vulnerability window configurada.
- A window deve integrar com `TargetVulnerabilityState` da spec 11.
- Vulnerability e global para o dano direto recebido durante a janela.
- Bosses/Elites podem ter janelas menores ou condicionais.
- Bestiary pode registrar que o jogador descobriu a window se atacar durante ela.

## Tamanhos no mapa

Criar/usar `EnemySizeProfileSO`.

Size classes:

```text
Tiny
Small
Medium
Large
Huge
Boss
```

Campos:

```text
SizeClass
SpriteScale
ColliderRadiusOrBounds
FootprintCells
TargetingOffset
DamageNumberOffset
KnockbackMultiplier
PathingRadius
```

Regras:

- Tamanho visual deve ser configuravel sem alterar PPU/import settings do sprite.
- Preferir `Transform.localScale`/visual child scale + collider/pathing ajustados por size profile.
- Collider e footprint devem acompanhar tamanho para nao gerar hitbox injusta.
- Damage numbers devem usar `DamageNumberOffset` por tamanho.
- Bosses/Huge nao devem nascer em salas pequenas; spawn profile deve validar area minima.
- Placeholder sprites podem ser escalados ate sprites finais existirem.

## Factions

Factions tecnicas MVP:

```text
faction_beast
faction_fungal
faction_undead
faction_cultist
faction_elemental
faction_construct
faction_abyssal
faction_corrupted
```

Regras:

- Faction controla spawn pools, bestiary grouping, ecologia e eventuais faction locks.
- Faction nao e relationship/friendship.
- Faction lock pode impedir spawn antes de boss gate/story flag.

## Cave bands / biomas

A caverna base do GDD possui 100 niveis e biomas:

```text
1-10: Caverna de pedra
11-25: Floresta subterranea
26-40: Caverna de gelo
41-55: Caverna de fogo
56-70: Ruinas antigas
71-85: Abismo sombrio
86-99: Nucleo corrompido
100: Boss final/lore futura
```

Specs/runtime recentes tambem usam boss gates/checkpoints em:

```text
15, 30, 45, 60, 75, 90
```

Regra desta spec:

- Enemy spawn profiles devem ser data-driven e aceitar ambos os modelos de band/gate.
- Nao hardcodar somente um mapa de boss gate.
- A spec 14 sera dona da reconciliação final do cave runtime/gates.
- Esta spec deve fornecer dados de spawn por ranges e tags.

Campos de spawn:

```text
CaveLevelMin
CaveLevelMax
BiomeTags[]
EnvironmentTags[]
FactionLockId opcional
RequiredBossGateProgress opcional
Weight
MaxCountPerRoom
CanSpawnAsElite
MinimumRoomSizeForSizeClass
```

## Roster oficial de 40 inimigos

Regra:

- Criar os 40 como `EnemyDataSO` oficiais.
- Nomes abaixo sao nomes de jogo originais/tecnicos, nao cópia de statblocks.
- Abilities sao descritas em nivel de design e devem virar `EnemyActionSO` parametrizados.
- XP usa valor existente se o repo ja tiver; fallback abaixo e provisório.

### Band 1 — Caverna de pedra / tutorial

| EnemyId | Nome | Faction | Roles | Movement | Size | Damage/Status | Habilidades principais | XP fallback |
|---|---|---|---|---|---|---|---|---:|
| enemy_cave_mite | Acari da Fenda | beast | Swarm, Chaser | SwarmErratic | Tiny | Physical | mordida curta, enxame | 4 |
| enemy_stone_rat | Rato de Basalto | beast | Chaser | GroundChase | Small | Physical | investida curta | 5 |
| enemy_mossling | Musguinho Errante | fungal | Guard, Tank | GroundPatrol | Small | Toxic | esporo fraco | 6 |
| enemy_cracked_bone | Osso Rachado | undead | Chaser | GroundChase | Medium | Physical | golpe osseo | 8 |
| enemy_candle_wisp | Lampejo de Cera | elemental | Caster | CasterKeepAway | Small | Fire | fagulha lenta | 9 |
| enemy_rust_crawler | Rastejante Ferrugem | construct | Tank, Guard | TankSlowPush | Medium | Physical | pancada lenta | 10 |
| enemy_blackroot_sprout | Broto Raiz-Negra | fungal | Ranged, Guard | GuardStationary | Small | Toxic | espinho de raiz | 8 |
| enemy_gate_pebblekin | Pedrinho do Umbral | construct | Guard, Tank | GuardStationary | Medium | Physical | bloqueio de passagem | 12 |

### Band 2 — Floresta subterranea

| EnemyId | Nome | Faction | Roles | Movement | Size | Damage/Status | Habilidades principais | XP fallback |
|---|---|---|---|---|---|---|---|---:|
| enemy_spore_imp | Diabrete de Esporo | fungal | Caster, Swarm | SwarmErratic | Small | Toxic/Poison | nuvem de esporos | 16 |
| enemy_rootsnare | Garra-Raiz | fungal | Guard, Burrower | BurrowAmbush | Medium | Physical/Slow | emerge e prende | 18 |
| enemy_hollow_stagling | Cervino Oco | beast | Chaser, Leaper | Leaper | Medium | Physical | salto de chifre | 20 |
| enemy_thorn_archer | Espinhador Sombrio | cultist | Ranged | KiteRanged | Medium | Physical/Bleed | flecha de espinho | 22 |
| enemy_mycobulwark | Baluarte Micelio | fungal | Tank, Guard | TankSlowPush | Large | Toxic | explosao de esporo curta | 26 |
| enemy_ivy_cultist | Cultista de Hera | cultist | Caster, Ranged | CasterKeepAway | Medium | Toxic | rito de vinhas | 24 |
| enemy_burrow_beetle | Besouro de Toca | beast | Burrower, Tank | BurrowAmbush | Medium | Physical | mordida subterranea | 24 |
| enemy_nyx_moth | Mariposa de Nyx | abyssal | Caster, Swarm | SwarmErratic | Small | Arcane/Slow | poeira lunar | 28 |

### Band 3 — Gelo

| EnemyId | Nome | Faction | Roles | Movement | Size | Damage/Status | Habilidades principais | XP fallback |
|---|---|---|---|---|---|---|---|---:|
| enemy_frost_gnawer | Roedor de Geada | beast | Chaser | GroundChase | Small | Ice | mordida fria | 34 |
| enemy_icebound_sentinel | Sentinela Enregelado | construct | Guard, Tank | GuardStationary | Large | Ice/Physical | escudo glacial | 42 |
| enemy_glassbone | Osso de Vidro | undead | Ranged, Chaser | GroundPatrol | Medium | Ice | estilhaço osseo | 38 |
| enemy_snowcap_fungus | Chapéu-de-Neve | fungal | Caster, Guard | GuardStationary | Medium | Ice/Toxic | esporo gelado | 40 |
| enemy_cold_cult_acolyte | Acólito do Frio | cultist | Caster | CasterKeepAway | Medium | Ice | raio de frio | 44 |
| enemy_crystal_leaper | Saltador Cristalino | elemental | Leaper, Chaser | Leaper | Medium | Ice | salto estilhaçante | 46 |
| enemy_frost_wailer | Lamento Frio | undead | Caster, Elite | CasterKeepAway | Medium | Ice/Slow | grito congelante | 52 |
| enemy_hoarfang | Presa-Alva | beast | Chaser, Elite | GroundChase | Large | Physical/Ice | mordida pesada | 58 |

### Band 4 — Fogo

| EnemyId | Nome | Faction | Roles | Movement | Size | Damage/Status | Habilidades principais | XP fallback |
|---|---|---|---|---|---|---|---|---:|
| enemy_ember_tick | Carrapato de Brasa | beast | Swarm, Chaser | SwarmErratic | Tiny | Fire | explosao curta ao morrer | 54 |
| enemy_ash_crawler | Rastejante de Cinza | elemental | Chaser | GroundChase | Medium | Fire | rastro quente | 60 |
| enemy_lava_bulwark | Baluarte de Lava | elemental | Tank, Guard | TankSlowPush | Large | Fire/Physical | pancada ardente | 72 |
| enemy_cinder_spitter | Cuspidor de Cinza | beast | Ranged | KiteRanged | Medium | Fire | projetil de brasa | 66 |
| enemy_scorched_cultist | Cultista Chamuscado | cultist | Caster | CasterKeepAway | Medium | Fire | selo incendiario | 70 |
| enemy_molten_bone | Osso Fundido | undead | Chaser, Tank | GroundChase | Medium | Fire/Physical | golpe fundido | 74 |
| enemy_flame_imp | Diabrete de Chama | abyssal | Caster, Swarm | SwarmErratic | Small | Fire | bolha de fogo | 76 |
| enemy_furnace_warden | Guardiao da Fornalha | construct | Guard, Elite | GuardStationary | Large | Fire | pulso termico | 90 |

### Band 5 — Ruinas antigas

| EnemyId | Nome | Faction | Roles | Movement | Size | Damage/Status | Habilidades principais | XP fallback |
|---|---|---|---|---|---|---|---|---:|
| enemy_rune_shard | Lasca Rúnica | construct | Swarm, Ranged | SwarmErratic | Small | Arcane | disparo rúnico | 82 |
| enemy_clockwork_guard | Guarda de Corda | construct | Guard, Tank | GuardStationary | Medium | Physical | golpe ritmado | 94 |
| enemy_relic_thief | Ladrão de Relíquia | cultist | Chaser, Ranged | KiteRanged | Medium | Physical/Bleed | arremesso de lâmina | 90 |
| enemy_archive_wisp | Sopro de Arquivo | elemental | Caster | CasterKeepAway | Small | Arcane | pulso de memoria | 96 |
| enemy_sealed_knight | Cavaleiro Selado | undead | Tank, Elite | TankSlowPush | Large | Physical/Arcane | corte selado | 110 |
| enemy_mirror_adept | Adepto do Espelho | cultist | Caster, Phase | PhaseShortBlink | Medium | Arcane | deslocamento curto | 108 |
| enemy_puzzle_golem | Golem de Enigma | construct | Tank, Guard | TankSlowPush | Large | Physical | zona de pressão | 120 |
| enemy_oathless_shade | Sombra Sem-Juramento | undead | Phase, Caster | PhaseShortBlink | Medium | Arcane/Slow | passo sombrio | 126 |

### Band 6 — Abismo sombrio / nucleo corrompido / bosses hooks

| EnemyId | Nome | Faction | Roles | Movement | Size | Damage/Status | Habilidades principais | XP fallback |
|---|---|---|---|---|---|---|---|---:|
| enemy_nyxling_pack | Ninhada de Nyx | abyssal | Swarm, Chaser | SwarmErratic | Small | Arcane | mordida em bando | 140 |
| enemy_void_caster | Conjurador do Vazio | abyssal | Caster, Elite | CasterKeepAway | Medium | Arcane | orbe vazio | 160 |
| enemy_corrupt_hulk | Massa Corrompida | corrupted | Tank, Chaser | TankSlowPush | Huge | Toxic/Physical | esmagar contaminado | 180 |
| enemy_black_meteor_spawn | Cria do Meteoro Negro | corrupted | Burrower, Elite | BurrowAmbush | Large | Toxic/Arcane | emergir corrompido | 190 |
| enemy_anya_echo | Eco de Anya | undead | Caster, MiniBoss | PhaseShortBlink | Medium | Arcane | lamento de cura invertida | 240 |
| enemy_gate_colossus_15 | Colosso do Primeiro Portao | construct | Boss, Tank, Guard | TankSlowPush | Boss | Physical | batida de portao | 300 |
| enemy_gate_colossus_30 | Colosso do Segundo Portao | elemental | Boss, Caster, Tank | TankSlowPush | Boss | Ice | pulso glacial | 450 |
| enemy_gate_colossus_45 | Colosso do Terceiro Portao | elemental | Boss, Caster, Tank | TankSlowPush | Boss | Fire | onda de calor | 600 |

Observacao:

- A tabela tem 48 entries para permitir os 40 obrigatorios + hooks de bosses/gates.
- Implementacao deve criar pelo menos 40. Entries extras podem ser criadas se viavel.
- Se o projeto exigir exatamente 40 na primeira entrega, priorizar os 40 primeiros e manter boss hooks como backlog da spec 14/15.

## XP

Regra principal:

1. Procurar no repo por XP ja definido em `EnemyDataSO`, initializers, docs implementados ou dados existentes.
2. Se existir XP definido para algum EnemyId, preservar.
3. Se nao existir, usar `XPReward` fallback da tabela acima.
4. Marcar fallback como balance provisório em dados/comentario/log.
5. Nao inventar formula complexa de XP nesta spec.

Campos:

```text
XPReward
DifficultyTier
XPMultiplier opcional futuro
```

## Bestiary

Bestiary persistente:

```text
BestiarySaveData
- Entries[]

BestiaryEntrySaveData
- EnemyId
- FirstSeen
- KillCount
- DropsDiscovered[]
- WeaknessesDiscovered[]
- ResistancesDiscovered[]
- VulnerabilityWindowDiscovered
- LastSeenCaveLevel
```

Regras:

- `FirstSeen` registra quando inimigo e visto/spawnado em range revelado pela primeira vez.
- `KillCount` incrementa em morte.
- Drops descobertos registram `ItemId` quando dropa pela primeira vez daquele EnemyId.
- Weakness/resistance discovered registra quando jogador causa dano e a spec 11 reporta Weak/Resistant/Immune.
- VulnerabilityWindowDiscovered registra quando jogador acerta inimigo durante a vulnerability window.
- Bestiary save usa IDs e tipos simples.

## Faction locks / ecologia

Spawn deve respeitar:

```text
Cave level range
BiomeTag
EnvironmentTag
FactionLockId
BossGateProgress
Rarity/Weight
Room size / SizeClass
MaxCountPerRoom
```

Regras:

- Beast/Fungal aparecem mais em bands organicas.
- Undead/Cultist aparecem em ruinas/abismo/story bands.
- Elemental aparece em gelo/fogo/zonas ambientais.
- Construct aparece em ruinas/gates.
- Abyssal/Corrupted aparece em abismo/nucleo/gates.
- Faction lock pode liberar pool apos boss gate ou flag futura.
- Spec 14 decide aplicacao final na cave generation; esta spec fornece contratos e dados.

## Spawn e runtime de inimigos

Criar/usar:

```text
EnemySpawnProfileSO
EnemySpawnResolver
EnemySpawnRequest
EnemySpawnResult
```

Regras:

- SpawnResolver recebe cave level, biome tags, environment tags, boss gate progress e room metadata.
- Resolve lista ponderada de EnemyDataSO validos.
- Nao spawnar Huge/Boss em sala pequena.
- Nao exceder MaxCountPerRoom.
- Runtime de spawn completo em cave fica para spec 14, mas resolver/data entram aqui.

## Save/load

Persistir agora:

```text
BestiarySaveData
```

Preparar contrato, sem prometer completo ate spec 14:

```text
EnemyRuntimeSaveData
- EnemyRuntimeId
- EnemyId
- CurrentHp
- Position
- ActiveStatuses[]
- IsDead
```

Regras:

- Bestiary deve persistir agora.
- Enemy runtime individual so persiste se cave snapshot/save ja existir.
- Se spec 14 ainda nao implementou snapshot, registrar pendencia e nao marcar como completo.
- Nunca serializar EnemyDataSO, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.

## Eventos

Usar/criar eventos oficiais:

```text
EnemySpawnedEvent
EnemySeenEvent
EnemyDamagedEvent
EnemyKilledEvent
EnemyDespawnedEvent
EnemyActionStartedEvent
EnemyActionResolvedEvent
EnemyTelegraphStartedEvent
EnemyTelegraphEndedEvent
BestiaryEntryUpdatedEvent
EnemyXPGrantedEvent
EnemyLootRolledEvent
```

Regras:

- Enemy damage/death deve integrar com eventos da spec 11.
- Loot usa LootTableSO da spec 10.
- XP nao deve ser concedido duas vezes para a mesma morte.

## UI minima

- Bestiary UI final fica para spec 17.
- Nesta spec pode haver debug/minimal bestiary panel se ja existir UI segura.
- Damage numbers sao da spec 11.
- Telegraph e blink sao runtime visual minimo, nao UI modal.

## Invariantes anti-regressao

Esta spec nao pode quebrar:

- DamageCalculator/damage numbers/status da spec 11;
- player combat da spec 12;
- LootTableSO/equipment instances da spec 10;
- stamina/time pause da spec 09;
- cave generation/runtime futuro da spec 14;
- boss gates/checkpoints futuros da spec 14/15;
- save migration e DTOs simples;
- GameEventBus;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

Se dados antigos de XP/monstros forem encontrados, migrar sem perda. Se contradisserem esta spec, registrar divergencia e preservar comportamento ja implementado ate decisao explicita.

## Criterios de aceite

- Existe pipeline data-driven para criar novos monstros.
- Existem pelo menos 40 `EnemyDataSO` oficiais.
- Roster inclui roles: Chaser, Guard, Ranged, Caster, Burrower, Swarm, Tank, Elite, MiniBoss, Boss.
- Monstros podem ter multiplos roles.
- Todos os monstros possuem vulnerability window.
- Todos os monstros possuem size profile.
- Movement profiles existem para todos os tipos aprovados, exceto Flying.
- Telegraph por piscada/cor funciona no AttackWindup.
- EnemyBrain usa state machine simples.
- Pelo menos 8 inimigos sao testaveis em runtime.
- Pelo menos 5 roles funcionam em runtime MVP.
- Ataques de inimigos usam DamageCalculator da spec 11.
- XPReward e resgatado do repo se existir; caso contrario, fallback da spec e usado e marcado como provisório.
- Bestiary persiste FirstSeen, KillCount, DropsDiscovered, Weaknesses/Resistances e VulnerabilityWindowDiscovered.
- Spawn resolver respeita cave band, faction, biome/environment, boss gate progress e size constraints.
- Loot vem de LootTableSO.
- Enemy runtime save completo fica preparado, mas nao e marcado como completo se spec 14 ainda nao suportar snapshot.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/UI/Bestiary/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Enemies/**
Assets/_Game/Data/Bestiary/**
Assets/_Game/Data/Loot/**
```

Managers/bridges Unity devem ser finos. EnemyBrain, decision scoring, spawn resolver e bestiary update devem ficar em classes testaveis quando possivel.

## Ordem segura de implementacao

1. Revalidar EnemyDataSO, EnemyHealth, Cave spawn atual, XP/drop e eventos existentes.
2. Confirmar specs 02-12 implementadas antes de runtime.
3. Resgatar XP/roster/movement lists existentes se houver.
4. Criar dados modulares: factions, movement profiles, size profiles, vulnerability profiles, actions e action sets.
5. Criar 40 EnemyDataSO.
6. Implementar EnemyBrain state machine.
7. Implementar telegraph blink/color.
8. Implementar vulnerability window por inimigo.
9. Implementar spawn resolver data-driven.
10. Implementar bestiary runtime/save.
11. Integrar XP e loot.
12. Validar roles/movement/tamanho/telegraph em Play Mode.
13. Atualizar tracking documental.

## Fluxos

### Spawn resolver

```text
Receber cave level + biome/environment + boss gate progress + room metadata
Filtrar EnemySpawnProfileSO por range/tags/faction locks/room size
Aplicar weights
Selecionar EnemyDataSO validos
Retornar spawn result
```

### EnemyBrain

```text
Idle/Patrol
Detecta player
Alert/Chase
Seleciona EnemyActionSO por range/cooldown/role
AttackWindup -> telegraph blink
Resolve action via DamageCalculator
AttackRecover
Retorna chase/patrol/dead
```

### Bestiary

```text
EnemySpawned/Seen -> FirstSeen
DamageResult -> Weakness/Resistance/Vulnerability discovery
EnemyKilled -> KillCount + XP + loot discovered
Save/load -> persistir por EnemyId
```

## Riscos de regressao

- 40 inimigos virarem dados incompletos ou inconsistentes.
- Roster copiar statblocks externos em vez de usar designs originais.
- EnemyBrain usar busca cara por frame.
- Telegraph modificar cor e nao restaurar.
- Tamanho visual nao bater com collider/pathing.
- Boss/Huge spawnar em sala pequena.
- XP ser duplicado em morte.
- Bestiary salvar referencia Unity.
- Spawn resolver invadir spec 14 com cave generation final.

## Mitigacao

- ScriptableObjects modulares.
- Nomes/designs originais inspirados em arquetipos, sem copia textual.
- Decision tick configuravel.
- Guardar/restaurar cor original no telegraph controller.
- SizeProfile controla visual/collider/pathing/damage number offset.
- Spawn valida room size.
- EnemyKilled idempotente por runtime id.
- Save DTO com IDs simples.
- Spec 14 continua dona do runtime final da cave.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar EnemyDataSO, EnemyHealth, Cave spawn, XP/drop e eventos existentes.
- [ ] Confirmar specs 02-12 implementadas antes de runtime.
- [ ] Resgatar XP/roster/movement lists existentes se houver.
- [ ] Criar/ajustar EnemyFactionSO.
- [ ] Criar/ajustar EnemyMovementProfileSO com todos exceto Flying.
- [ ] Criar/ajustar EnemySizeProfileSO.
- [ ] Criar/ajustar EnemyVulnerabilityProfileSO.
- [ ] Criar/ajustar EnemyActionSO e EnemyActionSetSO.
- [ ] Criar/ajustar EnemyTelegraphProfileSO e telegraph blink/color.
- [ ] Criar 40 EnemyDataSO oficiais.
- [ ] Criar EnemyDatabaseSO/registry.
- [ ] Implementar EnemyBrain state machine.
- [ ] Implementar pelo menos 8 inimigos testaveis em runtime.
- [ ] Implementar todos os roles em dados e pelo menos 5 roles funcionais em runtime.
- [ ] Implementar SpawnResolver respeitando band/faction/biome/environment/boss gate/size.
- [ ] Implementar Bestiary runtime/save.
- [ ] Integrar XPReward com EnemyKilled sem duplicar XP.
- [ ] Integrar LootTableSO para enemy drops.
- [ ] Preparar EnemyRuntimeSaveData sem marcar snapshot completo se spec 14 nao suportar.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/UI/Bestiary/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Enemies/**
Assets/_Game/Data/Bestiary/**
Assets/_Game/Data/Loot/**
docs/specs/**
docs/refinements/**
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Arquivos proibidos

```text
docs_old/**
Packages/**
ProjectSettings/**
```

## Definition of Done

- Roster 40+ criado.
- Enemy creation pipeline data-driven documentado e funcional.
- EnemyBrain/roles/movement/telegraph/vulnerability/tamanho funcionando no MVP.
- Bestiary e XP/loot integrados.
- Spawn resolver pronto para a cave spec 14.
- Invariantes anti-regressao preservadas.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

1. Validar que 40 EnemyDataSO existem e carregam sem Missing Script.
2. Spawnar/testar pelo menos 8 inimigos diferentes.
3. Validar roles Chaser, Guard, Ranged, Caster, Burrower, Swarm, Tank, Elite, MiniBoss, Boss em dados.
4. Validar pelo menos 5 roles funcionando em runtime.
5. Validar GroundChase, GuardStationary, KiteRanged, CasterKeepAway, BurrowAmbush, SwarmErratic, TankSlowPush, PhaseShortBlink, Leaper.
6. Confirmar que Flying nao foi implementado como runtime ativo.
7. Validar telegraph blink/cor no AttackWindup e restauracao de cor depois.
8. Validar vulnerability window por monstro e 1.5x via DamageCalculator.
9. Validar tamanhos Tiny/Small/Medium/Large/Huge/Boss com collider/offset coerentes.
10. Matar inimigo e validar XP uma vez, loot e Bestiary KillCount.
11. Causar dano Weak/Resistant/Immune e validar Bestiary discovery.
12. Validar spawn resolver por cave band/faction/biome/environment/boss gate/size.
13. Salvar/carregar Bestiary.
