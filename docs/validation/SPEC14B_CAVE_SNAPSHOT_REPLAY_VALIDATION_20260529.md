# SPEC 14B - Cave Snapshot Replay with EnemySpawnPlan Validation - 2026-05-29

## Resumo

Implementado o recorte SPEC 14B para consolidar snapshot/replay de cave level ja visitado dentro da mesma run.

O fluxo agora preserva, no snapshot:

- layout;
- entrance/exit;
- walkable/wall tiles;
- spawn points de inimigos/resources;
- `ResourceNodeStates`;
- `FishingSpotState` minimo;
- `CaveEnemySpawnPlan`;
- `LayoutHash` deterministico calculado sobre layout + resources + fishing + enemy plan.

Nao foram implementados respawn de inimigos por 2 dias, redistribuicao pos-morte, boss fights/rewards, checkpoint UI ou estado runtime individual completo de inimigos.

## Arquivos alterados

- `Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs`
- `Assets/_Game/Scripts/Cave/Runtime/CaveSnapshotService.cs`
- `Assets/_Game/Scripts/Cave/Runtime/CaveSnapshotService.cs.meta`
- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs`
- `Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs`
- `Assets/_Game/Scripts/Save/CaveSaveData.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateSpec14BCaveSnapshotReplay.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateSpec14BCaveSnapshotReplay.cs.meta`
- `Assembly-CSharp.csproj`
- `Assembly-CSharp-Editor.csproj`
- `docs/IMPLEMENTATION_STATUS.md`
- `PROJECT_LOG.md`

## Contratos criados / alterados

### CaveLevelSnapshot

Criado como tipo compativel com `VisitedLevelSnapshot`.

### VisitedLevelSnapshot

Expandido com:

- `CaveWorldSeed`
- `CaveRunSeed`
- `ResourceNodeStates`
- `FishingSpotState`
- `CaveEnemySpawnPlan EnemySpawnPlan`
- `LegacyEnemySpawnPlanEntries`
- `Warnings`
- `BuildSnapshotId(caveRunSeed, caveLevel)` deterministico

### CaveResourceNodeSnapshotEntry

Campos:

- `NodeInstanceId`
- `ResourceNodeId`
- `GridPosition`
- `IsDepleted`

### CaveFishingSpotSnapshotEntry

Campos:

- `HasFishingSpot`
- `FishingSpotId`
- `GridPosition`
- `FishingProfileId`

### CaveSnapshotService

Responsabilidades:

- chavear snapshot por `CaveRunSeed + CaveLevel`;
- capturar snapshot;
- restaurar `CaveGeneratedLevel`;
- criar hook deterministico de fishing spot 10%;
- calcular `LayoutHash` por SHA256 em string canonica;
- validar replay hash.

## Estrategia de LayoutHash

O hash usa uma string canonica com listas ordenadas:

- `CaveLevel`
- `BiomeId`
- `Width` / `Height`
- `Entrance`
- `Exit`
- `WalkableTiles`
- `WallTiles`
- `ResourceNodeStates`
- `FishingSpotState`
- `CaveEnemySpawnPlan.Entries`

O algoritmo usa SHA256 e retorna os primeiros 8 bytes em hexadecimal.

## Como snapshot e capturado

`CaveLevelRuntimeController.CaptureSnapshot()` agora delega para `CaveSnapshotService.CaptureSnapshot(...)`.

O snapshot recebe:

- `CurrentGeneratedLevel`;
- seeds do `CaveRunManager`;
- `CaveRuntimeMaterializer.LastEnemySpawnPlan`;
- `CaveRuntimeMaterializer.LastResourceNodeSnapshots`;
- `CaveRunManager.State.DepletedNodeIds`.

## Como snapshot e restaurado

`CaveLevelRuntimeController.GenerateCurrentLevel()` consulta `CaveSnapshotService.TryGetSnapshot(...)`.

Se existir snapshot valido para a mesma run e level:

- reconstrói `CaveGeneratedLevel` via `RestoreGeneratedLevel`;
- valida hash;
- chama `CaveRuntimeMaterializer.MaterializeFromSnapshot(...)`;
- nao chama `EnemySpawnResolver` novamente quando o snapshot ja possui `CaveEnemySpawnPlan`.

## Como EnemySpawnPlan entra no snapshot

O `CaveRuntimeMaterializer` expõe `LastEnemySpawnPlan`.

Na primeira visita, o plano gerado pela SPEC 14A e capturado em `VisitedLevelSnapshot.EnemySpawnPlan`.

Na revisita, `MaterializeFromSnapshot` usa esse plano salvo como override e materializa os mesmos `EnemyId`, `EnemyInstanceId`, `GridPosition`, `SpawnProfileId`, `PackId`, `SizeClass` e `FactionId`.

## Validações executadas

- `dotnet build .\Assembly-CSharp.csproj --no-restore`
  - Sucesso, 0 erros, 0 avisos.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`
  - Primeira tentativa paralela falhou por lock temporario de `Temp\obj\Assembly-CSharp\Assembly-CSharp.dll` pelo processo `aswEngSrv.exe`.
  - Reexecucao isolada: sucesso, 0 erros.
  - 3 avisos pre-existentes em `CreateEnemyActionsAndSets.cs`.
- `.\tools\docs\validate_docs.ps1`
  - Sucesso.
- `git diff --check`
  - Sucesso fora do sandbox.
- `.\.claude\hooks\check-runtime-forbidden-search.ps1`
  - Sucesso; nenhum uso novo proibido de busca global runtime nos arquivos alterados.
- `.\.claude\hooks\check-csproj-includes.ps1`
  - Sucesso; scripts C# alterados estao presentes nos `.csproj` locais.
- `.\.claude\hooks\check-cave-stable-run-scope.ps1`
  - Sucesso com aviso esperado de escopo cave; documentos FASE9F foram lidos antes da implementacao.
- `.\tools\unity\RunUnityCompileValidation.ps1`
  - Tentado.
  - O wrapper retornou exit code 1, mas `Logs/unity-compile-validation.log` mostra compilacao Bee/Csc concluida e `Exiting batchmode successfully now!` / return code 0.
  - Nao ha `error CS` no log.
  - Ha 1 warning CS0219 pre-existente em `CreateEnemyActionsAndSets.cs`.
- `.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"`
  - Tentado.
  - Falhou por falsos positivos nas linhas `Csc ... Assembly-CSharp.dll` e por assemblies `*-firstpass.dll not valid` ja observados anteriormente no projeto.

## Validações pendentes / não executadas

- Validator Editor `CindarsHope/Validation/Validate SPEC 14B - Cave Snapshot Replay`: pendente de execucao no Unity.
- Play Mode humano: pendente.

## Checklist Play Mode pendente

1. Entrar em `CaveScene` nivel 1.
2. Registrar `LayoutHash` e `EnemySpawnPlan`.
3. Sair e voltar para o mesmo nivel na mesma run.
4. Confirmar mesmo `LayoutHash`.
5. Confirmar mesmo `EnemySpawnPlan`.
6. Confirmar resources nao rerollam.
7. Confirmar fishing hook nao rerolla.
8. Salvar/carregar.
9. Confirmar snapshot preservado.
10. Confirmar console sem erros novos.

## Riscos residuais

- Resource node runtime ainda precisa expor seu `ResourceNodeDataSO` de forma direta; por ora o materializer registra o id a partir do dado selecionado/nome configurado.
- Fishing spot ainda e hook de snapshot, nao minigame/materializacao final.
- Estado individual de inimigo derrotado/HP/respawn fica para SPEC 14C.
- Redistribuicao pos-morte fica para SPEC 14D.
- Unity/Play Mode ainda precisa validar o wiring das cenas.

## Proximo recorte recomendado

`SPEC 14C - Enemy defeated state and 2-day respawn`.
