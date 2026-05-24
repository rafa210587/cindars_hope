# Agent Execution Protocol Ã¢â‚¬â€ Cindar's Hope

## Objetivo

Definir o fluxo operacional mÃƒÂ­nimo para Codex, Claude Code e outros agentes executarem tarefas no projeto sem ler documentaÃƒÂ§ÃƒÂ£o excessiva.

Este arquivo ÃƒÂ© a leitura operacional principal.
`AGENTS.md` e `CLAUDE.md` apontam para este protocolo.

---

## Camadas de leitura

### Camada 0 Ã¢â‚¬â€ leitura mÃƒÂ­nima obrigatÃƒÂ³ria

Para qualquer tarefa, ler:

1. `AGENTS.md` ou `CLAUDE.md`.
2. `PROJECT_LOG.md` Ã¢â‚¬â€ apenas o topo/entradas recentes.
3. `docs/IMPLEMENTATION_STATUS.md`.
4. Este arquivo: `docs/operations/AGENT_EXECUTION_PROTOCOL.md`.

### Camada 1 Ã¢â‚¬â€ quando a tarefa envolve spec

Ler tambÃƒÂ©m:

1. `docs/specs/SPEC_SOURCE_OF_TRUTH.md`.
2. A spec alvo:
   - `docs/specs/a_implementar/spec_*.md`, ou
   - `docs/specs/implementados/spec_*.md`.
3. O refinement alvo:
   - `docs/refinements/a_implementar/ref_*.md`, ou
   - `docs/refinements/implementados/ref_*.md`.
4. `docs/specs/SPEC_EXECUTION_ORDER.md`, quando a tarefa envolver specs futuras.

### Camada 2 Ã¢â‚¬â€ leitura sob demanda

Ler apenas se houver dÃƒÂºvida, conflito, migraÃƒÂ§ÃƒÂ£o documental ou se a tarefa pedir auditoria:

- `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`
- `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
- `docs/refinements/implementados/ref_implementados_map.md`
- `docs/refinements/a_implementar/ref_futuro_map.md`
- `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md`
- `docs_old/**`
- GDD completo
- arquitetura completa

---

## O que NÃƒÆ’O ler por padrÃƒÂ£o

NÃƒÂ£o ler por padrÃƒÂ£o:

- `docs_old/**`
- crosswalk completo
- registries inteiros
- GDD completo
- arquitetura completa
- todos os refinements
- documentos historicos fora do alvo; usar apenas `docs/specs/` quando a tarefa exigir

SÃƒÂ³ ler esses arquivos quando a tarefa justificar.

---

## Fluxo para bug fix simples

1. Ler Camada 0.
2. Ler spec implementada da ÃƒÂ¡rea se existir.
3. Ler cÃƒÂ³digo diretamente afetado.
4. Corrigir somente o necessÃƒÂ¡rio.
5. Atualizar `PROJECT_LOG.md`.
6. Atualizar `docs/IMPLEMENTATION_STATUS.md` somente se o status da capacidade mudou.
7. Rodar validaÃƒÂ§ÃƒÂ£o relevante.
8. Rodar `tools/docs/validate_docs.ps1` se documentaÃƒÂ§ÃƒÂ£o foi alterada.

---

## Fluxo para implementar spec futura

Antes de implementar:

1. Ler Camada 0.
2. Ler `docs/specs/SPEC_SOURCE_OF_TRUTH.md`.
3. Ler a spec alvo em `docs/specs/a_implementar/spec_*.md`.
4. Ler o refinement alvo em `docs/refinements/a_implementar/ref_*.md`.
5. Conferir `docs/specs/SPEC_EXECUTION_ORDER.md` e dependencias diretas.
6. Ler specs implementadas dependentes diretamente citadas pela spec alvo.
7. NÃƒÂ£o ler `docs_old/**` nem crosswalk completo, exceto se houver conflito histÃƒÂ³rico.

Durante a implementaÃƒÂ§ÃƒÂ£o:

- Implementar somente o escopo da spec/refinement.
- NÃƒÂ£o ampliar escopo sem amendment/correction.
- NÃƒÂ£o misturar vÃƒÂ¡rias specs grandes no mesmo PR.
- NÃƒÂ£o marcar nada como implementado sem evidÃƒÂªncia no repo.

Ao finalizar:

1. Criar ou atualizar `docs/specs/implementados/spec_*.md`.
2. Criar ou atualizar `docs/refinements/implementados/ref_*.md`.
3. Atualizar `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`.
4. Atualizar `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.
5. Atualizar `docs/refinements/implementados/ref_implementados_map.md`.
6. Atualizar `docs/refinements/a_implementar/ref_futuro_map.md`.
7. Atualizar `docs/IMPLEMENTATION_STATUS.md`.
8. Atualizar `PROJECT_LOG.md`.
9. Registrar testes executados e nÃƒÂ£o executados.
10. Rodar `tools/docs/validate_docs.ps1`.

---

## Fluxo para reorganizaÃƒÂ§ÃƒÂ£o documental

1. Ler Camada 0.
2. Ler registries.
3. Ler maps de refinements.
4. Ler crosswalk.
5. Ler `docs_old/MANIFEST.md`.
6. Alterar somente documentaÃƒÂ§ÃƒÂ£o.
7. Atualizar crosswalk se qualquer histÃƒÂ³rico for movido/absorvido/referenciado.
8. Rodar `tools/docs/validate_docs.ps1`.

---

## Fluxo para validaÃƒÂ§ÃƒÂ£o Unity

1. Ler Camada 0.
2. Ler `docs/validation/` relevante.
3. Ler spec implementada da ÃƒÂ¡rea.
4. Executar Play Mode ou validaÃƒÂ§ÃƒÂ£o manual solicitada.
5. Registrar resultado em `PROJECT_LOG.md`.
6. Atualizar `docs/IMPLEMENTATION_STATUS.md` se houver mudanÃƒÂ§a de status.
7. Se houver bug, criar tarefa/branch especÃƒÂ­fica.

---

## Encerramento obrigatÃƒÂ³rio de tarefa

Toda tarefa relevante deve terminar com:

- arquivos alterados;
- resumo do escopo;
- testes executados;
- testes nÃƒÂ£o executados;
- pendÃƒÂªncias;
- atualizaÃƒÂ§ÃƒÂ£o de `PROJECT_LOG.md`;
- atualizaÃƒÂ§ÃƒÂ£o de `docs/IMPLEMENTATION_STATUS.md` quando necessÃƒÂ¡rio.

Se documentaÃƒÂ§ÃƒÂ£o foi alterada, rodar:

```powershell
.\tools\docs\validate_docs.ps1
```

## Regra de economia de contexto

O agente deve evitar carregar documentos grandes se a tarefa puder ser resolvida com:

- Camada 0;
- spec alvo;
- refinement alvo;
- arquivos de cÃƒÂ³digo diretamente relacionados.

Se precisar de documento adicional, justificar no resumo final.

## Regra de fonte unica de specs

A fonte unica oficial de specs e docs/specs/. A pasta raiz specs/ foi removida e nao deve ser recriada.
