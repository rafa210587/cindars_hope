# spec_arch_scene_world_cycle_reduction_v9

> **Status:** Implementado e BUILD_VALIDATED  
> **Data:** 2026-07-07  
> **Escopo:** quebrar o par mútuo `SceneManagement|World` sem alterar gameplay, saves, cenas, prefabs, IDs, balanceamento ou fluxo de transição.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla:

```text
Antes:
- MutualModulePairs=42
- RuntimeModuleEdges=231
- Par presente: SceneManagement|World

Depois:
- MutualModulePairs=41
- RuntimeModuleEdges=230
- Par removido: SceneManagement|World
```

## Diagnóstico

O ciclo era causado por:

- `SceneManagement -> World`: direção legítima. Installers e portais de cena rebindam serviços/world objects como `TreeNode`, `FishingSpot` e contratos de spawn.
- `World -> SceneManagement`: dependência evitável. O router/resolver em `World.Scenes` importava contratos de transição que pertencem ao vocabulário de mundo/cena (`SceneNames`, `SceneTransitionState`, `SceneSpawnPoint` legacy).

## Implementação

- Movidos para `CindarsHope.World.Scenes`:
  - `SceneNames`;
  - `SceneTransitionState`;
  - `SceneSpawnPoint`.
- Os arquivos `.meta` foram movidos junto com os `.cs` para preservar GUID dos scripts Unity.
- `SceneTransitionRouter`, `SceneId` e `PlayerSpawnResolver` deixaram de importar `CindarsHope.SceneManagement`.
- `SceneManagement` continua consumindo os contratos de `World.Scenes` onde necessário.
- Nenhum campo serializado foi renomeado.
- Nenhum ID de cena/spawn foi alterado.
- Nenhum save/schema/cena/prefab foi alterado.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  exit 0
  MutualModulePairs: 42 -> 41
  RuntimeModuleEdges: 231 -> 230
  SceneManagement|World removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1
  exit 0
  2747/2747 PASS
  Resultados: TestResults/modularization-scene-world-editmode.xml
  Log: Logs/modularization-scene-world-editmode.log
```

## Pendências

Esta spec-filha fecha apenas `SceneManagement|World`. A modularização ampla não está concluída: ainda restam 41 pares mútuos para specs-filhas independentes.
