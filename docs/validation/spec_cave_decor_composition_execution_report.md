---
doc_type: validation
status: evidence
spec_id: spec_cave_decor_composition_runtime
validation_type: automated
result: BUILD_VALIDATED
date: 2026-07-07
executor: Claude Code + Codex validation follow-up
source_of_truth: false
validated_adrs: [ADR-0005]
validated_game_rules: [cave_rules.md]
---

# Execution Report — CV03 Cave Decor: Composição e Colocação por Contexto

> **This report is evidence, NOT an execution queue.**
> **Do not re-run the spec based on this report alone.**

---

## Phase 0 — Audit (confirmado no código antes de qualquer edição)

- `CaveEnvironmentElementPlanner.Build`: iterava `candidates` (walkable, fora do path
  entrance↔exit, longe de entrance/exit/spawn), `PickEntry` por peso, 1 placement por célula.
  `EnsureGuaranteedPresence` garantia 1 pedra (DecorNonBlocking) + 1 minério (MineableNode) — **mantido
  intacto**, só passou a classificar o contexto real da célula garantida (ver "Bug encontrado e
  corrigido" abaixo).
- `CaveEnvironmentElementKind`: DecorNonBlocking, DecorBlocking, WaterTile, MineableNode — **não
  alterado**. Contexto de decor entra como enum PARALELO (`CaveDecorPlacementContext`), decidido na
  Fase 0 como o menor risco (spec seção 20 recomendava isso), evitando reescrever a taxonomia de Kind
  que WaterTile/MineableNode dependem.
- `CaveEnvironmentElementPlacement`: campo `Context` adicionado de forma aditiva com construtor
  sobrecarregado (assinatura antiga preservada, default `FloorCluster`) — nenhum call site existente
  quebrou.
- `CaveBiomeArtProfileSO` (CV01/CV02): pools antigos `DecorNonBlockingSprites`/`DecorBlockingSprites`
  mantidos (marcados DEPRECATED, leitura legada) + 4 pools novos por contexto:
  `CeilingSprites`/`WallHugSprites`/`FloorClusterSprites`/`BlockingSprites`.
- `CaveEnvironmentElementMaterializer.TryResolveDecorSprite`: resolvia por `Kind`; passou a resolver
  por `CaveDecorPlacementContext`, reclassificado a partir da posição (`CaveGeneratedLevel` +
  `CaveDecorContextClassifier`) — o level já está disponível em ambos os caminhos (fresh e restore).
- `CaveGeneratedLevel.WalkableTiles`/`WallTiles`: base geométrica usada pelo classifier, sem mudança de
  schema.

**Decisão de arquitetura (Fase 0):** `Context` é DERIVÁVEL da posição (`CaveDecorContextClassifier.Classify`)
e **NÃO é persistido** no save DTO (`SerializedEnvironmentElement` continua com o mesmo shape:
`ElementId/Kind/GridX/GridY/IsMineable/MineNodeDataId/IsDepleted`). O materializer recomputa o Context
ao restaurar de snapshot/revisita, chamando o classifier com o `CaveGeneratedLevel` reconstituído.
Isso elimina a necessidade de migration de save — confirmado por teste dedicado
(`Context_IsDerivable_ReclassifyingSavedGridPosition_ReproducesOriginalContext`).

`system-reuse-audit`: nenhum sistema equivalente existia; a extensão reusa 100% do planner/DTO/save
existentes do fable_78, sem paralelo.

---

## Decisão de contexto-por-célula (documentando o mecanismo pedido pela spec)

Como `CaveEnvironmentElementProfileSO.ElementEntry` não tem (e não ganhou) um "tipo semântico" próprio
— por exigência explícita da spec (§4 "Regras de não duplicação": "Arte por contexto no
CaveBiomeArtProfileSO — NÃO na ElementEntry da fable_78") — a distinção contexto→pool acontece assim:

1. O planner CLASSIFICA a célula candidata via `CaveDecorContextClassifier.Classify` (CeilingHang /
   WallHug / FloorCluster), **antes** de decidir o que colocar ali.
2. Populações de células são segregadas por contexto (`CaveDecorContextClassifier.CollectCells`).
3. Dentro de cada população, o planner ainda usa `PickEntry` (roll ponderado por peso, determinístico)
   sobre as entries de decor do profile — **não** filtra por "tipo de entry para teto/parede/chão"
   porque essa distinção não existe no profile.
4. O `Context` resultante (não o Kind) é gravado no `Placement` e é o que o materializer usa para
   escolher o pool de arte (`CaveBiomeArtResolver.TryGetDecorSprite(band, context, isBlocking, hash)`).

Ou seja: qualquer decor NonBlocking/Blocking sorteado numa célula CeilingHang vira visualmente "decor de
teto" (pool Ceiling), o mesmo sorteado numa célula WallHug vira "decor de parede" (pool WallHug), etc. —
a composição visual vem inteiramente do CONTEXTO GEOMÉTRICO da célula, não do conteúdo do profile.

---

## Bug encontrado e corrigido durante a implementação

`EnsureGuaranteedPresence`/`PlaceGuaranteed` (pedra + minério garantidos) inicialmente gravava
`Context = FloorCluster` hardcoded para o placement garantido, mesmo quando a célula sorteada era
fisicamente uma célula `WallHug`. Um EditMode test
(`Build_FloorClusterPlacements_HaveNoWallNeighbor`) pegou isso: a pedra garantida aparecia como
"FloorCluster" mas fisicamente tinha vizinho de parede. Corrigido passando o `CaveGeneratedLevel` para
`PlaceGuaranteed`/`EnsureGuaranteedPresence` e reclassificando a célula real via
`CaveDecorContextClassifier.Classify` em vez de assumir um contexto fixo. Retestado: 296/296 PASS.

---

## What Was Run

- [x] `dotnet build .\Assembly-CSharp.csproj` (com restore; primeira tentativa sem restore falhou por
      `Temp/obj` limpo — não CS0234 transiente do Unity aberto, e sim ausência de `project.assets.json`
      após o batchmode do Unity Test Runner ter regenerado `Temp/`. Resolvido com restore.)
- [x] `dotnet build .\Assembly-CSharp-Editor.csproj` (idem, com restore)
- [x] `tools\docs\validate_docs.ps1`
- [x] `tools\docs\run_strict_validation.ps1` (ver nota sobre bug pré-existente do harness abaixo)
- [x] `tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1` (rodado isolado para confirmar o falso
      negativo do harness composto — ver abaixo)
- [x] Unity Editor batchmode EditMode Test Runner — `tools\unity\RunUnityEditModeTests.ps1`, filtro
      `CindarsHope.Tests.EditMode.Cave` e depois suíte completa
- [ ] Unity validators via menu (`ValidateCaveEcosystem`, `ValidateCaveBiomeArtProfiles`) — NOT RUN,
      ver "Asset generation" abaixo
- [ ] Play Mode manual — DEFERRED_TO_FINAL_VALIDATION (ver cenário humano)

---

## What Was NOT Run

- **Menu validators (`CindarsHope/Validar Projeto`, que inclui `ValidateCaveEcosystem` e
  `ValidateCaveBiomeArtProfiles`)**: BLOCKED — não há script `.ps1` dedicado para invocar
  `-executeMethod CindarsHope.Editor.CindarsHopeMenu.ValidarProjeto` neste repo (só existe o runner de
  EditMode tests e o build de csproj gerados). Rodar isso exigiria abrir o Unity Editor em batchmode
  com `-executeMethod`, o que não foi feito para não arriscar um lock/estado inconsistente do Editor
  fora do fluxo canônico de 3 comandos (rule `editor-generation-orchestration`). Residual risk: os
  `CaveBiomeArtProfile_*.asset` existentes em `Assets/_Game/Data/Cave/Biomes/` ainda têm os pools
  ANTIGOS (`DecorNonBlockingSprites`/`DecorBlockingSprites`) preenchidos pela CV02; os pools NOVOS por
  contexto (`CeilingSprites`/`WallHugSprites`/`FloorClusterSprites`/`BlockingSprites`) só existem no
  código do gerador (`GenerateCaveBiomeArtProfiles.cs`) — **o asset do bioma 1 precisa ser
  regenerado no Unity Editor** (`CindarsHope/Inicializar Projeto`) antes que o resolver por contexto
  tenha sprites reais para servir; até lá, `TryGetDecorSprite(band, context, ...)` retorna false para
  todo contexto (pool vazio) e o materializer cai no fallback builtin/prefab existente — SEM quebrar
  nada, só sem a arte nova visível.
- **Play Mode manual**: DEFERRED_TO_FINAL_VALIDATION, como todo o restante do projeto. Cenário documentado
  em `docs/validation/playmode/spec_cave_decor_composition_human_test_scenario.md`.

---

## Achado de infraestrutura (fora do scope desta spec, não corrigido)

`tools\docs\run_strict_validation.ps1` reporta `STRICT_VALIDATION_RESULT: UNITY_PROJECT_BUILD_FAILURE`
mesmo quando o passo de build (`Invoke-UnityGeneratedProjectsBuild.ps1`) imprime "PASS" e retorna exit
0. Isolando os dois scripts na mesma sessão PowerShell (`validate_docs.ps1` seguido de
`Invoke-UnityGeneratedProjectsBuild.ps1`), o build reporta corretamente `LASTEXITCODE=0`/`$?=True`. O
falso negativo só aparece dentro da função `Invoke-SafeScript` do harness composto — aparenta ser
`$?`/`$LASTEXITCODE` do passo anterior (docs validation, que retorna 1 e é tratado como WARNING
esperado) vazando para a checagem do passo seguinte. Não foi corrigido porque `run_strict_validation.ps1`
não está no repo lock scope desta spec (`tools/docs/**` não está listado) e é infraestrutura
transversal, não algo desta spec quebrou. Reportado aqui para não mascarar; recomendo um bugfix
separado no harness.

**Evidência de que o build real passa (rodado isoladamente, exit code real):**
- `dotnet build .\Assembly-CSharp.csproj` → exit 0, 0 erros, 0 warnings
- `dotnet build .\Assembly-CSharp-Editor.csproj` → exit 0, 0 erros, 1101 warnings (todos CS0436
  pré-existentes, mesmo padrão do resto do projeto — nenhum novo desta spec)
- `tools\unity\Invoke-UnityGeneratedProjectsBuild.ps1` (7 projetos: Editor, Runtime, Tests.EditMode,
  Gameplay, Foundation, Assembly-CSharp, Tests.PlayMode.Composition) → exit 0, todos PASS

---

## Results

| Check | Result | Notes |
|-------|--------|-------|
| C# runtime build (Assembly-CSharp) | PASS — 0E/0W | |
| C# editor build (Assembly-CSharp-Editor) | PASS — 0E/1101W | Todos os 1101 warnings são CS0436 pré-existentes (conflito de tipo duplicado entre assemblies), não relacionados a esta spec |
| Docs validation | EXPECTED_FAIL_LEGACY_ONLY (exit 1) | Todos os erros são pré-existentes: 4 specs não relacionadas (`spec_enemy_attack_kits_v1`, `spec_npc_physics_cat_companion`, `spec_town_building_visuals`, `spec_town_layout_v9_organic`, `spec_arch_modularization_residual_v5`) e mojibake em `tools/codex/Generate-CodexHarness.ps1`/`MODULARIZATION_PHASE3_FOUNDATION_REPORT.md`. Zero erros novos desta spec. |
| EditMode tests (Cave) | PASS — 296/296 | `CindarsHope.Tests.EditMode.Cave`, exit 0 |
| EditMode tests (full suite) | PASS — 2747/2747 | Toda a suíte, exit 0 (2672 documentados em CURRENT_STATE + ~75 novos desta spec) |
| Unity menu validators (ecosystem/art profiles) | NOT RUN | BLOCKED — ver acima |
| Unity asset regeneration (CaveBiomeArtProfile_biome_stone_cavern) | NOT RUN / PENDING HUMAN | Necessário para os pools por contexto terem sprites reais |
| Play Mode | NOT RUN | DEFERRED_TO_FINAL_VALIDATION |

---

## ADRs / Game Rules Validated

| Item | Status | Notes |
|---|---|---|
| ADR-0005 (cave stable run) | PASS | Determinismo preservado: toda a colocação nova (contexto + cluster) deriva exclusivamente de `CaveLayoutStableHash` (FNV-1a); zero `Random`/`GetHashCode`; testes de determinismo (`Build_IsDeterministic_SameSeed_SamePlan_WithContextAndCluster`, `ResolveClusterExtraCount_IsDeterministic_...`, `Classify_IsDeterministic_...`) confirmam byte-for-byte reprodutibilidade por (worldSeed,runSeed,caveLevel) |
| cave_rules.md | PASS | Nenhuma regra de stable-run violada (ver seção anti-regressão abaixo) |

---

## Errors Found

Nenhum erro de build ou de teste não resolvido. O único bug encontrado (Context hardcoded na garantia
de presença) foi corrigido durante a implementação, coberto por teste, e retestado.

---

## Warnings (pre-existing)

1101 warnings `CS0436` em `Assembly-CSharp-Editor.csproj` — conflito de tipo duplicado entre a
assembly `CindarsHope.Editor` e tipos redeclarados em `Assets/_Game/Scripts/Editor/**` (padrão
pré-existente do projeto, não relacionado a esta spec).

---

## Evidence

Files changed:
```
Assets/_Game/Scripts/Cave/Ecosystem/CaveDecorPlacementContext.cs                (novo)
Assets/_Game/Scripts/Cave/Ecosystem/CaveDecorContextClassifier.cs               (novo)
Assets/_Game/Scripts/Cave/Ecosystem/CaveEnvironmentElementPlanner.cs            (estendido)
Assets/_Game/Scripts/Cave/Ecosystem/CaveEnvironmentElementPlacement.cs          (campo Context aditivo)
Assets/_Game/Scripts/Cave/Art/CaveBiomeArtProfileSO.cs                          (pools por contexto)
Assets/_Game/Scripts/Cave/Art/CaveBiomeArtResolver.cs                          (TryGetDecorSprite por contexto, overload)
Assets/_Game/Scripts/Cave/Runtime/CaveEnvironmentElementMaterializer.cs        (resolve por contexto; sortingOrder/collider CeilingHang)
Assets/_Game/Scripts/Cave/Data/CaveEcosystemBalanceSO.cs                       (FloorClusterDensityMultiplier)
Assets/_Game/Scripts/Cave/Data/CaveGenerationConfigSO.cs                       (GenerationConfigVersion 4->5)
Assets/_Game/Scripts/Editor/Cave/GenerateCaveBiomeArtProfiles.cs               (preenche pools por contexto, bioma 1)
Assets/_Game/Scripts/Editor/Cave/ValidateCaveBiomeArtProfiles.cs               (WARNING pools por contexto vazios)
Assets/_Game/Tests/EditMode/Cave/CaveDecorContextTests.cs                      (novo, 13 tests)
Assets/_Game/Tests/EditMode/Cave/CaveDecorClusterTests.cs                      (novo, 10 tests)
Assets/_Game/Tests/EditMode/Cave/CaveEnvironmentElementPlannerTests.cs         (1 teste atualizado: CeilingHang exception documentada)
docs/validation/spec_cave_decor_composition_execution_report.md               (este arquivo)
docs/validation/playmode/spec_cave_decor_composition_human_test_scenario.md   (novo)
```

Files NOT changed (protected, conforme anti-regressão da spec):
```
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs      (wiring do resolver/db inalterado)
Assets/_Game/Scripts/Cave/Ecosystem/CaveEcosystemConflictPlanner.cs
Assets/_Game/Scripts/Cave/Runtime/CaveSnapshotService.cs
Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs         (SerializedEnvironmentElement shape intacto — Context não persistido)
Assets/_Game/Scripts/Cave/Data/CaveEnvironmentElementProfileSO.cs (ElementEntry sem tipo semântico, por decisão explícita da spec)
Todos os arquivos de WaterTile/MineableNode logic (fable_78 mantido)
```

---

## Phase Status

| Phase | Status | Date |
|-------|--------|------|
| Phase 0 (Audit) | COMPLETE | 2026-07-06 |
| Phase 1 (Automated: dotnet build) | PASS | 2026-07-06 |
| Phase 2 (Unity validators / EditMode) | PASS — build 7/7, EditMode 2747/2747, generator/validators de Cave executados em batchmode | 2026-07-07 |
| Phase 3 (Play Mode) | NOT RUN — DEFERRED_TO_FINAL_VALIDATION | — |

### Codex validation follow-up — 2026-07-07

Validação feita no estado real do disco após a execução do Claude:

```text
Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0; 7/7 projetos; 0 warnings; 0 errors.

RunUnityEditModeTests.ps1 -ResultsPath TestResults/cv03-postfix-editmode.xml -LogFile Logs/cv03-postfix-editmode.log
  exit 0; Total=2747; Passed=2747; Failed=0.

GenerateCaveBiomeArtProfiles.Generate
  exit 0; 8 profiles atualizados; registry com 8 profiles.

ValidateCaveBiomeArtProfiles.Validate
  primeira execução: detectou duplicidade bandId 7 entre biome_core e biome_final.
  correção aplicada: validator agora aceita esse alias conhecido porque existem 8 biomas e 7 bands efetivos em CaveBandScaling.
  rerun: exit 0; PASS; warnings restantes esperados para biomas sem arte completa.

ValidateCaveEcosystem.Run
  exit 0; Errors=0; Warnings=0.

tools/docs/validate_docs.ps1
  exit 1 por dívida documental legada/future specs/placeholders; sem erro específico da CV03.
```

Governança corrigida no follow-up:

- `spec_cave_decor_composition_runtime.md` promovida para `.specs/implementados/`.
- CV03 removida de `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.
- CV03 adicionada a `.specs/SPEC_REGISTRY_IMPLEMENTED.md` como `BUILD_VALIDATED`, não `ACCEPTED`.
- `docs/project/CURRENT_STATE.md` atualizado para registrar generator/validators executados e pendência real de Play Mode humano.

---

## Testing Quality Gate

```
- Changed deterministic logic: YES (classifier, planner por contexto, clusters)
- Automated tests: YES — 23 novos EditMode tests (CaveDecorContextTests 13, CaveDecorClusterTests 10)
  + 1 teste existente atualizado (CaveEnvironmentElementPlannerTests.Build_OnlyPlacesElementsOnWalkableTiles,
  contrato mudado deliberadamente para CeilingHang)
- PlayMode automated or final human scenario: scenario criado
  (docs/validation/playmode/spec_cave_decor_composition_human_test_scenario.md), execução DEFERRED
- Regression test: stable-run replay coberto por testes de determinismo EditMode; CaveReplayValidator
  (editor, batchmode) NOT RUN nesta sessão (mesma limitação dos menu validators — sem wrapper .ps1)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: builds exit 0 (feito) + EditMode PASS (feito) + replay
  validator PASS (NOT RUN — pendente humano) + Play Mode humano (pendente)
```

---

## Anti-regressão (checklist da spec, seção 32)

- [x] Determinismo/stable-run preservado — só `CaveLayoutStableHash`; zero `Random`/`GetHashCode` no
      código novo (classifier, planner, cluster).
- [x] Planner NÃO recriado — `CaveEnvironmentElementPlanner.Build` é o mesmo método público, mesma
      assinatura, mesmo `EnsureGuaranteedPresence`/`PickEntry`/`BuildElementId` reusados.
- [x] `EnsureGuaranteedPresence` mantido (pedra + minério garantidos) — coberto por
      `Build_StillGuaranteesStoneAndOre_WithContextualPlacement`.
- [x] Sem `GameObject.Find`/`FindObjectOfType` — nenhum arquivo tocado usa essas APIs.
- [x] Sem refs Unity no save — `Context` não é persistido (derivável); `SerializedEnvironmentElement`
      shape inalterado.
- [x] Sem edição manual de `.unity`/`.prefab`/`.asset` — apenas C# tocado; assets `.asset` de
      `CaveBiomeArtProfile_*` precisam ser REGENERADOS pelo gerador (Unity Editor), não editados à mão.
- [x] CeilingHang renderiza acima da parede (`sortingOrder = 1` vs `0`), sem collider mesmo quando
      `blocking=true` (guard defensivo em `MaterializeDecorElement`).

---

## Next Action

```
1. Humano roda o cenário de Play Mode em
   docs/validation/playmode/spec_cave_decor_composition_human_test_scenario.md.
2. Opcional antes do Play Mode: rodar "CindarsHope/Inicializar Projeto" / "CindarsHope/Validar Projeto"
   no Editor para confirmar que o estado local continua idempotente. O follow-up Codex já executou o
   gerador e validadores equivalentes em batchmode.
3. Se Play Mode confirmar composição (teto/parede/chão agrupado, sem confete), promover o status de
   BUILD_VALIDATED para ACCEPTED em uma spec/documento de aceite visual.
```

---

*Report generated: 2026-07-06; Codex follow-up appended: 2026-07-07*
