# Cindar's Hope — Cave Monster Roster ↔ Enemy Behaviors Adapter

> **Status:** documento canônico de ponte entre o roster da caverna e a arquitetura geral de inimigos  
> **Local:** `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md`  
> **Depende de:**  
> - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> **Função:** explicar como interpretar os campos atuais do roster da caverna dentro da nova estrutura geral de EnemyData/EnemyBrain/EnemyAction, sem reescrever nem invalidar o roster existente.  
> **Não é spec implementável.** Specs futuras devem usar este documento como ponte de leitura antes de converter o roster em ScriptableObjects/dados runtime.

---

## 0. Por que este adapter existe

O `CAVE_MONSTER_ROSTER_DIRECTION.md` foi criado antes de `ENEMY_BEHAVIORS_DIRECTION.md`.

Por isso, o roster da caverna mistura três coisas:

```text
1. dados concretos de monstros da caverna;
2. taxonomia geral de Move, Behavior e Trait;
3. direção de IA/comportamento que agora pertence ao documento geral de enemies.
```

Decisão atual:

```text
Não reescrever o roster inteiro agora.
Não mover a lista de monstros para outro arquivo agora.
Não alterar stats, drops, packs, bosses, scaling ou nomes de criaturas agora.
```

Em vez disso:

```text
ENEMY_BEHAVIORS_DIRECTION.md vira fonte canônica da taxonomia geral.
CAVE_MONSTER_ROSTER_DIRECTION.md continua fonte canônica das criaturas concretas da caverna.
Este adapter explica como mapear um para o outro.
```

---

# PARTE A — Autoridade dos documentos

## 1. Quem vence em caso de conflito

```text
Stats concretos de monstro da caverna:
  CAVE_MONSTER_ROSTER_DIRECTION.md vence.

Drops, XP, packs, boss gates e scaling da caverna:
  CAVE_MONSTER_ROSTER_DIRECTION.md vence.

Significado geral de Move, Behavior, Trait, EnemyAction, EnemyBrain e módulos injetáveis:
  ENEMY_BEHAVIORS_DIRECTION.md vence.

Vulnerabilidades, janelas, TTK, critical windows e active combat budget:
  CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md vence para caverna.

Inputs, Dash, Dodge, Block, movimento do jogador, Combat HUD e feeling geral:
  COMBAT_CORE_DIRECTION.md vence.
```

Regra:

```text
O roster não deve ser usado como fonte primária para criar nova taxonomia geral.
O roster deve ser usado como catálogo autorado de criaturas concretas.
```

---

# PARTE B — Mapeamento dos campos atuais do roster

## 2. Entrada textual atual

Formato atual do roster:

```text
enemy_id | Nome
Nível nativo: X-Y | Size ... | Behavior ... | Move ...
HP ... | MP ... | STA ... | FOR ... | CON ... | DES ... | INT ... | VON ... | CAR ... | XP ...
Traits: ...
Ataques: ...
Drops: ...
```

## 3. Mapeamento para dados futuros

| Campo atual | Interpretação futura |
|---|---|
| `enemy_id` | `EnemyDataSO.Id` |
| `Nome` | `EnemyDataSO.DisplayName` / `BestiaryEntry.Name` |
| `Nível nativo` | `EnemySpawnProfileSO.NativeMinLevel` / `NativeMaxLevel` |
| `Size` | `EnemyDataSO.SizeClass` |
| `Behavior` | `EnemyBrainProfileSO.PrimaryRole` + `EnemyBehaviorProfileSO.BehaviorProfiles` |
| `Move` | `EnemyMovementProfileSO.MoveIds` + movement overrides |
| `HP/MP/STA` | `EnemyDataSO.BaseResources` |
| `FOR/CON/DES/INT/VON/CAR` | `EnemyDataSO.BaseAttributes` |
| `XP` | `EnemyRewardProfileSO.XP` |
| `Traits` | `EnemyDataSO.Traits` + behavior modifiers |
| `Ataques` | `EnemyActionSetSO.ActionIds` |
| `Drops` | `LootTableSO` / `EnemyDropProfileSO` |
| Pack composition | `EnemySpawnPackSO` |
| Pack motivo | `Bestiary/Ecology/SpawnReason` |

Regra:

```text
Specs futuras devem converter o roster por mapeamento, não reinterpretar manualmente cada campo.
```

---

# PARTE C — Normalização de Behavior

## 4. Campo Behavior no roster é composto

No roster atual, `Behavior` pode conter:

```text
Role tática
BehaviorProfile
Elite/Boss marker
Família comportamental
```

Exemplo:

```text
Behavior Predator
Behavior CasterSupport
Behavior TreasureTrap/Tank
Behavior EliteDuelist/CorruptedFrenzy
Behavior MiniBoss/AberrantController
```

Normalização futura:

```text
Role primária: função tática principal.
BehaviorProfiles: módulos de decisão injetáveis.
Markers: Elite, MiniBoss, Boss ou LoreGuardian quando aplicável.
```

## 5. Exemplos de normalização

### Rato de Basalto

```text
Atual:
Behavior Predator | Move GroundChase

Futuro:
PrimaryRole: Chaser
BehaviorProfiles: [Predator]
MovementProfile: GroundChase
```

### Saqueador Grash'naar

```text
Atual:
Behavior Scavenger/FactionPatrol | Move PackFlanker

Futuro:
PrimaryRole: Chaser ou Ranged conforme action set final
BehaviorProfiles: [Scavenger, FactionPatrol]
MovementProfile: PackFlanker
PackRole: Flanker/Scavenger
```

### Cubo de Lodo Translúcido

```text
Atual:
Behavior TreasureTrap/Tank | Move TankSlowPush

Futuro:
PrimaryRole: TreasureTrap
SecondaryRoles: [Tank]
BehaviorProfiles: [TreasureTrap]
MovementProfile: TankSlowPush
Traits: [TankBody, TreasureAmbush, AcidBody, SlowRecovery]
```

### Observador Menor da Ruína

```text
Atual:
Behavior AberrantController/Elite | Move FloatingOrbit/BossArenaControl menor

Futuro:
PrimaryRole: Elite
SecondaryRoles: [Controller, Ranged]
BehaviorProfiles: [AberrantController]
MovementProfiles: [FloatingOrbit, BossArenaControl]
BossLike: false ou MiniBoss conforme spec
```

### Wyvern de Pedra Negra

```text
Atual:
Behavior Boss/DraconicCorruption etc. conforme roster

Futuro:
PrimaryRole: Boss
BehaviorProfiles: [BossMultiPhase, CorruptedFrenzy ou Draconic]
MovementProfiles por fase
EnemyBossPhaseProfileSO obrigatório
```

---

# PARTE D — Normalização de Move

## 6. Move no roster pode conter múltiplos valores

Exemplo:

```text
Move SwarmErratic/FloatingOrbit
Move Leaper/ChargeLine
Move KiteRanged/RetreatAndCall
Move PhaseShortBlink/FloatingSlow
```

Interpretação:

```text
Primeiro Move = movimento principal.
Moves adicionais = movement modes condicionais, por ação, por fase ou por estado.
```

## 7. Mapeamento futuro

```text
EnemyMovementProfileSO.PrimaryMove
EnemyMovementProfileSO.SecondaryMoves
EnemyActionSO.ActionMovementOverride
EnemyBossPhaseProfileSO.PhaseMovementMode
EnemyStateMovementOverride
```

Regra:

```text
Não transformar múltiplos Moves em múltiplos inimigos.
Múltiplos Moves representam variação de estado, ação ou fase.
```

---

# PARTE E — Normalização de Traits

## 8. Traits atuais viram modifiers

No roster, Traits aparecem como lista simples.

Exemplo:

```text
Traits: FloatingBody, PackCoordination, FastRecovery
```

Interpretação futura:

```text
FloatingBody -> altera movimento/collider/footbox e talvez resistência a hazards de chão.
PackCoordination -> habilita PackModule ou PackCoordinationRules.
FastRecovery -> reduz RecoveryDuration ou altera action scoring.
```

## 9. Regra de trait

```text
Trait não é comportamento inteiro.
Trait é modificador de leitura, corpo, resistência, tempo, loot, pack ou telegraph.
BehaviorProfile decide; Trait modifica.
```

---

# PARTE F — Normalização de ataques

## 10. Ataques atuais do roster

O roster usa nomes autorais por criatura:

```text
Bite
DashBite
EchoPulse
ScratchDive
Shiv
StoneThrow
RetreatAndCall
RootNeedle
ShortRoot
AcidContact
EngulfSlow
BlackstoneBeamMinor
SlowGaze
EyeShardVolley
```

Esses nomes não são ActionTypes.

Eles devem virar:

```text
EnemyActionSO.ActionId
```

Cada `ActionId` deve mapear para um `ActionType` geral em `ENEMY_BEHAVIORS_DIRECTION.md`.

Exemplo:

```text
DashBite -> ActionType DashAttack
StoneThrow -> ActionType Projectile
RootNeedle -> ActionType Projectile ou AreaCast conforme spec
ShortRoot -> ActionType DebuffPlayer ou AreaCast
RetreatAndCall -> ActionType Retreat + CallForHelp
EngulfSlow -> ActionType MeleeHeavy + DebuffPlayer
SlowGaze -> ActionType DebuffPlayer / Gaze-style action
EyeShardVolley -> ActionType Projectile
```

Regra:

```text
Não apagar nomes autorais dos ataques.
Eles são bons para bestiário, debug, animação e flavor.
Mas runtime deve conhecer ActionType e parâmetros data-driven.
```

---

# PARTE G — Packs

## 11. Packs continuam na caverna

Packs são específicos de ecologia, sala, facção e bioma.

Portanto, continuam no roster da caverna.

O que vem de `ENEMY_BEHAVIORS_DIRECTION.md`:

```text
PackModule
PackRole
PackCoordinationRules
AllyCallRadius
LeashRules
Active combat budget awareness
```

O que vem do roster:

```text
pack_id
composição
quantidades
motivo ecológico/social/faccional
faixa/bioma provável
boss/miniboss/treasure relation
```

Regra:

```text
A spec deve converter packs em EnemySpawnPackSO, mas usar Enemy Behaviors para coordenação e leash.
```

---

# PARTE H — Bosses

## 12. Bosses continuam no roster/caverna

Bosses da caverna dependem de:

```text
boss gate
andar
lore
ecologia da faixa
arena
recompensa
nível 100/101
```

Por isso continuam no roster e nos documentos de caverna.

O que vem de `ENEMY_BEHAVIORS_DIRECTION.md`:

```text
BossMultiPhase
PhaseModule
EnemyBossPhaseProfileSO
AllowedActions
ForbiddenActions
MovementMode
VulnerabilityWindow
RecoveryRules
AddRules
HazardRules
AntiCheeseRules
PhaseExitCondition
```

---

# PARTE I — Anti-regressão

## 13. O que não muda agora

Esta refatoração não altera:

```text
HP dos monstros
MP dos monstros
STA dos monstros
FOR/CON/DES/INT/VON/CAR
XP
drops
packs
bosses
nível 100
nível 101
active combat budget
vulnerabilidades já definidas
janelas já definidas
nomes de criaturas
nomes autorais de ataques
```

## 14. O que muda conceitualmente

```text
Move deixa de ser taxonomia exclusiva da caverna e vira taxonomia geral de enemies.
Behavior deixa de ser campo monolítico e passa a ser normalizado em Role + BehaviorProfiles.
Traits passam a ser modifiers categorizados.
Ataques autorais passam a mapear para ActionType + EnemyActionSO.
O roster deixa de orientar arquitetura de IA sozinho e passa a ser convertido pela arquitetura geral de Enemy Behaviors.
```

---

# PARTE J — Specs futuras derivadas

```text
spec_cave_monster_roster_to_enemy_data_conversion.md
spec_enemy_action_so_contract.md
spec_enemy_behavior_profiles_runtime.md
spec_enemy_movement_profiles_official_moves.md
spec_enemy_traits_enum_and_modifiers.md
spec_enemy_pack_coordination_leash_rules.md
spec_enemy_boss_phase_ai_contract.md
```

---

# PARTE K — Decisões fechadas

```text
Não mover lista de monstros da caverna agora.
Não reescrever roster grande agora.
Não alterar balance/stats/drops/packs/bosses agora.
Enemy Behaviors centraliza taxonomia geral.
Cave Monster Roster centraliza criaturas concretas da caverna.
Este adapter é a ponte obrigatória para specs que converterem roster em runtime.
Behavior atual do roster deve ser normalizado para Role + BehaviorProfiles.
Move atual do roster deve ser normalizado para EnemyMovementProfileSO.
Traits atuais devem ser normalizadas como modifiers.
Ataques autorais atuais devem virar EnemyActionSO com ActionType geral.
```
