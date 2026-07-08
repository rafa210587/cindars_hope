# spec_arch_npc_debug_expression_policy_v18

Status: Implementado e BUILD_VALIDATED.

## Escopo

Recorte incremental da fase de modularização de NPCs.

Objetivo: remover duplicação de IDs, labels e parse de debug expression entre `NpcController` e `NpcShopController`, sem alterar UI, eventos publicados, diálogos, saves, cenas, prefabs, IDs de domínio ou gameplay.

## Mudança implementada

- Criado `NpcDebugExpressionChoicePolicy`.
- `NpcController` e `NpcShopController` passaram a usar a mesma policy para:
  - `OpenChoiceId = "dbg_open"`;
  - `BackChoiceId = "dbg_back"`;
  - lista de escolhas de expressão;
  - parse de `NpcExpression` a partir de `ChoiceId`.
- `NpcShopChoiceUiAdapter` converte as escolhas de domínio para `DialogueChoice`.

## Não-regressão

- Labels preservados: `"Neutro"`, `"Felicidade"`, `"Amor"`, `"Desdem"`, `"Odio"`, `"Voltar"`.
- IDs preservados: `"dbg_open"`, `"dbg_back"` e prefixo interno `"dbg:"`.
- O texto do modal `"[Debug] Trocar expressao:"` foi preservado.
- `NpcExpressionOverrideEvent` continua sendo publicado apenas quando há `_npcData`.
- A opção de debug continua condicionada a `UnityEngine.Debug.isDebugBuild`.
- Nenhum save/schema/ID/cena/prefab/balanceamento foi alterado.

## Evidência

- `dotnet build .\CindarsHope.Runtime.csproj`: exit 0, 0 warnings, 0 errors.
- `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
- `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS.
  - Results: `TestResults/npc-debug-expression-policy-editmode.xml`
  - Log: `Logs/npc-debug-expression-policy-editmode.log`
- `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0.
  - `RuntimeModuleEdges=227`
  - `MutualModulePairs=38`
  - Recorte sem novo ciclo arquitetural.

## Pendências

`NpcShopController` ainda não está concluído como refactor amplo. Recortes pendentes recomendados:

- dialog/tree presenter;
- gift feedback/entrada de selector;
- quest bridge de Thalindra;
- transaction facade de buy/sell;
- availability/schedule presentation.
