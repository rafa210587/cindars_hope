# SPEC — Caverna Viva: Povoamento, Ecossistema de Monstros e Desafio por Nível

> **Spec ID:** `fable_78_spec_cave_ecosystem_population_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 13 (Cave Population)
> **Priority:** P1
> **Type:** Runtime / Data / Editor
> **Domain:** Cave / Enemy / Combat
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** specs de docs/governança puras; specs de UI que não toquem Cave/Enemy
> **Must not run with:** qualquer spec que toque `CaveEnemySpawnPlanner`, `CaveBiomeLayoutProfile`, `EnemyBrain`, `EnemyHealth`, `CaveRuntimeMaterializer`, `VisitedLevelSnapshot`, `CaveSaveData`, ou a geração procedural da caverna
> **Repo lock scope:**
> - `Assets/_Game/Scripts/Cave/**`
> - `Assets/_Game/Scripts/Enemy/EnemyBrain.cs`, `EnemyHealth.cs`, `EnemyPackCoordinator.cs`
> - `Assets/_Game/Scripts/Save/CaveSaveData.cs`
> - `CaveGenerationConfigSO.GenerationConfigVersion`
> **Depends on:**
> - `fable_33` (roster expandido — 60+ criaturas com famílias/tags `IsAquatic`)
> - `fable_24` (22 Moves + elite affixes runtime)
> - `fable_06` (loot tables + vulnerability fields, seed FNV-1a determinístico)
> - `fable_32` (itens de minério/material existem no catálogo)
> - `fable_09` (biomas/bandas da caverna + layout profile + `ResolveMapSize`)
> - `fable_13` (save de cave run + enemy HP por instância)
> - `fable_01` (pipeline de status effects — usado pelo status leve "Ferido" do conflito)
> - GAMEPLAY_EXPANSION_SLICE (densidade 16–32, `CaveWanderingMerchant`, hazards, treasure rooms)
> **Blocks:**
> - specs futuras de "cave biome art pass" (consomem o layer de elementos ambientais)
> **Scope:** transformar a caverna procedural em um ecossistema vivo — elementos ambientais temáticos por bioma (pedras/minério, fungos, lagos, cristais, lava, destroços), maior densidade e desafio calibrado por nível, mapas que crescem com a profundidade, mercadores errantes enriquecidos, e um sistema determinístico de **conflito inter-monstro** (5% dos níveis por run) onde duas espécies diferentes brigam entre si e dividem agressividade com o jogador, causando 1/10 do dano entre elas.
> **Out of scope:** arte final (placeholders por família/bioma já bastam), novas criaturas além do roster fable_33, novo modal de UI, pets/companions na caverna, rework do sistema de save (apenas campos aditivos), balance numérico "final" (esta spec entrega curvas determinísticas tunáveis, não o balance assinado pelo humano).

required_adrs: [ADR-0005, ADR-0018, ADR-0019]
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## 5. Contexto

A caverna procedural já tem espinha dorsal madura: geração determinística por seed
(`CaveProceduralGenerator` + `CaveLayoutStableHash`), 7 bandas temáticas com perfis de
layout (`CaveBiomeLayoutProfile`: Stone 1-10, Fungal 11-25, Ice 26-40, Fire 41-55,
Ruins 56-70, Deep 71-85, Void 86-101+), spawn de inimigos com escala por profundidade
(`CaveEnemySpawnPlanner`, 16–32, cap 44), elite affixes (fable_24), bosses de gate
(`CaveBossSpawner`), hazards e treasure rooms (`CaveHazardPlanner`), resource nodes
mineráveis (`ResourceNode` + `ResourceNodeDatabaseSO` + `CaveLootSnapshotService`),
mercador errante (`CaveWanderingMerchant`, 22%/nível), e persistência stable-run
(`VisitedLevelSnapshot` + `CaveSnapshotService` + F13 enemy HP).

Apesar disso, **o espaço jogável é visualmente vazio e mecanicamente monótono**: as salas
são grandes áreas de chão com poucos inimigos esparsos e quase nenhum elemento de mundo.
Não há a sensação de um bioma vivo, e cada nível "se parece" com o anterior. O usuário
pediu (referência: screenshot do layout procedural atual) para povoar a caverna com mais
elementos, dar a cada bioma uma identidade, aumentar o desafio, e introduzir um
**ecossistema de monstros** com conflito emergente entre espécies.

Esta spec destrava a fase "Caverna Viva" do roadmap: ela é a base de conteúdo sobre a qual
um futuro art pass e telemetria de balance vão operar.

Decisões de direção tomadas com o humano (registrar como vinculantes desta spec):
- **Tamanho do mapa:** escalar com profundidade (base ~60×60 nas bandas iniciais → ~90×90
  nas bandas Deep/Void), mantendo a variação por seed (small/base/large) do fable_09.
- **Distribuição de elementos:** temática por bioma (cada banda tem perfil próprio de
  elementos; pedras e minério em todas, mas a mistura muda por região).
- **Empacotamento:** uma spec densa multi-fase (este arquivo).

## 6. Problema

Sem este trabalho:
- A caverna permanece um corredor vazio: 100 níveis com a mesma textura de chão, poucos
  inimetros esparsos e nenhuma leitura visual de "onde estou" — quebra de imersão e de
  pacing de exploração.
- O minério prometido (fable_32: `item_material_copper_ore`, `iron_ore`, `silver_ore`,
  `arcane_crystal`, `mithril_ore`) não tem fonte real distribuída por bioma — a economia de
  crafting de gear (fable_49) fica sem source confiável.
- Lagos não existem, então as criaturas `[AQUÁTICA]` do roster fable_33 (ex.: `lake_lurker`)
  nunca podem aparecer — conteúdo morto.
- A dificuldade por nível é frouxa: níveis podem sair quase vazios por azar de seed, sem
  garantia de desafio mínimo.
- O "ecossistema" de monstros não existe: inimigos só miram o jogador (target hardcoded em
  `EnemyBrain`), não há facções hostis entre espécies, e não há o conflito emergente pedido.

Risco concreto adicional: implementar conflito inter-monstro de forma **não-determinística**
(ex.: rolar a chance com `UnityEngine.Random` a cada frame/visita) **violaria o contrato
stable-run da FASE9F (ADR-0005)** — composição, posição e estado de inimigos de um nível
revisitado dentro do mesmo `CaveRunSeed` não podem dar reroll.

## 7. Objetivo

Ao final desta spec, o projeto deve ter, na caverna procedural:

1. Um **layer de elementos ambientais por bioma** (decorativos + mineráveis + tiles de lago)
   colocado deterministicamente e persistido no snapshot stable-run.
2. **Mapas que crescem com a profundidade** e **densidade/desafio calibrados por um threat
   budget determinístico** por nível, garantindo desafio mínimo sem violar stable-run.
3. Um **sistema de conflito inter-monstro** rolado **por entrada** no nível: a cada vez que o
   jogador entra, há **5%** de chance de um conflito; se um conflito **já ocorreu naquele nível
   durante a run**, a chance cai para **0,5%** nas entradas seguintes. Quando ativo, duas
   espécies **diferentes** presentes no nível brigam entre si e com o jogador, dividindo
   agressividade, causando **1/10 do dano** entre elas, integrado à persistência de HP do F13.
4. **Mercadores errantes enriquecidos** (mais variedade de estoque temático por bioma),
   mantendo o determinismo por seed.

Tudo sem alterar o schema de save de forma destrutiva (apenas campos aditivos
backward-compatible), sem `GameObject.Find`/`FindObjectOfType` novos em runtime, e com toda
comunicação de gameplay via `GameEventBus`.

## 8. Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
.claude/rules/testing-quality-gate.md
.claude/rules/cave-stable-run.md
.claude/rules/unity-architecture.md
.claude/rules/no-magic-balance-values.md
.claude/rules/id-stability.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/cave-stable-run-guard/SKILL.md
.claude/skills/rng-and-determinism/SKILL.md
.claude/skills/enemy-ai-authoring/SKILL.md
.claude/skills/scene-interactable-wiring/SKILL.md
.claude/skills/unity-validation/SKILL.md
docs/decisions/ADR-0005-*  (seed determinístico canônico — fonte do contrato stable-run)
docs/game_rules/cave_rules.md  (comportamento canônico stable-run)
```

## 9. Estado atual do repo (Phase 0 deve confirmar antes de codar)

**Existe e DEVE ser reutilizado (não recriar):**
- `Assets/_Game/Scripts/Cave/Generation/CaveProceduralGenerator.cs` — layout + seeds.
- `Assets/_Game/Scripts/Cave/Generation/CaveBiomeLayoutProfile.cs` — 7 bandas, `ResolveMapSize`.
- `Assets/_Game/Scripts/Cave/Generation/CaveLayoutStableHash.cs` — FNV-1a 32-bit canônico.
- `Assets/_Game/Scripts/Cave/Generation/CaveGeneratedLevel.cs` — walkable/wall tiles, rooms, spawn points.
- `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs` — densidade/escala/elite/bandas.
- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` — instancia tiles/inimigos/nodes/portais.
- `Assets/_Game/Scripts/Cave/Runtime/CaveHazardPlanner.cs` — hazards + treasure room.
- `Assets/_Game/Scripts/Cave/Resources/ResourceNode.cs` + `Cave/Data/ResourceNodeDatabaseSO.cs` + `ResourceNodeDataSO`.
- `Assets/_Game/Scripts/Cave/Loot/CaveLootSnapshotService.cs` (+ entry/profile) — idempotência de loot/minério com `CaveLootSourceType.MiningNode`/`RareVein`.
- `Assets/_Game/Scripts/Cave/Runtime/CaveWanderingMerchant.cs` — 22%/nível determinístico.
- `Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs` + `CaveSnapshotService.cs` — stable-run snapshot.
- `Assets/_Game/Scripts/Save/CaveSaveData.cs` — serialização de snapshots/seeds.
- `Assets/_Game/Scripts/Enemy/EnemyBrain.cs` — FSM, 22 Moves, threat memory, pack coordination.
- `Assets/_Game/Scripts/Enemy/EnemyHealth.cs` — `TakeDamage(DamageRequest)`, loot seed, vulnerability.
- `Assets/_Game/Scripts/Enemy/EnemyPackCoordinator.cs` — alerta social de pack.
- `Assets/_Game/Scripts/Combat/Data/EnemyFactionSO.cs` + `Enemy/EnemyFactionLockSO.cs` — facção/lock (HÁ campo `FactionId` em `EnemyDataSO`).
- `Assets/_Game/Scripts/Combat/DamageRequest.cs` + `DamageCalculator.cs` — pipeline de dano.
- `Assets/_Game/Scripts/Combat/Bestiary/CanonicalBestiaryCatalog.cs` + `BestiaryCreatureDef.cs` — famílias, `IsAquatic`, tiers.
- `Assets/_Game/Scripts/Loot/EnemyLootResolver.cs` — `BuildLootSeed` (FNV-1a).

**Parcial / a estender:**
- `CaveBiomeLayoutProfile` — adicionar tamanho-por-banda (escalar com profundidade).
- `CaveEnemySpawnPlanner` — adicionar threat budget + curva de densidade revista.
- `CaveWanderingMerchant` — enriquecer catálogo por bioma.
- `VisitedLevelSnapshot` / `CaveSaveData` — campos aditivos (elementos ambientais + conflito).

**Não existe (a criar):**
- Layer de elementos ambientais (planner + plan + materialização + profiles por bioma).
- Sistema de conflito inter-monstro (planner + plan + targeting de rival + dano inter-monstro + eventos).
- `CaveEcosystemBalanceSO` (constantes tunáveis: chance de conflito, multiplicador de dano, pesos de aggro, threat budget).

**Regra de auditoria:** confirmar na Phase 0 a assinatura real de `EnemyBrain` (como o target
é resolvido hoje — `RefreshPlayerTarget()`/`_playerTarget`) e de `EnemyHealth.TakeDamage`
(qual `sourceId`/origem é registrada) antes de tocar targeting. **Não recriar** nenhum sistema
acima; estender com o mínimo de superfície.

## 10. User / engineering stories

```text
Como jogador, quero que cada bioma da caverna pareça diferente (pedras, fungos, lagos,
  cristais, lava, destroços) para que descer 100 níveis seja uma jornada, não repetição.
Como jogador, quero veios de minério distribuídos por profundidade para abastecer meu
  crafting de gear.
Como jogador, quero que cada nível ofereça um desafio mínimo confiável, sem níveis vazios.
Como jogador, quero ocasionalmente encontrar duas espécies brigando entre si — um momento
  emergente onde eu posso intervir, fugir, ou deixar que se enfraqueçam.
Como sistema stable-run, quero que conflito, elementos e tamanho sejam determinísticos por
  (worldSeed, runSeed, caveLevel) para que um nível revisitado não dê reroll.
Como mantenedor, quero todos os números tunáveis num SO de balance, não hardcoded.
Como save, quero persistir apenas IDs/estado simples dos elementos e do conflito, sem refs Unity.
```

## 11. Escopo (inclui)

```text
- Tamanho de mapa escalando com profundidade por banda, determinístico, mantendo variação por seed.
- CaveEnvironmentElementPlanner determinístico: coloca clusters de elementos por bioma.
- Perfis de elementos por bioma (CaveEnvironmentElementProfileSO) — quais tipos e densidade por banda.
- Materialização dos elementos (decor não-bloqueante, decor bloqueante, tiles de lago, nodes mineráveis).
- Lagos habilitam spawn de inimigos [AQUÁTICA] (gating por presença de água no nível).
- Veios de minério por banda (ore tiers mapeados à profundidade) via ResourceNode + CaveLootSnapshotService.
- Threat budget determinístico por nível em CaveEnemySpawnPlanner garantindo desafio mínimo.
- Curva de densidade revista (mais monstros, especialmente em profundidade) — tunável no balance SO.
- Sistema de conflito inter-monstro:
    * decisão rolada POR ENTRADA no nível (não por composição): 5% base, 0,5% se já houve conflito naquele nível na run;
    * roll seedado por (worldSeed, runSeed, caveLevel, entryIndex) → reproduzível e testável, mas variando a cada entrada;
    * seleção de DUAS espécies DIFERENTES presentes no nível;
    * targeting de rival no EnemyBrain (split de aggro player vs rival);
    * dano inter-monstro = multiplicador (default 0.1), tunável;
    * loot em kill monstro-vs-monstro = corpo com loot/XP reduzidos (default 0.4), não zero nem cheio;
    * status leve "Ferido" no monstro que apanha de um rival (gancho tático; reusa fable_01);
    * integração com persistência de HP/morte do F13;
    * feedback de HUD obrigatório ao iniciar conflito (toast via evento);
    * persistência da flag "já houve conflito neste nível" + contador de entradas (aditivo no snapshot);
    * eventos no GameEventBus.
- Mercador errante enriquecido: estoque temático por bioma, variedade ampliada, determinístico.
- Persistência aditiva (snapshot + CaveSaveData) de elementos ambientais e estado de conflito.
- CaveEcosystemBalanceSO com todas as constantes tunáveis.
- EditMode tests de determinismo/distribuição/regras.
- Editor validators + geradores de data assets (perfis/balance/nodes por bioma).
- GenerationConfigVersion bump (invalida snapshots legados — regeneração determinística).
```

## 12. Fora de escopo (não inclui)

```text
- Arte final / sprites definitivos (placeholders por família/bioma; cores/shapes simples).
- Novas criaturas além do roster fable_33.
- Novo modal/tela de UI (apenas eventos para HUD/feedback existente).
- Rework do SaveManager ou bump destrutivo de schema (apenas campos aditivos).
- Balance numérico final assinado (entrega curvas determinísticas tunáveis; telemetria = fable_59).
- Pets/companions na caverna.
- Mineração com física/desmoronamento; nós mineráveis seguem o contrato hit-based atual.
- Pathfinding novo; reusar o movimento atual do EnemyBrain (rival targeting usa o mesmo locomotor).
```

## 13. Regras de não duplicação

```text
- NÃO criar segundo gerador de layout — estender CaveBiomeLayoutProfile/CaveProceduralGenerator.
- NÃO criar segundo sistema de loot/minério — reusar ResourceNode + CaveLootSnapshotService (MiningNode/RareVein).
- NÃO criar segundo planner de spawn de inimigos — estender CaveEnemySpawnPlanner.
- NÃO criar segundo merchant — estender CaveWanderingMerchant.
- NÃO criar novo RNG/hash — reusar CaveLayoutStableHash (FNV-1a) e o padrão de BuildSeed.
- NÃO criar namespace CindarsHope.Debug/Temp.
- NÃO criar wrapper Result<T>/Either novo — falhas esperadas usam bool/FailureReason existentes.
- NÃO hardcodar números de balance — tudo em CaveEcosystemBalanceSO ou const nomeada.
- NÃO usar GameObject.Find/FindObjectOfType em runtime novo — injeção via materializer/bootstrap.
- Conflito inter-monstro reutiliza EnemyFactionSO/FactionId existente para marcar rivais (não inventar "team" paralelo se FactionId resolver).
```

## 14. Critérios de aceite

### 14.1 Tamanho de mapa escala com profundidade (determinístico)
- A largura/altura-alvo do nível cresce monotonicamente por banda: Stone (base ~60×60) →
  Deep/Void (base ~90×90), preservando os buckets de variação small/base/large por seed do fable_09.
- Para um mesmo `(worldSeed, runSeed, caveLevel)`, o tamanho resolvido é sempre idêntico.
- Evidência: EditMode test `CaveMapSizeScalingTests` (monotonicidade por banda + determinismo + variação preservada).

### 14.2 Elementos ambientais temáticos por bioma (determinístico, persistido)
- Cada bioma instancia seu conjunto de elementos conforme `CaveEnvironmentElementProfileSO`
  (ex.: Stone = pedras + cobre/ferro; Fungal = fungos gigantes + cogumelos + glowcap; Ice =
  lago congelado + cristais; Fire = veios de obsidiana/lava + minério ígneo; Ruins =
  destroços + tesouro; Deep/Void = veios de mithril/arcane raros).
- Elementos não bloqueiam o caminho entrance↔exit (validação BFS, igual ao hazard planner).
- Pedras e veios de minério aparecem em **todas** as bandas (mistura/raridade varia por banda).
- Para um mesmo `(worldSeed, runSeed, caveLevel)`, o conjunto/posições/tipos é idêntico em toda
  revisita; estado depletado de nós mineráveis persiste via `CaveLootSnapshotService`.
- Evidência: EditMode `CaveEnvironmentElementPlannerTests` (determinismo, não-bloqueio do path,
  presença obrigatória de pedra/minério, perfil por banda respeitado) + validator de assets.

### 14.3 Lagos habilitam criaturas aquáticas
- Quando o nível tem tiles de lago (perfil do bioma marca água), inimigos com `IsAquatic=true`
  do roster ficam elegíveis a spawn naquele nível; sem lago, não spawnam.
- Evidência: EditMode test confirmando o gating (lago presente → aquático elegível; ausente → não).

### 14.4 Threat budget garante desafio mínimo (com teto e entrada segura)
- `CaveEnemySpawnPlanner` computa um threat budget determinístico por nível (função de banda +
  profundidade, tunável no balance SO) e preenche o plano **entre piso e teto** (`ThreatBudgetMinByBand` /
  `ThreatBudgetMaxByBand`), sem ultrapassar o cap de densidade. O teto modula qualidade (evita inflar só a
  contagem) e o piso evita níveis vazios por azar de seed.
- A densidade é distribuída **por sala**, não pela área bruta do mapa, para que mapas maiores (90×90) não
  fiquem esparsos/tediosos.
- **Entrada segura:** nenhum inimigo/hazard dentro de `SafeEntryRadius` do spawn de entrada (anti-envelopamento
  ao entrar num nível denso) — invariante de design, número tunável.
- Nenhum nível regular sai abaixo do threat mínimo da sua banda.
- Evidência: EditMode `CaveThreatBudgetTests` (budget cresce com profundidade; piso e teto respeitados;
  raio de entrada limpo; distribuição por sala; determinismo; cap não estourado).

### 14.5 Conflito inter-monstro rolado por entrada (5% base, 0,5% após o primeiro)
- `CaveEcosystemConflictPlanner.Decide(worldSeed, runSeed, caveLevel, entryIndex, hasHadConflictBefore, presentEnemyIds, balance)`:
  - a chance efetiva é `InterMonsterConflictChance` (5%) se `hasHadConflictBefore == false`, senão
    `InterMonsterConflictReducedChance` (0,5%);
  - retorna `ConflictActive=true` segundo essa chance (medido sobre amostra grande, dentro de tolerância);
  - quando ativo, escolhe **duas espécies (enemyId) DIFERENTES** presentes no nível;
  - nunca seleciona a mesma espécie para os dois lados;
  - se o nível tem <2 espécies distintas, `ConflictActive=false` (sem conflito possível);
  - o roll é seedado por `(worldSeed, runSeed, caveLevel, entryIndex)` → **reproduzível** para um dado
    índice de entrada, mas **muda a cada entrada** (re-roll por visita, como pedido).
- **Carve-out stable-run (explícito):** o conflito é um evento de **comportamento por visita**, não de
  composição. Ele NÃO altera quais inimigos existem, sua contagem, posições ou IDs (esses permanecem
  determinísticos e estáveis conforme cave-stable-run/ADR-0005); apenas marca hostilidade entre duas
  espécies já presentes durante aquela visita. O re-roll por entrada é, portanto, compatível com o
  contrato (que proíbe reroll de *composição*, não de comportamento).
- Evidência: EditMode `CaveEcosystemConflictPlannerTests` (frequência ~5% e ~0,5% ± tolerância sobre
  10k seeds; queda de 5%→0,5% quando `hasHadConflictBefore=true`; espécies sempre distintas;
  reprodutibilidade por entryIndex; variação entre entryIndex; fallback <2 espécies).

### 14.6 Comportamento de conflito em runtime (aggro dividido + dano 1/10)
- Inimigos marcados como rivais (lados A/B do conflito) tratam tanto o jogador quanto a espécie
  rival como alvos hostis válidos, e **dividem agressividade** entre eles (pesos tunáveis;
  default escolhe o alvo hostil mais próximo no raio de detecção).
- Dano causado de monstro→monstro = `dano_normal × InterMonsterDamageMultiplier` (default 0.1),
  roteado por `EnemyHealth.TakeDamage` com `sourceId` do monstro atacante.
- Dano monstro↔jogador permanece **inalterado** (multiplicador só se aplica entre monstros).
- Monstros do **mesmo** tipo nunca se atacam (só lados rivais distintos).
- **Loot em kill monstro-vs-monstro (decisão de direção):** quando um monstro mata outro, o morto
  deixa um **corpo saqueável com loot/XP reduzidos** (`InterMonsterKillLootMultiplier`, default 0.4),
  em vez de loot zero ou loot cheio. Isso evita o exploit de "tankar e deixar se matarem" (o ganho é
  reduzido e o caso é raro com dano 1/10) sem a frustração de perder o drop inteiro. O estado de morte
  e o corpo persistem via F13 (EnemyHpRecords).
- **Gancho tático (decisão de direção):** um monstro que sofreu dano de um rival ganha um status leve
  **"Ferido"** (debuff tunável, ex.: `WoundedDefenseMultiplier` default 0.85) por uma janela curta,
  dando vantagem real ao jogador que intervém. Mantém o dano inter-monstro em 1/10 mas torna o conflito
  taticamente relevante, não só visual. O status reusa o pipeline de status effects (fable_01) — não criar paralelo.
- Dano monstro↔jogador permanece **inalterado** (multiplicador inter-monstro só se aplica entre monstros).
- Evidência: EditMode `InterMonsterCombatTests` (multiplicador de dano = default exato; same-type imune;
  player damage inalterado; corpo de kill monstro-vs-monstro dropa loot×`InterMonsterKillLootMultiplier`;
  status "Ferido" aplicado ao alvo do rival) + cenário Play Mode humano.

### 14.7 Mercador errante enriquecido (determinístico)
- O estoque do `CaveWanderingMerchant` passa a variar por bioma (ofertas temáticas), com
  variedade ampliada, mantendo a aparição e a seleção determinísticas por seed.
- Evidência: EditMode `CaveWanderingMerchantStockTests` (estoque por bioma determinístico; itens
  válidos no catálogo; sem duplicata de oferta).

### 14.8 Persistência stable-run e save backward-compatible
- Snapshot persiste: posições/tipos dos elementos, estado depletado de mineráveis, presença de
  lago, e o plano/estado de conflito do nível (lados + flags) — tudo fora do `LayoutHash`.
- `CaveSaveData` ganha apenas campos aditivos; um save antigo (sem os campos) carrega sem erro e
  regenera os elementos/conflito deterministicamente.
- `GenerationConfigVersion` é incrementado (snapshots legados invalidados → regeneração limpa).
- Evidência: EditMode `CaveSaveBackCompatTests` (load de save sem os campos → defaults seguros +
  regeneração determinística; round-trip dos novos campos) + replay validator PASS.

### 14.9 Invariantes de arquitetura e stable-run
- Zero `GameObject.Find/FindObjectOfType/FindObjectsOfType/FindObjectsByType` novos em runtime.
- Toda comunicação de gameplay nova via `GameEventBus`.
- Save DTOs novos só com tipos simples + IDs estáveis (sem refs Unity).
- Nenhum GUID/timestamp em IDs de conteúdo stable-runtime (só seeds + FNV-1a).
- Evidência: `runtime-code-guard` limpo no diff; `/review-non-regression` PASS; replay validator da cave PASS.

### 14.10 Governança canônica (ADRs obrigatórios antes de implementar) 
O game-design review apontou dois conflitos com `docs/game_rules/cave_rules.md` que mudam comportamento
canônico e, por `docs-governance`, exigem ADR superseding + atualização da game_rule **na Fase 1**, antes de codar:
- **G1 — "scene must be identical on revisit" vs. consequências de conflito:** brigas alteram HP/estado de morte
  de inimigos por causa não-jogador. ADR deve declarar exceção nomeada (coberta por F13) ao "scene identical",
  e `cave_rules.md` atualizado. Sem isso, o replay validator e a spec se contradizem.
- **G2 — "4-10 resource nodes per level" vs. mais elementos mineráveis:** esta spec amplia/tem-atiza a fonte de
  minério por bioma. ADR deve superseder a contagem fixa 4-10 (definindo a nova faixa por banda) e `cave_rules.md`
  atualizado. Mineáveis novos NÃO são um pool paralelo: são a expansão temática do orçamento de resource nodes.
- Evidência: ADR(s) criados em `docs/decisions/`; `cave_rules.md` atualizado; `validate_docs.ps1` PASS;
  `required_adrs`/`required_game_rules` da spec coerentes.

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Ecosystem/
  CaveEcosystemConflictPlanner.cs      (C# puro — decide conflito + escolhe rivais, determinístico)
  CaveEcosystemConflictPlan.cs         (DTO de plano: ConflictActive, FactionAId, FactionBId, instâncias por lado)
  CaveEnvironmentElementPlanner.cs     (C# puro — coloca clusters de elementos por bioma, determinístico)
  CaveEnvironmentElementPlan.cs        (DTO: lista de CaveEnvironmentElementPlacement)
  CaveEnvironmentElementPlacement.cs   (DTO: ElementId, ElementKind, GridPosition, IsMineable, MineNodeDataId)
  CaveEnvironmentElementKind.cs        (enum: DecorNonBlocking, DecorBlocking, WaterTile, MineableNode)

Assets/_Game/Scripts/Cave/Data/
  CaveEcosystemBalanceSO.cs            (SO: conflito %, dano inter-monstro, pesos aggro, threat budget por banda, densidades)
  CaveEnvironmentElementProfileSO.cs   (SO por bioma: tipos+densidade+ore tiers permitidos)
  CaveEnvironmentElementDatabaseSO.cs  (registry dos profiles por biome/banda)

Assets/_Game/Scripts/Cave/Runtime/   (EXTENSÕES, não recriações)
  CaveEnemySpawnPlanner.cs             (+ threat budget + curva densidade do balance SO + gating aquático por lago)
  CaveRuntimeMaterializer.cs          (+ instanciar elementos ambientais; + aplicar conflito ao spawn de inimigos)
  VisitedLevelSnapshot.cs             (+ EnvironmentElements, + HasWater, + ConflictState — campos aditivos)
  CaveSnapshotService.cs              (+ capturar/restaurar os novos campos)

Assets/_Game/Scripts/Cave/Generation/
  CaveBiomeLayoutProfile.cs           (+ tamanho-por-banda escalando com profundidade)

Assets/_Game/Scripts/Enemy/          (EXTENSÕES)
  EnemyBrain.cs                       (+ targeting de rival quando em conflito; split de aggro)
  EnemyHealth.cs                      (+ caminho de dano com origem-inimigo: aplica InterMonsterDamageMultiplier; kill-by-enemy não dropa pro jogador)

Assets/_Game/Scripts/Save/
  CaveSaveData.cs                     (+ campos aditivos serializados dos novos snapshot fields)

Assets/_Game/Scripts/Core/Events/    (eventos novos)
  CaveEcosystemConflictStartedEvent.cs
  EnemyKilledByEnemyEvent.cs

Assets/_Game/Scripts/Editor/Cave/    (geradores + validators)
  GenerateCaveEcosystemBalance.cs     (cria CaveEcosystemBalance.asset com defaults)
  GenerateCaveEnvironmentElementProfiles.cs (cria 1 profile por bioma + database)
  GenerateCaveBiomeOreNodes.cs        (cria ResourceNodeDataSO de minério por banda + registra no DB)
  ValidateCaveEcosystem.cs            (valida profiles/balance/nodes: ranges, IDs, referências)

Assets/_Game/Tests/EditMode/Cave/
  CaveMapSizeScalingTests.cs
  CaveEnvironmentElementPlannerTests.cs
  CaveThreatBudgetTests.cs
  CaveEcosystemConflictPlannerTests.cs
  InterMonsterCombatTests.cs
  CaveWanderingMerchantStockTests.cs
  CaveSaveBackCompatTests.cs

Assets/_Game/Data/Cave/              (assets gerados — criados via menu de editor, DEFERRED a Unity)
  CaveEcosystemBalance.asset
  CaveEnvironmentElementProfile_<biome>.asset (×7)
  CaveEnvironmentElementDatabase.asset
  ResourceNode_<ore>_<band>.asset (vários)

docs/validation/
  fable_78_execution_report.md
docs/validation/playmode/
  fable_78_human_test_scenario.md
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
- `CaveEcosystemBalanceSO` (todas tunáveis, sem magic values):
  - `float InterMonsterConflictChance` (default 0.05) — chance por entrada quando ainda não houve conflito no nível;
  - `float InterMonsterConflictReducedChance` (default 0.005) — chance por entrada após o primeiro conflito daquele nível;
  - `float InterMonsterDamageMultiplier` (default 0.10) — dano entre monstros = dano_normal × isto;
  - `float InterMonsterKillLootMultiplier` (default 0.40) — loot/XP do corpo quando um monstro mata outro;
  - `float WoundedDefenseMultiplier` (default 0.85) + `float WoundedDurationSeconds` (default 6) — status "Ferido" aplicado a quem apanha de um rival;
  - `float PlayerAggroWeight` / `float RivalAggroWeight` (default 1.0 / 1.0 → empate = mais próximo);
  - `int[] ThreatBudgetMinByBand` (piso de threat por banda 1-7) + `int[] ThreatBudgetMaxByBand` (teto que modula qualidade: poucos fortes vs. muitos fracos, não só contagem);
  - `int[] EnemyDensityMinByBand` / `EnemyDensityMaxByBand` / `int EnemyDensityHardCap`;
  - `float[] EnvironmentElementDensityByBand` (fração de tiles candidatos viram elemento);
  - `float SafeEntryRadius` (raio sem inimigos/hazards ao redor do spawn de entrada — anti-envelopamento);
  - **Design note (não-código):** densidade e threat devem ser proporcionais à recompensa (ore/loot density) da
    banda — nível mais perigoso = mais lucrativo; e a densidade é distribuída por SALA, não pela área bruta do
    mapa, para que 90×90 não vire campo aberto vazio. Calibragem numérica final = fable_59.
- `CaveEnvironmentElementProfileSO` (1 por bioma): `BiomeId`/`Band`, lista de
  `(CaveEnvironmentElementKind, weight, mineNodeDataId?)`, `bool HasWater`, ore tiers permitidos.
- `ResourceNodeDataSO` de minério por banda: reusa o tipo existente; `RequiredToolType=Pickaxe`,
  `RequiredToolTier` crescente com a banda, `PrimaryDropItemId` = ore canônico (fable_32).
- IDs estáveis (rule id-stability): `cave_elem_<biome>_<kind>`, `resnode_ore_<ore>_<band>`,
  `cave_eco_balance`. Sem GUID/timestamp.

### 16.2 Runtime contracts
- `CaveEnvironmentElementPlan CaveEnvironmentElementPlanner.Build(CaveGeneratedLevel, profile, worldSeed, runSeed, caveLevel, playerSpawnGrid)`
  — puro, determinístico, não bloqueia path (BFS).
- `CaveEcosystemConflictPlan CaveEcosystemConflictPlanner.Decide(worldSeed, runSeed, caveLevel, int entryIndex, bool hasHadConflictBefore, IReadOnlyList<string> presentEnemyIds, CaveEcosystemBalanceSO)`
  — puro, sem estado; o roll por entrada é seedado por `(worldSeed, runSeed, caveLevel, entryIndex)`. O caller
  (materializer) fornece `entryIndex` (contador de entradas persistido) e `hasHadConflictBefore` (flag persistida).
- `int CaveEnemySpawnPlanner.ResolveThreatBudget(band, caveLevel, balance)` + preenchimento até o piso.
- `EnemyBrain`: novo modo de targeting "conflict-aware" — método interno que resolve o alvo
  hostil mais próximo entre {player, rivais} respeitando os pesos; quando não há conflito, o
  caminho atual (só player) é preservado intacto.
- `EnemyHealth.TakeDamage`: quando `DamageRequest.SourceIsEnemy` (ou sourceId resolve a um inimigo),
  aplica `InterMonsterDamageMultiplier`, aplica o status "Ferido" (`WoundedDefenseMultiplier`/`WoundedDurationSeconds`
  via pipeline fable_01) e, se for kill, marca a morte como "by enemy" → corpo dropa loot×`InterMonsterKillLootMultiplier`
  (caminho reduzido reusando `EnemyDropSpawner`), publica `EnemyKilledByEnemyEvent` (não `EnemyKilledEvent`).

### 16.3 Event contracts (novos — additivos)
- `CaveEcosystemConflictStartedEvent { int CaveLevel; string FactionAEnemyId; string FactionBEnemyId; }`
  publicado na materialização quando o nível tem conflito ativo (HUD/SFX/telemetria podem assinar).
- `EnemyKilledByEnemyEvent { string VictimInstanceId; string KillerInstanceId; int CaveLevel; }`
  publicado quando um monstro mata outro (dispara o drop reduzido do corpo, não a rota normal de loot do jogador).
- Padrão unsubscribe obrigatório para qualquer subscriber novo (OnDisable/OnDestroy).
- Reuso: `EnemyKilledEvent` permanece o caminho de loot **cheio apenas** para kills causados pelo jogador;
  o corpo de kill monstro-vs-monstro usa o caminho reduzido (`InterMonsterKillLootMultiplier`).

### 16.4 Save contracts
- `VisitedLevelSnapshot` ganha (aditivo, fora do LayoutHash):
  `List<SerializedEnvironmentElement> EnvironmentElements`, `bool HasWater`,
  `CaveConflictSnapshot ConflictState` (ConflictActive, FactionAId, FactionBId, **bool HasHadConflict**, **int EntryCount**).
  `EntryCount` é incrementado a cada entrada no nível; `HasHadConflict` vira `true` na primeira vez que um
  conflito é rolado naquele nível (gatilho da queda 5%→0,5%). Como o conflito é comportamento por visita,
  `ConflictActive`/`FactionAId`/`FactionBId` refletem **a visita atual** e podem mudar entre entradas — isso é
  esperado e não fere o stable-run de composição.
- `CaveSaveData` espelha esses campos em DTOs `[Serializable]` de tipos simples.
- Backward-compat: ausência dos campos → defaults seguros + regeneração determinística.
- `GenerationConfigVersion++` (invalida snapshots legados — regeneração limpa, comportamento já suportado).
- Migração: NÃO necessária (aditivo + version bump já cobre).

### 16.5 UI contracts
- N/A de modal novo. **Feedback mínimo OBRIGATÓRIO** (não opcional): ao iniciar um conflito, um toast no HUD
  existente (ex.: "Criaturas em conflito!") via `NotificationToastRequestedEvent`/`PlayerActionFeedbackEvent`,
  assinando `CaveEcosystemConflictStartedEvent`. Sem feedback, a feature (cara, Fase 4) lê como bug
  (rule game-feel-checklist). VFX/SFX de juice ficam para um polish follow-up.

## 17. Sistemas afetados

```text
Cave generation (layout size)
Cave runtime materialization
Cave enemy spawn planning (densidade/threat/aquático)
Cave resource/mineração (nodes por bioma)
Cave loot snapshot (depleção idempotente)
Cave wandering merchant (estoque)
Enemy AI (targeting/aggro)
Combat damage (dano inter-monstro)
Save/load (snapshot + CaveSaveData)
Event bus (2 eventos novos)
Editor tooling (geradores + validator)
Validation reports
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Enemy/EnemyBrain.cs
Assets/_Game/Scripts/Enemy/EnemyHealth.cs
Assets/_Game/Scripts/Enemy/EnemyPackCoordinator.cs           (somente se necessário p/ alerta de conflito)
Assets/_Game/Scripts/Save/CaveSaveData.cs
Assets/_Game/Scripts/Core/Events/CaveEcosystemConflictStartedEvent.cs   (novo)
Assets/_Game/Scripts/Core/Events/EnemyKilledByEnemyEvent.cs             (novo)
Assets/_Game/Scripts/Editor/Cave/**                          (geradores + validator novos)
Assets/_Game/Tests/EditMode/Cave/**
Assets/_Game/Data/Cave/**                                    (SOMENTE data assets gerados via menu de editor)
docs/validation/fable_78_execution_report.md
docs/validation/playmode/fable_78_human_test_scenario.md
```

## 19. Arquivos proibidos

```text
Assets/**/*.unity                 (sem autorização explícita — usar editor scripts/menus se preciso)
Assets/**/*.prefab                (idem)
Qualquer .asset fora de Assets/_Game/Data/Cave/**
Assets/_Game/Scripts/Save/SaveManager.cs   (NÃO reescrever; mudanças de schema via CaveSaveData aditivo só)
docs_old/**, docs/archive/**
Packages/**, ProjectSettings/**
.specs/SPEC_EXECUTION_ORDER.md como se a spec já estivesse implementada
```

## 20. Estratégia de implementação (fases)

### Fase 0 — Auditoria (sem código)
Confirmar assinaturas reais e pontos de extensão: `EnemyBrain` target resolution, `EnemyHealth.TakeDamage`
origem/sourceId, `CaveEnemySpawnPlanner.CreatePlan` retorno, `VisitedLevelSnapshot`/`CaveSnapshotService`
captura/restore, `CaveBiomeLayoutProfile.ResolveMapSize`, `CaveWanderingMerchant` seleção. Rodar
`system-reuse-audit`. Produzir nota curta no report (Phase 0) confirmando "não recriar".

### Fase 1 — Governança canônica + Balance SO + data contracts
**Primeiro (gate de governança):** criar os ADRs G1 (carve-out stable-run "scene identical" p/ consequências de
conflito) e G2 (superseder "4-10 resource nodes" por faixa por banda) e atualizar `docs/game_rules/cave_rules.md`.
Sem isso, a spec contradiz a game_rule canônica (`docs-governance`). Depois: `CaveEcosystemBalanceSO`,
`CaveEnvironmentElementProfileSO`, `CaveEnvironmentElementDatabaseSO`, enums e
DTOs de plano/placement. Geradores de editor para criar os assets com defaults + nodes de minério por banda.
Validator `ValidateCaveEcosystem`.

### Fase 2 — Tamanho-por-banda + threat budget + densidade
Estender `CaveBiomeLayoutProfile` (size escalando com profundidade, variação preservada) e
`CaveEnemySpawnPlanner` (threat budget + curva de densidade do balance SO + gating aquático por presença
de lago). Determinismo via seeds existentes. EditMode tests da fase.

### Fase 3 — Elementos ambientais (planner + materialização + snapshot)
`CaveEnvironmentElementPlanner` (BFS de não-bloqueio, clusters por perfil), materialização em
`CaveRuntimeMaterializer` (decor/water/mineable; mineáveis via `ResourceNode` + `CaveLootSnapshotService`),
persistência aditiva em `VisitedLevelSnapshot`/`CaveSnapshotService`/`CaveSaveData`. EditMode tests + back-compat.

### Fase 4 — Conflito inter-monstro (ALTO RISCO — gate de revisão)
`CaveEcosystemConflictPlanner` (decisão 5% + rivais distintos), aplicação na materialização (marca lados
A/B por `FactionId`), targeting de rival no `EnemyBrain` (split de aggro), dano inter-monstro em
`EnemyHealth` (×0.1, kill-by-enemy sem loot), eventos. Integração com F13 (HP persist). EditMode tests.
**Parar e revisar (architecture-reviewer + non-regression) antes da Fase 5.**

### Fase 5 — Mercador errante enriquecido
Estoque temático por bioma + variedade, determinístico. EditMode tests.

### Fase 6 — Testes/validação/relatório
Suite EditMode completa, validators, replay validator da cave, build runtime+editor, docs validation,
cenário humano de Play Mode, execution report com Testing Quality Gate + status honesto.

## 21. Ordem segura de execução

```text
1. Phase 0 audit (confirmar pontos de extensão; não recriar).
2. Fase 1: contratos/SO/enums/DTOs + geradores + validator.
3. Fase 2: size-por-banda + threat budget + densidade (determinístico) + tests.
4. Fase 3: elementos ambientais + materialização + snapshot/save aditivo + tests + back-compat.
5. GATE: build + replay validator PASS antes de tocar combat.
6. Fase 4: conflito inter-monstro (planner → materialização → targeting → dano → eventos) + tests.
7. GATE: architecture-reviewer + non-regression-review (targeting/dano/stable-run).
8. Fase 5: merchant stock + tests.
9. Fase 6: validações completas + replay validator + cenário humano + report.
```

## 22. Paralelização

```text
- Parallelizable: NO
- Parallel group: N/A
- Can run with: specs de docs puras; UI que não toque Cave/Enemy/Combat.
- Must not run with: qualquer spec tocando CaveEnemySpawnPlanner, CaveBiomeLayoutProfile, CaveRuntimeMaterializer,
  EnemyBrain, EnemyHealth, VisitedLevelSnapshot, CaveSnapshotService, CaveSaveData, ou geração procedural da cave.
- Shared files/systems requiring lock: todo o lock scope do cabeçalho.
- Reason: altera contratos centrais de cave generation, enemy AI/dano e save da cave — risco de conflito alto.
```

## 23. Impacto em save/load

```text
Does this change save schema? YES (aditivo, não destrutivo)
Does this add a save section? NO (estende CaveSaveData / snapshot existente)
Does this require migration? NO (campos aditivos + GenerationConfigVersion bump regenera legado)
Does this persist Unity references? MUST BE NO (apenas IDs/posições/flags simples)
```
Owner do estado: `CaveSnapshotService`/`CaveRunManager` (como hoje). Restore order: inalterada;
novos campos restaurados junto do snapshot do nível. Save antigo sem os campos → defaults + regeneração
determinística (sem crash, sem perda do save).

## 24. Impacto em eventos

```text
Adds events: YES (CaveEcosystemConflictStartedEvent, EnemyKilledByEnemyEvent)
Changes existing events: NO (EnemyKilledEvent permanece caminho de loot só para kills do jogador)
Requires unsubscribe pattern: YES (qualquer subscriber novo desinscreve em OnDisable/OnDestroy)
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO (apenas eventos opcionais p/ HUD/toast existente)
Changes scenes: NO via edição manual de .unity (materialização é runtime; CaveScene já hospeda o materializer)
Changes prefabs: prefabs de elemento ambiental podem ser necessários — criar via editor script/PrefabUtility, não YAML manual
Changes ScriptableObjects/assets: YES (data assets novos via geradores de editor)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: re-roll de conflito por entrada ser confundido com reroll de composição → falso alarme de stable-run.
  Mitigação: conflito é COMPORTAMENTO por visita, não composição; não altera quais inimigos existem, contagem,
  posições ou IDs. Roll seedado por (worldSeed,runSeed,caveLevel,entryIndex) é reproduzível e testável.
  Carve-out documentado em 14.5; replay validator continua checando composição (que permanece estável).

Risco: targeting de rival no EnemyBrain quebrar o caminho atual (player-only) ou introduzir GameObject.Find.
  Mitigação: modo conflict-aware isolado; quando sem conflito, código atual intacto; rivais injetados pelo materializer (sem global search). architecture-reviewer no gate da Fase 4.

Risco: dano inter-monstro reentrar no loot do jogador (farm grátis) ou corromper XP.
  Mitigação: kill-by-enemy publica EnemyKilledByEnemyEvent (não EnemyKilledEvent); sem AddItem/XP ao jogador; EditMode cobre.

Risco: HP de monstros alterado por briga + F13 → inconsistência ao revisitar.
  Mitigação: reusar EnemyHpRecords (HP fora do LayoutHash já é o contrato F13); test de round-trip.

Risco: elementos bloquearem o caminho entrance↔exit (softlock).
  Mitigação: BFS de conectividade igual ao CaveHazardPlanner; test de não-bloqueio.

Risco: GenerationConfigVersion bump invalida saves em andamento de testers.
  Mitigação: comportamento esperado e já suportado (regeneração determinística); documentar no report.

Risco: performance (mais elementos + briga de monstros = mais Update/colisões).
  Mitigação: decor não-bloqueante sem lógica por frame; rival targeting reusa o tick existente; performance-auditor na Fase 6 se necessário.
```

## 27. Rollback

```text
- Remover scripts novos (Cave/Ecosystem/**, novos SO/eventos/editor/tests).
- Reverter extensões em CaveBiomeLayoutProfile/CaveEnemySpawnPlanner/CaveRuntimeMaterializer/EnemyBrain/EnemyHealth/VisitedLevelSnapshot/CaveSnapshotService/CaveSaveData.
- Reverter GenerationConfigVersion ao valor anterior.
- Remover data assets gerados em Assets/_Game/Data/Cave/**.
- Saves existentes permanecem válidos (campos aditivos ignorados ao reverter).
- Não apagar save real do usuário.
```

---

# /speckit.tasks

## 28. Tasks

```text
- [ ] T001 — Phase 0: auditar EnemyBrain target resolution, EnemyHealth.TakeDamage origem, CaveEnemySpawnPlanner retorno, snapshot/save, ResolveMapSize, merchant, pipeline de status (fable_01). Registrar "não recriar".
- [ ] T001b — Governança: criar ADR G1 (carve-out stable-run) + ADR G2 (superseder 4-10 resource nodes) + atualizar cave_rules.md; validate_docs PASS.
- [ ] T002 — Criar CaveEcosystemBalanceSO (constantes tunáveis, incl. loot reduzido/ferido/threat min-max/safe-entry) + gerador de editor + asset default.
- [ ] T003 — Criar CaveEnvironmentElementProfileSO + Database + enum CaveEnvironmentElementKind + DTOs de placement.
- [ ] T004 — Gerador de editor: 7 profiles por bioma + ResourceNodeDataSO de minério por banda + registro no DB; validator ValidateCaveEcosystem.
- [ ] T005 — Estender CaveBiomeLayoutProfile: tamanho escalando com profundidade (determinístico, variação preservada).
- [ ] T006 — Estender CaveEnemySpawnPlanner: ResolveThreatBudget + curva de densidade do balance SO + gating aquático por presença de lago.
- [ ] T007 — CaveEnvironmentElementPlanner (puro, BFS não-bloqueio, clusters por perfil) + CaveEnvironmentElementPlan.
- [ ] T008 — Materializar elementos em CaveRuntimeMaterializer (decor/water/mineable; mineável via ResourceNode + CaveLootSnapshotService).
- [ ] T009 — Persistência aditiva: VisitedLevelSnapshot + CaveSnapshotService + CaveSaveData (campos novos, tipos simples) + GenerationConfigVersion++.
- [ ] T010 — CaveEcosystemConflictPlanner.Decide (re-roll por entrada: 5%→0,5% após o 1º conflito; seed por entryIndex; 2 espécies distintas; fallback <2 espécies). Persistir HasHadConflict + EntryCount no snapshot.
- [ ] T011 — Aplicar conflito na materialização: marcar lados A/B por FactionId + publicar CaveEcosystemConflictStartedEvent.
- [ ] T012 — EnemyBrain: targeting conflict-aware (split de aggro player vs rival; caminho player-only intacto sem conflito).
- [ ] T013 — EnemyHealth: caminho de dano com origem-inimigo (×InterMonsterDamageMultiplier; same-type imune; aplica status "Ferido"; kill-by-enemy → EnemyKilledByEnemyEvent + corpo com loot×InterMonsterKillLootMultiplier via EnemyDropSpawner).
- [ ] T013b — Assinar CaveEcosystemConflictStartedEvent → toast de HUD obrigatório (NotificationToastRequestedEvent/PlayerActionFeedbackEvent).
- [ ] T014 — Enriquecer CaveWanderingMerchant: estoque temático por bioma + variedade (determinístico).
- [ ] T015 — EditMode tests: MapSizeScaling, EnvironmentElementPlanner, ThreatBudget, EcosystemConflictPlanner, InterMonsterCombat, WanderingMerchantStock, SaveBackCompat.
- [ ] T016 — Replay validator da cave + builds (runtime+editor) + docs validation; architecture-reviewer + non-regression no gate da Fase 4.
- [ ] T017 — Cenário humano de Play Mode (docs/validation/playmode/fable_78_human_test_scenario.md) + execution report com Testing Quality Gate.
```

## 29. Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1   # exit 0 obrigatório p/ BUILD_VALIDATED
```
Unity compile + replay validator da cave (quando o Editor estiver disponível):
```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
# + menu de editor: ValidateCaveEcosystem + replay validator stable-run (FASE9F)
```
Unity Test Runner — EditMode (suite Cave). Se algum comando não puder rodar: registrar `NOT RUN`
com motivo e risco residual (rule validation-truth).

## 30. Testing Quality Gate

```md
- Changed deterministic logic: YES (planners, size, threat, conflito, dano)
- Requires EditMode tests: YES (T015 — 7 suites)
- Requires PlayMode automated or final human scenario: YES (conflito/AI/materialização/scene)
- Requires regression test: YES (back-compat de save; stable-run replay)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: run_strict_validation exit 0; Assembly-CSharp + Editor PASS;
  7 EditMode suites PASS; replay validator stable-run PASS; cenário humano de Play Mode executado
  (conflito visível, dano 1/10, sem loot em kill monstro-vs-monstro, elementos/biomas materializados,
  minério mineável, lago habilita aquático, save round-trip e back-compat).
```

## 31. Definition of Done

```text
Spec implementada dentro dos arquivos permitidos; nenhum arquivo proibido alterado.
Todos os critérios de aceite 14.1–14.9 atendidos com evidência.
7 suites EditMode PASS; replay validator stable-run PASS.
Builds runtime+editor PASS (exit 0); docs validation PASS (ou EXPECTED_FAIL_LEGACY_ONLY).
Cenário humano de Play Mode criado (validação humana deferida ao lote).
Execution report com Spec Compliance Matrix + Testing Quality Gate + status honesto (máx. BUILD_VALIDATED sem Play Mode humano).
Sem claim de ACCEPTED sem evidência de Play Mode humano.
```

## 32. Anti-regressão

```text
- Não alterar o caminho de target player-only do EnemyBrain quando NÃO há conflito.
- Não alterar EnemyKilledEvent nem o pipeline de loot do jogador.
- Não serializar referência Unity em save; só IDs/posições/flags.
- Não usar GameObject.Find/FindObjectOfType em runtime novo.
- Não renomear IDs existentes (rule id-stability); novos IDs com prefixo de domínio.
- Não quebrar stable-run: conflito/elementos/tamanho determinísticos; HP fora do LayoutHash (F13).
- Não introduzir magic balance values; tudo em CaveEcosystemBalanceSO/const nomeada.
- Save antigo deve carregar sem erro (back-compat test obrigatório).
```

## 33. Notas para execução posterior

```text
- Esta spec entrega curvas determinísticas TUNÁVEIS; o balance final assinado fica para tuning + telemetria (fable_59).
- Arte final dos elementos/biomas é spec futura (art pass) — esta entrega usa placeholders por família/bioma.
- Toast/HUD de "conflito de criaturas" é opcional aqui; um polish de feedback pode virar follow-up.
- Conflito multi-facção (>2 espécies) é fora de escopo; o contrato suporta extensão futura.
- Validação humana é deferida ao final do lote (não bloquear micro-execução).
```

## 34. Checklist final da spec pronta

```text
[x] Cabeçalho completo.
[x] Declara Parallelizable / group / locks.
[x] Declara fontes obrigatórias.
[x] Declara estado atual do repo + "não recriar".
[x] Escopo pequeno-o-suficiente por fase (multi-fase, com gates).
[x] Fora de escopo explícito.
[x] Regras de não duplicação.
[x] Arquivos permitidos e proibidos.
[x] Contratos data/runtime/eventos/save/UI.
[x] Critérios de aceite verificáveis (14.1–14.9).
[x] Validações obrigatórias.
[x] Testing Quality Gate.
[x] Validação humana DEFERRED_TO_FINAL_VALIDATION.
[x] Riscos e rollback.
[x] Não pede execução humana intermediária.
[x] Não altera SPEC_EXECUTION_ORDER como se implementada.
```
