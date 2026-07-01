---
name: delegated-execution
description: "Como o loop principal (Opus) delega trabalho de execução a um subagent Sonnet **e verifica o resultado**, evitando os modos de falha já observados (narrar sem fazer; reportar sucesso omitindo desvios; dropar sub-itens em prompts longos). Complementa a rule `subagent-results-not-evidence` e o rout..."
---

# Skill: Delegated Execution (delegar execução a subagent e verificar)

Como o loop principal (Opus) delega trabalho de execução a um subagent Sonnet **e verifica o resultado**, evitando os modos de falha já observados (narrar sem fazer; reportar sucesso omitindo desvios; dropar sub-itens em prompts longos). Complementa a rule `subagent-results-not-evidence` e o routing de `feedback_execution_routing`.

## Quando usar

- Sempre que delegar implementação/edição de código, asset wiring, relayout de cena, migração ou bugfix mecânico a um subagent (`spec-implementer`, `general-purpose` Sonnet, etc.).

## Quando NÃO usar

- Tarefas que ficam no loop principal (responder, ler 1–2 arquivos, decidir design). Não delegue o que é debate/decisão.
- Buscas read-only puras (use `Explore`/haiku direto, sem este cerimonial de verificação de build).

## Prompt de delegação — exigir SEMPRE

1. **Sem delegar em cascata:** "NÃO use a ferramenta Agent/Task nem spawne sub-agentes — faça você mesmo." (Subagent em background que spawna filhos tende a retornar narração do pai antes do filho terminar.)
2. **Escopo + critério de pronto explícitos:** arquivos permitidos/proibidos, e a lista de aceite/anti-regressão da spec (especialmente **remoções** e **requisitos novos do usuário** — são os mais esquecidos).
3. **Provar com builds:** "rode `dotnet build` runtime+editor, exit 0 obrigatório; conserte até passar; nunca deixe o repo quebrado." (PowerShell, sem pipe filtrado.)
4. **Retorno = evidência, não narração:** "retorne a lista exata de arquivos criados/editados, os exit codes dos builds, e o que ficou de fora. Se não fez X, diga explicitamente — não narre intenção."
5. **Prompt grande → numere fases com build após cada uma**, para o agente não dropar sub-itens.

## Checklist do orquestrador ao receber o resultado (obrigatório)

- [ ] `Glob`/`Grep`: cada arquivo prometido existe e contém o esperado.
- [ ] Re-rodar `dotnet build` runtime+editor você mesmo → exit 0 (não confiar no exit relatado).
- [ ] Conferir contra os critérios de aceite + anti-regressão da spec (remoções feitas? requisito novo do usuário atendido? valores alinhados a uma única fonte?).
- [ ] Se divergir do relatado: corrigir (você ou novo subagent focado) — não propagar o claim.

## Anti-padrões observados (não repetir)

- Aceitar "BUILD_VALIDATED" sem verificar (build passa mesmo com desvios que não quebram compile).
- Spec com o mesmo valor (coords/balance) em duas seções → agente segue a errada. Defina cada valor **uma vez**; outras seções referenciam.
- Spec longa com remoções/adições enterradas no meio → agente completa o "grosso" e dropa o item pequeno.
