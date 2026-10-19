# spec_arch_core_npc_cycle_reduction_v12

> **Status:** Implementado e BUILD_VALIDATED  
> **Data:** 2026-07-07  
> **Escopo:** quebrar o par mútuo `Core|NPC` sem alterar gameplay, saves, cenas, prefabs, IDs, balanceamento ou payloads de evento.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla:

```text
Antes:
- MutualModulePairs=39
- RuntimeModuleEdges=228
- Par presente: Core|NPC

Depois:
- MutualModulePairs=38
- RuntimeModuleEdges=227
- Par removido: Core|NPC
```

## Diagnóstico

O ciclo era causado por:

- `NPC -> Core`: direção legítima. NPC usa `GameEventBus` e eventos base.
- `Core -> NPC`: três eventos em `Core.Events` carregavam enums/tipos de NPC (`NpcExpression`, `GiftTaste`, `RomanceStage`, `RomanceConfessionRejection`).

## Implementação

- Movidos para `CindarsHope.NPC.Events`:
  - `NpcExpressionOverrideEvent`;
  - `NpcGiftReactionEvent`;
  - `RomanceStageChangedEvent` / `RomanceConfessionRejectedEvent`.
- Os arquivos `.meta` foram movidos junto com os `.cs` para preservar GUID.
- Consumidores em NPC, Quests, UI e testes passaram a importar `CindarsHope.NPC.Events`.
- Payloads, nomes de eventos, semântica de publicação e subscriptions foram preservados.
- Nenhum save/schema/cena/prefab/asset foi alterado.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  exit 0
  MutualModulePairs: 39 -> 38
  RuntimeModuleEdges: 228 -> 227
  Core|NPC removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1
  exit 0
  2747/2747 PASS
  Resultados: TestResults/modularization-core-npc-editmode.xml
  Log: Logs/modularization-core-npc-editmode.log
```

## Pendências

Esta spec-filha fecha apenas `Core|NPC`. A modularização ampla não está concluída: ainda restam 38 pares mútuos para specs-filhas independentes.
