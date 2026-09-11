# Fluxo de refinamento e execução por specs

A ordem proposta faz sentido: **refinamento → spec → plan → tasks**. Antes do código, revisar
consistência; depois, implementar, validar e fechar com evidência. É uma adaptação do
[GitHub Spec Kit](https://github.com/github/spec-kit), mantendo as regras e pastas deste projeto.

| Etapa | Pergunta que resolve | Skill |
|---|---|---|
| Refinamento | Qual problema existe, por que mudar e que escolhas fazem sentido no mundo e nos loops? | `refinement-authoring` |
| Spec | Qual comportamento entregar e como reconhecer que está correto? | `spec-authoring` |
| Plan | Quais contratos, arquivos, integrações e testes concretizam esse comportamento? | `spec-planning` |
| Tasks | Qual a ordem, quais arquivos pertencem a cada executor e qual critério cada tarefa prova? | `spec-task-authoring` |
| Revisão | Existem contradições, critérios sem cobertura ou decisões abertas? | `spec-task-authoring` e revisão aplicável |
| Execução/fechamento | O resultado cumpre a spec com evidência atual? | `spec-execution`, `/validate-spec`, `/finish-spec` |

As três novas skills complementam `spec-authoring`; não substituem o harness inteiro.
`plan-wave` continua escolhendo o conjunto e a ordem macro das specs.
As regras de AGENTS, CURRENT_STATE e decisões canônicas já exercem o papel de princípios do projeto.
Nenhum Spec Kit foi instalado e não foi criada uma constituição concorrente.

Por padrão, Spec, Plan e Tasks são seções do mesmo arquivo, com uma revisão comum.
Uma mudança de requisito atualiza primeiro a spec e depois seus planos e tarefas afetados.
Uma mudança técnica atualiza o plan e as tasks correspondentes. Specs antigas completas continuam válidas.
Rascunho e aprovação de implementação são estados diferentes.

Exemplos de pedidos:

- “Use refinement-authoring para revisar esta mecânica com evidência do código e equilíbrio.”
- “Transforme o refinamento em spec, depois aplique spec-planning e spec-task-authoring.”
- “Revise a consistência deste pacote sem implementar.”

Primeira aplicação: [lote de habilidades de Vaalara](../../.specs/features_futuras/skills_sdd_v1/README.md).
Fonte técnica do contrato: [SDD workflow](../../.claude/skills/spec-authoring/references/sdd-workflow.md).
