# spec_arch_craft_save_cycle_reduction_v8

> **Status:** Implementado e BUILD_VALIDATED  
> **Data:** 2026-07-07  
> **Escopo:** quebrar o par mútuo `Craft|Save` sem alterar gameplay, saves, cenas, prefabs, IDs, balanceamento ou schema.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla:

```text
Antes:
- MutualModulePairs=43
- RuntimeModuleEdges=232
- Par presente: Craft|Save

Depois:
- MutualModulePairs=42
- RuntimeModuleEdges=231
- Par removido: Craft|Save
```

## Diagnóstico

O ciclo era causado por:

- `Save -> Craft`: direção legítima. `CraftingSectionProvider` captura/restaura `CraftingRuntime`.
- `Craft -> Save`: `CraftingStation.cs` mantinha `using CindarsHope.Save`, mas os DTOs usados por `CraftingStation` (`CraftingStationSaveData`, `CraftingJobSaveData`) já pertencem ao domínio `Craft`.

## Implementação

- Removido `using CindarsHope.Save` morto de `CraftingStation.cs`.
- Nenhum tipo foi movido.
- Nenhum DTO foi alterado.
- Nenhum campo serializado foi renomeado.
- Nenhuma cena/prefab/asset foi alterado.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  exit 0
  MutualModulePairs: 43 -> 42
  RuntimeModuleEdges: 232 -> 231
  Craft|Save removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1
  exit 0
  2747/2747 PASS
  Resultados: TestResults/modularization-craft-save-editmode.xml
  Log: Logs/modularization-craft-save-editmode.log
```

## Pendências

Esta spec-filha fecha apenas `Craft|Save`. A modularização ampla não está concluída: ainda restam 42 pares mútuos para specs-filhas independentes.
