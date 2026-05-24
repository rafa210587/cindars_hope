# SPEC DOCS-001 - Fonte unica documental de specs e refinements

> Status: Implementado documental parcial
> Camada: Documentacao / governanca
> Fonte historica: `docs/specs/a_implementar/spec_docs_single_source_specs_refinements_reconciliation_v1.md`
> Evidencia principal: `docs/specs/SPEC_SOURCE_OF_TRUTH.md`, `docs/specs/SPEC_EXECUTION_ORDER.md`, `docs/specs/SPEC_MIGRATION_AUDIT.md`, `docs/refinements/REFINEMENT_MIGRATION_AUDIT.md`

---

## 1. /speckit.specify

A fonte oficial de specs foi consolidada em:

```text
docs/specs/
```

A antiga pasta raiz `specs/` foi removida e nao deve ser recriada.

Os refinamentos vivos foram organizados em:

```text
docs/refinements/a_implementar/pre_refinamentos/
```

Cada pre-refinamento vivo aponta para uma spec futura consolidada em `docs/specs/a_implementar/`.

---

## 2. /speckit.plan

Estrutura ativa:

```text
docs/specs/implementados/
docs/specs/a_implementar/
docs/specs/SPEC_EXECUTION_ORDER.md
docs/refinements/implementados/
docs/refinements/a_implementar/pre_refinamentos/
docs_old/
```

Evidencias:

- `docs/specs/SPEC_SOURCE_OF_TRUTH.md`
- `docs/specs/SPEC_EXECUTION_ORDER.md`
- `docs/specs/SPEC_MIGRATION_AUDIT.md`
- `docs/refinements/REFINEMENT_MIGRATION_AUDIT.md`
- `tools/docs/validate_docs.ps1`

---

## 3. /speckit.tasks

### Implementado

- [x] Fonte unica `docs/specs/` consolidada.
- [x] Pasta raiz `specs/` removida.
- [x] `SPEC_EXECUTION_ORDER.md` criado.
- [x] `SPEC_MIGRATION_AUDIT.md` criado.
- [x] `REFINEMENT_MIGRATION_AUDIT.md` criado.
- [x] 18 pre-refinamentos vivos organizados em `pre_refinamentos/`.
- [x] `validate_docs.ps1` atualizado para validar a fonte unica.

### Pendente documental

- [ ] Corrigir encoding dos pre-refinamentos quando forem refinados.
- [ ] Enriquecer specs futuras 01-17 com seus pre-refinamentos antes de execucao runtime.
- [ ] Mover/marcar cada pre-refinamento como absorvido conforme a spec relacionada for amadurecida.

---

## 4. Regras futuras

- Nao criar specs duplicadas se ja houver spec em `docs/specs/a_implementar/`.
- Refinamentos init devem enriquecer specs existentes.
- Specs futuras devem seguir `docs/specs/SPEC_EXECUTION_ORDER.md`.
- Nenhum item deve ser marcado como implementado sem evidencia no repo.

---

## 5. Validacao

- `tools/docs/validate_docs.ps1`
- Nenhum arquivo de runtime Unity faz parte desta spec documental.
