# REF - Tracking documental e fonte unica reconciliados parcialmente

> Status: Implementado documental parcial
> Origem: `docs/refinements/a_implementar/pre_refinamentos/refinamento_init_tracking_documental_status_specs.md`
> Spec relacionada: `.specs/implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md`

---

## 1. Resultado

O tracking documental foi reconciliado parcialmente:

- `.specs/` passou a ser a fonte unica oficial de specs.
- A pasta raiz `specs/` foi removida e proibida.
- Specs futuras foram consolidadas em `.specs/a_implementar/`.
- Specs implementadas/parciais foram consolidadas em `.specs/implementados/`.
- Os 18 pre-refinamentos vivos foram movidos para `docs/refinements/a_implementar/pre_refinamentos/`.
- Refinements legados foram registrados em `docs/refinements/REFINEMENT_MIGRATION_AUDIT.md`.
- Root specs antigas foram registradas em `.specs/SPEC_MIGRATION_AUDIT.md`.

---

## 2. Pendencias restantes

- Corrigir encoding dos pre-refinamentos vivos quando forem refinados.
- Enriquecer specs futuras 01-17 usando seus pre-refinamentos relacionados.
- Reclassificar/mover cada pre-refinamento conforme for absorvido.
- Manter `docs_old/` somente como historico, nao como fonte ativa.

---

## 3. Regra futura

Pre-refinamentos nao devem gerar specs paralelas se ja existir spec em `.specs/a_implementar/`.

A acao correta e enriquecer a spec existente e depois marcar o pre-refinamento como absorvido.
