# SPEC — Caverna: Variedade de Layout por Bioma, Hazards e Salas de Tesouro

> **Spec ID:** `fable_09_spec_cave_biome_layout_variety_runtime`
> **Status:** A implementar
> **Wave:** FABLE — Gap Closure Bloco D (mundo)
> **Priority:** P2
> **Type:** Runtime
> **Domain:** Cave
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_bloco_D
> **Can run with:** fable_01, fable_04 (locks diferentes), fable_11, fable_12
> **Must not run with:** specs que alterem CaveGeneratedLevel/materializer simultaneamente
> **Repo lock scope:** `Assets/_Game/Scripts/Cave/Generation/**`, `CaveRuntimeMaterializer.cs`
> **Depends on:**
> - cave_001-008 (geração estável), ADR-0005
> **Blocks:** N/A
> **Scope:** parametrizar geração por banda de bioma (7 bandas) com hazards e sala de tesouro determinísticos.
> **Out of scope:** arte/tiles visuais por bioma, novos inimigos, clima/lua na caverna (spec WAVE 15/24 futura).

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## Contexto

`CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md` descreve 7 bandas de bioma (stone 1-10,
fungal 11-25, ice 26-40, fire 41-55, ruins 56-70, deep 71-85, void 86-101) com identidade de
layout (densidade de salas, corredores, hazards, salas especiais). Hoje as bandas existem
apenas como **tags de spawn** no `CaveEnemySpawnPlanner.BuildBiomeTags`; a geração
(`CaveGeneratedLevel`/geradores em Cave/Generation) produz o mesmo layout em qualquer
profundidade, sem hazards nem salas de tesouro — o `TreasureTableSO`/mining loot da WAVE 06
não tem onde aparecer com destaque.

## Problema

101 níveis visualmente e estruturalmente idênticos matam a sensação de descida. A direction
de tesouro/armadilha (counterplay) e os biomas do roster não têm expressão espacial. Tudo
precisa permanecer determinístico por seed (ADR-0005), o que exige parametrização central —
não hacks locais.

## Objetivo

Ao final desta spec, a geração deve consumir um `CaveBiomeLayoutProfile` (estático em código,
por banda) que parametriza tamanho/quantidade de salas e densidade de paredes internas, e o
materializer deve materializar por nível: 0-2 hazards (poça tóxica/estalactite/gelo
escorregadio conforme banda) e, em ~15% dos níveis, 1 sala de tesouro com baú trancado por
guardião — tudo derivado de `StableHash(worldSeed|runSeed|level|salt)` e coberto pelos
validators de replay existentes.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
docs/game_rules/cave_rules.md
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
.claude/rules/cave-stable-run.md
.claude/skills/cave-stable-run-guard/SKILL.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- Cave/Generation: CaveGeneratedLevel, CaveRoom, geradores procedurais, LayoutHash
- CaveRuntimeMaterializer (floor/walls/entrance/exit/resources/enemies)
- CaveReplayValidator (replay do layout), VisitedLevelSnapshot
- ResourceNode/ResourceNodeDatabaseSO (nós de recurso já materializados)
- CaveEnemySpawnPlanner.StableHash + BuildBiomeTags (bandas)
- TreasureTableSO conceitual na WAVE 06 (auditar: LootTableSO serve de tabela do baú)
Não existe:
- perfis de layout por banda; hazards; salas de tesouro; baús
Auditar Fase 0:
- parâmetros reais do gerador (sala min/max, contagem) e onde injetar perfil
- shape do snapshot (hazards/baú precisam entrar no VisitedLevelSnapshot? estado de baú aberto SIM)
```

## Engineering stories

```text
Como jogador descendo, quero sentir mudança estrutural por banda (salas maiores em ruins,
labirinto apertado em fungal, cavernas abertas em ice).
Como explorador, quero sala de tesouro rara com guardião e baú que vale o desvio.
Como vítima de hazard, quero dano/efeito telegrafado por tile visível (cor própria).
Como stable run, exijo que revisitar o nível reproduza hazards/baú/estado de aberto.
```

## Escopo

```text
Inclui:
- CaveBiomeLayoutProfile (estático em código, 7 bandas): RoomCountRange, RoomSizeRange,
  CorridorWidth, HazardDensity, HazardTypes permitidos, TreasureRoomChancePercent;
- aplicar perfil no gerador via parâmetros (sem reescrever algoritmo);
- CaveHazardTile (NOVO componente): tipos ToxicPool (Poison F01 ao pisar), IceSlick
  (reduz atrito/controle 1.5s), FallingRock (dano único telegrafado ao entrar no tile);
  materializados em tiles walkable derivados por hash, longe de entrance/exit/spawns;
- sala de tesouro: seleção determinística de sala elegível (~15% dos níveis), spawn de
  TreasureChest (NOVO IInteractable: rola LootTableSO de tesouro via resolver F06 quando
  existir, senão tabela fixa) + 2 guardiões do plano de spawn realocados para a sala;
- estado de baú aberto persistido no VisitedLevelSnapshot (lista de chestId abertos) —
  padrão dos resource nodes depletados existente;
- EditMode tests: perfis por banda, determinismo de hazards/baú, replay com snapshot.
```

## Fora de escopo

```text
Não inclui: tiles/arte por bioma (cores placeholder por banda apenas); armadilhas complexas;
clima/lua modificando caverna (24_spec futura); minimap.
```

## Regras de não duplicação

```text
Não criar segundo gerador — parametrizar o existente.
Não criar segundo snapshot — estender VisitedLevelSnapshot (campo aditivo).
Baú usa IInteractable + InventoryManager.AddItem (padrão resource node), não sistema novo.
```

## Critérios de aceite

### CA-1 Identidade por banda
- Níveis 5/30/60 geram métricas de layout distintas conforme perfil (contagem/tamanho de salas).
- Evidência: testes que geram níveis por banda e validam métricas dentro dos ranges.

### CA-2 Hazards determinísticos e justos
- Mesmo nível/run → mesmos hazards; nunca em entrance/exit/spawn de player; telegraph visual (cor).
- Evidência: teste de determinismo + validação de distância mínima.

### CA-3 Sala de tesouro estável
- ~15% dos níveis (medido em 200 níveis sintéticos: 10-20%); baú abre 1x; revisita mostra baú aberto.
- Evidência: teste de taxa + round-trip do snapshot com chestId.

### CA-4 Replay intacto
- CaveReplayValidator continua PASS; LayoutHash estável com perfil aplicado.
- Evidência: execução do validator nos testes.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Generation/CaveBiomeLayoutProfile.cs (NOVO — estático)
Assets/_Game/Scripts/Cave/Runtime/CaveHazardTile.cs            (NOVO)
Assets/_Game/Scripts/Cave/Runtime/TreasureChestInteractable.cs (NOVO)
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs   (hazards/tesouro)
Assets/_Game/Scripts/Cave/Generation/<gerador existente>.cs    (consumir perfil)
Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs      (campo aditivo OpenedChestIds)
Assets/_Game/Tests/EditMode/World/CaveBiomeLayoutTests.cs
```

## Contratos

### Data contracts — perfis estáticos em código (sem assets novos).
### Runtime contracts — `CaveBiomeLayoutProfile.ForLevel(int) → profile`.
### Event contracts — `TreasureChestOpenedEvent(chestId)` (NOVO).
### Save contracts — snapshot aditivo (OpenedChestIds: List<string>) — dentro do CaveRun
state existente; sem mudança no GameSaveData schema.
### UI contracts — N/A (prompt via IInteractable).

## Sistemas afetados

```text
Cave generation/materialization, snapshot/replay, Inventory (baú), Status (hazard Poison), Event bus
```

## Arquivos permitidos

```text
Arquivos da arquitetura + Assets/_Game/Scripts/Core/Events/CaveEvents aditivos
Assets/_Game/Tests/EditMode/World/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity/*.prefab/*.asset; Packages/ProjectSettings; SaveManager/GameSaveData;
CaveEnemySpawnPlanner.cs (exceto leitura de StableHash)
```

## Estratégia de implementação

```md
### Fase 0 — Auditar gerador (parâmetros injetáveis), snapshot e validators (LER os 3 docs do cave-stable-run-guard).
### Fase 1 — Perfis por banda + consumo no gerador + testes de métrica/replay.
### Fase 2 — Hazards determinísticos + integração F01 (Poison) com fallback sem status.
### Fase 3 — Sala de tesouro + baú + snapshot de abertos.
### Fase 4 — Validação estrita + report com seção cave-stable-run compliance.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Must not run with: qualquer spec tocando Generation/materializer
- Reason: lock de geração; F04 toca só EnemyBrain/coordinator (compatível com cuidado).

## Impacto em save/load

```text
Does this change save schema? NO (snapshot interno aditivo do cave run; default vazio)
```

## Impacto em eventos

```text
Adds events: YES (TreasureChestOpenedEvent) | Changes existing: NO | Unsubscribe: N/A
```

## Impacto em UI/Unity

```text
Changes UI/scenes/prefabs/assets: NO (tudo materializado em runtime)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: perfil alterar LayoutHash de runs em andamento. Mitigação: perfil entra no seed source
de forma idêntica para mesma run (level seed já inclui level); documentar que runs NOVAS mudam — runs salvas re-geram igual pois o gerador é determinístico pela mesma entrada (validar com replay test).
Risco: hazard spawn em corredor obrigatório bloquear caminho. Mitigação: hazards nunca em
tiles do caminho entrance↔exit (usar pathfinding existente do confinement, auditar).
```

## Rollback

```text
Perfil default = parâmetros atuais (banda stone para tudo); remover hazards/baú do materializer.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar gerador/snapshot/validators + docs do guard.
- [ ] T002 — CaveBiomeLayoutProfile + consumo + testes de métricas.
- [ ] T003 — Replay/LayoutHash tests com perfis.
- [ ] T004 — CaveHazardTile (3 tipos) determinístico + distâncias.
- [ ] T005 — TreasureChestInteractable + sala + guardiões.
- [ ] T006 — OpenedChestIds no snapshot + round-trip.
- [ ] T007 — csproj; run_strict_validation; report (compliance ADR-0005).
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES
- Requires EditMode tests: YES (replay é mandatório)
- Requires PlayMode automated or final human scenario: YES
- Requires regression test: YES (replay validator)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes de replay + cenário humano com bioma/tesouro/hazard

## Definition of Done

```text
7 bandas com identidade estrutural; hazards e tesouro determinísticos com snapshot;
replay PASS; builds 0E; report com compliance.
```

## Anti-regressão

```text
INVARIANTE FASE9F: revisita não reroda layout/inimigos/recursos/baú.
ForwardExit/BackExit não mudam CaveRunSeed.
Sem GUID/timestamp. Confinement do player intacto.
```

## Notas para execução posterior

```text
Clima/lua na caverna: promover 15_/24_spec futuras.
Tiles visuais por bioma: fase de arte.
```


---

## EMENDA 2026-06-12 (pós-canonização dos catálogos — VINCULANTE)

```text
1. TAMANHOS (decisão Q12.1): base 55×55; ~20% dos níveis 42×42; ~20% 65×65 — oscilação
   determinística por StableHash(worldSeed|runSeed|level|"size").
2. DENSIDADE re-escalada por área: count_final = count_base × (área/3025).
3. LAGOS: garantir 1 lago subterrâneo a cada 5 níveis nas bandas com criaturas aquáticas
   (Lake Lurker 1-10, Mirrorfin 26-40) — pré-requisito das fichas do bestiário.
4. SOLO PLANTÁVEL: 1 bolsão de terra a cada ~8 níveis (fungal+) para shadowroot (ITEM_CATALOG).
5. Confinement/replay validators DEVEM passar com os 3 tamanhos.
```
