# spec_arch_npcshop_choice_ui_adapter_v15

> **Status:** Implementado e BUILD_VALIDATED  
> **Data:** 2026-07-07  
> **Escopo:** reduzir responsabilidade de apresentação do `NpcShopController` ao converter escolhas de domínio para escolhas de UI, sem alterar gameplay, saves, cenas, prefabs, IDs, horários, diálogos, preços ou quantidades.

## Objetivo

Executar mais um recorte seguro da fase `NpcShopController`:

- remover do controller a conversão manual de `NpcShopChoiceDefinition` para `DialogueChoice`;
- deixar a adaptação de UI no módulo `UI.Dialogue`;
- preservar labels e choice IDs.

## Implementação

- Criado `NpcShopChoiceUiAdapter` em `CindarsHope.UI.Dialogue`.
- `NpcShopController` passou a usar `NpcShopChoiceUiAdapter.ToUiChoices`.
- Removido o método privado `ToUiChoices` do controller.
- Nenhum fluxo de diálogo, serviço, shop, quest ou debug expression foi alterado.

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
  Resultados: TestResults/npcshop-choice-ui-adapter-editmode.xml
  Log: Logs/npcshop-choice-ui-adapter-editmode.log
```

## Pendências

Esta spec fecha apenas a adaptação de escolhas para UI. Ainda faltam extrações maiores do `NpcShopController`:

- `NpcDialogueFlowController`;
- `NpcGiftInteractionService`;
- `NpcQuestInteractionBridge`;
- `NpcShopTransactionFacade`;
- `NpcSchedulePresentationAdapter`.

