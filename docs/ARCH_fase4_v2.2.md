# Cindar's Hope — Arquitetura Técnica Completa v2.2

> **Fase:** 4 de 13
> **Status:** ✅ Base aprovada — pendências não bloqueiam o MVP Fazenda
> **Última atualização:** 2026-05-16
> **Depende de:** GDD_v2.6.md
> **Alimenta:** Fase 5 (Ambiente), Fase 6 (Histórias), Fase 7 (Spec), Fase 8 (MVP)

---

## HANDOFF PARA OUTRA LLM

### Plano macro (13 fases)

| # | Fase | Status |
|---|---|---|
| 1 | Ideação | ✅ Concluída |
| 2 | Refinamento por Sistema | ✅ Concluída |
| 3 | Identidade | ✅ Concluída |
| 4 | **Arquitetura Técnica** | ✅ Base aprovada |
| 5 | Estruturação do Ambiente | ✅ Documento pronto / execução local a validar |
| 6 | Quebra de Histórias | ✅ FARM detalhado / demais esboçados |
| 7 | Spec Kit MVP Fazenda | ✅ Specs completas |
| 8 | Prototipagem do Core Loop | 🔄 Próxima execução |
| 9 | Execução Iterativa | ⏳ Pendente |
| 10 | Arte e Polish | ⏳ Pendente |
| 11 | Testes | ⏳ Pendente |
| 12 | Build Local Final | ⏳ Pendente |
| 13 | Iteração Pós-Launch | ⏳ Pendente |

### O que este documento cobre
Arquitetura técnica **completa**: código, gráficos, IA generativa, pipeline arte→Unity, conexão sprite↔mecânica, ferramentas e fluxo de trabalho. É o documento de referência para qualquer pessoa (ou LLM) que for trabalhar no projeto.

### Seções deste documento

1. Visão geral da arquitetura de código
2. GameEventBus — implementação e catálogo de eventos
3. TimeManager — ciclo de tempo, luas, seasons
4. ScriptableObjects — estrutura base de todos os dados
5. State Machine do personagem
6. SaveManager
7. Estrutura de cenas
8. **Pipeline gráfico completo** ← novo
9. **IA generativa na arte** ← novo
10. **Conexão arte ↔ mecânica no Unity** ← novo
11. **Geração de sprites — fluxo completo** ← novo
12. Padrões obrigatórios para Codex / CLAUDE.md

### Itens ainda pendentes de decisão (não bloqueiam início)
- [ ] Receitas completas de cada workshop (Fase 7)
- [ ] ScriptableObjects de NPCData e BiomeData completos (Fase 7)
- [ ] Decisão sobre asset store vs geração própria para tileset base (seção 8.4)
- [x] Ferramenta de pixel art/sprites: Aseprite como principal; Pixelorama/LibreSprite como fallback

---

## 1. Visão Geral da Arquitetura de Código

```
┌──────────────────────────────────────────────────────────────┐
│                        CENAS UNITY                           │
│   BootScene │ FarmScene │ TownScene │ CaveScene │ UIOverlay  │
└───────────────────────┬──────────────────────────────────────┘
                        │ carrega / descarrega
┌───────────────────────▼──────────────────────────────────────┐
│              MANAGERS  (DontDestroyOnLoad)                    │
│  TimeManager  SaveManager  GameEventBus                       │
│  PlayerManager  InventoryManager  AudioManager(placeholder)  │
└───────────────────────┬──────────────────────────────────────┘
                        │ publica / ouve eventos
┌───────────────────────▼──────────────────────────────────────┐
│                   SISTEMAS (por cena)                         │
│  FarmSystem  CaveSystem  CombatSystem  CraftSystem            │
│  CompanionSystem  NPCSystem  ShopSystem  QuestSystem          │
└───────────────────────┬──────────────────────────────────────┘
                        │ lê dados de
┌───────────────────────▼──────────────────────────────────────┐
│              SCRIPTABLEOBJECTS  (Assets/_Game/Data/)          │
│  ItemDataSO  SeedDataSO  CreatureDataSO  RecipeDataSO         │
│  CompanionDataSO  TreeDataSO  NPCDataSO  BiomeDataSO          │
│  WorkshopDataSO  LootTableSO  DialogueSO  QuestDataSO        │
└───────────────────────┬──────────────────────────────────────┘
                        │ referencia sprites de
┌───────────────────────▼──────────────────────────────────────┐
│                  ASSETS VISUAIS                               │
│  Sprites (32x32 px)  Tilemaps  Animations  VFX Particles     │
│  Gerados via: Aseprite manual + IA assistida (ver seção 8-11) │
└──────────────────────────────────────────────────────────────┘
```

**Princípios:**
- Nenhum sistema se comunica diretamente com outro — tudo via `GameEventBus`
- Dados nunca ficam hardcodados em MonoBehaviour — sempre em ScriptableObject
- Sprites e animações são referenciados pelos SOs, não pelos sistemas
- O Codex gera código; humano ou IA gera arte; Unity integra os dois

---

## 2. GameEventBus

### Implementação

```csharp
public static class GameEventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public static void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type)) _handlers[type] = new List<Delegate>();
        _handlers[type].Add(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (_handlers.ContainsKey(type)) _handlers[type].Remove(handler);
    }

    public static void Publish<T>(T evt)
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type)) return;
        foreach (var handler in _handlers[type].ToList())
            ((Action<T>)handler)?.Invoke(evt);
    }
}
```

### Uso padrão

```csharp
// Publicar
GameEventBus.Publish(new DayStartedEvent { DayNumber = 5 });

// Ouvir (sempre em OnEnable/Awake)
void OnEnable() => GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
void OnDisable() => GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
void OnDayStarted(DayStartedEvent e) { /* reage */ }
```

### Catálogo completo de eventos

| Evento | Payload | Publicado por | Ouvido por |
|---|---|---|---|
| `HourChangedEvent` | `Hour` | TimeManager | NPCSystem, UIManager |
| `DayStartedEvent` | `DayNumber` | TimeManager | FarmSystem, NPCSystem, CaveSystem, SaveManager |
| `NightStartedEvent` | `DayNumber` | TimeManager | FarmSystem, CreatureSpawner, NPCSystem |
| `MoonChangedEvent` | `MoonType` | TimeManager | SeedSystem, MerchantSpawner, CaveSystem, UIManager |
| `SeasonChangedEvent` | `Season` | TimeManager | FarmSystem, ShopSystem, NPCSystem, UIManager |
| `PlayerLevelUpEvent` | `NewLevel` | PlayerManager | UIManager, SkillTreeSystem |
| `AttributeChangedEvent` | `Attribute, NewValue` | PlayerManager | UIManager, CombatSystem |
| `HungerChangedEvent` | `CurrentValue, MaxValue` | HungerSystem | UIManager |
| `HungerCriticalEvent` | `CurrentValue` | HungerSystem | PlayerManager, UIManager |
| `HungerEmptyEvent` | — | HungerSystem | CombatSystem, PlayerManager |
| `StatusAppliedEvent` | `StatusType, Duration` | CombatSystem | PlayerManager, UIManager |
| `StatusRemovedEvent` | `StatusType` | PlayerManager | UIManager |
| `PlayerDiedEvent` | — | CombatSystem | SaveManager, RespawnSystem, UIManager |
| `PlayerRespawnedEvent` | — | RespawnSystem | UIManager, FarmSystem |
| `ItemPickedUpEvent` | `ItemId, Amount` | PlayerController | InventoryManager, UIManager, QuestSystem |
| `ItemUsedEvent` | `ItemId` | InventoryManager | PlayerManager, UIManager |
| `ItemEquippedEvent` | `ItemId, Slot` | InventoryManager | CombatSystem, UIManager |
| `ItemCraftedEvent` | `ItemId, Amount` | CraftSystem | InventoryManager, UIManager, QuestSystem |
| `SeedPlantedEvent` | `SeedId, TilePos` | FarmSystem | UIManager, QuestSystem |
| `CropHarvestedEvent` | `SeedId, Yield` | FarmSystem | InventoryManager, UIManager, QuestSystem |
| `TreeChoppedEvent` | `TreeId, WoodAmount` | FarmSystem | InventoryManager, UIManager |
| `FishCaughtEvent` | `FishId` | FarmSystem | InventoryManager, UIManager, QuestSystem |
| `FarmExpandedEvent` | `NewSize` | FarmSystem | UIManager |
| `WorkshopBuiltEvent` | `WorkshopType` | FarmSystem | CraftSystem, UIManager |
| `WorkshopUpgradedEvent` | `WorkshopType, NewLevel` | CraftSystem | UIManager |
| `CompanionRecruitedEvent` | `CompanionId` | CompanionSystem | UIManager |
| `CompanionJobChangedEvent` | `CompanionId, Job` | CompanionSystem | UIManager |
| `CompanionDiedEvent` | `CompanionId` | CombatSystem | CompanionSystem, UIManager |
| `CompanionRessurectedEvent` | `CompanionId` | CompanionSystem | UIManager |
| `EnemyDiedEvent` | `CreatureId, Position` | CombatSystem | LootSystem, UIManager, QuestSystem |
| `CheckpointReachedEvent` | `CaveLevel` | CaveSystem | SaveManager, UIManager |
| `CaveLevelChangedEvent` | `Level` | CaveSystem | UIManager, CreatureSpawner |
| `MerchantFoundEvent` | `MerchantId` | CaveSystem | UIManager |
| `ReputationChangedEvent` | `Delta, NewTotal` | NPCSystem | ShopSystem, UIManager |
| `QuestStartedEvent` | `QuestId` | QuestSystem | UIManager |
| `QuestCompletedEvent` | `QuestId` | QuestSystem | ReputationSystem, UIManager, ShopSystem |
| `SceneChangingEvent` | `TargetScene` | SceneManager | SaveManager, UIManager |
| `GoldChangedEvent` | `Delta, NewTotal` | InventoryManager | UIManager |

---

## 3. TimeManager

```csharp
[CreateAssetMenu(fileName = "GameConfig", menuName = "CindarsHope/GameConfig")]
public class GameConfigSO : ScriptableObject
{
    public float RealSecondsPerGameHour = 50f; // 50s × 24h = 20min/dia real
    public int DaysPerMonth = 30;
    public int NightStartHour = 20;
    public int DawnStartHour = 6;
    public int MonthsPerSeason = 3;  // 3 meses = 3 luas por season
}
```

```csharp
public enum MoonType  { Alihana, Senya, Nyx }
public enum Season    { Spring, Summer, Autumn, Winter }

public class TimeManager : MonoBehaviour
{
    // Lua ativa: ciclo de 3 meses independente da season
    public MoonType ActiveMoon => (MoonType)((CurrentMonth - 1) % 3);
    public Season CurrentSeason => (Season)(((CurrentMonth - 1) / Config.MonthsPerSeason) % 4);

    // Eventos publicados automaticamente ao atingir cada limiar
}
```

**Efeitos das luas nos sistemas:**

| Lua | FarmSystem | CaveSystem | ShopSystem |
|---|---|---|---|
| Alihana | Sementes noturnas +20% yield | Mercadores com itens raros/proféticos | Blueprints raros disponíveis |
| Senya | — | Criaturas de caos mais frequentes, magia +15% dmg | Preços +10% (festa = demanda) |
| Nyx | — | Criaturas noturnas também aparecem de dia | Yael (NPC noturna) tem estoque extra |

---

## 4. ScriptableObjects — Estrutura Completa

### 4.1 ItemDataSO
```csharp
[CreateAssetMenu(menuName = "CindarsHope/Items/Item")]
public class ItemDataSO : ScriptableObject
{
    public string ItemId;
    public string DisplayName;
    [TextArea] public string Description;
    public Sprite Icon;                   // ← referência ao sprite 32x32
    public ItemCategory Category;         // Weapon, Armor, Seed, Food, Material, Tool, Consumable, Blueprint, KeyItem
    public int MaxStack;
    public int BaseValue;
    public bool IsEquippable;
    public EquipSlot EquipSlot;           // Head, Chest, Legs, Hand, Offhand, Ring
    public StatModifier[] StatModifiers;  // ex: {Strength, +5}
}
```

### 4.2 SeedDataSO
```csharp
[CreateAssetMenu(menuName = "CindarsHope/Farm/Seed")]
public class SeedDataSO : ScriptableObject
{
    public ItemDataSO SeedItem;
    public Sprite[] GrowthStageSprites;   // ← sprites por estágio de crescimento
    public Sprite HarvestedSprite;
    public ItemDataSO[] HarvestItems;
    public int[] HarvestAmounts;
    public int GrowthDays;
    public GrowthPeriod Period;           // Day, Night, Both
    public Season[] ValidSeasons;
    public int MinYield;
    public int MaxYield;
    public float FertilizerYieldMultiplier;
}
```

### 4.3 CreatureDataSO
```csharp
[CreateAssetMenu(menuName = "CindarsHope/Cave/Creature")]
public class CreatureDataSO : ScriptableObject
{
    public string CreatureId;
    public string DisplayName;
    public Sprite IdleSprite;             // ← sprite principal
    public RuntimeAnimatorController Animator; // ← animações
    public int BaseHP;
    public int BaseAttack;
    public int BaseDefense;
    public float MoveSpeed;
    public ActivePeriod ActivePeriod;
    public MoonType[] StrongerDuringMoon;
    public Season[] StrongerDuringSeason;
    public Vector2Int SpawnFloorRange;    // ex: (1, 10)
    public LootEntryDataSO[] LootTable;
    public StatusEffect[] CanApplyStatus;
}
```

### 4.4 RecipeDataSO
```csharp
[CreateAssetMenu(menuName = "CindarsHope/Craft/Recipe")]
public class RecipeDataSO : ScriptableObject
{
    public string RecipeId;
    public WorkshopType Workshop;
    public int WorkshopLevel;
    public IngredientEntry[] Ingredients;
    public ItemDataSO Output;
    public int OutputAmount;
    public float CraftTimeSeconds;
    public bool RequiresBlueprintUnlock;
    public ItemDataSO BlueprintItem;      // null se não precisar
}
```

### 4.5 CompanionDataSO
```csharp
[CreateAssetMenu(menuName = "CindarsHope/Companions/Companion")]
public class CompanionDataSO : ScriptableObject
{
    public string CompanionId;
    public string DisplayName;
    public Sprite Portrait;               // ← busto 64x64 para UI
    public RuntimeAnimatorController Animator;
    public Race Race;
    public CompanionArchetype Archetype;
    public CompanionJob PreferredJob;
    public int BaseHP;
    public int BaseAttack;
    public float JobEfficiencyBonus;
    public bool IsRecruitable;
    public string RecruitQuestId;
    public string[] IdleDialogueKeys;     // chaves para sistema de diálogo
}
```

### 4.6 NPCDataSO
```csharp
[CreateAssetMenu(menuName = "CindarsHope/NPC/NPC")]
public class NPCDataSO : ScriptableObject
{
    public string NpcId;
    public string DisplayName;
    public Sprite Portrait;
    public RuntimeAnimatorController Animator;
    public Race Race;
    public int ShopOpenHour;
    public int ShopCloseHour;
    public ItemDataSO[] ShopInventory;
    public float[] ShopPriceMultipliers;
    public MoonType[] BonusStockDuringMoon; // ex: Yael tem mais estoque em Nyx
    public string[] DialogueKeys;
    public bool IsRecruitable;
    public string RecruitQuestId;
}
```

### 4.7 WorkshopDataSO
```csharp
[CreateAssetMenu(menuName = "CindarsHope/Farm/Workshop")]
public class WorkshopDataSO : ScriptableObject
{
    public WorkshopType Type;
    public string DisplayName;
    public Sprite[] LevelSprites;         // ← sprite por nível 1/2/3
    public BuildCostEntry[] BuildCost;    // custo pra construir (nível 1)
    public BuildCostEntry[][] UpgradeCosts; // custo pra cada upgrade
    public RecipeDataSO[] UnlockedRecipes; // receitas por nível
}
```

### 4.8 BiomeDataSO
```csharp
[CreateAssetMenu(menuName = "CindarsHope/Cave/Biome")]
public class BiomeDataSO : ScriptableObject
{
    public string BiomeId;
    public string DisplayName;
    public Vector2Int FloorRange;         // ex: (1, 10)
    public TileBase[] WallTiles;          // ← tiles do Tilemap
    public TileBase[] FloorTiles;
    public Color AmbientLight;
    public Color FogColor;
    public float FogDensity;
    public StatusEffect PassiveStatusEffect; // ex: Frio no bioma gelo
    public CreatureDataSO[] NativeCreatures;
    public Sprite[] DecorSprites;
}
```

### 4.9 LootTableSO
```csharp
[CreateAssetMenu(menuName = "CindarsHope/Cave/LootTable")]
public class LootTableSO : ScriptableObject
{
    public LootEntry[] Entries;
    // LootEntry: ItemDataSO, float DropChance, int MinAmount, int MaxAmount
}
```

### 4.10 TreeDataSO
```csharp
[CreateAssetMenu(menuName = "CindarsHope/Farm/Tree")]
public class TreeDataSO : ScriptableObject
{
    public string TreeId;
    public Sprite[] ChopLevelSprites;     // ← 4 sprites (nível 0 a 3 de corte)
    public ItemDataSO WoodItem;
    public int DaysToGrow;                // = 5
    public int MaxChopLevels;             // = 4
    public int[] WoodPerChop;             // ex: {10, 15, 22, 30}
    public int ToolLevelRequired;         // ferramenta mínima para cada nível
}
```

---

## 5. State Machine do Personagem

### Estados

```
Idle ──────────────────────► Walking
  │                              │
  ├──► Farming (sub-states)      │
  │     ├── Planting             │
  │     ├── Harvesting           │
  │     ├── Chopping             │
  │     ├── Fishing              │
  │     └── Mining               │
  │                              │
  ├──► Combat (sub-states)   ◄───┘
  │     ├── AttackMelee
  │     ├── AttackRanged
  │     ├── CastSpell
  │     └── Dodging
  │
  ├──► Crafting (parado no workshop)
  ├──► Sleeping
  ├──► Interacting (diálogo NPC)
  └──► Dead ──► [RespawnSystem] ──► Idle
```

```csharp
public abstract class PlayerState
{
    protected PlayerController Player;
    public PlayerState(PlayerController player) => Player = player;
    public virtual void Enter() {}
    public virtual void Tick(float dt) {}
    public virtual void Exit() {}
    public virtual void OnAnimationEvent(string eventName) {}
}

public class PlayerStateMachine
{
    public PlayerState Current { get; private set; }
    public void ChangeState(PlayerState next)
    {
        Current?.Exit();
        Current = next;
        Current.Enter();
    }
    public void Tick(float dt) => Current?.Tick(dt);
}
```

---

## 6. SaveManager

```csharp
[Serializable]
public class SaveData
{
    // Mundo
    public int CurrentDay;
    public int CurrentMonth;
    public int CurrentHour;
    public Season CurrentSeason;
    public string WorldSeed;          // seed da caverna, gerada 1x por save
    public float ReputationScore;

    // Jogador
    public PlayerSaveData Player;
    public List<string> UnlockedRecipes;
    public List<string> UnlockedSkills;

    // Fazenda
    public FarmSaveData Farm;
    public List<WorkshopSaveData> Workshops;
    public List<TreeSaveData> Trees;
    public List<AnimalSaveData> Animals;

    // Caverna
    public int DeepestCaveLevelReached;
    public List<int> UnlockedCheckpoints;

    // Companions
    public List<CompanionSaveData> Companions;

    // Inventário
    public List<InventorySlotData> Inventory;
    public List<string> EquippedItemIds;
}
```

**Gatilhos de save automático:**
- `SceneChangingEvent` → salva antes de mudar de cena
- `CheckpointReachedEvent` → salva ao atingir checkpoint
- `DayStartedEvent` (ao dormir) → salva início de novo dia

---

## 7. Estrutura de Cenas

| Cena | Conteúdo | Carregamento |
|---|---|---|
| `BootScene` | Inicializa Managers, carrega save, redireciona | Automático na abertura |
| `FarmScene` | Fazenda, workshops, lago, árvores, entrada da caverna | Por portal |
| `TownScene` | Cindar's Hope, NPCs, lojas, estalagem | Por portal |
| `CaveScene` | Nível atual da caverna (carregado proceduralmente) | Por portal na fazenda |
| `UIOverlay` | HUD, inventário, menus, diálogos — additive | Sempre carregada |

**Transições:**
```
BootScene
    └──► FarmScene ◄──────────────────────────────┐
              │                                   │
              ├──► TownScene ────────────────────►─┤
              │                                   │
              └──► CaveScene (nível atual) ───────►┘
```

---

## 8. Pipeline Gráfico Completo

### 8.1 Visão geral do fluxo arte → Unity

```
[Ideia / Referência]
        │
        ▼
[Geração de conceito]
 IA generativa (Midjourney / Leonardo.ai / Stable Diffusion)
 → conceito visual, paleta, proporções
        │
        ▼
[Criação do sprite final]
 Aseprite (pixel art manual ou retoque de geração IA)
 → arquivo .aseprite com camadas e animações
        │
        ▼
[Exportação]
 Aseprite → PNG spritesheet (automático via script ou Export Sprite)
 Convenção: NomeDoCritter_Idle_32x32.png
        │
        ▼
[Importação no Unity]
 Sprite Mode: Multiple
 Pixels Per Unit: 32
 Filter Mode: Point (sem blur)
 Compression: None
 → Sprite Slicer automático (grid 32x32)
        │
        ▼
[Referência no ScriptableObject]
 CreatureDataSO.IdleSprite = <sprite>
 CreatureDataSO.Animator = <AnimatorController>
        │
        ▼
[Animator Controller]
 Parâmetros: IsMoving (bool), IsAttacking (bool), IsDead (bool)
 Estados: Idle, Walk, Attack, Hit, Die
 Transições via Animator parameters publicados pelo sistema de combate
        │
        ▼
[Prefab]
 GameObject com: SpriteRenderer, Animator, Collider2D, componentes de sistema
 Prefab referenciado pelo CreatureSpawner via CreatureDataSO
```

### 8.2 Camadas de renderização (Sorting Layers)

```
Background      ← céu, paredes de fundo, chão da caverna
Ground          ← tiles de chão, água, sombras
Decoration      ← plantas, pedras decorativas, árvores (parte de baixo)
Characters      ← jogador, companions, NPCs, criaturas
TreeTops        ← copas de árvores (ficam na frente do personagem)
Items           ← itens dropados no chão
UI_World        ← barras de HP em mundo, balões de texto
UI              ← HUD, inventário, menus
```

### 8.3 Configurações de importação padrão (obrigatório)

```
Texture Type:      Sprite (2D and UI)
Sprite Mode:       Multiple (para spritesheets) / Single (para ícones)
Pixels Per Unit:   32
Filter Mode:       Point  ← CRÍTICO para pixel art não ficar borrado
Compression:       None   ← CRÍTICO para pixel art
Max Size:          2048
Generate Mip Maps: false
```

> Criar um `TextureImporterPreset` no Unity com essas configurações e aplicar em todos os sprites do projeto.

### 8.4 Tilesets — Estrutura

Cada bioma tem seu tileset próprio. Estrutura de tiles por bioma:

| Tile | Quantidade | Descrição |
|---|---|---|
| Floor variants | 4–8 | Variação de chão para evitar repetição |
| Wall full | 1 | Parede sólida |
| Wall variants | 4 | Faces N, S, E, O para bordas corretas |
| Wall corners | 4 | Cantos NE, NO, SE, SO |
| Wall tops | 2 | Topo de parede (visível acima do chão) |
| Decoration | 4–12 | Pedras, fungos, raízes, itens do bioma |
| Doors / Gates | 2–4 | Entrada de nível, salas especiais |

**Fonte dos tilesets:**
- Opção A (recomendada para prototipagem): **itch.io** — buscar "cave tileset 32x32 pixel art"; packs gratuitos de Kenney.nl como base
- Opção B (produção): Criar no Aseprite com IA assistida (ver seção 9)
- Decisão: usar assets prontos até MVP, substituir por arte própria na Fase 10

### 8.5 Ferramenta de pixel art — Aseprite principal

**Decisão:** usar **Aseprite** como ferramenta principal de sprites, animações, tilesets e exportação de spritesheets.

| Ferramenta | Papel | Custo/licença | Uso no projeto |
|---|---|---|---|
| **Aseprite** ✅ | Principal | Pago na Steam/site; source-available, não FOSS | Sprites finais, animações, spritesheets, palettes, tile mode, export PNG/JSON, CLI |
| Pixelorama | Fallback gratuito | Open source | Protótipo, edição simples, alternativa se Aseprite não estiver disponível |
| LibreSprite | Fallback open-source | GPL-2.0, fork do último Aseprite GPL | Alternativa para fluxo 100% FOSS |
| Spine 2D | Futuro opcional | Pago | Só avaliar se houver necessidade de animação com bones; não usar no MVP |

**Regra:** a ferramenta pode mudar, mas o output não muda. O Unity sempre recebe PNG/spritesheet com nomenclatura padronizada, PPU 32, Filter Mode Point e Compression None.

**Direção visual:** cozy farm pixel art inspirado por Harvest Moon/Stardew Valley em termos de câmera, legibilidade e sensação de gênero, mas com paleta e silhuetas próprias de Cindar's Hope/Vaalara.


### 8.6 Animações

Cada personagem / criatura tem um **Animator Controller** com os estados:

| Estado | Frames | Loop |
|---|---|---|
| Idle | 4–6 | Sim |
| Walk | 6–8 | Sim |
| Attack (melee) | 6 | Não |
| Attack (ranged / spell) | 6 | Não |
| Hit | 3 | Não |
| Die | 8 | Não |
| Special (farming, fishing, crafting) | 6–10 | Não |

**Ferramentas:**
- **Aseprite** para criar frames e exportar como spritesheet PNG
- **Unity Animator** para montar o controller
- Parâmetros: `IsMoving`, `IsAttacking`, `IsDead`, `ActionType` (int — farming, fishing, etc.)

**Alternativa avançada:** **Spine 2D** para animações com bones (mais fluido, maior custo de setup) — avaliar na Fase 10.

### 8.6 Efeitos visuais (VFX)

| Efeito | Sistema Unity | Quando usar |
|---|---|---|
| Partículas de colheita | Particle System 2D | CropHarvestedEvent |
| Impacto de ataque | Particle System 2D | Hit detection em combate |
| Magia (fogo, gelo, raio) | Particle System 2D + Sprite animation | CastSpell state |
| Luz das luas | 2D Light (URP) | Sprites de lua no background + ambient light |
| Ciclo dia/noite | Global Light 2D (URP) — muda cor e intensidade | TimeManager: HourChangedEvent |
| Névoa da caverna | URP Full Screen Shader ou Particle | BiomeDataSO.FogDensity |

### 8.7 Iluminação dia/noite

```csharp
// LightingSystem.cs — ouve TimeManager
void OnEnable() => GameEventBus.Subscribe<HourChangedEvent>(OnHourChanged);

void OnHourChanged(HourChangedEvent e)
{
    // Interpola cor e intensidade da Global Light 2D conforme hora
    float t = e.Hour / 24f;
    globalLight.color = dayNightGradient.Evaluate(t);
    globalLight.intensity = Mathf.Lerp(0.2f, 1.0f, daytimeCurve.Evaluate(t));
}
```

---

## 9. IA Generativa na Arte

### 9.1 Para que usar IA

| Tarefa | IA útil? | Ferramenta | Resultado esperado |
|---|---|---|---|
| Conceito de personagem (proporções, paleta) | ✅ Sim | Midjourney, Leonardo.ai | Referência visual para Aseprite |
| Tileset base (chão, paredes) | ✅ Sim | Stable Diffusion + pixel art LoRA | Base para retocar no Aseprite |
| Ícones de itens (32x32) | ✅ Sim | DALL-E 3, Midjourney | Retocar no Aseprite para consistência |
| Sprites de personagem animado | ⚠️ Parcial | Midjourney → Aseprite retoque | Bom para conceito, animação manual |
| UI / HUD | ✅ Sim | Midjourney, Figma + IA | Mockup que vira arte final no Unity |
| Logotipo / título | ✅ Sim | Midjourney + retoque | Arte de título do jogo |
| Música / SFX | 🔜 Fase 10 | Suno, Udio, ElevenLabs | Adiado para polish |

### 9.2 Ferramentas disponíveis (setup do Rafa)

| Ferramenta | Disponível | Uso principal |
|---|---|---|
| **ChatGPT Plus (DALL-E 3)** | ✅ | Ícones de itens 32x32, conceitos rápidos, mockups de NPC |
| **Claude (este)** | ✅ | Prompts refinados, revisão de arte, descrições de personagem para gerar |
| **Codex** | ✅ | Geração de código — não gera arte, mas pode gerar scripts de pipeline |
| **Midjourney** | ❌ não tem | — |
| **Leonardo.ai** | ❌ não tem | — |
| **Stable Diffusion local** | ❌ não tem | — |

**Fluxo com DALL-E 3 via ChatGPT Plus:**
```
1. Prompt no ChatGPT: "pixel art icon 32x32, [item], RPG top-down,
   cozy farm RPG readability, original Cindar's Hope visual identity, warm orange/bronze palette, transparent background,
   no antialiasing, clean silhouette, single item centered"

2. Baixar PNG gerado

3. Abrir no Aseprite
   → reduzir canvas para 32x32 se necessário
   → ajustar paleta para as cores fixas do projeto
   → adicionar outline 1px preto
   → fundo transparente

4. Exportar PNG → Assets/Sprites/Items/

5. Importar no Unity com TextureImporterPreset
```

> **Limitação do DALL-E 3:** não é especializado em pixel art e às vezes gera arte com antialiasing. Sempre retocar no Aseprite para garantir consistência. Para personagens animados, a geração manual no Pixelorama é mais eficiente que tentar usar IA.

### 9.3 SpecKit — integração com geração de arte

| Ferramenta | Tipo | Custo | Uso principal |
|---|---|---|---|
| **Midjourney** | Cloud | ~$10/mês | Conceitos visuais, referências, splash art |
| **Leonardo.ai** | Cloud | Freemium | Sprites e tilesets com modelos pixel art |
| **Stable Diffusion** (local) | Local | Gratuito | Geração em volume, tilesets, variações |
| **DALL-E 3** (via API/ChatGPT) | Cloud | Pay-per-use | Ícones de itens, conceitos rápidos |
| **Aseprite** | Local | ~$20 único | Arte final, retoque, animação |
| **Pixellab** (mobile) | Mobile | Gratuito/Pro | Esboços rápidos no celular |

### 9.3 SpecKit — integração com geração de arte

O GitHub SpecKit é um framework de Spec-Driven Development que usa documentos `.md` estruturados como memória persistente para LLMs. O fluxo é linear: **Specify → Plan → Tasks → Implement**.

**Como usaremos no Cindar's Hope:**

```
Para cada sistema (Farm, Cave, Combat, Craft...):

1. /speckit.specify  → gerar spec do sistema (WHAT + WHY, sem HOW)
2. /speckit.clarify  → perguntas estruturadas para cobrir edge cases
3. /speckit.plan     → plano técnico com constitutional check
4. /speckit.tasks    → quebra em tarefas implementáveis
5. /speckit.implement → Codex implementa tarefa por tarefa
```

**Estrutura de arquivos SpecKit no projeto:**
```
.specify/
  memory/
    constitution.md      ← regras invioláveis do projeto
    context.md           ← referência ao GDD e ARCH
.github/
  prompts/               ← slash commands do SpecKit
    speckit.specify.md
    speckit.plan.md
    speckit.tasks.md
    speckit.implement.md
specs/                   ← gerado na Fase 7
  farm/
    farm-system.spec.md
    seed-system.spec.md
  cave/
    cave-generation.spec.md
    combat-system.spec.md
  ...
```

**constitution.md — regras verificáveis (não vagas):**
```markdown
# Cindar's Hope — Constitution

1. Todo dado de jogo DEVE estar em ScriptableObject
2. Toda comunicação entre sistemas DEVE usar GameEventBus
3. Nenhum script PODE usar GameObject.Find() ou FindObjectOfType()
4. Todo evento DEVE ter Unsubscribe correspondente em OnDisable
5. Commits DEVEM estar em português
6. Nenhuma feature PODE ser implementada sem spec aprovada
7. Sprites DEVEM ser 32x32px com Filter Mode: Point
```

> SpecKit será instalado na Fase 5 (setup do ambiente). A Fase 7 usa SpecKit para gerar todas as specs de sistema.

```
1. PROMPT para IA generativa
   "pixel art 32x32, top-down RPG, [nome do item/criatura],
    style: cozy original farm RPG pixel art, not copying any existing game, warm palette, bronze/orange tones,
    white background, no antialiasing"

2. GERAÇÃO em lote (5–10 variações)
   → escolher a mais próxima do estilo do jogo

3. RETOQUE no Aseprite
   → ajustar paleta para as cores definidas (fazenda: laranja/bronze)
   → corrigir pixels isolados ("pixel noise")
   → adicionar outline consistente de 1px
   → garantir silhueta legível em 32x32

4. EXPORTAÇÃO
   → PNG 32x32 (ícones) ou spritesheet (personagens/animações)
   → nomeação: Category_Name_Size.png (ex: Item_SwordIron_32x32.png)

5. IMPORTAÇÃO no Unity
   → aplicar TextureImporterPreset de pixel art
   → fatiar no Sprite Editor
   → referenciar no ScriptableObject
```

### 9.4 Consistência visual — regras

Para o jogo ter identidade visual coesa mesmo com IA gerando partes:

- **Paleta fixa:** definir paleta de ~32 cores no Aseprite e usar para retocar tudo
  - Fazenda: laranja #D4832A, bronze #8B6914, terra #6B3A2A, verde musgo #4A6741
  - Cidade: azul ardósia #4A5E7A, pedra #7A8A9A, âmbar de lanterna #D4A850
  - Caverna: cinza carvão #2A2A2A, roxo escuro #3A1F4A, cinza úmido #4A4A5A
- **Outline:** sempre 1px preto (#0A0A0A) em personagens e criaturas
- **Tamanho:** personagem jogador = 32x48 (2 tiles de altura), NPCs = 32x48, criaturas variam
- **Sombra:** sombra oval simples embaixo de todos os personagens

---

## 10. Conexão Arte ↔ Mecânica no Unity

### 10.1 Como sprite chega na mecânica

```
ScriptableObject (dados) ──► Prefab (estrutura) ──► Sistema (comportamento)
     │                           │
     │ CreatureDataSO             │ GameObject com:
     │   .IdleSprite ─────────►  │   SpriteRenderer.sprite = SO.IdleSprite
     │   .Animator ────────────► │   Animator.runtimeController = SO.Animator
     │   .BaseHP ──────────────► │   HealthComponent.maxHP = SO.BaseHP
     │   .MoveSpeed ───────────► │   MovementComponent.speed = SO.MoveSpeed
     └───────────────────────────┘
```

### 10.2 Exemplo: Semente no farm

```csharp
public class CropTile : MonoBehaviour
{
    [SerializeField] private SeedDataSO _seedData; // definido no Prefab
    private int _currentGrowthStage = 0;
    private SpriteRenderer _renderer;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
        GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
    }

    void OnDayStarted(DayStartedEvent e)
    {
        bool canGrow = IsPeriodCorrect() && IsSeasonCorrect();
        if (canGrow) AdvanceGrowth();
    }

    void AdvanceGrowth()
    {
        _currentGrowthStage++;
        UpdateSprite();

        if (_currentGrowthStage >= _seedData.GrowthDays)
            GameEventBus.Publish(new CropReadyEvent { TilePosition = transform.position });
    }

    void UpdateSprite()
    {
        // Sprite muda conforme estágio de crescimento — definido no SO
        int spriteIndex = Mathf.Min(_currentGrowthStage, _seedData.GrowthStageSprites.Length - 1);
        _renderer.sprite = _seedData.GrowthStageSprites[spriteIndex];
    }
}
```

### 10.3 Exemplo: Ciclo dia/noite no visual

```csharp
// TimeVisualSystem.cs — só cuida do visual, não da mecânica
public class TimeVisualSystem : MonoBehaviour
{
    [SerializeField] private Light2D _globalLight;
    [SerializeField] private Gradient _dayNightGradient;
    [SerializeField] private AnimationCurve _intensityCurve;

    void OnEnable() => GameEventBus.Subscribe<HourChangedEvent>(OnHourChanged);
    void OnDisable() => GameEventBus.Unsubscribe<HourChangedEvent>(OnHourChanged);

    void OnHourChanged(HourChangedEvent e)
    {
        float t = e.Hour / 24f;
        _globalLight.color = _dayNightGradient.Evaluate(t);
        _globalLight.intensity = _intensityCurve.Evaluate(t);
    }
}
```

### 10.4 Exemplo: Workshop — sprite por nível

```csharp
public class WorkshopVisual : MonoBehaviour
{
    [SerializeField] private WorkshopDataSO _data;
    private SpriteRenderer _renderer;

    void OnEnable() => GameEventBus.Subscribe<WorkshopUpgradedEvent>(OnUpgraded);
    void OnDisable() => GameEventBus.Unsubscribe<WorkshopUpgradedEvent>(OnUpgraded);

    void OnUpgraded(WorkshopUpgradedEvent e)
    {
        if (e.WorkshopType != _data.Type) return;
        _renderer.sprite = _data.LevelSprites[e.NewLevel - 1]; // 0-indexed
    }
}
```

---

## 11. Geração de Sprites — Fluxo Completo por Tipo

### 11.1 Personagem jogador (Anão / Humano)

```
1. Prompt Midjourney:
   "pixel art RPG character sprite sheet, 32x48px, top-down,
    [dwarf/human] farmer adventurer, idle walk attack animations,
    Stardew Valley art style, warm bronze palette, white background"

2. Retoque Aseprite:
   → ajustar para paleta fixa do projeto
   → 4 direções (up, down, left, right) — ou 2 (down, side) com flip horizontal
   → animar: Idle (4f), Walk (6f), Attack (6f), Tool Use (6f)

3. Spritesheet final:
   Tamanho: 32x48 por frame
   Layout: 1 linha por animação, 1 coluna por frame
   Ex: Player_Human_Female_32x48.png

4. Unity:
   Sprite Mode: Multiple
   Slice: Grid By Cell Size 32x48
   Criar AnimatorController com todos os estados
```

### 11.2 Criaturas da caverna

```
1. Por bioma: usar BiomeDataSO.BiomeId como contexto do prompt
   "pixel art monster sprite, 32x32, [biome: stone cave / ice cave / fire cave],
    [creature type], top-down RPG, dark palette, white background"

2. Mínimo por criatura: Idle (4f), Walk (4f), Attack (4f), Die (6f)

3. Variantes do mesmo sprite base para criaturas de bioma similar
   (economiza tempo — cave slime azul e verde são o mesmo sprite com paleta swap)
```

### 11.3 Ícones de itens (32x32)

```
1. DALL-E 3 ou Leonardo.ai gera ícones individuais
   Prompt: "pixel art icon 32x32, [item name], RPG item, dark background,
            clean silhouette, detailed, no antialiasing"

2. Aseprite: ajustar paleta + outline 1px + fundo transparente

3. Exportar como PNG individual, nomear: Item_SwordIron_32x32.png

4. Unity: Sprite Mode Single, PPU 32
```

### 11.4 Tiles de ambiente

```
1. Stable Diffusion com modelo pixel art (LoRA: "pixel art tileset")
   Prompt: "pixel art tileset 32x32, top-down RPG, [biome],
            seamless floor tiles, wall tiles, dark palette"

2. Recortar e retocar no Aseprite
   → garantir que tiles de chão fazem seamless
   → adicionar variações (4–8 tiles de chão para o mesmo bioma)

3. Importar no Unity como Sprite Multiple
   → configurar no Tile Palette do Tilemap
   → usar Rule Tile para paredes automáticas (Unity 2D Extras)
```

### 11.5 NPCs e Companions

```
Same pipeline do personagem jogador.
Diferença: companions têm portrait 64x64 para UI (busto com expressão).

Prompt portrait: "pixel art character portrait 64x64, RPG, [race] [class],
                  [personality: fierce/kind/mysterious], cozy farm RPG readability, original Cindar's Hope visual identity"
```

---

## 12. Padrões Obrigatórios — CLAUDE.md (esqueleto)

Este é o conteúdo que vai no `CLAUDE.md` na raiz do repositório. Será finalizado na Fase 5.

```markdown
# CLAUDE.md — Cindar's Hope

## Contexto do projeto
- Unity LTS, C#, pixel art 32x32, 1280x720
- RPG dungeon crawler + farm sim ambientado em Vaalara (ver GDD_v2.6.md)

## Regras obrigatórias de código

1. NUNCA usar GameObject.Find() ou FindObjectOfType()
2. NUNCA criar comunicação direta entre sistemas — sempre via GameEventBus
3. NUNCA hardcodar dados de jogo — sempre ScriptableObject
4. SEMPRE unsubscribe de eventos em OnDisable/OnDestroy
5. SEMPRE prefixar SOs com o tipo: ItemDataSO, SeedDataSO, etc.
6. SEMPRE prefixar eventos com ação no passado: DayStartedEvent, PlayerDiedEvent
7. Coroutines apenas em MonoBehaviours — lógica de negócio em classes C# puras
8. Um Manager por responsabilidade — sem God Objects
9. Sprites SEMPRE referenciados via ScriptableObject, nunca direto no script

## Modelo padrão para código gerado
claude-sonnet-4-6 (sempre)

## Estrutura de pastas
Assets/_Game/Scripts/[Sistema]/ para novos scripts
Assets/_Game/Data/[Tipo]/ para novos ScriptableObjects

## Convenções de nomenclatura
- Classes: PascalCase (PlayerController, FarmSystem)
- Eventos: [Acao][Substantivo]Event (DayStartedEvent, ItemCraftedEvent)
- ScriptableObjects: [Tipo]DataSO (ItemDataSO, SeedDataSO)
- Prefabs: [Categoria]_[Nome] (Creature_Slime, NPC_Brumdar)
- Sprites: [Categoria]_[Nome]_[Tamanho].png (Item_SwordIron_32x32.png)
- Variáveis private: _camelCase
- Variáveis public/SerializeField: PascalCase

## Modelo de ScriptableObject mínimo
[CreateAssetMenu(fileName = "X_", menuName = "CindarsHope/[Categoria]/[Tipo]")]
public class [Tipo]DataSO : ScriptableObject { ... }

## EventBus: padrão de uso
void OnEnable() => GameEventBus.Subscribe<XEvent>(OnX);
void OnDisable() => GameEventBus.Unsubscribe<XEvent>(OnX);
```

---

## 13. Próximos Passos — Fase 5 (Estruturação do Ambiente)

### Checklist da Fase 5

**Repositório:**
- [ ] Criar repositório Git (GitHub recomendado)
- [ ] Configurar `.gitignore` para Unity
- [ ] Configurar Git LFS: `*.png, *.aseprite, *.wav, *.mp3, *.ogg, *.psd`
- [ ] Criar branch `main` (estável) e `dev` (desenvolvimento)

**Unity:**
- [ ] Instalar Unity LTS mais recente (verificar versão LTS atual em unity.com/releases)
- [ ] Criar projeto com template **2D (URP)**
- [ ] Instalar packages: `Input System`, `Cinemachine`, `TextMeshPro`, `2D Extras` (Rule Tile)
- [ ] Instalar `SuperTiled2Unity` via Package Manager
- [ ] Criar `TextureImporterPreset` de pixel art (configurações da seção 8.3)
- [ ] Criar Sorting Layers (seção 8.2)
- [ ] Configurar resolução: 1280x720, aspect ratio locked

**Estrutura de pastas:**
```
Assets/
├── _Game/
│   ├── Data/
│   │   ├── Items/
│   │   ├── Seeds/
│   │   ├── Creatures/
│   │   ├── Recipes/
│   │   ├── Companions/
│   │   ├── NPCs/
│   │   ├── Workshops/
│   │   ├── Biomes/
│   │   ├── LootTables/
│   │   ├── Quests/
│   │   └── Config/
│   ├── Scripts/
│   │   ├── Core/         (GameEventBus, SaveManager, TimeManager, Events/)
│   │   ├── Player/
│   │   ├── Farm/
│   │   ├── Cave/
│   │   ├── Combat/
│   │   ├── Craft/
│   │   ├── Companion/
│   │   ├── NPC/
│   │   ├── UI/
│   │   └── Utils/
│   ├── Scenes/
│   ├── Prefabs/
│   │   ├── Characters/
│   │   ├── Creatures/
│   │   ├── Farm/
│   │   ├── UI/
│   │   └── VFX/
│   ├── Sprites/
│   │   ├── Characters/
│   │   ├── Creatures/
│   │   ├── Items/
│   │   ├── Tilesets/
│   │   ├── UI/
│   │   └── VFX/
│   ├── Animations/
│   ├── Tilemaps/
│   └── Audio/            (placeholder)
├── ThirdParty/
└── PersistentDataPath/   (saves JSON locais; nunca Application.persistentDataPath)
```

**CLAUDE.md:**
- [ ] Criar `CLAUDE.md` na raiz com conteúdo da seção 12
- [ ] Criar `AGENTS.md` com mesmo conteúdo (para outros agentes)

---

*v2.0 — Arquitetura técnica completa com pipeline gráfico, IA generativa, fluxo arte↔mecânica, geração de sprites e padrões para Codex. Fase 4 concluída. Próxima: Fase 5 — Estruturação do Ambiente.*


---

## 12. Contratos Core obrigatórios antes do MVP

Esta seção torna explícitos os contratos que precisam existir antes de `InventoryManager`, `FarmSystem`, `SaveManager` e `SellPoint` ficarem estáveis.

Documento detalhado: `CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`.

### 12.1 IDs estáveis para dados de jogo

Todo `ScriptableObject` persistível deve ter um ID textual estável, único e imutável após publicado.

Exemplos:

```text
item_seed_wheat
item_seed_carrot
item_crop_wheat
item_crop_carrot
item_wood
item_common_fish
seed_wheat
seed_carrot
tree_oak
```

Regras:

- `DisplayName` pode mudar; `Id` não.
- Save nunca salva referência de Unity.
- Save salva apenas IDs e quantidades.
- Registries resolvem ID → ScriptableObject durante load.

### 12.2 Registry de ScriptableObjects

Criar registries antes do save final:

```csharp
public interface IDataRegistry<T>
{
    bool TryGetById(string id, out T data);
    T GetRequired(string id);
    IReadOnlyCollection<T> All { get; }
}
```

Implementações mínimas:

- `ItemDatabaseSO`
- `SeedDatabaseSO`
- `TreeDatabaseSO`

No MVP, os registries podem ser assets manuais com arrays serializados. Depois, podem evoluir para Addressables ou carregamento automático por pasta.

### 12.3 Eventos como contratos

Eventos são payloads de integração. Não devem carregar `GameObject`, `MonoBehaviour`, `Transform` ou referência pesada de cena.

Preferir:

```csharp
public readonly struct CropHarvestedEvent
{
    public readonly string SeedId;
    public readonly string ItemId;
    public readonly int Amount;
    public readonly Vector2Int TilePosition;

    public CropHarvestedEvent(string seedId, string itemId, int amount, Vector2Int tilePosition)
    {
        SeedId = seedId;
        ItemId = itemId;
        Amount = amount;
        TilePosition = tilePosition;
    }
}
```

Evitar:

```csharp
public class CropHarvestedEvent
{
    public CropTile Tile;
    public ItemDataSO Item;
}
```

### 12.4 Save path correto

Save deve escrever em:

```csharp
Path.Combine(Application.persistentDataPath, "saves", "slot_1.json")
```

Não usar `StreamingAssets` para save editável. `StreamingAssets` serve para dados empacotados com o build; em várias plataformas ele não é local de escrita apropriado.

### 12.5 Versionamento de save

Todo save deve ter schema version:

```csharp
[Serializable]
public class SaveData
{
    public int SchemaVersion = 1;
    public string GameVersion = "0.1.0-mvp";
    public int CurrentDay;
    public PlayerSaveData Player;
    public InventorySaveData Inventory;
    public FarmSaveData Farm;
}
```

Mesmo no MVP, isso evita quebra quando o projeto crescer.

### 12.6 Load sequence

Ordem obrigatória de boot:

```text
BootScene
  1. Inicializa registries de dados
  2. Inicializa managers persistentes
  3. SaveManager verifica save existente
  4. Se não existe: cria NewGameState a partir dos SOs iniciais
  5. Se existe: desserializa JSON e resolve IDs via registries
  6. Carrega FarmScene
  7. FarmSystem aplica estado nos canteiros/árvores
  8. UIOverlay lê managers e renderiza HUD
```

---

## 13. Execução com Codex — padrão arquitetural

Documento detalhado: `FASE8_EXECUTION_PLAN_CODEX_v1.0.md`.

### 13.1 Unidade de trabalho

A unidade de trabalho não é “uma spec inteira”. A unidade é um PR pequeno.

Cada PR deve conter:

- objetivo único;
- arquivos permitidos;
- arquivos proibidos;
- critérios de done;
- teste manual;
- rollback simples.

### 13.2 O que Codex pode fazer

- Criar scripts C#.
- Criar Editor scripts.
- Criar testes EditMode/PlayMode.
- Refatorar código já criado.
- Gerar arquivos `.md` operacionais.
- Criar placeholders programáticos simples quando especificado.

### 13.3 O que Codex não deve fazer sem aprovação

- Alterar lore/GDD.
- Trocar arquitetura de eventos por referências diretas.
- Trocar ScriptableObjects por JSON hardcoded.
- Criar cenas complexas sem Editor script ou checklist manual.
- Implementar V2/FULL junto com MVP.
- Incluir packages externos sem registrar no ambiente.
- Usar assets protegidos de terceiros.

### 13.4 Preferir automação de Editor

Sempre que uma spec exigir muita configuração manual no Unity, preferir criar scripts em `Assets/Editor/` para gerar:

- pastas;
- cenas base;
- prefabs placeholder;
- ScriptableObjects iniciais;
- tiles placeholder;
- presets de importação.

Isso deixa o trabalho do Codex reproduzível e reduz passos manuais invisíveis.

---

## 14. Pipeline de sprites com IA e Aseprite

Documento detalhado: `SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md`.

Resumo da regra:

```text
IA gera conceito/rascunho → Aseprite limpa e padroniza → Unity importa com preset → ScriptableObject referencia o sprite
```

A IA nunca é fonte final de verdade visual. O arquivo final aprovado é o `.aseprite/.ase` e o PNG exportado revisado.
