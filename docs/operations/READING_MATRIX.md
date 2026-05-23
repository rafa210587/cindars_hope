# Reading Matrix — Cindar's Hope

## Objetivo

Definir quais documentos o agente deve ler por tipo de tarefa, evitando leitura excessiva.

| Tipo de tarefa | Ler sempre | Ler se necessário | Não ler por padrão |
|---|---|---|---|
| Bug fix simples | `AGENTS.md`/`CLAUDE.md`, topo do `PROJECT_LOG.md`, `docs/IMPLEMENTATION_STATUS.md`, `AGENT_EXECUTION_PROTOCOL.md` | spec implementada da área, código relacionado | `docs_old/**`, GDD completo, crosswalk completo |
| Implementar spec futura | Camada 0, spec alvo, refinement alvo, `specs/<FEATURE>/` | specs implementadas dependentes, registries afetados | crosswalk completo, docs_old inteiro |
| Completar spec parcial | Camada 0, spec implementada parcial, refinement implementado/futuro relacionado | registry implementado, registry futuro | GDD completo, docs_old inteiro |
| Reorganizar docs | Camada 0, registries, maps, crosswalk, `docs_old/MANIFEST.md` | arquivos específicos de docs_old | código |
| Validar Unity | Camada 0, docs de validation, spec da área | logs antigos, specs dependentes | docs_old inteiro |
| Criar nova spec | Camada 0, source of truth, registry futuro, refinement relacionado | GDD/architecture se a spec tocar design/arquitetura | código não relacionado |
| Atualizar roadmap/backlog | Camada 0, roadmap/backlog ativo | future ideas, crosswalk | código |
| Corrigir agente/harness | Camada 0, AGENTS, CLAUDE, protocolo, matriz | source of truth, README | specs de gameplay completas |

## Regra

Se a tarefa indicar explicitamente a spec alvo, não ler todos os registries por padrão.

Se a tarefa não indicar a spec alvo, usar registries para localizar a spec correta.

Se houver conflito histórico, usar `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` e `docs_old/`.
