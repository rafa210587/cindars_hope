# SPEC SAVE-002 - Save schema migration v2

> Status: Implementado parcial
> Ordem original: 02
> Tipo: Runtime / Save System
> Fonte: `docs/specs/a_implementar/spec_save_schema_migration_v2.md`
> Refinement: `docs/refinements/implementados/ref_save_schema_migration_v2.md`
> Evidencia principal: `Assets/_Game/Scripts/Save/Migrations/**`, `Assets/_Game/Scripts/Save/SaveManager.cs`

---

# /speckit.specify

## Contexto

O save atual permanece em `CurrentSchemaVersion = 1`, mas agora possui uma infraestrutura minima para migrations futuras sem rejeitar caminhos validos por pontos diferentes do `SaveManager`.

## Problema resolvido

Antes, `LoadGame()` e `TryReadExistingValidSave()` tinham rejeicoes independentes de schema. Isso criava risco de perda de dados preservados quando o save evoluisse.

## Objetivo entregue

- Criar contratos de migration.
- Criar registry sequencial e idempotente.
- Criar resultado estruturado.
- Criar backup service.
- Unificar leitura por `TryReadSaveWithMigration`.
- Manter `CurrentSchemaVersion = 1`.
- Preparar base para migrations reais das specs seguintes.

## Fora de escopo mantido

- Nao foi criada migration real `v1 -> v2`.
- Inventory slots, skill trees, cave death/corpse recovery e bestiary/faction discoveries permanecem em specs futuras.

---

# /speckit.plan

## Arquitetura implementada

Arquivos criados:

```text
Assets/_Game/Scripts/Save/Migrations/ISaveMigration.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationContext.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationRegistry.cs
Assets/_Game/Scripts/Save/Migrations/SaveMigrationResult.cs
Assets/_Game/Scripts/Save/Migrations/SaveBackupService.cs
```

`SaveManager` agora usa:

```text
TryReadSaveWithMigration(savePath, allowWriteBack, out GameSaveData saveData, out SaveMigrationResult result)
ValidateAndNormalizeSave(GameSaveData saveData, out string errorMessage)
WriteTextSafely(path, contents)
```

## Sistemas afetados

```text
Assets/_Game/Scripts/Save/SaveManager.cs
Assets/_Game/Scripts/Save/Migrations/**
```

## Save/load

- Save v1 continua carregando sem migration.
- Saves com schema futuro sao rejeitados claramente.
- Saves sem `SchemaVersion` sao tratados como legacy candidate se tiverem estrutura minima compativel.
- Escrita de save usa `.tmp`.
- Migrations futuras devem criar backup antes de sobrescrever arquivo original.

## Riscos de regressao

- `dotnet build` nao compilou por falta de `Temp/obj/Assembly-CSharp/project.assets.json`, entao a validacao formal Unity final ainda e obrigatoria.
- Como `CurrentSchemaVersion` segue em 1, o fluxo de backup/migration real sera exercitado pela primeira migration de payload real.

---

# /speckit.tasks

## Implementado

- [x] Contratos de migration criados.
- [x] Registry sequencial criado.
- [x] Resultado estruturado criado.
- [x] Backup service criado.
- [x] `LoadGame()` usa caminho comum.
- [x] `TryReadExistingValidSave()` usa caminho comum.
- [x] `CurrentSchemaVersion` permanece 1.
- [x] Schema futuro rejeita sem downgrade.
- [x] Escrita segura usa `.tmp`.

## Pendente

- [ ] Criar migration real `v1 -> v2` quando inventory slots definir payload final.
- [ ] Validar em Unity batchmode no final da sequencia 02-10.

## Validacao executada nesta etapa

```text
dotnet build .\Assembly-CSharp.csproj --no-restore
```

Resultado: nao concluiu compilacao por falta de `Temp/obj/Assembly-CSharp/project.assets.json`. A primeira tentativa tambem encontrou bloqueio de escrita sandbox em `Temp/obj`; a segunda com permissao removeu esse bloqueio, mas manteve a falta de assets NuGet.
