# Refinement — Estabilização pós-execução overnight das specs FASE9C–FASE9L

> **Status:** Refinement futuro / estabilização técnica  
> **Data:** 2026-05-23  
> **Branch analisada:** `wave/specs-overnight-07-ui-menu-minimal`  
> **Base de comparação:** `dev`  
> **Spec relacionada:** `docs/specs/a_implementar/spec_stabilization_overnight_fase9c_to_fase9l_v1.md`

---

## 1. Objetivo

Este refinement consolida a análise da execução overnight das specs FASE9C–FASE9L e define o que precisa ser corrigido antes de qualquer merge em `dev`.

A branch gerou material útil, mas o estado real observado é:

```text
backend/data skeleton + initializers + documentação gerada
```

Ela não deve ser tratada como implementação completa das specs de gameplay.

---

## 2. Diagnóstico consolidado

### 2.1 Documentação superestima o estado real

`docs/IMPLEMENTATION_DELIVERY_20260523.md` declara `COMPLETE` e `13 Specifications Fully Implemented`, mas a análise mostra que muitas entregas são apenas schemas, SOs, managers mínimos ou initializers.

Correção:

```text
Status correto: Implementado parcial — backend/data skeleton; integração runtime e validação Unity pendentes.
```

### 2.2 Registries não refletem a execução

`SPEC_REGISTRY_TO_IMPLEMENT.md` continua listando como `A implementar` specs que foram copiadas para `docs/specs/implementados/`.

`SPEC_REGISTRY_IMPLEMENTED.md` não recebeu as novas specs com status correto.

Correção:

- specs com skeleton devem ser marcadas como `Implementado parcial`;
- specs sem runtime devem continuar como futuras ou parcialmente implementadas;
- nunca manter uma spec simultaneamente como `A implementar` e `Implementado` sem explicar o estado.

### 2.3 Specs em `implementados` ainda parecem futuras

Algumas specs copiadas para `docs/specs/implementados/` ainda carregam semântica de spec futura.

Correção:

Cada spec implementada/parcial deve ter:

```text
Estado real implementado
Evidência no repo
Pendências
Validação Unity
Próximos passos
```

---

## 3. Regressões técnicas identificadas

### 3.1 `ItemCategory` reordenado

Em `dev`, o enum era:

```csharp
Seed,
Crop,
Food,
Material,
Tool,
Fish,
Misc
```

Na branch, foi alterado para iniciar com `None` e substituir `Food` por `Consumable`.

Risco:

Unity serializa enums por valor numérico. Inserir valor antes dos antigos desloca assets existentes silenciosamente.

Correção esperada:

```csharp
public enum ItemCategory
{
    Seed = 0,
    Crop = 1,
    Food = 2,
    Material = 3,
    Tool = 4,
    Fish = 5,
    Misc = 6,

    None = 100,
    Consumable = 101,
    Weapon = 102,
    Magic = 103,
    Ammo = 104,
    Ore = 105,
    Gem = 106,
    MonsterDrop = 107,
    Quest = 108,
    KeyItem = 109,
    Furniture = 110
}
```

`Food` deve permanecer como categoria antiga. `ConsumableSubtype` pode detalhar `Potion`, `Food`, `BuffFood`.

### 3.2 SkillPoint com regra antiga

A decisão consolidada da FASE9K é:

```text
1 SkillPoint a cada 2 níveis, nos níveis pares.
```

A branch introduziu ou manteve cálculo equivalente a 1 ponto a cada 3 níveis.

Correção:

Centralizar regra em `PlayerProgressionRules`:

```csharp
public static int CalculateSkillPointsGrantedOnLevelUp(int newLevel)
{
    return newLevel > 1 && newLevel % 2 == 0 ? 1 : 0;
}

public static int CalculateTotalSkillPointsAtLevel(int level)
{
    return Mathf.Max(0, level / 2);
}
```

Depois alinhar `PlayerProgressionManager` e `LevelUpManager` ou remover duplicidade.

### 3.3 Dois modelos de EnemyDataSO

A branch mantém/adiciona dois modelos:

```text
CindarsHope.Combat.EnemyDataSO
CindarsHope.Enemy.EnemyDataSO
```

Risco:

Assets criados pelo initializer novo podem não alimentar o runtime de combat/cave.

Correção recomendada:

- manter `CindarsHope.Combat.EnemyDataSO` como oficial por enquanto;
- migrar campos úteis do modelo novo para o oficial;
- atualizar `EnemyDataInitializer` para gerar assets do modelo oficial;
- remover ou obsoletar o modelo paralelo.

### 3.4 Dois modelos de crafting

Runtime existente usa:

```text
CindarsHope.Craft.CraftingManager
CindarsHope.Craft.Data.RecipeDataSO
CindarsHope.Craft.Data.RecipeDatabaseSO
```

A branch criou:

```text
CindarsHope.Crafting.CraftingRecipeSO
```

Risco:

Recipes novas não são usadas pelo runtime real.

Correção recomendada:

- manter `RecipeDataSO` como modelo oficial;
- migrar campos úteis de `CraftingRecipeSO` para `RecipeDataSO` se necessário;
- atualizar initializer para gerar `RecipeDataSO`;
- remover/obsoletar `CraftingRecipeSO` paralelo.

---

## 4. Estado real por spec

| Spec | Estado real observado | Correção esperada |
|---|---|---|
| `spec_fase9e_item_taxonomy_ids.md` | Parcial; enum com risco de serialização | Corrigir enum e documentar migration |
| `spec_fase9e_item_examples_variations.md` | Parcial; assets gerados | Validar enum, registry e uso real |
| `spec_fase9e_save_schema_migration.md` | Não comprovada | Implementar version/migration ou manter pendente |
| `spec_fase9e_player_level_up_progression.md` | Parcial; regra SkillPoint errada | Unificar regra a cada 2 níveis |
| `spec_fase9e_damage_status_elements_complete.md` | Parcial; status skeleton | Integrar damage/status pipeline ou marcar pendente |
| `spec_fase9h_loot_crafting_equipment_durability_environment.md` | Parcial; dados/managers isolados | Integrar equipment/loot/crafting/durability ao runtime |
| `spec_fase9i_player_combat_weapons_magic_skill_actions.md` | Parcial; Weapon/Spell/SkillAction SOs | Criar runtime futuro ou marcar explicitamente parcial |
| `spec_fase9d_enemy_actions_ai_combat.md` | Parcial; AI data skeleton | Criar EnemyBrain/action scoring em spec futura |
| `spec_fase9d_enemy_architecture_40_monsters.md` | Parcial; poucos exemplos | Criar roster real em spec futura |
| `spec_fase9g_cave_bestiary_faction_locks.md` | Parcial; BestiaryDataSO | Integrar event-driven bestiary/faction locks |
| `spec_fase9j_cave_entry_loadout_death_anya_corpse.md` | Parcial; configs/SOs | Integrar morte/corpse/Fonte de Anya ao runtime |
| `spec_fase9k_skill_trees_nodes_active_slots_respec.md` | Parcial; manager mínimo | Corrigir SkillPoint, active slots, save e respec |
| `spec_ui_menu_systems_final.md` | Parcial; menu manager simples | Manter parcial até UI real |
| `spec_fase9l_ui_ux_full_gameplay.md` | Não implementar ainda | Materializar spec completa antes |

---

## 5. Plano de correção

### Wave S0 — Documentação e tracking

Objetivo: tornar o estado do projeto honesto.

Tasks:

1. Rebaixar `IMPLEMENTATION_DELIVERY_20260523.md` para `PARTIAL`.
2. Atualizar `SPEC_REGISTRY_IMPLEMENTED.md` com status `Implementado parcial` onde aplicável.
3. Atualizar `SPEC_REGISTRY_TO_IMPLEMENT.md` removendo contradições.
4. Atualizar `IMPLEMENTATION_STATUS.md`.
5. Corrigir headers das specs em `docs/specs/implementados/`.
6. Atualizar mapas de refinements.
7. Registrar essa estabilização no `PROJECT_LOG.md`.

### Wave S1 — Correções bloqueadoras

Tasks:

1. Corrigir `ItemCategory` preservando valores antigos.
2. Corrigir SkillPoint para níveis pares.
3. Unificar `PlayerProgressionRules`, `PlayerProgressionManager` e `LevelUpManager`.
4. Rodar Unity batchmode.
5. Corrigir erros de compile.

### Wave S2 — Modelos paralelos

Tasks:

1. Consolidar `EnemyDataSO` oficial.
2. Atualizar `EnemyDataInitializer`.
3. Consolidar recipe/crafting model.
4. Atualizar `CraftingRecipeInitializer`.
5. Remover/obsoletar classes paralelas.

### Wave S3 — Reclassificação de skeletons

Tasks:

1. Marcar FASE9I como parcial.
2. Marcar FASE9K como parcial.
3. Marcar FASE9D/G como parcial.
4. Marcar FASE9H/J/UI menus como parciais quando runtime estiver pendente.
5. Criar próximas specs runtime separadas.

### Wave S4 — Validação final

Tasks:

1. Rodar `tools/docs/validate_docs.ps1`.
2. Rodar Unity batchmode.
3. Gerar `docs/implementation_runs/RUN_STABILIZATION_OVERNIGHT_20260523.md`.
4. Não fazer merge automático em `dev`.

---

## 6. Critérios de aceite

A estabilização só termina quando:

- [ ] Unity batchmode compila sem erro.
- [ ] `ItemCategory` preserva valores antigos.
- [ ] `Food` continua existindo.
- [ ] SkillPoint usa regra a cada 2 níveis.
- [ ] Não há dois modelos oficiais de `EnemyDataSO`.
- [ ] Não há dois modelos oficiais de recipe/crafting.
- [ ] `IMPLEMENTATION_DELIVERY_20260523.md` não declara `COMPLETE` indevidamente.
- [ ] Registries estão coerentes.
- [ ] Specs parciais estão marcadas como parciais.
- [ ] `IMPLEMENTATION_STATUS.md` reflete o estado real.
- [ ] FASE9L continua fora do escopo.
- [ ] Relatório final de estabilização existe.

---

## 7. Validações obrigatórias

### Docs

```powershell
.\tools\docs\validate_docs.ps1
```

### Unity

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "D:\Projetos\Jogos\Cindars_hope\cindars_hope" `
  -logFile "Logs\unity-compile-stabilization.log"
```

### Parse de erro

```powershell
Select-String -Path "Logs\unity-compile-stabilization.log" -Pattern `
  "error CS|Compilation failed|Scripts have compiler errors|Exception|NullReferenceException|MissingReferenceException|Missing script|The referenced script on this Behaviour is missing|not found|Failed|AssetDatabase|ScriptableObject"
```

### APIs proibidas

```powershell
Select-String -Path "Assets/_Game/Scripts/**/*.cs" -Pattern `
  "GameObject.Find|FindObjectOfType|FindObjectsByType"
```

---

## 8. Próximas specs após estabilização

Criar specs separadas para runtime real:

```text
spec_fase9i_runtime_player_combat_weapons_spells_skill_actions.md
spec_fase9k_runtime_skill_tree_active_slots_respec_anya.md
spec_fase9d_runtime_enemy_ai_actions_40_monsters.md
spec_fase9g_runtime_bestiary_faction_locks_ecology.md
spec_fase9h_runtime_equipment_loot_crafting_durability.md
spec_fase9j_runtime_cave_entry_death_corpse_recovery.md
spec_fase9l_ui_ux_full_gameplay_spec.md
```

Essas specs devem usar os refinements recuperados como fonte.
