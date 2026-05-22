# FASE 9C — Handoff documental v1.0

## Escopo

Atualização documental para planejar a próxima fase: Tools, Farm Actions e Combat Refinement.

## Arquivos criados

- `docs/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md`
- `docs/NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md`
- `docs/ARCH_fase4_v2.3_FASE9C_DELTA.md`
- `docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md`
- `docs/GDD_v2.7_FASE9C_DELTA.md`
- `docs/FASE6_INDEX_global_v1.3_FASE9C_DELTA.md`
- `docs/CHANGELOG_FASE9C_DOCUMENTACAO_v1.0.md`

## Arquivos atualizados

- `README.md`
- `AGENTS.md`
- `CLAUDE.md`

## Decisão

A FASE 9C vem antes de UI final e arte final.

A próxima implementação recomendada é PR-100 — Tool contracts.

## Próximo PR

PR-100 deve criar apenas contratos:

- `ToolType`
- `ToolTier`
- `ToolDataSO`
- `ToolDatabaseSO`
- `ToolRequirement`
- `ToolActionResolver`
- eventos mínimos de ferramenta

Fora de escopo do PR-100:

- FarmPlot
- TreeNode
- FishingSpot
- combate
- UI
- cenas
- arte final
