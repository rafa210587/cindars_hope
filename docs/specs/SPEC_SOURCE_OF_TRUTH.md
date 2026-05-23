# Spec Source of Truth — Cindar's Hope

## Fontes oficiais

| Pasta/arquivo | Função |
|---|---|
| `docs/specs/implementados/` | Specs consolidadas do que já existe no repo. |
| `docs/specs/a_implementar/` | Specs futuras ou preparadas para implementação. |
| `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md` | Índice oficial das specs implementadas/parciais. |
| `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` | Índice oficial das specs futuras. |
| `docs/refinements/implementados/` | Refinamentos, PR waves, audits e contexto de capacidades já implementadas. |
| `docs/refinements/a_implementar/` | Refinamentos futuros ainda não implementados. |
| `docs/refinements/implementados/ref_implementados_map.md` | Mapa oficial de refinamentos implementados. |
| `docs/refinements/a_implementar/ref_futuro_map.md` | Mapa oficial de refinamentos futuros. |
| `specs/` | SpecKit operacional por feature, usado por agentes para execução. |
| `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` | Rastreabilidade entre histórico antigo e documentação ativa. |
| `docs_old/` | Histórico integral preservado. |

## Regra de leitura

Para planejamento humano, usar `docs/specs/`, `docs/refinements/` e os registries.

Para execução SpecKit, usar `specs/<FEATURE>/` junto com a spec/refinement ativa correspondente.

Para auditoria histórica, usar `docs_old/` e `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md`.

A pasta `spec/` da raiz não é fonte oficial e não deve ser recriada.

## Fluxo obrigatório de uma spec futura para implementada

Quando uma spec de `docs/specs/a_implementar/` for implementada:

1. Criar ou atualizar uma spec em `docs/specs/implementados/spec_*.md` com o estado real implementado.
2. Criar ou atualizar um refinement em `docs/refinements/implementados/ref_*.md` preservando decisões, escopo executado, audits, handoffs e pendências.
3. Preservar o conteúdo da spec futura e do refinement futuro no implementado antes de remover ou marcar como substituído.
4. Atualizar `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`.
5. Atualizar `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.
6. Atualizar `docs/refinements/implementados/ref_implementados_map.md`.
7. Atualizar `docs/refinements/a_implementar/ref_futuro_map.md`.
8. Atualizar `docs/IMPLEMENTATION_STATUS.md`.
9. Atualizar `PROJECT_LOG.md`.
10. Atualizar `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` se houver nova migração documental.

Não marcar uma spec como implementada sem evidência no repo.

## Regra de mudança de escopo

Se a implementação precisar mudar o escopo aprovado:

- não alterar silenciosamente a spec antiga;
- criar amendment, correction ou errata conforme `docs/operations/SPEC_EVOLUTION_POLICY_v1.0.md`;
- registrar o motivo no `PROJECT_LOG.md`;
- atualizar registries e maps afetados.

## Regra de preservação

`docs_old/` não deve ser apagado ou reduzido.

Quando houver dúvida entre apagar ou preservar, preservar como `ref_*.md`, linkar no registry ou registrar no crosswalk.
