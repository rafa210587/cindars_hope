# Agent Execution Protocol — Cindar's Hope

## Objetivo

Definir o fluxo operacional mínimo para Codex, Claude Code e outros agentes executarem tarefas no projeto sem ler documentação excessiva.

Este arquivo é a leitura operacional principal.  
`AGENTS.md` e `CLAUDE.md` apontam para este protocolo.

---

## Camadas de leitura

### Camada 0 — leitura mínima obrigatória

Para qualquer tarefa, ler:

1. `AGENTS.md` ou `CLAUDE.md`.
2. `PROJECT_LOG.md` — apenas o topo/entradas recentes.
3. `docs/IMPLEMENTATION_STATUS.md`.
4. Este arquivo: `docs/operations/AGENT_EXECUTION_PROTOCOL.md`.

### Camada 1 — quando a tarefa envolve spec

Ler também:

1. `docs/specs/SPEC_SOURCE_OF_TRUTH.md`.
2. A spec alvo:
   - `docs/specs/a_implementar/spec_*.md`, ou
   - `docs/specs/implementados/spec_*.md`.
3. O refinement alvo:
   - `docs/refinements/a_implementar/ref_*.md`, ou
   - `docs/refinements/implementados/ref_*.md`.
4. `specs/<FEATURE>/`, se existir.

### Camada 2 — leitura sob demanda

Ler apenas se houver dúvida, conflito, migração documental ou se a tarefa pedir auditoria:

- `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`
- `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
- `docs/refinements/implementados/ref_implementados_map.md`
- `docs/refinements/a_implementar/ref_futuro_map.md`
- `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md`
- `docs_old/**`
- GDD completo
- arquitetura completa

---

## O que NÃO ler por padrão

Não ler por padrão:

- `docs_old/**`
- crosswalk completo
- registries inteiros
- GDD completo
- arquitetura completa
- todos os refinements
- todos os arquivos de `specs/`

Só ler esses arquivos quando a tarefa justificar.

---

## Fluxo para bug fix simples

1. Ler Camada 0.
2. Ler spec implementada da área se existir.
3. Ler código diretamente afetado.
4. Corrigir somente o necessário.
5. Atualizar `PROJECT_LOG.md`.
6. Atualizar `docs/IMPLEMENTATION_STATUS.md` somente se o status da capacidade mudou.
7. Rodar validação relevante.
8. Rodar `tools/docs/validate_docs.ps1` se documentação foi alterada.

---

## Fluxo para implementar spec futura

Antes de implementar:

1. Ler Camada 0.
2. Ler `docs/specs/SPEC_SOURCE_OF_TRUTH.md`.
3. Ler a spec alvo em `docs/specs/a_implementar/spec_*.md`.
4. Ler o refinement alvo em `docs/refinements/a_implementar/ref_*.md`.
5. Ler `specs/<FEATURE>/`, se existir.
6. Ler specs implementadas dependentes diretamente citadas pela spec alvo.
7. Não ler `docs_old/**` nem crosswalk completo, exceto se houver conflito histórico.

Durante a implementação:

- Implementar somente o escopo da spec/refinement.
- Não ampliar escopo sem amendment/correction.
- Não misturar várias specs grandes no mesmo PR.
- Não marcar nada como implementado sem evidência no repo.

Ao finalizar:

1. Criar ou atualizar `docs/specs/implementados/spec_*.md`.
2. Criar ou atualizar `docs/refinements/implementados/ref_*.md`.
3. Atualizar `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`.
4. Atualizar `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.
5. Atualizar `docs/refinements/implementados/ref_implementados_map.md`.
6. Atualizar `docs/refinements/a_implementar/ref_futuro_map.md`.
7. Atualizar `docs/IMPLEMENTATION_STATUS.md`.
8. Atualizar `PROJECT_LOG.md`.
9. Registrar testes executados e não executados.
10. Rodar `tools/docs/validate_docs.ps1`.

---

## Fluxo para reorganização documental

1. Ler Camada 0.
2. Ler registries.
3. Ler maps de refinements.
4. Ler crosswalk.
5. Ler `docs_old/MANIFEST.md`.
6. Alterar somente documentação.
7. Atualizar crosswalk se qualquer histórico for movido/absorvido/referenciado.
8. Rodar `tools/docs/validate_docs.ps1`.

---

## Fluxo para validação Unity

1. Ler Camada 0.
2. Ler `docs/validation/` relevante.
3. Ler spec implementada da área.
4. Executar Play Mode ou validação manual solicitada.
5. Registrar resultado em `PROJECT_LOG.md`.
6. Atualizar `docs/IMPLEMENTATION_STATUS.md` se houver mudança de status.
7. Se houver bug, criar tarefa/branch específica.

---

## Encerramento obrigatório de tarefa

Toda tarefa relevante deve terminar com:

- arquivos alterados;
- resumo do escopo;
- testes executados;
- testes não executados;
- pendências;
- atualização de `PROJECT_LOG.md`;
- atualização de `docs/IMPLEMENTATION_STATUS.md` quando necessário.

Se documentação foi alterada, rodar:

```powershell
.\tools\docs\validate_docs.ps1
```

## Regra de economia de contexto

O agente deve evitar carregar documentos grandes se a tarefa puder ser resolvida com:

- Camada 0;
- spec alvo;
- refinement alvo;
- arquivos de código diretamente relacionados.

Se precisar de documento adicional, justificar no resumo final.
