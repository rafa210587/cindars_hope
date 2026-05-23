# refinamento_init_enemy_ai_roster_bestiary_faction_locks

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_enemy_ai_roster_bestiary_faction_locks_runtime.md`  
> **Objetivo:** completar IA, roster, bestiário, faction locks, ecologia e XP dos inimigos.

---

## 1. Estado atual

O modelo oficial estabilizado para inimigos é:

```text
Assets/_Game/Scripts/Combat/EnemyDataSO.cs
```

A branch também tem schemas iniciais para AI/bestiary e initializers, mas runtime de IA ainda é parcial/MVP.

---

## 2. Gaps

- Não há roster completo de 40+ monstros.
- Não há `EnemyBrain` completo.
- Não há action scoring/behavior tree/state machine refinada.
- Não há movement profiles por inimigo.
- Não há telegraph de ataques.
- Bestiary não é event-driven completo.
- Faction locks não controlam spawn/ecologia real.
- XP formula existe em parte, mas balance por difficulty/bioma precisa validação.

---

## 3. Escopo esperado

### Enemy runtime

Criar/evoluir:

```text
EnemyBrain
EnemyActionSO
EnemyActionSetSO
EnemyMovementProfileSO
EnemySpawnProfileSO
EnemyFactionSO
```

### Roster

Criar pelo menos 40 definições agrupadas por:

```text
Cave band
Faction
Role
Damage type
Movement profile
Difficulty
Loot table
```

Roles mínimos:

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

### Bestiary

Registrar:

```text
FirstSeen
KillCount
DropsDiscovered
WeaknessesDiscovered
ResistancesDiscovered
Faction
```

### Faction locks/ecologia

Spawn deve respeitar:

```text
Cave level range
Boss gate progress
Faction lock
Biome/environment
Rarity
```

---

## 4. Arquivos prováveis

```text
Assets/_Game/Scripts/Combat/EnemyDataSO.cs
Assets/_Game/Scripts/Enemy/AIBehaviorSO.cs
Assets/_Game/Scripts/Enemy/BestiaryDataSO.cs
Assets/_Game/Scripts/Cave/Generation/**
Assets/_Game/Scripts/Cave/Runtime/**
Assets/_Game/Scripts/Loot/LootTableSO.cs
Assets/_Game/Scripts/Save/SaveData.cs
```

---

## 5. Definition of Done

- [ ] Há roster data-driven com pelo menos 40 inimigos.
- [ ] Enemy spawn respeita cave band/faction/bioma/boss gate.
- [ ] EnemyBrain executa movement/action profiles.
- [ ] Pelo menos 5 roles diferentes existem e funcionam.
- [ ] Bestiary atualiza first seen/kill count/drops.
- [ ] XP e loot vêm dos dados oficiais.
- [ ] Unity Play Mode valida spawn/combate sem Missing Script.

---

## 6. Validação

1. Gerar cave em diferentes níveis.
2. Validar inimigos por faction/band.
3. Matar inimigo e validar XP/drop/bestiary.
4. Verificar que boss gate muda spawn pool.
5. Testar inimigo ranged/caster/tank/swarm.
