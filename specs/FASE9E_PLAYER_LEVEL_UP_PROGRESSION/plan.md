# Plan — FASE9E Player Level Up, XP e Progressão

> **Feature:** FASE9E_PLAYER_LEVEL_UP_PROGRESSION  
> **Spec:** `spec.md`

---

## 1. Estratégia

Implementar progressão em PRs pequenos:

1. contratos de atributos, XP e dificuldade;
2. PlayerProgressionManager;
3. integração com save/load;
4. eventos e HUD debug;
5. XP rewards de combate/farm/recursos;
6. integração com EnemyDataSO e CaveLevel;
7. validator/handoff.

---

## 2. Arquitetura proposta

### Novos tipos

- `PlayerAttributeType`
- `XpSourceType`
- `EnemyDifficulty`

### Novas classes

- `PlayerStats`
- `PlayerProgression`
- `PlayerProgressionManager`
- `XpRewardCalculator`

### Eventos

- `XpGainedEvent`
- `LevelUpEvent`
- `AttributePointSpentEvent`

---

## 3. Integrações

### Save

Persistir:

- Level;
- CurrentXp;
- XpToNextLevel;
- UnspentAttributePoints;
- UnspentSkillPoints;
- Strength;
- Dexterity;
- Intelligence;
- Willpower;
- Constitution;
- Breath.

### Damage

DamageCalculator usa Strength/Dexterity/Intelligence.

### Resources

MaxMana, MaxHP e MaxStamina são derivados de Willpower/Constitution/Breath.

### Enemies

EnemyDataSO adiciona EnemyLevel, BaseDifficulty e XpRewardOverride.

### Cave

CaveSpawner futuro usa CaveLevel para gerar EnemyLevel.

---

## 4. Fórmulas

### XP

```text
LevelBand = floor((Level - 1) / 10)
LevelMultiplier = 50 + (LevelBand * 10)
XpToNextLevel = 100 + ((Level - 1) * LevelMultiplier)
```

### Stats derivados

```text
MaxMana = 10 + (Willpower * 4)
MaxHP = 12 + (Constitution * 5)
MaxStamina = 10 + (Breath * 2)
```

### Enemy XP

```text
XpReward = EnemyLevel × DifficultyXpMultiplier
```

---

## 5. Riscos

| Risco | Mitigação |
|---|---|
| Progressão desbalanceada | manter valores data-driven e ajustar depois |
| XP farm exploit | XP só em conclusão de ação, não em hit/interação parcial |
| Dificuldade relativa confusa | separar BaseDifficulty de Difficulty final calculada |
| Skill tree virar escopo grande | salvar SkillPoints agora; árvore fica para spec própria |
| Save antigo sem atributos novos | defaults seguros via Save Migration |

---

## 6. Testes manuais mínimos

- Ganhar XP debug.
- Subir 1 nível.
- Subir múltiplos níveis.
- Ganhar AttributePoint.
- Ganhar SkillPoint no level múltiplo de 3.
- Gastar ponto em Strength e ver dano melee aumentar.
- Gastar ponto em Willpower e ver mana aumentar.
- Gastar ponto em Constitution e ver HP aumentar.
- Gastar ponto em Breath e ver stamina aumentar.
- Derrotar inimigo e ganhar XP por EnemyLevel/Difficulty.
- Validar regra de CaveLevel quando spawner existir.
- Salvar/carregar progressão.

---

## 7. Fora de escopo técnico

- árvore de skills completa;
- UI final de level up;
- respec;
- balance final;
- classes/subclasses;
- perks complexos.
