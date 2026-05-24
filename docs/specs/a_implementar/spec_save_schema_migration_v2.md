# SPEC - Save schema migration v2

> Spec ID: spec_save_schema_migration_v2
> Status: A implementar
> Ordem de execucao: 02
> Depende de: spec_unity_compile_validation_protocol_and_scripts
> Bloqueia: 03-17
> Tipo: Runtime / Save System
> Fonte: docs/specs/ como fonte unica; pre-refinamento absorvido em `docs/refinements/a_implementar/pre_refinamentos/refinamento_init_save_schema_migration_v2.md`.
> Escopo: implementar pipeline robusto de migracao de save com schema version, backup, migrations sequenciais, logs e validacao.
> Fora de escopo: inventory slots finais, skill trees, cave death/corpse recovery, bestiary/faction discoveries e UI final.

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

Esta spec prepara o save para as proximas mudancas estruturais: inventory slots, skill tree/active slots, cave death/corpse recovery e bestiary/faction discoveries.

## Problema

O comportamento atual funciona para MVP, mas nao suporta evolucao segura de saves.

Gaps atuais:

- save antigo e rejeitado quando `SchemaVersion` diverge;
- nao existe pipeline de migrations;
- nao existe backup automatico antes de migrar;
- nao existe resultado estruturado de migration;
- nao existe log padronizado com versao origem/destino;
- estruturas futuras podem quebrar saves existentes.

## Objetivo

Implementar uma camada de migration que permita carregar saves antigos quando houver migration disponivel, preservando backup e registrando resultado.

## User stories / engineering stories

- Como jogador, quero que um save antigo continue carregando apos evolucoes de schema quando houver migration suportada.
- Como desenvolvedor, quero migrations versionadas, sequenciais e idempotentes.
- Como agente, devo criar a infraestrutura de migration sem implementar agora todas as versoes futuras.

## Regras funcionais obrigatorias

### Versao atual

- `v1` e o schema atual.
- `CurrentSchemaVersion` deve permanecer em `1` nesta spec, salvo se uma migration real para `v2` for implementada e validada.
- Esta spec deve criar a infraestrutura para migration, nao forcar mudanca de schema sem payload real.

### Migration pipeline

O load deve seguir o fluxo:

1. Ler JSON bruto.
2. Detectar `SchemaVersion` antes de aplicar no runtime.
3. Se `SchemaVersion == CurrentSchemaVersion`, carregar normalmente.
4. Se `SchemaVersion < CurrentSchemaVersion`, procurar migrations sequenciais.
5. Antes de alterar arquivo, criar backup do save original.
6. Aplicar migrations em ordem crescente.
7. Validar o save migrado.
8. Salvar a versao migrada.
9. Aplicar no runtime.
10. Logar origem, destino, migrations aplicadas e backup.

### Saves sem SchemaVersion

Save sem `SchemaVersion` deve ser tratado como legacy conhecido ou rejeitado com mensagem clara. A decisao preferida para MVP e tratar como `v1` somente se o JSON possuir estrutura compatível com v1.

### Backup

Backup deve ser criado antes de sobrescrever o save original.

Formato sugerido:

```text
slot_1.json.bak_yyyyMMdd_HHmmss
```

### Idempotencia

Uma migration nao deve quebrar se for chamada em save ja migrado. O registry deve impedir reaplicar migration fora de ordem.

### Falha de migration

Se migration falhar:

- nao sobrescrever o save original;
- manter backup;
- retornar `false` no load;
- logar erro claro;
- publicar/registrar falha quando houver evento apropriado.

## Contratos esperados

Criar ou equivalente:

```text
Assets/_Game/Scripts/Save/Migrations/ISaveMigration.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationContext.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationRegistry.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationResult.cs
Assets/_Game/Scripts/Save/Migrations/SaveBackupService.cs
```

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

- Save com schema atual continua carregando sem migration.
- Save antigo nao e rejeitado automaticamente se houver migration disponivel.
- Migration cria backup antes de sobrescrever arquivo.
- Migrations sao sequenciais.
- Save sem `SchemaVersion` recebe tratamento claro.
- Logs indicam versao origem/destino, backup e migrations aplicadas.
- Falha de migration nao corrompe o save original.
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

`SaveManager.LoadGame()` deve ser ajustado para delegar a decisao de migration antes de rejeitar schema diferente.

## Sistemas afetados

```text
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Save/Migrations/**
docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md
```

## Fluxo detalhado

### Load sem migration

```text
Read file
Deserialize GameSaveData
SchemaVersion == CurrentSchemaVersion
ApplySaveData
```

### Load com migration

```text
Read raw json
Detect source schema version
Create backup
Run migrations source -> current
Deserialize migrated json/save object
Validate minimum data
Write migrated save
ApplySaveData
```

### Load com schema futuro

Se `SchemaVersion > CurrentSchemaVersion`, rejeitar com mensagem clara:

```text
Save schema version X is newer than supported version Y.
```

## Dados / DTOs / IDs

`SaveMigrationContext` deve carregar pelo menos:

```text
SaveFilePath
BackupFilePath
SourceSchemaVersion
TargetSchemaVersion
RawJson
```

`SaveMigrationResult` deve carregar pelo menos:

```text
Success
SourceSchemaVersion
TargetSchemaVersion
BackupFilePath
MigratedJson
ErrorMessage
AppliedMigrationIds
```

## Eventos

Nao criar evento novo se nao houver necessidade. Se existir evento de save result, reutilizar. Caso crie evento, seguir padrao `*Event`.

## Save/load

O objetivo central e save/load. Nunca serializar:

```text
ScriptableObject
GameObject
Transform
MonoBehaviour
Sprite
Collider
Rigidbody
```

## UI

Fora de escopo. Mensagem/log e suficiente.

## Riscos de regressao

- Save atual pode parar de carregar se `LoadGame` for alterado de forma agressiva.
- Backup pode ser criado depois da escrita e nao proteger o original.
- Migration pode sobrescrever dados de inventory/gold/day/position.
- Aumentar `CurrentSchemaVersion` sem migration real pode quebrar todos os saves existentes.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar `SaveManager.cs` e `SaveData.cs` antes de alterar.
- [ ] Criar contratos em `Assets/_Game/Scripts/Save/Migrations/`.
- [ ] Implementar `SaveBackupService`.
- [ ] Implementar `SaveMigrationRegistry`.
- [ ] Implementar `SaveMigrationResult`.
- [ ] Implementar detection de schema antes da rejeicao atual.
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
- Backup e criado antes de qualquer sobrescrita.
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
