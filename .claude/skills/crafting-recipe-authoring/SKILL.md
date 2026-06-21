---
name: crafting-recipe-authoring
description: Autoria de recipes (ingredientes, estação, timing, unlock/gate) e save de jobs em andamento, com alerta obrigatório sobre os dois sistemas paralelos de crafting (Craft/ vs Crafting/). Usar em specs que toquem em crafting, processing, upgrade de equipamento ou unlock de recipe.
---

# Skill: Autoria de Recipe de Crafting

> ⚠️ **ALERTA — DOIS SISTEMAS PARALELOS EXISTEM NO PROJETO**
>
> `Assets/_Game/Scripts/Craft/` — jobs/timing/processing (runtime de station): `CraftingRuntime`, `CraftingStation`, `CraftingJob`, `RecipeDataSO`, `RecipeDatabaseSO`, `CraftingPoint`, `CraftingManager`.
>
> `Assets/_Game/Scripts/Crafting/` — unlock/gate/upgrade de equipamento: `RecipeUnlockService`, `CraftingRecipeGate`, `RecipeFirstKillUnlockHook`, `EquipmentUpgradeService`, `EquipmentUpgradeRegistry`, `UpgradeRecipeSO`, `HighTierGearCanon`.
>
> **Antes de adicionar qualquer recipe ou feature de crafting, rode `(skill: system-reuse-audit)`.** Se não houver decisão canônica de convergência dos dois sistemas para o contexto da sua spec, **PARE e reporte ao humano**. Não escolha um silenciosamente nem crie um terceiro sistema.

O sistema `Craft/` (WAVE 06, WAVE_INTEGRATION_14) é o runtime de processing/station ativo na FarmScene — usado para crafting com tempo, stamina e save de job em andamento. O sistema `Crafting/` é o runtime de unlock de receita por evento de first-kill (fable_49) e upgrade de equipamento tier alto — complementar, não substituto.

## Quando usar

- Spec adiciona nova `RecipeDataSO` ou novo `WorkshopType`.
- Spec adiciona unlock de recipe via first-kill de boss (fable_49).
- Spec conecta upgrade de equipamento (tier alto) a um gate de recipe.
- Spec de save/load que inclua jobs de crafting em andamento (`CraftingJobSaveData`).
- Checklist de closeout para qualquer spec que mencione "crafting", "recipe", "processing", "upgrade".

## Sistemas existentes (reusar, não duplicar)

### `Craft/` — runtime de station/job

| Classe | Papel |
|---|---|
| `CraftingRuntime` | MonoBehaviour; gerencia stations; `TryStartCraft`, `TryCollect`, `TryCancel`; `CaptureSaveData`/`LoadFromSaveData`; `ActiveInstances` (lista estática, sem FindObjectsOfType) |
| `CraftingStation` | Estado de uma estação: recipe em progresso, `CraftingJob`, `TryStartCraft`, `TryCollectOutput`, `TryCancelJob` |
| `CraftingJob` | DTO de job em andamento: `RecipeId`, `RemainingTime`, `IsComplete`; serializado em `CraftingJobSaveData` |
| `RecipeDataSO` | SO de recipe: `RecipeId`, `Ingredients[]`, `OutputItem`, `RequiredStationType`, `CraftingTimeSeconds`, `IsUnlockedByDefault`, `RecipeUnlockId` (vazio = sempre disponível) |
| `RecipeDatabaseSO` | SO de catálogo: `All` (lista de `RecipeDataSO`) |
| `CraftingPoint` | MonoBehaviour de interação na scene; delegado ao `CraftingRuntime` |
| `CraftingStationRuntimeBootstrap` | `RuntimeInitializeOnLoadMethod`; une `CraftingRuntime` ao `InventoryManager`+`StaminaManager` em AfterSceneLoad; lê de `CraftingRuntime.ActiveInstances` (não FindObjectsOfType) |
| `WorkshopType` | Enum de estação: `None` (pocket craft), `Workbench`, `Forge`, … |

### `Crafting/` — unlock/gate/upgrade

| Classe | Papel |
|---|---|
| `RecipeUnlockService` | Conjunto de slugs aprendidos; `IsUnlocked(id)`, `Unlock(id)` (idempotente); `CaptureUnlockedIds`/`RestoreUnlockedIds`; `static Active` |
| `CraftingRecipeGate` | Consulta `RecipeUnlockService.Active` antes de aceitar craft de recipe gated |
| `RecipeFirstKillUnlockHook` | Assina evento de first-kill e chama `RecipeUnlockService.Active.Unlock(id)` |
| `EquipmentUpgradeService` | Upgrade de item via `UpgradeRecipeSO` (tier alto); consome materiais via `InventoryManager` |
| `UpgradeRecipeSO` | SO de upgrade: `FromItemId`, `ToItemId`, requisitos de materiais e tier |
| `HighTierGearCanon` / `HighTierGearCatalog` | Catálogo de gear tier alto e regras de craft de tier alto |

## Procedimento

### Criar recipe nova (`Craft/`)

1. Criar `RecipeDataSO` asset: preencher `RecipeId` (slug único, ex.: `recipe_forge_iron_sword`), `Ingredients[]`, `OutputItem`, `RequiredStationType`, `CraftingTimeSeconds`.
2. Adicionar ao `RecipeDatabaseSO` existente (campo `All`).
3. Se a recipe exige unlock por boss: preencher `RecipeUnlockId` (slug, ex.: `recipe_unlock_mithril_work`) e garantir que `RecipeFirstKillUnlockHook` está wired ao evento de kill do boss.
4. Se `IsUnlockedByDefault = true`: recipe aparece desde o início na estação correta via `CraftingRuntime.GetRecipesForStation(WorkshopType)`.

### Salvar/restaurar job em andamento

```csharp
// Captura (em SaveManager ou provider):
var saveData = craftingRuntime.CaptureSaveData();
// Restore:
craftingRuntime.LoadFromSaveData(saveData);
```

`CraftingRuntimeSaveData` contém `List<CraftingStationSaveData>` — simples types (sem Unity refs).

### CraftTimeReduction derivada (F18)

`CraftingRuntime.TryStartCraft` já aplica `DerivedFollowupFormulas.CraftTimeMultiplier(reduction)` via `PlayerVitalsApplier.CraftTimeReductionSource` — não recalcule manualmente.

### Unlock por first-kill (`Crafting/`)

```csharp
// Em RecipeFirstKillUnlockHook ou outro handler:
bool firstTime = RecipeUnlockService.Active.Unlock("recipe_unlock_mithril_work");
if (firstTime)
    GameEventBus.Publish(new RecipeUnlockedEvent("recipe_unlock_mithril_work"));
```

## Testes

- `CraftingJob`, `CraftingStation` têm lógica determinística → EditMode:
  - Ingredientes suficientes → `TryStartCraft` retorna true; insuficientes → false + `failureReason`.
  - `RemainingTime` decrementa corretamente; `IsComplete` após tempo zerado.
  - Round-trip `CaptureSaveData` / `LoadFromSaveData` preserva job em andamento.
- `RecipeUnlockService` é pura → EditMode:
  - `IsUnlocked("")` → true (sem gating); `IsUnlocked("slug")` → false antes de `Unlock`.
  - `Unlock` é idempotente; segundo `Unlock` retorna false.

## Regras

- Consumo de ingredientes é atômico via `InventoryManager` — não remova ingredientes manualmente antes de confirmar o craft.
- Jobs em andamento **devem** ser incluídos no save (`CraftingRuntimeSaveData`) — não deixar job sem restore.
- Nunca criar um terceiro sistema de crafting para "simplificar" — ver alerta no topo.
- Recipe sem `RecipeUnlockId` (vazio) é sempre disponível; não adicionar check manual de unlock.

## Relacionados

- `(skill: system-reuse-audit)` — obrigatória antes de tocar em crafting
- `(skill: economy-balance-tuning)` — processing agrega valor por tempo; checar anti-arbitrage
- `(skill: inventory-transactions)` — consumo de ingredientes atômico
- `(skill: editmode-test-authoring)` — timing de job e idempotência pós-reload
- `(skill: save-load-pattern)` — `CraftingRuntimeSaveData` segue DTOs de tipos simples
