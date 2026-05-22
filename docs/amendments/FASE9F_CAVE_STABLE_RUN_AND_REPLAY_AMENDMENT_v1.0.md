# FASE9F — Cave Stable Run and Replay Amendment v1.0

> **Status:** aprovado para orientar implementação.  
> **Feature:** `FASE9F_CAVE_STABLE_RUN_REPLAY_AND_PROGRESSION`  
> **Base:** `docs/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md` + `specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/spec.md`.  
> **Motivo:** fixar a regra de estabilidade de níveis já visitados na mesma run da caverna.

---

## 1. Regra central

A cave é procedural por run, não por entrada em nível.

Dentro da mesma `CaveRunSeed`, um `CaveLevel` já visitado deve manter:

- layout/estrutura;
- entrada e saída;
- composição de inimigos;
- quantidade de inimigos;
- posições dos inimigos;
- IDs/tipos dos inimigos;
- composição de resource nodes;
- quantidade de resource nodes;
- posições dos resource nodes;
- IDs/tipos dos resource nodes;
- estado de depletion dos nodes;
- estado de boss/miniboss quando aplicável.

Procedural só pode mudar em:

1. novo jogo;
2. KO/morte/derrota do personagem;
3. comando debug explícito de regeneração de run.

`ForwardExit` e `BackExit` nunca podem alterar `CaveRunSeed`.

---

## 2. Primeira visita vs revisita

### Primeira visita ao CaveLevel em uma run

O sistema deve:

1. gerar layout;
2. gerar composição de inimigos;
3. gerar composição de resource nodes;
4. criar `CaveVisitedLevelSnapshot`;
5. registrar `LayoutHash` e `ContentHash`;
6. salvar snapshot no runtime state e no save quando salvar;
7. materializar a cena a partir do snapshot.

### Revisita ao CaveLevel na mesma run

O sistema deve:

1. carregar `CaveVisitedLevelSnapshot` existente;
2. não sortear layout novamente;
3. não sortear inimigos novamente;
4. não sortear resource nodes novamente;
5. não alterar quantidades, tipos, posições ou composição;
6. materializar a cena a partir do snapshot salvo.

---

## 3. Ranges obrigatórios

- Inimigos por `CaveLevel`: aleatório entre `12` e `20` na primeira visita do nível dentro da run.
- Resource nodes por `CaveLevel`: aleatório entre `4` e `10` na primeira visita do nível dentro da run.

Revisitas não rerollam esses ranges.

---

## 4. Distribuição de inimigos

A primeira geração de composição do nível deve respeitar:

```text
70%–80%: EnemyLevel = CaveLevel
10%–20%: EnemyLevel = CaveLevel + 1
10%: Special EnemyLevel = CaveLevel + 2
```

Special enemy deve ter pelo menos:

- visual/cor diferente;
- nome debug diferente;
- XP maior;
- loot melhor ou loot table própria.

---

## 5. Distribuição de resource nodes

Resource nodes devem usar raridade/config por faixa de nível.

Raridades mínimas:

```text
Common
Uncommon
Rare
Epic
Special
```

MVP obrigatório:

```text
Levels 1–15:
- Stone = Common
- Copper = Uncommon
- CaveRootTree = Rare

Levels 16–30:
- Copper = Common
- Iron = Uncommon
- UndergroundHardwood = Rare, se assets mínimos existirem
```

Se os assets de 16–30 ainda não existirem, criar contratos/configs e registrar pendência sem bloquear o pacote.

---

## 6. Snapshot mínimo

Cada `CaveVisitedLevelSnapshot` deve conter apenas dados serializáveis simples:

```text
CaveLevel
BiomeId
EncounterEcologyId
FactionLockId
LayoutSeed
ContentSeed
GenerationVersion
EntrancePosition
ExitPosition
WalkableTiles
WallTiles
EnemySpawns
ResourceNodeSpawns
LayoutHash
ContentHash
```

Não salvar `GameObject`, `Transform`, `MonoBehaviour`, `ScriptableObject`, `Sprite`, `Collider`, `Rigidbody` ou qualquer Unity ref.

---

## 7. Save/load

`CaveSaveData` vNext deve armazenar:

```text
CaveSaveSchemaVersion
CurrentCaveLevel
DeepestLayerReached
CaveWorldSeed
CaveRunSeed
UnlockedCheckpoints
VisitedLevels
DepletedNodeIds
DefeatedBossIds
```

Save antigo sem snapshots não deve quebrar. Ele deve gerar snapshot apenas do nível atual quando necessário, sem inventar níveis não visitados.

---

## 8. Logs e debug obrigatórios

Transições internas da cave devem logar:

```text
ExitMode
BeforeLevel
AfterLevel
UsedSnapshot
GeneratedNewSnapshot
RunSeed
LayoutHash
ContentHash
EnemyCount
ResourceNodeCount
```

HUD debug deve expor esses valores quando em CaveScene.

---

## 9. Critérios de aceite

- `ForwardExit` avança CaveLevel sem trocar `CaveRunSeed`.
- `BackExit` retrocede CaveLevel sem trocar `CaveRunSeed`.
- `BackExit` no level 1 volta para `FarmScene` com spawn `farm_from_cave`.
- Revisitar nível já visitado mantém layout.
- Revisitar nível já visitado mantém inimigos.
- Revisitar nível já visitado mantém resource nodes.
- KO/morte troca `CaveRunSeed` e limpa snapshots da run.
- Checkpoints persistem após KO/morte.
- Save/load preserva snapshots já visitados.
- Enemy count inicial por level fica entre 12 e 20.
- Resource node count inicial por level fica entre 4 e 10.

