# refinamento_init_cave_runtime_generation_checkpoints_boss_gates

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md`
> Objetivo: completar cave runtime, 100 niveis macro, snapshots/replay, checkpoint portals, menu lateral, boss gates, boss AI inicial, confinement, materializacao, camera bounds, enemy spawn plan estavel e save/load.

---

## 1. Estado atual

A caverna possui implementacao parcial em codigo para run state, seed, niveis, snapshots, materializacao runtime, checkpoints, boss gates, spawn anchors, confinement e debug skip.

Evidencias principais:

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

---

## 2. Gaps

- Unity Play Mode ainda precisa validar materializacao real da cena.
- Checkpoints/boss gates existem em codigo, mas precisam de UX clara via portal/menu.
- Boss gates precisam bloquear/liberar progresso com estado persistente.
- Boss AI inicial ainda nao existe como teste runtime confiavel.
- Spawn pools por cave band/faction/bioma precisam integrar com spec 13.
- Snapshot replay/backtracking precisa teste comparando layout antes/depois.
- Inimigos precisam permanecer estaveis dentro da run, com redistribuicao permitida por morte sem trocar EnemyIds.
- Fishing spots/resources/enemy plans precisam entrar no snapshot.
- Debug skip deve respeitar confinement e nao corromper run state.
- `CaveBossGateRegistrySO` ou referencia equivalente precisa estar garantida em Resources/Inspector/installer.

---

## 3. Decisoes aprovadas

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
- Em caso de morte do jogador, os inimigos podem ser redistribuidos nas cavernas, mas nao rerollados/trocados.
- Reentrar nivel visitado na mesma run usa snapshot, nao reroll.
- Fishing spot procedural 10% por level e maximo 1 por level deve entrar no snapshot.
- Resources nao renovam dentro da mesma run no MVP.
- Debug skip so funciona com flag explicita e nao deve criar progresso permanente acidental.
- Camera deve ficar confinada aos bounds do nivel materializado.

---

## 4. Checkpoint portal e menu lateral

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

---

## 5. Cave run identity

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
- Nova run pode usar nova seed, mas nao deve apagar progresso permanente de checkpoints.

---

## 6. Snapshot replay

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
EnvironmentZoneStates[]
```

---

## 7. Enemy spawn plan e redistribuicao por morte

Dentro de uma run:

- O conjunto/identidade dos inimigos sorteados para um nivel deve permanecer estavel.
- `EnemySpawnPlan` nao deve trocar EnemyIds ao revisitar o nivel.
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
- RedistributionGroupId opcional
```

```text
EnemyRedistributionState
- RedistributionCount
- LastRedistributionReason
- RedistributionSeedOffset
```

Regras:

- Redistribuicao so ocorre por evento explicito, por exemplo futura morte do jogador na spec 15.
- Redistribuicao deve respeitar confinement e spawn anchors validos.
- Inimigo derrotado nao deve voltar na mesma run, salvo regra futura explicita.

---

## 8. Boss AI inicial

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
- Boss AI final, fases complexas e arena final ficam futuro.

---

## 9. Resources e fishing spot na cave

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

---

## 10. Confinement e safe spawning

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

---

## 11. Materializacao runtime e camera

`CaveRuntimeMaterializer` deve:

- materializar layout por snapshot;
- criar tiles/placeholders de bioma;
- criar portals/gates/checkpoint portals;
- criar resources/fishing/enemy spawns conforme snapshot;
- configurar bounds de camera;
- falhar com erro claro se registry critico estiver ausente.

Camera:

- Camera fica confinada ao bounds do nivel materializado.
- Camera polish final fica fora.
- Bounds vem do layout/snapshot materializado.

---

## 12. Debug skip

Criar/usar:

```text
CaveDebugSkipService
```

Regras:

- So funciona se `EnableDebugSkip = true`.
- `SkipToLevel` nao desbloqueia checkpoint permanente por si so.
- `UnlockGateForDebug` exige flag explicita.
- `CompleteBossForDebug` exige flag explicita e publica evento/log.
- Toda acao debug deve registrar `CaveDebugSkipUsedEvent`.
- Debug skip nao pode corromper run state.

---

## 13. Save/load

Persistir usando DTOs simples:

```text
CaveSaveData
- ActiveRun
- UnlockedCheckpoints[]
- BossGateStates[]

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
```

Nunca serializar ScriptableObject, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.

---

## 14. Eventos

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
CaveEnemiesRedistributedEvent
CaveDebugSkipUsedEvent
```

---

## 15. Definition of Done

- [ ] Cave possui 100 niveis macro com biomas por resolver data-driven.
- [ ] Boss gates/checkpoints existem em 15/30/45/60/75/90.
- [ ] Existe checkpoint portal ao lado da entrada da caverna na fazenda.
- [ ] Existem checkpoint portals nos niveis de checkpoint ao lado do portal da cave.
- [ ] Checkpoint portal abre menu lateral com checkpoints bloqueados/desbloqueados.
- [ ] Selecionar checkpoint desbloqueado teleporta/carrega o nivel correto.
- [ ] Gate bloqueia progresso enquanto locked.
- [ ] Boss AI inicial testavel existe.
- [ ] Derrotar boss inicial/test publica evento e libera gate/checkpoint.
- [ ] Run possui RunId e Seed estaveis.
- [ ] Nivel visitado usa snapshot ao revisitar, sem reroll.
- [ ] LayoutHash se mantem igual no replay do snapshot.
- [ ] Fishing spot procedural 10% e maximo 1 por level entram no snapshot.
- [ ] Resources nao renovam dentro da mesma run no MVP.
- [ ] EnemySpawnPlan mantem os mesmos inimigos da run/nivel.
- [ ] Apos morte do jogador/evento futuro, inimigos podem ser redistribuidos sem trocar EnemyIds.
- [ ] Player/enemies/resources/pickups/fishing/portals respeitam confinement.
- [ ] Camera fica confinada ao bounds materializado.
- [ ] Debug skip respeita flags e nao gera progresso permanente acidental.
- [ ] Save/load preserva run, snapshots, checkpoints e boss gates.

---

## 16. Validacao

1. Abrir FarmScene e validar checkpoint portal ao lado da entrada da cave.
2. Interagir com checkpoint portal e validar menu lateral.
3. Validar checkpoints locked/unlocked/current no menu.
4. Entrar CaveScene via entrada normal e criar run com seed.
5. Gerar nivel 1 e registrar LayoutHash.
6. Avancar/voltar para nivel ja visitado e validar mesmo LayoutHash.
7. Validar fishing spot procedural salvo em snapshot sem reroll.
8. Validar resources/pickups salvos em snapshot sem renovacao na mesma run.
9. Validar EnemySpawnPlan estavel por nivel.
10. Simular redistribuicao por morte e validar mesmos EnemyIds em novos anchors.
11. Forcar boss gate locked e validar bloqueio de avanco.
12. Spawnar boss AI inicial e validar telegraph/ataques basicos/vulnerability.
13. Derrotar boss e validar `CaveBossDefeatedEvent`, gate completed e checkpoint unlocked.
14. Usar checkpoint portal para teleportar ao checkpoint desbloqueado.
15. Validar player/enemy/resource/fishing/portal safe spawn e confinement.
16. Validar camera bounds no nivel materializado.
17. Validar debug skip com flag off/on e logs.
18. Salvar/carregar run, checkpoints, boss gates e snapshots.
19. Validar Unity compile validation e docs validation.
