# RUN ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â Stabilization Overnight Specs 2026-05-23

> **Branch:** `review/stabilize-overnight-specs`
> **Base:** `wave/specs-overnight-07-ui-menu-minimal`
> **Spec:** `docs/refinements/implementados/ref_stabilizacao_overnight_specs_20260523.md`
> **Refinement:** `docs/refinements/a_implementar/ref_stabilizacao_overnight_specs_20260523.md`
> **Status:** CÃƒÆ’Ã‚Â³digo/documentaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o estabilizados por anÃƒÆ’Ã‚Â¡lise estÃƒÆ’Ã‚Â¡tica; Unity batchmode local ainda obrigatÃƒÆ’Ã‚Â³rio.

---

## 1. Plano executado

### S0 ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â DocumentaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o honesta e tracking

- Reclassificado `docs/IMPLEMENTATION_DELIVERY_20260523.md` de `PARTIAL` para `PARTIAL`.
- Removida a afirmaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o de `13 specs fully implemented`.
- Documentado o estado real como `backend/data skeleton + initializers + documentaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o gerada`.
- Registradas pendÃƒÆ’Ã‚Âªncias reais de runtime.

### S1 ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â RegressÃƒÆ’Ã‚Âµes bloqueadoras

- Corrigido `ItemCategory` para preservar valores antigos serializados pelo Unity.
- Corrigida regra de SkillPoint para 1 ponto a cada 2 nÃƒÆ’Ã‚Â­veis.
- Centralizada regra em `PlayerProgressionRules`.
- Alinhados `PlayerProgressionManager` e `LevelUpManager`.

### S2 ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â Modelos paralelos

- Modelo oficial de inimigo: `CindarsHope.Combat.EnemyDataSO`.
- `EnemyDataInitializer` atualizado para gerar `Combat.EnemyDataSO`.
- Modelo paralelo `Assets/_Game/Scripts/Enemy/EnemyDataSO.cs` removido.
- Modelo oficial de recipe: `CindarsHope.Craft.Data.RecipeDataSO`.
- `CraftingRecipeInitializer` atualizado para gerar `RecipeDataSO`.
- Modelo paralelo `Assets/_Game/Scripts/Crafting/CraftingRecipeSO.cs` removido.

### S3 ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â Skeletons e escopo

- Mantido status `PARTIAL` para os sistemas que ainda sÃƒÆ’Ã‚Â£o data skeleton.
- NÃƒÆ’Ã‚Â£o foi implementada feature grande fora de escopo.
- FASE9L UI/UX full gameplay continua fora desta estabilizaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o.

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
- mantÃƒÆ’Ã‚Âª-los geraria ambiguidade e risco de assets mortos.

---

## 4. DecisÃƒÆ’Ã‚Âµes tÃƒÆ’Ã‚Â©cnicas

| Tema | DecisÃƒÆ’Ã‚Â£o |
|---|---|
| ItemCategory | Preservar valores antigos `Seed=0`, `Crop=1`, `Food=2`, `Material=3`, `Tool=4`, `Fish=5`, `Misc=6`; novas categorias comeÃƒÆ’Ã‚Â§am em `100`. |
| SkillPoint | +1 em nÃƒÆ’Ã‚Â­veis pares, comeÃƒÆ’Ã‚Â§ando no nÃƒÆ’Ã‚Â­vel 2. |
| EnemyDataSO oficial | `CindarsHope.Combat.EnemyDataSO`. |
| Crafting recipe oficial | `CindarsHope.Craft.Data.RecipeDataSO`. |
| Delivery report | `PARTIAL`, nÃƒÆ’Ã‚Â£o `PARTIAL`. |

---

## 5. ValidaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o feita nesta execuÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o

ValidaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o possÃƒÆ’Ã‚Â­vel via GitHub/anÃƒÆ’Ã‚Â¡lise estÃƒÆ’Ã‚Â¡tica:

- Compare branch base vs estabilizaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o.
- ConfirmaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o de remoÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o dos arquivos paralelos.
- ConfirmaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o de `EnemyDataInitializer` usando `CindarsHope.Combat.EnemyDataSO`.
- ConfirmaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o de `CraftingRecipeInitializer` usando `CindarsHope.Craft.Data.RecipeDataSO`.
- ConfirmaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o de `CraftingManager` usando `RecipeDatabaseSO`/`RecipeDataSO`.
- ConfirmaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o de delivery report rebaixado para `PARTIAL`.

---

## 6. ValidaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o local ainda obrigatÃƒÆ’Ã‚Â³ria

Esta execuÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o nÃƒÆ’Ã‚Â£o rodou Unity localmente. Antes de mergear, executar:

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
| Corrigir documentaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o/status | Feito. |
| Corrigir `ItemCategory` | Feito. |
| Corrigir SkillPoint | Feito. |
| Consolidar EnemyDataSO | Feito; modelo paralelo removido. |
| Consolidar CraftingRecipe/RecipeData | Feito; modelo paralelo removido. |
| Reclassificar specs parciais | Feito no delivery report; registries permanecem dependentes da polÃƒÆ’Ã‚Â­tica de mover specs apÃƒÆ’Ã‚Â³s validaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o Unity. |
| Rodar docs validation | Pendente local. |
| Rodar Unity batchmode | Pendente local. |
| Corrigir compile errors | NÃƒÆ’Ã‚Â£o executado porque batchmode local nÃƒÆ’Ã‚Â£o foi rodado. |

---

## 8. RecomendaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o

NÃƒÆ’Ã‚Â£o mergear direto em `dev` sem a validaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o local.

Se Unity compilar limpo, a branch pode ser considerada candidata a merge/cherry-pick da estabilizaÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o.

Se Unity acusar erro, corrigir primeiro nesta mesma branch.
