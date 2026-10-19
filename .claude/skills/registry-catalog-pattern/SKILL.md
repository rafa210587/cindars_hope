---
name: registry-catalog-pattern
description: Pattern de static catalog/registry com const IDs, TryGet, IReadOnlyList All e lazy init. Usar quando spec cria ou estende um catalog de dados (items, animais, NPCs, criaturas, quests, skills, receitas) ou quando spec faz lookup por ID de domínio estável.
---

# Skill: Registry & Catalog Pattern

O projeto tem ~8 catálogos de dados que seguem o mesmo shape: `static class`, IDs como `const string`, `TryGet`, e `All` read-only. Seguir o pattern evita criar catálogo com mutabilidade desnecessária, IDs instáveis, ou lookups que lançam exceção. Exemplos canônicos: `FarmAnimalCatalog`, `CanonicalBestiaryCatalog`, `DefaultSkillCatalog`, `NpcTownRosterRegistry`.

## Quando usar

- Spec cria novo tipo de dado de domínio (animal, item, criatura, NPC, receita, buff) que precisa de lookup por ID.
- Spec estende um catálogo existente com novos entries.
- Spec faz `TryGetById` ou precisa iterar `All`.
- Spec menciona "catalog", "registry", "roster", "database", "entry", "lookup por id".

## Shapes canônicos (3 variações — escolha pela escala)

### Shape 1 — Catalog pequeno (<100 entries), dados inline

```csharp
public static class FarmAnimalCatalog
{
    // IDs estáveis como const — NUNCA renomear sem migration (rule: id-stability)
    public const string Chicken = "animal_chicken";
    public const string Goat    = "animal_goat";

    public readonly struct Entry
    {
        public readonly string Id;
        public readonly string DisplayNameKey; // chave de localização
        public readonly int    BaseYield;
    }

    private static readonly List<Entry> s_entries = new List<Entry>
    {
        new Entry { Id = Chicken, DisplayNameKey = "animal.chicken.name", BaseYield = 1 },
        new Entry { Id = Goat,    DisplayNameKey = "animal.goat.name",    BaseYield = 2 },
    };

    public static IReadOnlyList<Entry> All => s_entries;

    public static bool TryGet(string id, out Entry entry)
    {
        foreach (var e in s_entries) { if (e.Id == id) { entry = e; return true; } }
        entry = default;
        return false;
    }
}
```

### Shape 2 — Catalog grande, lazy init + partial class

```csharp
// CanonicalBestiaryCatalog.cs
public static partial class CanonicalBestiaryCatalog
{
    private static List<BestiaryCreatureDef> _all;

    public static IReadOnlyList<BestiaryCreatureDef> All
    {
        get
        {
            if (_all != null) return _all;
            var list = new List<BestiaryCreatureDef>(96);
            list.AddRange(BandStone());   // definido em BandStone.cs
            list.AddRange(BandFungal());  // definido em BandFungal.cs
            _all = list;
            return _all;
        }
    }
}

// BandStone.cs
public static partial class CanonicalBestiaryCatalog
{
    private static IEnumerable<BestiaryCreatureDef> BandStone() { /* entries */ }
}
```

### Shape 3 — Factory com post-passes (dados derivados de múltiplas fontes)

```csharp
public static class DefaultSkillCatalog
{
    public const int CanonicalNodeCount = 69;

    public static List<SkillNodeDataSO> BuildAllNodes()
    {
        var nodes = new List<SkillNodeDataSO>();
        nodes.AddRange(BuildMeleeNodes());
        nodes.AddRange(BuildRangedNodes());
        ApplyTiers(nodes);         // post-pass: metadados keyed by ID
        ApplyEffectRoutes(nodes);
        return nodes;
    }

    // Post-pass: Dictionary<string, T> keyed by const ID
    private static readonly Dictionary<string, int> NodeTiers = new Dictionary<string, int>
    {
        { "node_warrior_heavy_strike_1", 2 },
    };

    private static void ApplyTiers(List<SkillNodeDataSO> nodes)
    {
        foreach (var n in nodes)
            n.Tier = NodeTiers.TryGetValue(n.SkillNodeId, out var t) ? t : 1; // fallback seguro
    }
}
```

## Regras do pattern

### IDs (ver rule: id-stability)

- Sempre `public const string X = "dominio_noun"` no catalog — a const é a source of truth.
- Convenção: `"{dominio}_{substantivo}"` — ex.: `"animal_chicken"`, `"item_crop_carrot"`, `"quest_first_supplies"`.
- Gameplay usa a const (`FarmAnimalCatalog.Chicken`), nunca o literal (`"animal_chicken"`).
- **IDs que aparecem em saves não podem ser renomeados sem migration.**

### TryGet vs GetRequired

| Método | Retorno | Quando usar |
|---|---|---|
| `TryGet(id, out T entry)` | false + default | ID ausente é gameplay normal (Category 1) |
| `GetRequired(id)` → T ou throw | throw `InvalidOperationException` | ID ausente é bug de wiring (Category 4) |

Nunca retornar `null` de TryGet que promete struct — usar `default`.

### Mutabilidade

- `All` expõe `IReadOnlyList<T>` ou `IReadOnlyCollection<T>` — jamais `List<T>` diretamente.
- State que muda em runtime (ex.: estado de NPC já visto) vai em um Registry separado; o Catalog é imutável.

### Performance de lookup

- Até ~100 items: linear search é correto (legível, sem alocação extra).
- Acima de 100 ou hot-path: construa `Dictionary<string, T>` lazy no primeiro `TryGet`.

## Quando NÃO usar

- Dados com estado mutável em runtime → Registry separado ou `SaveSection`.
- Dados configuráveis pelo designer no Editor → ScriptableObject database (skill `data-catalog-authoring`).
- Singleton com lógica operacional complexa → não é catalog, é um service.

## Testes

Catálogos são pure C# → testáveis em EditMode:

```csharp
[Test]
public void FarmAnimalCatalog_TryGet_Known_ReturnsTrue()
{
    Assert.IsTrue(FarmAnimalCatalog.TryGet(FarmAnimalCatalog.Chicken, out var entry));
    Assert.AreEqual(FarmAnimalCatalog.Chicken, entry.Id);
}

[Test]
public void FarmAnimalCatalog_TryGet_Unknown_ReturnsFalse()
{
    Assert.IsFalse(FarmAnimalCatalog.TryGet("animal_dragon", out _));
}

[Test]
public void FarmAnimalCatalog_All_NotEmpty()
{
    Assert.IsTrue(FarmAnimalCatalog.All.Count > 0);
}
```

## Relacionados

- `(rule: id-stability)` — IDs como const; nunca renomear sem migration
- `(skill: data-catalog-authoring)` — quando o catalog é ScriptableObject no Editor
- `(skill: save-load-pattern)` — IDs do catalog são o que vai/vem do save DTO
- `(rule: unity-architecture)` — save DTOs contêm IDs (string), nunca refs Unity
- `(rule: error-handling-resilience)` — Category 1 vs Category 4 em lookups
