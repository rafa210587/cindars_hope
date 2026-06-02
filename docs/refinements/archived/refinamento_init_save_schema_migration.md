# refinamento_init_save_schema_migration

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/implementados/spec_save_002_schema_migration_v2.md`
> Objetivo: criar contrato e migracao de save entre versoes do schema.

---

## 1. Estado atual

Save v1 ja existente. Necessario criar infra de migracao para v2 sem perder progresso.

---

## 2. Gaps

- Nao ha versionamento formal de save schema.
- Nao ha sistema de migracao entre versoes.
- Dados persistidos podem ficar inconsistentes entre builds.

---

## 3. Decisoes aprovadas

- Save deve ser versionado.
- Migracao deve ser automatica ao carregar.
- Dados seguem: IDs + tipos simples, nunca refs Unity.
- Backups criados antes de migracao.
