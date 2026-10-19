# SPEC 09 - Hunger, Stamina, Status e Time Balance Validation - 2026-05-24

## Automated Evidence

| Validacao | Resultado | Evidencia |
|---|---|---|
| Unity compile | PASS | `Logs/spec09-scene-validation-final.log`: `*** Tundra build success` e retorno interno Unity `0`. |
| Farm/Town/Cave scene generation | PASS | `Logs/spec09-generate-farm-final.log`, `Logs/spec09-generate-town-final.log` e `Logs/spec09-generate-cave-final.log`: cenas criadas e retorno interno `0`. |
| SPEC 09 scene/assets wiring | PASS | `Logs/spec09-scene-validation-final.log`: bootstrap, dependencies e assets de balance passaram nas tres cenas. |
| SPEC 06 regression | PASS | `Logs/spec09-regression-spec06.log`: TownScene e FarmScene passaram. |
| SPEC 07 regression | PASS | `Logs/spec09-regression-spec07.log`: FarmScene passou. |
| Docs validation | PASS | `tools/docs/validate_docs.ps1`. |
| Unity log scanner | FAIL documentado | `ScanUnityLogs.ps1` captura linhas normais contendo `Assembly-CSharp-Editor.dll` e linhas conhecidas de `firstpass.dll`, apesar de `Tundra build success`. |

Observation: o inicializador editor da SPEC 07 ainda registra duplicidade preexistente de `item_crop_wheat` no `ItemDatabase`; nenhuma assercao da SPEC 09 falhou por isso.

## Play Mode Checklist

```text
PLAY MODE TEST: SPEC 09 - Hunger, Stamina, Status e Time Balance
Scene used: Assets/_Game/Scenes/FarmScene.unity, TownScene.unity e CaveScene.unity
Steps executed: NOT RUN
Expected result:
1. Gastar stamina em arar/molhar/plantar/colher, pesca, arvore, craft e ataque; a barra/DebugHud acompanha o valor.
2. Reduzir Hunger para 10-29 e 1-9; observar regeneracao reduzida e movimento a 0.85x no tier critico.
3. Levar Hunger a zero; confirmar regeneracao fixa de stamina em 2/s e dano de HP apenas em ticks, seguido de respawn sem quebrar managers.
4. Consumir item com HungerRestore/StaminaRestore/status; confirmar restauracoes, refresh do mesmo status e duracao visivel.
5. Salvar/carregar e trocar entre Farm/Town/Cave; confirmar hunger, stamina, fase/tempo e status restaurados.
Observed result: NOT RUN
Bugs found: N/A
Passed: NOT RUN
Evidence: Automated batchmode logs listed above.
```

Reason: a execucao automatizada disponivel opera Unity em batchmode e nao conduz input interativo de Play Mode.
Command attempted: `MvpSceneValidator.ValidateSpec09Scenes`, `MvpSceneValidator.ValidateSpec06Scenes` e `MvpSceneValidator.ValidateSpec07Scene`.
Residual risk: input/UX, timing visivel de regeneracao/dano e save/load acionado pelo jogador ainda requerem validacao humana final.
