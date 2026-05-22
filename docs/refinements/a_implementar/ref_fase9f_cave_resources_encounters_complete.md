# REF FUTURO — FASE9F cave resources encounters complete

> Origem histórica: $Source
> Status: Refinamento futuro preservado
> Spec futura relacionada: $Spec

---

## Decisões preservadas

Conteúdo histórico preservado abaixo para evitar perda operacional de decisões, escopo e pendências.

---

# FASE 9F — Cave, Resources, Encounters e Loot Progression Spec v1.0

> **Status:** spec aprovada para orientar próximas waves.  
> **Feature:** `FASE9F_CAVE_RESOURCES_ENCOUNTERS`  
> **Base:** FASE9B Cave/Combat MVP + FASE9E UI/Hotbar/Equipment + Damage/Status + Item Taxonomy + Save/Migration + Player Progression.  
> **Objetivo:** transformar a caverna MVP em um sistema progressivo, procedural, labiríntico e persistente, com níveis grandes, recursos por faixa, nodes, ferramentas, stamina, inimigos, bosses, checkpoints, XP, loot e integração com save/load.

---

## 1. Problema

A cave atual valida o fluxo mínimo de entrada, saída, combate simples e drop básico. Para virar um pilar real do jogo, ela precisa de uma especificação única para geração procedural, níveis grandes, checkpoints, recursos, nodes, ferramentas, stamina, inimigos, bosses, loot, XP, renovação diária e persistência.

---

## 2. User story

Como jogador, quero explorar níveis grandes e labirínticos da caverna, coletar recursos exclusivos, enfrentar criaturas e bosses, liberar checkpoints e obter materiais melhores para craftar equipamentos mais fortes e avançar cada vez mais fundo.

---

## 3. Decisões fechadas

| Tema | Decisão |
|---|---|
| Entrada inicial | Jogador começa em `CaveLevel = 1` |
| Checkpoints | A cada 15 níveis |
| Entrada por checkpoint | Jogador pode escolher entrar em qualquer checkpoint liberado |
| ResourceNode | Exige ferramenta e consome stamina |
| Tool tier | Node exige tipo e nível específico de ferramenta |
| Minério | Pickaxe obrigatória para minerar minério real |
| Fallback sem pickaxe/tier | Jogador extrai `1x item_material_stone`, não coleta minério principal e não depleta o node principal |
| Baús | Ficam para depois de nodes + enemies |
| Renovação diária | Só nodes com `RespawnsDaily = true` renovam no novo dia |
| Layout | Já deve nascer procedural, mesmo no primeiro slice |
| Novo jogo | Novo jogo gera seed da cave |
| Save/load | Save mantém seed e estado da cave |
| Derrota/KO do jogador | Regenera toda a cave run; layout dos níveis muda, mas checkpoints liberados permanecem |
| Slime especial | Deve ter cor/visual diferente, não apenas stats maiores |
| Tamanho dos níveis | Cada nível deve ser grande, labiríntico e explorável |
| Recursos por faixa | Cada nível/faixa pode ter recursos e itens específicos |
| Ãrvores subterrâneas | Alguns níveis têm árvores/nodes que geram madeiras melhores |
| Refinamento | Madeiras/minérios melhores podem exigir refinamento para craftar itens melhores |
| Boss por bioma | Toda mudança de bioma tem boss poderoso e difícil |
| ItemRarity | Continua fora do MVP; drop melhor usa tabela/quantidade/tier/chance |

---

## 4. Objetivos funcionais

### O1 — CaveLevel inicial e progressão

A primeira entrada na caverna começa em `CaveLevel = 1`. O jogador avança por níveis usando saída/escada/portal de profundidade. `CaveLevel` define bioma, nível base das criaturas, recursos, encontros, boss e chances de drop.

### O2 — Checkpoints a cada 15 níveis

Checkpoints permanentes:

| Checkpoint | Condição |
|---:|---|
| 1 | sempre disponível |
| 15 | alcançar/vencer CaveLevel 15 |
| 30 | alcançar/vencer CaveLevel 30 |
| 45 | alcançar/vencer CaveLevel 45 |
| 60 | alcançar/vencer CaveLevel 60 |
| 75 | alcançar/vencer CaveLevel 75 |
| 90 | alcançar/vencer CaveLevel 90 |

Ao entrar na caverna, o jogador pode escolher `Level 1` ou qualquer checkpoint liberado.

### O3 — Bosses de transição de bioma

| Faixa | Bioma | Boss |
|---:|---|---|
| 1–15 | Stone Cave | boss no 15 |
| 16–30 | Underground Forest | boss no 30 |
| 31–45 | Ice Cave | boss no 45 |
| 46–60 | Fire Cave | boss no 60 |
| 61–75 | Ancient Ruins | boss no 75 |
| 76–90 | Shadow Abyss | boss no 90 |
| 91–99 | Corrupted Core | elite/boss chain |
| 100 | Final Depth | boss final/lore a definir |

Boss bloqueia o avanço para o próximo bioma. Derrotar boss pode liberar checkpoint e faixa seguinte.

### O4 — Procedural determinístico por save/run

Usar duas seeds:

```text
CaveWorldSeed = seed macro persistente do save
CaveRunSeed = seed da run atual da cave
```

Regras:

- novo jogo gera `CaveWorldSeed`;
- nova run gera `CaveRunSeed`;
- save/load mantém as seeds atuais;
- `CaveWorldSeed + CaveRunSeed + CaveLevel` define layout/conteúdo;
- ao sofrer KO/derrota, gerar nova `CaveRunSeed`;
- checkpoints liberados permanecem.

### O5 — Níveis grandes e labirínticos

Cada nível deve ter múltiplas salas, corredores, becos sem saída, caminhos alternativos, áreas opcionais, clusters de recursos, pontos de encontro e entrada/saída distantes.

### O6 — Biomas e recursos por faixa

| Faixa | Bioma | Recursos comuns | Recursos melhores |
|---:|---|---|---|
| 1–15 | Stone Cave | stone, copper, slime gel | cave root wood |
| 16–30 | Underground Forest | copper, iron, root fiber | underground hardwood |
| 31–45 | Ice Cave | iron, ice crystal | frostwood |
| 46–60 | Fire Cave | iron, gold, fire crystal | emberwood |
| 61–75 | Ancient Ruins | gold, relic fragment | ancient timber |
| 76–90 | Shadow Abyss | shadow fungus, dark crystal | gloomwood |
| 91–99 | Corrupted Core | diamond, arcane ore | corrupted heartwood |
| 100 | Final Depth | boss/lore drops | a definir |

### O7 — ResourceNodes com ferramenta, tier e stamina

Campos mínimos:

```csharp
public string RequiredToolType;
public int RequiredToolTier;
public int StaminaCost;
public int HitsRequired;
public string LootTableId;
public bool RespawnsDaily;
```

Ferramenta correta e tier suficiente permitem coletar loot principal. Cada hit consome stamina. Ao completar `HitsRequired`, o node fica depleted.

### O8 — Pickaxe obrigatória para minério

Se o jogador não tiver pickaxe ou usar tier insuficiente em minério:

```text
- recebe 1x item_material_stone;
- consome stamina;
- não recebe minério principal;
- não depleta o node principal;
- recebe feedback de ferramenta inadequada.
```

### O9 — Ãrvores e madeiras subterrâneas

| Node | Drop bruto | Refinado alvo | Uso |
|---|---|---|---|
| CaveRootTree | `item_material_cave_root_wood` | `item_material_refined_cave_wood` | craft inicial/médio |
| UndergroundHardwoodTree | `item_material_underground_hardwood` | `item_material_refined_hardwood` | upgrades Bronze/Iron |
| FrostwoodTree | `item_material_frostwood` | `item_material_refined_frostwood` | itens de gelo/resistência |
| EmberwoodTree | `item_material_emberwood` | `item_material_refined_emberwood` | itens de fogo |
| AncientTimberNode | `item_material_ancient_timber` | `item_material_refined_ancient_timber` | equipamentos avançados |
| GloomwoodNode | `item_material_gloomwood` | `item_material_refined_gloomwood` | magia/sombra |
| CorruptedHeartwoodNode | `item_material_corrupted_heartwood` | `item_material_refined_corrupted_heartwood` | late game |

Nodes de madeira exigem Axe ou ferramenta apropriada, podem consumir stamina e podem ter tier mínimo por faixa.

### O10 — Spawn de criaturas por CaveLevel

```text
70%–80% das criaturas: EnemyLevel = CaveLevel
10%–20% das criaturas: EnemyLevel = CaveLevel + 1
10% chance: criatura especial EnemyLevel = CaveLevel + 2
```

### O11 — Slime especial com cor diferente

Criaturas especiais devem ter feedback visual: cor/sprite diferente, nome diferente no debug/HUD, XP maior, loot melhor e possivelmente escala maior.

### O12 — XP por inimigo

```text
XpReward = EnemyLevel Ã— DifficultyXpMultiplier
```

| Dificuldade | Multiplicador XP |
|---|---:|
| VeryEasy | 5 |
| Easy | 8 |
| Normal | 10 |
| Hard | 15 |
| Elite | 25 |
| MiniBoss | 50 |
| Boss | 100 |

Se `XpRewardOverride > 0`, usar valor explícito.

### O13 — Loot tables

Substituir gradualmente `dropItemId/dropAmount` por loot table.

```csharp
public class LootTableSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public LootEntry[] Entries;
}

[Serializable]
public class LootEntry
{
    public string ItemId;
    public int MinAmount;
    public int MaxAmount;
    public float DropChance;
    public bool IsBetterDrop;
}
```

Inimigos, inimigos especiais, nodes e baús futuros usam loot table por ID.

### O14 — Renovação diária controlada

```text
RespawnsDaily = true  -> remove de DepletedNodeIds no DayStartedEvent
RespawnsDaily = false -> continua depleted até regra própria
```

### O15 — Baús fora do primeiro slice

Baús entram depois de nodes + enemies. Design reservado: `ChestId`, `LootTableId`, `OpenedChestIds`, baú comum, raro, boss chest e puzzle chest.

---

## 5. Non-goals

Fora do primeiro pacote FASE9F:

- arte final da cave;
- todos os 100 níveis implementados manualmente;
- boss final do nível 100;
- puzzles completos;
- armadilhas complexas;
- companion ativo na cave;
- mercadores da Guilda das Estradas;
- baús no primeiro slice;
- minimap;
- iluminação/fog final;
- ItemRarity;
- balanceamento final;
- animações finais dos bosses.

---

## 6. Regras de negócio

1. `CaveLevel` é fonte de dificuldade, bioma, recursos, encontros, boss e requisitos médios de ferramenta.
2. Layout e conteúdo são separados: layout = salas/corredores/entrada/saída; conteúdo = inimigos/nodes/encontros/drops.
3. Save nunca salva Unity refs; salvar apenas IDs, números, bools, strings, enums, posições simples e DTOs.
4. Sistemas novos não podem usar `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType` em runtime.
5. Nodes salvam depleted state por ID estável.
6. KO/derrota do jogador regenera a cave run com nova `CaveRunSeed`, preservando checkpoints.
7. Boss bloqueia bioma seguinte.

---

## 7. Entidades funcionais

### 7.1 CaveGenerationConfigSO

```csharp
public class CaveGenerationConfigSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public int MinRooms;
    public int MaxRooms;
    public int MinRoomWidth;
    public int MaxRoomWidth;
    public int MinRoomHeight;
    public int MaxRoomHeight;
    public int TargetWidth;
    public int TargetHeight;
    public int ExtraConnectionChancePercent;
}
```

### 7.2 CaveLevelConfigSO

```csharp
public class CaveLevelConfigSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public int CaveLevel;
    public string BiomeId;
    public int MinEnemyCount;
    public int MaxEnemyCount;
    public int MinResourceNodeCount;
    public int MaxResourceNodeCount;
    public string EncounterTableId;
    public string ResourceTableId;
    public bool HasCheckpoint;
    public bool HasBoss;
    public string BossEnemyId;
}
```

### 7.3 CaveBiomeDataSO

```csharp
public class CaveBiomeDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public int MinLevel;
    public int MaxLevel;
    public string PassiveStatusId;
    public string[] AllowedEnemyIds;
    public string[] AllowedResourceNodeIds;
}
```

### 7.4 ResourceNodeDataSO

```csharp
public class ResourceNodeDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public string RequiredToolType;
    public int RequiredToolTier;
    public int StaminaCost;
    public int HitsRequired;
    public string LootTableId;
    public bool RespawnsDaily;
    public string FallbackItemId;
    public int FallbackAmount;
    public bool FallbackDepletesNode;
}
```

### 7.5 CaveRuntimeState

```csharp
public sealed class CaveRuntimeState
{
    public int CurrentCaveLevel;
    public int DeepestLayerReached;
    public string CaveWorldSeed;
    public string CaveRunSeed;
    public HashSet<int> UnlockedCheckpoints;
    public HashSet<string> DepletedNodeIds;
    public HashSet<string> OpenedChestIds;
    public HashSet<string> DefeatedPersistentEnemyIds;
    public HashSet<string> DefeatedBossIds;
}
```

### 7.6 CaveSaveData vNext

```csharp
[Serializable]
public class CaveSaveData
{
    public int CurrentLayer = 1;
    public int DeepestLayerReached = 1;
    public string CaveWorldSeed;
    public string CaveRunSeed;
    public List<int> UnlockedCheckpoints = new();
    public List<string> OpenedChestIds = new();
    public List<string> DepletedNodeIds = new();
    public List<string> DefeatedPersistentEnemyIds = new();
    public List<string> DefeatedBossIds = new();
}
```

---

## 8. Eventos

Eventos carregam apenas IDs e tipos simples.

```csharp
public readonly struct CaveRunRegeneratedEvent
{
    public readonly string CaveRunSeed;
    public readonly string Reason; // PlayerDefeated, NewGame, Debug
}

public readonly struct CaveLevelEnteredEvent
{
    public readonly int CaveLevel;
    public readonly string BiomeId;
    public readonly string CaveRunSeed;
}

public readonly struct ResourceNodeDepletedEvent
{
    public readonly string NodeInstanceId;
    public readonly string ResourceNodeId;
    public readonly string LootTableId;
}

public readonly struct CaveCheckpointUnlockedEvent
{
    public readonly int CaveLevel;
}

public readonly struct CaveBossDefeatedEvent
{
    public readonly string BossEnemyId;
    public readonly int CaveLevel;
    public readonly string BiomeId;
}
```

---

## 9. Critérios de aceite

- Novo jogo entra na cave em `CaveLevel = 1`.
- Checkpoint 15 aparece como opção após liberação.
- KO/derrota regenera a cave run, mas checkpoints continuam liberados.
- Nova `CaveRunSeed` muda layout dos níveis.
- Save/load preserva `CaveWorldSeed`, `CaveRunSeed`, `CurrentLayer`, `DeepestLayerReached` e checkpoints.
- CaveLevel gerado tem múltiplas salas, corredores, entrada, saída e áreas opcionais.
- ResourceNode valida ferramenta e tier antes de gerar loot principal.
- ResourceNode consome stamina.
- Minerar sem pickaxe/tier suficiente gera apenas `1x item_material_stone`, não depleta node principal e não entrega minério.
- Novo dia restaura somente nodes `RespawnsDaily = true`.
- Spawn respeita distribuição base/+1/especial +2.
- Slime especial tem cor/sprite diferente.
- Boss de fim de faixa bloqueia bioma seguinte.
- Inimigos e nodes usam loot table por ID.
- Save de cave contém apenas DTOs e tipos simples.
- Nenhum sistema novo usa busca global runtime proibida.

---

## 10. Plano sugerido de PRs pequenos

| PR | Escopo |
|---|---|
| PR-170 | Cave procedural contracts: configs, runtime state e eventos |
| PR-171 | Cave procedural generator MVP: rooms, corridors, entrada, saída, spawn/resource points |
| PR-172 | Cave run regeneration on player defeat/KO |
| PR-173 | Cave checkpoints a cada 15 níveis e entrada por checkpoint |
| PR-174 | ResourceNode contracts com ferramenta, tier, stamina e fallback |
| PR-175 | ResourceNode runtime MVP: Stone, CopperOre, CaveRootTree |
| PR-176 | Cave save/load: seeds, current layer, deepest layer, checkpoints, depleted nodes |
| PR-177 | Enemy spawn by CaveLevel com Slime especial colorido |
| PR-178 | Loot tables para inimigos e nodes |
| PR-179 | XP integration com EnemyLevel Ã— DifficultyMultiplier |
| PR-180 | Daily cave refresh apenas para `RespawnsDaily = true` |
| PR-181 | Biome boss MVP no CaveLevel 15 |
| PR-182 | Cave validator |

---

## 11. Milestone jogável alvo

```text
Entrar CaveLevel 1
→ layout grande procedural
→ minerar node com stamina/ferramenta
→ fallback sem pickaxe gera 1 stone
→ enfrentar Slime comum/especial
→ avançar níveis
→ liberar checkpoint 15
→ sofrer KO/derrota
→ cave run regenera layout
→ checkpoint permanece
→ save/load preserva estado
```


## Itens que devem virar implementação

- [ ] Refinar em spec executável antes de código, quando aplicável.
- [ ] Validar dependências contra specs implementadas atuais.

## Fora de escopo / cuidado

- Não tratar este refinement como autorização automática de implementação.
- Não sobrescrever specs implementadas sem amendment/correction explícito.


