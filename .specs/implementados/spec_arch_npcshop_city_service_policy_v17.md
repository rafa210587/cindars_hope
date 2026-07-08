# spec_arch_npcshop_city_service_policy_v17

Status: Implementado e BUILD_VALIDATED.

## Escopo

Recorte incremental da fase `NpcShopController` do plano de modularização restante.

Objetivo: remover do controller a regra de escolha/compra de serviço cívico da cidade sem alterar gameplay, saves, cenas, prefabs, IDs, horários, diálogos, preços ou quantidades.

## Mudança implementada

- Criado `NpcCityServiceChoicePolicy`.
- `NpcShopController.TryGetCityServiceChoice` agora delega:
  - verificação de provedor canônico;
  - resolução de `serviceId`;
  - label de serviço contratado;
  - label canônico vindo de `CityServiceCatalog`.
- `NpcShopController.PurchaseCityServiceForThisNpc` agora delega:
  - resolução de `serviceId`;
  - chamada idempotente a `CityServiceAccess.TryPurchase`;
  - fallback `"Servico indisponivel."`.
- O controller continua responsável por transformar a escolha em `UiDialogueChoice` e publicar `PlayerActionFeedbackEvent`.

## Não-regressão

- `ChoiceId` continua `"service"`.
- Texto `"Servico (ja contratado)"` foi preservado.
- Fallback `"Servico indisponivel."` foi preservado.
- Ausência de `serviceId` continua retornando sem publicar feedback.
- `CityServiceCatalog` e `CityServiceAccess` continuam sendo as fontes canônicas de catálogo, posse, label e compra.
- Nenhum save/schema/ID/cena/prefab/balanceamento foi alterado.

## Evidência

- `dotnet build .\CindarsHope.Runtime.csproj`: exit 0, 0 warnings, 0 errors.
- `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
- `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS.
  - Results: `TestResults/npcshop-city-service-policy-editmode.xml`
  - Log: `Logs/npcshop-city-service-policy-editmode.log`
- `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0.
  - `RuntimeModuleEdges=227`
  - `MutualModulePairs=38`
  - Recorte sem novo ciclo arquitetural.

## Pendências

`NpcShopController` ainda não está concluído como refactor amplo. Recortes pendentes recomendados:

- debug expression menu;
- gift feedback/entrada de selector;
- quest bridge de Thalindra;
- transaction facade de buy/sell;
- dialog/tree presenter.
