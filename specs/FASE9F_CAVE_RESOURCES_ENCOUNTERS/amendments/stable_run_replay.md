# SpecKit Amendment — FASE9F Cave Stable Run Replay

> **Feature:** `FASE9F_CAVE_STABLE_RUN_REPLAY_AND_PROGRESSION`  
> **Status:** aprovado para implementação.  
> **Source amendment:** `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`

---

## User Story

Como jogador, quero que os níveis da caverna que eu já visitei dentro da mesma run permaneçam reconhecíveis e consistentes, mantendo layout, inimigos e resource nodes, para que exploração, retorno e progressão tenham memória espacial e consequência.

---

## Functional Requirements

### FR-001 — Stable visited level snapshot

O sistema deve criar um snapshot na primeira visita de cada `CaveLevel` dentro da `CaveRunSeed` atual.

O snapshot deve preservar layout, entrada, saída, inimigos, resource nodes, hashes de layout e hashes de conteúdo.

### FR-002 — Replay on revisit

Quando o jogador revisitar um `CaveLevel` já existente na run atual, o sistema deve materializar o snapshot salvo, sem reroll de layout, inimigos ou resource nodes.

### FR-003 — Run regeneration boundary

O sistema só pode gerar nova composição procedural global em novo jogo, KO/morte/derrota do personagem ou comando debug explícito.

`ForwardExit` e `BackExit` não podem alterar `CaveRunSeed`.

### FR-004 — Enemy count range

Na primeira geração do nível dentro da run, o sistema deve sortear `EnemyCount` entre `12` e `20`, inclusive.

### FR-005 — Resource node count range

Na primeira geração do nível dentro da run, o sistema deve sortear `ResourceNodeCount` entre `4` e `10`, inclusive.

### FR-006 — Enemy level distribution

A composição de inimigos deve respeitar:

```text
70%–80% EnemyLevel = CaveLevel
10%–20% EnemyLevel = CaveLevel + 1
10% Special EnemyLevel = CaveLevel + 2
```

### FR-007 — Special enemy visual

Enemy especial deve ter visual/cor diferente, nome debug diferente, XP maior e loot melhor ou loot table própria.

### FR-008 — Resource rarity by band

Resource nodes devem ser distribuídos por raridade e faixa de nível.

MVP:

```text
Levels 1–15:
- Stone = Common
- Copper = Uncommon
- CaveRootTree = Rare

Levels 16–30:
- Copper = Common
- Iron = Uncommon
- UndergroundHardwood = Rare, se assets existirem
```

### FR-009 — Save/load snapshots

Save/load deve preservar snapshots já visitados.

Save antigo sem snapshots não deve quebrar e não deve inventar níveis não visitados.

### FR-010 — Debug visibility

HUD e logs devem mostrar se o nível foi carregado de snapshot ou gerado pela primeira vez.

---

## Acceptance Criteria

### AC-001

Dado CaveLevel 1 gerado pela primeira vez, quando o jogador vai para CaveLevel 2 e volta para CaveLevel 1, então layout, inimigos e nodes do CaveLevel 1 são idênticos.

### AC-002

Dado CaveLevel 2 gerado pela primeira vez, quando o jogador vai para CaveLevel 3 e volta para CaveLevel 2, então layout, inimigos e nodes do CaveLevel 2 são idênticos.

### AC-003

ForwardExit 1 → 2 não muda `CaveRunSeed`.

### AC-004

BackExit 2 → 1 não muda `CaveRunSeed`.

### AC-005

KO/morte muda `CaveRunSeed`, limpa snapshots da run e preserva checkpoints.

### AC-006

EnemyCount de todo snapshot novo fica entre 12 e 20.

### AC-007

ResourceNodeCount de todo snapshot novo fica entre 4 e 10.

### AC-008

Save/load preserva snapshots visitados.

### AC-009

HUD exibe CaveLevel, RunSeed, HasSnapshot, UsedSnapshot, EnemyCount, ResourceNodeCount, LayoutHash e ContentHash.

### AC-010

Nenhum runtime novo usa `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType` ou `StreamingAssets`.

