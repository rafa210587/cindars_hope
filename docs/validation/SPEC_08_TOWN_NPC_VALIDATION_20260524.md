# SPEC 08 - Town NPC Dialogue Validation - 2026-05-24

## Automated Validation

```text
Unity compile: PASS - `*** Tundra build success` in `Logs/spec08-scene-validation-final.log`.
TownScene generation: PASS - `MVP TownScene created at Assets/_Game/Scenes/TownScene.unity` in `Logs/spec08-scene-generation.log`.
SPEC 08 scene/data validator: PASS - `SPEC 08 scene and NPC dialogue validation passed for TownScene.` in `Logs/spec08-scene-validation-final.log`.
SPEC 06 regression validator: PASS - `SPEC 06 scene validation passed for TownScene and FarmScene.` in `Logs/spec08-regression-spec06.log`.
SPEC 07 regression validator: PASS - `SPEC 07 scene validation passed for FarmScene.` in `Logs/spec08-regression-spec07.log`.
Docs validation: PASS - `tools/docs/validate_docs.ps1`.
Unity log scanner: FAIL - `ScanUnityLogs.ps1` classifica as duas linhas conhecidas de assemblies `firstpass` como criticas, apesar de `Tundra build success`.
Observation: Unity logged duplicate `item_crop_wheat` in `ItemDatabase` from the SPEC 07 initializer during editor initialization; no SPEC 08 assertion failed.
```

## Play Mode Test

```text
PLAY MODE TEST: SPEC 08 - Town NPC dialogue, schedule e quests
Scene: TownScene
Steps: Interagir com Pip, os dois lojistas e o wanderer; salvar/carregar; tentar abrir modais concorrentes.
Expected: Pip exibe escolhas sem loja; lojistas abrem lojas corretas; wanderer anda em limites seguros e alterna lore; estado simples restaura; modais sao exclusivos.
Observed: NOT RUN - somente validacoes batchmode foram executadas.
Passed: NOT RUN
```

## Validacao Manual Restante

1. Executar Play Mode conforme checklist acima.
2. Confirmar save/load interativo do estado do wanderer e `HasMet`.
