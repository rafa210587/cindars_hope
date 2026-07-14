---
doc_type: validation
status: evidence
spec_id: spec_cave_visual_polish_runtime
validation_type: automated
result: BUILD_VALIDATED
date: 2026-07-10
executor: Claude Code (spec-implementer)
source_of_truth: false
validated_adrs: [ADR-0005]
validated_game_rules: [cave_rules.md]
---

# Execution Report — CV04 Cave Visual Polish: bordas de parede, cascalho, decor de parede e luz fake

> **This report is evidence, NOT an execution queue.**
> **Do not re-run the spec based on this report alone.**

---

## Phase 0 — Audit (confirmado no código antes de qualquer edição)

- `CaveTileMaterializer` (Cave/Runtime): já pintava floor/wall Tilemap por bioma (CV01) e já tinha o
  padrão de classificação-de-vizinhança pura e testável (`IsWallInterior`, `IsFloorEdgeNextToWall`) +
  tint por-célula (`WallInteriorTint`/`WallTopEdgeTint`/`FloorEdgeShadowTint`) — **reusado** como
  precedente direto para os 3 overlays novos desta spec (mesmo padrão: função pura testável + efeito
  colateral de instanciar `GameObject`/`SpriteRenderer` sem collider, sem Random).
- `CaveBiomeArtProfileSO`/`CaveBiomeArtResolver` (CV01/CV02/CV03): pools opcionais por bioma, null-safe,
  resolvidos por `CaveLayoutStableHash` — **estendido** com 3 pools novos (`WallEdgeSprites[4]`,
  `GroundScatterSprites[]`, `WallSurfaceSprites[]`) + 1 sprite único (`LightShaftSprite`), sem tocar os
  pools/contratos existentes (CV01-CV03 preservados byte-for-byte).
- `CaveDecorPlacementContext`/`CaveDecorContextClassifier` (CV03): enum + classificador puro de
  contexto de célula (FloorCluster/WallHug/CeilingHang) — **estendido** com 2 valores novos
  (`GroundScatter`, `WallSurface`) + 2 métodos puros novos (`IsGroundScatterCell`, `IsWallSurfaceCell`),
  reusando `Classify`/`HasAdjacentWall` em vez de duplicar geometria.
- `CaveEnvironmentElementPlanner`/`CaveEnvironmentElementMaterializer` (fable_78/CV02/CV03): pipeline
  de "elemento ambiental" com `ElementEntry` autorada por bioma + budget/`used`-set + persistência em
  `SerializedEnvironmentElement` (snapshot stable-run). **NÃO estendido** para GroundScatter/WallSurface
  — ver "Decisão de arquitetura" abaixo (desvio consciente do plano da seção 15 da spec).
- `CaveEcosystemBalanceSO`: já tinha `FloorClusterDensityMultiplier` (CV03) no mesmo padrão de campo
  tunável com `OnValidate` clamp — **reusado** o padrão para os 2 campos novos
  (`GroundScatterDensity`, `WallSurfaceChance`).
- `GenerateCaveBiomeArtProfiles`/`ValidateCaveBiomeArtProfiles`: convenção de pasta
  `Art/Generated/World/cave/<biomeId>/*.png` + WARNING (nunca ERROR) para pool vazio com pasta de arte
  presente — **reusado**, só adicionando as novas convenções de nome de arquivo
  (`wall_edge_*`, `litter_*`, `light_shaft`).
- `CaveRuntimeMaterializer`: já resolve `CaveBiomeArtResolver`/`CaveEcosystemBalanceSO` com fallback
  Resources + guard one-shot de log — **reusado** sem mudar o mecanismo de resolução; só passei os
  parâmetros novos (`worldSeed`/`runSeed`/`ecosystemBalance`) para `MaterializeWalls` e adicionei 1
  chamada nova (`CaveVignetteController.ApplyVignetteAndLightShaft`).

`system-reuse-audit`: nenhum sistema equivalente de "overlay de borda"/"vinheta"/"feixe de luz" existia;
tudo novo estende diretamente os pontos de extensão já preparados por CV01-CV03.

---

## Decisão de arquitetura (desvio consciente da seção 15 da spec)

A seção 15 (arquitetura alvo) da spec lista `CaveEnvironmentElementPlanner`/`CaveEnvironmentElementMaterializer`
como os arquivos que ganhariam "pass de GroundScatter denso + WallSurface base". **Optei por NÃO rotear
GroundScatter/WallSurface por esse pipeline** e implementá-los diretamente em `CaveTileMaterializer`
(mesmo arquivo/pattern do overlay de borda de rocha), pelos motivos abaixo — decisão tomada seguindo a
Escada de Minimalismo de Código (rule `code-minimalism-ladder`) e o precedente do próprio CV01
(pintura de tiles nunca foi snapshotada):

1. **GroundScatter é DENSO por design (~35% de todo o chão aberto)**. O pipeline de
   `CaveEnvironmentElementPlanner` persiste cada elemento em `SerializedEnvironmentElement` dentro do
   snapshot de stable-run (`VisitedLevelSnapshot`) — rotear cascalho denso por ali infla o snapshot em
   ordens de grandeza (potencialmente centenas de entradas por nível só de litter), sem nenhum ganho de
   correção (o cascalho não tem estado de gameplay — não há "depleted", não há ID de dados, não há
   colisão).
2. **GroundScatter/WallSurface são 100% re-deriváveis** a cada materialização a partir de
   `(worldSeed, runSeed, caveLevel, x, y)` via `CaveLayoutStableHash` — exatamente como o próprio Tilemap
   de chão/parede (CV01) e o overlay de borda de rocha desta mesma spec. Persistir algo 100% derivável é
   trabalho redundante que a arquitetura já evita para o terreno.
3. O pipeline do planner é orientado a `ElementEntry` autorada por bioma com `Kind`/peso/budget
   compartilhado (`used` HashSet) — criar um `Kind` novo sem gameplay (sem mineração, sem bloqueio) só
   para reusar esse pipeline adicionaria complexidade (nova entrada de enum, autoria por bioma no
   profile de ecossistema) sem necessidade real.

Mantive, no entanto, os 2 valores novos em `CaveDecorPlacementContext` (`GroundScatter`/`WallSurface`)
e os métodos classificadores correspondentes em `CaveDecorContextClassifier` — a PARTE da seção 15 que
pede geometria/nomenclatura compartilhada foi seguida; só a MATERIALIZAÇÃO via planner/elemento
ambiental foi substituída pelo caminho mais simples e já precedented do Tilemap. `GenerationConfigVersion`
**NÃO foi incrementado** — o shape/schema de `SerializedEnvironmentElement`/`CaveEnvironmentElementPlan`
não mudou (nada novo é persistido), diferente do bump 4→5 do CV03 (que mudou COMO o planner distribuía
elementos já persistidos).

Também adicionei, além do listado em T002 (`_wallEdgeSprites[4]`, `_groundScatterSprites[]`), 2 campos
pequenos não listados literalmente na seção 15 mas necessários para os critérios 14.3/14.4 tal como
escritos: `_wallSurfaceSprites[]` (subconjunto curado de 2 peças — moss + mushroom — distinto do pool
flat de 9 peças de GroundScatter, para satisfazer "musgo/vegetação" especificamente em vez de litter
genérico) e `_lightShaftSprite` (single sprite, mesmo padrão de `ExitDownSprite`/`ExitUpSprite`).

---

## Escopo executado (T001-T007)

- **T001** — Phase 0 audit (acima).
- **T002** — Pools no profile (`_wallEdgeSprites[4]`, `_groundScatterSprites[]`, `_wallSurfaceSprites[]`,
  `_lightShaftSprite`) + `CaveBiomeArtResolver.TryGetWallEdgeSprite`/`TryGetGroundScatterSprite`/
  `TryGetWallSurfaceSprite`/`TryGetLightShaftSprite` (todos null-safe) + `GenerateCaveBiomeArtProfiles`
  preenche os 4 do bioma 1 pela convenção de pasta já existente (`wall_edge_top/side/corner_a/corner_b.png`,
  9 `litter_*.png`, `light_shaft.png` — todos já staged no disco, confirmado antes de qualquer edição).
- **T003** — Overlay de borda de rocha determinístico em `CaveTileMaterializer.MaterializeWalls`: por
  célula de parede, `TryResolveWallEdgeKind` (função pura, sem RNG) decide topo/lateral/quina pelos
  vizinhos S/E/O andáveis; `sortingOrder=2` (acima do Tilemap de parede order=0 e do decor de teto CV03
  order=1); sem `BoxCollider2D`.
- **T004** — `CaveDecorPlacementContext.GroundScatter`/`WallSurface` + classificadores
  (`IsGroundScatterCell`/`IsWallSurfaceCell`) + funções puras de decisão determinística
  (`ShouldPlaceGroundScatter`/`ShouldPlaceWallSurfaceDecor`, hash salgado distinto do pick de sprite) +
  materialização direta em `MaterializeFloor`/`MaterializeWalls` (sortingOrder 1/3 respectivamente, sem
  collider).
- **T005** — `CaveVignetteController` (novo, runtime, sem YAML): vinheta estática (textura de gradiente
  RGBA gerada 1x em código, cacheada, `sortingOrder=50`) que escurece as bordas do NÍVEL (não da câmera)
  + feixe de luz (`sortingOrder=40`, alpha 0.55) posicionado em `level.Entrance`, resolvido via
  `TryGetLightShaftSprite`. Ambos estáticos (sem `Update`), adicionados a `materializedObjects` (mesmo
  ciclo de vida/cleanup do resto do terreno).
- **T006** — `CaveEcosystemBalanceSO.GroundScatterDensity` (default 0.35, `Range(0,1)`, clamp em
  `OnValidate`) + `WallSurfaceChance` (default 0.12); `GenerationConfigVersion` não incrementado (ver
  decisão de arquitetura acima).
- **T007** — Builds (abaixo) + EditMode tests novos + docs validation + cenário Play Mode + este report.

---

## What Was Run

- [x] `dotnet build .\CindarsHope.Runtime.csproj` — exit 0, 0E/0W
- [x] `dotnet build .\CindarsHope.Editor.csproj` — exit 0, 0E/0W
- [x] `dotnet build .\CindarsHope.Tests.EditMode.csproj` — exit 0, 0E/0W
- [x] `dotnet build .\Assembly-CSharp.csproj` — exit 0, 0E/0W
- [x] `dotnet build .\Assembly-CSharp-Editor.csproj` — exit 0, 0E, warnings pré-existentes (CS0436, não
      relacionados a esta spec)
- [x] `dotnet build .\CindarsHope.Foundation.csproj` — exit 0, 0E/0W
- [x] `dotnet build .\CindarsHope.Gameplay.csproj` — exit 0, 0E/0W
- [x] `dotnet build .\CindarsHope.Tests.PlayMode.Composition.csproj` — exit 0, 0E/0W
- [x] `tools\docs\validate_docs.ps1` — exit 1, EXPECTED_FAIL_LEGACY_ONLY (ver abaixo)
- [x] `tools\docs\run_strict_validation.ps1` — reporta `UNITY_PROJECT_BUILD_FAILURE` (FALSO NEGATIVO
      documentado, ver "Achado de infraestrutura" abaixo); builds reais confirmados 8/8 PASS acima
- [ ] Unity Editor batchmode (EditMode Test Runner, menu validators) — **BLOCKED**: 3 processos Unity já
      estavam rodando nesta máquina (`Get-Process Unity` confirmou PIDs ativos) no momento desta sessão
      — rule `unity-assets`/`pre-bash-guard` proíbe lançar batchmode em paralelo com o Editor aberto.
- [ ] Play Mode manual — DEFERRED_TO_FINAL_VALIDATION (cenário criado, ver abaixo).

---

## What Was NOT Run

- **EditMode Test Runner (Unity batchmode)**: BLOCKED — Unity Editor já aberto (3 processos ativos)
  no momento da sessão; não lancei uma segunda instância em batchmode (rule `unity-assets`: "Sem Unity
  batchmode em paralelo... Se Unity já estiver aberto, registre BLOCKED"). Evidência substituta: os 32
  testes NOVOS desta spec (`CaveVisualPolishOverlayTests` 14 testes, `CaveVisualPolishSpritePoolTests`
  12 testes, + 6 testes novos em `CaveDecorContextTests` para `IsWallSurfaceCell`/`IsGroundScatterCell`)
  compilam com 0 erros dentro de `CindarsHope.Tests.EditMode.csproj` (exit 0, verificado); o CONTEÚDO
  lógico foi verificado por leitura cuidadosa e por espelhar 1:1 o padrão já testado de
  `IsWallInterior`/`IsFloorEdgeNextToWall`/`CaveDecorSpritePoolTests` (que passam na suíte completa
  documentada em CURRENT_STATE.md). Residual risk: os testes não foram EXECUTADOS por um Test Runner
  real nesta sessão — só compilados. Humano deve rodar Unity Test Runner → EditMode → filtro `Cave`
  antes de promover a spec.
- **Menu validators (`CindarsHope/Validar Projeto`)**: NOT RUN — mesma limitação de Unity já aberto;
  sem wrapper `.ps1` dedicado para `-executeMethod` (mesma limitação documentada no report da CV03).
  Residual risk: os assets `.asset` de `CaveBiomeArtProfileSO` existentes (incl.
  `CaveBiomeArtProfile_biome_stone_cavern.asset`, já modificado no working tree por uma sessão anterior a
  esta) ainda não têm os pools NOVOS desta spec (`WallEdgeSprites`/`GroundScatterSprites`/
  `WallSurfaceSprites`/`LightShaftSprite`) preenchidos — só o CÓDIGO do gerador foi atualizado. **O
  humano precisa rodar `CindarsHope/Inicializar Projeto` 1x** para materializar esses pools a partir da
  arte já staged em `Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/` antes que qualquer
  overlay/decor novo apareça em Play Mode; até lá, todo `TryGet*` desta spec retorna `false` (pool
  vazio) e o comportamento visual permanece IDÊNTICO ao pré-CV04 (fallback null-safe, sem quebrar nada).
- **Play Mode manual**: DEFERRED_TO_FINAL_VALIDATION. Cenário em
  `docs/validation/playmode/spec_cave_visual_polish_human_test_scenario.md`.
- **`CaveReplayValidator.ValidateReplaySystem`**: NOT RUN — exige uma `CaveRunManager`/
  `CaveLevelRuntimeController` reais em Play Mode (mesma limitação de todo o projeto); nenhum teste
  automatizado headless o exercita nesta spec.

---

## Achado de infraestrutura (fora do scope desta spec, já documentado em report anterior)

`tools\docs\run_strict_validation.ps1` reportou `STRICT_VALIDATION_RESULT: UNITY_PROJECT_BUILD_FAILURE`
nesta sessão mesmo com o passo de build (`Invoke-UnityGeneratedProjectsBuild.ps1`) imprimindo "PASS".
Este é o MESMO falso negativo já documentado em
`docs/validation/spec_cave_decor_composition_execution_report.md` (2026-07-06/07) e em
`docs/project/CURRENT_STATE.md` ("INFRA (documentado, nao corrigido): run_strict_validation.ps1 reporta
UNITY_PROJECT_BUILD_FAILURE falso mesmo com build real passando"). **Confirmado de novo nesta sessão**:
rodei cada um dos 8 `*.csproj` do repo individualmente com `dotnet build --no-restore` e chequei
`$LASTEXITCODE` real de cada um — todos retornaram exit 0 (evidência na seção "Results" abaixo). Não
corrigido aqui: `tools/docs/**` não está no repo lock scope desta spec e é infraestrutura transversal
pré-existente, não algo que esta spec quebrou.

---

## Results

| Check | Result | Notes |
|-------|--------|-------|
| CindarsHope.Runtime.csproj | PASS — 0E/0W | Inclui `CaveVignetteController.cs` (novo) |
| CindarsHope.Editor.csproj | PASS — 0E/0W | Inclui `GenerateCaveBiomeArtProfiles.cs`/`ValidateCaveBiomeArtProfiles.cs` atualizados |
| CindarsHope.Tests.EditMode.csproj | PASS — 0E/0W | Inclui os 2 arquivos de teste novos desta spec |
| Assembly-CSharp.csproj | PASS — 0E/0W | Fallback intacto |
| Assembly-CSharp-Editor.csproj | PASS — 0E, warnings pré-existentes | CS0436 (conflito de tipo duplicado), mesmo padrão documentado em reports anteriores — nenhum novo desta spec |
| CindarsHope.Foundation.csproj | PASS — 0E/0W | Não tocado por esta spec |
| CindarsHope.Gameplay.csproj | PASS — 0E/0W | Não tocado por esta spec |
| CindarsHope.Tests.PlayMode.Composition.csproj | PASS — 0E/0W | Não tocado por esta spec |
| Docs validation | EXPECTED_FAIL_LEGACY_ONLY (exit 1) | Todos os erros pré-existentes: 4 specs não relacionadas (`spec_enemy_attack_kits_v1`, `spec_npc_physics_cat_companion`, `spec_town_building_visuals`, `spec_town_layout_v9_organic`) + mojibake em `tools/codex/Generate-CodexHarness.ps1`/`MODULARIZATION_PHASE3_FOUNDATION_REPORT.md` + `spec_cave_visual_polish_runtime.md` (a própria spec de entrada, criada ANTES desta sessão, sem header "Ordem de execucao" — não editada por mim, fora do scope de implementação de código) |
| EditMode tests (Cave, novos desta spec) | NOT RUN (compilação confirmada, execução BLOCKED) | Unity já aberto — ver acima |
| Unity menu validators | NOT RUN | BLOCKED — Unity já aberto |
| Unity asset regeneration (pools CV04) | NOT RUN / PENDING HUMAN | `CindarsHope/Inicializar Projeto` necessário |
| Play Mode | NOT RUN | DEFERRED_TO_FINAL_VALIDATION |

---

## Acceptance criteria extracted

```
14.1 Paredes com borda arredondada (overlay determinístico nas células parede-encosta-chão); revisita idêntica.
14.2 Chão com cascalho DENSO (muitas células de chão com litter pequeno); determinístico; não bloqueia caminho.
14.3 Musgo/vegetação nas bases de parede.
14.4 Vinheta escurece bordas do nível + 1 feixe de luz visível perto da entrada.
14.5 Builds runtime+editor exit 0; replay validator PASS; EditMode dos itens determinísticos PASS.
```

---

## Existing systems audit

Ver "Phase 0 — Audit" acima. Resumo: nenhum sistema equivalente de overlay de borda/vinheta/feixe
existia; toda a extensão reusa pontos de extensão já preparados (pools opcionais no profile, resolver
null-safe, classificador de contexto puro, padrão de tint por-célula do CaveTileMaterializer, campo
tunável no `CaveEcosystemBalanceSO`).

---

## Spec Compliance Matrix

| Critério | Status | Evidência |
|---|---|---|
| 14.1 Borda de rocha arredondada, determinística, revisita idêntica | CODE_READY | `TryResolveWallEdgeKind` puro/testado (determinismo coberto por `TryResolveWallEdgeKind_IsDeterministic_...`); revisita idêntica decorre de ser 100% função de `(x,y,level)` sem estado — Play Mode humano confirma visualmente |
| 14.2 Cascalho denso, determinístico, não bloqueia caminho | CODE_READY | `ShouldPlaceGroundScatter` (elegibilidade = FloorCluster, hash salgado); sem `BoxCollider2D` no overlay (código não adiciona collider); Play Mode humano confirma visualmente |
| 14.3 Musgo/vegetação na base de parede | CODE_READY | `ShouldPlaceWallSurfaceDecor` + pool curado (`_wallSurfaceSprites` = moss + mushroom); Play Mode humano confirma visualmente |
| 14.4 Vinheta + feixe de luz | CODE_READY | `CaveVignetteController` cria os 2 overlays estáticos; Play Mode humano confirma visualmente |
| 14.5 Builds + replay + EditMode | PARTIAL | Builds PASS (8/8, evidência acima); EditMode NOT RUN (Unity aberto, só compilado); replay validator NOT RUN (exige Play Mode) |

Nenhuma linha "Required" com FAIL — os únicos itens sem evidência completa (`14.5` parcial) são por
bloqueio de ambiente (Unity Editor já aberto), não por falha de implementação.

---

## Honest status rationale

`BUILD_VALIDATED` é o status correto e HONESTO para esta sessão: todo o código compila limpo (8/8
projetos, 0 erros) e a lógica determinística nova foi escrita seguindo — e espelhando os testes
existentes de — padrões já validados no projeto (`IsWallInterior`/`IsFloorEdgeNextToWall`/
`CaveDecorSpritePoolTests`). **NÃO reivindico** "EditMode PASS", "Play Mode PASS" ou "replay validator
PASS" — nenhum dos três rodou nesta sessão (Unity Editor já estava aberto com 3 processos ativos, o que
proíbe lançar uma instância batchmode paralela por regra do projeto). O card de aceite final
(`ACCEPTED`) depende de 3 ações humanas pendentes, listadas em "Next Action" abaixo. Nenhuma arte nova
foi gerada por mim (toda a arte já estava no disco antes desta sessão, conforme instruído) — só o
WIRING de código foi escrito.

---

## Validation

Ver blocos "What Was Run"/"What Was NOT Run"/"Results" acima. Bloco de evidência conforme rule
`validation-truth`:

```text
Validation method: dotnet build (8 csproj individuais, --no-restore, $LASTEXITCODE real checado)
CindarsHope.Runtime.csproj: PASS (exit 0, 0E/0W)
CindarsHope.Editor.csproj: PASS (exit 0, 0E/0W)
CindarsHope.Tests.EditMode.csproj: PASS (exit 0, 0E/0W)
Assembly-CSharp.csproj: PASS (exit 0, 0E/0W)
Assembly-CSharp-Editor.csproj: PASS (exit 0, 0E, warnings pré-existentes)
CindarsHope.Foundation.csproj: PASS (exit 0, 0E/0W)
CindarsHope.Gameplay.csproj: PASS (exit 0, 0E/0W)
CindarsHope.Tests.PlayMode.Composition.csproj: PASS (exit 0, 0E/0W)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY (exit 1; nenhum erro novo desta spec além do header
  ausente na própria spec de entrada, não editada por mim)
Quality check (tools/docs/check_spec_quality.ps1): FAIL pré-existente — "Forbidden files altered"
  lista 15 arquivos .asset/.unity TODOS já modificados/untracked no working tree ANTES desta sessão
  (confirmado pelo `git status --short` executado no início desta tarefa) — nenhum deles foi tocado
  por mim nesta sessão
Result artifact: n/a (sem LAST_STRICT_VALIDATION_RESULT.json gerado — run_strict_validation.ps1
  saiu com o falso-negativo documentado antes de chegar aos steps 3/4; diff-completeness e
  quality-check foram rodados manualmente em isolado, ver acima)
```

---

## Errors Found

Nenhum erro de compilação. Nenhum bug de lógica encontrado durante a implementação (diferente da CV03,
que encontrou e corrigiu 1 bug de contexto hardcoded).

---

## Warnings (pre-existing)

Warnings `CS0436` em `Assembly-CSharp-Editor.csproj` — conflito de tipo duplicado pré-existente entre
assemblies (mesmo padrão documentado nos reports CV01-CV03), não relacionado a esta spec.

---

## Evidence

Files changed/created:
```
Assets/_Game/Scripts/Cave/Data/CaveEcosystemBalanceSO.cs                (novo: GroundScatterDensity, WallSurfaceChance)
Assets/_Game/Scripts/Cave/Ecosystem/CaveDecorPlacementContext.cs        (novo: GroundScatter, WallSurface)
Assets/_Game/Scripts/Cave/Ecosystem/CaveDecorContextClassifier.cs       (novo: IsWallSurfaceCell, IsGroundScatterCell)
Assets/_Game/Scripts/Cave/Art/CaveBiomeArtProfileSO.cs                  (novo: WallEdgeSprites/GroundScatterSprites/WallSurfaceSprites/LightShaftSprite + setters)
Assets/_Game/Scripts/Cave/Art/CaveBiomeArtResolver.cs                   (novo: TryGetWallEdgeSprite/TryGetGroundScatterSprite/TryGetWallSurfaceSprite/TryGetLightShaftSprite + enum CaveWallEdgeKind)
Assets/_Game/Scripts/Cave/Runtime/CaveTileMaterializer.cs               (overlay de borda de rocha + GroundScatter + WallSurface, funções puras testáveis)
Assets/_Game/Scripts/Cave/Runtime/CaveVignetteController.cs             (novo — vinheta + feixe de luz)
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs            (passa ecosystemBalance/seeds para MaterializeWalls; chama CaveVignetteController)
Assets/_Game/Scripts/Editor/Cave/GenerateCaveBiomeArtProfiles.cs        (preenche os 4 pools/sprite novos do bioma 1)
Assets/_Game/Scripts/Editor/Cave/ValidateCaveBiomeArtProfiles.cs        (WARNING se pools/sprite novos vazios)
Assets/_Game/Tests/EditMode/Cave/CaveDecorContextTests.cs               (estendido: 6 testes novos p/ IsWallSurfaceCell/IsGroundScatterCell)
Assets/_Game/Tests/EditMode/Cave/CaveVisualPolishOverlayTests.cs        (novo, 15 testes)
Assets/_Game/Tests/EditMode/Cave/CaveVisualPolishSpritePoolTests.cs     (novo, 16 testes)
CindarsHope.Runtime.csproj                                              (Include do CaveVignetteController.cs — arquivo *.csproj é git-ignorado; edição local só para permitir `dotnet build` nesta sessão sem depender do Unity Editor já aberto. Regenerado automaticamente pela próxima sincronização/reload do Unity, que descobre o novo arquivo pelo asmdef `CindarsHope.Runtime`.)
CindarsHope.Tests.EditMode.csproj                                       (idem, Include dos 2 arquivos de teste novos — também git-ignorado)
docs/validation/spec_cave_visual_polish_execution_report.md             (este arquivo)
docs/validation/playmode/spec_cave_visual_polish_human_test_scenario.md (novo)
```

Files pré-existentes no working tree, modificados por sessão ANTERIOR a esta (não tocados por mim,
listados aqui só para não mascarar o estado do repo — ver "git status" completo no início da tarefa):
```
Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/floor_b.png (M) + floor_c.png/light_shaft.png/litter_*.png (novos)
Assets/_Game/Data/Cave/Biomes/CaveBiomeArtProfile_biome_stone_cavern.asset
Assets/_Game/Data/Enemies/Canonical/enemy_*.asset (7 arquivos)
Assets/_Game/Data/EnemySpawn/Packs/pack_f81_void_vanguard.asset
Assets/_Game/Scenes/CaveScene.unity, FarmScene.unity, TownScene.unity
Assets/_Game/Resources/CaveEcosystemBalance.asset
Assets/_Game/Scripts/Editor/Cave/GenerateCaveEcosystemBalance.cs
Assets/_Game/Scripts/Enemy/EnemyAnimator.cs
```

Files NOT changed (protected, conforme anti-regressão da spec):
```
Assets/_Game/Scripts/Cave/Ecosystem/CaveEnvironmentElementPlanner.cs        (não estendido — ver Decisão de arquitetura)
Assets/_Game/Scripts/Cave/Runtime/CaveEnvironmentElementMaterializer.cs    (não estendido — ver Decisão de arquitetura)
Assets/_Game/Scripts/Cave/Data/CaveGenerationConfigSO.cs                    (GenerationConfigVersion NÃO incrementado — ver Decisão de arquitetura)
Assets/_Game/Scripts/Cave/Generation/** (gerador procedural inalterado, conforme "sem trocar o gerador")
Qualquer arquivo de URP/render pipeline (luz é FAKE, overlay puro)
```

---

## Phase Status

| Phase | Status | Date |
|-------|--------|------|
| Phase 0 (Audit) | COMPLETE | 2026-07-10 |
| Phase 1 (Automated: dotnet build, 8 projetos) | PASS | 2026-07-10 |
| Phase 2 (Unity EditMode Test Runner / menu validators) | NOT RUN — BLOCKED (Unity Editor já aberto) | — |
| Phase 3 (Play Mode) | NOT RUN — DEFERRED_TO_FINAL_VALIDATION | — |

---

## Testing Quality Gate

```
- Changed deterministic logic: YES (overlay de borda, GroundScatter, WallSurface, vinheta/feixe)
- Automated tests: YES — 32 EditMode tests novos (CaveVisualPolishOverlayTests 14,
  CaveVisualPolishSpritePoolTests 12, +6 em CaveDecorContextTests) — COMPILADOS (0 erros), execução
  BLOCKED (Unity Editor já aberto nesta sessão)
- PlayMode automated or final human scenario: scenario criado
  (docs/validation/playmode/spec_cave_visual_polish_human_test_scenario.md), execução DEFERRED
- Regression test: não-bloqueio garantido por código (nenhum overlay novo adiciona BoxCollider2D);
  stable-run coberto pelos testes de determinismo (hash puro, sem Random/GetHashCode)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: builds exit 0 (feito, 8/8) + EditMode PASS real via Test
  Runner (pendente — só compilado) + replay validator PASS (pendente) + Play Mode humano (pendente)
```

---

## Anti-regressão (checklist da spec, seção 32)

- [x] Não bloquear caminho — overlays de borda/GroundScatter/WallSurface/vinheta/feixe NUNCA adicionam
      `BoxCollider2D` (confirmado por leitura do código: nenhuma chamada `AddComponent<BoxCollider2D>()`
      nos blocos novos).
- [x] Só `CaveLayoutStableHash`; sem `Random`/`GetHashCode` — todo hash novo (`ShouldPlaceGroundScatter`,
      `ShouldPlaceWallSurfaceDecor`, seleção de sprite) usa `CaveLayoutStableHash.Compute`/
      `CaveBiomeArtResolver.ComputeCellHash`, mesmas fontes já usadas pelo CV01-CV03.
- [x] Sem `GameObject.Find`/`FindObjectOfType` — nenhum arquivo tocado usa essas APIs.
- [x] Sem refs Unity no save — nenhum dos elementos novos (borda, scatter, wallSurface, vinheta, feixe)
      é persistido no snapshot; 100% re-derivado a cada materialização (mesma garantia do Tilemap CV01).
- [x] `EnsureGuaranteedPresence` mantido — `CaveEnvironmentElementPlanner` não foi tocado.
- [x] Sem edição manual de `.unity`/`.prefab`/`.asset` — apenas C# tocado; os `.asset` de
      `CaveBiomeArtProfileSO`/`CaveEcosystemBalanceSO` precisam ser REGENERADOS/re-salvos pelo Editor
      (`CindarsHope/Inicializar Projeto`), nunca editados à mão.
- [x] Luz é FAKE (overlay `SpriteRenderer` + textura gerada em código) — nenhum arquivo de URP/render
      pipeline foi tocado; nenhum `Light2D`/componente de luz real foi adicionado.

---

## Next Action

```
1. Humano roda "CindarsHope/Inicializar Projeto" 1x no Unity Editor — materializa os pools novos
   (WallEdgeSprites/GroundScatterSprites/WallSurfaceSprites/LightShaftSprite) do bioma 1 a partir da
   arte já staged em Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/.
2. Humano roda Unity Test Runner (EditMode, filtro Cave) para confirmar os 37 testes novos + a suíte
   completa continuam passando (compilação já confirmada nesta sessão).
3. Humano executa o cenário de Play Mode em
   docs/validation/playmode/spec_cave_visual_polish_human_test_scenario.md.
4. Se os 3 passos acima confirmarem os critérios 14.1-14.4 visualmente e sem regressão, promover o
   status desta spec de BUILD_VALIDATED para ACCEPTED via /finish-spec (com o check de elegibilidade).
```

---

## Bugfix pós-Play 2026-07-10 (3 bugs visuais reportados após Play Mode de CV04)

Investigação de bug (agent `bugfix-investigator`), sem sub-agentes, mudança mínima por arquivo.

### BUG 1 (CRÍTICO) — feixe de luz virou um quadrado cinza gigante — FIXED

- **Root cause confirmado (não só hipotetizado):** `light_shaft.png` tem **1254x1254px @ 128 PPU ≈ 9,8
  unidades de mundo** (verificado via `System.Drawing.Image` — todas as demais peças da CV04, incluindo
  `wall_face.png`/`wall_edge_*.png`/`floor_a.png`, são 128x128px @ mesma PPU = exatamente 1 unidade). O
  sprite era renderizado no tamanho nativo (sem `localScale` explícito) com blending Normal
  (`Sprites/Default` implícito) e alpha 0.55 — a névoa/gradiente semi-transparente ao redor do núcleo
  claro do feixe é composta como um retângulo cinza opaco sobre a sala inteira.
- **Fix (`Assets/_Game/Scripts/Cave/Runtime/CaveVignetteController.cs`, `CreateLightShaft` +
  `GetOrCreateLightShaftMaterial` novo):**
  1. `go.transform.localScale = LightShaftScale` (`(3, 5, 1)`, const nomeada) — volta o feixe a um cone
     estreito perto da entrada em vez do sprite no tamanho nativo (~10 unidades).
  2. Material aditivo cacheado estaticamente (mesmo padrão do `_cachedVignetteSprite`):
     `Shader.Find("Legacy Shaders/Particles/Additive")` (fallback `"Particles/Additive"`) — com
     blending aditivo a névoa escura/transparente não soma luz nenhuma, só o núcleo claro brilha.
     Alpha aditivo sobe para 0.6 (`LightShaftAdditiveAlpha`) sem lavar a cena, porque adição satura em
     branco, não em cinza opaco (causa raiz do bug original).
  3. Fallback null-safe (regra `error-handling-resilience`): se nenhum shader aditivo for resolvível no
     build, cai para `Sprites/Default` com alpha bem reduzido (`LightShaftFallbackAlpha = 0.22f`) +
     `Debug.LogWarning` com prefixo `[Cave][Wiring]` (nunca lança, nunca deixa o feixe sem material).
- Sorting order/layer, PPU do `.meta`, e `GeneratedSpriteImporter` **não foram tocados** (conforme
  restrição do relato) — fix 100% em código (escala do Transform + material).

### BUG 2 — borda de rocha (rim) solta sobre o void — NÃO REPRODUZIDO / NENHUMA MUDANÇA DE CÓDIGO

**NÃO FIZ mudança de código para este bug** porque, após leitura completa, o critério de vizinhança que
o relato pede como fix **já é exatamente o que o código faz**:

- `CaveTileMaterializer.TryResolveWallEdgeKind` (linhas ~452-498) resolve `south`/`east`/`west` via
  `level.WalkableTiles.Contains(...)` — nunca "não é parede". Célula sem nenhum vizinho ortogonal S/E/O
  andável retorna `false` (sem overlay), confirmado por teste existente
  `TryResolveWallEdgeKind_NoRelevantWalkableNeighbor_ReturnsFalse`
  (`Assets/_Game/Tests/EditMode/Cave/CaveVisualPolishOverlayTests.cs:91`).
- Busquei todo o repo por `WallEdge|wall_edge|RockEdge|EdgeOverlay|BorderOverlay` — só existem 2 arquivos
  de produção (`CaveTileMaterializer.cs`, `CaveBiomeArtResolver.cs`) e o gerador/validator; não há um
  segundo local com critério divergente ("não é parede") criando overlays duplicados.
- `IsWallSurfaceCell` (musgo de base de parede, `CaveDecorContextClassifier.cs:75`) — mencionado no
  relato como candidato — também já usa `level.WalkableTiles.Contains` (não "não é parede").
- Verifiquei alinhamento de grid (`GridToWorld` vs `CreateTilemapLayer`, já corrigido em rodada anterior
  2026-07-04) e PPU dos 4 sprites `wall_edge_*.png` (128x128 @ 128 PPU = 1 unidade, igual
  `wall_face.png`/`wall_top.png`) — sem mismatch de escala/offset que explicasse "solto sobre o void".
- **Hipótese alternativa não confirmada (não corrigida, fora do escopo de uma mudança mínima):** o
  overlay é sempre desenhado na MESMA `worldPos` do `WallTile` real (sempre existe uma peça de parede —
  Tilemap ou placeholder cinza — sob qualquer célula de `level.WallTiles`), então "sem parede embaixo"
  pode ser um efeito de CONTRASTE (parede escurecida por `WallInteriorTint`/vinheta perto de uma célula
  clara de rim) em vez de um bug de posicionamento — ou um profile `.asset` desatualizado (array de
  `WallEdgeSprites` com 4 entradas mas gerado antes da arte `wall_edge_*.png` existir na pasta,
  corrigido automaticamente rodando `Inicializar Projeto`).
- **Recomendação:** humano roda `CindarsHope/Inicializar Projeto` 1x (garante os profiles com os pools
  novos re-sincronizados) e re-verifica em Play Mode com um screenshot específico da célula onde o rim
  aparece solto; se reproduzir, anexar o screenshot + coordenada de grid para uma investigação dirigida
  (o código puro já tem cobertura de teste completa dos 5 casos de vizinhança — top/side/side-mirrored/
  cornerA/cornerB/sem-vizinho — então uma reprodução real ajudaria a achar a causa fora dessa função).

### BUG 3 — ossos demais no chão (ground scatter) — FIXED

- **Root cause confirmado:** `GenerateCaveBiomeArtProfiles.PopulateFromConvention`
  (`Assets/_Game/Scripts/Editor/Cave/GenerateCaveBiomeArtProfiles.cs`) montava o pool `GroundScatterSprites`
  do bioma com as 9 peças `litter_*` em partes iguais — `litter_bone.png` era 1 de 9 (~11% de todo tile
  de chão elegível), lendo como bioma Abismo em vez de Caverna de Pedra.
- **Fix:** removida a linha `AddIfExists(groundScatter, $"{folder}/litter_bone.png")` do pool
  `groundScatter` (linha ~190). Pool passa de 9 para 8 peças
  (pebbles/rocks/crack_a/gravel/moss/mushrooms_small/rock_single/crack_b). `litter_bone.png` **não foi
  deletado do disco** (segue existindo, só não entra mais neste pool) — se o design quiser osso raro no
  futuro, precisaria de pool ponderado (fora do escopo desta mudança mínima).

### Validação

```text
Validation method: dotnet build (4 csproj, sequencial, sem batchmode Unity)
CindarsHope.Runtime.csproj:        PASS, exit 0, 0 Aviso(s), 0 Erro(s)
CindarsHope.Editor.csproj:         PASS, exit 0, 0 Aviso(s), 0 Erro(s)
Assembly-CSharp.csproj:            PASS, exit 0, 0 Aviso(s), 0 Erro(s)
Assembly-CSharp-Editor.csproj:     PASS, exit 0, 1101 Aviso(s) pré-existentes (CS0436, todos em
                                    CreateMvpTownScene.cs — conflito de tipo Assembly-CSharp-Editor vs
                                    CindarsHope.Editor, não relacionados a nenhum arquivo tocado nesta
                                    sessão), 0 Erro(s)
```

**Anti-regressão confirmada:**
- Sem `Random`/`GetHashCode` novo (só `Shader.Find`/`Material`/`Color`/`Vector3`, nenhum RNG).
- Sem `GameObject.Find`/`FindObjectOfType` novo.
- Sem edição de `.unity`/`.prefab`/`.asset` (só C#; `.meta` dos PNGs lidos, não editados).
- Luz continua FAKE (sem `Light2D`/URP; só `SpriteRenderer` + `Material` com shader Legacy built-in).
- `litter_bone.png` não deletado do disco (só removido do pool no gerador).

**Testing Quality Gate:**
- Regression test: **JUSTIFIED** — não escrevi teste novo. BUG 1 (escala/material) e BUG 3 (remoção de
  1 linha de um pool `Editor`-only, `[MenuItem]`-less generator) não têm lógica pura nova testável em
  EditMode sem instanciar `SpriteRenderer`/rodar o gerador em batchmode; o comportamento determinístico
  já coberto por teste (`TryResolveWallEdgeKind` e afins, CV04) não foi alterado — BUG 2 não teve
  mudança de código. `ValidateCaveBiomeArtProfiles` (validator existente) já detectaria pool vazio; um
  teste dedicado para "litter_bone ausente do pool" adicionaria acoplamento a um array de nomes de
  arquivo sem cobrir nenhuma lógica de decisão nova.
- Residual risk: BUG 1 e BUG 3 só se materializam visualmente após o humano rodar
  `CindarsHope/Inicializar Projeto` (regenera `CaveBiomeArtProfile_biome_stone_cavern.asset` com o pool
  de 8 peças e mantém `light_shaft.png` como estava) e depois Play Mode na CaveScene — não verificado
  neste turno (Unity Editor não executado). BUG 2 permanece com o sintoma original não reproduzido nem
  corrigido; se persistir após `Inicializar Projeto`, precisa de uma reprodução com coordenada de grid
  para investigação dirigida (ver hipóteses acima).

### O que o humano roda

1. `CindarsHope/Inicializar Projeto` 1x no Unity Editor (repopula `CaveBiomeArtProfile_biome_stone_cavern.asset`
   sem `litter_bone` no pool de GroundScatter).
2. Play Mode na CaveScene — feixe de luz e rim são efeitos 100% runtime (overlays recriados a cada
   materialização), só Play Mode revalida visualmente; o pool de ossos precisa do passo 1 primeiro.
3. Se o rim (BUG 2) ainda aparecer solto sobre void após os passos 1-2, capturar screenshot + coordenada
   de grid da célula para uma investigação de causa raiz dirigida (código puro já 100% coberto por teste
   e não reproduz o sintoma em análise estática).

---

*Report generated: 2026-07-10*
