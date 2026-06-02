---
required_adrs: [ADR-0005-cave-stable-run-and-replay]
required_game_rules: [cave_rules.md]
---

# SPEC - Cave runtime generation, checkpoints e boss gates

> Spec ID: spec_cave_runtime_generation_checkpoints_boss_gates
> Status: Implementado parcial - escopo residual ativo
> Ordem de execucao: 14
> Depende de: 00-13
> Bloqueia: 15, 17
> Tipo: Runtime/UI minima
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar cave runtime, 100 niveis macro, snapshots/replay, checkpoints com portal e menu lateral, boss gates, boss AI inicial, confinement, materializacao, camera bounds, enemy spawn plans estaveis, respawn de inimigos comuns, controle de drops de boss e save/load.
> Fora de escopo: fluxo completo de morte/corpse recovery/Fonte de Anya, UI final da cave, boss fights finais/polish, arte final dos biomas, calendario completo de renovacao de recursos, multiplayer, Packages, ProjectSettings e docs_old.

Fontes absorvidas:
- specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/spec.md
- specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/amendments/stable_run_replay.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_cave_runtime_generation_checkpoints_boss_gates.md

---

# /speckit.specify

## Contexto

A caverna possui implementacao parcial em codigo para run state, seed, niveis, snapshots, materializacao runtime, checkpoints, boss gates, spawn anchors, confinement e debug skip.

Evidencias/parciais relevantes:

```text
Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs
Assets/_Game/Scripts/Cave/**
docs/specs/implementados/spec_cave_002_procedural_resources_parcial.md
docs/specs/implementados/spec_cave_003_stable_run_snapshots_replay_parcial.md
docs/specs/implementados/spec_cave_004_boss_gates_checkpoints_confinement_parcial.md
docs/specs/implementados/spec_cave_005_visual_runtime_materializer_camera_enemy_visuals_parcial.md
docs/specs/implementados/spec_cave_006_spawn_anchor_safe_positioning_parcial.md
docs/specs/implementados/spec_cave_007_snapshot_replay_full_layout_parcial.md
docs/specs/implementados/spec_cave_008_debug_skip_confinement_wall_distance_parcial.md
```

Specs anteriores relevantes:

```text
05 - world activities/fishing/trees/pickups/loot: fishing spot procedural 10% na cave e pickups persistentes
09 - hunger/stamina/status/time: tempo, pause e eventos
10 - equipment/durability/environment/loot: environment zones e resistencias
11 - damage/status/elements/resistances: damage pipeline, vulnerability e damage numbers
12 - player combat/weapons/spells: player combat runtime
13 - enemy AI/roster/bestiary/faction locks: enemy data, spawn resolver, boss hooks, bestiary e XP
```

Esta spec deve consolidar a cave runtime sem invadir a spec 15, que sera dona de morte, retorno, Fonte de Anya, corpse recovery e penalidades.

## Pre-condicoes

Implementar runtime somente depois de specs 02-13 estarem realmente implementadas:

```text
02 - save schema migration
03 - inventory slots/capacity/painel de itens
04 - farm irrigacao/solo/menu contextual
05 - world activities/fishing/trees/pickups/loot
06 - economy/shop/dialogue modal
07 - crafting/workstations/modal stack
08 - town/npc/dialogue
09 - hunger/stamina/status/time
10 - equipment/durability/environment/loot
11 - damage/status/elements/resistances
12 - player combat/weapons/spells/skill actions
13 - enemy AI/roster/bestiary/faction locks
```

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/SceneManagement/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Cave/**
Assets/_Game/Data/Enemies/**
```

Se os sistemas de enemy spawn/bestiary da spec 13 ainda nao estiverem implementados, nao criar spawn resolver paralelo; registrar bloqueio ou usar contrato ja existente.

## Problema

Gaps atuais:

- Cave runtime ainda esta fragmentada entre parciais.
- Snapshot replay precisa garantir que nivel ja visitado nao rerolla dentro da mesma run.
- Checkpoints/boss gates existem parcialmente, mas nao ha UX clara de portal/checkpoint.
- Boss gates precisam bloquear/liberar progresso com estado persistente.
- Boss fights finais ainda nao existem, mas precisamos de boss AI inicial testavel.
- Inimigos de uma run precisam permanecer consistentes, sem reroll injusto.
- Inimigos comuns precisam regenerar depois de 2 dias in-game, sem trocar o conjunto planejado da run.
- Bosses derrotados precisam manter gate/progresso e nao entregar os mesmos itens unicos novamente.
- Em caso de morte do jogador, a run pode redistribuir inimigos, mas sem trocar o conjunto de inimigos da run.
- Fishing spots/resources/enemy plans precisam entrar no snapshot.
- Confinement precisa impedir spawn em parede/fora de area segura.
- Debug skip precisa respeitar gates e nao corromper progresso permanente.
- Camera e materializacao precisam funcionar com bounds reais do nivel.

## Objetivo

Completar cave runtime MVP com:

- modelo macro de 100 niveis;
- biomas por ranges de nivel;
- boss gates/checkpoints em 15, 30, 45, 60, 75 e 90;
- checkpoint portal ao lado da entrada da caverna na fazenda;
- checkpoint portal nos niveis de checkpoint, ao lado do portal da cave;
- menu lateral de checkpoint para teleportar para checkpoints desbloqueados;
- run seed e snapshots estaveis;
- replay identico de nivel ja visitado;
- enemy spawn plan estavel por run/nivel;
- respawn de inimigos comuns derrotados apos 2 dias in-game;
- redistribuicao permitida apos morte, sem trocar os inimigos da run;
- boss gate runtime e boss AI inicial testavel;
- boss rewards/drops unicos controlados para nao repetir o mesmo premio apos gate completo;
- confinement de player, inimigos, resources, pickups e fishing spots;
- save/load de run, snapshots, gates e checkpoints;
- generation testavel por seed + layout hash.

## Decisoes aprovadas

- Cave tem 100 niveis macro.
- Bioma e resolvido por `CaveBiomeResolver` usando ranges do GDD.
- Boss gates/checkpoints existem nos niveis:

```text
15
30
45
60
75
90
```

- Checkpoint deve ser um portal ao lado da entrada da caverna na fazenda.
- Nos niveis de checkpoint, ao lado do portal da cave, tambem existe esse checkpoint portal.
- O checkpoint portal abre um menu lateral para escolher qual checkpoint teleportar.
- O menu lista checkpoints desbloqueados e bloqueados/desabilitados.
- Boss gate runtime entra agora.
- Boss AI inicial entra agora para teste.
- Boss AI final/polish fica futuro.
- Dentro de uma run, os inimigos de um nivel/run nao mudam de identidade/conjunto.
- Inimigos comuns derrotados voltam na mesma run apos 2 dias in-game.
- O respawn de inimigos comuns reativa entradas planejadas do `EnemySpawnPlan`; nao rerolla EnemyIds/faction/roles.
- Em caso de morte do jogador, os inimigos podem ser redistribuidos nas cavernas, mas nao rerollados/trocados.
- Boss derrotado nao reabre o gate e nao concede novamente os mesmos itens unicos/recompensas de gate.
- Boss pode ser reencenado/debugado somente com flag explicita, sem repetir recompensa unica por padrao.
- Reentrar nivel visitado na mesma run usa snapshot, nao reroll.
- Fishing spot procedural 10% por level e maximo 1 por level deve entrar no snapshot.
- Resources nao renovam dentro da mesma run no MVP.
- Debug skip so funciona com flag explicita e nao deve criar progresso permanente acidental.
- Camera deve ficar confinada aos bounds do nivel materializado.

## Modelo de cave e biomas

Modelo macro:

```text
1-10: Caverna de pedra
11-25: Floresta subterranea
26-40: Caverna de gelo
41-55: Caverna de fogo
56-70: Ruinas antigas
71-85: Abismo sombrio
86-99: Nucleo corrompido
100: Boss final/lore futura
```

Boss gates/checkpoints:

```text
15, 30, 45, 60, 75, 90
```

Regras:

- Bioma e data-driven via `CaveBiomeDataSO`/`CaveBiomeRegistrySO`.
- Boss gates sao data-driven via `CaveBossGateDataSO`/`CaveBossGateRegistrySO`.
- A reconciliacao entre bioma GDD e gates runtime deve ficar explicita nos dados.
- Level 100 fica hook futuro, nao boss final completo nesta spec.

## Dados de bioma

Criar/consolidar:

```text
CaveBiomeDataSO
CaveBiomeRegistrySO
CaveBiomeResolver
```

Campos minimos:

```text
BiomeId
DisplayName
LevelMin
LevelMax
TilePaletteId
EnemySpawnProfileIds[]
ResourceSpawnProfileIds[]
EnvironmentHazardIds[]
FishingSpotAllowed
DefaultRoomStyleId opcional
```

## Boss gates e checkpoints

Criar/consolidar:

```text
CaveBossGateDataSO
CaveBossGateRegistrySO
CaveBossGateService
CaveCheckpointService
CaveCheckpointPortal
CaveCheckpointMenuController
```

Campos minimos de `CaveBossGateDataSO`:

```text
GateId
GateLevel
RequiredPreviousGateId opcional
BossEnemyId opcional
CheckpointLevelUnlocked
IsDebugCompletable
UnlocksCheckpointPortalDestination
UniqueRewardIds[] opcional
RepeatableLootTableId opcional
```

Estados de gate:

```text
Locked
Available
BossActive
Completed
```

Regras:

- Gate bloqueia avanco para alem do nivel se nao estiver completed/liberado.
- Derrotar boss inicial/test ou receber `CaveBossDefeatedEvent` marca gate como completed.
- Gate completed desbloqueia checkpoint correspondente.
- Checkpoint desbloqueado aparece habilitado no menu lateral do checkpoint portal.
- Checkpoints bloqueados podem aparecer desabilitados com feedback simples.
- `UniqueRewardIds[]` de boss/gate so podem ser concedidos uma vez por save/progresso permanente.
- Loot repetivel de boss, se existir no futuro, deve usar tabela separada e nao repetir itens unicos.

## Checkpoint portal e menu lateral

### Portal na fazenda

Na fazenda, ao lado da entrada da caverna, deve existir:

```text
CaveCheckpointPortal_FarmEntrance
```

Comportamento:

- Player interage com `E`.
- Abre menu lateral de checkpoints.
- Lista checkpoints desbloqueados.
- Selecionar checkpoint teleporta/inicia carregamento da cave naquele nivel.
- Se nenhum checkpoint desbloqueado, mostrar somente entrada padrao ou mensagem clara.
- Menu lateral respeita modal stack e pausa tempo/logica de gameplay.

### Portal nos niveis de checkpoint

Nos niveis de checkpoint, ao lado do portal da cave, deve existir:

```text
CaveCheckpointPortal_Level15
CaveCheckpointPortal_Level30
CaveCheckpointPortal_Level45
CaveCheckpointPortal_Level60
CaveCheckpointPortal_Level75
CaveCheckpointPortal_Level90
```

Comportamento:

- Abre o mesmo menu lateral.
- Permite teleportar para checkpoints desbloqueados.
- Nao deve burlar boss gates bloqueados.
- Nao deve alterar run seed sem regra explicita.

### Menu lateral

Criar/usar:

```text
CaveCheckpointSideMenu
```

Mostrar:

```text
Checkpoint level
Biome name
Status: Locked/Unlocked/Current
Opcao de teleportar
Feedback de bloqueio
```

Input minimo:

```text
W/S navegam
E/Enter/Space confirma
Esc fecha
```

Regras:

- Menu e modal/interativo.
- Nao sobrepor Dialogue/Shop/Crafting/Inventory sem controle do modal stack.
- Teleporte so executa se checkpoint estiver unlocked.
- Teleporte deve publicar evento de transicao, nao mover player silenciosamente sem runtime state.

## Cave run identity

Criar/consolidar:

```text
CaveRunId
CaveSeed
StartedAtGameDay
CurrentLevel
RunStatus
```

RunStatus:

```text
NotStarted
Active
Completed
Abandoned
Failed
```

Regras:

- Toda run ativa possui `RunId` e `Seed` estaveis.
- Save deve preservar run ativa.
- Snapshot pertence a `RunId + LevelIndex`.
- Nova run pode usar nova seed, mas nao deve apagar progresso permanente de checkpoints, boss gates completed ou boss unique rewards ja concedidos.

## Snapshot replay

Regra central:

```text
Entrar em nivel ja visitado na mesma run usa CaveLevelSnapshot existente.
Nao rerolla layout, resources, fishing spot ou enemy spawn plan.
```

`CaveLevelSnapshot` minimo:

```text
RunId
Seed
LevelIndex
BiomeId
LayoutHash
RoomGraphData
TileLayoutData ou LayoutDescriptor
SpawnAnchors[]
PlayerSafeSpawnAnchorId
ExitPortalAnchorId
CheckpointPortalAnchorId opcional
BossGateState opcional
FishingSpotState opcional
ResourceNodeStates[]
PickupStates[]
EnemySpawnPlan[]
EnemyRedistributionState opcional
EnemyRespawnState opcional
EnvironmentZoneStates[]
```

Regras:

- Snapshot deve conter dados suficientes para replay visual e funcional identico.
- LayoutHash deve ser gerado e comparavel em testes.
- Snapshot nao serializa referencias Unity.
- Snapshot salva IDs/posicoes/dados simples.

## Enemy spawn plan, respawn e redistribuicao por morte

Dentro de uma run:

- O conjunto/identidade dos inimigos sorteados para um nivel deve permanecer estavel.
- `EnemySpawnPlan` nao deve trocar EnemyIds ao revisitar o nivel.
- Inimigos comuns derrotados voltam depois de 2 dias in-game.
- Respawn significa reativar a mesma entrada planejada de inimigo, nao gerar outro inimigo.
- Se o jogador morrer, a cave pode redistribuir os inimigos ao retornar.
- Redistribuicao significa escolher novos anchors/posicoes validas para os mesmos inimigos planejados, nao rerollar EnemyIds/faction/roles.

Dados sugeridos:

```text
EnemySpawnPlanEntry
- PlannedEnemyInstanceId
- EnemyId
- InitialAnchorId
- CurrentAnchorId opcional
- IsDefeated
- DefeatedAtGameDay opcional
- RespawnAvailableAtGameDay opcional
- IsBoss
- UniqueDropsClaimed opcional
- RedistributionGroupId opcional
```

```text
EnemyRedistributionState
- RedistributionCount
- LastRedistributionReason
- RedistributionSeedOffset
```

```text
EnemyRespawnState
- RespawnDelayGameDays default 2
- LastRespawnEvaluationDay
```

Regras:

- Inimigo comum derrotado fica indisponivel ate `RespawnAvailableAtGameDay`.
- Ao passar 2 dias in-game, inimigo comum pode voltar na mesma run usando o mesmo `PlannedEnemyInstanceId` e `EnemyId`.
- Respawn deve escolher anchor seguro e pode respeitar redistribuicao atual.
- Respawn deve publicar evento e nao duplicar inimigo ja ativo.
- Boss derrotado nao respawna como gate boss normal.
- Boss derrotado pode permanecer em estado `Completed/Defeated` e nao deve conceder novamente seus itens unicos.
- Redistribuicao so ocorre por evento explicito, por exemplo futura morte do jogador na spec 15.
- Redistribuicao deve respeitar confinement e spawn anchors validos.
- Se enemy runtime save/snapshot da spec 13 ainda nao estiver completo, salvar pelo menos `EnemyId`, planned id, anchor, defeated state e respawn day.

## Boss AI inicial

Esta spec implementa boss AI inicial para teste de boss gates.

Criar/usar:

```text
CaveBossController
CaveBossAIProfileSO
CaveBossEncounterService
```

Boss AI MVP:

```text
Idle
Aggro
AttackWindup
AttackRecover
Vulnerable
Dead
```

Actions minimas:

```text
BossMeleeSmash
BossProjectileOrPulse
```

Regras:

- Boss usa `EnemyDataSO`/`EnemyActionSO` quando possivel.
- Boss damage passa pelo `DamageCalculator` da spec 11.
- Boss usa telegraph blink/cor da spec 13 durante windup.
- Boss possui vulnerability window apos ataque ou recover.
- Boss death publica `CaveBossDefeatedEvent`.
- `CaveBossDefeatedEvent` completa o gate associado e desbloqueia checkpoint.
- Boss unique rewards/drops sao concedidos uma vez e marcados como claimed.
- Boss AI final, fases complexas e arena final ficam futuro.

## Resources e fishing spot na cave

### Fishing spot

Integrar regra da spec 05:

```text
10% chance por cave level
Maximo 1 fishing spot por level quando cair nos 10%
```

Regras:

- Resultado da rolagem entra no snapshot.
- Revisitar nivel nao rerolla fishing spot.
- Fishing spot deve usar safe anchor e nao spawnar em parede.

### Resources

MVP:

- Resources da cave nao renovam dentro da mesma run.
- Coleta/estado de resource entra no snapshot.
- Nova run pode gerar novos resources conforme seed nova.
- Hook futuro: `CaveResourceRenewalPolicy` por dia/nivel.

## Confinement e safe spawning

Aplicar a:

```text
Player
Enemies
Boss
Resources
Pickups
Fishing spots
Portals
Checkpoint portals
Environment zones
```

Regras:

- Nada deve spawnar dentro de parede.
- Nada deve spawnar fora da area navegavel.
- Respeitar `MinDistanceFromWall`.
- Respeitar room bounds.
- Nao sobrepor portal/gate/entrada.
- Huge/Boss nao spawnam em sala pequena.
- Validar collider/footprint por size profile da spec 13.

Criar/usar:

```text
CaveSpawnAnchorService
CaveConfinementValidator
CaveSafeSpawnResolver
```

## Materializacao runtime

`CaveRuntimeMaterializer` deve:

- materializar layout por snapshot;
- criar tiles/placeholders de bioma;
- criar portals/gates/checkpoint portals;
- criar resources/fishing/enemy spawns conforme snapshot;
- configurar bounds de camera;
- falhar com erro claro se registry critico estiver ausente.

Regras:

- Arte final nao e obrigatoria.
- Missing visual opcional nao deve quebrar runtime.
- Missing critical data deve bloquear com log claro.
- Nao usar fallback silencioso que minta sucesso.

## Camera bounds

Criar/usar:

```text
CaveCameraBounds
CaveCameraConfinementService
```

Regras:

- Camera fica confinada ao bounds do nivel materializado.
- Camera polish final fica fora.
- Bounds vem do layout/snapshot materializado.

## Debug skip

Criar/usar:

```text
CaveDebugSkipService
```

Regras:

- So funciona se `EnableDebugSkip = true`.
- `SkipToLevel` nao desbloqueia checkpoint permanente por si so.
- `UnlockGateForDebug` exige flag explicita.
- `CompleteBossForDebug` exige flag explicita e publica evento/log.
- `CompleteBossForDebug` nao deve conceder unique rewards sem flag explicita separada.
- Toda acao debug deve registrar `CaveDebugSkipUsedEvent`.
- Debug skip nao pode corromper run state.

## Save/load

Persistir usando DTOs simples:

```text
CaveSaveData
- ActiveRun
- UnlockedCheckpoints[]
- BossGateStates[]
- BossUniqueRewardsClaimed[]

CaveRunSaveData
- RunId
- Seed
- StartedAtGameDay
- CurrentLevel
- RunStatus
- LevelSnapshots[]

CaveLevelSnapshotSaveData
- RunId
- LevelIndex
- BiomeId
- LayoutHash
- RoomGraphData
- SpawnAnchors[]
- ResourceNodeStates[]
- PickupStates[]
- FishingSpotState
- EnemySpawnPlan[]
- EnemyRedistributionState
- EnemyRespawnState
- EnvironmentZoneStates[]

CaveCheckpointSaveData
- CheckpointLevel
- IsUnlocked
- UnlockedByGateId

CaveBossGateSaveData
- GateId
- GateLevel
- State
- BossEnemyId
- IsCompleted
- UniqueRewardsClaimed[]
```

Regras:

- Nunca serializar ScriptableObject, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.
- Se snapshot estiver invalido/corrompido, nao sobrescrever save bom sem backup.
- Save migration da spec 02 deve ser usada para novos campos.

## Eventos

Criar/usar eventos oficiais:

```text
CaveRunStartedEvent
CaveRunLoadedEvent
CaveLevelGeneratedEvent
CaveLevelMaterializedEvent
CaveLevelSnapshotCreatedEvent
CaveLevelSnapshotLoadedEvent
CaveLevelTransitionRequestedEvent
CaveLevelTransitionBlockedEvent
CaveCheckpointPortalOpenedEvent
CaveCheckpointTeleportRequestedEvent
CaveCheckpointTeleportCompletedEvent
CaveCheckpointUnlockedEvent
CaveBossGateLockedEvent
CaveBossGateUnlockedEvent
CaveBossGateCompletedEvent
CaveBossSpawnedEvent
CaveBossDefeatedEvent
CaveBossUniqueRewardClaimedEvent
CaveEnemyRespawnScheduledEvent
CaveEnemyRespawnedEvent
CaveEnemiesRedistributedEvent
CaveDebugSkipUsedEvent
```

Nao duplicar eventos com mesmo significado se ja existirem.

## UI minima

- Checkpoint side menu entra agora.
- UI final da cave fica spec 17.
- Boss health UI final fica futuro; debug/minimo permitido se refletir runtime real.
- Menu lateral deve respeitar modal stack.

## Testabilidade

Implementar/testar:

```text
GenerateLevel(seed, levelIndex)
ComputeLayoutHash(snapshot)
ReenterLevel(runId, levelIndex)
Assert layout hash igual
```

Cenarios:

- gerar nivel N com seed X;
- salvar snapshot;
- sair/reentrar nivel N;
- comparar `LayoutHash`;
- validar que fishing spot/resource/enemy plan nao rerollou;
- matar inimigo comum;
- avancar 2 dias in-game;
- validar respawn do mesmo EnemyId/PlannedEnemyInstanceId;
- simular morte/redistribuicao;
- validar mesmos EnemyIds com anchors redistribuidos;
- matar boss;
- validar gate/checkpoint e reward unico sem repeticao.

## Invariantes anti-regressao

Esta spec nao pode quebrar:

- Fishing spot cave 10% da spec 05;
- pickups persistentes da spec 05;
- time/pause da spec 09;
- environmental zones/resistencias da spec 10;
- damage/combat/status da spec 11;
- player combat da spec 12;
- enemy spawn resolver/EnemyDataSO/bestiary da spec 13;
- future cave entry/death/corpse recovery da spec 15;
- save migration e DTOs simples;
- modal stack;
- GameEventBus;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

Se alguma dependencia ainda nao existir, registrar bloqueio claro em vez de criar sistema paralelo.

## Criterios de aceite

- Cave possui 100 niveis macro com biomas por resolver data-driven.
- Boss gates/checkpoints existem em 15/30/45/60/75/90.
- Existe checkpoint portal ao lado da entrada da caverna na fazenda.
- Existem checkpoint portals nos niveis de checkpoint ao lado do portal da cave.
- Checkpoint portal abre menu lateral com checkpoints bloqueados/desbloqueados.
- Selecionar checkpoint desbloqueado teleporta/carrega o nivel correto.
- Gate bloqueia progresso enquanto locked.
- Boss AI inicial testavel existe.
- Derrotar boss inicial/test publica evento e libera gate/checkpoint.
- Boss derrotado nao concede novamente os mesmos itens unicos/recompensas de gate.
- Run possui RunId e Seed estaveis.
- Nivel visitado usa snapshot ao revisitar, sem reroll.
- LayoutHash se mantem igual no replay do snapshot.
- Fishing spot procedural 10% e maximo 1 por level entram no snapshot.
- Resources nao renovam dentro da mesma run no MVP.
- EnemySpawnPlan mantem os mesmos inimigos da run/nivel.
- Inimigos comuns derrotados voltam apos 2 dias in-game usando o mesmo EnemyId/PlannedEnemyInstanceId.
- Apos morte do jogador/evento futuro, inimigos podem ser redistribuidos sem trocar EnemyIds.
- Player/enemies/resources/pickups/fishing/portals respeitam confinement.
- Camera fica confinada ao bounds materializado.
- Debug skip respeita flags e nao gera progresso permanente acidental.
- Save/load preserva run, snapshots, checkpoints, boss gates, respawn de inimigos e boss unique rewards claimed.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/SceneManagement/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/UI/Cave/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Cave/**
Assets/_Game/Data/Enemies/**
```

Managers/bridges Unity devem ser finos. Generation, snapshot, boss gate, checkpoint, confinement e spawn resolution devem ficar em classes testaveis quando possivel.

## Ordem segura de implementacao

1. Revalidar cave parciais, save, scene installers, boss gate registry, materializer e snapshot atual.
2. Confirmar specs 02-13 implementadas antes de runtime.
3. Consolidar biome/gate/checkpoint data.
4. Implementar checkpoint portal + side menu.
5. Consolidar CaveRunId/CaveSeed/RunStatus.
6. Consolidar snapshot replay e LayoutHash.
7. Integrar fishing spot/resource/enemy plan ao snapshot.
8. Implementar enemy respawn state com delay de 2 dias in-game.
9. Implementar enemy redistribution state sem reroll de EnemyIds.
10. Implementar boss AI inicial, gate completion event e unique reward claim tracking.
11. Implementar confinement/safe spawn validators.
12. Implementar materialization e camera bounds.
13. Implementar debug skip seguro.
14. Implementar save/load.
15. Validar anti-regressao e atualizar tracking.

## Fluxos

### Abrir checkpoint portal

```text
Player pressiona E no CaveCheckpointPortal
Abrir CaveCheckpointSideMenu
Listar checkpoints locked/unlocked/current
Selecionar checkpoint unlocked
Publicar CaveCheckpointTeleportRequestedEvent
Carregar/materializar nivel correspondente
Publicar CaveCheckpointTeleportCompletedEvent
```

### Gerar ou carregar nivel

```text
Request level N da run R
Se snapshot R+N existe, carregar snapshot
Se nao existe, gerar por seed, criar snapshot e layout hash
Materializar snapshot
Configurar camera bounds e portals
```

### Boss gate

```text
Player tenta avancar alem de gate locked
Bloquear transicao e publicar CaveLevelTransitionBlockedEvent
Boss inicial/test e spawnado no gate level
Boss derrotado publica CaveBossDefeatedEvent
Gate marca Completed
Checkpoint e desbloqueado
Unique rewards sao marcados como claimed e nao repetem
```

### Respawn de inimigo comum

```text
Enemy comum derrotado
Marcar IsDefeated=true e RespawnAvailableAtGameDay=CurrentGameDay+2
Ao avaliar cave/day transition/reentrada
Se CurrentGameDay >= RespawnAvailableAtGameDay
Reativar mesmo PlannedEnemyInstanceId/EnemyId em safe anchor valido
Publicar CaveEnemyRespawnedEvent
```

### Redistribuir inimigos apos morte

```text
Receber evento futuro de morte/retorno da spec 15
Para cada EnemySpawnPlanEntry aplicavel
Escolher novo safe anchor valido
Atualizar CurrentAnchorId/RedistributionState
Nao alterar EnemyId
Publicar CaveEnemiesRedistributedEvent
```

## Riscos de regressao

- Reentrar nivel rerollar layout.
- Checkpoint portal burlar gate locked.
- Debug skip desbloquear progresso permanente sem flag.
- Redistribuicao trocar inimigos e quebrar run fairness.
- Respawn duplicar inimigo ativo.
- Boss derrotado conceder recompensa unica repetida.
- Snapshot serializar referencias Unity.
- Boss AI duplicar sistema de enemy AI da spec 13.
- Fishing spot procedural rerollar ao revisitar nivel.
- Camera bounds ausente deixar player fora de visao.

## Mitigacao

- Snapshot chaveado por RunId + LevelIndex.
- Teleporte valida checkpoint unlocked.
- Debug flags explicitas.
- EnemySpawnPlan com EnemyIds estaveis e PlannedEnemyInstanceId.
- Respawn por estado e dia, com validacao de instancia ativa.
- Unique rewards claimed persistidos por GateId/RewardId.
- DTOs simples.
- Boss AI reaproveita EnemyDataSO/EnemyActionSO quando possivel.
- FishingSpotState persistido no snapshot.
- Camera bounds gerado do layout.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar CaveRunManager, CaveRuntimeMaterializer, snapshots, boss gates, checkpoints e debug skip existentes.
- [ ] Confirmar specs 02-13 implementadas antes de runtime.
- [ ] Criar/ajustar `CaveBiomeDataSO`, `CaveBiomeRegistrySO`, `CaveBiomeResolver`.
- [ ] Criar/ajustar `CaveBossGateDataSO`, `CaveBossGateRegistrySO`, `CaveBossGateService`.
- [ ] Criar/ajustar `CaveCheckpointService`.
- [ ] Criar checkpoint portal na fazenda ao lado da entrada da cave.
- [ ] Criar checkpoint portals nos niveis 15/30/45/60/75/90.
- [ ] Criar `CaveCheckpointSideMenu`.
- [ ] Implementar teleport para checkpoint desbloqueado.
- [ ] Consolidar `CaveRunId`, `CaveSeed`, `RunStatus`.
- [ ] Implementar snapshot replay por `RunId + LevelIndex`.
- [ ] Implementar `LayoutHash` e teste de replay.
- [ ] Persistir fishing spot procedural no snapshot.
- [ ] Persistir resources/pickups no snapshot.
- [ ] Integrar `EnemySpawnPlan` da spec 13 ao snapshot.
- [ ] Implementar respawn de inimigos comuns apos 2 dias in-game.
- [ ] Implementar redistribuicao de inimigos sem reroll de EnemyIds.
- [ ] Implementar boss AI inicial testavel.
- [ ] Integrar boss death com gate/checkpoint unlock.
- [ ] Persistir/validar boss unique rewards claimed.
- [ ] Implementar confinement/safe spawn validators.
- [ ] Implementar materialization e camera bounds.
- [ ] Implementar debug skip seguro.
- [ ] Implementar save/load de run, snapshots, gates, checkpoints, enemy respawn e boss unique rewards.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/SceneManagement/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Enemy/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/UI/Cave/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Cave/**
Assets/_Game/Data/Enemies/**
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
```

## Definition of Done

- Cave runtime completo para run/snapshot/checkpoint/gate MVP.
- Checkpoint portal e menu lateral funcionam.
- Boss AI inicial libera gate/checkpoint ao derrotar.
- Boss unique rewards nao repetem.
- Snapshot replay, enemy plan estavel e respawn de inimigos comuns funcionam.
- Confinement/materialization/camera bounds validos.
- Save/load preserva run/gates/checkpoints/snapshots/respawn/rewards.
- Invariantes anti-regressao preservadas.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

1. Abrir FarmScene e validar checkpoint portal ao lado da entrada da cave.
2. Interagir com checkpoint portal e validar menu lateral.
3. Validar checkpoints locked/unlocked/current no menu.
4. Entrar CaveScene via entrada normal e criar run com seed.
5. Gerar nivel 1 e registrar LayoutHash.
6. Avancar/voltar para nivel ja visitado e validar mesmo LayoutHash.
7. Validar fishing spot procedural salvo em snapshot sem reroll.
8. Validar resources/pickups salvos em snapshot sem renovacao na mesma run.
9. Validar EnemySpawnPlan estavel por nivel.
10. Matar inimigo comum e validar respawn agendado para CurrentGameDay+2.
11. Avancar 2 dias in-game e validar respawn do mesmo EnemyId/PlannedEnemyInstanceId.
12. Simular redistribuicao por morte e validar mesmos EnemyIds em novos anchors.
13. Forcar boss gate locked e validar bloqueio de avanco.
14. Spawnar boss AI inicial e validar telegraph/ataques basicos/vulnerability.
15. Derrotar boss e validar `CaveBossDefeatedEvent`, gate completed, checkpoint unlocked e unique rewards claimed.
16. Tentar repetir boss/reward e validar que mesmos itens unicos nao sao concedidos novamente.
17. Usar checkpoint portal para teleportar ao checkpoint desbloqueado.
18. Validar player/enemy/resource/fishing/portal safe spawn e confinement.
19. Validar camera bounds no nivel materializado.
20. Validar debug skip com flag off/on e logs.
21. Salvar/carregar run, checkpoints, boss gates, snapshots, respawn e rewards claimed.
