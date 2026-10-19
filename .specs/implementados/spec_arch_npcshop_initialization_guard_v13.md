# spec_arch_npcshop_initialization_guard_v13

> **Status:** Implementado e BUILD_VALIDATED  
> **Data:** 2026-07-07  
> **Escopo:** reduzir responsabilidade de inicialização do `NpcShopController` sem alterar gameplay, saves, cenas, prefabs, IDs, horários, diálogos, preços, quantidades ou fluxos de shop.

## Objetivo

Executar um recorte seguro da fase `NpcShopController` do plano restante de modularização:

- retirar do controller a validação repetitiva de referências obrigatórias;
- preservar a ordem atual de validação e o comportamento de short-circuit;
- manter todos os fields serializados e APIs externas intactos.

## Implementação

- Criado `NpcShopInitializationGuard`.
- `NpcShopController.TryEnsureShopInitialized` passou a delegar a validação de referências obrigatórias para o guard.
- Removido o método local `ValidateReference` do controller.
- Preservado o comportamento anterior:
  - mesma ordem de validação;
  - mesma mensagem de erro;
  - só a primeira referência ausente é logada, como antes;
  - nenhum fluxo de shop/diálogo/transação/horário foi alterado.

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
  Resultados: TestResults/npcshop-initialization-guard-editmode.xml
  Log: Logs/npcshop-initialization-guard-editmode.log
```

## Pendências

Esta spec fecha apenas o primeiro recorte de manutenção do `NpcShopController`. Ainda faltam extrações maiores:

- `NpcDialogueFlowController`;
- `NpcShopAvailabilityPolicy`/expansão do gate atual, se necessário;
- `NpcGiftInteractionService`;
- `NpcQuestInteractionBridge`;
- `NpcShopTransactionFacade`;
- `NpcSchedulePresentationAdapter`.

