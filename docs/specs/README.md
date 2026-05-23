# Specs — Cindar's Hope

Ver também: `docs/specs/SPEC_SOURCE_OF_TRUTH.md`.

## Estrutura

- `docs/specs/implementados/`: specs consolidadas do que já existe no repo.
- `docs/specs/a_implementar/`: specs futuras ou preparadas para implementação.
- `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`: registry oficial das specs implementadas/parciais.
- `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`: registry oficial das specs futuras.
- `docs/refinements/implementados/`: refinamentos e PR waves já absorvidos.
- `docs/refinements/a_implementar/`: refinamentos futuros ainda não implementados.
- `specs/`: SpecKit operacional por feature.
- `docs_old/`: histórico integral preservado.

## Regras de nomes

- Toda spec implementada deve ter prefixo `spec_`.
- Toda spec futura deve ter prefixo `spec_`, exceto `README.md`.
- Todo refinement implementado deve ter prefixo `ref_`.
- Todo refinement futuro deve ter prefixo `ref_`.
- A pasta raiz `spec/` foi absorvida e não deve ser recriada.

## Como implementar uma spec

Antes de implementar:

1. Ler `docs/specs/SPEC_SOURCE_OF_TRUTH.md`.
2. Ler `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`.
3. Ler `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.
4. Ler a spec futura em `docs/specs/a_implementar/spec_*.md`.
5. Ler o refinement futuro em `docs/refinements/a_implementar/ref_*.md`.
6. Ler `specs/<FEATURE>/`, se existir.
7. Validar dependências em `docs/specs/implementados/`.

Ao finalizar:

1. Criar/atualizar `docs/specs/implementados/spec_*.md`.
2. Criar/atualizar `docs/refinements/implementados/ref_*.md`.
3. Atualizar os dois registries de specs.
4. Atualizar os dois maps de refinements.
5. Atualizar `docs/IMPLEMENTATION_STATUS.md`.
6. Atualizar `PROJECT_LOG.md`.

Specs antigas continuam preservadas em `docs_old/` e rastreadas em `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md`.
