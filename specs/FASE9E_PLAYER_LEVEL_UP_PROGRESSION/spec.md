# SpecKit — FASE9E Player Level Up, XP e Progressão

> **Feature:** FASE9E_PLAYER_LEVEL_UP_PROGRESSION  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9E_PLAYER_LEVEL_UP_PROGRESSION_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero ganhar XP ao realizar atividades importantes, subir de nível e melhorar meu personagem para sentir progressão real entre farm, exploração, combate e crafting.

---

## 2. Objetivos funcionais

### O1 — Level global

Criar progressão global do personagem até MaxLevel 100.

### O2 — XP curve

Usar curva de XP com multiplicador que aumenta a cada bloco de 10 níveis.

### O3 — Attribute points

Cada level concede +1 AttributePoint.

### O4 — Skill points

A cada 3 níveis concede +1 SkillPoint.

### O5 — Atributos MVP

Suportar Strength, Dexterity, Intelligence, Willpower, Constitution e Breath.

### O6 — XP de criaturas

Calcular XP por EnemyLevel e Difficulty final.

### O7 — CaveLevel spawn

Definir regra de spawn por CaveLevel com criaturas do nível base, +1 e rara +2.

### O8 — Save/load

Persistir level, XP, pontos e atributos.

---

## 3. Non-goals

Fora de escopo:

- skill tree completa;
- respec;
- UI final de level up;
- animação/vfx de level up;
- classes/subclasses;
- perks complexos;
- balanceamento final.

---

## 4. Regras de negócio

### R1 — MaxLevel

MaxLevel = 100.

### R2 — XP curve

```text
LevelBand = floor((Level - 1) / 10)
LevelMultiplier = 50 + (LevelBand * 10)
XpToNextLevel = 100 + ((Level - 1) * LevelMultiplier)
```

### R3 — Rewards

- +1 AttributePoint por level.
- +1 SkillPoint quando `Level % 3 == 0`.

### R4 — Atributos

- Strength soma em melee.
- Dexterity soma em ranged físico.
- Intelligence soma em magia.
- Willpower aumenta MaxMana em +4 por ponto.
- Constitution aumenta MaxHP em +5 por ponto.
- Breath aumenta MaxStamina em +2 por ponto.

### R5 — Bases

```text
BaseMaxMana = 10
BaseMaxHP = 12
BaseMaxStamina = 10
```

### R6 — Creature XP

```text
XpReward = EnemyLevel × DifficultyXpMultiplier
```

### R7 — Cave spawn

```text
70%–80%: EnemyLevel = CaveLevel
10%–20%: EnemyLevel = CaveLevel + 1
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

## 6. Critérios de aceite

### CA1 — XP ganho

Jogador ganha XP por evento válido.

### CA2 — Level up

Ao atingir XP necessário, jogador sobe de nível.

### CA3 — Multi-level

Multi-level up funciona.

### CA4 — Pontos

Level concede AttributePoint e, a cada 3 níveis, SkillPoint.

### CA5 — Atributos

Jogador pode gastar AttributePoint nos 6 atributos MVP.

### CA6 — Derivados

MaxMana, MaxHP e MaxStamina refletem Willpower, Constitution e Breath.

### CA7 — Enemy XP

XP de inimigo usa EnemyLevel e Difficulty final.

### CA8 — CaveLevel

CaveLevel gera criaturas com variação de nível conforme regra MVP.

### CA9 — Save/load

Progressão e atributos persistem.

### CA10 — HUD

Debug HUD mostra level, XP, pontos e atributos.

---

## 7. Dependências

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


