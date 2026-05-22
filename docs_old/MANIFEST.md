# Manifesto docs_old - Cindar's Hope

## Fontes historicas principais

- `docs_old/GDD_v2.6.md`
- `docs_old/ARCH_fase4_v2.2.md`
- `docs_old/FASE5_ambiente_v1.2.md`
- `docs_old/FASE6_INDEX_global_v1.2.md`
- `docs_old/FASE6_FARM_backlog_v1.2.md`
- `docs_old/FASE7_SPEC_MVP_FARM_v2.2.md`
- `docs_old/FASE8_EXECUTION_PLAN_CODEX_v1.0.md`
- `docs_old/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`
- `docs_old/SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md`
- `docs_old/IMPLEMENTATION_STATUS.md`
- `docs_old/SPEC_EVOLUTION_POLICY_v1.0.md`
- `docs_old/FASE9A_HANDOFF_FARM_TOWN_SAVE_v1.0.md`
- `docs_old/FASE9B_CAVE_COMBAT_MVP_v1.0.md`
- `docs_old/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md`
- `docs_old/FASE9E_*.md`
- `docs_old/FASE9F_*.md`
- `docs_old/FASE9G_*.md`
- `docs_old/amendments/**`
- `docs_old/audits/**`
- `docs_old/logs/**`
- `docs_old/roadmap/**`

## Fonte auxiliar local preservada

A pasta local nao rastreada `spec/` foi preservada sem alteracao e usada apenas como referencia manual de merge logico nesta reorganizacao. Ela contem specs implementadas e preparadas em formato SpecKit, incluindo blocos de Core, Farm, Save, Town, Cave procedural, stable run replay e boss gates.

## Regra de validacao contra perda

Uma nova spec em `docs/specs/implementados/` so e considerada valida quando contem:

1. referencia as fontes antigas relevantes;
2. referencia ao estado registrado em `PROJECT_LOG.md` ou `docs_old/IMPLEMENTATION_STATUS.md`;
3. evidencia de codigo, quando a capacidade ja existe;
4. pendencias explicitas, se a implementacao for parcial;
5. distincao entre MVP/debug e comportamento final pretendido.

## Preservacao adicional

- docs_old/README_LEGACY.md preserva o antigo docs/README.md movido por git mv.
