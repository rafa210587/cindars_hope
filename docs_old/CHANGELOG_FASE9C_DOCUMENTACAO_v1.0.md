# Changelog Documental — FASE 9C Tools/Farm/Combat v1.0

> Data: 2026-05-18
> Tipo: atualização documental e planejamento de próxima fase.

## Arquivos criados

| Arquivo | Objetivo |
|---|---|
| `docs/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md` | Spec principal da FASE 9C |
| `docs/NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md` | Delta do roadmap pós 9B-1 |
| `docs/ARCH_fase4_v2.3_FASE9C_DELTA.md` | Delta arquitetural com Tool/Weapon/Equipment/Dodge |
| `docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md` | Delta de eventos, save e IDs |
| `docs/GDD_v2.7_FASE9C_DELTA.md` | Delta de design do jogo |
| `docs/FASE6_INDEX_global_v1.3_FASE9C_DELTA.md` | Delta do índice global de épicos |

## Arquivos atualizados

| Arquivo | Atualização |
|---|---|
| `README.md` | Próxima fase e roadmap atualizados para FASE 9C |
| `AGENTS.md` | Regras específicas para Tools/Farm/Combat |
| `CLAUDE.md` | Regras específicas para Tools/Farm/Combat |

## Decisões registradas

1. FASE 9C vem antes de UI final e arte final.
2. Ferramentas passam a ser progressão central.
3. Ações de fazenda devem depender de ferramenta, tier e fallback controlado.
4. Combate deve migrar para armas equipáveis.
5. Esquiva lateral/para trás entra como próximo refinamento de game feel.
6. Armas precisam suportar corpo a corpo, distância física e magia à distância.

## Próximo passo recomendado

Iniciar PR-100 — Tool contracts.

Escopo do PR-100:

- `ToolType`
- `ToolTier`
- `ToolDataSO`
- `ToolDatabaseSO`
- `ToolRequirement`
- `ToolActionResolver`
- eventos mínimos de ferramenta

Fora de escopo do PR-100:

- alterar FarmPlot;
- alterar TreeNode;
- alterar FishingSpot;
- alterar combate;
- criar UI;
- criar arte final;
- alterar cenas.
