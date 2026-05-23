# DOCS OLD → ACTIVE CROSSWALK

> Status: mapa de preservação documental.
> Objetivo: garantir que docs_old/ continue preservado e que a documentação ativa tenha referência explícita ao que foi migrado, absorvido ou mantido como histórico.

## Legenda

| Status | Significado |
|---|---|
| Ativo copiado | Arquivo antigo foi copiado para uma pasta ativa em docs/. |
| Absorvido em spec implementada | Conteúdo antigo virou spec em docs/specs/implementados/. |
| Absorvido em spec futura | Conteúdo antigo virou spec em docs/specs/a_implementar/. |
| Absorvido em refinement implementado | Conteúdo antigo virou refinement em docs/refinements/implementados/. |
| Absorvido em refinement futuro | Conteúdo antigo virou refinement em docs/refinements/a_implementar/. |
| Histórico preservado | Mantido apenas em docs_old/, com referência ativa quando necessário. |

## Crosswalk

| Arquivo em docs_old | Destino ativo | Tipo de migração | Status | Observação |
|---|---|---|---|---|
| $old | $dest | cópia ativa | Ativo copiado | Design principal do jogo. |
| $old | $dest | cópia ativa | Ativo copiado | Delta de design FASE9C. |
| $old | $dest | cópia ativa | Ativo copiado | Changelog de atualização de design. |
| $old | $dest | cópia ativa + absorção | Ativo copiado | Arquitetura base e core specs. |
| $old | $dest | cópia ativa | Ativo copiado | Delta arquitetural. |
| $old | $dest | cópia ativa + absorção | Ativo copiado | Contratos core, eventos, IDs e save. |
| $old | $dest | cópia ativa | Ativo copiado | Delta FASE9C dos contratos core. |
| $old | $dest | cópia ativa | Ativo copiado | Setup de ambiente. |
| $old | $dest | cópia ativa | Ativo copiado | Instruções de handoff LLM. |
| $old | $dest | cópia ativa | Ativo copiado | Controle de drift SpecKit. |
| $old | $dest | cópia ativa | Ativo copiado | Política de evolução de specs. |
| $old | $dest | cópia ativa | Ativo copiado | Pipeline de sprites. |
| $old | $dest | referência histórica | Histórico preservado | Mantido no histórico; consultar quando houver dúvida sobre correções pré-Codex. |
| $old | $dest | cópia ativa | Ativo copiado | Validação estrutural FASE9F. |
| $old | $dest | cópia ativa | Ativo copiado | Backlog global base. |
| $old | $dest | cópia ativa | Ativo copiado | Delta FASE9C do backlog global. |
| $old | $dest | cópia ativa | Ativo copiado | Backlog FARM. |
| $old | $dest | cópia ativa | Ativo copiado | Roadmap ativo. |
| $old | $dest | cópia ativa | Ativo copiado | Roadmap delta FASE9C. |
| $old | $dest | cópia ativa + absorção | Ativo copiado | Ideias futuras preservadas sem compromisso imediato de implementação. |
| $old | $dest | cópia ativa | Ativo copiado | Roadmap stable run/replay. |
| $old | $dest | absorção | Absorvido em spec implementada | MVP Farm normalizado em specs implementadas. |
| $old | $dest | absorção | Absorvido em refinement implementado | Plano de execução preservado por waves e validação. |
| $old | $dest | absorção | Absorvido em spec implementada | Town, crafting, economy e save. |
| $old | $dest | absorção | Absorvido em spec implementada | Cave/combat MVP. |
| $old | $dest | absorção | Absorvido em spec implementada | Enemy stats data-driven. |
| $old | $dest | referência ativa | Histórico preservado | Usado como contexto de transição documental. |
| $old | $dest | absorção | Absorvido em spec futura | Restante de equipment/items/combat. |
| $old | $dest | absorção mista | Absorvido em spec futura | Parte implementada e restante futuro separados. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9D enemy actions/AI. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9D 40 monsters. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9E UI final. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9E damage/status. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9E item taxonomy. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9E item examples. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9E save migration. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9E progression. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9F cave resources/encounters. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9G bestiary/faction locks. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9H loot/crafting/equipment. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9I player combat. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9J cave entry/death flow. |
| $old | $dest | absorção | Absorvido em spec futura | FASE9K skill trees. |
| $old | $dest | cópia ativa | Ativo copiado | Amendment stable run/replay. |
| $old | $dest | cópia ativa + absorção | Ativo copiado | Amendment enemy combat roles/AI/status. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | absorção | Absorvido em refinement implementado | Audit/handoff preservado na camada ativa de refinements. |
| $old | $dest | cópia ativa | Ativo copiado | Smoke test Farm/Town/Cave. |
| $old | $dest | cópia ativa | Ativo copiado | Smoke visual cave procedural. |
| $old | $dest | cópia ativa | Ativo copiado | Validação imports/estrutura FASE9F. |
| $old | $dest | referência histórica | Histórico preservado | Não copiado para docs ativo para evitar duplicidade de log; referenciado por PROJECT_LOG.md e docs_old/MANIFEST.md. |
