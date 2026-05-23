# refinamento_init_save_schema_migration_v2

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_save_schema_migration_v2.md`  
> **Objetivo:** evoluir o save/load de schema fixo para migration robusta entre versões.

---

## 1. Estado atual

`SaveManager` usa `CurrentSchemaVersion = 1` e rejeita saves com versão diferente.

Evidência:

```text
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Save/SaveData.cs
docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md
```

---

## 2. Problema

O sistema atual funciona para MVP, mas não suporta evolução segura de saves.

Gaps:

- save de versão antiga é rejeitado;
- não há pipeline de migrations;
- novas estruturas como slots, skill tree, corpse, cave snapshots e equipment avançado podem quebrar saves antigos;
- não há backup automático antes de migration;
- não há relatório de migration.

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
6. Salvar versão nova.
7. Aplicar no runtime.

---

## 4. Versões mínimas sugeridas

```text
v1: schema atual.
v2: inventory slots.
v3: skill tree / active slots.
v4: cave death/corpse/recovery.
v5: bestiary/faction discoveries.
```

Não implementar todas as versões agora; criar estrutura para receber migrations incrementais.

---

## 5. Arquivos prováveis

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

- [ ] Save antigo não é rejeitado automaticamente se houver migration disponível.
- [ ] Migration gera backup antes de alterar arquivo.
- [ ] Migrations são sequenciais e idempotentes.
- [ ] Save sem `SchemaVersion` é tratado como versão legacy conhecida ou rejeitado com mensagem clara.
- [ ] Logs indicam versão origem/destino.
- [ ] Play Mode valida save v1 -> v2.

---

## 7. Validação

1. Criar save v1.
2. Atualizar schema para v2.
3. Rodar LoadGame.
4. Validar backup.
5. Validar save migrado.
6. Validar que não há perda de inventory/gold/day/position.
