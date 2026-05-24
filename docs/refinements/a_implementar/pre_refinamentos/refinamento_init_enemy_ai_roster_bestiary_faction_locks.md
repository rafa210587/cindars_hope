# refinamento_init_enemy_ai_roster_bestiary_faction_locks

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_enemy_ai_roster_bestiary_faction_locks_runtime.md`
> **Objetivo:** completar IA, roster, bestiÃ¡rio, faction locks, ecologia e XP dos inimigos.

---

## 1. Estado atual

O modelo oficial estabilizado para inimigos Ã©:

```text
Assets/_Game/Scripts/Combat/EnemyDataSO.cs
```

A branch tambÃ©m tem schemas iniciais para AI/bestiary e initializers, mas runtime de IA ainda Ã© parcial/MVP.

---

## 2. Gaps

- NÃ£o hÃ¡ roster completo de 40+ monstros.
- NÃ£o hÃ¡ `EnemyBrain` completo.
- NÃ£o hÃ¡ action scoring/behavior tree/state machine refinada.
- NÃ£o hÃ¡ movement profiles por inimigo.
- NÃ£o hÃ¡ telegraph de ataques.
- Bestiary nÃ£o Ã© event-driven completo.
- Faction locks nÃ£o controlam spawn/ecologia real.
- XP formula existe em parte, mas balance por difficulty/bioma precisa validaÃ§Ã£o.

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

Criar pelo menos 40 definiÃ§Ãµes agrupadas por:

```text
Cave band
Faction
Role
Damage type
Movement profile
Difficulty
Loot table
```

Roles mÃ­nimos:

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

## 4. Arquivos provÃ¡veis

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

- [ ] HÃ¡ roster data-driven com pelo menos 40 inimigos.
- [ ] Enemy spawn respeita cave band/faction/bioma/boss gate.
- [ ] EnemyBrain executa movement/action profiles.
- [ ] Pelo menos 5 roles diferentes existem e funcionam.
- [ ] Bestiary atualiza first seen/kill count/drops.
- [ ] XP e loot vÃªm dos dados oficiais.
- [ ] Unity Play Mode valida spawn/combate sem Missing Script.

---

## 6. ValidaÃ§Ã£o

1. Gerar cave em diferentes nÃ­veis.
2. Validar inimigos por faction/band.
3. Matar inimigo e validar XP/drop/bestiary.
4. Verificar que boss gate muda spawn pool.
5. Testar inimigo ranged/caster/tank/swarm.
