# Enemy Roster Template — SPEC 13

**Status:** 5 inimigos exemplo criados, 35 pendentes para refinamento

---

## 5 Inimigos Exemplo (Band 1 — Caverna de Pedra)

### 1. enemy_cave_mite (Acari da Fenda)
- **Faction:** beast
- **Role:** Swarm, Chaser
- **Size:** Tiny
- **Movement:** SwarmErratic
- **HP:** 4
- **XP:** 4
- **Loot:** loot_cave_mite
- **Status:** ✅ Criado em Assets/_Game/Data/Enemies/enemy_cave_mite.asset

### 2. enemy_stone_rat (Rato de Basalto)
- **Faction:** beast
- **Role:** Chaser
- **Size:** Small
- **Movement:** GroundChase
- **HP:** 8
- **XP:** 5
- **Loot:** loot_stone_rat
- **Status:** ⏳ Pendente

### 3. enemy_mossling (Musguinho Errante)
- **Faction:** fungal
- **Role:** Guard, Tank
- **Size:** Small
- **Movement:** GroundPatrol
- **HP:** 10
- **XP:** 6
- **Loot:** loot_mossling
- **Status:** ⏳ Pendente

### 4. enemy_cracked_bone (Osso Rachado)
- **Faction:** undead
- **Role:** Chaser
- **Size:** Medium
- **Movement:** GroundChase
- **HP:** 12
- **XP:** 8
- **Loot:** loot_cracked_bone
- **Status:** ⏳ Pendente

### 5. enemy_candle_wisp (Lampejo de Cera)
- **Faction:** elemental
- **Role:** Caster
- **Size:** Small
- **Movement:** CasterKeepAway
- **HP:** 6
- **XP:** 9
- **Loot:** loot_candle_wisp
- **Status:** ⏳ Pendente

---

## Template para Criar Inimigos (use como guia)

```yaml
# Assets/_Game/Data/Enemies/enemy_[ID].asset
enemyId: enemy_[ID]
DisplayName: "[Nome em Português]"
Description: "[Descrição lore Vaalara-inspired]"
FactionId: faction_[beast|fungal|undead|cultist|elemental|construct|abyssal|corrupted]
PrimaryRole: [Chaser|Guard|Ranged|Caster|Burrower|Swarm|Tank|Elite|MiniBoss|Boss]
SecondaryRoles: [optional list of roles]
CaveBand: [1-7]
BiomeTags: [stone_cavern|underground_forest|ice_cave|fire_cave|ancient_ruins|shadow_abyss|corrupted_core]
EnvironmentTags: [underground|water|lava|ice|toxic]
SizeProfileId: size_[tiny|small|medium|large|huge|boss]
MovementProfileId: movement_[GroundChase|GroundPatrol|GuardStationary|KiteRanged|CasterKeepAway|BurrowAmbush|SwarmErratic|TankSlowPush|PhaseShortBlink|Leaper]
ActionSetId: actionset_[combat_actions]
VulnerabilityProfileId: vuln_[profile_type]
maxHp: [valor]
contactDamage: [valor]
xpReward: [valor]
lootTableId: loot_[ID]
```

---

## Próximos Passos

1. ✅ 5 exemplos template criado
2. ⏳ **Refinamento do usuário:** Rafa refina os 5 exemplos + cria os 35 restantes
3. ⏳ **EnemyBrain:** Implementar state machine
4. ⏳ **Telegraph:** Implementar blink/color
5. ⏳ **Bestiary:** Sistema persistente
6. ⏳ **Validações:** Compile + anti-regressão

---

## Factions Disponíveis (criar em Assets/_Game/Data/Enemies/Factions/)

- faction_beast
- faction_fungal
- faction_undead
- faction_cultist
- faction_elemental
- faction_construct
- faction_abyssal
- faction_corrupted

---

## Nota

Os 40 inimigos da spec estão documentados em `.specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` linhas 527-612, com tabela completa pronta para você refinar e expandir conforme Vaalara lore.
