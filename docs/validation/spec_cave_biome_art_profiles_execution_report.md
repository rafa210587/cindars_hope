# Execution Report — spec_cave_biome_art_profiles_runtime (CV01)

> **Spec:** `.specs/a_implementar/spec_cave_biome_art_profiles_runtime.md`
> **Status final desta execução:** `BUILD_VALIDATED` (Play Mode deferido — ver Testing Quality Gate)
> **Data:** 2026-07-03

---

## 1. Achados da Fase 0 (auditoria obrigatória)

1. **Renderização de chão/parede HOJE (antes desta spec):** `CaveTileMaterializer.MaterializeFloor`
   e `MaterializeWalls` já materializam a cave **objeto a objeto** via `SpriteRenderer`
   (1 `GameObject` por tile walkable/wall), com fallback procedural (`GetBuiltinSprite()` + cor sólida)
   quando os prefabs seriados (`_floorTilePrefab`/`_wallTilePrefab` do `CaveRuntimeMaterializer`) são
   nulos. **Não havia Tilemap** — confirma o pressuposto da spec ("SE a Fase 0 confirmar ausência de
   grid visual, criar Tilemap").
2. **`CaveLevelEnteredEvent`:** já existe em `Core/Events/CaveLevelEnteredEvent.cs`
   (`{ int CaveLevel; string BiomeId; string CaveRunSeed; }`), publicado em 2 pontos de
   `CaveLevelRuntimeController.cs` (geração nova e `RestoreFromSnapshot`).
   **Achado crítico pré-existente (não introduzido nem corrigido por esta spec):** o `BiomeId`
   publicado vem de um campo serializado `_defaultBiomeId` com literal fixo `"biome_cave_earth"`,
   que **não casa com nenhum id do `CaveBiomeRegistrySO`** (`biome_stone_cavern`, `biome_forest`, …).
   Ou seja, o biomeId de GAMEPLAY nunca é resolvido dinamicamente por nível hoje. Existe também um
   `CaveBiomeResolver.cs` (código morto/órfão — nunca instanciado em lugar nenhum do projeto) que
   resolveria isso via `CaveBiomeRegistrySO.GetBiomeByLevel`, mas não está religado.
   **Decisão:** consertar esse bug está FORA de escopo desta spec (que só cobre apresentação visual e
   proíbe tocar layout/spawn/loot/decisões de gameplay). O novo `CaveBiomeChangedEvent` resolve seu
   próprio `BandId` via `CaveBandScaling.BandForLevel(caveLevel)` (puro, já fonte de verdade de banda
   em todo o resto do código — traps, inimigos) e seu `BiomeId` a partir do `CaveBiomeArtProfileSO`
   da banda (dado de arte, não de gameplay). Ver nota de flag abaixo.
3. **`CaveLayoutStableHash`:** já existe (`Cave/Generation/CaveLayoutStableHash.cs`), FNV-1a 32-bit
   puro e público. Reutilizado integralmente pelo `CaveBiomeArtResolver.ComputeCellHash` — nenhum
   hash novo foi criado.
4. **`CaveBiomeRegistrySO`:** `DataRegistrySO<CaveBiomeDataSO>` com fallback runtime em código
   (8 biomas hardcoded em `DefaultBiomes`, privado). Não há gerador dedicado no `CindarsHopeMenu.cs`
   para popular assets `CaveBiomeDataSO` — o registry já tolera rodar sem assets via fallback. A
   lista de 8 biomas (id + minLevel) foi espelhada (comentada como tal) dentro do novo gerador de
   profiles, já que a lista original é `private`.
5. **Traps/chest/hazard/exits:** todos usavam `SpriteRenderer.color` hardcoded isolado em métodos
   próprios (`TreasureChestInteractable.UpdateVisual`, `FalseChestTrap.ApplyVisual`,
   `CaveHazardTile.ResolveTelegraphColor`, `TrapBehaviour.ResolveTelegraphColor`) — fácil interceptar
   com "sprite custom vence, senão placeholder atual" sem reescrever a máquina de estados de nenhum.
6. **Precedente de geração de tile assets:** `Assets/_Game/Scripts/Editor/Art/WorldTilemapGround.cs`
   já tem o padrão exato (Grid+Tilemap via `GetOrCreateLayer`, `Tile` asset cacheado por nome via
   `AssetDatabase.CreateAsset`, variação determinística por hash em `PaintRectWeighted`). O novo
   gerador de profiles segue o mesmo padrão de `Tile` asset, adaptado para runtime (a cave é
   procedural, então o Tilemap é pintado em runtime pelo materializer, não no gerador de editor —
   o gerador só cria os `Tile` assets e os referencia no profile).
7. **`CaveBandScaling.BandForLevel`:** já mapeia nível→banda 1-7 (puro, sem dependência de asset).
   Usado como a chave de resolução banda→profile em TODOS os pontos novos desta spec.
8. **Arte de teste já staged:** `Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/` já
   continha `floor_a.png`, `floor_b.png`, `wall_face.png`, `wall_top.png` (sem `floor_detail.png`) —
   exatamente a convenção de pasta pedida pela spec/emenda 2026-07-03 (fatia de teste do bioma 1).

## 2. Escolhas de implementação (menor mudança)

- **Resolução banda→profile (item C da spec):** optei por um array serializado
  `CaveBiomeArtProfileSO[] _biomeArtProfiles` no próprio `CaveRuntimeMaterializer` (mesmo padrão dos
  demais arrays de database/prefab já existentes ali), resolvido em memória por um
  `CaveBiomeArtResolver` (C# puro) construído em `EnsureCollaborators()`. **Não criei um segundo
  registry** — o `CaveBiomeRegistrySO` continua sendo a fonte de verdade de biomas/níveis; o novo
  array é só a lista dos 8 profiles de ARTE, e a chave de lookup é `BandId` (int), não `biomeId`.
  Motivo: evita depender de `Resources.Load` em runtime (padrão do projeto para dados injetáveis é
  serialized field + fallback) e mantém tudo null-safe/testável sem tocar em `CaveBiomeRegistrySO`.
- **Tilemap (item F):** confirmado pela Fase 0 que não havia grid visual — criei
  `Grid+Tilemap` em runtime dentro de `CaveTileMaterializer.MaterializeFloor/MaterializeWalls`,
  condicional a `biomeArtResolver` ter `FloorTiles`/`WallFaceTile`/`WallTopTile` para a banda. Sem
  esses tiles, **nenhum Tilemap é criado** — comportamento 100% idêntico ao atual (GameObjects por
  tile). Os GameObjects de fallback continuam sendo criados mesmo quando há Tilemap (o Tilemap fica
  visualmente por baixo/ao lado), porque a colisão de parede (`BoxCollider2D`) e a identidade de nome
  (`FloorTile_x_y`/`WallTile_x_y`, usada por outros sistemas via busca por nome, ex.
  `CaveHazardMaterializer.RelocateGuardians`) dependem desses GameObjects — não removi esse
  mecanismo (risco 26 da spec: "Tilemap em runtime conflitar com colisão existente"; mitigação
  aplicada: SEM collider no Tilemap, colisão 100% inalterada). **Correção aplicada durante a própria
  execução (achado próprio, antes da auditoria formal):** tanto no chão quanto na parede, quando o
  Tilemap está ativo (pelo menos 1 tile do profile presente), o `SpriteRenderer` do GameObject de
  fallback é desabilitado (`enabled = false`) — sem isso, o placeholder sólido (mesma
  sortingLayer/Order do Tilemap) ficava por CIMA da arte real e a escondia completamente. O
  `GameObject` continua existindo (nome estável `FloorTile_x_y`/`WallTile_x_y`, usado por
  `CaveHazardMaterializer.RelocateGuardians` e pela colisão `BoxCollider2D` da parede) — só o
  `SpriteRenderer` fica invisível quando a arte real já está pintada no Tilemap por baixo.
  Parede: aproximação v1 da spec — célula de parede com chão imediatamente ao sul usa `wallFaceTile`
  (face frontal); demais células usam `wallTopTile`.
- **Trap sprite + tint de estado:** `TrapBehaviour` tem estados dinâmicos (Armed/Telegraphing/
  Triggered/Disarmed) que hoje trocam de COR. Em vez de substituir o sprite (que apagaria o
  telegraph), o sprite do bioma vira a base e o tint de estado continua sendo aplicado por cima
  (multiplicativo via `SpriteRenderer.color`) nos estados de aviso, preservando a legibilidade de
  hazard/trap exigida pela direção visual (§2 do doc de referência). **Ajuste pós-auditoria:** o
  estado padrão (Armed, não detectado) fica opaco (`Color.white`) quando há sprite do bioma — a
  auditoria de não-regressão (seção 15.1) apontou que aplicar o tint incondicionalmente divergia do
  padrão `Color.white` dos outros 4 pontos de integração quando há sprite custom; corrigido para só
  aplicar tint nos estados que precisam continuar visíveis como aviso.
- **BiomeId do evento:** `CaveBiomeChangedEvent.BiomeId` vem do `CaveBiomeArtProfileSO.BiomeId`
  resolvido para a banda (string vazia se não houver profile) — deliberadamente não usa
  `_defaultBiomeId`/`CaveBiomeResolver` (achado #2), pois esses são bugs de gameplay pré-existentes
  fora do escopo desta spec de apresentação.

## 3. Arquivos criados

```
Assets/_Game/Scripts/Cave/Art/CaveBiomeArtProfileSO.cs
Assets/_Game/Scripts/Cave/Art/CaveBiomeArtResolver.cs
Assets/_Game/Scripts/Cave/Art/CaveBiomeChangeDecision.cs
Assets/_Game/Scripts/Cave/Art/CaveBiomeArtDebug.cs
Assets/_Game/Scripts/Core/Events/CaveBiomeChangedEvent.cs
Assets/_Game/Scripts/Editor/Cave/GenerateCaveBiomeArtProfiles.cs
Assets/_Game/Scripts/Editor/Cave/ValidateCaveBiomeArtProfiles.cs
Assets/_Game/Scripts/Editor/Cave/CaveBiomeArtDebugMenu.cs
Assets/_Game/Tests/EditMode/Cave/CaveBiomeArtProfilesTests.cs
docs/validation/spec_cave_biome_art_profiles_execution_report.md
```

## 4. Arquivos alterados (diff mínimo, fallback-first)

```
Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs      (+ publicação CaveBiomeChangedEvent)
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs (+ array de profiles, colaborador resolver)
Assets/_Game/Scripts/Cave/Runtime/CaveTileMaterializer.cs    (+ Tilemap condicional floor/wall)
Assets/_Game/Scripts/Cave/Runtime/CaveHazardMaterializer.cs  (+ resolver de sprite hazard/chest)
Assets/_Game/Scripts/Cave/Runtime/CaveTrapMaterializer.cs    (+ resolver de sprite trap/false-chest)
Assets/_Game/Scripts/Cave/Runtime/CaveExitMaterializer.cs    (+ resolver de sprite de saída)
Assets/_Game/Scripts/Cave/Runtime/CaveHazardTile.cs          (+ hasCustomSprite opcional)
Assets/_Game/Scripts/Cave/Runtime/TreasureChestInteractable.cs (+ sprites opcionais closed/open)
Assets/_Game/Scripts/Cave/Traps/FalseChestTrap.cs            (+ sprites opcionais closed/revealed)
Assets/_Game/Scripts/Cave/Traps/TrapBehaviour.cs             (+ sprite opcional do bioma)
Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs               (+ 2 RunStep: gerador + validator)
Assembly-CSharp.csproj            (entradas de compile para os novos arquivos runtime/teste)
Assembly-CSharp-Editor.csproj     (entradas de compile para os novos arquivos de editor)
```

**Nota sobre o `git status` do repo:** o working tree já continha, antes desta sessão, mudanças não
relacionadas de outra spec em progresso paralelo (ex.: normalização de `SpriteRenderer.sortingOrder`/
`CaveWorldSortingLayers` em `CaveBossSpawner.cs`, `CaveEnemyMaterializer.cs`,
`CaveEnvironmentElementMaterializer.cs`, `CaveResourceNodeMaterializer.cs`,
`CaveWanderingMerchant.cs`, `CaveEnemySpawner.cs`, `CaveSmokeTestSpawnerBridge.cs`, e o teste
`EnemyAttackKitPrimitivesTests.cs`/spec `spec_enemy_attack_kits_v1.md`). Esses arquivos **não foram
tocados por esta execução** — confirmado por `git diff` antes de escrever este report.

## 5. Integração fallback-first (critério 14.2)

Todos os pontos de placeholder foram alterados com o padrão: "resolver retorna sprite/tile válido →
usa; caso contrário, mantém EXATAMENTE o código/cor placeholder anterior":

- `CaveHazardMaterializer.MaterializeHazards` → `CaveHazardTile.Configure(hasCustomSprite)`.
- `CaveHazardMaterializer.MaterializeTreasureRoom` → `TreasureChestInteractable.Configure(closedSprite, openSprite)`.
- `CaveTrapMaterializer.MaterializeTraps` → `FalseChestTrap.Configure(closedSprite, revealedSprite)` e `TrapBehaviour.Configure(biomeSprite)`.
- `CaveExitMaterializer.Materialize` → sprite de saída opcional nos 2 GameObjects de fallback (BackExit/ForwardExit).
- `CaveTileMaterializer.MaterializeFloor/MaterializeWalls` → Tilemap só criado se `FloorTiles.Length > 0` / `WallFaceTile/WallTopTile != null`.

Com os 8 profiles vazios (estado atual sem gerador rodado em Unity), `CaveBiomeArtResolver` retorna
`false` em todo `TryGet*`, e o comportamento é idêntico ao pré-spec.

## 6. Determinismo (stable-run)

Variação de floor tile usa exclusivamente `CaveBiomeArtResolver.ComputeCellHash` (que delega para
`CaveLayoutStableHash.Compute` — FNV-1a, mesmo hash já usado por `CaveEnemySpawner`/
`CaveResourceNodeMaterializer`). Nenhum `System.Random`/`UnityEngine.Random`/`string.GetHashCode` foi
usado. Nenhum campo novo de snapshot/save foi adicionado — a arte é 100% derivada em runtime pelo
mesmo (worldSeed, runSeed, level, x, y) a cada materialização, incluindo revisitas via snapshot
(`MaterializeFromSnapshot` chama o mesmo `MaterializeInternal`, que passa pelo mesmo caminho de
`CaveTileMaterializer`).

## 7. Evento de bioma (critério 14.4)

`CaveBiomeChangedEvent { PreviousBiomeId, BiomeId, BandId, CaveLevel }` publicado por
`CaveLevelRuntimeController.PublishBiomeChangedIfNeeded`, chamado nos 2 pontos onde
`CaveLevelEnteredEvent` já era publicado (geração nova + restauração de snapshot). A decisão de
"a banda mudou?" é 100% pura (`CaveBiomeChangeDecision.HasBandChanged`), com sentinela
`NoPreviousBand = -1` garantindo que a primeira entrada da run sempre publica. Checado o catálogo de
eventos antes de criar: **não existe `docs/architecture/EVENT_CATALOG.md`** no repo (nenhum evento
catalogado formalmente ainda neste projeto) e um `grep` por `CaveBiomeChangedEvent`/conceito
equivalente não encontrou nada — sem duplicação.

## 8. Toggle dev T011 (fatia de teste autorizada 2026-07-03)

`CaveBiomeArtDebug.ForcedBandId` (nullable int, default `null`), guard `#if UNITY_EDITOR ||
DEVELOPMENT_BUILD` (mesmo padrão double-guard do `CaveDebugLevelSkipController`/spec_codex_11).
`CaveBiomeArtDebug.ResolveBandForArt(naturalBandId)` é chamado em TODOS os pontos que resolvem banda
para ARTE (Tilemap, hazard, chest, trap, exit, evento) — nunca nos pontos de gameplay
(`CaveBandScaling.BandForLevel` puro continua sendo usado sem alteração para spawn/loot/dano por
tier do trap). Menu: `CindarsHope/Dev/Cave/Forcar Banda De Arte (Bioma 1)` e
`CindarsHope/Dev/Cave/Desligar Banda De Arte Forcada` — carve-out `Dev/` permitido pela rule
`editor-generation-orchestration` (não é geração canônica, não tem passo em
Inicializar/Validar/Reparar).

## 9. Editor tooling

- `GenerateCaveBiomeArtProfiles.Generate()` — público, `static`, SEM `[MenuItem]` — registrado como
  `RunStep("Gerar perfis de arte de bioma da caverna", ...)` em `CindarsHopeMenu.InicializarProjeto`,
  logo após "Anexar perfis default de fase de boss" (FASE A, antes de salvar assets/recriar cenas).
  Idempotente: `AssetDatabase.LoadAssetAtPath` por path fixo (`CaveBiomeArtProfile_<biomeId>.asset`)
  decide criar vs. atualizar; `Tile` assets cacheados por nome do sprite em
  `Assets/_Game/Data/Cave/Biomes/_TileAssets/`. Convenção de pasta de arte:
  `Assets/_Game/Art/Generated/World/cave/<biomeId>/{floor_a,floor_b,floor_detail,wall_face,wall_top}.png`.
  Arte ausente = campo permanece/vira null, sem erro (confirmado: só `biome_stone_cavern` tem os 4
  arquivos hoje; os outros 7 biomas geram profile vazio).
- `ValidateCaveBiomeArtProfiles.Validate()` — read-only, registrado em `CindarsHopeMenu.ValidarProjeto`.
  ERROR apenas para: contagem != 8, biomeId ausente/desalinhado com `CaveBiomeRegistrySO`, bandId
  duplicado entre profiles. WARNING (nunca ERROR) para profile totalmente vazio de arte.

## 10. Bloco de validação (rule validation-truth)

```text
Validation method: dotnet build (Assembly-CSharp + Assembly-CSharp-Editor) + tools/docs/run_strict_validation.ps1
Assembly-CSharp: PASS (exit 0, 0 erros, 0 warnings novos — 5 warnings pré-existentes)
Assembly-CSharp-Editor: PASS (exit 0, 0 erros, 0 warnings novos — 7 warnings pré-existentes)
Docs validation (validate_docs.ps1, dentro do strict harness): EXPECTED_FAIL_LEGACY_ONLY
  - Erros de "missing dependency header"/"missing required_adrs/required_game_rules field" também
    ocorrem em 3 outras specs não relacionadas já na fila (spec_enemy_attack_kits_v1,
    spec_npc_physics_cat_companion, spec_town_building_visuals, spec_town_layout_v9_organic — todas
    untracked no git antes desta sessão). A spec desta execução
    (spec_cave_biome_art_profiles_runtime.md) também não tem esses headers de metadata — pré-existente
    no arquivo entregue, não introduzido/alterado por esta execução (spec não editada). Gap sistêmico
    de format de metadata na fila .specs/a_implementar/, fora do scope desta spec de runtime/arte.
  - "Placeholder found" nos arquivos tools/codex/Generate-CodexHarness.ps1: arquivo já modificado no
    working tree antes desta sessão (git status --short mostra " M", não introduzido por mim).
Quality check (Assembly builds): PASS
Result artifact: (LAST_STRICT_VALIDATION_RESULT.json não verificado nesta execução — strict harness
  parou no gate de diff completeness antes de gerar o artifact; resolvido por este próprio report)
```

## 11. Testing Quality Gate

```text
Changed deterministic logic: YES (CaveBiomeArtResolver, CaveBiomeChangeDecision)
Requires EditMode tests: YES — CaveBiomeArtProfilesTests.cs (18 testes):
  - Fallback null-safe: 4 testes (lista vazia, lista null, profile sem floor tiles, trapId desconhecido)
  - Mapeamento banda->profile: 2 testes (múltiplos profiles, bandId duplicado)
  - Determinismo do hash: 4 testes (mesma entrada -> mesmo hash, célula diferente -> hash diferente,
    hash sempre não-negativo, mesma célula -> mesmo tile ao longo de chamadas repetidas)
  - Decisão de troca de banda: 3 testes (primeira entrada conta, banda igual não conta, banda
    diferente conta)
  - CaveBandScaling.BandForLevel (sanity contra os 8 biomas): 1 teste
  - Toggle dev T011: 2 testes (default resolve banda natural; forçado sobrescreve só arte, não gameplay)
Requires PlayMode automated or final human scenario: YES — cenário humano abaixo (seção 12)
Requires regression test: YES — coberto pelos testes de fallback null-safe acima + revisão manual do
  diff (nenhum ponto de placeholder perdeu seu else/fallback original)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
Minimum validation evidence for ACCEPTED: builds exit 0 (OK) + EditMode PASS (NOT RUN — Unity Test
  Runner requer o Editor; ver seção 13) + validator OK (NOT RUN pelo mesmo motivo) + Play Mode humano
  do cenário documentado (NOT RUN)
```

## 12. Cenário de teste humano (Play Mode, deferido)

1. Abrir o Unity Editor no projeto (branch `dev`).
2. Rodar `CindarsHope/Inicializar Projeto` (materializa os 8 `CaveBiomeArtProfileSO` em
   `Assets/_Game/Data/Cave/Biomes/`; `biome_stone_cavern` deve vir com `floorTiles` (2),
   `wallFaceTile` e `wallTopTile` preenchidos; os outros 7 devem vir vazios).
3. Rodar `CindarsHope/Validar Projeto` e conferir no Console: `[CV01 art profile validation] PASS`
   (ou só WARNING de campos vazios para os 7 biomas sem arte — nunca ERROR).
4. Em Play Mode, abrir `CindarsHope/Dev/Cave/Forcar Banda De Arte (Bioma 1)` e entrar na
   `CaveScene` em qualquer nível (mesmo nível 50+): o chão/parede devem mostrar os tiles de
   `biome_stone_cavern` (não os placeholders cinza/marrom) — confirma a arquitetura de 3 camadas
   (doc de direção §3.9) ponta a ponta.
5. Descer/voltar de nível (ForwardExit/BackExit) e **revisitar o mesmo nível**: o padrão de tiles
   deve ser visualmente IDÊNTICO à primeira visita (determinismo do hash — critério 14.3).
6. Desligar `CindarsHope/Dev/Cave/Desligar Banda De Arte Forcada`, descer por níveis reais até trocar
   de banda natural (ex.: nível 10→11): confirmar no Console/log (ou breakpoint) que
   `CaveBiomeChangedEvent` é publicado exatamente uma vez na transição, não a cada entrada de nível.
7. Conferir hazards/traps/baú/saídas em `biome_stone_cavern`: como os sprites soltos (hazard/trap/
   chest/exit) não foram populados pelo gerador nesta execução (T007 popula só floor/wall tiles do
   lote de teste — ver nota abaixo), eles devem continuar exatamente com os placeholders de cor
   atuais mesmo com o toggle ligado — confirma fallback-first também para os campos ainda vazios.

## 13. NOT RUN / BLOCKED

```text
Unity Editor batchmode / Test Runner EditMode: NOT RUN
  Reason: ambiente de execução desta sessão é headless (sem Unity Editor aberto/acessível via
  automação); só dotnet build foi possível.
  Residual risk: os 18 testes novos compilam (confirmado via dotnet build), mas não foram executados
  via NUnit/Unity Test Runner nesta sessão. Humano deve rodar via Window > General > Test Runner >
  EditMode antes de promover a spec.
Geração dos 8 CaveBiomeArtProfileSO (Inicializar Projeto): NOT RUN (mesma razão acima)
  Residual risk: gerador não foi exercitado em Unity real; só revisado estaticamente. Caminho de
  criação de Tile assets (AssetDatabase.CreateAsset) segue o padrão comprovado de
  WorldTilemapGround.GetTile, mas não foi confirmado em runtime.
Play Mode scenario (seção 12): NOT RUN — deferido para validação humana final.
```

## 14. Anti-regressão (autoverificação)

```text
Layout/spawn/loot/snapshot da cave: NÃO alterados (CaveBiomeLayoutProfile, planners de
  spawn/loot/hazard/trap não foram tocados; apenas os pontos de MATERIALIZAÇÃO VISUAL consultam o
  resolver antes do fallback).
Random/GetHashCode para variação visual: NÃO usado — só CaveLayoutStableHash via
  CaveBiomeArtResolver.ComputeCellHash.
Campo novo em save/snapshot: NÃO adicionado.
[MenuItem] avulso: NÃO criado (gerador/validador registrados via RunStep; o único [MenuItem] novo,
  CaveBiomeArtDebugMenu, está sob o submenu CindarsHope/Dev/ — carve-out explícito da rule).
Placeholders atuais quebrados com profile vazio: NÃO — fallback-first verificado ponto a ponto
  (seção 5).
biomeIds existentes renomeados: NÃO.
```

## 15. Escopo NÃO executado / desvios

```text
Não corrigido: _defaultBiomeId literal fixo ("biome_cave_earth") em CaveLevelRuntimeController e
  CaveBiomeResolver órfão (achado #2) — bug de gameplay pré-existente, fora do escopo desta spec de
  apresentação visual. Recomendo bugfix dedicado em spec futura (fora deste lote).
Sprites soltos de hazard/trap/chest/exit do biome_stone_cavern: NÃO populados pelo gerador nesta
  execução — a convenção de pasta staged (floor_a/b, wall_face/top) cobre só terreno; o lote 2 de
  arte (props/hazard/trap/chest/exit sprites) fica para entrega futura, conforme "Fora de escopo"
  da spec (linha 151: "produzir arte nova"). O código do resolver/materializers já suporta esses
  campos assim que a arte existir — só rodar Inicializar Projeto de novo.
Reskin de baú bandas 5-7 e collider do Tilemap de parede: não avaliados (nota "34. Notas para
  execução posterior" da spec já assinala isso para depois do lote 2).
```

## 15.1 Auditoria de não-regressão (non-regression-auditor)

Executada via agent `non-regression-auditor` sobre o diff completo (9 dimensões). Resultado:
**WARNING** (sem blocker/FAIL). Achados e resolução:

```text
[HIGH] Build não re-verificado pelo auditor (ambiente Bash indisponível na sessão dele).
  RESOLVIDO: builds re-executados nesta sessão (orquestrador) após TODAS as edições, incluindo a
  correção do item abaixo — Assembly-CSharp exit 0 (0E/5W pré-existentes), Assembly-CSharp-Editor
  exit 0 (0E/7W pré-existentes). Ver bloco de validação (seção 10).

[MEDIUM] TrapBehaviour.ApplyVisual aplicava o tint de estado incondicionalmente por cima do sprite
  do bioma (diferente do padrão Color.white dos outros 4 pontos de integração quando há sprite
  custom). CORRIGIDO: estado padrão (Armed, não detectado) agora fica opaco (Color.white) quando há
  biomeSprite — a arte real já "camufla" a trap no cenário; os demais estados
  (Telegraphing/Triggered/Disarmed/detectado) continuam aplicando o tint de aviso por cima
  (necessário para o telegraph CA-2 continuar legível). Rebuildado após a correção — exit 0 ambos.

[LOW] Duplicação da tabela de 8 biomas (GenerateCaveBiomeArtProfiles.CanonicalBiomes espelha
  CaveBiomeRegistrySO.DefaultBiomes, que é private). ACEITO como risco residual documentado (seção 2
  já registrava esta escolha); recomendação do auditor (expor a tabela publicamente no registry)
  registrada como nota para spec futura, não bloqueia esta execução.

[LOW/NIT] "Runtime.CaveBandScaling" com qualificação de namespace redundante dentro do próprio
  namespace Runtime — puramente estético, sem risco funcional; não alterado (evitar diff-churn
  adicional sem ganho).
```

## 16. Definition of Done — checklist

```text
[x] T001 Fase 0 auditoria completa (seção 1)
[x] T002 CaveBiomeArtProfileSO + resolução banda->profile (array serializado, menor mudança)
[x] T003 CaveBiomeArtResolver (C# puro, TryGet*, hash FNV-1a reusando CaveLayoutStableHash)
[x] T004 CaveBiomeChangedEvent + publicação na troca de banda + checagem de duplicação no catálogo
[x] T005 Integração fallback-first: trap, chest, false chest, hazard tiles, exits
[x] T006 Tilemap floor/wall condicional (sem collider — colisão via GameObject inalterada)
[x] T007 GenerateCaveBiomeArtProfiles + RunStep em InicializarProjeto
[x] T008 ValidateCaveBiomeArtProfiles (read-only) + RunStep em ValidarProjeto
[x] T009 EditMode tests (18 testes, 5 categorias)
[x] T010 Builds (dotnet, exit 0/0 erros ambos) + docs validation (EXPECTED_FAIL_LEGACY_ONLY,
    não introduzida por esta execução) + este execution report
[x] T011 Toggle dev de bioma fixo (banda 1), guard duplo, menu Dev/, afeta só arte
[ ] Unity batchmode/EditMode Test Runner real: NOT RUN (seção 13)
[ ] Play Mode humano: DEFERRED_TO_FINAL_VALIDATION (seção 12)
```

Status máximo desta spec nesta execução: **BUILD_VALIDATED** (Play Mode/EditMode Unity real
deferidos — consistente com a Definition of Done da spec, linha 419-423).

---

## 17. Fix pós-Play-Mode 2026-07-04: wiring registry via Resources

### Bug confirmado (Play Mode real)

Os 8 `CaveBiomeArtProfileSO` existiam em `Assets/_Game/Data/Cave/Biomes/` (gerados pelo
`Inicializar Projeto`, com `TileAssets` do bioma 1 preenchidos), mas o `CaveRuntimeMaterializer`
só os recebia via `[SerializeField] private CaveBiomeArtProfileSO[] _biomeArtProfiles`. A
`CaveScene` existente (criada ANTES desta spec) tinha esse array **vazio** — nada o preenchia
automaticamente. Resultado: `CaveBiomeArtResolver` era construído sempre vazio, o `hasFloorTiles`/
`hasWallTiles` de `CaveTileMaterializer` era sempre `false`, e nenhum tile/sprite de bioma
aparecia. Pior: era **100% silencioso** — nem um log indicava que o wiring estava ausente. Já
documentado como escolha deliberada na seção 2 ("evita depender de `Resources.Load` em runtime"),
mas essa escolha assumia implicitamente que a cena seria sempre recriada/rewireada — o que não
acontece para uma `CaveScene` pré-existente.

### Root cause

`_biomeArtProfiles` é um array serializado no `CaveRuntimeMaterializer` (MonoBehaviour de cena).
Cenas criadas antes da spec CV01 nunca tiveram esse campo populado no Inspector, e nenhum
generator/scene-creator o preenche. `EnsureCollaborators()` construía o resolver diretamente do
array vazio, sem qualquer fallback ou log.

### Fix (2 partes, diff mínimo)

1. **Fallback via Resources** (precedente exato: `EnsureCombatDatabasesBound()` /
   `Resources.Load<CombatRuntimeDatabasesRegistrySO>("CombatRuntimeDatabasesRegistry")` na mesma
   classe):
   - Novo `CaveBiomeArtProfileRegistrySO` (`Assets/_Game/Scripts/Cave/Art/CaveBiomeArtProfileRegistrySO.cs`)
     — `List<CaveBiomeArtProfileSO> Profiles` + `EditorSetProfiles` (editor-only, mesmo padrão de
     `EditorSetBandId`/`EditorSetFloorTiles`).
   - `GenerateCaveBiomeArtProfiles.Generate()` agora também cria/atualiza idempotentemente
     `Assets/_Game/Resources/CaveBiomeArtProfileRegistry.asset` com os 8 profiles (mesma pasta
     Resources de `CombatRuntimeDatabasesRegistry.asset`).
   - `CaveRuntimeMaterializer.EnsureCollaborators()` agora chama `ResolveBiomeArtProfiles()`: se
     `_biomeArtProfiles` (array serializado) está vazio/null, tenta
     `Resources.Load<CaveBiomeArtProfileRegistrySO>("CaveBiomeArtProfileRegistry")`. Serialized
     field continua sendo a fonte de verdade quando preenchido — nenhuma mudança de comportamento
     para cenas já wireadas manualmente.
2. **Observabilidade** (skill `observability-and-logging`, formato canônico de wiring-error):
   - `CaveRuntimeMaterializer.LogBiomeArtWiringStatusOnce` — log one-shot por instância na
     construção do resolver: quantos profiles resolvidos e de onde (`serialized field` /
     `Resources/CaveBiomeArtProfileRegistry` / `none`). Zero profiles emite `Debug.LogWarning` com
     formato `[Cave][Wiring]` (sistema, scene, GameObject, campo, fallback ativo, como corrigir:
     rodar `CindarsHope/Inicializar Projeto` ou wireiar manualmente).
   - `CaveTileMaterializer.LogFloorPaintStatusOnce`/`LogWallPaintStatusOnce` — log one-shot
     informativo (`[Cave]`) quando pinta Tilemap (banda, N células) e quando pula por falta de
     tiles no profile — antes era silencioso nos dois casos.
   - `ValidateCaveBiomeArtProfiles.Validate()` agora também checa a existência/contagem do registry
     em `Assets/_Game/Resources/CaveBiomeArtProfileRegistry.asset` (WARNING se ausente/incompleto,
     nunca ERROR — mesma severidade dos demais campos de arte ausente).

### Arquivos alterados/criados nesta correção

```
Criados:
  Assets/_Game/Scripts/Cave/Art/CaveBiomeArtProfileRegistrySO.cs

Alterados:
  Assets/_Game/Scripts/Editor/Cave/GenerateCaveBiomeArtProfiles.cs   (+ materializa registry em Resources)
  Assets/_Game/Scripts/Editor/Cave/ValidateCaveBiomeArtProfiles.cs   (+ checagem do registry, WARNING)
  Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs       (+ ResolveBiomeArtProfiles fallback + log one-shot)
  Assets/_Game/Scripts/Cave/Runtime/CaveTileMaterializer.cs          (+ log one-shot floor/wall paint status)
  Assets/_Game/Tests/EditMode/Cave/CaveBiomeArtProfilesTests.cs      (+ 2 testes: registry alimenta resolver, null-safety)
  Assembly-CSharp.csproj                                             (+ entrada de compile do registry SO)
```

Nenhum arquivo `.unity`/`.prefab`/YAML editado manualmente. Nenhuma mudança em layout/spawn/loot/
snapshot da cave. Sem `FindObjectOfType`/`GameObject.Find`. Nenhum sub-agente foi spawnado durante
esta correção.

---

## 20. Fix pós-Play-Mode 2026-07-04 (2ª rodada) — massa de parede lendo como piso + player atravessando parede

Dois bugs reportados pelo humano em Play Mode real na CaveScene, via screenshot: (1) a massa de
parede (várias células grossas) pintada inteira com `wallTopTile` claro/texturizado parecia um
segundo piso andável; (2) o player estava de fato **andando sobre** essa massa de parede.

### 20.1 FIX 1 — leitura visual de parede fina (borda vs. miolo)

**Causa:** `CaveTileMaterializer.MaterializeWalls` pintava toda célula sem chão-ao-sul com o mesmo
`wallTopTile`, sem distinguir o aro de 1 célula (visível, deveria ler como parede) do interior da
massa (nunca deveria ser confundido com chão). Como o tile é o mesmo claro/texturizado em toda a
extensão, uma massa de 3+ células de espessura lia visualmente como platô caminhável.

**Fix:** novo método puro `CaveTileMaterializer.IsWallInterior(Vector2Int wallPos, CaveGeneratedLevel
level)` — miolo = nenhum dos 4 vizinhos ortogonais (N/S/L/O) está em `WalkableTiles`; caso contrário é
borda. Em `MaterializeWalls`, célula de miolo recebe tint quase-preto quente por-célula via
`wallTilemap.SetTileFlags(pos, TileFlags.None); wallTilemap.SetColor(pos, WallInteriorTint)`
(`WallInteriorTint = new Color(0.35f, 0.33f, 0.32f)`, constante de apresentação local ao
materializer — não é valor de gameplay/balance, é puramente visual, então não vai para um SO de
balance). Nenhuma mudança em `WalkableTiles`/`WallTiles`/geração/layout/spawn/snapshot — só
apresentação do `WallTilemap`. Log one-shot de parede (`LogWallPaintStatusOnce`) agora reporta
`borda=N, miolo=M` junto da contagem total.

**Resultado esperado:** aro fino de 1 célula ao redor de cada sala/corredor (tile normal, mesmo
comportamento de antes) + massa interior sólida escurecida (estilo minas do Stardew) — visualmente
não-caminhável.

### 20.2 FIX 2 — player atravessando parede (achados a-d solicitados)

**a) Physics layer da parede:** os `GameObject`s de parede criados por `CaveTileMaterializer.
MaterializeWalls` **nunca recebem layer explícito** — ficam em `Default` (`GameplayLayerNames.
WorldSolid`, criado pela spec_codex_13, só é usado hoje para `OverlapCircle`/obstacle avoidance de
inimigo em `CaveEnemyMaterializer.cs` e `EnemyBrain.cs`; não há nenhuma entrada de collision matrix
nem `Physics2D.IgnoreLayerCollision` no projeto). O player (`CreateMvpCaveScene.CreatePlayer`)
também fica em `Default`. **Conclusão: layers não são a causa** — Default colide com Default por
padrão do Unity; não há filtro de camada bloqueando a colisão.

**b) Rigidbody2D/Collider2D do player — CAUSA RAIZ:** `CreateMvpCaveScene.CreatePlayer()` criava o
`Rigidbody2D` do player como **`RigidbodyType2D.Kinematic`**. Um rigidbody Kinematic **nunca sofre
resolução física de colisão do Unity contra colliders estáticos** (o `BoxCollider2D` das paredes,
`size=Vector2.one`, sem `Rigidbody2D` próprio) — `Rigidbody2D.MovePosition` num Kinematic simplesmente
move, sem física de bloqueio/empurrão. `PlayerController.FixedUpdate` move via
`_rigidbody.MovePosition(nextPosition)` sem nenhuma verificação de colisão própria — a única defesa
contra atravessar era o `CavePlayerPathConfinement` (snap de grid pós-frame em `LateUpdate`), que
verifica só o **ponto central** do transform (`WorldToGridPosition` com `RoundToInt`), não o corpo do
`BoxCollider2D` (`size = 0.6 x 1`). Isso permite o corpo/sprite do player sobrepor visualmente até
~0.5 unidade de parede antes do centro cruzar a fronteira da célula — combinado ao FIX 1 (antes do
qual toda a parede parecia igual), o efeito lido pelo humano era "andando sobre a parede".
Comparando com `CreateMvpFarmScene.CreatePlayer()` (linha ~458): a Farm já usa
`RigidbodyType2D.Dynamic + gravityScale=0 + CollisionDetectionMode2D.Continuous + FreezeRotation` —
o padrão comprovado nunca foi aplicado à Cave.

**c) PlayerDashController / PlayerMovementDisplacementResolver:** o Dash usa
`_collider.Cast(direction, filter, hits, distance)` com `Physics2D.DefaultRaycastLayers` — isso
detecta corretamente o `BoxCollider2D` da parede independente do bodyType do player (é uma cast
explícita, não física passiva), e recorta a distância segura (`nearestDistance - 0.05f`). Dash **não
é** o vetor do bug; já tinha proteção própria.

**d) CavePlayerPathConfinement:** `IsGridWalkable` usa corretamente
`generatedLevel.WalkableTiles.Contains(gridPos)` — células de parede nunca estão em `WalkableTiles`,
então a lógica de bloqueio em si está correta. O problema não é a lógica de validação, é que ela
sozinha (amostragem de ponto, sem correlação com o tamanho do collider do player) não é suficiente
para impedir sobreposição visual quando não há física real de colisão por trás (item b). Nenhuma
mudança feita neste arquivo — mantido como segunda camada de defesa.

**Fix aplicado (menor mudança):** `CreateMvpCaveScene.cs`, método `CreatePlayer()` — trocado
`RigidbodyType2D.Kinematic` para `RigidbodyType2D.Dynamic` + `gravityScale=0f` +
`CollisionDetectionMode2D.Continuous`, alinhado ao padrão já validado da `CreateMvpFarmScene.cs`.
`CavePlayerPathConfinement` permanece sem alterações (segunda camada, grid-snap). Nenhuma mudança em
spawn anchor, layout, geração procedural ou snapshot.

**Escopo intencionalmente não alterado:** não foi tocado o `KnockbackController`, `PlayerAttackController`,
nem o rigidbody do slime inimigo (linha 607 do mesmo arquivo, kinematic, não relacionado ao player).

### 20.3 Testing Quality Gate

Regression test: `Assets/_Game/Tests/EditMode/Cave/CaveBiomeArtProfilesTests.cs` — 5 testes novos
cobrindo `CaveTileMaterializer.IsWallInterior` (lógica pura de vizinhança, extraível sem Unity):
célula de parede com vizinho ortogonal walkable ao sul/norte/leste-oeste é borda; célula cercada só
por outras paredes é miolo; vizinho apenas diagonal (não-ortogonal) não conta como borda.

O FIX 2 (Rigidbody2D bodyType do player na cave) é **wiring de scene creator** (`CreateMvpCaveScene.
CreatePlayer`), não lógica pura extraível para EditMode — JUSTIFICATIVA: resolução de colisão física
Kinematic-vs-Dynamic só é observável em Play Mode real (é comportamento do motor de física do Unity,
não código do projeto). Play Mode scenario (residual risk documentado): humano deve regenerar a
CaveScene via `CindarsHope/Inicializar Projeto` (ou re-rodar `CreateMvpCaveScene` diretamente) e
confirmar em Play Mode que (1) o aro fino de parede está visualmente distinto da massa escura
interior, e (2) o player não atravessa/sobrepõe a massa de parede ao andar contra ela em qualquer
direção (incluindo diagonal) nem durante o Dash.

**Residual risk:** a CaveScene existente no disco (`.unity`) só reflete este fix depois de
regenerada — até lá, o player em cena continua Kinematic. `CavePlayerPathConfinement` (grid-snap) e
o novo `Dynamic` rigidbody são camadas complementares; nenhuma delas sozinha foi projetada para ser
a única defesa, então a combinação deve ser reverificada em Play Mode antes de fechar como resolvido.

### 20.4 Validação

| Level | Result |
|-------|--------|
| `dotnet build Assembly-CSharp.csproj --no-restore` | PASS, exit 0 (0 erros, 5 warnings pré-existentes não relacionados) |
| `dotnet build Assembly-CSharp-Editor.csproj --no-restore` | PASS, exit 0 (0 erros, 7 warnings pré-existentes não relacionados) |
| EditMode test (novo) | `CaveBiomeArtProfilesTests.cs` +5 testes — compila junto (verificado no build acima); execução real via Unity Test Runner NÃO RODADA nesta sessão (BLOCKED — evitar batchmode paralelo com Unity aberto, rule unity-assets) |

### Arquivos alterados nesta correção (2ª rodada)

```
Alterados:
  Assets/_Game/Scripts/Cave/Runtime/CaveTileMaterializer.cs         (+ IsWallInterior, tint de miolo, log borda/miolo)
  Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs   (Rigidbody2D do player: Kinematic -> Dynamic)
  Assets/_Game/Tests/EditMode/Cave/CaveBiomeArtProfilesTests.cs     (+5 testes de IsWallInterior)
```

Nenhum arquivo `.unity`/`.prefab`/YAML editado manualmente. Nenhuma mudança em `WalkableTiles`/
`WallTiles`/spawn anchor/geração procedural/snapshot. Sem `Random`/`FindObjectOfType`/`GameObject.Find`
novos. Nenhum sub-agente foi spawnado durante esta correção.

### Bloco de validação (rule validation-truth)

```text
Validation method: dotnet build (Assembly-CSharp + Assembly-CSharp-Editor), via PowerShell
Assembly-CSharp: PASS — exit code 0, 0 erros, 5 warnings pré-existentes (mesmos da execução original)
Assembly-CSharp-Editor: PASS — exit code 0, 0 erros, 7 warnings pré-existentes (mesmos da execução original)
Unity batchmode / EditMode Test Runner: NOT RUN (ambiente headless desta sessão; ver testes novos
  descritos abaixo — compilam via dotnet build, não executados via NUnit/Unity Test Runner)
```

### Testing Quality Gate

```text
Regression test: Assets/_Game/Tests/EditMode/Cave/CaveBiomeArtProfilesTests.cs
  - Registry_EditorSetProfiles_ProfilesResolveThroughResolver_SameAsSerializedArray: prova que a
    lista do registry alimenta o CaveBiomeArtResolver exatamente como o array serializado (mesma
    resolução banda->profile).
  - Registry_EditorSetProfiles_Null_BecomesEmptyList_NeverNull: null-safety do setter do registry.
  - A lógica de decisão "array vazio -> Resources.Load -> array vazio -> log de 0 profiles" dentro
    de ResolveBiomeArtProfiles() não foi extraída para C# puro isolado (ela chama
    UnityEngine.Resources.Load, que exige um asset real carregado por engine — não testável em
    EditMode sem Resources.Load real). A parte pura (registry -> resolver) já está coberta pelos 2
    testes acima; o caminho Resources.Load em si é coberto pelo cenário humano abaixo.
Residual risk: o carregamento real via Resources.Load (Unity Editor/Play Mode) não foi exercitado
  nesta sessão headless — só revisado estaticamente, seguindo o mesmo padrão já comprovado de
  CombatRuntimeDatabasesRegistrySO. Humano deve confirmar no cenário abaixo.
```

### O que o humano precisa rodar no Unity

1. Rodar `CindarsHope/Inicializar Projeto` **1 vez** — isso materializa (idempotentemente) os 8
   `CaveBiomeArtProfileSO` já existentes E cria/atualiza
   `Assets/_Game/Resources/CaveBiomeArtProfileRegistry.asset` com os 8 profiles.
2. Rodar `CindarsHope/Validar Projeto` e conferir no Console: deve aparecer
   `CaveBiomeArtProfileRegistry encontrado em ... com 8 profile(s)` na lista de passed (sem ERROR;
   WARNING apenas para os 7 biomas ainda sem arte, como já era o caso).
3. Entrar em Play Mode na `CaveScene`: no Console deve aparecer 1 log
   `[Cave] CaveRuntimeMaterializer: biome art profiles resolved from Resources/CaveBiomeArtProfileRegistry (8 profile(s))`
   (ou `serialized field` se a cena já tiver o array wireado manualmente) — não mais silêncio total.
4. Com `CindarsHope/Dev/Cave/Forcar Banda De Arte (Bioma 1)` ligado, confirmar que o chão/parede da
   `CaveScene` mostram os tiles de `biome_stone_cavern` (não mais só os placeholders cinza/marrom).
5. Caso o Console mostre `[Cave][Wiring] ... 0 CaveBiomeArtProfileSO resolved (source='none ...')`,
   isso confirma que nem o array serializado nem o registry em Resources têm profiles — sinal de
   que o passo 1 não foi executado ou falhou; reportar antes de prosseguir.

## 18. Fix pos-Play-Mode 2026-07-04 (2o teste): offset do Tilemap vs GridToWorld

Sintoma no Play Mode humano (screenshot): fundo cinza total - placeholders sumiram e nenhum tile visivel.
Diagnostico: o 1o fix (registry via Resources) FUNCIONOU - os profiles chegaram ao resolver e
hasFloorTiles=true (prova: os placeholders foram desligados). O bug era geometrico: SetTile(x,y) em Grid
na posicao zero pinta em (x,y) puro, mas GridToWorld centraliza o nivel na origem (x - Width/2). Tiles
pintados meio nivel fora da camera enquanto os placeholders (desligados) ficavam no lugar certo.
Fix: CreateTilemapLayer agora recebe o level e posiciona o Grid em (-Width/2 - 0.5, -Height/2 - 0.5)
- o -0.5 alinha o centro da celula (anchor 0.5) com a posicao dos placeholders.
Validacao: dotnet build Assembly-CSharp exit 0 (verificado pelo orquestrador). Play Mode humano: repetir
o teste (nao precisa re-rodar Inicializar Projeto - fix e codigo puro).
Residual: alinhamento visual fino (meio-tile) so confirmavel em Play Mode.

## 19. Iteracao de qualidade de arte 2026-07-04 (pos 3o Play Mode)

Feedback humano: pipeline funcionou mas o chao ficou "ruido" e a parede avermelhada. Causa: v1 do
_reseam_cave_tiles.py encolhia a textura inteira (1254px) para 64px, destruindo as pedras; blend
seamless borrava o padrao de blocos da parede. Fix v2: crop de regiao central de 512px -> 128px/tile
(PPU 128 no carve-out do GeneratedSpriteImporter), blend so em chao (paredes crop-only), e
floor_detail novo (crop deslocado). Prefabs da cena confirmados nulos ({fileID: 0}) - o render em
Play e 100% a arte nova. Humano: re-rodar Inicializar Projeto 1x (wire do floor_detail) e Play.
Residual: transicao chao->parede ainda quadrada (Rule Tile de borda = lote 2); decor/props = fable_78.

## 21. Bugfix 2026-07-04 (4ª rodada) — inimigos atravessando/em cima da parede + degradê de parede mais escuro

### 21.1 BUG A — inimigos da cave atravessam/ficam em cima da massa de parede

**Investigação (sem adivinhar, evidência lida antes de editar):**

1. **Criação real dos inimigos:** confirmado que `CreateMvpCaveScene.CreateEnemies`/`CreateSlime`
   (linha ~607, `Rigidbody2D.Kinematic`) é **código morto** — a chamada está comentada
   (`// CreateEnemies(playerTransform);`, linha 100) e não existe nenhum `.prefab` de inimigo no
   projeto (`Glob **/Prefabs/**/*nemy*.prefab` = 0 resultados). O path real é
   `CaveEnemyMaterializer.CreateEnemyRuntimeObject` (via `CaveEnemySpawnPlanner`), confirmado pela
   cena (`CaveScene.unity` linha 277: `_enemyPrefab: {fileID: 0}` — null, então
   `ConfigureEnemyRuntimeObject` usa o path procedural `new GameObject(...)`, não
   `Object.Instantiate(_enemyPrefab, ...)`).
2. **Configuração do Rigidbody2D do inimigo real** (`CaveEnemyMaterializer.
   ConfigureEnemyRuntimeObject`, antes do fix): `AddComponent<Rigidbody2D>()` (bodyType default do
   Unity = Dynamic) + `gravityScale = 0f` + `constraints = FreezeRotation`. **Faltava**:
   `collisionDetectionMode = Continuous` — exatamente o componente do fix já documentado e validado
   para o player na seção 20.2b, que usa `RigidbodyType2D.Dynamic + gravityScale=0 +
   CollisionDetectionMode2D.Continuous + FreezeRotation` (padrão também usado em
   `CreateMvpFarmScene.CreatePlayer`).
3. **Como o inimigo se move:** `EnemyBrain.Update()` chama
   `EnemyMovementExecutor.ExecuteMovement()` a cada frame (não `FixedUpdate`), que seta
   `_rb.linearVelocity` (física real, não `transform.position`) para a maioria dos moves — correto
   em princípio (`EnemyChaseController` legado usa `Rigidbody2D.MovePosition`, também correto).
   Porém: (a) setar velocidade em `Update()` em vez de `FixedUpdate()` introduz jitter de timing
   entre o tick de decisão e o passo de física; (b) vários moves multiplicam a velocidade bastante
   (`Leaper` 3.2x, `TryBlink` teleporta via `_rb.position` direto, `ChargeLine` 3.2x) — com
   `CollisionDetectionMode2D` default (`Discrete`) e o `BoxCollider2D` da parede tendo só 1 unidade
   de espessura (`collider.size = Vector2.one`, `CaveTileMaterializer.MaterializeWalls`), esses
   bursts de velocidade têm risco real de atravessar/pousar sobre o collider da parede num único
   passo de física — o mesmo mecanismo já documentado para o player (Discrete + corpo rápido
   através de um collider fino), só que ali a causa raiz era Kinematic (bloqueio total) e aqui é
   Dynamic-mas-sem-Continuous (bloqueio parcial/best-effort do motor, que falha em bursts rápidos).
4. **Layer/collision matrix:** `Physics2DSettings.asset` e `DynamicsManager.asset` têm
   `m_LayerCollisionMatrix` = todos os bits `f` (nenhuma exclusão) — layers não são a causa
   (mesma conclusão já documentada na seção 20.2a para o player). `WallTile_x_y` nunca recebe layer
   explícito (fica em `Default`); o inimigo recebe `GameplayLayerNames.Enemy` via
   `TryAssignRuntimeLayer` — mas como a matrix não exclui nada, Default-vs-Enemy colide normalmente.
   Não é a causa.

**Fix aplicado (menor mudança, espelha o fix já validado do player):**
`CaveEnemyMaterializer.ConfigureEnemyRuntimeObject` — no bloco que já configurava o `Rigidbody2D`
do inimigo, adicionado `rigidbody.bodyType = RigidbodyType2D.Dynamic` (defensivo — documenta a
intenção mesmo já sendo o default do Unity, e protege caso um dia `_enemyPrefab` passe a ser
assinado com um Rigidbody2D serializado Kinematic) e
`rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous`. Nenhuma mudança em
spawn position, layout, pack coordination, snapshot, movement logic ou velocity math — só a
configuração física do Rigidbody2D, mesmo escopo do fix do player.

**Escopo intencionalmente não alterado:** `EnemyMovementExecutor.ExecuteMovement` continua sendo
chamado de `Update()` (não `FixedUpdate()`) — mover isso para `FixedUpdate` seria um refactor maior
(o `EnemyBrain.Update` também faz `EvaluateState`/decision-tick/telegraph/threat-memory no mesmo
método, e reordenar o loop de decisão tem risco de regressão em toda a IA); com
`CollisionDetectionMode2D.Continuous`, o Unity já resolve corretamente a colisão contra o
`BoxCollider2D` estático da parede mesmo com velocity setada fora de `FixedUpdate` (o mesmo padrão
já vale para o player, cujo `PlayerController.FixedUpdate` roda em fixed-step mas cujo Dash —
`PlayerDashController`, fora de escopo aqui também — não). `TryBlink`
(`EnemyMovementExecutor.cs`, `_rb.position = destination`) teleporta o Phase-type sem checar
colisão contra parede — não tocado nesta correção: o bug reportado é goblin/spider/rat (tipos
comuns de chase/leap), sem evidência de que `PhaseShortBlink` seja o vetor relatado; se blink
atravessar parede for observado depois, é um bug separado (teleporte precisa de raycast/clamp
próprio, fora do escopo "física de colisão" desta tarefa).

### 21.2 AJUSTE B — parede lendo mais fina (degradê aro→miolo)

**Mudança 1 — miolo mais escuro:** `WallInteriorTint` de `(0.35, 0.33, 0.32)` para
`(0.22, 0.21, 0.20)` (quase-preto), em `CaveTileMaterializer.cs`. Só a constante de apresentação;
nenhuma mudança em geração/colisão/classificação.

**Mudança 2 — tint intermediário no aro-topo (trivial de aplicar, feito):** a classificação
`IsWallInterior` já existente não precisou mudar. Reaproveitando o `southIsFloor` já computado no
loop de `MaterializeWalls`: célula que é **borda** (`!IsWallInterior`) mas **não é a face**
(`!southIsFloor`, ou seja, o vizinho walkable está a norte/leste/oeste, não ao sul) agora recebe
`WallTopEdgeTint = (0.55, 0.52, 0.5)` — mais escuro que o tile normal/wallFaceTile, mais claro que
`WallInteriorTint`. Resultado: degradê face-iluminada (1 célula, sem tint) → aro-topo (tint médio)
→ miolo (tint quase-preto), em vez de "face clara + resto do aro igualmente claro". Log one-shot
(`LogWallPaintStatusOnce`) agora reporta `borda=N (topo-tintado=T), miolo=M`.

### 21.3 Testing Quality Gate

- **BUG A**: mesma justificativa já usada na seção 20.3 para o fix do player — resolução de colisão
  física Dynamic+Continuous-vs-Discrete só é observável em Play Mode real (comportamento do motor
  de física do Unity, não lógica do projeto). Nenhuma lógica pura nova foi introduzida (só
  configuração de componente). JUSTIFIED: sem regression test automatizado; Play Mode scenario
  abaixo é a validação.
- **AJUSTE B**: a classificação de vizinhança (`IsWallInterior`/`southIsFloor`) já está coberta
  pelos 5 testes existentes em `CaveBiomeArtProfilesTests.cs` (não mudou nesta correção — só
  reaproveitada). A aplicação do tint em si (`Tilemap.SetTileFlags`/`SetColor`) exige um
  `Grid`/`Tilemap` real do Unity — não é lógica pura extraível (mesmo padrão já documentado na
  seção 20.3 para `Resources.Load`). JUSTIFIED: sem novo teste; verificação visual via Play Mode.

**Residual risk:** ambas as correções só se manifestam numa `CaveScene` já materializada em Play
Mode; a cena atual em disco só reflete os fixes depois de rodar
`CindarsHope/Inicializar Projeto` (o `CaveRuntimeMaterializer` recria os inimigos/paredes em
runtime a cada carga de nível, então não precisa recriar a cena do zero — mas o Editor precisa
recompilar o código novo antes do próximo Play).

### 21.4 Validação

| Level | Result |
|-------|--------|
| `dotnet build Assembly-CSharp.csproj --no-restore` | PASS, exit 0 (5 warnings pré-existentes não relacionados) |
| `dotnet build Assembly-CSharp-Editor.csproj --no-restore` | **FAIL, exit 1 — NÃO relacionado a esta correção.** Erro: `CindarsHopeMenu.cs(221,23): CS0234 — CindarsHope.Editor.Enemy não existe`. Causa confirmada: `Assembly-CSharp-Editor.csproj` não inclui `Assets/_Game/Scripts/Editor/Enemy/GenerateEnemyWalkAnimations.cs` (arquivo existe no disco, `grep` no `.csproj` não encontra a entrada `<Compile Include=...>`) — csproj desatualizado/stale, provavelmente por o Unity Editor estar aberto e ainda não ter regenerado o projeto após import recente (padrão já registrado em memória do projeto: `reference-unity-open-csproj-transient-build-errors`). Retry (rebuild) 1x deu o mesmo erro. Confirmado que nenhum arquivo desta correção (`CaveEnemyMaterializer.cs`, `CaveTileMaterializer.cs`) está envolvido — ambos já estavam corretamente listados no `Assembly-CSharp.csproj` (que passou) antes desta sessão. |
| EditMode tests | Nenhum teste novo — ver Testing Quality Gate acima (justificativa, sem lógica pura nova) |

**Bloco de validação (rule validation-truth):**

```text
Validation method: dotnet build (Assembly-CSharp + Assembly-CSharp-Editor), via PowerShell
Assembly-CSharp: PASS — exit code 0, 5 warnings pré-existentes (mesmos de sessões anteriores)
Assembly-CSharp-Editor: FAIL — exit code 1, NEW_FAILURE porém NÃO RELACIONADA a este bugfix
  (CS0234 em CindarsHopeMenu.cs por csproj stale faltando GenerateEnemyWalkAnimations.cs; arquivo
  existe no disco). NÃO classificado como regressão desta correção — nenhum arquivo tocado aqui
  participa do erro. Retry 1x: mesmo resultado. Residual risk: humano deve fechar o Unity Editor
  (se aberto) e reabrir/recompilar para regenerar o .csproj, ou rodar Assets > Reimport All, antes
  de confiar em validação futura do Assembly-CSharp-Editor.
Unity batchmode / EditMode Test Runner: NOT RUN (mesma limitação de ambiente headless das sessões
  anteriores)
```

### Arquivos alterados nesta correção (4ª rodada)

```
Alterados:
  Assets/_Game/Scripts/Cave/Runtime/CaveEnemyMaterializer.cs  (Rigidbody2D do inimigo: + bodyType
    Dynamic explícito + CollisionDetectionMode2D.Continuous — BUG A)
  Assets/_Game/Scripts/Cave/Runtime/CaveTileMaterializer.cs   (WallInteriorTint mais escuro; novo
    WallTopEdgeTint intermediário para borda-topo não-face; log com topo-tintado — AJUSTE B)
  docs/validation/spec_cave_biome_art_profiles_execution_report.md  (esta seção 21)
```

Nenhum arquivo `.unity`/`.prefab`/YAML editado manualmente. Nenhuma mudança em spawn
position/layout/geração procedural/snapshot/pack coordination. Sem `Random`/`FindObjectOfType`/
`GameObject.Find` novos. Sem commit. Nenhum sub-agente foi spawnado durante esta correção.

### O que o humano precisa rodar no Unity

1. **Não é necessário rodar `CindarsHope/Inicializar Projeto`** — nenhum dos dois fixes desta rodada
   mexe em `CreateMvpCaveScene` (scene creator) nem em geração de assets; ambos são código runtime
   puro (`CaveEnemyMaterializer`/`CaveTileMaterializer`), aplicado toda vez que o
   `CaveRuntimeMaterializer` materializa um nível.
2. Se o Unity Editor estiver aberto, force a recompilação (foco na janela do Editor ou
   Assets > Reimport All se necessário) antes de entrar em Play Mode, para garantir que o C# novo
   está compilado.
3. Entrar em Play Mode na `CaveScene` e confirmar: (a) inimigos (goblin/spider/rat) perseguindo o
   player não ficam parados em cima da massa de parede nem atravessam para o outro lado; (b)
   visualmente, a parede agora lê como aro fino iluminado (1 célula, wallFaceTile) + faixa
   intermediária no topo-borda + massa claramente escura no miolo — não mais "chapa clara larga".
4. Reportar qualquer caso remanescente de inimigo cruzando parede, especialmente durante
   `Leaper`/`ChargeLine`/`TreasureIdleAmbush` (moves de alta velocidade) ou perto de cantos — esses
   são os candidatos mais prováveis a ainda tunelar mesmo com Continuous (Discrete→Continuous reduz
   drasticamente mas não elimina 100% o tunelamento em cantos côncavos com múltiplos
   BoxCollider2D vizinhos); se acontecer, é um bug separado de fine-tuning, não uma regressão desta
   correção.

## 22. Arte v3 alinhada a keyart + colisao de inimigo (2026-07-04)

Direcao elevada (humano): a cave deve refletir as keyarts de art/world_gpt/raw/cave_guides/ (doc
CAVE_BIOME_VISUAL_REFERENCE secao 1.b). Terreno do bioma 1 regerado via GPT p/ bater com a keyart:
- cave_stone_floor_cobble.png -> floor_a/floor_b/floor_detail (lajota arredondada + terra, nao pedrisco fino)
- cave_stone_wall_boulder.png -> wall_face/wall_top (boulders empilhados, mais escuro que o chao)
Processados por _reseam_cave_tiles.py v3 (crop 512->128px, seamless so no chao). Minerios NAO regerados
(ja prontos em Art/Generated/World/props/rock_ore_*).
Colisao de inimigo: CaveEnemyMaterializer ganhou Rigidbody2D Dynamic + CollisionDetectionMode Continuous
(espelha o fix do player, secao 20) - inimigos deixam de tunelar/pousar sobre paredes de 1 celula.
Parede: WallInteriorTint escurecido p/ (0.22,0.21,0.20) + WallTopEdgeTint intermediario (degrade face->miolo).
Builds: Assembly-CSharp e Assembly-CSharp-Editor exit 0 (reverificado pelo orquestrador; o exit 1 do
subagent era o csproj transiente com Unity aberto).
PENDENTE (lote 2, nao feito): props do bioma (bau/carrinho/cogumelos/poca sliced das folhas-guia),
bordas de Rule Tile (transicao chao->parede arredondada), escuridao/vinheta. Reflexo "exato" da keyart
exige essas camadas - terreno agora e o piso base correto.

## 23. Lote 2 de arte — fatiamento das folhas-guia + wire de chest/hazard/exit (2026-07-04)

Continuação do PENDENTE da seção 22: fatiar `art/world_gpt/raw/cave_guides/gpt_cave_guide_{chests,hazards,fixtures}.png`
e `art/world_gpt/raw/cave_props/cave_props_decor.png` em sprites individuais e ligá-los ao
`CaveBiomeArtProfileSO` via extensão do `GenerateCaveBiomeArtProfiles`. Spawn de decor (`prop_*`,
fable_78) explicitamente fora de escopo — só deixa os 4 sprites prontos na pasta.

### 23.1 Script de corte

`art/world_gpt/_slice_cave_guides.py` (novo, idempotente). As 4 folhas têm 1254×1254px, fundo cinza
sólido uniforme (confirmado via `Read` de cada imagem antes de cortar). Ambiente local não tem
`rembg`/`onnxruntime` funcional (`ModuleNotFoundError` mesmo após `pip install rembg scipy` — falta
backend onnxruntime); em vez de puxar essa dependência pesada, reusei o **mesmo algoritmo de
fallback chroma-key por cor de borda já implementado em `postprocess_v2.remove_bg`** (mediana da
borda + `scipy.ndimage.label` para isolar região de fundo conectada), apropriado aqui porque o fundo
é comprovadamente sólido. Pipeline por célula: crop da grade → chroma-key → trim do bbox alpha →
downscale NEAREST para 128px no lado maior → salva PNG RGBA direto em
`Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/`.

Grades confirmadas por leitura visual das 4 imagens (bateram com o mapeamento do prompt, nenhum
ajuste de índice necessário):

| Folha | Grade | Células extraídas |
|---|---|---|
| `gpt_cave_guide_chests.png` | 3 col × 2 lin | (0,0)→chest_closed, (0,2)→chest_open, (1,2)→chest_false |
| `gpt_cave_guide_hazards.png` | 3 col × 2 lin | (0,0)→hazard_toxic, (0,1)→hazard_ice, (1,2)→hazard_rock |
| `gpt_cave_guide_fixtures.png` | 2 col × 3 lin | (0,0)→exit_down, (0,1)→exit_up (linha 1 = minérios, NÃO extraída — já existem) |
| `cave_props_decor.png` | 2×2 | (0,0)→prop_mine_cart, (0,1)→prop_broken_pickaxe, (1,0)→prop_planks_rail, (1,1)→prop_water_puddle |

### 23.2 Sprites gerados (evidência)

12 PNGs criados em `Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/`, todos RGBA com
`alpha_min=0, alpha_max=255` (transparência real, sem fundo cinza residual) e tamanho > 0:

```
chest_closed.png         26732 bytes  128x115
chest_open.png           22858 bytes   98x128
chest_false.png          29992 bytes  128x116
hazard_toxic.png         33798 bytes  128x125
hazard_ice.png           31615 bytes  128x125
hazard_rock.png          30548 bytes  128x121
exit_down.png            25971 bytes  128x94
exit_up.png              35397 bytes  128x125
prop_mine_cart.png       31074 bytes  128x114
prop_broken_pickaxe.png  21030 bytes  128x93
prop_planks_rail.png     31999 bytes  128x110
prop_water_puddle.png    28024 bytes  128x104
```

Verificado via `py -c "..."` (PIL, `getchannel('A').getextrema()`) — não recriados `rock_ore_*`,
`mushroom_cluster.png`, `log_fallen.png` (já existiam em `Art/Generated/World/props` e
`Art/Generated/World/foliage`, checados por `Glob` antes de qualquer geração).

### 23.3 Campos wired no CaveBiomeArtProfileSO

- **`CaveBiomeArtProfileSO.cs`**: 3 novos setters editor-only, mesmo padrão dos já existentes
  (`EditorSetFloorTiles`/`EditorSetWallTiles`): `EditorSetHazardSprites(toxicPool, iceSlick,
  fallingRock)`, `EditorSetChestSprites(closed, open, falseRevealed)`, `EditorSetExitSprites(exitDown,
  exitUp)`. Nenhum campo novo — os 8 campos de `Sprite` já existiam na SO desde CV01 (T007), só
  estavam sem setter/populador.
- **`GenerateCaveBiomeArtProfiles.PopulateFromConvention`**: estendido para, além dos 5 tiles, ler
  (via `AssetDatabase.LoadAssetAtPath<Sprite>`, mesmo `LoadSpriteIfExists` já usado) os 8 arquivos
  `chest_closed/chest_open/chest_false/hazard_toxic/hazard_ice/hazard_rock/exit_down/exit_up.png` da
  pasta `Art/Generated/World/cave/<biomeId>/` e atribuí-los direto como `Sprite` (sem criar `Tile`
  asset — são objetos, não tiles de Tilemap). Ausente na pasta = `null`, sem erro (mesma política
  do resto do gerador). `_trapSprites` (mapa trapId→Sprite) **não** foi populado neste lote — não há
  convenção de nome-por-trapId definida ainda; fica para um lote futuro se necessário.
- **`CaveBiomeArtResolver`**: já tinha `TryGetChestSprite`/`TryGetHazardSprite`/`TryGetExitSprite`
  (T007) e os materializers (`TreasureChestInteractable`, `FalseChestTrap`,
  `CaveHazardMaterializer`/`CaveHazardTile`, `CaveExitMaterializer`) já consultavam o resolver com
  fallback ao placeholder — **nenhuma mudança de código de materializer foi necessária**; popular o
  profile é suficiente para a arte aparecer.
- **`ValidateCaveBiomeArtProfiles`**: novo método `CollectEmptyObjectSpriteWarnings` emite 1 WARNING
  granular por campo de sprite de objeto vazio (ex.: `"Profile 'X': ChestClosedSprite vazio
  (chest_closed.png ausente)"`), chamado por profile dentro do loop de validação existente. Mantém a
  severidade WARNING-nunca-ERROR já estabelecida (`IsVisuallyEmpty` não mudou).
- **`GeneratedSpriteImporter.cs`**: o carve-out de tile de cave (PPU 128 + pivot `Center`) estava
  restrito só por **prefixo de pasta** (`Assets/_Game/Art/Generated/World/cave/`), o que faria os 12
  sprites de objeto deste lote herdarem incorretamente pivot Center/PPU de tile por estarem na mesma
  pasta. Corrigido para exigir também que o **nome do arquivo** seja um dos 5 nomes de tile
  (`floor_a.png`, `floor_b.png`, `floor_detail.png`, `wall_face.png`, `wall_top.png`) — `chest_*`,
  `hazard_*`, `exit_*`, `prop_*` agora caem no default (`Ppu = 128f`, pivot `BottomCenter`), o
  comportamento correto de sprite de objeto. **Atenção:** o Unity Editor estava aberto durante esta
  tarefa e auto-importou os 12 PNGs com a versão ANTIGA do importer antes da correção ser recompilada
  (`.meta` de `chest_closed.png` confirma `alignment: 7`/Center) — ver ACHADO e passo de force-reimport
  na seção "O que o humano precisa rodar no Unity" abaixo.

### 23.4 O que fica para o spawner de decor (fable_78, fora de escopo)

Os 4 `prop_*.png` (mine_cart, broken_pickaxe, planks_rail, water_puddle) estão prontos na pasta
(`biome_stone_cavern/`, RGBA, trim+downscale feitos) mas **não são lidos** por
`GenerateCaveBiomeArtProfiles` nem têm campo correspondente em `CaveBiomeArtProfileSO` —
`CaveBiomeArtProfileSO` não tem (e esta tarefa não adicionou) um array de decor solto. Quando
fable_78 (spawn de decor) for implementado, ele pode consumir esses 4 PNGs diretamente por convenção
de nome/pasta sem precisar de nova geração de arte.

### 23.5 Validação

```text
Validation method: dotnet build (Assembly-CSharp + Assembly-CSharp-Editor), via PowerShell
Assembly-CSharp: PASS — exit code 0 (5 warnings pré-existentes, não relacionados)
Assembly-CSharp-Editor: PASS — exit code 0 (7 warnings pré-existentes, não relacionados)
CS0234 transiente (CindarsHope.Editor.Enemy, mencionado na seção 21.4): NÃO reproduzido nesta
  rodada — ambos os builds passaram limpo na primeira tentativa, sem necessidade de rebuild.
Unity batchmode / EditMode Test Runner: NOT RUN (mesma limitação de ambiente headless das sessões
  anteriores)
Sprite file evidence: 12/12 PNGs verificados no disco (tamanho > 0, modo RGBA, alpha_min=0/alpha_max=255)
```

### Arquivos alterados/criados nesta seção

```
Criados:
  art/world_gpt/_slice_cave_guides.py
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/chest_closed.png (+ .meta gerado pelo Unity)
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/chest_open.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/chest_false.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/hazard_toxic.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/hazard_ice.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/hazard_rock.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/exit_down.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/exit_up.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/prop_mine_cart.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/prop_broken_pickaxe.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/prop_planks_rail.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/prop_water_puddle.png

Alterados:
  Assets/_Game/Scripts/Cave/Art/CaveBiomeArtProfileSO.cs        (+3 setters editor-only)
  Assets/_Game/Scripts/Editor/Cave/GenerateCaveBiomeArtProfiles.cs (le+popula chest/hazard/exit)
  Assets/_Game/Scripts/Editor/Cave/ValidateCaveBiomeArtProfiles.cs (+WARNING granular por campo vazio)
  Assets/_Game/Scripts/Editor/GeneratedSpriteImporter.cs         (carve-out de tile restrito a 5 nomes)
  docs/validation/spec_cave_biome_art_profiles_execution_report.md (esta seção 23)
```

Nenhum arquivo `.unity`/`.prefab` editado manualmente. Nenhum `Random`/`FindObjectOfType` novo. Sem
spawn de decor (fable_78 fora de escopo — só os 4 PNGs prontos). Sem commit. Nenhum sub-agente foi
spawnado durante esta tarefa.

### ACHADO durante esta tarefa: Unity Editor já estava aberto e auto-importou os 12 PNGs

Confirmado via `Get-Process -Name Unity` que 3 processos Unity estavam rodando enquanto os PNGs
foram criados no disco. O Editor auto-importou os 12 arquivos assim que apareceram, ANTES da
correção do `GeneratedSpriteImporter.cs` ter sido recompilada pelo Editor — evidência: o `.meta`
de `chest_closed.png` já existe com `alignment: 7` (= `SpriteAlignment.Center`) e
`spritePixelsToUnits: 128`, ou seja, foi importado com o carve-out ANTIGO (só prefixo de pasta, sem
checar nome de arquivo), o mesmo bug que esta tarefa corrigiu. Isso não é um erro desta tarefa — é
o auto-import do Editor correndo em paralelo à edição de código, fora do meu controle (não abri/rodei
Unity). **Ação obrigatória do humano antes de confiar na arte em Play Mode:**

### O que o humano precisa rodar no Unity

1. **Forçar reimport dos 12 PNGs** (não basta rodar Inicializar Projeto — reimport só dispara para
   textura já existente se o Unity perceber que precisa; mais seguro fazer explícito): selecionar a
   pasta `Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/` no Project window → botão direito
   → Reimport. Confirmar depois que `chest_closed.png` (Inspector → Sprite) mostra `Pivot: Bottom` em
   vez de `Center`.
2. Rodar `CindarsHope/Inicializar Projeto` **1 vez** — repopula os 8 `CaveBiomeArtProfileSO` (o bioma 1
   ganha os 8 campos de sprite de objeto preenchidos; os outros 7 biomas continuam vazios/WARNING,
   como esperado antes de gerar arte deles).
3. Rodar `CindarsHope/Validar Projeto` — Console deve mostrar os novos WARNINGs granulares só para
   os biomas 2–8 (ex.: `"Profile '...': ChestClosedSprite vazio..."`); o bioma 1
   (`biome_stone_cavern`) não deve gerar mais nenhum desses 8 WARNINGs específicos.
4. Entrar em Play Mode na `CaveScene`: baú de tesouro deve renderizar com a arte nova
   (fechado/aberto/mimic revelado), hazards de tile (poça tóxica/gelo/queda de rocha) e as duas
   saídas (escada descida/subida) devem mostrar sprite em vez do placeholder de cor sólida, com pivot
   correto (base do sprite alinhada ao chão, não flutuando/deslocado por causa do pivot Center errado).
5. `prop_*` (carrinho, picareta quebrada, tábuas+trilho, poça d'água) **não aparecem em lugar
   nenhum** ainda — são decor não-wired, aguardando o spawner do fable_78.

## 24. Lote 3 — chunks/bordas fatiados + sombra de borda chão↔parede (2026-07-04)

Continuação do lote 2 (seção 23): duas tarefas independentes.

### 24.1 Fatiamento de 2 novas folhas-guia

Adicionadas ao MANIFEST de `art/world_gpt/_slice_cave_guides.py` (idempotente, mesmo pipeline
chroma-key da seção 23.1): `cave_stone_chunks.png` (2×2 — rubble/ore mound/cogumelos
gigantes/estalactites) e `cave_stone_wall_edges.png` (2×2 — borda topo/lateral/2 cantos). Ambas
1254×1254px, fundo cinza uniforme confirmado via `Read` de cada folha antes do corte; grade 2×2
bateu com o mapeamento pedido, sem ajuste de índice.

**ACHADO e correção no script (afeta as 2 folhas novas, não regride as 12 anteriores):**

1. **Bug do `getbbox()` em RGBA**: `Image.getbbox()` considera "conteúdo" qualquer pixel com **algum
   canal != 0** — em RGBA isso inclui pixels de fundo com `alpha=0` mas RGB de cor não-preta (o cinza
   do fundo). Nas 12 saídas do lote 2 isso não se manifestou (coincidência de proporção), mas em
   `wall_edge_corner_a/b.png` o objeto ocupa quase toda a célula e o bbox "errado" cortava fora
   justamente a fina franja que ainda era transparente, produzindo um PNG 128×128 **100% opaco** (sem
   trim, fundo cinza visível). Corrigido em `process_cell` para calcular o bbox a partir do **canal
   alpha isolado** (`cut.split()[-1].getbbox()`), que é a extensão correta do objeto real.
2. **Tolerância de chroma-key insuficiente só em `cave_stone_wall_edges.png`**: mesmo com o bbox
   corrigido, as 4 células dessa folha continuavam 100% opacas — a sombra de solo (dust) sob a barra/
   curva de pedra tem um gradiente suave que quebra a conectividade 4-direcional da máscara de
   "distância de cor" com `CHROMA_TOL=42` (o `scipy.ndimage.label` via flood-fill não conseguia ligar
   o fundo distante da sombra ao label que toca a borda — ficavam como ilhas isoladas, tratadas como
   "objeto"). Testado visualmente `tol=70/85/100`: `tol=100` remove o fundo cinza corretamente nas 4
   células desta folha sem comer a pedra/sombra (`debug_tol100_top.png`/`debug_tol100_corner_a.png`,
   removidos após a verificação). **`tol=100` NÃO é seguro globalmente** — testado contra
   `prop_mine_cart.png` (lote 2) e reproduziu uma regressão real: os highlights claros do metal/madeira
   ficam "comidos" nas bordas (`debug_tol100_minecart.png`). Por isso `process_sheet`/`process_cell`
   ganharam um parâmetro `tol` opcional (default `CHROMA_TOL=42`), e só a chamada de
   `cave_stone_wall_edges.png` passa `tol=100`; as outras 5 chamadas (incluindo `cave_stone_chunks.png`)
   continuam no default 42, idênticas ao lote 2.

Reverificado depois do fix: as 12 saídas do lote 2 mantiveram **exatamente as mesmas dimensões** de
antes (sem regressão), e as 8 novas saem com alpha real (não mais 0%/100% opaco).

### 24.2 Sprites gerados (evidência)

8 PNGs criados em `Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/`, todos RGBA com alpha
real (verificado via `PIL`, contagem de pixels `alpha==0` > 0 e < 100% em todos):

```
chunk_rubble.png            128x116   alpha0=5180  (34.9%)
chunk_ore_mound.png         128x119   alpha0=5257  (34.5%)
chunk_mushrooms_giant.png   107x128   alpha0=3892  (28.4%)
chunk_stalactites.png       125x128   alpha0=6700  (41.9%)
wall_edge_top.png           128x128   alpha0=12309 (75.1%)
wall_edge_side.png          128x128   alpha0=12325 (75.2%)
wall_edge_corner_a.png      128x128   alpha0=12097 (73.8%)
wall_edge_corner_b.png      128x128   alpha0=12120 (74.0%)
```

**Explicitamente NÃO feito (fora de escopo desta tarefa, conforme instrução):**
- `chunk_*` **não** foram wired a nenhum campo de `CaveBiomeArtProfileSO` — não existe campo de
  array de decor/chunk no profile hoje; ficam prontos no disco para o spawn de chunks via CV02 (spec
  futura), mesmo padrão de "pronto mas não consumido" já usado para os `prop_*` do lote 2 (fable_78).
- `wall_edge_*` **não** foram montados em Rule Tile nem em nenhum autotile funcional — ficam só como
  os 4 sprites soltos no disco. Montar a transição de borda arredondada chão/parede via Rule Tile é
  follow-up (fora de escopo desta tarefa).
- Nenhum dos 8 cai no carve-out de tile do `GeneratedSpriteImporter` (restrito aos 5 nomes exatos
  `floor_*`/`wall_*` de tile — `chunk_*` e `wall_edge_*` não estão nessa lista), então importam com o
  default de sprite de objeto (Ppu 128, pivot BottomCenter), como pedido.

### 24.3 Sombra de borda chão↔parede (CaveTileMaterializer)

`Assets/_Game/Scripts/Cave/Runtime/CaveTileMaterializer.cs`, `MaterializeFloor`: quando
`hasFloorTiles` e uma célula de chão é pintada no `FloorTilemap`, agora checa
`IsFloorEdgeNextToWall(tilePos, level)` — método estático puro novo (espelho de `IsWallInterior` já
existente, mesma convenção testável em EditMode) que verifica se algum dos 4 vizinhos ortogonais está
em `level.WallTiles`. Se sim, aplica `SetTileFlags(pos, TileFlags.None)` +
`SetColor(pos, FloorEdgeShadowTint)` — const nova `new Color(0.62f, 0.6f, 0.58f)` (chão levemente
escurecido), mesmo padrão de apresentação do `WallInteriorTint`/`WallTopEdgeTint` já existentes, só
que do lado do chão. Célula de chão longe de parede (nenhum vizinho ortogonal é `WallTiles`) fica com
cor cheia (branco/sem tint), comportamento inalterado. Objetivo: profundidade/aro escuro onde o chão
encontra a parede, sem asset novo — complementa o rim que fica para depois (mencionado na seção 22).

O log one-shot de chão (`LogFloorPaintStatusOnce`) foi estendido para incluir a contagem de células
de borda sombreadas: `"... ({cellCount} celula(s) walkable; borda-sombreada={edgeShadowCount})."`.

**Testes EditMode** (`Assets/_Game/Tests/EditMode/Cave/CaveBiomeArtProfilesTests.cs`, mesma
convenção/arquivo dos testes de `IsWallInterior` já existentes): 5 novos testes cobrindo
`IsFloorEdgeNextToWall` — parede ao sul/norte/leste/oeste conta como borda; sem vizinho-parede
ortogonal não é borda; vizinho-parede só na diagonal não conta como borda. Lógica pura, sem
dependência de Unity runtime além de `Vector2Int`/`CaveGeneratedLevel` (mesmo padrão dos testes
irmãos de `IsWallInterior`).

**Pendente (não feito nesta tarefa, fora de escopo):** Rule Tile de autotile funcional para as bordas
`wall_edge_*` fatiadas na seção 24.1 (ficam só como sprites soltos); spawn de `chunk_*` como decor via
CV02.

### 24.4 Validação

```text
Validation method: dotnet build (Assembly-CSharp + Assembly-CSharp-Editor), via PowerShell
Assembly-CSharp: PASS — exit code 0 (mesmos 5 warnings pré-existentes, não relacionados)
Assembly-CSharp-Editor: PASS — exit code 0 (mesmos 7 warnings pré-existentes, não relacionados)
CS0234 transiente: NÃO reproduzido nesta rodada — ambos os builds passaram limpo na primeira tentativa.
EditMode Test Runner (Unity batchmode): NOT RUN — mesma limitação de ambiente headless das sessões
  anteriores; os 5 testes novos de IsFloorEdgeNextToWall foram adicionados ao arquivo existente e
  seguem a mesma convenção dos testes de IsWallInterior já cobertos por essa suite, mas a execução
  real do Test Runner requer o Editor.
Sprite file evidence: 8/8 PNGs verificados no disco (tamanho > 0, modo RGBA, alpha0 > 0 e < 100% em
  todos — sem regressão nas 12 saídas do lote 2, mesmas dimensões de antes)
Slicer script: py _slice_cave_guides.py, exit code 0 (rodado 3x durante a investigação/fix; saída
  final idempotente)
```

### Arquivos alterados/criados nesta seção

```
Criados:
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/chunk_rubble.png (+ .meta gerado pelo Unity)
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/chunk_ore_mound.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/chunk_mushrooms_giant.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/chunk_stalactites.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/wall_edge_top.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/wall_edge_side.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/wall_edge_corner_a.png
  Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/wall_edge_corner_b.png

Alterados:
  art/world_gpt/_slice_cave_guides.py (MANIFEST +2 folhas; fix bbox-por-alpha; tol por-folha)
  Assets/_Game/Scripts/Cave/Runtime/CaveTileMaterializer.cs (+FloorEdgeShadowTint, +IsFloorEdgeNextToWall,
    aplicação no loop de MaterializeFloor, log estendido com contagem de borda)
  Assets/_Game/Tests/EditMode/Cave/CaveBiomeArtProfilesTests.cs (+5 testes de IsFloorEdgeNextToWall)
  docs/validation/spec_cave_biome_art_profiles_execution_report.md (esta seção 24)
```

Nenhum arquivo `.unity`/`.prefab` editado manualmente. Nenhum `Random`/`FindObjectOfType` novo. Sem
commit. Nenhum sub-agente foi spawnado durante esta tarefa.

### Inspector wiring required (human action in Unity Editor)

Nenhum — a sombra de borda é 100% runtime (`CaveTileMaterializer` já é invocado pelo
`CaveRuntimeMaterializer` existente, sem novo campo de Inspector). O único wiring pendente é de arte
(reimport de sprite), coberto abaixo.

### O que o humano precisa rodar no Unity

1. Rodar `CindarsHope/Inicializar Projeto` **1 vez** — reimporta os 8 PNGs novos desta seção (e
   também aplica o fix de cor de fundo da câmera já mencionado em seções anteriores, se ainda
   pendente). Confirmar no Project window que `chunk_*`/`wall_edge_*` importaram com pivot
   `BottomCenter` (não `Center`) — não deveriam cair no carve-out de tile (nomes não batem com os 5
   nomes de tile), mas vale conferir dado o histórico de auto-import concorrente da seção 23.
2. A sombra de borda chão↔parede é 100% runtime — só aparece em **Play Mode** na `CaveScene` (não em
   Edit Mode, pois o `FloorTilemap` é materializado por `CaveRuntimeMaterializer` em tempo de
   execução). Confirmar visualmente: células de chão encostadas em parede devem ler levemente mais
   escuras que o miolo do chão, dando uma leve sensação de profundidade/aro na transição chão→parede.
3. `chunk_*` e `wall_edge_*` **não aparecem em lugar nenhum** ainda — não há spawn/autotile
   consumindo esses 8 sprites; são arte pronta aguardando CV02 (chunks) e um follow-up de Rule Tile
   (bordas).
