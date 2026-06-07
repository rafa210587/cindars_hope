# Specs – Cindar's Hope

Ver também: `docs/specs/SPEC_SOURCE_OF_TRUTH.md`.

## Estrutura

- `docs/specs/implementados/`: specs consolidadas do que já existe no repo.
- `docs/specs/a_implementar/`: specs futuras ou preparadas para implementação.
- `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`: registry oficial das specs implementadas/parciais.
- `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`: registry oficial das specs futuras.
- `docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md`: roadmap macro de geração das próximas specs; não é spec implementável.
- `docs/refinements/implementados/`: refinamentos e PR waves já absorvidos.
- `docs/refinements/a_implementar/`: refinamentos futuros ainda não implementados.
- `docs/specs/`: fonte única oficial de specs.
- `docs_old/`: histórico integral preservado.

## Regras de nomes

- Toda spec implementada deve ter prefixo `spec_`.
- Toda spec futura deve ter prefixo `spec_`, exceto `README.md`.
- Todo refinement implementado deve ter prefixo `ref_`.
- Todo refinement futuro deve ter prefixo `ref_`.
- A pasta raiz `spec/` foi absorvida e não deve ser recriada.

## Como implementar uma spec

Antes de implementar:

1. Ler `docs/operations/AGENT_EXECUTION_PROTOCOL.md`.
2. Ler `docs/operations/READING_MATRIX.md`.
3. Ler `docs/specs/SPEC_SOURCE_OF_TRUTH.md`.
4. Ler a spec alvo em `docs/specs/a_implementar/spec_*.md` ou `docs/specs/implementados/spec_*.md`.
5. Ler o refinement alvo em `docs/refinements/a_implementar/ref_*.md` ou `docs/refinements/implementados/ref_*.md`.
6. Conferir `docs/specs/SPEC_EXECUTION_ORDER.md`.
7. Ler registries, crosswalk e `docs_old/` somente quando o protocolo/matriz indicar.

Ao finalizar:

1. Criar/atualizar `docs/specs/implementados/spec_*.md`.
2. Criar/atualizar `docs/refinements/implementados/ref_*.md`.
3. Atualizar os dois registries de specs.
4. Atualizar os dois maps de refinements.
5. Atualizar `docs/IMPLEMENTATION_STATUS.md`.
6. Atualizar `PROJECT_LOG.md`.
7. Rodar `tools/docs/validate_docs.ps1`.

Specs antigas continuam preservadas em `docs_old/` e rastreadas em `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md`.
