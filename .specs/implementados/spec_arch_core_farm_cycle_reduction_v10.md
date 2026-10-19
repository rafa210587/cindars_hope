# spec_arch_core_farm_cycle_reduction_v10

> **Status:** Implementado e BUILD_VALIDATED  
> **Data:** 2026-07-07  
> **Escopo:** quebrar o par mútuo `Core|Farm` sem alterar gameplay, saves, cenas, prefabs, IDs, balanceamento ou assets.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla:

```text
Antes:
- MutualModulePairs=41
- RuntimeModuleEdges=230
- Par presente: Core|Farm

Depois:
- MutualModulePairs=40
- RuntimeModuleEdges=229
- Par removido: Core|Farm
```

## Diagnóstico

O ciclo era causado por:

- `Farm -> Core`: direção legítima. O domínio Farm consome contratos/dados comuns de Core.
- `Core -> Farm`: `SeedDatabaseSO` morava em `Core.Data`, mas era um catálogo específico de sementes/fazenda e dependia de `SeedDataSO` de `Farm.Data`.

## Implementação

- `SeedDatabaseSO` foi movido de `CindarsHope.Core.Data` para `CindarsHope.Farm.Data`.
- O arquivo `.meta` foi movido junto com o `.cs` para preservar GUID do ScriptableObject.
- A base `DataRegistrySO<SeedDataSO>` continua em `Core.Data`.
- Consumidores editor/runtime já importavam ou passaram a importar `CindarsHope.Farm.Data`.
- Nenhum ID de seed foi alterado.
- Nenhum asset, cena, prefab, save/schema ou balanceamento foi alterado.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  exit 0
  MutualModulePairs: 41 -> 40
  RuntimeModuleEdges: 230 -> 229
  Core|Farm removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1
  exit 0
  2747/2747 PASS
  Resultados: TestResults/modularization-core-farm-editmode.xml
  Log: Logs/modularization-core-farm-editmode.log
```

## Pendências

Esta spec-filha fecha apenas `Core|Farm`. A modularização ampla não está concluída: ainda restam 40 pares mútuos para specs-filhas independentes.
