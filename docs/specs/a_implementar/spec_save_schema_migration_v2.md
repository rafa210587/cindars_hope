# SPEC - Save schema migration v2

> Spec ID: spec_save_schema_migration_v2
> Status: A implementar
> Ordem de execucao: 02
> Depende de: spec_unity_compile_validation_protocol_and_scripts
> Bloqueia: 03-17
> Tipo: Runtime / Save System
> Fonte: docs/specs/ como fonte unica; pre-refinamento absorvido em `docs/refinements/a_implementar/pre_refinamentos/refinamento_init_save_schema_migration_v2.md`.
> Escopo: implementar infraestrutura robusta de migracao de save com schema version, backup, migrations sequenciais, logs, escrita segura e validacao.
> Fora de escopo: inventory slots finais, skill trees, cave death/corpse recovery, bestiary/faction discoveries, UI final e mudanca real de schema para v2.

---

# /speckit.specify

## Contexto

O sistema de save atual usa `GameSaveData.SchemaVersion` e `SaveManager.CurrentSchemaVersion = 1`. Hoje, se o arquivo salvo tiver versao diferente, o load e rejeitado.

Evidencia atual:

```text
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Save/SaveData.cs
docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md
```

Esta spec prepara o save para mudancas estruturais futuras: inventory slots, skill tree/active slots, cave death/corpse recovery e bestiary/faction discoveries.

## Problema

O comportamento atual funciona para MVP, mas nao suporta evolucao segura de saves.

Gaps atuais:

- save antigo e rejeitado quando `SchemaVersion` diverge;
- nao existe pipeline de migrations;
- nao existe backup automatico antes de migrar;
- nao existe escrita segura com arquivo temporario;
- nao existe resultado estruturado de migration;
- nao existe log padronizado com versao origem/destino;
- `LoadGame()` e `TryReadExistingValidSave()` possuem rejeicao propria de schema e podem divergir;
- estruturas futuras podem quebrar saves existentes.

## Objetivo

Implementar uma infraestrutura de migration que permita carregar saves antigos automaticamente quando houver migration disponivel, preservando backup, registrando resultado e evitando corrupcao do save original.

## Decisoes aceitas

- Migration deve ser automatica no `LoadGame()` quando houver caminho completo de migration.
- `TryReadExistingValidSave()` tambem deve usar a mesma leitura com migration para preservar dados ao salvar fora da Farm.
- Deve existir um caminho unico de leitura, por exemplo `TryReadSaveWithMigration(...)`, usado por `LoadGame()` e por qualquer preservacao de save existente.
- `CurrentSchemaVersion` permanece `1` nesta spec.
- A primeira migration real `v1 -> v2` pertence a spec de inventory slots.

## User stories / engineering stories

- Como jogador, quero que um save antigo continue carregando apos evolucoes de schema quando houver migration suportada.
- Como desenvolvedor, quero migrations versionadas, sequenciais, seguras e idempotentes.
- Como agente, devo criar a infraestrutura de migration sem implementar agora o payload real de v2.

## Regras funcionais obrigatorias

### Versao atual

- `v1` e o schema atual.
- `CurrentSchemaVersion` deve permanecer em `1` nesta spec.
- Esta spec entrega infraestrutura de migration, nao mudanca funcional de schema.
- Nao criar migration real `v1 -> v2` sem payload real de inventory slots e validacao.

### Caminho unico de leitura

Criar ou equivalente:

```text
TryReadSaveWithMigration(savePath, allowWriteBack, out GameSaveData saveData, out SaveMigrationResult result)
```

Esse caminho deve ser usado por:

```text
LoadGame()
TryReadExistingValidSave()
```

Motivo: `SaveGame()` usa `TryReadExistingValidSave()` para preservar Farm/World/Cave quando salva fora da Farm. Se essa leitura continuar rejeitando schema diferente, dados preservados podem ser perdidos ou substituidos por defaults.

### Migration automatica no LoadGame

Ao carregar save antigo:

1. Detectar `SchemaVersion`.
2. Se `SchemaVersion == CurrentSchemaVersion`, carregar normal.
3. Se `SchemaVersion < CurrentSchemaVersion`, verificar se existe caminho sequencial completo.
4. Se existir, criar backup, migrar em memoria, validar e escrever save migrado.
5. Aplicar save migrado no runtime.
6. Se faltar migration no caminho, rejeitar com erro claro.

### Migration em JSON bruto + DTO tipado

O contrato de migration deve receber `RawJson` e tambem poder materializar `GameSaveData` quando compativel.

Nao adicionar biblioteca JSON nova nesta spec.
Nao alterar `Packages/**`.

### Saves sem SchemaVersion

Unity `JsonUtility` tende a materializar `int SchemaVersion` como `0` quando o campo nao existe.

Regra:

```text
SchemaVersion == 0 = legacy candidate.
```

Se o JSON tiver estrutura minima compativel com v1, tratar como sourceVersion `1`. Se nao tiver, rejeitar com erro claro.

Estrutura minima sugerida:

```text
Player existe ou pode ser normalizado
Inventory existe ou pode ser normalizado
CurrentDay existe ou pode assumir default seguro
Farm/World/Cave podem ser null e normalizados para defaults
```

### Backup e escrita segura

Backup deve ser criado antes de qualquer sobrescrita do save original.

Destino recomendado:

```text
saves/backups/slot_1_yyyyMMdd_HHmmss.json
```

MVP nao precisa limpar backups antigos.

Escrita segura obrigatoria:

```text
slot_1.json.tmp
validar escrita temporaria
substituir/mover para slot_1.json
```

Se a escrita temporaria falhar, o save original deve permanecer intacto.

### Idempotencia

- Migration nao deve ser reaplicada fora de ordem.
- Registry deve impedir reaplicar migration em save ja migrado.
- Se faltar qualquer migration sequencial no caminho, rejeitar.

### Falha de migration

Se migration falhar:

- nao sobrescrever o save original;
- manter backup se ja foi criado;
- retornar `false` no load;
- nao aplicar save parcial no runtime;
- logar erro claro com origem/destino e migration que falhou.

### Schema futuro

Se `SchemaVersion > CurrentSchemaVersion`, rejeitar com mensagem clara.

Nunca tentar downgrade.

Mensagem esperada:

```text
Save schema version X is newer than supported version Y.
```

## Contratos esperados

Criar ou equivalente:

```text
Assets/_Game/Scripts/Save/Migrations/ISaveMigration.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationContext.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationRegistry.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationResult.cs
Assets/_Game/Scripts/Save/Migrations/SaveBackupService.cs
```

O registry deve expor comportamento equivalente a:

```text
CanMigrate(sourceVersion, targetVersion)
TryMigrate(context, out result)
```

`SaveMigrationContext` deve incluir, no minimo:

```text
SaveFilePath
BackupFilePath
SourceSchemaVersion
TargetSchemaVersion
RawJson
```

`SaveMigrationResult` deve incluir, no minimo:

```text
Success
SourceSchemaVersion
TargetSchemaVersion
BackupFilePath
MigratedJson
ErrorMessage
AppliedMigrationIds
```

## Validacao e normalizacao minima

Criar ou equivalente:

```text
ValidateAndNormalizeSave(GameSaveData saveData)
```

Checar/normalizar:

```text
saveData != null
SchemaVersion == CurrentSchemaVersion
Inventory ??= new InventorySaveData()
Farm ??= new FarmSaveData()
World ??= new WorldSaveData()
Cave ??= new CaveSaveData()
```

Player pode ser validado de forma conservadora: se ausente, rejeitar ou aplicar default apenas se o comportamento atual ja aceitar isso com seguranca.

## Versoes futuras planejadas

Nao implementar agora, mas deixar o registry preparado para:

```text
v1: schema atual
v2: inventory slots
v3: skill tree / active slots
v4: cave death / corpse recovery
v5: bestiary / faction discoveries
```

## Criterios de aceite

- Save v1 atual continua carregando sem migration.
- `CurrentSchemaVersion` continua `1`.
- Registry vazio e suportado sem quebrar load v1.
- Save com schema futuro e rejeitado claramente.
- Save sem `SchemaVersion` e tratado como legacy candidate ou rejeitado claramente.
- `LoadGame()` usa caminho comum com migration.
- `TryReadExistingValidSave()` usa o mesmo caminho comum com migration.
- Migration cria backup antes de sobrescrever arquivo.
- Escrita usa arquivo temporario antes de substituir o save original.
- Migrations sao sequenciais e idempotentes.
- Falha de migration nao corrompe save original e nao aplica runtime parcial.
- Nenhum DTO serializa referencias Unity.
- Unity compile validation e docs validation sao registradas no final.

---

# /speckit.plan

## Arquitetura

Criar subpasta:

```text
Assets/_Game/Scripts/Save/Migrations/
```

Componentes:

```text
ISaveMigration
SaveMigrationContext
SaveMigrationRegistry
SaveMigrationResult
SaveBackupService
```

`SaveManager` deve delegar a leitura para um caminho comum antes de rejeitar schema diferente.

## Sistemas afetados

```text
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Save/Migrations/**
docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md
```

## Fluxo detalhado

### Load v1 sem migration

```text
Read raw json
Detect schema version = 1
Deserialize GameSaveData
ValidateAndNormalizeSave
ApplySaveData
```

### Load com migration automatica

```text
Read raw json
Detect source schema version
If source < current, check complete migration path
Create backup in saves/backups
Run migrations source -> current in memory
ValidateAndNormalizeSave
Write slot_1.json.tmp
Replace/move tmp to slot_1.json
ApplySaveData
```

### TryReadExistingValidSave com migration

```text
SaveGame calls TryReadExistingValidSave
TryReadExistingValidSave delegates to TryReadSaveWithMigration
If migration succeeds, returns migrated GameSaveData
If migration fails, returns null and logs reason without corrupting original
```

### Load com schema futuro

```text
If SchemaVersion > CurrentSchemaVersion, reject.
No downgrade.
No partial apply.
```

## Eventos

Nao criar evento novo nesta spec. Usar logs claros. Evento de migration pode ser futuro quando houver UI para expor falhas ao jogador.

## UI

Fora de escopo.

## Riscos de regressao

- Save v1 pode parar de carregar se `LoadGame` for alterado de forma agressiva.
- `TryReadExistingValidSave` pode continuar rejeitando schema e perder preservacao de Farm/World/Cave.
- Backup pode ser criado tarde demais e nao proteger o original.
- Escrita direta pode corromper save se falhar no meio.
- Aumentar `CurrentSchemaVersion` sem migration real pode quebrar todos os saves existentes.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar `SaveManager.cs` e `SaveData.cs` antes de alterar.
- [ ] Criar contratos em `Assets/_Game/Scripts/Save/Migrations/`.
- [ ] Implementar `SaveBackupService` com pasta `saves/backups/`.
- [ ] Implementar escrita segura via `.tmp`.
- [ ] Implementar `SaveMigrationRegistry`.
- [ ] Implementar `SaveMigrationResult`.
- [ ] Implementar detection de schema antes da rejeicao atual.
- [ ] Implementar caminho comum `TryReadSaveWithMigration` ou equivalente.
- [ ] Fazer `LoadGame()` usar o caminho comum.
- [ ] Fazer `TryReadExistingValidSave()` usar o caminho comum.
- [ ] Manter load normal de v1 sem migration.
- [ ] Preparar registry para migrations futuras, sem implementar payload v2/v3/v4/v5 agora.
- [ ] Validar falha de schema futuro.
- [ ] Validar save sem `SchemaVersion` conforme regra definida.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Save/**
docs/specs/**
docs/refinements/**
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Arquivos proibidos

```text
docs_old/**
Packages/**
ProjectSettings/**
Assets/_Game/Scenes/**
Assets/_Game/Data/**
```

## Definition of Done

- Pipeline de migration existe.
- Save v1 atual continua carregando.
- `CurrentSchemaVersion` permanece 1.
- Backup e criado antes de qualquer sobrescrita.
- Escrita usa `.tmp` antes de substituir o save original.
- `LoadGame()` e `TryReadExistingValidSave()` usam o caminho comum de leitura com migration.
- Migration falha sem corromper save original.
- Schema futuro e rejeitado claramente.
- Registry esta pronto para v2-v5 futuras.
- Documentacao de status foi atualizada sem falso positivo.

## Validacao

Rodar:

```powershell
.\tools\docs\validate_docs.ps1

.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"

.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```

Validacao manual/Play Mode minima:

1. Criar save v1.
2. Carregar save v1 sem migration.
3. Simular save com schema futuro e validar rejeicao clara.
4. Simular save sem SchemaVersion e validar comportamento definido.
5. Validar que inventory/gold/day/position nao sao perdidos.
6. Validar que backup e criado antes de sobrescrita quando migration e acionada.
7. Validar que erro de migration nao aplica runtime parcial.
