# spec_arch_npcshop_special_identity_policy_v16

> **Status:** Implementado e BUILD_VALIDATED  
> **Data:** 2026-07-07  
> **Escopo:** reduzir duplicação do `NpcShopController` ao identificar NPCs especiais, sem alterar gameplay, saves, cenas, prefabs, IDs, horários, diálogos, preços ou quantidades.

## Objetivo

Executar mais um recorte seguro da fase `NpcShopController`:

- extrair a regra de identificação de `Thalindra` e `Brumdar`;
- preservar fallback por `NpcId` e por token no `DisplayName`;
- manter as mesmas comparações case-insensitive.

## Implementação

- Criado `NpcSpecialIdentityPolicy`.
- `NpcShopController.IsThalindra` e `NpcShopController.IsBrumdar` delegam para a policy.
- Nenhum fluxo de quest, tempering, shop ou diálogo foi alterado.

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
  Resultados: TestResults/npcshop-special-identity-policy-editmode.xml
  Log: Logs/npcshop-special-identity-policy-editmode.log
```

## Pendências

Esta spec fecha apenas a policy de identidade especial. Ainda faltam extrações maiores do `NpcShopController`:

- `NpcDialogueFlowController`;
- `NpcGiftInteractionService`;
- `NpcQuestInteractionBridge`;
- `NpcShopTransactionFacade`;
- `NpcSchedulePresentationAdapter`.

