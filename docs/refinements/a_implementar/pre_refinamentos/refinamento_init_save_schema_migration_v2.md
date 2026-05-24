# refinamento_init_save_schema_migration_v2

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_save_schema_migration_v2.md`
> **Objetivo:** evoluir o save/load de schema fixo para migration robusta entre versÃµes.

---

## 1. Estado atual

`SaveManager` usa `CurrentSchemaVersion = 1` e rejeita saves com versÃ£o diferente.

EvidÃªncia:

```text
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Save/SaveData.cs
docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md
```

---

## 2. Problema

O sistema atual funciona para MVP, mas nÃ£o suporta evoluÃ§Ã£o segura de saves.

Gaps:

- save de versÃ£o antiga Ã© rejeitado;
- nÃ£o hÃ¡ pipeline de migrations;
- novas estruturas como slots, skill tree, corpse, cave snapshots e equipment avanÃ§ado podem quebrar saves antigos;
- nÃ£o hÃ¡ backup automÃ¡tico antes de migration;
- nÃ£o hÃ¡ relatÃ³rio de migration.

---

## 3. Escopo esperado

Criar sistema de migration:

```text
ISaveMigration
SaveMigrationContext
SaveMigrationRegistry
SaveBackupService
```

Fluxo:

1. Ler JSON bruto.
2. Detectar `SchemaVersion`.
3. Se antigo, criar backup do arquivo original.
4. Aplicar migrations sequenciais.
5. Validar save migrado.
6. Salvar versÃ£o nova.
7. Aplicar no runtime.

---

## 4. VersÃµes mÃ­nimas sugeridas

```text
v1: schema atual.
v2: inventory slots.
v3: skill tree / active slots.
v4: cave death/corpse/recovery.
v5: bestiary/faction discoveries.
```

NÃ£o implementar todas as versÃµes agora; criar estrutura para receber migrations incrementais.

---

## 5. Arquivos provÃ¡veis

```text
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Save/Migrations/ISaveMigration.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationRegistry.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationResult.cs
Assets/_Game/Scripts/Save/Migrations/SaveBackupService.cs
docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md
```

---

## 6. Definition of Done

- [ ] Save antigo nÃ£o Ã© rejeitado automaticamente se houver migration disponÃ­vel.
- [ ] Migration gera backup antes de alterar arquivo.
- [ ] Migrations sÃ£o sequenciais e idempotentes.
- [ ] Save sem `SchemaVersion` Ã© tratado como versÃ£o legacy conhecida ou rejeitado com mensagem clara.
- [ ] Logs indicam versÃ£o origem/destino.
- [ ] Play Mode valida save v1 -> v2.

---

## 7. ValidaÃ§Ã£o

1. Criar save v1.
2. Atualizar schema para v2.
3. Rodar LoadGame.
4. Validar backup.
5. Validar save migrado.
6. Validar que nÃ£o hÃ¡ perda de inventory/gold/day/position.
