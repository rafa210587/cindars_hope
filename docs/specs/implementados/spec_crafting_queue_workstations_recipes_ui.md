# SPEC - Crafting queue, workstations, recipes e UI

> Spec ID: spec_crafting_queue_workstations_recipes_ui
> Status: Implementado
> Ordem de execucao: 07
> Data: 2026-05-24
> Evidencia: Assets/_Game/Scripts/Craft/CraftingRuntime.cs, CraftingStation.cs, RecipeDataSO.cs, CraftingJob.cs, SaveData.cs (CraftingRuntimeSaveData)

## Resumo de Implementacao

### Implementado:
- `CraftingRuntime` como manager de workstations
- `CraftingStation` para cada estacao com queue/jobs
- `CraftingJob` para representar um craft em andamento
- `RecipeDataSO` com campos: CraftTimeSeconds, StaminaCost, IsUnlocked, IsInstantaneous
- Craft instantaneo quando CraftTimeSeconds <= 0
- Craft com tempo quando CraftTimeSeconds > 0
- Validacao de ingredientes antes de iniciar craft
- Consumo de ingredientes ao iniciar craft
- Output aguardando coleta apos craft completo
- Cancelamento seguro devolvendo 100% ingredientes
- Save/load de jobs em andamento
- CraftingRuntimeSaveData para persistencia
- 1 job ativo/completo por estacao (MVP)
- Status enum: Pending, InProgress, Completed, Cancelled

### Nao implementado (futuro):
- Fila multipla de jobs
- UI/Modal final de crafting integrado
- Stamina cost enforcement
- Workstations fisicas presentes na FarmScene
- Recipe unlock avancado
- Durabilidade/qualidade de items crafted
- Crafting de bolso

### Validacao pendente:
- Unity compile validation
- Play mode test de crafting completo
- Integracao de UI modal de crafting
