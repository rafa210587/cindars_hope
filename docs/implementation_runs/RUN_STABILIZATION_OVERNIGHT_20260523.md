# RUN — Stabilization Overnight Specs 2026-05-23

> **Branch:** `review/stabilize-overnight-specs`  
> **Base:** `wave/specs-overnight-07-ui-menu-minimal`  
> **Spec:** `docs/specs/a_implementar/spec_stabilization_overnight_fase9c_to_fase9l_v1.md`  
> **Refinement:** `docs/refinements/a_implementar/ref_stabilizacao_overnight_specs_20260523.md`  
> **Status:** Código/documentação estabilizados por análise estática; Unity batchmode local ainda obrigatório.

---

## 1. Plano executado

### S0 — Documentação honesta e tracking

- Reclassificado `docs/IMPLEMENTATION_DELIVERY_20260523.md` de `COMPLETE` para `PARTIAL`.
- Removida a afirmação de `13 Specifications Fully Implemented`.
- Documentado o estado real como `backend/data skeleton + initializers + documentação gerada`.
- Registradas pendências reais de runtime.

### S1 — Regressões bloqueadoras

- Corrigido `ItemCategory` para preservar valores antigos serializados pelo Unity.
- Corrigida regra de SkillPoint para 1 ponto a cada 2 níveis.
- Centralizada regra em `PlayerProgressionRules`.
- Alinhados `PlayerProgressionManager` e `LevelUpManager`.

### S2 — Modelos paralelos

- Modelo oficial de inimigo: `CindarsHope.Combat.EnemyDataSO`.
- `EnemyDataInitializer` atualizado para gerar `Combat.EnemyDataSO`.
- Modelo paralelo `Assets/_Game/Scripts/Enemy/EnemyDataSO.cs` removido.
- Modelo oficial de recipe: `CindarsHope.Craft.Data.RecipeDataSO`.
- `CraftingRecipeInitializer` atualizado para gerar `RecipeDataSO`.
- Modelo paralelo `Assets/_Game/Scripts/Crafting/CraftingRecipeSO.cs` removido.

### S3 — Skeletons e escopo

- Mantido status `PARTIAL` para os sistemas que ainda são data skeleton.
- Não foi implementada feature grande fora de escopo.
- FASE9L UI/UX full gameplay continua fora desta estabilização.

---

## 2. Arquivos alterados

```text
Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs
Assets/_Game/Scripts/Player/Progression/PlayerProgressionRules.cs
Assets/_Game/Scripts/Player/Progression/PlayerProgressionManager.cs
Assets/_Game/Scripts/Core/Progression/LevelUpManager.cs
Assets/_Game/Scripts/Combat/EnemyDataSO.cs
Assets/_Game/Scripts/Editor/EnemyDataInitializer.cs
Assets/_Game/Scripts/Craft/Data/RecipeDataSO.cs
Assets/_Game/Scripts/Editor/CraftingRecipeInitializer.cs
docs/IMPLEMENTATION_DELIVERY_20260523.md
```

## 3. Arquivos removidos como legado/lixo

```text
Assets/_Game/Scripts/Enemy/EnemyDataSO.cs
Assets/_Game/Scripts/Crafting/CraftingRecipeSO.cs
```

Motivo:

- ambos criavam modelos paralelos aos modelos usados pelo runtime;
- initializers foram migrados para os modelos oficiais;
- mantê-los geraria ambiguidade e risco de assets mortos.

---

## 4. Decisões técnicas

| Tema | Decisão |
|---|---|
| ItemCategory | Preservar valores antigos `Seed=0`, `Crop=1`, `Food=2`, `Material=3`, `Tool=4`, `Fish=5`, `Misc=6`; novas categorias começam em `100`. |
| SkillPoint | +1 em níveis pares, começando no nível 2. |
| EnemyDataSO oficial | `CindarsHope.Combat.EnemyDataSO`. |
| Crafting recipe oficial | `CindarsHope.Craft.Data.RecipeDataSO`. |
| Delivery report | `PARTIAL`, não `COMPLETE`. |

---

## 5. Validação feita nesta execução

Validação possível via GitHub/análise estática:

- Compare branch base vs estabilização.
- Confirmação de remoção dos arquivos paralelos.
- Confirmação de `EnemyDataInitializer` usando `CindarsHope.Combat.EnemyDataSO`.
- Confirmação de `CraftingRecipeInitializer` usando `CindarsHope.Craft.Data.RecipeDataSO`.
- Confirmação de `CraftingManager` usando `RecipeDatabaseSO`/`RecipeDataSO`.
- Confirmação de delivery report rebaixado para `PARTIAL`.

---

## 6. Validação local ainda obrigatória

Esta execução não rodou Unity localmente. Antes de mergear, executar:

```powershell
.\tools\docs\validate_docs.ps1

& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "D:\Projetos\Jogos\Cindars_hope\cindars_hope" `
  -logFile "Logs\unity-compile-stabilization-final.log"

Select-String -Path "Logs\unity-compile-stabilization-final.log" -Pattern `
  "error CS|Compilation failed|Scripts have compiler errors|Exception|NullReferenceException|MissingReferenceException|Missing script|The referenced script on this Behaviour is missing|not found|Failed|AssetDatabase|ScriptableObject"
```

---

## 7. Resultado contra o plano

| Item do plano | Resultado |
|---|---|
| Corrigir documentação/status | Feito. |
| Corrigir `ItemCategory` | Feito. |
| Corrigir SkillPoint | Feito. |
| Consolidar EnemyDataSO | Feito; modelo paralelo removido. |
| Consolidar CraftingRecipe/RecipeData | Feito; modelo paralelo removido. |
| Reclassificar specs parciais | Feito no delivery report; registries permanecem dependentes da política de mover specs após validação Unity. |
| Rodar docs validation | Pendente local. |
| Rodar Unity batchmode | Pendente local. |
| Corrigir compile errors | Não executado porque batchmode local não foi rodado. |

---

## 8. Recomendação

Não mergear direto em `dev` sem a validação local.

Se Unity compilar limpo, a branch pode ser considerada candidata a merge/cherry-pick da estabilização.

Se Unity acusar erro, corrigir primeiro nesta mesma branch.
