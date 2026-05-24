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

Tambem ha dois pontos de rejeicao de schema que precisam ser unificados:

```text
LoadGame()
TryReadExistingValidSave()
```

`TryReadExistingValidSave()` e usado durante `SaveGame()` para preservar dados de Farm/World/Cave quando o jogador salva fora da Farm. Se ele continuar rejeitando schema diferente sozinho, pode haver perda de dados preservados.

---

## 2. Problema

O sistema atual funciona para MVP, mas nao suporta evolucao segura de saves.

Gaps:

- save de versao antiga e rejeitado;
- nao ha pipeline de migrations;
- nao ha caminho unico de leitura com migration;
- novas estruturas como slots, skill tree, corpse, cave snapshots e equipment avancado podem quebrar saves antigos;
- nao ha backup automatico antes de migration;
- nao ha escrita segura com `.tmp`;
- nao ha resultado estruturado de migration;
- nao ha relatorio/log padronizado de migration.

---

## 3. Decisoes aceitas

- Migration deve ser automatica no `LoadGame()` quando houver caminho completo de migration.
- `TryReadExistingValidSave()` tambem deve usar a mesma leitura com migration.
- Criar caminho unico, por exemplo:

```text
TryReadSaveWithMigration(savePath, allowWriteBack, out GameSaveData saveData, out SaveMigrationResult result)
```

- `CurrentSchemaVersion` permanece `1` nesta spec.
- A primeira migration real `v1 -> v2` pertence a spec de inventory slots.
- Esta spec entrega infraestrutura e seguranca, nao mudanca funcional de schema.

---

## 4. Escopo esperado

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
4. Se schema for antigo e houver migration completa, criar backup do arquivo original.
5. Aplicar migrations sequenciais em memoria.
6. Validar e normalizar save migrado.
7. Escrever em arquivo temporario `.tmp`.
8. Substituir save original apenas apos escrita temporaria valida.
9. Aplicar no runtime.

---

## 5. Regras importantes

### SchemaVersion 0

`SchemaVersion == 0` deve ser tratado como legacy candidate.

Se o JSON tiver estrutura minima compativel com v1, considerar sourceVersion `1`. Se nao tiver, rejeitar com erro claro.

### Schema futuro

Se `SchemaVersion > CurrentSchemaVersion`, rejeitar com mensagem clara.

Nunca tentar downgrade.

### Falha de migration

Se migration falhar:

- nao sobrescrever o save original;
- manter backup se ja foi criado;
- nao aplicar save parcial no runtime;
- retornar falha clara;
- logar origem, destino e migration que falhou.

### Backup

Backup em:

```text
saves/backups/slot_1_yyyyMMdd_HHmmss.json
```

MVP nao precisa cleanup automatico de backups.

---

## 6. Versoes minimas sugeridas

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

## 7. Arquivos provaveis

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

## 8. Definition of Done

- [ ] Save atual v1 continua carregando sem migration.
- [ ] `CurrentSchemaVersion` permanece `1`.
- [ ] `LoadGame()` usa caminho comum com migration.
- [ ] `TryReadExistingValidSave()` usa o mesmo caminho comum.
- [ ] Save antigo nao e rejeitado automaticamente se houver migration completa disponivel.
- [ ] Migration gera backup antes de alterar arquivo.
- [ ] Escrita usa `.tmp` antes de substituir o save original.
- [ ] Migrations sao sequenciais e idempotentes.
- [ ] Save sem `SchemaVersion` e tratado como legacy candidate ou rejeitado com mensagem clara.
- [ ] Schema futuro e rejeitado com mensagem clara.
- [ ] Logs indicam versao origem/destino, backup e migrations aplicadas.
- [ ] Falha de migration nao corrompe o save original e nao aplica runtime parcial.
- [ ] Play Mode valida save v1 e casos negativos.

---

## 9. Validacao

1. Criar save v1.
2. Rodar LoadGame com schema v1 e validar que carrega sem migration.
3. Simular save com schema futuro e validar rejeicao clara.
4. Simular save sem `SchemaVersion` e validar comportamento definido.
5. Quando houver migration disponivel, validar backup.
6. Validar que save migrado nao perde inventory/gold/day/position.
7. Validar que erro de migration nao aplica runtime parcial.
8. Rodar validacao documental e Unity compile validation.

---

## 10. Relacao com specs futuras

Esta spec deve vir antes de:

- inventory slots;
- skill trees / active slots;
- cave death / corpse recovery;
- bestiary / faction discoveries.

Motivo: essas specs devem poder adicionar migrations incrementais sem reescrever o SaveManager novamente.
