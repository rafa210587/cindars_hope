# spec_arch_core_world_cycle_reduction_v11

> **Status:** Implementado e BUILD_VALIDATED  
> **Data:** 2026-07-07  
> **Escopo:** quebrar o par mútuo `Core|World` sem alterar gameplay, saves, cenas, prefabs, IDs, balanceamento ou eventos observáveis.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla:

```text
Antes:
- MutualModulePairs=40
- RuntimeModuleEdges=229
- Par presente: Core|World

Depois:
- MutualModulePairs=39
- RuntimeModuleEdges=228
- Par removido: Core|World
```

## Diagnóstico

O ciclo era causado por:

- `World -> Core`: direção legítima. Serviços de World usam `GameEventBus` e eventos base.
- `Core -> World`: `WeatherChangedEvent` morava em `Core.Events`, mas carregava `WeatherType` de `World.Weather` e só era publicado pelo `WorldWeatherService`.

## Implementação

- `WeatherChangedEvent` foi movido de `CindarsHope.Core.Events` para `CindarsHope.World.Weather`.
- O arquivo `.meta` foi movido junto com o `.cs` para preservar GUID.
- `WorldWeatherService` continua publicando o mesmo payload via `GameEventBus`.
- Nenhum valor de `WeatherType` foi alterado.
- Nenhum save/schema/cena/prefab/asset foi alterado.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  exit 0
  MutualModulePairs: 40 -> 39
  RuntimeModuleEdges: 229 -> 228
  Core|World removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1
  exit 0
  2747/2747 PASS
  Resultados: TestResults/modularization-core-world-editmode.xml
  Log: Logs/modularization-core-world-editmode.log
```

## Pendências

Esta spec-filha fecha apenas `Core|World`. A modularização ampla não está concluída: ainda restam 39 pares mútuos para specs-filhas independentes.
