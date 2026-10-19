# spec_arch_save_world_cycle_reduction_v21

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-07
> **Escopo:** quebrar o par mútuo `Save|World` sem alterar gameplay, saves, cenas, prefabs, IDs ou balanceamento.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla (Tier 1 do
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`):

```text
Antes:
- MutualModulePairs=37
- RuntimeModuleEdges=226
- Par presente: Save|World

Depois:
- MutualModulePairs=36
- RuntimeModuleEdges=225
- Par removido: Save|World
```

## Diagnóstico

O ciclo era causado por:

- `Save -> World`: direção legítima. `WorldSectionProvider` (em `CindarsHope.Save.Providers`) já
  injeta `ItemPickupRegistry` e `TreeRegistry` via construtor.
- `World -> Save`: dois arquivos de World importavam `CindarsHope.Save` só para tipar assinaturas
  de método:
  - `TreeRegistry.RestoreFromSaveData(FarmSaveData saveData)` lia apenas `saveData.Trees`.
  - `GameCalendarService.RestoreFromSaveData(CalendarSaveData saveData)` lia apenas
    `saveData.AbsoluteDayIndex`.

Verificação de Passo 0 (obrigatória antes do corte): grep de `using CindarsHope.Save` em toda a
pasta `Assets/_Game/Scripts/World/` confirmou que `TreeRegistry.cs` e
`Calendar/GameCalendarService.cs` eram os **únicos** arquivos de World importando o namespace Save.
Outros 9 arquivos de World que casavam com o grep textual de `SaveData` (`GodMarkSaveData`,
`TreeSaveData`, `ItemPickupSaveData` etc.) são DTOs já owned pelo próprio namespace `CindarsHope.World`
— não geravam a aresta.

Achado adicional durante a verificação: `GameCalendarService.RestoreFromSaveData(CalendarSaveData)`
não tinha **nenhum caller** em todo o repositório (grep completo de `RestoreFromSaveData` confirmado).
`CalendarSaveData` (`Assets/_Game/Scripts/Save/CalendarSaveData.cs`) também não é referenciado em
nenhum outro lugar — não faz parte do `GameSaveData` raiz. Era código morto isolado, sem risco de
regressão ao estreitar a assinatura.

`TreeRegistry.RestoreFromSaveData` tinha dois callers, ambos identificados e ajustados:
`WorldSectionProvider` (`CindarsHope.Save.Providers`) e `FarmSceneRuntimeStateCache`
(`CindarsHope.SceneManagement`, cache de estado entre troca de cena — não fazia parte da estratégia
original do mapa, adicionado à verificação por segurança).

## Implementação

- `TreeRegistry.RestoreFromSaveData(FarmSaveData saveData)` virou
  `TreeRegistry.RestoreFromSaveData(IReadOnlyList<TreeSaveData> trees)` — mesmo corpo (mesmo guard de
  nulo, mesmo loop, mesmo fallback de tree ausente com `Debug.LogWarning`), só trocando o parâmetro
  DTO por uma lista de `TreeSaveData` (já `CindarsHope.World`).
- `WorldSectionProvider.Restore` deixou de montar um `FarmSaveData` wrapper só para carregar
  `Trees`; agora chama `_treeRegistry.RestoreFromSaveData(data.Trees ?? new List<TreeSaveData>())`
  diretamente.
- `FarmSceneRuntimeStateCache.TryRestore` teve o mesmo ajuste — chama
  `treeRegistry.RestoreFromSaveData(_cachedWorldState?.Trees ?? new List<TreeSaveData>())` sem
  montar `FarmSaveData` wrapper. `FarmSaveData` continua usada nesse arquivo para o estado de
  `_cachedFarmState`/plots (sem alteração).
- `GameCalendarService.RestoreFromSaveData(CalendarSaveData saveData)` virou
  `GameCalendarService.RestoreFromAbsoluteDay(int absoluteDayIndex)` — sem caller, renomeado e
  estreitado para primitivo conforme a estratégia do mapa; nenhum comportamento observável muda
  porque não havia comportamento observado (método morto).
- `using CindarsHope.Save;` removido de `TreeRegistry.cs` e de
  `Calendar/GameCalendarService.cs`.
- Nenhum DTO (`FarmSaveData`, `TreeSaveData`, `CalendarSaveData`, `WorldSaveData`) foi movido de
  namespace; nenhum schema/campo de save foi alterado.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  exit 0
  MutualModulePairs: 37 -> 36
  RuntimeModuleEdges: 226 -> 225
  Save|World removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-save-world-editmode.xml -LogFile Logs\cut-save-world.log
  exit 0
  2747/2747 PASS
  Resultados: TestResults/cut-save-world-editmode.xml
  Log: Logs/cut-save-world.log
```

## Pendências

Esta spec-filha fecha apenas `Save|World`. A modularização ampla não está concluída: ainda restam
36 pares mútuos para specs-filhas independentes.
