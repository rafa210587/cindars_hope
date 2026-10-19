# Refinement - Crafting queue, workstations, recipes e UI

> Status: Implementado completo
> Spec: `.specs/implementados/spec_crafting_queue_workstations_recipes_ui.md`
> Data: 2026-05-24

## Decisoes Consolidadas

- `RecipeDataSO` e o contrato unico de recipe; nenhum modelo paralelo foi recriado.
- Craft de bolso usa exclusivamente recipes `RequiredStationType = None`.
- Workbench, Forge e CookingStation usam IDs estaveis na `FarmScene`.
- O MVP mantem um job ativo ou output completo por workstation.
- Ingredientes sao consumidos apenas apos validacao; instant craft e cancelamento restauram o snapshot do inventory se a operacao nao puder concluir.
- Output completo permanece na workstation quando a coleta nao cabe no inventory.
- Save/load persiste apenas DTOs simples e rebind por IDs.
- O modal minimo com teclado e Canvas funcional e suficiente para esta spec; polish visual continua sob responsabilidade da SPEC 17.

## Evidencia Automatizada

- `ValidateCraftingSystem.ValidateSpec07`: PASS.
- `MvpSceneValidator.ValidateSpec07Scene`: PASS.
- Unity compile: PASS por Tundra build, sem erro C#.
- Docs validation: exigida no fechamento.

## Validacao Humana Pendente

Play Mode interativo permanece `NOT RUN` em batchmode; o roteiro completo esta em `docs/validation/SPEC_07_CRAFTING_VALIDATION_20260524.md`.
