# FASE9F â€” Cave Stable Run and Replay Amendment v1.0

> **Status:** aprovado para orientar implementaÃ§Ã£o.
> **Feature:** `FASE9F_CAVE_STABLE_RUN_REPLAY_AND_PROGRESSION`
> **Base:** `docs/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md` + `docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md`.
> **Motivo:** fixar a regra de estabilidade de nÃ­veis jÃ¡ visitados na mesma run da caverna.

---

## 1. Regra central

A cave Ã© procedural por run, nÃ£o por entrada em nÃ­vel.

Dentro da mesma `CaveRunSeed`, um `CaveLevel` jÃ¡ visitado deve manter:

- layout/estrutura;
- entrada e saÃ­da;
- composiÃ§Ã£o de inimigos;
- quantidade de inimigos;
- posiÃ§Ãµes dos inimigos;
- IDs/tipos dos inimigos;
- composiÃ§Ã£o de resource nodes;
- quantidade de resource nodes;
- posiÃ§Ãµes dos resource nodes;
- IDs/tipos dos resource nodes;
- estado de depletion dos nodes;
- estado de boss/miniboss quando aplicÃ¡vel.

Procedural sÃ³ pode mudar em:

1. novo jogo;
2. KO/morte/derrota do personagem;
3. comando debug explÃ­cito de regeneraÃ§Ã£o de run.

`ForwardExit` e `BackExit` nunca podem alterar `CaveRunSeed`.

---

## 2. Primeira visita vs revisita

### Primeira visita ao CaveLevel em uma run

O sistema deve:

1. gerar layout;
2. gerar composiÃ§Ã£o de inimigos;
3. gerar composiÃ§Ã£o de resource nodes;
4. criar `CaveVisitedLevelSnapshot`;
5. registrar `LayoutHash` e `ContentHash`;
6. salvar snapshot no runtime state e no save quando salvar;
7. materializar a cena a partir do snapshot.

### Revisita ao CaveLevel na mesma run

O sistema deve:

1. carregar `CaveVisitedLevelSnapshot` existente;
2. nÃ£o sortear layout novamente;
3. nÃ£o sortear inimigos novamente;
4. nÃ£o sortear resource nodes novamente;
5. nÃ£o alterar quantidades, tipos, posiÃ§Ãµes ou composiÃ§Ã£o;
6. materializar a cena a partir do snapshot salvo.

---

## 3. Ranges obrigatÃ³rios

- Inimigos por `CaveLevel`: aleatÃ³rio entre `12` e `20` na primeira visita do nÃ­vel dentro da run.
- Resource nodes por `CaveLevel`: aleatÃ³rio entre `4` e `10` na primeira visita do nÃ­vel dentro da run.

Revisitas nÃ£o rerollam esses ranges.

---

## 4. DistribuiÃ§Ã£o de inimigos

A primeira geraÃ§Ã£o de composiÃ§Ã£o do nÃ­vel deve respeitar:

```text
70%â€“80%: EnemyLevel = CaveLevel
10%â€“20%: EnemyLevel = CaveLevel + 1
10%: Special EnemyLevel = CaveLevel + 2
```

Special enemy deve ter pelo menos:

- visual/cor diferente;
- nome debug diferente;
- XP maior;
- loot melhor ou loot table prÃ³pria.

---

## 5. DistribuiÃ§Ã£o de resource nodes

Resource nodes devem usar raridade/config por faixa de nÃ­vel.

Raridades mÃ­nimas:

```text
Common
Uncommon
Rare
Epic
Special
```

MVP obrigatÃ³rio:

```text
Levels 1â€“15:
- Stone = Common
- Copper = Uncommon
- CaveRootTree = Rare

Levels 16â€“30:
- Copper = Common
- Iron = Uncommon
- UndergroundHardwood = Rare, se assets mÃ­nimos existirem
```

Se os assets de 16â€“30 ainda nÃ£o existirem, criar contratos/configs e registrar pendÃªncia sem bloquear o pacote.

---

## 6. Snapshot mÃ­nimo

Cada `CaveVisitedLevelSnapshot` deve conter apenas dados serializÃ¡veis simples:

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

NÃ£o salvar `GameObject`, `Transform`, `MonoBehaviour`, `ScriptableObject`, `Sprite`, `Collider`, `Rigidbody` ou qualquer Unity ref.

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

Save antigo sem snapshots nÃ£o deve quebrar. Ele deve gerar snapshot apenas do nÃ­vel atual quando necessÃ¡rio, sem inventar nÃ­veis nÃ£o visitados.

---

## 8. Logs e debug obrigatÃ³rios

TransiÃ§Ãµes internas da cave devem logar:

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

## 9. CritÃ©rios de aceite

- `ForwardExit` avanÃ§a CaveLevel sem trocar `CaveRunSeed`.
- `BackExit` retrocede CaveLevel sem trocar `CaveRunSeed`.
- `BackExit` no level 1 volta para `FarmScene` com spawn `farm_from_cave`.
- Revisitar nÃ­vel jÃ¡ visitado mantÃ©m layout.
- Revisitar nÃ­vel jÃ¡ visitado mantÃ©m inimigos.
- Revisitar nÃ­vel jÃ¡ visitado mantÃ©m resource nodes.
- KO/morte troca `CaveRunSeed` e limpa snapshots da run.
- Checkpoints persistem apÃ³s KO/morte.
- Save/load preserva snapshots jÃ¡ visitados.
- Enemy count inicial por level fica entre 12 e 20.
- Resource node count inicial por level fica entre 4 e 10.

