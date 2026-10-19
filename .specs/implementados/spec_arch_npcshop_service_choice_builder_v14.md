# spec_arch_npcshop_service_choice_builder_v14

> **Status:** Implementado e BUILD_VALIDATED  
> **Data:** 2026-07-07  
> **Escopo:** reduzir responsabilidade do `NpcShopController` na montagem de opções de serviços únicos de NPC, sem alterar gameplay, saves, cenas, prefabs, IDs, horários, diálogos, preços, quantidades ou execução dos serviços.

## Objetivo

Executar mais um recorte seguro da fase `NpcShopController`:

- remover do controller a montagem manual de `NpcShopServiceChoiceDefinition`;
- manter o fluxo de menu/diálogo e o comportamento de opções gated;
- preservar `NpcServiceAccess` como fonte única de serviços do NPC.

## Implementação

- Criado `NpcShopServiceChoiceBuilder`.
- `NpcShopController.BuildNpcServiceChoices` passou a delegar para o builder.
- Preservado o comportamento anterior:
  - uma opção por serviço único;
  - serviços gated continuam aparecendo;
  - `ServiceId` vazio continua ignorado;
  - labels continuam vindo de `NpcServiceAccess.BuildOptions`.

## Evidência

```text
dotnet build .\CindarsHope.Runtime.csproj
  exit 0
  0 warnings
  0 errors

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1
  exit 0
  2747/2747 PASS
  Resultados: TestResults/npcshop-service-choice-builder-editmode.xml
  Log: Logs/npcshop-service-choice-builder-editmode.log
```

## Pendências

Esta spec fecha apenas o builder de escolhas de serviços únicos. Ainda faltam extrações maiores do `NpcShopController`:

- `NpcDialogueFlowController`;
- `NpcGiftInteractionService`;
- `NpcQuestInteractionBridge`;
- `NpcShopTransactionFacade`;
- `NpcSchedulePresentationAdapter`.

