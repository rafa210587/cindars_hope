# Roadmap — FASE9F Cave Stable Run PR-170 a PR-192

> **Branch alvo:** `feature/pr-170-192-cave-stable-run-progression`  
> **Base:** `dev`  
> **Entrega:** pacote inteiro em uma branch, PR único contra `dev`.  
> **Modo:** SpecKit / spec-driven development.

---

## Regra central

CaveLevel já visitado dentro da mesma `CaveRunSeed` não pode mudar layout, inimigos ou resource nodes.

Procedural só muda em:

- novo jogo;
- KO/morte/derrota do personagem;
- comando debug explícito.

ForwardExit e BackExit não podem alterar `CaveRunSeed`.

---

## PR-170 — Stable run contracts amendment

Criar contratos de snapshot/replay:

- `CaveVisitedLevelSnapshot`
- `CaveVisitedLevelSaveData`
- `CaveLayoutSnapshotData`
- `CaveEnemySpawnSnapshotData`
- `CaveResourceNodeSpawnSnapshotData`
- `GridPositionData`
- `CaveContentGenerationRulesSO`

---

## PR-171 — Generator outputs replayable snapshot

Generator deve produzir snapshot reexecutável com:

- `CaveGeneratedLevel`
- `CaveVisitedLevelSnapshot`
- `LayoutHash`
- `ContentHash`

---

## PR-172 — Run regeneration only on KO/new game/debug

KO/morte cria nova run, limpa snapshots e preserva checkpoints.

Forward/Back não alteram run seed.

---

## PR-173 — Checkpoints oficiais + debug entry

Checkpoints: `1, 15, 30, 45, 60, 75, 90`.

Entrada por checkpoint usa snapshot se existir.

---

## PR-174 — ResourceNode contracts with range and rarity

Resource nodes por level: `4–10`.

Adicionar rarity/weight/level band/biome constraints.

---

## PR-175 — ResourceNode runtime stable by snapshot

Primeira visita sorteia nodes e salva snapshot.

Revisita usa snapshot.

---

## PR-176 — Cave save/load stable run snapshots

Expandir save para `VisitedLevels` e schema vNext.

---

## PR-177 — Enemy spawn by CaveLevel 12–20

Primeira visita sorteia `12–20` inimigos e salva enemy snapshots.

---

## PR-178 — Enemy distribution + special visual

Aplicar distribuição base/+1/+2 special e visual diferenciado.

---

## PR-179 — Loot tables for enemies and nodes

Criar `LootTableSO`, `LootEntry`, `LootRollResult`, `LootRoller`.

---

## PR-180 — XP integration

XP = `EnemyLevel × DifficultyXpMultiplier`.

---

## PR-181 — Daily cave refresh

`RespawnsDaily=true` reativa o mesmo node; não troca composição.

---

## PR-182 — Boss gate MVP level 15

Level 15 bloqueia avanço 15→16 até boss derrotado.

---

## PR-183 — Exit transition hardening

Corrigir e validar ForwardExit/BackExit.

Logs obrigatórios: `ExitMode`, `BeforeLevel`, `AfterLevel`, `UsedSnapshot`, `GeneratedNewSnapshot`, `RunSeed`, `LayoutHash`, `ContentHash`.

---

## PR-184 — Materialization cleanup and replay

Limpar runtime antigo e materializar snapshot.

Materializer não deve sortear conteúdo.

---

## PR-185 — Cave HUD diagnostics v2

HUD exibe runtime state, snapshot state e counters reais.

---

## PR-186 — ResourceNodeDatabase reliability

Validar database, IDs, fallback, tool/tier e duplicidade.

---

## PR-187 — Resource rarity by cave band

MVP de raridade por faixas 1–15 e contratos para 16–30.

---

## PR-188 — FASE9G pre-contracts only

Adicionar campos preparatórios de ecology/faction, sem implementar bestiário completo.

---

## PR-189 — Cave composition replay debug tools

Debug para imprimir snapshot, inimigos, nodes e comparar materialização.

---

## PR-190 — Cave save migration vNext

Schema v2 e compatibilidade com saves antigos.

---

## PR-191 — Scene regeneration + CameraFollow validation

Validar Farm/Town/Cave com `CameraFollow2D` e `UnityEngine.Camera` explícito.

---

## PR-192 — Validator + handoff completo

Expandir validators, atualizar logs/status e criar handoff final.

---

## Checklist final Unity

- `CindarsHope/Validate/Validate MVP Data`
- `CindarsHope/Scenes/Create MVP FarmScene`
- `CindarsHope/Scenes/Create MVP TownScene`
- `CindarsHope/Scenes/Create MVP CaveScene`
- Play Mode Farm → Cave
- ForwardExit 1 → 2
- ForwardExit 2 → 3
- BackExit 3 → 2
- BackExit 2 → 1
- BackExit 1 → Farm
- Confirmar spawn `farm_from_cave`
- Confirmar enemy count 12–20
- Confirmar resource node count 4–10
- Confirmar revisita mantém snapshot
- Confirmar KO/morte regenera run
- Confirmar save/load preserva snapshots
- Confirmar câmera nas três cenas
- Confirmar Console sem erro vermelho

