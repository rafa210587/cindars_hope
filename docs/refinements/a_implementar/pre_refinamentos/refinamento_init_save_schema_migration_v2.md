# refinamento_init_save_schema_migration_v2

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_save_schema_migration_v2.md`
> Objetivo: evoluir o save/load de schema fixo para migration robusta entre versoes.

---

## 1. Estado atual

`SaveManager` usa `CurrentSchemaVersion = 1` e rejeita saves com versao diferente.

Evidencia:

```text
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Save/SaveData.cs
docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md
```

O `GameSaveData` atual possui `SchemaVersion`, mas nao existe pipeline de migration.

---

## 2. Problema

O sistema atual funciona para MVP, mas nao suporta evolucao segura de saves.

Gaps:

- save de versao antiga e rejeitado;
- nao ha pipeline de migrations;
- novas estruturas como slots, skill tree, corpse, cave snapshots e equipment avancado podem quebrar saves antigos;
- nao ha backup automatico antes de migration;
- nao ha resultado estruturado de migration;
- nao ha relatorio/log padronizado de migration.

---

## 3. Escopo esperado

Criar sistema de migration:

```text
ISaveMigration
SaveMigrationContext
SaveMigrationRegistry
SaveMigrationResult
SaveBackupService
```

Fluxo:

1. Ler JSON bruto.
2. Detectar `SchemaVersion`.
3. Se schema for atual, carregar normalmente.
4. Se schema for antigo e houver migration disponivel, criar backup do arquivo original.
5. Aplicar migrations sequenciais.
6. Validar save migrado.
7. Salvar versao migrada.
8. Aplicar no runtime.

---

## 4. Versoes minimas sugeridas

```text
v1: schema atual.
v2: inventory slots.
v3: skill tree / active slots.
v4: cave death / corpse recovery.
v5: bestiary / faction discoveries.
```

Nao implementar todas as versoes agora. Esta etapa deve criar a estrutura para receber migrations incrementais.

Regra importante:

```text
CurrentSchemaVersion nao deve subir de 1 para 2 sem payload real de migration v1 -> v2 e validacao.
```

---

## 5. Arquivos provaveis

```text
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Save/Migrations/ISaveMigration.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationContext.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationRegistry.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationResult.cs
Assets/_Game/Scripts/Save/Migrations/SaveBackupService.cs
docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md
```

---

## 6. Definition of Done

- [ ] Save atual v1 continua carregando sem migration.
- [ ] Save antigo nao e rejeitado automaticamente se houver migration disponivel.
- [ ] Migration gera backup antes de alterar arquivo.
- [ ] Migrations sao sequenciais e idempotentes.
- [ ] Save sem `SchemaVersion` e tratado como legacy conhecido ou rejeitado com mensagem clara.
- [ ] Schema futuro e rejeitado com mensagem clara.
- [ ] Logs indicam versao origem/destino, backup e migrations aplicadas.
- [ ] Falha de migration nao corrompe o save original.
- [ ] Play Mode valida save v1 e casos negativos.

---

## 7. Validacao

1. Criar save v1.
2. Rodar LoadGame com schema v1 e validar que carrega sem migration.
3. Simular save com schema futuro e validar rejeicao clara.
4. Simular save sem `SchemaVersion` e validar comportamento definido.
5. Quando houver migration disponivel, validar backup.
6. Validar que save migrado nao perde inventory/gold/day/position.
7. Rodar validacao documental e Unity compile validation.

---

## 8. Relacao com specs futuras

Esta spec deve vir antes de:

- inventory slots;
- skill trees / active slots;
- cave death / corpse recovery;
- bestiary / faction discoveries.

Motivo: essas specs devem poder adicionar migrations incrementais sem reescrever o SaveManager novamente.
