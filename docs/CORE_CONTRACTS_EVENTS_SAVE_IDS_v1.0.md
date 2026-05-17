# Cindar's Hope — Contratos Core: Eventos, IDs, Registries e Save v1.0

> **Status:** obrigatório antes do MVP Fazenda estabilizar  
> **Depende de:** `ARCH_fase4_v2.2.md`, `FASE7_SPEC_MVP_FARM_v2.2.md`  
> **Objetivo:** evitar rework de save, inventário, plantio e comunicação entre sistemas

---

## 1. Problema que este documento resolve

Unity facilita referências diretas entre objetos, mas isso quebra rápido quando entram save/load, cenas múltiplas e agentes gerando código.

Este projeto deve evitar:

- salvar referência de `ScriptableObject` em JSON;
- salvar `GameObject`, `Transform` ou `MonoBehaviour`;
- procurar objetos por nome em runtime;
- depender de ordem invisível de cena;
- deixar cada sistema inventar seu próprio formato de evento.

---

## 2. Regra central

```text
Runtime usa referências Unity.
Save usa IDs e valores simples.
Load resolve IDs via registries.
Sistemas conversam por eventos.
```

---

## 3. IDs estáveis

### 3.1 Convenção

| Tipo | Prefixo | Exemplo |
|---|---|---|
| Item | `item_` | `item_crop_wheat` |
| Semente | `seed_` | `seed_wheat` |
| Árvore | `tree_` | `tree_oak` |
| Peixe | `item_fish_` | `item_fish_common` |
| Ferramenta | `item_tool_` | `item_tool_basic_rod` |
| Receita | `recipe_` | `recipe_simple_healing_potion` |
| NPC | `npc_` | `npc_pip_miudinho` |
| Companion | `companion_` | `companion_zrix` |

### 3.2 Regras

- ID é minúsculo, em inglês técnico, com `_`.
- `DisplayName` pode ser português/lore; ID não muda.
- ID não deve conter acento.
- ID não deve conter espaço.
- ID não deve ser renomeado depois de existir save.

---

## 4. Interface para dados identificados

```csharp
public interface IIdentifiedData
{
    string Id { get; }
}
```

Exemplo:

```csharp
[CreateAssetMenu(menuName = "CindarsHope/Items/Item")]
public class ItemDataSO : ScriptableObject, IIdentifiedData
{
    [SerializeField] private string _id;
    public string Id => _id;

    public string DisplayName;
    [TextArea] public string Description;
    public Sprite Icon;
    public ItemCategory Category;
    public int MaxStack = 99;
    public int BaseValue;
    public int HungerRestore;
    public bool IsEquippable;
}
```

---

## 5. Registries

### 5.1 Interface

```csharp
public interface IDataRegistry<T> where T : IIdentifiedData
{
    bool TryGetById(string id, out T data);
    T GetRequired(string id);
    IReadOnlyCollection<T> All { get; }
}
```

### 5.2 Implementação base

```csharp
public abstract class DataRegistrySO<T> : ScriptableObject, IDataRegistry<T>
    where T : ScriptableObject, IIdentifiedData
{
    [SerializeField] private T[] _items;
    private Dictionary<string, T> _byId;

    public IReadOnlyCollection<T> All => _items;

    private void OnEnable()
    {
        _byId = new Dictionary<string, T>();
        foreach (var item in _items)
        {
            if (item == null) continue;
            if (string.IsNullOrWhiteSpace(item.Id))
                Debug.LogError($"Data sem Id em {item.name}");
            else if (_byId.ContainsKey(item.Id))
                Debug.LogError($"Id duplicado: {item.Id}");
            else
                _byId[item.Id] = item;
        }
    }

    public bool TryGetById(string id, out T data) => _byId.TryGetValue(id, out data);

    public T GetRequired(string id)
    {
        if (TryGetById(id, out var data)) return data;
        throw new KeyNotFoundException($"Data id não encontrado: {id}");
    }
}
```

### 5.3 Registries do MVP

```csharp
[CreateAssetMenu(menuName = "CindarsHope/Database/Items")]
public class ItemDatabaseSO : DataRegistrySO<ItemDataSO> {}

[CreateAssetMenu(menuName = "CindarsHope/Database/Seeds")]
public class SeedDatabaseSO : DataRegistrySO<SeedDataSO> {}
```

---

## 6. Contratos de eventos

### 6.1 Regras

Eventos devem:

- ser pequenos;
- carregar tipos simples;
- carregar IDs em vez de SOs quando possível;
- não carregar `GameObject`, `Transform`, `MonoBehaviour`;
- não executar lógica.

### 6.2 Eventos mínimos do MVP

```csharp
public readonly struct DayStartedEvent
{
    public readonly int DayNumber;
    public DayStartedEvent(int dayNumber) => DayNumber = dayNumber;
}

public readonly struct GoldChangedEvent
{
    public readonly int Delta;
    public readonly int NewTotal;
    public GoldChangedEvent(int delta, int newTotal)
    {
        Delta = delta;
        NewTotal = newTotal;
    }
}

public readonly struct InventoryChangedEvent
{
    public readonly string ItemId;
    public readonly int NewAmount;
    public InventoryChangedEvent(string itemId, int newAmount)
    {
        ItemId = itemId;
        NewAmount = newAmount;
    }
}

public readonly struct SeedPlantedEvent
{
    public readonly string SeedId;
    public readonly Vector2Int TilePosition;
    public SeedPlantedEvent(string seedId, Vector2Int tilePosition)
    {
        SeedId = seedId;
        TilePosition = tilePosition;
    }
}

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

---

## 7. Save path

### 7.1 Caminho correto

```csharp
public static class SavePaths
{
    public static string SaveDirectory => Path.Combine(Application.persistentDataPath, "saves");
    public static string SlotPath(int slot) => Path.Combine(SaveDirectory, $"slot_{slot}.json");
}
```

### 7.2 Por que não StreamingAssets

`StreamingAssets` é para arquivos empacotados com o build. O save do jogador é dado local editável, portanto deve ir para `Application.persistentDataPath`.

---

## 8. DTOs de save do MVP

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

[Serializable]
public class PlayerSaveData
{
    public int CurrentHP;
    public int MaxHP;
    public int Gold;
    public int CurrentHunger;
    public Vector2 PlayerPosition;
}

[Serializable]
public class InventorySaveData
{
    public List<InventorySlotSaveData> Slots = new();
}

[Serializable]
public class InventorySlotSaveData
{
    public string ItemId;
    public int Amount;
}

[Serializable]
public class FarmSaveData
{
    public List<PlotSaveData> Plots = new();
    public List<TreeSaveData> Trees = new();
}

[Serializable]
public class PlotSaveData
{
    public int PlotIndex;
    public string SeedId;
    public int DaysGrown;
    public string State; // Empty, Growing, Ready
}

[Serializable]
public class TreeSaveData
{
    public int TreeIndex;
    public int ChopLevel;
    public bool IsActive;
}
```

---

## 9. Load sequence detalhada

```text
BootScene
  ↓
GameBootstrap cria managers persistentes
  ↓
DataRegistryProvider recebe ItemDatabaseSO e SeedDatabaseSO
  ↓
SaveManager verifica slot_1.json
  ↓
Se não existe:
  cria NewGameState usando PlayerDataSO.StartingItems
Se existe:
  desserializa JSON
  valida SchemaVersion
  resolve ItemId/SeedId via registries
  ↓
Carrega FarmScene
  ↓
FarmSystem recebe PlotSaveData e TreeSaveData
  ↓
InventoryUI/HUD renderizam estado dos managers
```

---

## 10. Testes mínimos

### EditMode

- `ItemDatabase_GetRequired_ReturnsItem_WhenIdExists`
- `ItemDatabase_GetRequired_Throws_WhenIdMissing`
- `Inventory_AddItem_StacksSameItem`
- `Inventory_RemoveItem_Fails_WhenInsufficientAmount`
- `SaveData_Serializes_WithoutUnityReferences`

### PlayMode

- `TimeManager_AdvanceDay_PublishesDayStartedEvent`
- `CropTile_PlantedSeed_GrowsAfterDayStartedEvent`
- `SaveLoad_RestoresInventoryGoldAndPlots`

---

## 11. Critérios de rejeição

Rejeitar implementação se:

- JSON tiver campo de `ScriptableObject` direto.
- Save path usar `StreamingAssets`.
- Evento carregar `GameObject` sem motivo explícito.
- Registry permitir IDs duplicados sem erro.
- Load falhar silenciosamente em ID faltante.
- Sistema depender de busca por nome de objeto.
