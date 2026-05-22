# SpecKit â€” FASE9E Player Level Up, XP e ProgressÃ£o

> **Feature:** FASE9E_PLAYER_LEVEL_UP_PROGRESSION  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9E_PLAYER_LEVEL_UP_PROGRESSION_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero ganhar XP ao realizar atividades importantes, subir de nÃ­vel e melhorar meu personagem para sentir progressÃ£o real entre farm, exploraÃ§Ã£o, combate e crafting.

---

## 2. Objetivos funcionais

### O1 â€” Level global

Criar progressÃ£o global do personagem atÃ© MaxLevel 100.

### O2 â€” XP curve

Usar curva de XP com multiplicador que aumenta a cada bloco de 10 nÃ­veis.

### O3 â€” Attribute points

Cada level concede +1 AttributePoint.

### O4 â€” Skill points

A cada 3 nÃ­veis concede +1 SkillPoint.

### O5 â€” Atributos MVP

Suportar Strength, Dexterity, Intelligence, Willpower, Constitution e Breath.

### O6 â€” XP de criaturas

Calcular XP por EnemyLevel e Difficulty final.

### O7 â€” CaveLevel spawn

Definir regra de spawn por CaveLevel com criaturas do nÃ­vel base, +1 e rara +2.

### O8 â€” Save/load

Persistir level, XP, pontos e atributos.

---

## 3. Non-goals

Fora de escopo:

- skill tree completa;
- respec;
- UI final de level up;
- animaÃ§Ã£o/vfx de level up;
- classes/subclasses;
- perks complexos;
- balanceamento final.

---

## 4. Regras de negÃ³cio

### R1 â€” MaxLevel

MaxLevel = 100.

### R2 â€” XP curve

```text
LevelBand = floor((Level - 1) / 10)
LevelMultiplier = 50 + (LevelBand * 10)
XpToNextLevel = 100 + ((Level - 1) * LevelMultiplier)
```

### R3 â€” Rewards

- +1 AttributePoint por level.
- +1 SkillPoint quando `Level % 3 == 0`.

### R4 â€” Atributos

- Strength soma em melee.
- Dexterity soma em ranged fÃ­sico.
- Intelligence soma em magia.
- Willpower aumenta MaxMana em +4 por ponto.
- Constitution aumenta MaxHP em +5 por ponto.
- Breath aumenta MaxStamina em +2 por ponto.

### R5 â€” Bases

```text
BaseMaxMana = 10
BaseMaxHP = 12
BaseMaxStamina = 10
```

### R6 â€” Creature XP

```text
XpReward = EnemyLevel Ã— DifficultyXpMultiplier
```

### R7 â€” Cave spawn

```text
70%â€“80%: EnemyLevel = CaveLevel
10%â€“20%: EnemyLevel = CaveLevel + 1
10% chance: special EnemyLevel = CaveLevel + 2
```

---

## 5. Entidades funcionais

- `PlayerStats`
- `PlayerProgression`
- `PlayerProgressionManager`
- `XpSourceType`
- `EnemyDifficulty`
- `XpGainedEvent`
- `LevelUpEvent`
- `AttributePointSpentEvent`

---

## 6. CritÃ©rios de aceite

### CA1 â€” XP ganho

Jogador ganha XP por evento vÃ¡lido.

### CA2 â€” Level up

Ao atingir XP necessÃ¡rio, jogador sobe de nÃ­vel.

### CA3 â€” Multi-level

Multi-level up funciona.

### CA4 â€” Pontos

Level concede AttributePoint e, a cada 3 nÃ­veis, SkillPoint.

### CA5 â€” Atributos

Jogador pode gastar AttributePoint nos 6 atributos MVP.

### CA6 â€” Derivados

MaxMana, MaxHP e MaxStamina refletem Willpower, Constitution e Breath.

### CA7 â€” Enemy XP

XP de inimigo usa EnemyLevel e Difficulty final.

### CA8 â€” CaveLevel

CaveLevel gera criaturas com variaÃ§Ã£o de nÃ­vel conforme regra MVP.

### CA9 â€” Save/load

ProgressÃ£o e atributos persistem.

### CA10 â€” HUD

Debug HUD mostra level, XP, pontos e atributos.

---

## 7. DependÃªncias

- `SaveData`
- `PlayerManager`
- `DamageCalculator`
- `EnemyDataSO`
- `EnemyHealth`
- `FarmPlot`
- `TreeNode`
- `ResourceNode` futuro
- `CaveSpawner` futuro
- `DebugHud`
- `GameEventBus`

---

## 8. Pronto para Plan quando

- Esta spec estiver aprovada.
- `docs_old/FASE9E_PLAYER_LEVEL_UP_PROGRESSION_SPEC_v1.0.md` estiver lida.
- Estado real em `dev` tiver sido validado.

