# Next Waves Roadmap v1.1 — Delta FASE 9C

> Este arquivo complementa `docs/roadmap/NEXT_WAVES_ROADMAP_v1.0.md` sem apagar o histórico anterior.
> A fonte detalhada da fase é `docs_old/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md`.

## Decisão atualizada

A próxima prioridade passa a ser **FASE 9C — Tools, Farm Actions e Combat Refinement**.

Motivo: antes de UI final e arte final, precisamos estabilizar regras centrais de interação:

- ferramenta ativa;
- níveis de ferramenta;
- plantio sem seleção automática fixa;
- colheita com eficiência por ferramenta;
- árvores exigindo ferramenta correta para progresso real;
- pesca e mineração usando regra genérica de ferramenta;
- equipamento persistido no save;
- combate baseado em equipamento;
- esquiva lateral/para trás;
- suporte a ataques corpo a corpo, projéteis físicos e projéteis mágicos.

## Nova ordem recomendada

| Ordem | Bloco | PRs | Objetivo |
|---:|---|---|---|
| 1 | Tool foundation | PR-100 a PR-102 | contratos, assets e EquipmentManager |
| 2 | Farm action refinement | PR-103 a PR-106 | plantio, colheita, árvore e pesca com ferramenta |
| 3 | Combat equipment foundation | PR-107 a PR-109 | dados de equipamento ofensivo e ataque data-driven |
| 4 | Dodge + projectiles | PR-110 a PR-112 | esquiva, projétil físico e projétil mágico |
| 5 | UI real MVP | após PR-112 | inventário, loja e crafting visual |
| 6 | Conteúdo | depois da UI | diálogo e primeira quest |
| 7 | Visual Slice | depois da quest | integração jogável completa |
| 8 | Fase 10 | depois do slice | arte, áudio, polish e release prep |

## PRs planejados

### PR-100 — Tool contracts
Criar `ToolType`, `ToolTier`, `ToolDataSO`, `ToolDatabaseSO`, `ToolRequirement`, `ToolActionResolver` e eventos de ferramenta.

### PR-101 — Tool assets MVP
Criar dados para Hoe, Sickle, Axe, Pickaxe e FishingRod básicos. Atualizar validator.

### PR-102 — EquipmentManager mínimo
Adicionar ferramenta/equipamento ofensivo ativo, save/load e exibição no HUD debug.

### PR-103 — Plantio com ferramenta
Remover escolha automática fixa de seed e usar seed explícita com regra de Hoe/fallback.

### PR-104 — Colheita com ferramenta
Aplicar Sickle, yield mínimo sem ferramenta e bônus por tier.

### PR-105 — Árvores com ferramenta correta
Sem Axe, permitir somente fallback mínimo sem progresso real. Com Axe, aplicar corte e tier.

### PR-106 — FishingSpot com ToolRequirement
Migrar FishingSpot para o mesmo modelo de ferramenta usado por ações de mundo.

### PR-107 — Contratos de combate por equipamento
Criar dados para ataques corpo a corpo, projéteis físicos e projéteis mágicos.

### PR-108 — Slot ofensivo no EquipmentManager
Persistir item ofensivo equipado e mostrar no HUD debug.

### PR-109 — PlayerCombatController data-driven
Substituir gradualmente o ataque hardcoded por ataque configurado por dados.

### PR-110 — PlayerDodgeController
Adicionar esquiva lateral/para trás com cooldown e janela curta de invulnerabilidade.

### PR-111 — Projétil físico
Adicionar ataque à distância física simples.

### PR-112 — Projétil mágico
Adicionar ataque mágico à distância simples, com custo temporário ou MP futuro.

## Critérios para começar UI/arte depois

- Tool system implementado.
- Plantio, colheita, árvore e pesca respeitando ferramentas.
- Equipamento ofensivo ativo e persistido.
- Esquiva funcionando.
- Pelo menos três estilos de ataque validados.
- Save/load validado com equipamento e cena atual.
- Console sem erro vermelho.

## Decisão final

A FASE 9C vem antes de UI real e arte final porque muda regras centrais de gameplay. UI e arte devem refletir um comportamento já estabilizado.


