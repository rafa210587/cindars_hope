# Specs – Cindar's Hope

Ver também: `.specs/SPEC_SOURCE_OF_TRUTH.md`.

## Estrutura

- `.specs/implementados/`: specs consolidadas do que já existe no repo.
- `.specs/a_implementar/`: specs futuras ou preparadas para implementação.
- `.specs/SPEC_REGISTRY_IMPLEMENTED.md`: registry oficial das specs implementadas/parciais.
- `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md`: registry oficial das specs futuras.
- `.specs/SPEC_GENERATION_ROADMAP_MASTER.md`: roadmap macro de geração das próximas specs; não é spec implementável.
- `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`: template canônico para gerar novas specs implementáveis no padrão SpecKit/SDD.
- `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`: protocolo canônico de execução por waves, paralelização, locks e handoff para agentes; não é spec implementável.
- `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`: matriz canônica de validação por tipo de mudança, evidência mínima e status caps; não é spec implementável.
- `.specs/SPEC_GENERATION_ROADMAP_TESTING_QUALITY_GATE_ADDENDUM.md`: addendum do roadmap para exigir quality gate de testes antes das próximas specs runtime em massa; candidato a arquivamento após validação humana.
- `docs/refinements/implementados/`: refinamentos e PR waves já absorvidos.
- `docs/refinements/a_implementar/`: refinamentos futuros ainda não implementados.
- `.specs/`: fonte única oficial de specs.
- `docs_old/`: histórico integral preservado.

## Regras de nomes

- Toda spec implementada deve ter prefixo `spec_`.
- Toda spec futura deve ter prefixo `spec_`, exceto `README.md`.
- Documentos canônicos de governança em `.specs/` podem usar prefixo `SPEC_` e não entram no registry como specs executáveis.
- Todo refinement implementado deve ter prefixo `ref_`.
- Todo refinement futuro deve ter prefixo `ref_`.
- A pasta raiz `spec/` foi absorvida e não deve ser recriada.

## Como implementar uma spec

Antes de implementar:

1. Ler `CLAUDE.md`.
2. Ler `docs/project/CURRENT_STATE.md`.
3. Ler `.specs/SPEC_SOURCE_OF_TRUTH.md`.
4. Ler `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`.
5. Ler `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`.
6. Ler a spec alvo em `.specs/a_implementar/spec_*.md` ou `.specs/implementados/spec_*.md`.
7. Conferir `.specs/SPEC_EXECUTION_ORDER.md` somente quando a execução da spec/lote estiver autorizada.
8. Ler `.claude/rules/testing-quality-gate.md` para qualquer spec que altere código/runtime.
9. Ler registries, crosswalk, refinements e `docs_old/` somente quando o protocolo/matriz/spec indicar.

Ao finalizar:

1. Criar/atualizar `.specs/implementados/spec_*.md` apenas quando houver evidência de elegibilidade.
2. Criar/atualizar `docs/refinements/implementados/ref_*.md` quando aplicável.
3. Atualizar registries de specs quando aplicável.
4. Atualizar maps de refinements quando aplicável.
5. Atualizar `docs/IMPLEMENTATION_STATUS.md` e `PROJECT_LOG.md` apenas em fluxo de closeout autorizado.
6. Rodar `tools/docs/validate_docs.ps1`.
7. Registrar Testing Quality Gate em `docs/validation/<spec_id>_execution_report.md` para specs com código/runtime.

## Como gerar uma nova spec

Antes de criar qualquer novo arquivo em `.specs/a_implementar/`:

1. Ler `docs/design/SPEC_SOURCE_MAP.md`.
2. Ler `docs/design/SPECIFICATION_PROCESS.md`.
3. Ler `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`.
4. Ler `.specs/SPEC_GENERATION_ROADMAP_MASTER.md`.
5. Ler as fontes canônicas do domínio.
6. Validar estado real do repo.
7. Declarar se a spec pode rodar em paralelo, com quais specs e quais arquivos/sistemas exigem lock.
8. Não exigir validação humana intermediária; quando necessário, deixar cenário humano como `DEFERRED_TO_FINAL_VALIDATION`.

Specs antigas continuam preservadas em `docs_old/` e rastreadas em `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md`.
