
# Cindar's Hope — Changelog de Atualização Documental v2.6

> Data: 2026-05-16
> Objetivo: enriquecer stories/specs para execução com Codex, estabilizar contratos técnicos e detalhar pipeline de sprites com IA + Aseprite.

## Arquivos atualizados

| Arquivo novo | Origem | Alteração principal |
|---|---|---|
| `GDD_v2.6.md` | `GDD_v2.5.md` | Fonte de verdade atualizada, decisão Codex, decisão sprites IA, escopo rígido do MVP |
| `ARCH_fase4_v2.2.md` | `ARCH_fase4_v2.1.md` | Contratos core, registries, save path, execução com Codex, pipeline IA→Aseprite |
| `FASE5_ambiente_v1.2.md` | `FASE5_ambiente_v1.1.md` | Setup micro de Codex, ferramentas de sprite/IA, smoke test expandido |
| `FASE6_INDEX_global_v1.2.md` | `FASE6_INDEX_global_v1.1.md` | Plano de PRs pequenos para Fase 8 e Definition of Ready/Done |
| `FASE6_FARM_backlog_v1.2.md` | `FASE6_FARM_backlog_v1.1.md` | Enriquecimento das stories com critérios de rastreabilidade, save e eventos |
| `FASE7_SPEC_MVP_FARM_v2.2.md` | `FASE7_SPEC_MVP_FARM_v2.1.md` | Notas para Codex, save path correto, registry por IDs, remoção de busca global runtime |
| `CLAUDE_v1.2.md` | `CLAUDE_v1.1.md` | Regras adicionais para Codex e arte com IA |
| `AGENTS.md` | `AGENTS.md` | Espelhado com regras adicionais para agentes genéricos |

## Arquivos novos

| Arquivo | Objetivo |
|---|---|
| `FASE8_EXECUTION_PLAN_CODEX_v1.0.md` | Plano operacional completo para implementar o MVP com Codex por PRs pequenos |
| `CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md` | Contratos técnicos para eventos, IDs, registries, save path, schema e load sequence |
| `SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md` | Pipeline completo de sprites com ChatGPT/DALL-E, PixelLab/Scenario opcionais e Aseprite final |

## Correções técnicas importantes

- Save editável deve usar `Application.persistentDataPath`, não `StreamingAssets`.
- Save serializa IDs e valores simples, não referências Unity.
- `FindObjectsByType` em runtime foi removido como prática recomendada para o FarmSystem.
- Fase 8 agora tem sequência de PRs pequenos e testáveis.
- IA de arte foi reposicionada como assistente de conceito/rascunho; Aseprite permanece como etapa final.

## Próximo passo recomendado

1. Copiar estes documentos para `/docs` do repositório.
2. Abrir Unity e passar no smoke test expandido da Fase 5.
3. Criar branch `feature/fase8-pr-001-core-foundation`.
4. Rodar o primeiro prompt do `FASE8_EXECUTION_PLAN_CODEX_v1.0.md` no Codex.
