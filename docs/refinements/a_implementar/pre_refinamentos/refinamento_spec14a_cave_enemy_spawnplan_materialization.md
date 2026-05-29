# refinamento_spec14a_cave_enemy_spawnplan_materialization

> Status: Refinamento detalhado a implementar
> Spec relacionada: `docs/specs/a_implementar/spec_14a_cave_enemy_spawnplan_materialization_run_stability.md`
> Spec ampla relacionada: `docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md`
> Ordem de execucao sugerida: 14A
> Tipo: Cave Runtime / Enemy Spawn / Run Stability / Bestiary Integration
> Depende de: SPEC 13A-13F implementadas em codigo; SPEC 13G closeout preferencialmente executado antes ou dentro deste pacote.

---

## 1. Contexto

A SPEC 13 construiu a base data-driven dos inimigos:

- taxonomy/profiles/contracts;
- roster de inimigos;
- actions/action sets;
- EnemyBrain runtime MVP;
- Bestiary runtime/save;
- EnemySpawnResolver/ecology/faction locks.

A SPEC 14 ampla ja fala de:

- enemy spawn plan estavel por run/nivel;
- snapshots/replay;
- inimigos comuns derrotados voltando apos 2 dias;
- redistribuicao apos morte;
- `EnemySpawnPlan[]` dentro de `CaveLevelSnapshot`;
- materializacao runtime da cave.

Mas essa SPEC 14 e ampla demais para ser executada inteira agora. O elo que falta no runtime e:

```text
EnemySpawnResolver
-> CaveEnemySpawnPlan
-> CaveRuntimeMaterializer
-> Enemy runtime instances
-> EnemySpawnedEvent / EnemySeenEvent
-> BestiaryManager
```

Hoje a cave materializa floor, walls, portals e resource nodes. O plano de inimigos ainda nao esta conectado ao runtime da cave.

---

## 2. Problema

Os monstros novos existem como dados/contratos, e o resolver consegue decidir uma composicao logica. Mas ainda falta aparecerem no jogo de forma correta:

- os inimigos ainda nao sao materializados pela cave real;
- nao ha `CaveEnemySpawnPlan` persistente/reconstruivel por seed;
- a composicao dos inimigos ainda nao esta garantida como estavel dentro da mesma run;
- novo jogo/nova run ainda nao gera composicao diferente de inimigos na cave real;
- o Bestiary so sera efetivamente alimentado pela cave quando os inimigos forem materializados e publicarem eventos;
- SPEC 14 completa inclui snapshot, respawn e redistribuicao, mas isso deve ser quebrado em etapas.

---

## 3. Decisoes de design

### 3.1 Esta spec e SPEC 14A, nao SPEC 18

SPEC 18 trata combate do player:

- flechas;
- magias;
- melee;
- dash;
- dodge;
- Fireball;
- loadout.

A conexao dos monstros com a cave e parte da SPEC 14, porque envolve:

- cave generation;
- materialization;
- run seed;
- replay de nivel;
- snapshot futuro;
- spawn anchors;
- boss gates/faction locks.

### 3.2 Escopo minimo da 14A

A 14A deve implementar somente:

```text
Resolver -> SpawnPlan -> Materializer -> Enemy instances -> Bestiary events
```

Nao deve implementar:

- snapshot completo;
- respawn apos 2 dias;
- redistribuicao pos-morte;
- boss fight;
- boss rewards;
- UI final.

### 3.3 Regra de estabilidade

Comportamento esperado:

```text
Mesmo CaveWorldSeed + CaveRunSeed + CaveLevel = mesmo SpawnPlan.
Novo CaveRunSeed = novo SpawnPlan.
Novo jogo / novo world seed = composicao diferente.
Save/load com mesmas seeds = mesmo SpawnPlan reconstruido.
```

### 3.4 Run vs snapshot

Nesta spec, o SpawnPlan pode ser reconstruido deterministicamente por seed.

Nao e obrigatorio persistir snapshot completo do estado runtime dos inimigos ainda.

Mas o design deve preparar a proxima etapa:

```text
SPEC 14B = snapshot/replay full de layout/resources/enemy plan
SPEC 14C = defeated/respawn state comum 2 dias
SPEC 14D = redistribuicao pos-morte e death hooks
```

---

## 4. Contratos desejados

### 4.1 CaveEnemySpawnPlan

Representa o plano deterministico de inimigos para um cave level dentro de uma run.

Campos sugeridos:

```text
CaveLevel
BiomeId
CaveWorldSeed
CaveRunSeed
LevelSeed
LayoutHash opcional
Entries[]
Warnings[]
```

Regras:

- nao serializa GameObject/Transform/ScriptableObject;
- pode ser reconstruido por seed;
- futuro snapshot pode persistir o plano ou um hash dele;
- deve ser estavel para o mesmo input.

### 4.2 CaveEnemySpawnPlanEntry

Campos sugeridos:

```text
EnemyInstanceId
EnemyId
SpawnProfileId
PackId opcional
GridPosition
WorldPosition
RoomId opcional
SpawnIndex
IsElite
SizeClass
FactionId opcional
```

Regras:

- `EnemyInstanceId` deve ser estavel/deterministico;
- nao usar `Guid.NewGuid()` para entries do plano;
- nao alterar EnemyId ao revisitar o mesmo level/run;
- posicao pode mudar em specs futuras apenas por redistribuicao explicita.

Sugestao de ID:

```text
enemy_{caveLevel}_{roomId}_{spawnIndex}_{enemyId}_{stableHash}
```

Se nao houver RoomId real:

```text
enemy_{caveLevel}_tile_{x}_{y}_{spawnIndex}_{enemyId}_{stableHash}
```

### 4.3 CaveEnemySpawnPlanner

Responsavel por conectar `CaveGeneratedLevel` ao `EnemySpawnResolver`.

Inputs:

```text
CaveGeneratedLevel
CaveRunManager/CaveWorldSeed/CaveRunSeed
EnemySpawnResolver
EnemyDataDatabase/registry
EnemySpawnProfiles/Packs/Locks
```

Outputs:

```text
CaveEnemySpawnPlan
```

Regras:

- usar seed deterministica;
- resolver biome/environment tags a partir do level gerado;
- montar `EnemySpawnRequest`;
- converter resultado logico em entries com posicoes validas;
- respeitar tamanho de sala quando houver metadata;
- filtrar entrada/saida/player safe spawn;
- retornar warnings quando nao houver spawn valido.

### 4.4 CaveRuntimeMaterializer.MaterializeEnemies

Adicionar uma etapa de materializacao de inimigos.

Ordem desejada:

```text
MaterializeFloor
MaterializeWalls
MaterializeEntranceAndExit
MaterializeResourceNodes
MaterializeEnemies
```

Regras:

- criar parent `GeneratedEnemies`;
- instanciar prefab/config runtime de inimigo;
- configurar EnemyDataSO por EnemyId;
- aplicar EnemyInstanceId;
- aplicar position;
- aplicar size profile quando possivel;
- publicar eventos simples;
- incrementar `CreatedEnemies`.

---

## 5. Como escolher posicoes

### 5.1 Fonte preferida

Usar primeiro, se existir:

```text
CaveGeneratedLevel.EnemySpawnPoints
Room spawn anchors
Room centers
```

### 5.2 Fallback permitido

Se ainda nao houver EnemySpawnPoints/RoomGraph real, derivar a partir de `WalkableTiles` com filtros:

- nao usar Entrance;
- nao usar Exit;
- nao usar tiles muito perto do player spawn;
- nao usar parede;
- respeitar distancia minima entre inimigos;
- evitar corredores para Large/Huge/Boss quando possivel;
- evitar sobrepor resource nodes/portals.

### 5.3 Distancia minima inicial

Defaults sugeridos:

```text
MinDistanceFromEntrance = 5 tiles
MinDistanceFromExit = 2 tiles
MinDistanceBetweenEnemies = 2 tiles
LargeRequiresRoomLikeArea = true quando houver room metadata
```

### 5.4 Quantidade por level

Regra de design ja discutida:

```text
12-20 criaturas por cave level, variando por seed e band.
```

Para 14A, se o resolver/packs ainda nao suportar exatamente isso:

- usar `EnemySpawnRequest.MaxEnemies` entre 12 e 20 por seed;
- respeitar limite real de walkable/spawn points;
- registrar warning se nao houver espaço suficiente.

---

## 6. Eventos e Bestiary

Ao materializar inimigo, publicar evento compatível com SPEC 13E:

```text
EnemySpawnedEvent
ou
EnemySeenEvent
```

Payload deve conter apenas tipos simples:

```text
EnemyId
EnemyInstanceId
CaveLevel
Position opcional como Vector2/Vector3 se eventos ja aceitarem
```

Regras:

- `BestiaryManager` deve registrar FirstSeen;
- kill/damage continuam pelo EnemyHealth/EnemyBrain;
- nao criar Bestiary paralelo;
- nao criar event bus paralelo;
- nao carregar GameObject/Transform no evento.

---

## 7. Relação com save/load

Nesta spec:

- Save/load deve preservar `CaveWorldSeed` e `CaveRunSeed`, ja existentes no CaveRunManager.
- O spawn plan pode ser reconstruido a partir dessas seeds.
- Nao implementar enemy runtime snapshot completo ainda.

Futuro:

- SPEC 14B deve persistir/replay `CaveLevelSnapshot` com `EnemySpawnPlan[]`.
- SPEC 14C deve persistir `IsDefeated`, `DefeatedAtGameDay`, `RespawnAvailableAtGameDay`.
- SPEC 14D deve persistir redistribuicao/posicoes apos morte.

---

## 8. Fora de escopo

- Snapshot completo de level.
- Respawn de inimigos depois de 2 dias.
- Redistribuicao pos-morte.
- Boss fight e boss rewards.
- UI de Bestiary.
- UI de checkpoint.
- Materializacao final de boss gates.
- Balance final de quantidade/dificuldade.
- Arte final dos inimigos.
- Flying runtime.

---

## 9. Criterios de pronto do refinamento

A SPEC 14A esta pronta quando:

- existir `CaveEnemySpawnPlan`;
- existir `CaveEnemySpawnPlanEntry`;
- existir `CaveEnemySpawnPlanner`;
- `CaveRuntimeMaterializer` tiver etapa `MaterializeEnemies`;
- `EnemySpawnResolver` for usado;
- inimigos aparecerem na cave;
- a composicao for deterministica por `CaveWorldSeed + CaveRunSeed + CaveLevel`;
- nova run trocar composicao;
- mesma run preservar composicao;
- Bestiary receber FirstSeen dos inimigos materializados;
- houver validator e doc de validacao;
- a SPEC 14 ampla continuar aberta para snapshot/respawn/boss/checkpoint.
