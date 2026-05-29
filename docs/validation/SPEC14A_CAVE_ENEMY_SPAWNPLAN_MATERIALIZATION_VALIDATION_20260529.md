# SPEC 14A - Cave Enemy SpawnPlan / Materialization Validation - 2026-05-29

## Resumo

Implementado o elo minimo da SPEC 14A para conectar o `EnemySpawnResolver` da SPEC 13F ao runtime da cave:

```text
EnemySpawnResolver
-> CaveEnemySpawnPlan
-> CaveRuntimeMaterializer.MaterializeEnemies
-> Enemy runtime instances
-> EnemySpawnedEvent / EnemySeenEvent
-> BestiaryManager
```

Nao foi implementada a SPEC 14 completa. Snapshot/replay completo de inimigos, respawn de inimigos comuns, redistribuicao pos-morte, boss fights, rewards e UI continuam fora deste recorte.

## Arquivos alterados

- `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlan.cs`
- `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs`
- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs`
- `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanService.cs`
- `Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs`
- `Assets/_Game/Scripts/Core/Events/EnemyEvents.cs`
- `Assets/_Game/Scripts/Enemy/BestiaryManager.cs`
- `Assets/_Game/Scripts/Enemy/EnemyBrain.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateSpec14AEnemySpawnMaterialization.cs`
- `docs/IMPLEMENTATION_STATUS.md`
- `PROJECT_LOG.md`

## Contratos criados / ajustados

### CaveEnemySpawnPlan

Campos:

- `CaveLevel`
- `BiomeId`
- `CaveWorldSeed`
- `CaveRunSeed`
- `LevelSeed`
- `LayoutHash`
- `Entries`
- `Warnings`

### CaveEnemySpawnPlanEntry

Campos:

- `EnemyInstanceId`
- `EnemyId`
- `SpawnProfileId`
- `PackId`
- `GridPosition`
- `WorldPosition`
- `RoomId`
- `SpawnIndex`
- `IsElite`
- `SizeClass`
- `FactionId`

### CaveEnemySpawnPlanner

Responsabilidades implementadas:

- monta `EnemySpawnRequest`;
- usa `EnemySpawnResolver`;
- usa `CaveWorldSeed + CaveRunSeed + CaveLevel + BiomeId` para seed deterministica;
- seleciona posicoes a partir de `EnemySpawnPoints` ou fallback por `WalkableTiles`;
- filtra entrance, exit, paredes e tiles proximos demais da entrada/saida;
- aplica distancia minima entre inimigos;
- gera `EnemyInstanceId` deterministico sem `Guid.NewGuid`;
- retorna warnings quando resolver/posicoes/dados limitam a materializacao.

## Como a seed e usada

O planner deriva `LevelSeed` por hash estavel de:

```text
CaveWorldSeed | CaveRunSeed | CaveLevel | BiomeId | enemy_spawn_plan
```

O mesmo world seed, run seed, cave level, biome e layout geram o mesmo plano. Uma nova `CaveRunSeed` altera o hash, a ordem dos pontos e os `EnemyInstanceId`.

## Como os inimigos sao materializados

`CaveRuntimeMaterializer` agora executa `MaterializeEnemies(generatedLevel)` depois de floor/walls/exits/resources.

O fluxo:

1. cria `CaveEnemySpawnPlan`;
2. cria parent `GeneratedEnemies`;
3. busca `EnemyDataSO` por `EnemyId` no `EnemyDatabaseSO`;
4. instancia prefab configurado ou fallback runtime seguro;
5. configura `EnemyHealth`, `EnemyBrain`, `EnemyChaseController`, vulnerabilidade/telegraph/knockback/hit flash e contact damage;
6. posiciona em `WorldPosition`;
7. incrementa `CreatedEnemies`;
8. publica eventos de spawn/seen.

Se `EnemyDatabaseSO` ou spawn profiles nao estiverem ligados na cena, a materializacao loga warning claro e nao cria fallback silencioso de dados.

## Como o Bestiary recebe eventos

Ao materializar cada inimigo, o materializer publica:

- `EnemySpawnedEvent(EnemyId, Position, EnemyInstanceId, CaveLevel)`
- `EnemySeenEvent(EnemyId, Position, EnemyInstanceId, CaveLevel)`

`BestiaryManager` consome esses eventos e registra `FirstSeen`/`LastSeenCaveLevel` usando IDs e tipos simples. Nao foi criado bestiary paralelo.

## Validações executadas

- `dotnet restore .\Assembly-CSharp.csproj`
  - Sucesso.
- `dotnet restore .\Assembly-CSharp-Editor.csproj`
  - Sucesso.
- `dotnet build .\Assembly-CSharp.csproj --no-restore`
  - Sucesso, 0 erros, 0 avisos.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`
  - Sucesso, 0 erros.
  - 3 avisos pre-existentes em `CreateEnemyActionsAndSets.cs`.
- `.\tools\docs\validate_docs.ps1`
  - Sucesso.
  - Observacao: primeira execucao apontou headers `Bloqueia` ausentes em 14A/14B; os headers foram corrigidos e a validacao passou.
- `git diff --check`
  - Sucesso quando executado fora do sandbox.
  - Primeira tentativa no sandbox falhou por erro Git/MSYS `couldn't create signal pipe, Win32 error 5`.
- `.\.claude\hooks\check-runtime-forbidden-search.ps1`
  - Sucesso; nenhum uso novo proibido de busca global runtime nos arquivos alterados.
- `.\.claude\hooks\check-csproj-includes.ps1`
  - Sucesso; scripts C# alterados estao presentes nos `.csproj` locais.
- `.\.claude\hooks\check-cave-stable-run-scope.ps1`
  - Sucesso com aviso esperado de escopo cave; documentos FASE9F foram lidos nesta execucao.

## Validações pendentes / não executadas

- `.\tools\unity\RunUnityCompileValidation.ps1`
  - Tentado.
  - Resultado: falhou antes da compilacao porque outra instancia do Unity esta com o projeto aberto.
  - Comando tentado: `.\tools\unity\RunUnityCompileValidation.ps1`
  - Log: `Logs/unity-compile-validation.log`
  - Risco residual: Unity compile/batchmode nao foi validado nesta maquina.
- `.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"`
  - Tentado.
  - Resultado: falhou porque o log contem encerramento fatal do batchmode antes da compilacao (`Application will terminate with return code 1`).
- Play Mode humano: pendente.
- Validator Editor `CindarsHope/Validation/Validate SPEC 14A - Enemy Spawn Materialization`: pendente de execucao no Unity.

## Checklist Play Mode pendente

1. Abrir `CaveScene`.
2. Entrar no nivel 1 da cave.
3. Confirmar inimigos materializados.
4. Confirmar inimigos compativeis com band 1-10.
5. Sair/entrar na mesma run e confirmar mesmo plano.
6. Gerar nova run seed e confirmar plano diferente.
7. Confirmar `EnemySpawnedEvent` / `EnemySeenEvent`.
8. Confirmar `FirstSeen` no Bestiary.
9. Salvar/carregar e confirmar que seeds preservadas reconstroem o mesmo plano.
10. Confirmar console sem erros novos.

## Riscos residuais

- A cena precisa estar wired com `EnemyDatabaseSO`, spawn profiles, packs e faction locks gerados pela SPEC 13.
- Assets `.asset` de roster/spawn/bestiary ainda podem depender de geracao Editor/Unity pendente da SPEC 13G.
- Snapshot completo com `EnemySpawnPlan` persistido fica para SPEC 14B.
- Respawn comum, defeated state, redistribuicao pos-morte e boss gate full continuam fora de escopo.
- O fallback runtime usa componentes existentes e sprite builtin quando prefab final nao estiver ligado; arte/prefab final seguem pendentes.

## Proximo recorte recomendado

`SPEC 14B - Cave Snapshot Replay with EnemySpawnPlan`.
