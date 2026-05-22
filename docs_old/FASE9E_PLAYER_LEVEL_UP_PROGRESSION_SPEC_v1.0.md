# FASE 9E — Player Level Up, XP e Progressão Spec v1.0

> **Status:** spec aprovada para orientar próximas waves.  
> **Feature:** FASE9E_PLAYER_LEVEL_UP_PROGRESSION  
> **Base:** Save Schema/Migration + Damage/Status Formula + Enemy Architecture + Cave futura.  
> **Objetivo:** definir level, XP, atributos, pontos de atributo, skill points, dificuldade de criaturas e progressão base do personagem.

---

## 1. Problema

O save já reserva Level, XP e pontos. A fórmula de dano já usa atributos ofensivos. Precisamos fechar o contrato de progressão antes de implementar cave, inimigos, recompensas e balanceamento.

---

## 2. Decisões fechadas

| Tema | Decisão |
|---|---|
| MaxLevel | 100 |
| Level global | Sim, único no MVP |
| AttributePoint | +1 por level |
| SkillPoint | +1 a cada 3 níveis |
| XP curve | linear com multiplicador por bloco de 10 níveis |
| Morte perde XP? | Não no MVP |
| Skill trees completas | Outra spec |
| XP de criaturas | por EnemyLevel e dificuldade relativa |
| Cave spawn | usa CaveLevel como base, com variação de níveis |

---

## 3. Atributos MVP

```csharp
public class PlayerStats
{
    public int Strength = 1;
    public int Dexterity = 1;
    public int Intelligence = 1;
    public int Willpower = 1;
    public int Constitution = 1;
    public int Breath = 1;
}
```

| Atributo | Nome PT-BR | Função | Fórmula MVP |
|---|---|---|---|
| Strength | Força | dano físico corpo a corpo | `MeleeDamage = BaseDamage + Strength` |
| Dexterity | Destreza | dano físico à distância | `RangedDamage = BaseDamage + Dexterity` |
| Intelligence | Inteligência | dano mágico | `MagicDamage = BaseDamage + Intelligence` |
| Willpower | Vontade | mana para magias | `MaxMana = BaseMaxMana + (Willpower * 4)` |
| Constitution | Constituição | vida máxima | `MaxHP = BaseMaxHP + (Constitution * 5)` |
| Breath | Fôlego | stamina física | `MaxStamina = BaseMaxStamina + (Breath * 2)` |

Bases MVP:

```text
BaseMaxMana = 10
BaseMaxHP = 12
BaseMaxStamina = 10
```

Regras:

- todos os atributos começam em 1;
- AttributeMax MVP = 100;
- Willpower representa mana, recurso usado por magia;
- Breath representa stamina física para correr, pular, esquivar, atacar e defender;
- Constitution representa vida base e robustez.

---

## 4. Progression data

```csharp
[Serializable]
public class PlayerProgressionSaveData
{
    public int Level = 1;
    public int CurrentXp = 0;
    public int XpToNextLevel = 100;
    public int UnspentAttributePoints = 0;
    public int UnspentSkillPoints = 0;
}
```

---

## 5. Curva de XP

A curva usa uma base linear, mas o incremento por nível aumenta a cada bloco de 10 níveis.

```text
LevelBand = floor((Level - 1) / 10)
LevelMultiplier = 50 + (LevelBand * 10)
XpToNextLevel = 100 + ((Level - 1) * LevelMultiplier)
```

Regras:

- níveis 1–10 usam multiplicador 50;
- níveis 11–20 usam multiplicador 60;
- níveis 21–30 usam multiplicador 70;
- níveis 31–40 usam multiplicador 80;
- e assim por diante.

Exemplos:

| Level atual | LevelBand | Multiplicador | XP para próximo |
|---:|---:|---:|---:|
| 1 | 0 | 50 | 100 |
| 2 | 0 | 50 | 150 |
| 3 | 0 | 50 | 200 |
| 10 | 0 | 50 | 550 |
| 11 | 1 | 60 | 700 |
| 12 | 1 | 60 | 760 |
| 20 | 1 | 60 | 1240 |
| 21 | 2 | 70 | 1500 |
| 30 | 2 | 70 | 2130 |
| 31 | 3 | 80 | 2500 |

---

## 6. Level up rules

Quando o jogador ganha XP:

```text
CurrentXp += amount
while CurrentXp >= XpToNextLevel and Level < MaxLevel:
    CurrentXp -= XpToNextLevel
    Level += 1
    UnspentAttributePoints += 1
    if Level % 3 == 0:
        UnspentSkillPoints += 1
    XpToNextLevel = CalculateXpToNextLevel(Level)
```

Regras:

- multi-level up é permitido;
- ao atingir Level 100, não processar novos level ups;
- XP após Level 100 pode continuar acumulando ou travar em decisão futura;
- morte não remove XP no MVP.

---

## 7. Skill trees conceituais

Skill points não aumentam atributos diretamente. Eles desbloqueiam habilidades/passivas em árvores.

### 7.1 Combat

Foco: armas, resistência em combate e golpes especiais.

- melhoria de uso de espada;
- melhoria de machados/axes em combate;
- melhoria de marretas/hammers;
- armas de duas mãos;
- duas armas, uma em cada mão;
- golpes especiais por tipo de arma;
- resistências de combate;
- defesa/bloqueio futuro.

### 7.2 Magic

Foco: elementos, poder mágico e suporte.

- liberar uso de magias além do fogo;
- aumentar dano mágico;
- aumentar nível natural de magias;
- misturar elementos nos ataques;
- aumentar duração de magias de suporte;
- melhorar chance de aplicar status elemental.

### 7.3 Dexterity

Foco: mobilidade, precisão e ataques à distância.

- aumentar dano ranged;
- liberar chance de crítico;
- melhorar velocidade de movimento;
- melhorar recuperação de knockback;
- bônus em esquiva;
- melhorar arco/flechas.

### 7.4 Survival

Foco: crafting, resistência, eficiência e vida no mundo.

- construir itens com materiais melhores;
- liberar craft de madeira até materiais avançados;
- melhorar resistência a elementos;
- diminuir queda de fome;
- reduzir custo de stamina para pular, esquivar, correr, atacar e defender;
- liberar craft de itens especiais.

A implementação detalhada de skill tree fica para outra spec.

---

## 8. XP de combate

XP de combate vem do nível da criatura e da dificuldade relativa, não de cada hit.

```text
XpReward = EnemyLevel × DifficultyXpMultiplier
```

### 8.1 Dificuldade relativa

```text
LevelDelta = EnemyLevel - ReferenceLevel
```

`ReferenceLevel` pode ser o nível do jogador ou o `CaveLevel`, conforme contexto.

Exemplo com Slime:

| Caso | Difficulty final |
|---|---|
| Slime do mesmo nível | Easy/fraca |
| Slime 2 níveis acima | Normal |
| Slime 4 níveis acima | Hard |
| Slime muito acima | Elite/perigoso |

Regra inicial:

| LevelDelta | Difficulty sugerida |
|---:|---|
| <= 0 | Easy, se criatura base for fraca |
| +1 | Easy ou Normal |
| +2 | Normal |
| +3 | Normal ou Hard |
| +4 | Hard |
| +5 ou mais | Elite ou regra especial |

### 8.2 Multiplicadores de XP

| Dificuldade | Multiplicador XP |
|---|---:|
| VeryEasy | 5 |
| Easy | 8 |
| Normal | 10 |
| Hard | 15 |
| Elite | 25 |
| MiniBoss | 50 |
| Boss | 100 |

Exemplos:

| Criatura | EnemyLevel | ReferenceLevel | Difficulty final | XP |
|---|---:|---:|---|---:|
| Slime comum | 1 | 1 | Easy | 8 |
| Slime acima | 3 | 1 | Normal | 30 |
| Slime perigoso | 5 | 1 | Hard | 75 |
| Elite Slime | 3 | 3 | Elite | 75 |
| MiniBoss | 5 | 5 | MiniBoss | 250 |

---

## 9. CaveLevel e spawn

Cada nível da caverna define um `CaveLevel`.

Spawn rule MVP:

```text
70%–80% das criaturas: EnemyLevel = CaveLevel
10%–20% das criaturas: EnemyLevel = CaveLevel + 1
10% de chance: criatura especial EnemyLevel = CaveLevel + 2
```

A criatura especial `CaveLevel + 2`:

- é rara;
- dá mais XP;
- dropa item melhor;
- pode ser marcada como elite/rare spawn.

Exemplo:

```text
CaveLevel 3:
- maioria dos inimigos nível 3
- parte dos inimigos nível 4
- chance de criatura especial nível 5 com drop melhor
```

---

## 10. EnemyDataSO

Adicionar:

```csharp
public int EnemyLevel;
public EnemyDifficulty BaseDifficulty;
public int XpRewardOverride;
```

Se `XpRewardOverride > 0`, usar valor explícito. Caso contrário, calcular por nível/dificuldade.

```csharp
public enum EnemyDifficulty
{
    VeryEasy,
    Easy,
    Normal,
    Hard,
    Elite,
    MiniBoss,
    Boss
}
```

---

## 11. Outras fontes de XP

### Farm

| Ação | XP MVP |
|---|---:|
| plantar seed comum | 1 |
| colher crop comum | 3 |
| colher crop incomum | 6 |
| colher crop raro | 12 |

### Recursos

| Ação | XP MVP |
|---|---:|
| cortar árvore | 5 |
| minerar pedra comum | 3 |
| minerar cobre | 5 |
| minerar ferro | 8 |
| minerar ouro | 12 |
| minerar diamante | 20 |
| pescar peixe comum | 5 |
| pescar peixe raro | 15 |

### Quests/eventos

| Ação | XP MVP |
|---|---:|
| completar tutorial | 50 |
| completar quest pequena | 100 |
| completar quest média | 250 |
| completar quest grande | 500+ |

---

## 12. Anti-exploit

- XP de combate só ao morrer inimigo.
- Hit parcial em inimigo não dá XP.
- Hit parcial em recurso não dá XP.
- XP de recursos só quando node/tree é concluído/depleted.
- Plantar/remover repetidamente não deve gerar XP infinito.
- Pegar item do chão não dá XP por padrão.
- Inimigos debug podem dar 0 XP.

---

## 13. Eventos

### XpGainedEvent

Payload:

- amount;
- sourceType;
- sourceId;
- totalCurrentXp;
- level.

### LevelUpEvent

Payload:

- oldLevel;
- newLevel;
- grantedAttributePoints;
- grantedSkillPoints.

### AttributePointSpentEvent

Payload:

- attributeType;
- oldValue;
- newValue;
- remainingPoints.

---

## 14. Save/load

Save deve persistir:

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

Valores derivados como MaxMana, MaxHP e MaxStamina podem ser recalculados no load a partir dos atributos e bases atuais.

---

## 15. HUD/debug MVP

Mostrar em DebugHud ou OnGUI:

```text
Level: 3
XP: 40 / 200
Attribute Points: 1
Skill Points: 1
STR: 2  DEX: 1  INT: 1
WILL: 1  CON: 1  BREATH: 1
HP: 17/17  Mana: 14/14  Stamina: 12/12
```

---

## 16. Critérios de aceite

- Jogador ganha XP por evento válido.
- Jogador sobe de nível ao atingir XP necessário.
- Multi-level up funciona.
- Cada level concede +1 AttributePoint.
- A cada 3 níveis concede +1 SkillPoint.
- Jogador pode gastar AttributePoint nos 6 atributos MVP.
- Dano reflete Strength/Dexterity/Intelligence.
- MaxMana reflete Willpower.
- MaxHP reflete Constitution.
- MaxStamina reflete Breath.
- XP de inimigo usa EnemyLevel e Difficulty final.
- CaveLevel gera criaturas com variação de nível conforme regra MVP.
- Save/load preserva level, XP, pontos e atributos.
- HUD mostra level, XP, pontos e atributos.

---

## 17. Tasks sugeridas

- PR-162 — Player progression contracts.
- PR-163 — PlayerProgressionManager.
- PR-164 — XP rewards combat/farm/resources.
- PR-165 — Attribute spending MVP.
- PR-166 — Progression save/load integration.
- PR-167 — Progression events/debug HUD.
- PR-168 — Enemy XP difficulty integration.
- PR-169 — CaveLevel spawn progression rules.
- PR-170 — Progression validator/handoff.

---

## 18. Decisão final

O MVP terá progressão global até level 100, seis atributos base, pontos de atributo por nível e skill points a cada 3 níveis. XP de criaturas será calculado por nível e dificuldade relativa, preparando a progressão da caverna e de inimigos sem hardcode por monstro.
