# SPEC 14B - Cave Snapshot Replay with EnemySpawnPlan

> Spec ID: spec_14b_cave_snapshot_replay_enemy_plan
> Status: A implementar
> Ordem de execucao: 14B
> Tipo: Cave Runtime / Snapshot Replay / Stable Level State / Enemy Plan Persistence
> Fonte de refinamento: `docs/refinements/a_implementar/pre_refinamentos/refinamento_spec14b_cave_snapshot_replay_enemy_plan.md`
> Spec ampla relacionada: `docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md`
> Depende de: SPEC 14A implementada em codigo.
> Fora de escopo: respawn comum 2 dias, redistribuicao pos-morte, boss fights/rewards, checkpoint UI, cave UI final, save runtime individual completo de inimigo.

---

# /speckit.specify

## O QUE

Implementar snapshot/replay funcional para níveis da cave já visitados na mesma run.

Ao entrar em um nível:

```text
se snapshot existe para CaveRunSeed + CaveLevel:
    restaurar o nível a partir do snapshot
senão:
    gerar nível novo
    gerar ResourceNodes/FishingSpot/EnemySpawnPlan
    capturar snapshot
    materializar
```

O snapshot deve conter dados suficientes para evitar reroll de:

- layout;
- entrance/exit;
- resources;
- fishing spot;
- enemy spawn plan.

## POR QUE

A SPEC 14A conecta os inimigos à cave por seed. Porém a SPEC 14 ampla exige que níveis visitados sejam replay estável, sem reroll invisível.

Sem snapshot funcional:

- o jogador pode voltar ao mesmo nível e ver layout diferente;
- resources podem reaparecer ou mudar;
- fishing spot pode rerollar;
- inimigos planejados podem mudar;
- save/load pode reconstruir algo diferente do que o jogador já viu.

## ESCOPO

Inclui:

- `CaveLevelSnapshot` funcional/consolidado;
- `CaveSnapshotService` ou equivalente;
- `LayoutHash` determinístico;
- snapshot contendo `EnemySpawnPlan`;
- snapshot contendo resources;
- snapshot contendo fishing spot hook/estado;
- integração com `CaveRunManager.State.VisitedLevelSnapshots`;
- integração com `CaveSaveData`;
- materialização a partir de snapshot;
- validator e documentação.

Não inclui:

- respawn comum depois de 2 dias;
- redistribuição pós-morte;
- boss fight;
- boss rewards;
- UI de checkpoint;
- UI de cave final;
- estado runtime completo individual de HP/AI de cada inimigo;
- fishing minigame final.

---

# /speckit.clarify

## Decisões fechadas

| Tema | Decisão |
|---|---|
| Snapshot key | CaveRunSeed + CaveLevel |
| Snapshot inclui layout | Sim |
| Snapshot inclui EnemySpawnPlan | Sim |
| Snapshot inclui resources | Sim |
| Snapshot inclui fishing spot | Sim, hook/estado mínimo |
| Snapshot inclui inimigo derrotado/respawn | Não nesta spec; fica para 14C |
| Snapshot inclui redistribuição pós-morte | Não; fica para 14D |
| Reentrar nível visitado rerolla? | Não |
| Save/load preserva snapshots? | Sim |
| LayoutHash | Determinístico e comparável |

---

# /speckit.plan

## Arquitetura alvo

```text
CaveRunManager
└── CaveRuntimeState
    └── VisitedLevelSnapshots[level] = CaveLevelSnapshot

CaveSnapshotService
├── TryGetSnapshot(runSeed, level)
├── CaptureSnapshot(generatedLevel, enemySpawnPlan, resources, fishing)
├── RestoreGeneratedLevel(snapshot)
├── CalculateLayoutHash(snapshot/generatedLevel)
└── Save/restore bridge

CaveLevelRuntimeController / Cave entry flow
├── Enter level
├── Check snapshot
├── Generate or restore
└── Materialize

CaveRuntimeMaterializer
├── Materialize(generatedLevel)
└── Materialize snapshot-equivalent generated level + EnemySpawnPlan
```

## Contratos

### CaveLevelSnapshot

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
CaveFishingSpotSnapshotEntry FishingSpotState
CaveEnemySpawnPlan EnemySpawnPlan
List<string> Warnings
```

Se já existir contrato parcial, migrar/expandir sem quebrar save.

### CaveResourceNodeSnapshotEntry

Campos mínimos:

```text
string NodeInstanceId
string ResourceNodeId
Vector2Int GridPosition
bool IsDepleted
```

### CaveFishingSpotSnapshotEntry

Campos mínimos:

```text
bool HasFishingSpot
string FishingSpotId
Vector2Int GridPosition
string FishingProfileId
```

### CaveSnapshotService

Responsabilidades:

- chavear snapshot por run+level;
- criar snapshot inicial;
- restaurar level a partir de snapshot;
- calcular layout hash;
- validar replay;
- não serializar referências Unity.

## LayoutHash

Hash deve incluir, em ordem estável:

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

Regras:

- ordenar listas antes do hash;
- evitar `GetHashCode()` nativo para persistência cross-session;
- usar algoritmo estável simples, por exemplo SHA256 de string canônica, se disponível;
- registrar hash no snapshot.

---

# /speckit.tasks

## SPEC14B-C01 — Revalidar contratos existentes

- [ ] Ler `CaveRunManager`.
- [ ] Ler `CaveRuntimeState`.
- [ ] Ler `CaveSaveData`.
- [ ] Ler contratos parciais de snapshot existentes.
- [ ] Ler `CaveGeneratedLevel`.
- [ ] Ler `CaveRuntimeMaterializer`.
- [ ] Ler SPEC 14A e contratos `CaveEnemySpawnPlan`.

## SPEC14B-C02 — Consolidar CaveLevelSnapshot

- [ ] Criar/expandir `CaveLevelSnapshot`.
- [ ] Garantir campos de layout.
- [ ] Garantir fields para resources.
- [ ] Garantir field para fishing spot.
- [ ] Garantir field para EnemySpawnPlan.
- [ ] Garantir DTO com tipos simples.

## SPEC14B-C03 — CaveSnapshotService

- [ ] Criar `CaveSnapshotService` ou equivalente.
- [ ] Implementar `BuildSnapshotId`.
- [ ] Implementar `CaptureSnapshot`.
- [ ] Implementar `RestoreGeneratedLevel`.
- [ ] Implementar `CalculateLayoutHash`.
- [ ] Implementar validação de replay/hash.

## SPEC14B-C04 — Integração com CaveRunManager/CaveRuntimeState

- [ ] Usar `VisitedLevelSnapshots` como fonte de snapshots visitados.
- [ ] Ao entrar nível novo, salvar snapshot.
- [ ] Ao revisitar nível, recuperar snapshot.
- [ ] Save/load preserva snapshots.
- [ ] Não apagar snapshots salvo nova run/morte/regra explícita existente.

## SPEC14B-C05 — Integração com materialização

- [ ] Permitir materializar a partir de snapshot.
- [ ] Não chamar geração procedural nova ao revisitar nível com snapshot.
- [ ] Não chamar EnemySpawnResolver novamente quando snapshot já possui EnemySpawnPlan.
- [ ] Materializar resources/fishing/enemies conforme snapshot.

## SPEC14B-C06 — Fishing spot hook

- [ ] Implementar estado mínimo de fishing spot no snapshot.
- [ ] Aplicar regra 10% por cave level, máximo 1 por level, se ainda não estiver consolidada.
- [ ] Resultado não rerolla ao revisitar snapshot.
- [ ] Se fishing runtime ainda não existir, manter hook e registrar pendência.

## SPEC14B-C07 — Validator

Criar:

```text
Assets/_Game/Scripts/Editor/Validation/ValidateSpec14BCaveSnapshotReplay.cs
```

Verificar:

- `CaveLevelSnapshot` existe;
- snapshot possui EnemySpawnPlan;
- snapshot possui ResourceNodeStates;
- snapshot possui FishingSpotState ou hook equivalente;
- `CaveSnapshotService` existe;
- `LayoutHash` é calculado de forma determinística;
- `VisitedLevelSnapshots` é usado;
- save/load preserva snapshots;
- não há Unity refs em DTOs;
- não há busca global proibida runtime.

## SPEC14B-DOC — Documento de validação

Criar:

```text
docs/validation/SPEC14B_CAVE_SNAPSHOT_REPLAY_VALIDATION_<YYYYMMDD>.md
```

Conteúdo:

- resumo;
- arquivos alterados;
- contratos criados/alterados;
- estratégia de hash;
- como snapshot é capturado;
- como snapshot é restaurado;
- como EnemySpawnPlan entra no snapshot;
- validações executadas;
- pendências;
- próximos recortes.

---

# /speckit.implement

## Prompt completo para Claude/Codex

```md
Estamos no projeto Cindar's Hope / repositório rafa210587/cindars_hope.

Branch alvo: dev.

Implemente somente:
SPEC 14B - Cave Snapshot Replay with EnemySpawnPlan

Leia primeiro:
- AGENTS.md
- CLAUDE.md
- PROJECT_LOG.md
- docs/IMPLEMENTATION_STATUS.md
- docs/specs/a_implementar/spec_14b_cave_snapshot_replay_enemy_plan.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_spec14b_cave_snapshot_replay_enemy_plan.md
- docs/specs/a_implementar/spec_14a_cave_enemy_spawnplan_materialization_run_stability.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_spec14a_cave_enemy_spawnplan_materialization.md
- docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md

Contexto:
A SPEC 14A conecta EnemySpawnResolver à CaveRuntimeMaterializer via CaveEnemySpawnPlan. Agora a SPEC 14B deve garantir snapshot/replay de níveis visitados, incluindo layout, resources, fishing spot e EnemySpawnPlan.

Objetivo:
Ao revisitar um cave level já visitado na mesma run, restaurar a partir de snapshot e não rerollar layout/resources/fishing/enemies.

Regras obrigatórias:
- Snapshot key = CaveRunSeed + CaveLevel.
- Snapshot deve conter EnemySpawnPlan.
- Snapshot deve conter ResourceNodeStates.
- Snapshot deve conter FishingSpotState/hook.
- LayoutHash deve ser determinístico e comparável.
- Save/load deve preservar snapshots.
- Não implementar respawn de inimigos 2 dias.
- Não implementar redistribuição pós-morte.
- Não implementar boss fights/rewards.
- Não criar sistema paralelo de cave runtime/save/enemy spawn.
- Não usar GameObject.Find/FindObjectOfType/FindObjectsByType em runtime.
- Não serializar referências Unity em save DTO.

Escopo:
1. Consolidar/criar CaveLevelSnapshot.
2. Criar CaveResourceNodeSnapshotEntry.
3. Criar CaveFishingSpotSnapshotEntry.
4. Criar CaveSnapshotService ou equivalente.
5. Integrar com CaveRunManager/CaveRuntimeState.VisitedLevelSnapshots.
6. Integrar com CaveSaveData.
7. Garantir que revisitar nível usa snapshot.
8. Garantir que EnemySpawnPlan entra no snapshot.
9. Criar validator SPEC 14B.
10. Criar docs/validation/SPEC14B_CAVE_SNAPSHOT_REPLAY_VALIDATION_<YYYYMMDD>.md.

Validação obrigatória:
Rodar, se disponível:
```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
git diff --check
```

Checklist Play Mode mínimo:
1. Entrar em CaveScene nível 1.
2. Registrar LayoutHash e EnemySpawnPlan.
3. Sair e voltar para o mesmo nível na mesma run.
4. Confirmar mesmo LayoutHash.
5. Confirmar mesmo EnemySpawnPlan.
6. Confirmar resources não rerollam.
7. Confirmar fishing spot não rerolla.
8. Salvar/carregar.
9. Confirmar snapshot preservado.
10. Confirmar console sem erros novos.

Entrega final:
Responder com:
1. resumo técnico curto;
2. arquivos alterados;
3. contratos criados/alterados;
4. estratégia de LayoutHash;
5. validações executadas;
6. validações não executadas e motivo;
7. riscos residuais;
8. próximo recorte recomendado: SPEC 14C - Enemy defeated state and 2-day respawn.
```

---

# Definition of Done

- Projeto compila.
- `CaveLevelSnapshot` existe/consolidado.
- `CaveSnapshotService` existe ou equivalente.
- Snapshot inclui layout.
- Snapshot inclui resources.
- Snapshot inclui fishing spot/hook.
- Snapshot inclui `EnemySpawnPlan`.
- `LayoutHash` é determinístico.
- Revisitar nível usa snapshot.
- Save/load preserva snapshots.
- Validator SPEC 14B existe.
- Documento de validação criado.

---

# Próximo recorte recomendado

Depois da 14B:

```text
14C - Enemy defeated state and 2-day respawn
14D - Death redistribution hooks
14E - Boss gates/boss AI/rewards closeout
18A - Projectile visual foundation
```
