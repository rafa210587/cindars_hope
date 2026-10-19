# spec_arch_locations_player_cycle_reduction_v7

> **Status:** Implementado e BUILD_VALIDATED  
> **Data:** 2026-07-07  
> **Escopo:** quebrar o par mútuo `Locations|Player` sem alterar gameplay, saves, cenas, prefabs, IDs, balanceamento ou fluxo de respawn.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla:

```text
Antes:
- MutualModulePairs=44
- RuntimeModuleEdges=233
- Par presente: Locations|Player

Depois:
- MutualModulePairs=43
- RuntimeModuleEdges=232
- Par removido: Locations|Player
```

## Diagnóstico

O ciclo era causado por duas direções:

- `Locations -> Player`: `AnyaFountainInteractable` ainda usa `AnyaRespawnService` para abrir o menu/serviço de respawn. Essa direção foi preservada.
- `Player -> Locations`: `AnyaFountainRespawnFlow` importava `CindarsHope.Locations` apenas para localizar `AnyaFountain` e ler `RespawnPoint`.

## Implementação

- Criado `IAnyaFountainRespawnPoint` em `CindarsHope.Core.Respawn`.
- `AnyaFountain` implementa esse contrato e continua expondo o mesmo `RespawnPoint`.
- `AnyaFountainRespawnFlow` deixou de importar `CindarsHope.Locations` e resolve a âncora ativa pelo contrato.
- Nenhum campo serializado foi renomeado.
- Nenhum save DTO foi alterado.
- Nenhuma cena/prefab/asset foi alterado.
- `CindarsHope.Runtime.csproj` foi atualizado para incluir o novo arquivo runtime.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  exit 0
  MutualModulePairs: 44 -> 43
  RuntimeModuleEdges: 233 -> 232
  Locations|Player removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1
  exit 0
  2747/2747 PASS
  Resultados: TestResults/modularization-locations-player-editmode.xml
  Log: Logs/modularization-locations-player-editmode.log
```

## Pendências

Esta spec-filha fecha apenas `Locations|Player`. A modularização ampla não está concluída: ainda restam 43 pares mútuos para specs-filhas independentes.
