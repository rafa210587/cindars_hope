# SPEC 07 - Crafting Validation - 2026-05-24

## Automated Evidence

| Validacao | Resultado | Evidencia |
|---|---|---|
| Unity compile | PASS | `Logs/unity-compile-spec07-final.log`: `Tundra build success`, sem `error CS`, retorno interno `0`. |
| Crafting domain | PASS | `Logs/spec07-crafting-validation-final.log`: instant craft, timed job, cancel, collect, full inventory retention, save/load e exclusividade Inventory/Crafting passaram. |
| FarmScene wiring | PASS | `Logs/spec07-scene-validation-final.log`: runtime/modal e Workbench/Forge/CookingStation passaram. |
| Unity log scanner | FAIL documentado | Marca apenas `Assembly-CSharp-Editor-firstpass.dll` e `Assembly-CSharp-firstpass.dll` invalidos; nao encontrou erro C#. |

## Delivered Test Content

- `recipe_pocket_processed_wood`: `None`, instantaneo.
- `recipe_workbench_processed_wood`: `Workbench`, instantaneo.
- `recipe_forge_iron_sword`: `Forge`, temporizado.
- `recipe_cooking_bread`: `CookingStation`, temporizado.
- Starter/test kit: madeira, minerio de ferro e trigo suficientes para validar os fluxos iniciais.

## Play Mode Checklist

```text
PLAY MODE TEST: SPEC 07 - Crafting Queue, Workstations, Recipes e UI
Scene used: Assets/_Game/Scenes/FarmScene.unity
Steps executed: NOT RUN
Expected result:
1. C abre craft de bolso e executa recipe instantanea compativel.
2. E em Workbench abre CraftingModal e cria output instantaneo.
3. E em Forge/CookingStation inicia job temporizado; output requer coleta.
4. Cancel em job ativo devolve 100% dos ingredientes.
5. Inventory cheio impede coleta sem remover o output da station.
6. Save/load preserva job ativo e output completo.
7. CraftingModal nao se sobrepoe a shop/dialogue/inventory.
Observed result: NOT RUN
Bugs found: N/A
Passed: NOT RUN
Evidence: Automated batchmode logs listed above.
```

Reason: a validacao disponivel nesta execucao e batchmode e nao conduz input interativo de Play Mode.
Command attempted: validadores batchmode `ValidateCraftingSystem.ValidateSpec07` e `MvpSceneValidator.ValidateSpec07Scene`.
Residual risk: input/UX interativo e fluxo de save/load acionado pelo jogador ainda requerem validacao humana final.
