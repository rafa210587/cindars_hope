# spec_arch_fonte_mainprogression_cycle_reduction_v6

> **Status:** Implementado e BUILD_VALIDATED  
> **Data:** 2026-07-07  
> **Escopo:** quebrar o par mútuo `Fonte|MainProgression` sem alterar gameplay, saves, cenas, prefabs, IDs, balanceamento ou fluxo de final choice.

## Objetivo

Reduzir um ciclo residual pequeno da modularização ampla:

```text
Antes:
- MutualModulePairs=45
- RuntimeModuleEdges=234
- Par presente: Fonte|MainProgression

Depois:
- MutualModulePairs=44
- RuntimeModuleEdges=233
- Par removido: Fonte|MainProgression
```

## Diagnóstico

O ciclo era causado por duas direções:

- `Fonte -> MainProgression`: direção legítima. A Fonte integra fragmentos, avalia unlocks e hospeda a `MainProgressionSection`.
- `MainProgression -> Fonte`: direção desnecessária. `FinalChoiceService` e `FinalChoiceRuntimeAdapter` dependiam diretamente de `FonteAnyaSection`/`FonteState` apenas para aplicar o estado final da Fonte.

## Implementação

- Criado `IFinalChoiceFonteStateSink` em `CindarsHope.MainProgression`.
- `FinalChoiceService.EvaluateFinalChoice` agora recebe o contrato `IFinalChoiceFonteStateSink`, não `FonteAnyaSection`.
- `FinalChoiceRuntimeAdapter.Preview/Apply` usam o mesmo contrato.
- `FonteAnyaSection` implementa `IFinalChoiceFonteStateSink` e mantém o mapeamento concreto de string canônica para `FonteState`.
- Nenhum campo serializado foi renomeado.
- Nenhum save DTO foi alterado.
- Nenhuma cena/prefab/asset foi alterado.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  exit 0
  MutualModulePairs: 45 -> 44
  RuntimeModuleEdges: 234 -> 233
  Fonte|MainProgression removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1 -Filter "CindarsHope.Tests.EditMode.MainProgression"
  exit 0
  2747/2747 PASS
  Resultados: TestResults/modularization-fonte-mainprogression-editmode.xml
  Log: Logs/modularization-fonte-mainprogression-editmode.log
```

Observação operacional: a primeira tentativa de build direto `dotnet build --no-restore` falhou por ausência de `Temp/obj/*/project.assets.json`. Após `dotnet restore .\CindarsHope.Runtime.csproj`, o build wrapper oficial passou.

## Pendências

Esta spec-filha fecha apenas `Fonte|MainProgression`. A modularização ampla não está concluída: ainda restam 44 pares mútuos para specs-filhas independentes.
