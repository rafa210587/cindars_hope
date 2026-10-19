# Auditoria de absorcao - feature/docs-fase9f-cave-stable-run-spec

> Data: 2026-05-23
> Branch analisada: `feature/docs-fase9f-cave-stable-run-spec`
> Base de decisao: `dev`
> Status: Conteudo util absorvido parcialmente; branch nao deve ser mergeada inteira.

---

## 1. Resultado da comparacao

A branch `feature/docs-fase9f-cave-stable-run-spec` diverge da `dev` e contem 32 commits/arquivos documentais antigos.

A branch adiciona principalmente:

- READMEs e handoffs de reorganizacao documental antigos;
- registry antigo em formato anterior;
- specs implementadas com nomes antigos sem prefixo `spec_`;
- `docs_old/MANIFEST.md` e `docs_old/README.md` antigos.

A `dev` atual ja possui a reorganizacao documental nova, com:

- `.specs/` como fonte unica;
- specs normalizadas com prefixo `spec_`;
- registries atuais;
- `SPEC_EXECUTION_ORDER.md`;
- pre-refinamentos organizados.

Portanto, a branch nao deve ser mergeada inteira.

---

## 2. Conteudo util absorvido

Foram absorvidos detalhes mais objetivos da branch historica nas specs normalizadas da `dev`:

### CAVE-002

Destino:

```text
.specs/implementados/spec_cave_002_procedural_contracts_resources_parcial.md
```

Conteudo absorvido:

- objetivo de substituir Cave MVP fixa por niveis grandes, reprodutiveis e exploraveis;
- arquitetura real com `CaveProceduralGenerator`, `CaveGeneratedLevel`, `CaveRuntimeMaterializer`, `ResourceNodeDataSO`, `ResourceNode` e `CaveRunManager`;
- tarefas implementadas: contratos de geracao, configuracao, resultado gerado, materializer, tool checks parciais e save parcial de depleted nodes;
- pendencias: balance de layouts, loot/scaling, biomas, resource distribution e validacao Unity.

### CAVE-003

Destino:

```text
.specs/implementados/spec_cave_003_stable_run_snapshots_replay_parcial.md
```

Conteudo absorvido:

- regra central de que niveis ja visitados dentro da mesma `CaveRunSeed` devem ser restaurados por snapshot, sem reroll de layout, recursos ou inimigos;
- arquitetura com `VisitedLevelSnapshot`, `CaveLevelRuntimeController`, `CaveSaveData` e `CaveRunManager`;
- tarefas implementadas: snapshot contracts, `LayoutHash`, capture/restore, save/load, reset por KO/derrota e daily refresh de nodes respawnaveis;
- pendencias: Play Mode, backtrack, save/load de snapshots e KO preservando checkpoints.

### CAVE-004

Destino:

```text
.specs/implementados/spec_cave_004_boss_gates_checkpoints_confinement_parcial.md
```

Conteudo absorvido:

- regra de boss gate em marco de progressao, checkpoint desbloqueavel, selecao de checkpoint na entrada e confinamento do player ao caminho walkable;
- arquitetura com `CaveBossGateDataSO`, registry, `CaveBossSpawner`, `CaveBossDefeatMonitor`, `CaveCheckpointSelectionUI`, `CaveEntryController` e `CavePlayerPathConfinement`;
- tarefas implementadas: gate data/registry, save/load de boss defeat, spawn condicional, checkpoint unlock/selecao, entry por checkpoint, path confinement, validator/debug parcial;
- pendencias: teste boss level 15, gate 15 -> 16, checkpoint apos boss e save/load preservando boss defeat.

---

## 3. Conteudo nao absorvido

Nao foi absorvido o formato antigo de:

- nomes sem prefixo `spec_`;
- registries antigos;
- READMEs antigos;
- handoff antigo de reorganizacao;
- `docs_old` antigo adicionado pela branch.

Motivo: a `dev` atual ja possui estrutura documental mais nova e fonte unica em `.specs/`.

---

## 4. Decisao

Nao fazer merge da branch `feature/docs-fase9f-cave-stable-run-spec` em `dev`.

Apos esta auditoria, a branch pode ser deletada com seguranca operacional, pois o conteudo util identificado foi absorvido nas specs normalizadas CAVE-002, CAVE-003 e CAVE-004.

---

## 5. Escopo desta auditoria

Alterados somente documentos em `.specs/`.

Nao foram alterados:

```text
Assets/**
Packages/**
ProjectSettings/**
docs_old/**
PROJECT_LOG.md
```
