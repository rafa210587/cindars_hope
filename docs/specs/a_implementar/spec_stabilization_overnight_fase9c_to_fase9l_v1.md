# SPEC — Estabilização e integração da execução overnight FASE9C–FASE9L

> **Spec ID:** `spec_stabilization_overnight_fase9c_to_fase9l_v1`  
> **Status:** A implementar  
> **Tipo:** SpecKit / estabilização técnica  
> **Data:** 2026-05-23  
> **Branch base sugerida:** `wave/specs-overnight-07-ui-menu-minimal`  
> **Nova branch sugerida:** `review/stabilize-overnight-specs`  
> **Base de comparação:** `dev`  
> **Escopo:** corrigir regressões, consolidar modelos paralelos, tornar documentação honesta e preparar merge seguro.  
> **Fora de escopo:** implementar FASE9L UI/UX completa; criar novas features grandes; reescrever sistemas estáveis sem necessidade.

---

## 1. Contexto

A execução overnight gerou uma branch com 15 commits à frente de `dev`, adicionando assets, ScriptableObjects, initializers e specs em `docs/specs/implementados/`.

O compare contra `dev` mostra criação/alteração de:

```text
Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs
Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs
Assets/_Game/Scripts/Core/Progression/LevelUpManager.cs
Assets/_Game/Scripts/Combat/StatusEffect/*
Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs
Assets/_Game/Scripts/Combat/Magic/SpellDataSO.cs
Assets/_Game/Scripts/Combat/Skills/SkillActionSO.cs
Assets/_Game/Scripts/Equipment/*
Assets/_Game/Scripts/Loot/LootTableSO.cs
Assets/_Game/Scripts/Crafting/CraftingRecipeSO.cs
Assets/_Game/Scripts/Enemy/*
Assets/_Game/Scripts/Cave/CaveEntryDataSO.cs
Assets/_Game/Scripts/Player/Death/*
Assets/_Game/Scripts/Skills/*
Assets/_Game/Scripts/UI/MenuManager.cs
Assets/_Game/Scripts/UI/MenuSystemDataSO.cs
docs/specs/implementados/spec_fase9*.md
docs/IMPLEMENTATION_DELIVERY_20260523.md
docs/validation/VALIDATION_GUIDE_13_FEATURES_20260523.md
```

O relatório gerado na branch declara `Status: COMPLETE` e `13 Specifications Fully Implemented`, mas a análise do código e dos registries indica que a implementação real é majoritariamente:

```text
backend/data skeleton + initializers + documentação gerada
```

e não implementação completa das specs FASE9D/E/H/I/J/K.

Esta spec define o trabalho necessário para estabilizar, corrigir e tornar a branch segura.

---

## 2. Problema

A branch atual mistura quatro coisas diferentes:

1. implementação parcial de dados;
2. documentação dizendo que tudo foi completamente implementado;
3. specs movidas para `implementados`, mas ainda semanticamente pendentes;
4. modelos paralelos que podem não alimentar o runtime existente.

Isso cria risco de:

- mergear código que compila mas não funciona;
- quebrar assets serializados por mudança de enum;
- criar duas fontes de verdade para inimigos;
- criar duas fontes de verdade para crafting;
- declarar specs como concluídas sem gameplay real;
- dificultar trabalho futuro por tracking incorreto.

---

## 3. Objetivo

Estabilizar a branch overnight antes de merge, garantindo:

1. compilação Unity limpa;
2. documentação honesta;
3. registries coerentes;
4. ausência de regressões óbvias;
5. um modelo oficial por domínio;
6. specs marcadas como `Implementado parcial` quando forem apenas skeleton/data;
7. runtime mínimo onde for necessário para não deixar assets mortos;
8. refinements futuros preservados.

---

## 4. Não objetivos

Esta spec **não deve**:

- implementar FASE9L UI/UX full gameplay;
- criar UI final de inventory/equipment/skill tree/crafting;
- completar 40 monstros se isso não couber na estabilização;
- criar todo sistema avançado de magia/combate final;
- refatorar todo projeto;
- alterar `Packages/`, `ProjectSettings/`, `.sln`, `.slnx`;
- alterar `docs_old/`.

---

# /speckit.specify

## 5. User stories / engineering stories

### Story 1 — Status documental honesto

Como mantenedor do projeto, quero que specs, registries e delivery report indiquem o estado real da implementação, para não planejar próximas waves em cima de informação falsa.

#### Critérios de aceite

- `IMPLEMENTATION_DELIVERY_20260523.md` não declara `COMPLETE` se há apenas skeleton.
- `SPEC_REGISTRY_TO_IMPLEMENT.md` e `SPEC_REGISTRY_IMPLEMENTED.md` não se contradizem.
- Specs em `docs/specs/implementados/` não mantêm cabeçalho `SPEC FUTURA / A implementar`.
- Cada spec parcial tem evidência real, pendências, validação Unity e status correto.

### Story 2 — Preservar serialização de assets

Como desenvolvedor Unity, quero que alterações em enums não mudem o significado dos assets existentes.

#### Critérios de aceite

- `ItemCategory` preserva valores antigos:
  - `Seed = 0`
  - `Crop = 1`
  - `Food = 2`
  - `Material = 3`
  - `Tool = 4`
  - `Fish = 5`
  - `Misc = 6`
- Novas categorias são adicionadas com valores explícitos posteriores.
- `Food` não desaparece como categoria antiga sem migration.
- Assets antigos não precisam ser recriados por causa da enum migration.

### Story 3 — Regra única de SkillPoint

Como designer de progressão, quero que todo cálculo de SkillPoint siga a regra consolidada da FASE9K: 1 SkillPoint a cada 2 níveis.

#### Critérios de aceite

- `PlayerProgressionManager` concede SkillPoint em níveis pares.
- `LevelUpManager`, se mantido, usa a mesma regra.
- `PlayerProgressionRules` concentra a regra ou é a única fonte oficial.
- Documentação e código dizem a mesma coisa.
- Não existe cálculo paralelo com regra de 3 níveis.

### Story 4 — Um modelo oficial de inimigo

Como desenvolvedor de combate/cave, quero evitar dois `EnemyDataSO` oficiais, para que os assets criados sejam usados pelo runtime real.

#### Critérios de aceite

- Existe uma decisão explícita:
  - manter `CindarsHope.Combat.EnemyDataSO` como oficial; ou
  - migrar todo runtime para `CindarsHope.Enemy.EnemyDataSO`.
- Não existem initializers gerando assets para um modelo que o runtime não usa.
- Se modelo novo for mantido, há adapter/migration.
- `EnemyDataInitializer` cria assets compatíveis com o modelo oficial.
- Documentação explica a decisão.

### Story 5 — Uma linguagem de recipes

Como desenvolvedor, quero que recipes novas sejam consumidas pelo `CraftingManager` real.

#### Critérios de aceite

- Ou `CraftingRecipeSO` é integrado ao runtime existente;
- ou `RecipeDataSO`/`RecipeDatabaseSO` existentes são evoluídos e `CraftingRecipeSO` é removido/obsoletado.
- Não existem duas fontes de verdade para recipes.
- Initializer cria assets que o `CraftingManager` consegue usar.
- Smoke test de crafting passa.

### Story 6 — Data skeleton não é gameplay completo

Como designer, quero que sistemas data-only sejam marcados como parcial, para evitar falsa sensação de conclusão.

#### Critérios de aceite

- FASE9I é marcada como parcial enquanto não houver runtime de weapon/spell/skill use.
- FASE9K é marcada como parcial enquanto não houver save/load, active skill actions reais, capstones e respec Fonte de Anya.
- FASE9D/G são marcadas como parciais enquanto não houver AI runtime, roster real e faction locks no spawn.
- FASE9H é marcada como parcial enquanto equipment/loot/crafting/durability não estiverem integrados ao runtime.

---

## 6. Requisitos funcionais

### RF-01 — Corrigir status documental

Atualizar:

```text
docs/IMPLEMENTATION_DELIVERY_20260523.md
docs/IMPLEMENTATION_STATUS.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/refinements/implementados/ref_implementados_map.md
docs/refinements/a_implementar/ref_futuro_map.md
PROJECT_LOG.md
```

Regras:

- specs implementadas parcialmente devem aparecer como `Implementado parcial`;
- specs apenas copiadas/movidas devem voltar para `a_implementar` ou serem marcadas como parcial;
- `COMPLETE` só pode ser usado se Unity e runtime confirmarem.

### RF-02 — Corrigir `ItemCategory`

Arquivo:

```text
Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs
```

Estado esperado:

```csharp
namespace CindarsHope.Inventory.Data
{
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

    public enum ConsumableSubtype
    {
        None = 0,
        Potion = 1,
        Food = 2,
        BuffFood = 3
    }
}
```

Se algum asset novo foi salvo com os valores deslocados da versão anterior da branch, criar migration manual ou regenerar assets novos após estabilizar enum.

### RF-03 — Unificar progressão e SkillPoints

Arquivos afetados:

```text
Assets/_Game/Scripts/Player/Progression/PlayerProgressionRules.cs
Assets/_Game/Scripts/Player/Progression/PlayerProgressionManager.cs
Assets/_Game/Scripts/Core/Progression/LevelUpManager.cs
```

Regra oficial:

```text
SkillPoint: +1 em níveis pares, começando no nível 2.
AttributePoint: +1 por level up.
MaxLevel: 100.
```

Implementação sugerida:

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

Se `LevelUpManager` duplicar `PlayerProgressionRules`, preferir:

- mover regra para `PlayerProgressionRules`;
- `LevelUpManager` chama `PlayerProgressionRules`;
- ou remover `LevelUpManager` se não for usado.

### RF-04 — Consolidar EnemyDataSO

Arquivos afetados:

```text
Assets/_Game/Scripts/Combat/EnemyDataSO.cs
Assets/_Game/Scripts/Enemy/EnemyDataSO.cs
Assets/_Game/Scripts/Editor/EnemyDataInitializer.cs
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Combat/**
```

Decisão recomendada:

```text
Manter `CindarsHope.Combat.EnemyDataSO` como modelo oficial neste momento,
porque ele já existe na dev e está mais próximo do runtime de combate/cave.
```

Ação:

1. Migrar campos úteis do modelo novo para o modelo oficial:
   - `Description`;
   - `Defense`;
   - `AIBehaviorId`;
   - `LootTableId`;
   - atributos opcionais, se realmente usados.
2. Atualizar `EnemyDataInitializer` para criar `CindarsHope.Combat.EnemyDataSO`.
3. Remover `Assets/_Game/Scripts/Enemy/EnemyDataSO.cs` ou renomear para não ser fonte oficial.
4. Manter `AIBehaviorSO` e `BestiaryDataSO` se não conflitarem, mas documentar que são parciais.
5. Atualizar specs/refinements com decisão.

### RF-05 — Consolidar crafting recipe model

Arquivos afetados:

```text
Assets/_Game/Scripts/Craft/CraftingManager.cs
Assets/_Game/Scripts/Craft/Data/RecipeDataSO.cs
Assets/_Game/Scripts/Craft/Data/RecipeDatabaseSO.cs
Assets/_Game/Scripts/Crafting/CraftingRecipeSO.cs
Assets/_Game/Scripts/Editor/CraftingRecipeInitializer.cs
```

Decisão recomendada:

```text
Manter `CindarsHope.Craft.Data.RecipeDataSO` como modelo oficial,
pois `CraftingManager` já usa esse contrato.
```

Ação:

1. Migrar campos úteis de `CraftingRecipeSO` para `RecipeDataSO` se necessário:
   - craft time;
   - required level;
   - station;
   - skill requirement.
2. Atualizar `CraftingRecipeInitializer` para gerar `RecipeDataSO`.
3. Remover/obsoletar `CindarsHope.Crafting.CraftingRecipeSO`.
4. Garantir que `CraftingManager.TryCraft` funciona com as recipes geradas.

### RF-06 — Reclassificar FASE9I como parcial

Arquivos criados:

```text
Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs
Assets/_Game/Scripts/Combat/Magic/SpellDataSO.cs
Assets/_Game/Scripts/Combat/Skills/SkillActionSO.cs
Assets/_Game/Scripts/Editor/CombatDataInitializer.cs
```

Status correto:

```text
Implementado parcial — schemas e assets iniciais.
```

Gaps que devem virar pendência explícita:

- `PlayerWeaponController`;
- `PlayerSpellCaster`;
- `SkillActionExecutor`;
- active slots usando `SkillActionSO`;
- mana/stamina/cooldown runtime;
- damage/status pipeline;
- roll/dash/parry/block/sprint;
- bow/ammo;
- save de active slots.

### RF-07 — Reclassificar FASE9K como parcial

Arquivos criados:

```text
Assets/_Game/Scripts/Skills/SkillTreeManager.cs
Assets/_Game/Scripts/Skills/SkillTreeDataSO.cs
Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs
```

Status correto:

```text
Implementado parcial — manager mínimo e dados iniciais.
```

Correções necessárias:

1. active slots devem guardar `SkillActionId`/`SpellId`, não apenas `nodeId`;
2. save/load de purchased nodes e equipped active skills;
3. capstones;
4. respec com Fonte de Anya;
5. custo de respec;
6. eventos;
7. regra de SkillPoint a cada 2 níveis.

### RF-08 — Reclassificar FASE9D/G como parciais

Arquivos criados:

```text
Assets/_Game/Scripts/Enemy/AIBehaviorSO.cs
Assets/_Game/Scripts/Enemy/BestiaryDataSO.cs
Assets/_Game/Scripts/Enemy/EnemyDataSO.cs
Assets/_Game/Scripts/Editor/EnemyDataInitializer.cs
```

Status correto:

```text
Implementado parcial — dados iniciais; runtime IA/spawn/ecologia pendente.
```

Pendências:

- EnemyBrain;
- action scoring;
- movement profile;
- enemy action profile;
- 40+ monsters;
- faction locks reais;
- bestiary event-driven;
- XP formula por difficulty.

### RF-09 — Reclassificar FASE9H como parcial

Arquivos criados:

```text
Assets/_Game/Scripts/Equipment/EquipmentDataSO.cs
Assets/_Game/Scripts/Equipment/DurabilityManager.cs
Assets/_Game/Scripts/Equipment/EnvironmentalResistanceManager.cs
Assets/_Game/Scripts/Loot/LootTableSO.cs
Assets/_Game/Scripts/Crafting/CraftingRecipeSO.cs
```

Status correto:

```text
Implementado parcial — dados e managers isolados.
```

Pendências:

- EquipmentManager real;
- stat recompute;
- durability em ataque/defesa/tool use;
- loot table conectada a drops;
- crafting integrado ao `CraftingManager`;
- save/load de equipment/durability.

### RF-10 — Validar compile e não avançar com erro

Comando obrigatório:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "D:\Projetos\Jogos\Cindars_hope\cindars_hope" `
  -logFile "Logs\unity-compile-stabilization.log"

Select-String -Path "Logs\unity-compile-stabilization.log" -Pattern `
  "error CS|Compilation failed|Scripts have compiler errors|Exception|NullReferenceException|MissingReferenceException|Missing script|The referenced script on this Behaviour is missing|not found|Failed|AssetDatabase|ScriptableObject"
```

Critério:

- zero `error CS`;
- zero `Scripts have compiler errors`;
- warnings devem ser avaliados e registrados;
- se houver erro, corrigir antes de atualizar status como validado.

---

## 7. Requisitos não funcionais

### RNF-01 — Segurança de save

Nenhum DTO de save novo deve serializar:

```text
ScriptableObject
GameObject
Transform
MonoBehaviour
Sprite
Collider
Rigidbody
```

Usar apenas:

```text
string
int
float
bool
Vector3 se já for padrão aceito no projeto
listas/dicionários simples serializáveis
```

### RNF-02 — Compatibilidade com assets existentes

- não reordenar enums serializados;
- não remover campos públicos usados por assets sem migration;
- não renomear namespaces usados por assets sem migration.

### RNF-03 — Não criar modelo paralelo

Antes de criar novo SO/manager:

1. procurar modelo existente;
2. evoluir modelo existente quando possível;
3. criar adapter se migração for necessária;
4. documentar decisão.

### RNF-04 — Observabilidade documental

Cada mudança deve registrar:

```text
por que mudou
classe afetada
spec relacionada
estado antes
estado depois
validação
pendência
```

---

# /speckit.plan

## 8. Arquitetura alvo

### 8.1 Estado desejado pós-estabilização

```text
Inventory
└── ItemCategory preserva valores antigos e adiciona novos sem quebrar assets.

Progression
├── PlayerProgressionRules = fonte oficial de cálculo.
├── PlayerProgressionManager usa PlayerProgressionRules.
└── LevelUpManager removido ou delega para PlayerProgressionRules.

Enemy
├── CindarsHope.Combat.EnemyDataSO = modelo oficial temporário.
├── AIBehaviorSO = dados auxiliares parciais.
├── BestiaryDataSO = dados auxiliares parciais.
└── EnemyDataInitializer gera assets compatíveis com runtime.

Craft
├── CindarsHope.Craft.Data.RecipeDataSO = modelo oficial.
├── RecipeDatabaseSO = registry/runtime.
└── CraftingRecipeInitializer gera RecipeDataSO.

Combat Data
├── WeaponDataSO = schema parcial oficial.
├── SpellDataSO = schema parcial oficial.
└── SkillActionSO = schema parcial oficial.

Skills
├── SkillTreeDataSO = schema parcial.
├── SkillNodeDataSO = schema parcial.
└── SkillTreeManager = manager parcial, não runtime final.
```

---

## 9. Classes e arquivos a mexer

### P0 — Documentação/tracking

```text
docs/IMPLEMENTATION_DELIVERY_20260523.md
docs/IMPLEMENTATION_STATUS.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/specs/implementados/spec_fase9*.md
docs/specs/implementados/spec_ui_menu_systems_final.md
docs/refinements/a_implementar/ref_futuro_map.md
docs/refinements/implementados/ref_implementados_map.md
PROJECT_LOG.md
```

### P0 — Regressões diretas

```text
Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs
Assets/_Game/Scripts/Player/Progression/PlayerProgressionRules.cs
Assets/_Game/Scripts/Player/Progression/PlayerProgressionManager.cs
Assets/_Game/Scripts/Core/Progression/LevelUpManager.cs
```

### P1 — Modelos paralelos

```text
Assets/_Game/Scripts/Combat/EnemyDataSO.cs
Assets/_Game/Scripts/Enemy/EnemyDataSO.cs
Assets/_Game/Scripts/Editor/EnemyDataInitializer.cs

Assets/_Game/Scripts/Craft/Data/RecipeDataSO.cs
Assets/_Game/Scripts/Craft/Data/RecipeDatabaseSO.cs
Assets/_Game/Scripts/Craft/CraftingManager.cs
Assets/_Game/Scripts/Crafting/CraftingRecipeSO.cs
Assets/_Game/Scripts/Editor/CraftingRecipeInitializer.cs
```

### P2 — Classificação honesta de skeletons

```text
Assets/_Game/Scripts/Combat/Weapon/WeaponDataSO.cs
Assets/_Game/Scripts/Combat/Magic/SpellDataSO.cs
Assets/_Game/Scripts/Combat/Skills/SkillActionSO.cs
Assets/_Game/Scripts/Skills/SkillTreeManager.cs
Assets/_Game/Scripts/Equipment/*
Assets/_Game/Scripts/Loot/*
Assets/_Game/Scripts/Cave/CaveEntryDataSO.cs
Assets/_Game/Scripts/Player/Death/*
Assets/_Game/Scripts/UI/MenuManager.cs
```

---

## 10. Plano de execução

### Wave S0 — Documentação honesta e registries

#### Objetivo

Corrigir tracking e impedir que o projeto acredite que as specs estão completas.

#### Tasks

1. Alterar `IMPLEMENTATION_DELIVERY_20260523.md`.
2. Atualizar `SPEC_REGISTRY_IMPLEMENTED.md`.
3. Atualizar `SPEC_REGISTRY_TO_IMPLEMENT.md`.
4. Atualizar `IMPLEMENTATION_STATUS.md`.
5. Corrigir headers das specs em `docs/specs/implementados/`.
6. Atualizar maps de refinements.
7. Adicionar este refinement como referência.

#### Validação

```powershell
.\tools\docs\validate_docs.ps1
```

### Wave S1 — Regressões bloqueadoras

#### Objetivo

Corrigir riscos imediatos de regressão.

#### Tasks

1. Corrigir `ItemCategory` com valores explícitos.
2. Corrigir SkillPoint a cada 2 níveis.
3. Unificar ou remover duplicidade de `LevelUpManager`.
4. Rodar Unity batchmode.
5. Corrigir erros de compilação.

### Wave S2 — Consolidar modelos paralelos

#### Objetivo

Eliminar duplicidade de EnemyDataSO e Recipe models.

#### Tasks

1. Escolher modelo oficial de enemy.
2. Migrar fields úteis.
3. Atualizar `EnemyDataInitializer`.
4. Escolher modelo oficial de crafting.
5. Atualizar `CraftingRecipeInitializer`.
6. Remover/obsoletar classes paralelas.

### Wave S3 — Runtime mínimo sem ampliar escopo

#### Objetivo

Garantir que os sistemas skeleton não quebrem e estejam corretamente marcados.

#### Tasks

1. Se `WeaponDataSO` não é usado em runtime, marcar como parcial.
2. Se `SpellDataSO` não é usado em runtime, marcar como parcial.
3. Se `SkillActionSO` não é usado em runtime, marcar como parcial.
4. Não implementar combate completo nesta wave.
5. Criar tasks futuras claras para FASE9I/K/H/D/G/J.

### Wave S4 — Relatório final e merge readiness

#### Objetivo

Gerar visão honesta para decisão de merge/cherry-pick.

#### Tasks

1. Criar `docs/implementation_runs/RUN_STABILIZATION_OVERNIGHT_20260523.md`.
2. Listar arquivos alterados, regressões corrigidas, modelos consolidados, pendências reais, validações Unity e recomendação de merge/cherry-pick.
3. Rodar validação documental.
4. Rodar Unity batchmode.
5. Não fazer merge em dev.

---

# /speckit.tasks

## 11. Checklist executável

### S0 — Docs

- [ ] Editar `IMPLEMENTATION_DELIVERY_20260523.md`.
- [ ] Substituir `COMPLETE` por `PARTIAL`.
- [ ] Trocar “13 Fully Implemented” por “13 backend/data skeletons parcialmente implementados”.
- [ ] Atualizar `SPEC_REGISTRY_IMPLEMENTED.md`.
- [ ] Atualizar `SPEC_REGISTRY_TO_IMPLEMENT.md`.
- [ ] Atualizar `IMPLEMENTATION_STATUS.md`.
- [ ] Corrigir headers das specs em `docs/specs/implementados/`.
- [ ] Atualizar `PROJECT_LOG.md`.
- [ ] Rodar `tools/docs/validate_docs.ps1`.

### S1 — Progression e enums

- [ ] Corrigir `ItemCategory` preservando valores antigos.
- [ ] Manter `Food`.
- [ ] Adicionar novos valores com números explícitos posteriores.
- [ ] Corrigir `PlayerProgressionRules`.
- [ ] Corrigir `PlayerProgressionManager`.
- [ ] Corrigir/remover `LevelUpManager`.
- [ ] Validar SkillPoint em níveis 2, 4, 6, 8, 10.
- [ ] Rodar Unity batchmode.

### S2 — Enemy

- [ ] Decidir modelo oficial.
- [ ] Atualizar `EnemyDataInitializer`.
- [ ] Evitar dois `EnemyDataSO` oficiais.
- [ ] Garantir assets gerados compatíveis com runtime.
- [ ] Atualizar docs da FASE9D/G como parcial.

### S3 — Crafting

- [ ] Decidir modelo oficial.
- [ ] Atualizar initializer para modelo usado pelo `CraftingManager`.
- [ ] Remover/obsoletar `CraftingRecipeSO` paralelo.
- [ ] Garantir `TryCraft` compila.
- [ ] Atualizar docs FASE9H como parcial.

### S4 — Skeletons

- [ ] Marcar FASE9I como parcial.
- [ ] Marcar FASE9K como parcial.
- [ ] Marcar FASE9J como parcial.
- [ ] Marcar menu systems como parcial.
- [ ] Registrar tasks futuras específicas.

### S5 — Validação final

- [ ] Rodar docs validation.
- [ ] Rodar Unity batchmode.
- [ ] Verificar padrões proibidos:
  - `GameObject.Find`
  - `FindObjectOfType`
  - `FindObjectsByType`
- [ ] Criar relatório final.
- [ ] Deixar branch sem merge/push automático.

---

## 12. Testes e validações

### 12.1 Validação documental

```powershell
.\tools\docs\validate_docs.ps1
```

### 12.2 Unity batchmode

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "D:\Projetos\Jogos\Cindars_hope\cindars_hope" `
  -logFile "Logs\unity-compile-stabilization-final.log"
```

### 12.3 Parse de erros

```powershell
Select-String -Path "Logs\unity-compile-stabilization-final.log" -Pattern `
  "error CS|Compilation failed|Scripts have compiler errors|Exception|NullReferenceException|MissingReferenceException|Missing script|The referenced script on this Behaviour is missing|not found|Failed|AssetDatabase|ScriptableObject"
```

### 12.4 Busca de APIs proibidas

```powershell
Select-String -Path "Assets/_Game/Scripts/**/*.cs" -Pattern `
  "GameObject.Find|FindObjectOfType|FindObjectsByType"
```

### 12.5 Teste de SkillPoint

Criar teste manual ou script debug:

```text
Level 1 => 0 skill points
Level 2 => +1
Level 3 => +0
Level 4 => +1
Level 5 => +0
Level 6 => +1
```

### 12.6 Teste de enum

Validar assets antigos:

```text
item_seed_wheat continua Seed
crop continua Crop
food continua Food ou ConsumableSubtype Food sem quebrar categoria antiga
tool continua Tool
fish continua Fish
```

---

## 13. Definition of Done

Esta spec só está concluída quando:

- [ ] Unity batchmode compila sem erro.
- [ ] `ItemCategory` preserva valores antigos.
- [ ] SkillPoint usa regra a cada 2 níveis.
- [ ] Não existem dois modelos oficiais de `EnemyDataSO`.
- [ ] Não existem dois modelos oficiais de recipe/crafting.
- [ ] `IMPLEMENTATION_DELIVERY_20260523.md` não superestima status.
- [ ] Registries estão coerentes.
- [ ] Specs parciais estão marcadas como parciais.
- [ ] FASE9L continua fora de escopo.
- [ ] Relatório final de estabilização existe.
- [ ] Nenhum merge automático em `dev` foi feito.

---

## 14. Riscos

| Risco | Mitigação |
|---|---|
| Assets novos serializados com enum antigo deslocado | corrigir enum antes de abrir/salvar mais assets |
| Runtime usa modelo antigo e assets novos usam modelo novo | consolidar EnemyDataSO |
| Crafting novo não é usado pelo CraftingManager | consolidar RecipeDataSO |
| Docs dizem completo e time planeja errado | rebaixar status para parcial |
| Corrigir tudo vira feature creep | separar estabilização de implementação futura |
| Unity compile encontra erros gerados por initializers | corrigir antes de qualquer merge |

---

## 15. Próximas specs futuras após estabilização

Depois desta estabilização, criar specs separadas para:

```text
spec_fase9i_runtime_player_combat_weapons_spells_skill_actions.md
spec_fase9k_runtime_skill_tree_active_slots_respec_anya.md
spec_fase9d_runtime_enemy_ai_actions_40_monsters.md
spec_fase9g_runtime_bestiary_faction_locks_ecology.md
spec_fase9h_runtime_equipment_loot_crafting_durability.md
spec_fase9j_runtime_cave_entry_death_corpse_recovery.md
spec_fase9l_ui_ux_full_gameplay_spec.md
```

Essas specs devem usar os refinements recuperados de:

```text
ref_recovered_enemy_roster_movesets_xp_20260523.md
ref_recovered_enemy_ai_roles_status_ecology_20260523.md
ref_recovered_player_weapons_damage_types_20260523.md
ref_recovered_magic_spells_status_elements_20260523.md
ref_recovered_skill_trees_nodes_purchase_respec_20260523.md
ref_recovered_active_slots_equip_use_hotbar_20260523.md
ref_recovered_equipment_loot_crafting_durability_environment_20260523.md
```

---

## 16. Prompt recomendado para execução

```md
Estamos no repo `rafa210587/cindars_hope`.

Branch base:
`wave/specs-overnight-07-ui-menu-minimal`

Crie branch:
`review/stabilize-overnight-specs`

Leia:
1. `AGENTS.md`
2. `docs/operations/AGENT_EXECUTION_PROTOCOL.md`
3. `docs/operations/READING_MATRIX.md`
4. `docs/specs/a_implementar/spec_stabilization_overnight_fase9c_to_fase9l_v1.md`
5. `docs/IMPLEMENTATION_DELIVERY_20260523.md`
6. `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
7. `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`
8. `docs/IMPLEMENTATION_STATUS.md`

Objetivo:
executar a spec de estabilização, não implementar features novas grandes.

Ordem:
1. Corrigir documentação/status.
2. Corrigir ItemCategory.
3. Corrigir SkillPoint a cada 2 níveis.
4. Consolidar EnemyDataSO.
5. Consolidar CraftingRecipe/RecipeData.
6. Reclassificar specs parciais.
7. Rodar docs validation.
8. Rodar Unity batchmode.
9. Corrigir compile errors.
10. Criar `docs/implementation_runs/RUN_STABILIZATION_OVERNIGHT_20260523.md`.

Não fazer merge.
Não fazer push sem autorização.
```
