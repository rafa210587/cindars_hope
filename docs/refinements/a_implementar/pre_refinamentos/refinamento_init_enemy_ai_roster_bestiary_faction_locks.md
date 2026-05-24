# refinamento_init_enemy_ai_roster_bestiary_faction_locks

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md`
> Objetivo: completar IA, roster 40+, bestiario, faction locks, ecologia, XP, telegraph, vulnerability windows, tamanhos de inimigos no mapa e pipeline data-driven para criar novos monstros.

---

## 1. Estado atual

O modelo oficial estabilizado para inimigos e:

```text
Assets/_Game/Scripts/Combat/EnemyDataSO.cs
```

O projeto tambem possui skeletons/inicializadores de AI, bestiary, boss gates e cave. O runtime de IA ainda e parcial/MVP.

---

## 2. Gaps

- Nao ha roster completo de 40+ monstros em dados oficiais.
- Nao ha fluxo simples para criar novos monstros sem duplicar codigo.
- EnemyBrain ainda e parcial.
- Movement profiles por inimigo nao estao formalizados.
- Roles de combate nao estao completos.
- Monstros ainda nao possuem vulnerability window individual.
- Habilidades dos monstros nao estao data-driven.
- Telegraph ainda nao e padronizado.
- Bestiary nao e event-driven completo.
- Faction locks/ecologia nao controlam spawn real.
- Tamanhos diferentes de monstros no mapa ainda nao estao formalizados.
- XP pode existir parcialmente, mas precisa ser resgatado/validado ou refeito como fallback.

---

## 3. Decisoes aprovadas

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

---

## 4. Pipeline para criar novos monstros

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

---

## 5. EnemyDataSO minimo

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

---

## 6. Roles oficiais

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

---

## 7. EnemyBrain

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

---

## 8. Movement profiles

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

---

## 9. Enemy actions / habilidades

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

---

## 10. Telegraph visual

Telegraph MVP:

```text
O monstro pisca/muda cor durante AttackWindup.
```

Criar/usar:

```text
EnemyTelegraphProfileSO
EnemyTelegraphController
```

Regras:

- Telegraph nao deve exigir sprite novo.
- Usar material tint, SpriteRenderer color ou mecanismo equivalente.
- Ao entrar em `AttackWindup`, iniciar piscada.
- Ao resolver/cancelar ataque, parar piscada e restaurar cor original.

---

## 11. Vulnerability window por monstro

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

---

## 12. Tamanhos no mapa

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

---

## 13. Factions

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
- Faction lock pode impedir spawn antes de boss gate ou flag futura.

---

## 14. Cave bands / biomas

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

Runtime recente tambem usa boss gates/checkpoints em:

```text
15, 30, 45, 60, 75, 90
```

Regra desta spec:

- Enemy spawn profiles devem ser data-driven e aceitar ambos os modelos de band/gate.
- Nao hardcodar somente um mapa de boss gate.
- A spec 14 sera dona da reconciliacao final do cave runtime/gates.

---

## 15. Roster oficial de 40 inimigos

Regra:

- Criar os 40 como `EnemyDataSO` oficiais.
- Nomes devem ser originais/tecnicos, nao copia de statblocks.
- Abilities sao descritas em nivel de design e devem virar `EnemyActionSO` parametrizados.
- XP usa valor existente se o repo ja tiver; fallback abaixo e provisório.

Resumo por band:

```text
Band 1: 8 inimigos de tutorial/pedra
Band 2: 8 inimigos de floresta subterranea
Band 3: 8 inimigos de gelo
Band 4: 8 inimigos de fogo
Band 5: 8 inimigos de ruinas antigas
Band 6: hooks de abismo/nucleo/bosses
```

A tabela completa vive na spec 13 e deve ser a fonte de implementacao.

---

## 16. XP

Regra principal:

1. Procurar no repo por XP ja definido em `EnemyDataSO`, initializers, docs implementados ou dados existentes.
2. Se existir XP definido para algum EnemyId, preservar.
3. Se nao existir, usar `XPReward` fallback da tabela da spec.
4. Marcar fallback como balance provisório em dados/comentario/log.
5. Nao inventar formula complexa de XP nesta spec.

---

## 17. Bestiary

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

---

## 18. Spawn e faction locks

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

---

## 19. Save/load

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

---

## 20. Definition of Done

- [ ] Existe pipeline data-driven para criar novos monstros.
- [ ] Existem pelo menos 40 `EnemyDataSO` oficiais.
- [ ] Roster inclui roles: Chaser, Guard, Ranged, Caster, Burrower, Swarm, Tank, Elite, MiniBoss, Boss.
- [ ] Monstros podem ter multiplos roles.
- [ ] Todos os monstros possuem vulnerability window.
- [ ] Todos os monstros possuem size profile.
- [ ] Movement profiles existem para todos os tipos aprovados, exceto Flying.
- [ ] Telegraph por piscada/cor funciona no AttackWindup.
- [ ] EnemyBrain usa state machine simples.
- [ ] Pelo menos 8 inimigos sao testaveis em runtime.
- [ ] Pelo menos 5 roles funcionam em runtime MVP.
- [ ] Ataques de inimigos usam DamageCalculator da spec 11.
- [ ] XPReward e resgatado do repo se existir; caso contrario, fallback da spec e usado e marcado como provisório.
- [ ] Bestiary persiste FirstSeen, KillCount, DropsDiscovered, Weaknesses/Resistances e VulnerabilityWindowDiscovered.
- [ ] Spawn resolver respeita cave band, faction, biome/environment, boss gate progress e size constraints.
- [ ] Loot vem de LootTableSO.
- [ ] Enemy runtime save completo fica preparado, mas nao e marcado como completo se spec 14 ainda nao suportar snapshot.

---

## 21. Validacao

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
14. Validar Unity compile validation e docs validation.
