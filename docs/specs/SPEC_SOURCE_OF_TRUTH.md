# Spec Source of Truth - Cindar's Hope

## Fonte unica oficial

A fonte unica de specs do projeto e `docs/specs/`.

| Pasta/arquivo | Funcao |
|---|---|
| `docs/specs/implementados/` | Specs consolidadas do que ja existe no repo, inclusive estados parciais. |
| `docs/specs/a_implementar/` | Specs futuras aprovadas/consolidadas no padrao SpecKit. |
| `docs/specs/SPEC_EXECUTION_ORDER.md` | Ordem oficial de execucao das specs futuras. |
| `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md` | Indice oficial das specs implementadas/parciais. |
| `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` | Indice oficial das specs futuras. |
| `docs/refinements/implementados/` | Refinamentos, PR waves, audits e contexto de capacidades ja implementadas. |
| `docs/refinements/a_implementar/pre_refinamentos/` | Pre-refinamentos vivos que alimentam specs futuras. |
| `docs/refinements/a_implementar/ref_futuro_map.md` | Mapa oficial dos pre-refinamentos futuros. |
| `docs_old/` | Historico integral preservado; nao editar como fonte ativa. |

A pasta raiz `specs/` foi removida e nao deve ser recriada. A pasta raiz `spec/` tambem nao deve ser recriada.

## Regra de leitura

Para planejamento e implementacao, ler somente `docs/specs/`, `docs/refinements/` e os registries/maps ativos necessarios.

## Regra de execucao

Uma spec futura so pode ser implementada se suas dependencias anteriores em `docs/specs/SPEC_EXECUTION_ORDER.md` estiverem reconciliadas e sem pendencia bloqueadora.