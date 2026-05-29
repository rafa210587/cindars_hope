# refinamento_spec14b_cave_snapshot_replay_enemy_plan

> Status: Refinamento detalhado a implementar
> Spec relacionada: `docs/specs/a_implementar/spec_14b_cave_snapshot_replay_enemy_plan.md`
> Spec ampla relacionada: `docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md`
> Ordem de execucao sugerida: 14B
> Tipo: Cave Runtime / Snapshot Replay / Stable Level State / Enemy Plan Persistence
> Depende de: SPEC 14A implementada em codigo.

---

## 1. Contexto

A SPEC 14A conecta `EnemySpawnResolver` à cave runtime:

```text
EnemySpawnResolver
-> CaveEnemySpawnPlan
-> CaveRuntimeMaterializer.MaterializeEnemies
-> Enemy runtime instances
-> Bestiary events
```

A proxima lacuna é garantir que um nível já visitado na mesma run não seja re-gerado de forma invisível.

A SPEC 14 ampla já define que:

- entrar em nível já visitado usa snapshot;
- não rerolla layout, resources, fishing spot ou enemy spawn plan;
- `CaveLevelSnapshot` deve conter `EnemySpawnPlan[]`;
- snapshot não serializa referências Unity.

A SPEC 14B separa esse recorte para execução segura.

---

## 2. Problema

Mesmo com a 14A gerando inimigos por seed, ainda existem riscos:

- o layout pode ser re-gerado e ficar diferente ao revisitar;
- resource nodes podem mudar de posição;
- fishing spot pode rerollar;
- enemy spawn plan pode ser reconstruído de forma diferente se algum input mudar;
- não há um contrato único de snapshot contendo layout + resources + fishing + enemy plan;
- `VisitedLevelSnapshots` já existe no `CaveRuntimeState`, mas precisa ser consolidado para replay funcional.

---

## 3. Decisões de design

### 3.1 Snapshot é por run + level

Chave lógica:

```text
CaveRunSeed + CaveLevel
```

Campos auxiliares:

```text
CaveWorldSeed
LayoutHash
BiomeId
```

### 3.2 Snapshot deve impedir reroll

Ao entrar em um nível:

```text
se snapshot existe para RunSeed + Level:
    restaurar snapshot
senão:
    gerar level novo
    gerar resources/fishing/enemy plan
    capturar snapshot
```

### 3.3 Snapshot da 14B não é respawn completo

Esta spec pode preparar campos de inimigo derrotado/respawn, mas não deve implementar a política de 2 dias.

Fica para 14C:

```text
EnemyDefeatedState
RespawnAvailableAtGameDay
EnemyRespawn 2 dias
```

### 3.4 Snapshot da 14B não é redistribuição pós-morte

Redistribuição pós-morte fica para 14D.

---

## 4. Contratos desejados

### 4.1 CaveLevelSnapshot

Campos mínimos:

```text
string SnapshotId
string CaveWorldSeed
string CaveRunSeed
int CaveLevel
string BiomeId
string LayoutHash
int Width
int Height
List<Vector2Int> WalkableTiles
List<Vector2Int> WallTiles
Vector2Int Entrance
Vector2Int Exit
List<CaveResourceNodeSnapshotEntry> ResourceNodeStates
CaveFishingSpotSnapshotEntry FishingSpotState opcional
CaveEnemySpawnPlan EnemySpawnPlan
List<string> Warnings
```

Se o projeto já possuir `CaveLevelSnapshot`, ajustar compativelmente.

### 4.2 CaveResourceNodeSnapshotEntry

Campos mínimos:

```text
string NodeInstanceId
string ResourceNodeId
Vector2Int GridPosition
bool IsDepleted
```

Na 14B, registrar estado inicial e preservar depleted se o sistema atual já marcar depleted.

### 4.3 CaveFishingSpotSnapshotEntry

Campos mínimos:

```text
bool HasFishingSpot
string FishingSpotId
Vector2Int GridPosition
string FishingProfileId opcional
```

Regra:

```text
chance 10% por cave level
máximo 1 por level
resultado entra no snapshot
```

Se fishing spot runtime ainda não estiver materializado, snapshot deve deixar hook claro sem quebrar.

### 4.4 CaveSnapshotService

Responsável por:

- criar snapshot a partir do level gerado;
- restaurar `CaveGeneratedLevel` a partir do snapshot;
- calcular `LayoutHash`;
- ler/gravar snapshot em `CaveRuntimeState.VisitedLevelSnapshots`;
- converter snapshot para `CaveSaveData` e restaurar do save.

---

## 5. LayoutHash

O hash deve ser determinístico e comparável.

Entradas sugeridas:

```text
CaveLevel
BiomeId
Width
Height
Entrance
Exit
WalkableTiles ordenados
WallTiles ordenados
ResourceNodeStates ordenados
FishingSpotState
EnemySpawnPlan entries ordenadas
```

Regra:

- ordenar listas antes de gerar hash;
- não depender da ordem instável de HashSet/Dictionary;
- não usar hash que muda entre plataformas/sessões se houver alternativa estável.

---

## 6. Relação com CaveRuntimeMaterializer

O materializer deve aceitar dois modos:

```text
Materialize(generatedLevel, spawnAnchor)
MaterializeFromSnapshot(snapshot, spawnAnchor)
```

Se não criar método separado, o service deve reconstruir um `CaveGeneratedLevel` equivalente a partir do snapshot e chamar o fluxo existente.

Regra:

- revisitar nível já visitado usa snapshot;
- não chama geração procedural nova para o mesmo run+level;
- não chama EnemySpawnResolver novamente se `EnemySpawnPlan` já está no snapshot.

---

## 7. Fora de escopo

- Respawn de inimigos derrotados após 2 dias.
- Redistribuição pós-morte.
- Boss AI/rewards.
- Checkpoint side menu.
- UI de cave final.
- Save de HP/estado runtime individual de inimigo.
- Fishing minigame final.
- Renovação de resources por calendário.

---

## 8. Critérios de pronto do refinamento

A SPEC 14B está pronta quando:

- existe `CaveLevelSnapshot` funcional;
- existe `CaveSnapshotService` ou equivalente;
- entrar em nível novo cria snapshot;
- revisitar nível usa snapshot;
- `LayoutHash` é estável;
- `EnemySpawnPlan` entra no snapshot;
- resources/fishing hooks entram no snapshot;
- save/load preserva snapshots;
- validator comprova que não há reroll para level visitado;
- SPEC 14C fica responsável por defeated/respawn 2 dias.
