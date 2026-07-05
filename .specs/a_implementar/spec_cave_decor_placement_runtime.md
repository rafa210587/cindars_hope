# SPEC — Cave Decor Art Pass (mapa de sprite para o decor da fable_78)

> **Spec ID:** `spec_cave_decor_placement_runtime`
> **Status:** A implementar
> **Wave:** CAVE_VISUALS — art pass do decor
> **Priority:** P1
> **Type:** Runtime / Data / Editor
> **Domain:** Cave
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** specs docs-only
> **Must not run with:** specs tocando `CaveEnvironmentElementMaterializer`, `CaveBiomeArtProfileSO` ou `CaveRuntimeMaterializer`
> **Repo lock scope:** `Assets/_Game/Scripts/Cave/Art/**`, `Assets/_Game/Scripts/Cave/Runtime/CaveEnvironmentElementMaterializer.cs`, `Assets/_Game/Scripts/Editor/Cave/GenerateCaveBiomeArtProfiles.cs`, `Assets/_Game/Data/Cave/Biomes/**`
> **Ordem de execucao:** depois da fable_78 (JÁ implementada/commitada) e do lote 2/3 de arte (sprites prop_*/chunk_* no disco)
> **Depende de:**
> - `fable_78` (JÁ IMPLEMENTADA — CaveEnvironmentElementPlanner/Materializer/Kind/snapshot existem e rodam)
> - `spec_cave_biome_art_profiles_runtime` (CV01 — CaveBiomeArtProfileSO/resolver)
> - ADR-0005 (cave stable run)
> **Bloqueia:** nada
> **Scope:** dar arte real ao decor que a fable_78 JÁ posiciona — um pool de sprites de decor por bioma (dividido por Kind: DecorNonBlocking/DecorBlocking) no CaveBiomeArtProfileSO, consumido pelo CaveEnvironmentElementMaterializer (que hoje usa um prefab genérico/sprite builtin), com seleção determinística por hash estável da posição do elemento.
> **Out of scope:** planner de colocação (fable_78 já tem), snapshot/save (fable_78 já persiste), WaterTile/MineableNode art, conflito/threat/mercador, densidade (fable_78 controla), Rule Tile de borda de parede.

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## 5. Contexto

**Descoberta 2026-07-04 (Phase 0):** a `fable_78` já está implementada e commitada. O sistema de decor
ambiental **funciona** — `CaveEnvironmentElementPlanner` coloca elementos deterministicamente,
`CaveEnvironmentElementMaterializer` os instancia, o snapshot persiste (revisita estável). MAS o
materializer usa um único `_decorElementPrefab` (SerializeField); quando null na cena, cai num sprite
builtin minúsculo com cor chapada. Resultado: o decor É posicionado, mas **invisível/genérico** — a
sala parece vazia e nenhum dos sprites reais (`prop_mine_cart`, `chunk_rubble`, `chunk_mushrooms_giant`
etc., já no disco) é usado.

A `ElementEntry` da fable_78 (`Kind`/`Weight`/`MineNodeDataId`) não tem campo de arte, de propósito
(a fable_78 declara "arte final = spec futura de art pass"). O ElementId é posicional
(`cave_elem_{level}_{x}_{y}_{kind}`), não semântico — então a arte é um **pool por Kind**, não um
mapa 1:1 por tipo.

Esta spec É essa art pass: a menor mudança que faz o decor já-posicionado mostrar os sprites reais.

## 6. Problema

Sem isto: os sprites de decor gerados (caros, via GPT) ficam órfãos; a caverna não reflete a keyart
(o objetivo visual validado) apesar de todo o sistema de colocação já existir e rodar.

## 7. Objetivo

O `CaveEnvironmentElementMaterializer` passa a resolver, para cada placement de decor, um sprite real
do **pool do bioma** (por Kind), escolhido deterministicamente pela posição estável do elemento —
mantendo o fallback atual quando o bioma não tem pool. Bioma 1 (Caverna de Pedra) mostra os props reais
espalhados; biomas 2–8 seguem no fallback até terem arte. Zero mudança no planner/snapshot/densidade.

## 8. Fontes obrigatórias lidas

```text
.specs/a_implementar/fable/fable_78_spec_cave_ecosystem_population_runtime.md  (§15-16 — sistema existente)
.specs/a_implementar/spec_cave_biome_art_profiles_runtime.md  (CV01)
docs/design/gameplay/cave/CAVE_BIOME_VISUAL_REFERENCE.md
.claude/rules/cave-stable-run.md
.claude/skills/cave-stable-run-guard/SKILL.md
```

## 9. Estado atual do repo (confirmado na Phase 0 desta reescrita)

```text
EXISTE E RODA (reusar, NÃO tocar a lógica):
- CaveEnvironmentElementPlanner (Cave/Ecosystem/) — coloca decor determinístico + BFS não-bloqueio. NÃO MEXER.
- CaveEnvironmentElementMaterializer (Cave/Runtime/) — instancia decor; usa _decorElementPrefab genérico. ESTENDER: resolver sprite do pool.
- CaveEnvironmentElementProfileSO.ElementEntry — Kind/Weight/MineNodeDataId (sem arte). NÃO adicionar arte AQUI (é gameplay/densidade).
- VisitedLevelSnapshot.EnvironmentElements / CaveSaveData — persistência. NÃO MEXER.
- SerializedEnvironmentElement.ElementId = cave_elem_{level}_{x}_{y}_{kind} (posicional).
- CaveBiomeArtProfileSO + CaveBiomeArtResolver (CV01) — DONO da arte por bioma. ESTENDER: pool de decor.
- CaveLayoutStableHash (FNV-1a) — para o pick determinístico por posição.
- Sprites no disco (biome_stone_cavern/): prop_mine_cart, prop_broken_pickaxe, prop_planks_rail,
  prop_water_puddle, chunk_rubble, chunk_ore_mound, chunk_mushrooms_giant, chunk_stalactites; + rock_ore_0-5, mushroom_cluster reutilizáveis.

NÃO EXISTE (criar):
- Pool de sprites de decor por bioma no CaveBiomeArtProfileSO (2 listas: NonBlocking, Blocking).
- Resolução no materializer: sprite do pool por (Kind, hash estável da posição do elemento).
```

## 11. Escopo (inclui)

```text
- CaveBiomeArtProfileSO: 2 listas de Sprite de decor (DecorNonBlockingSprites, DecorBlockingSprites) + setter editor-only.
- CaveBiomeArtResolver: TryGetDecorSprite(bandId, CaveEnvironmentElementKind, long stableHash, out Sprite) — pick determinístico index = hash % pool.Length; null-safe (pool vazio → false).
- CaveEnvironmentElementMaterializer: no MaterializeDecorElement, ANTES do fallback do _decorElementPrefab, tentar resolver o sprite via CaveBiomeArtResolver usando a banda do nível + Kind + StableHash da posição do elemento; sucesso → usar esse sprite; falha → manter comportamento atual (prefab/builtin). O materializer recebe o resolver por injeção (mesmo padrão do CaveTileMaterializer, sem GameObject.Find).
- GenerateCaveBiomeArtProfiles: preencher os 2 pools do bioma 1 por convenção de pasta —
  NonBlocking = chunk_mushrooms_giant, chunk_stalactites, prop_mine_cart, prop_broken_pickaxe, prop_planks_rail, prop_water_puddle, mushroom_cluster;
  Blocking = chunk_rubble, chunk_ore_mound, rock_ore_0..5 (massas que ocupam célula). Documentar a classificação.
- ValidateCaveBiomeArtProfiles: WARNING se os pools estiverem vazios num bioma que tem sprites na pasta.
- EditMode test: determinismo do pick (mesma banda+kind+hash → mesmo índice), null-safety (pool vazio → false).
```

## 12. Fora de escopo

```text
- Planner/colocação/densidade/snapshot (fable_78 — NÃO tocar).
- Arte de WaterTile/MineableNode (o materializer já tem caminho próprio; esta spec cobre só Decor*).
- Semântica 1:1 por tipo (ElementId é posicional; pool por Kind é o contrato).
- Rule Tile de borda; biomas 2–8; conflito/mercador/threat.
```

## 13. Regras de não duplicação

```text
- NÃO recriar planner/materializer/snapshot da fable_78 — só ESTENDER o materializer com a resolução de sprite.
- NÃO adicionar arte na ElementEntry da fable_78 (arte mora no CaveBiomeArtProfileSO da CV01).
- NÃO criar novo RNG/hash — CaveLayoutStableHash.
- NÃO usar GameObject.Find/FindObjectOfType; resolver injetado.
```

## 14. Critérios de aceite

### 14.1 Sprite real por Kind, determinístico
- Cada placement de DecorNonBlocking/DecorBlocking do bioma 1 renderiza um sprite real do pool respectivo.
- O pick é determinístico por (banda, Kind, hash estável da posição) → revisita idêntica (stable-run).
- Evidência: EditMode `CaveDecorSpritePoolTests` (determinismo do índice; pool vazio → false).

### 14.2 Fallback preservado
- Bioma sem pool → materializer mantém o comportamento atual (prefab/builtin), sem erro.
- Evidência: verificação estática do diff do materializer (caminho de fallback intacto).

### 14.3 Builds
- `dotnet build` runtime+editor exit 0.

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Art/
  CaveBiomeArtProfileSO.cs        (+ 2 listas de Sprite de decor + setter)
  CaveBiomeArtResolver.cs         (+ TryGetDecorSprite(band, kind, stableHash))
Assets/_Game/Scripts/Cave/Runtime/
  CaveEnvironmentElementMaterializer.cs   (+ resolver sprite do pool antes do fallback; receber resolver por injeção)
  CaveRuntimeMaterializer.cs      (passar o CaveBiomeArtResolver ao construir o CaveEnvironmentElementMaterializer)
Assets/_Game/Scripts/Editor/Cave/
  GenerateCaveBiomeArtProfiles.cs (+ preencher pools do bioma 1 por convenção)
  ValidateCaveBiomeArtProfiles.cs (+ WARNING pool vazio)
Assets/_Game/Tests/EditMode/Cave/
  CaveDecorSpritePoolTests.cs
docs/validation/spec_cave_decor_placement_execution_report.md
docs/validation/playmode/spec_cave_decor_placement_human_test_scenario.md
```

## 16. Contratos

- **Runtime:** `bool CaveBiomeArtResolver.TryGetDecorSprite(int bandId, CaveEnvironmentElementKind kind, long stableHash, out Sprite sprite)` — puro, index = stableHash % pool.Length.
- **Save/Eventos/UI:** N/A (nada persiste novo; arte é derivada; fable_78 já persiste o placement).

## 20. Estratégia (fases)

```text
Fase 0 — Confirmar (feito nesta reescrita): fable_78 implementada; materializer usa prefab genérico; ElementId posicional.
Fase 1 — Pools no CaveBiomeArtProfileSO + TryGetDecorSprite no resolver + EditMode test.
Fase 2 — Estender CaveEnvironmentElementMaterializer (resolver injetado; sprite do pool antes do fallback) + CaveRuntimeMaterializer passa o resolver.
Fase 3 — GenerateCaveBiomeArtProfiles preenche pools do bioma 1 + validator WARNING.
Fase 4 — Builds + docs validation + cenário humano + report.
```

## 22. Paralelização

Parallelizable: NO. Must not run with specs tocando o materializer de decor, o profile de arte ou o CaveRuntimeMaterializer.

## 23. Impacto save/load

```text
Changes save schema? NO (arte é derivada; fable_78 já persiste o placement). Persists Unity refs? NO.
```

## 25. Impacto UI/Unity

```text
Changes UI/scenes/prefabs manual? NO. Changes SO/assets? YES (pools no profile, via gerador). Play Mode final? YES. Human timing: DEFERRED_TO_FINAL_VALIDATION.
```

# /speckit.tasks

## 28. Tasks

```text
- [ ] T001 — Pools de decor (NonBlocking/Blocking) no CaveBiomeArtProfileSO + setter editor-only.
- [ ] T002 — TryGetDecorSprite no CaveBiomeArtResolver (pick determinístico por hash) + EditMode test.
- [ ] T003 — Estender CaveEnvironmentElementMaterializer: resolver injetado; sprite do pool antes do fallback; fallback intacto.
- [ ] T004 — CaveRuntimeMaterializer passa o CaveBiomeArtResolver ao materializer de decor.
- [ ] T005 — GenerateCaveBiomeArtProfiles preenche os 2 pools do bioma 1 por convenção + ValidateCaveBiomeArtProfiles WARNING pool vazio.
- [ ] T006 — Builds + docs validation + cenário humano + execution report.
```

## 29. Validações obrigatórias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
# Unity Test Runner EditMode (Cave) quando o Editor estiver disponível
```

## 30. Testing Quality Gate

```md
- Changed deterministic logic: YES (pick de sprite por hash)
- Requires EditMode tests: YES (determinismo + null-safety)
- Requires PlayMode automated or final human scenario: YES (sala povoada com props reais; revisita estável)
- Requires regression test: YES (fallback com pool vazio)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: builds exit 0 + EditMode PASS + Play Mode humano do cenário
```

## 31. Definition of Done

Pools + resolução implementados nos arquivos permitidos; planner/snapshot da fable_78 intocados; bioma 1
mostra props reais; builds exit 0; report criado; status máximo BUILD_VALIDATED (Play Mode deferido).

## 32. Anti-regressão

```text
- NÃO tocar CaveEnvironmentElementPlanner/snapshot/densidade (fable_78).
- NÃO usar Random/GetHashCode para o pick — só CaveLayoutStableHash.
- Fallback do materializer (prefab/builtin) preservado quando pool vazio.
- Sem refs Unity no save; sem GameObject.Find.
```
