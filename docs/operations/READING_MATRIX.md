# Reading Matrix Ã¢â‚¬â€ Cindar's Hope

## Objetivo

Definir quais documentos o agente deve ler por tipo de tarefa, evitando leitura excessiva.

| Tipo de tarefa | Ler sempre | Ler se necessÃƒÂ¡rio | NÃƒÂ£o ler por padrÃƒÂ£o |
|---|---|---|---|
| Bug fix simples | `AGENTS.md`/`CLAUDE.md`, topo do `PROJECT_LOG.md`, `docs/IMPLEMENTATION_STATUS.md`, `AGENT_EXECUTION_PROTOCOL.md` | spec implementada da ÃƒÂ¡rea, cÃƒÂ³digo relacionado | `docs_old/**`, GDD completo, crosswalk completo |
| Implementar spec futura | Camada 0, spec alvo, refinement alvo, `docs/specs/SPEC_EXECUTION_ORDER.md` | specs implementadas dependentes, registries afetados | crosswalk completo, docs_old inteiro |
| Completar spec parcial | Camada 0, spec implementada parcial, refinement implementado/futuro relacionado | registry implementado, registry futuro | GDD completo, docs_old inteiro |
| Reorganizar docs | Camada 0, registries, maps, crosswalk, `docs_old/MANIFEST.md` | arquivos especÃƒÂ­ficos de docs_old | cÃƒÂ³digo |
| Validar Unity | Camada 0, docs de validation, spec da ÃƒÂ¡rea | logs antigos, specs dependentes | docs_old inteiro |
| Criar nova spec | Camada 0, source of truth, registry futuro, refinement relacionado | GDD/architecture se a spec tocar design/arquitetura | cÃƒÂ³digo nÃƒÂ£o relacionado |
| Atualizar roadmap/backlog | Camada 0, roadmap/backlog ativo | future ideas, crosswalk | cÃƒÂ³digo |
| Corrigir agente/harness | Camada 0, AGENTS, CLAUDE, protocolo, matriz | source of truth, README | specs de gameplay completas |

## Regra

Se a tarefa indicar explicitamente a spec alvo, nÃƒÂ£o ler todos os registries por padrÃƒÂ£o.

Se a tarefa nÃƒÂ£o indicar a spec alvo, usar registries para localizar a spec correta.

Se houver conflito histÃƒÂ³rico, usar `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` e `docs_old/`.
