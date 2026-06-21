# Rule: Estabilidade de ID

IDs de domínio aparecem em save files, catálogos, specs e game_rules. Uma vez que um ID existe em qualquer save file, ele não pode ser renomeado sem uma migration explícita.

## O que é um ID de domínio

Qualquer `string` que identifica uma entidade de jogo de forma persistida e cruzada:

- Save DTOs (`SkillTreeSaveData`, `QuestSaveData`, `InventorySaveData`, etc.)
- Catálogos de dados (`FarmAnimalCatalog`, `DefaultSkillCatalog`, `CanonicalBestiaryCatalog`)
- Registries de NPCs, quests, items, animais, receitas, buffs, enemies
- Specs e game_rules que referenciam IDs por nome

Exemplos: `"item_crop_carrot"`, `"animal_chicken"`, `"quest_first_supplies"`, `"node_warrior_heavy_strike_1"`, `"npc_elara"`.

## Regra de definição

Todo ID de domínio deve ser `public const string` no catalog canônico:

```csharp
// CORRETO — single source of truth
public static class FarmAnimalCatalog
{
    public const string Chicken = "animal_chicken";
}

// CORRETO — usar a const no código de gameplay (nunca o literal)
if (animalId == FarmAnimalCatalog.Chicken) { ... }

// ERRADO — literal no código (quebra silenciosamente se a const for renomeada)
if (animalId == "animal_chicken") { ... }
```

## Regra de renaming

1. **Nunca renomear** um ID que pode estar em save files sem:
   - Migration explícita em `SaveMigrationService` (ou equivalente)
   - Documentação em `docs/decisions/` se a mudança for ampla

2. **Nunca remover** um ID de um catalog sem verificar que nenhum save file o referencia.

3. **Prefixo de domínio obrigatório**: `"{dominio}_{noun}"` — evita colisão entre domínios.
   - Prefixos canônicos: `"item_"`, `"animal_"`, `"quest_"`, `"node_"`, `"npc_"`, `"recipe_"`, `"buff_"`, `"enemy_"`

## O que nunca acontece

- Renomear uma `const string` de ID como "refactor de naming" sem migration.
- Criar IDs via `Guid.NewGuid()` ou `DateTime.Now` — instáveis, não-legíveis.
- Usar o literal string no código de gameplay em vez da const do catalog.

## Caso especial: Cave

Cave IDs (`CaveRunSeed`, `CaveLevel`, entity IDs) têm restrição ainda mais forte — ver rule `cave-stable-run`. São determinísticos por seed e nunca podem usar GUIDs ou timestamps.

## Enforcement

- Revisional: `/review-non-regression` e o agent `non-regression-auditor` verificam renames de const strings de ID.
- A skill `registry-catalog-pattern` ensina como definir IDs corretamente.
- Sem hook mecânico (falsos positivos muito altos para um grep de strings).
