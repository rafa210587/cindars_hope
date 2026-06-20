# Rule: Política de Leitura de Contexto

## Regra

Agents executando uma spec devem ler apenas: CLAUDE.md (ou AGENTS.md), `docs/project/CURRENT_STATE.md`, a spec ativa, e arquivos explicitamente citados pela spec. Não leia PROJECT_LOG.md, ROADMAP.md, GDD completo, refinements antigos, archived specs, ou validation reports não relacionados por padrão.

## Por que existe

Contexto default pesado (PROJECT_LOG.md ~500+ linhas, IMPLEMENTATION_STATUS.md completo) aumenta o custo em tokens e o risco de drift por dados históricos desatualizados. CURRENT_STATE.md (~80 linhas) contém toda a informação operacionalmente relevante para uma tarefa de execução.

## Onde se aplica

Toda execução de spec, bug fix e tarefa de documentação rodada por agent.

## Exemplos de violação

- Ler PROJECT_LOG.md como "Camada 0" antes de uma tarefa de implementação de código
- Ler todo o IMPLEMENTATION_STATUS.md para achar o status da spec atual
- Ler todo o SPEC_EXECUTION_ORDER.md quando CURRENT_STATE.md já lista a fila de specs ativas

## Exceções permitidas

- **Tarefas de audit/reconciliation** (`/reconcile-status`): podem ler PROJECT_LOG.md e IMPLEMENTATION_STATUS.md
- **Investigação de regressão**: pode ler validation reports anteriores
- **Wave planning** (`/plan-wave`): pode ler ROADMAP.md e current_backlog.md
- **Pedido humano explícito**: "read PROJECT_LOG for context"

## O que fazer se a exceção for necessária

Declare a justificativa antes de ler o arquivo pesado. Exemplo: "Reading PROJECT_LOG.md for audit of SPEC_18-24 evidence."

## Validação

O hook `.claude/hooks/context-policy-check.ps1` (disabled by default) consegue detectar quando planos de execução incluem leituras históricas pesadas sem justificativa.
