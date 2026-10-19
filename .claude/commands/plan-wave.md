# /plan-wave

Planeja a próxima FASE ou wave de desenvolvimento. Único command autorizado a ler ROADMAP.md por padrão.

**Arguments:** `$ARGUMENTS` — opcional: nome da wave ou número da FASE (ex.: `FASE10` ou `post-mvp`)

---

## Objetivo

Produzir um documento de planejamento para a próxima wave de desenvolvimento. Sem implementação. Sem execução de spec.

---

## Leitura mínima

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md` — fila ativa e blockers
3. `.specs/SPEC_GENERATION_ROADMAP_MASTER.md` — macro roadmap de todas as specs planejadas e dependências
4. `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` — execution phases, paralelização, regras de batch size
5. `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` — validation requirements a planejar
6. `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md` — lista de specs a planejar e dependências
7. `docs/backlog/current_backlog.md` — itens de backlog operacional

## Leitura opcional

- `docs/backlog/post_mvp_backlog.md`
- `docs/project/DOCUMENT_INDEX.md`
- Architecture docs relevantes para o domínio da wave

## Não ler por padrão

```
PROJECT_LOG.md
IMPLEMENTATION_STATUS.md (full)
All validation reports
docs_old/**
```

---

## Procedimento

1. Leia o CURRENT_STATE.md para entender itens bloqueantes e a fila atual
2. Leia o SPEC_GENERATION_ROADMAP_MASTER.md — use como fonte primária de waves/specs
3. Leia o SPEC_WAVE_EXECUTION_PROTOCOL.md para:
   - Regras de paralelização (quais lanes podem rodar em paralelo)
   - Timing de lock/checkpoint entre waves
   - Restrições de batch size e capacidade de execução
4. Leia o SPEC_VALIDATION_MATRIX_MASTER.md para entender os custos de validação (tempo, recursos)
5. Leia o SPEC_REGISTRY_TO_IMPLEMENT.md para cruzar com a lista de specs
6. Produza o wave plan:
   - Objetivos da wave
   - Specs a criar (IDs, títulos, esforço aproximado, dependências)
   - Dependency graph entre specs e waves propostas
   - Specs que precisam ser concluídas primeiro (a partir do CURRENT_STATE.md)
   - Plano de paralelização (quais lanes, checkpoints)
   - Plano de validação (tipos e timing por mudança)
   - Riscos e questões em aberto
7. Produza o documento de planejamento
8. NÃO crie specs nem implemente código — apenas planeje.

---

## Edições permitidas

Opcional: criar uma atualização de `docs/project/ROADMAP.md` ou um documento de wave planning em `docs/backlog/`.

## Edições proibidas

- Nenhuma implementação de spec
- Nenhuma movimentação de spec
- Nenhuma mudança de código
- Nenhuma operação de delete

---

## Saída esperada

```markdown
# Wave Plan — <FASE/Wave Name>

## Prerequisite (must complete first)
- <item from CURRENT_STATE.md>

## Wave Objectives
- <objective>

## Proposed Specs

| Spec ID | Title | Effort | Depends On |
|---------|-------|--------|-----------|
| SPEC_XX | <title> | Xh | <dep> |

## Risks and Open Questions
- <risk>

## Recommended Execution Order
1. <spec>
2. <spec>
```
