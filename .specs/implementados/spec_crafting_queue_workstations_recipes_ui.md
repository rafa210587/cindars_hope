# SPEC - Crafting queue, workstations, recipes e UI

> Spec ID: spec_crafting_queue_workstations_recipes_ui
> Status: Implementado completo
> Ordem de execucao: 07
> Data de fechamento: 2026-05-24
> Refinement: `docs/refinements/implementados/ref_crafting_queue_workstations_recipes_ui.md`

## Escopo Entregue

- `RecipeDataSO` permanece a unica fonte oficial de recipes e inclui station type, tempo, unlock hooks e stamina hook.
- `CraftingRuntime` gerencia uma estacao de bolso e workstations por ID estavel.
- `CraftingStation` executa craft instantaneo atomicamente, jobs com tempo, cancelamento com rollback, coleta segura e DTOs simples de save/load.
- `CraftingModal` usa a exclusividade de `ModalManager`, abre por `E` nas estacoes e por `C` para craft de bolso.
- `FarmScene` contem `farm_workbench_01`, `farm_forge_01` e `farm_cooking_01`.
- O starter/test kit e quatro recipes de validacao sao criados por `CraftingRecipeInitializer` idempotente.
- Eventos de abertura, job, cancelamento, coleta e falha sao publicados por `GameEventBus`.

## Persistencia

`CraftingRuntimeSaveData`, `CraftingStationSaveData`, `CraftingJobSaveData` e `CraftingIngredientSaveData` persistem somente IDs, status, quantidades e tempo restante. Um recipe ausente no load mantem output/ingredientes salvos e registra erro, sem perda silenciosa.

## Evidencia

- `Assets/_Game/Scripts/Craft/CraftingRuntime.cs`
- `Assets/_Game/Scripts/Craft/CraftingStation.cs`
- `Assets/_Game/Scripts/UI/Crafting/CraftingModal.cs`
- `Assets/_Game/Scripts/Core/Events/CraftingEvents.cs`
- `Assets/_Game/Scripts/Editor/CraftingRecipeInitializer.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateCraftingSystem.cs`
- `Assets/_Game/Scenes/FarmScene.unity`
- `docs/validation/SPEC_07_CRAFTING_VALIDATION_20260524.md`

## Validacao

- Unity compile: PASS por `Tundra build success`, sem `error CS`, com retorno interno do Unity `0`.
- Crafting automated validation: PASS para instant craft, timed job, cancel, collect, full inventory retention e save/load.
- Scene wiring validation: PASS para runtime/modal e tres workstations.
- `ScanUnityLogs.ps1`: acusa apenas assemblies `Assembly-CSharp*-firstpass.dll` invalidos, ruido documentado do harness sem erro de compilacao.
- Play Mode humano: NOT RUN; checklist registrado no artefato de validacao.
