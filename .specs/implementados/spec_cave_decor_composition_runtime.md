# SPEC — Cave Decor: Composição e Colocação por Contexto (CV03)

> **Spec ID:** `spec_cave_decor_composition_runtime`
> **Status:** Implementado e BUILD_VALIDATED (Play Mode humano pendente)
> **Wave:** CAVE_VISUALS — composição de decor
> **Priority:** P1
> **Type:** Runtime / Data / Editor
> **Domain:** Cave
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** specs docs-only
> **Must not run with:** fable_78 e specs tocando CaveEnvironmentElementPlanner/Materializer, CaveBiomeArtProfileSO, CaveRuntimeMaterializer
> **Repo lock scope:** `Assets/_Game/Scripts/Cave/Ecosystem/**`, `Assets/_Game/Scripts/Cave/Art/**`, `Assets/_Game/Scripts/Cave/Runtime/CaveEnvironmentElementMaterializer.cs`, `Assets/_Game/Scripts/Editor/Cave/GenerateCaveBiomeArtProfiles.cs`, `Assets/_Game/Data/Cave/Biomes/**`
> **Ordem de execucao:** depois de CV02 (art pass) e do fix de wiring do database
> **Depende de:**
> - `fable_78` (planner/materializer/snapshot — ESTENDER, não recriar)
> - `spec_cave_biome_art_profiles_runtime` (CV01), `spec_cave_decor_placement_runtime` (CV02 — pools de decor)
> - ADR-0005 (cave stable run)
> **Bloqueia:** nada
> **Scope:** substituir a colocação de decor "1 elemento por célula aleatória" por colocação POR CONTEXTO (teto/wall-hug/chão) + CLUSTERS compostos, com pools de arte por contexto — para o decor ler como cena curada (keyart) em vez de confete. Determinístico, stable-run preservado.
> **Out of scope:** stamps de salas-herói (tesouro/boss — spec futura separada); tamanho de nível maior (knob futuro, decidido "compor primeiro"); biomas 2–8; WaterTile/MineableNode logic (fable_78 mantém).

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## 5. Contexto

O decor da fable_78 agora aparece com arte real (CV01+CV02+fix de wiring), mas o `CaveEnvironmentElementPlanner`
coloca **1 elemento por célula andável aleatória, ponderado só por Kind** (planner linha ~100-108, `PickEntry`
por peso). Resultado validado em Play Mode: **confete de props** — singletons espalhados sem composição, e
bug de contexto gritante (**estalactites no chão**, quando deviam pender do teto). O humano validou a direção:
placement inteligente + clusters + stamps só para salas especiais (esta spec faz os 2 primeiros; stamps =
spec futura).

## 6. Problema

Colocação sem contexto = o decor nunca vai ler como a keyart (cena curada). Estalactite no chão lê como bug;
props soltos leem como ruído. A arte (cara, gerada) fica desvalorizada por má colocação.

## 7. Objetivo

O decor é colocado por CONTEXTO — estalactite/teto na borda superior de parede; carrinho/minério/entulho
encostado em parede; cogumelo/picareta/tábua/poça no chão aberto em CLUSTERS (não singletons) e em densidade
menor — com o sprite resolvido do pool do contexto. Determinístico por seed (stable-run), revisita idêntica.

## 8. Fontes obrigatórias lidas

```text
.specs/a_implementar/fable/fable_78_spec_cave_ecosystem_population_runtime.md
.specs/a_implementar/spec_cave_decor_placement_runtime.md  (CV02 — pools)
docs/design/gameplay/cave/CAVE_BIOME_VISUAL_REFERENCE.md
.claude/rules/cave-stable-run.md  +  .claude/skills/cave-stable-run-guard/SKILL.md
.claude/skills/rng-and-determinism/SKILL.md
```

## 9. Estado atual do repo (Phase 0 confirma)

```text
CaveEnvironmentElementPlanner (Cave/Ecosystem/): Build itera candidatos, PickEntry por peso, 1 placement/célula.
  EnsureGuaranteedPresence garante 1 pedra + 1 minério. BuildElementId posicional. ESTENDER a colocação.
CaveEnvironmentElementKind: DecorNonBlocking, DecorBlocking, WaterTile, MineableNode. Adicionar contexto SEM
  quebrar os 4 (ver Estratégia — usar um enum de contexto separado OU sub-tags, decidir na Phase 0 pelo menor risco).
CaveEnvironmentElementPlacement: ElementId, Kind, GridPosition, IsMineable, MineNodeDataId. Adicionar contexto (aditivo).
CaveBiomeArtProfileSO (CV01/CV02): pools DecorNonBlocking/DecorBlocking. Reorganizar em pools POR CONTEXTO.
CaveEnvironmentElementMaterializer: TryResolveDecorSprite por Kind. Passar a resolver por CONTEXTO.
CaveGeneratedLevel: WalkableTiles + WallTiles (fonte da detecção de contexto: teto = WallTile com sul walkable;
  wall-hug = walkable com ≥1 vizinho WallTile; chão-aberto = walkable com 0 vizinhos WallTile).
VisitedLevelSnapshot.EnvironmentElements + CaveSaveData: persistência. Contexto é derivável da posição OU
  aditivo no snapshot (decidir; se derivável, não muda save).
GenerationConfigVersion: bump p/ invalidar snapshots legados (mudou a colocação).
```

## 10. Engineering stories

```text
Como jogador, quero decor que pareça uma cena (carrinho junto de trilho e entulho; estalactite no teto), não confete.
Como stable-run, quero a nova colocação determinística por seed — revisita idêntica.
Como fable_78, quero que ESTENDAM meu planner (contexto/cluster), não recriem.
```

## 11. Escopo (inclui)

```text
- Enum de contexto de decor: CeilingHang (teto), WallHug (encostado em parede), FloorCluster (chão aberto, agrupado).
  (MineableNode/WaterTile/DecorBlocking existentes seguem seu caminho; contexto é p/ o decor visual.)
- Detecção de contexto por célula (puro, determinístico, a partir de WalkableTiles/WallTiles):
  * CeilingHang: célula WallTile cujo vizinho SUL é walkable (borda superior de parede visível).
  * WallHug: célula walkable com ≥1 vizinho ortogonal WallTile.
  * FloorCluster: célula walkable com 0 vizinhos WallTile (miolo aberto).
- Planner por contexto:
  * elementos de teto (estalactites) SÓ em células CeilingHang;
  * elementos wall-hug (carrinho, minério, entulho grande) SÓ em WallHug;
  * elementos de chão (cogumelo, picareta, tábua, poça) em FloorCluster, colocados em CLUSTERS:
    escolhe uma célula-semente determinística, coloca a semente + 1-3 elementos em células vizinhas livres
    (contadas por hash), formando um grupo; reduz a densidade de singletons.
- Cada elemento carrega seu contexto (aditivo no Placement) para o materializer resolver o pool certo.
- Pools de arte POR CONTEXTO no CaveBiomeArtProfileSO (CeilingSprites, WallHugSprites, FloorClusterSprites,
  BlockingSprites) — reclassificar os sprites do bioma 1:
    Ceiling = chunk_stalactites;
    WallHug = prop_mine_cart, chunk_ore_mound, chunk_rubble, rock_ore_0..5;
    FloorCluster = chunk_mushrooms_giant, mushroom_cluster, prop_broken_pickaxe, prop_planks_rail, prop_water_puddle.
- CaveBiomeArtResolver.TryGetDecorSprite passa a receber o CONTEXTO (não só Kind).
- Densidade de decor de chão reduzida (menos confete) — ajuste no CaveEcosystemBalanceSO ou peso do perfil (tunável, não inline).
- GenerationConfigVersion bump (regeneração determinística de snapshots legados).
- EditMode tests: detecção de contexto correta; cluster determinístico; estalactite nunca em chão aberto;
  wall-hug sempre adjacente a parede; determinismo por seed; back-compat de save.
```

## 12. Fora de escopo

```text
- Stamps de salas-herói (tesouro/boss/entrada) — spec futura (a parte "pré-moldada" curada).
- Tamanho de nível maior (knob futuro; decidido compor primeiro).
- Biomas 2–8; conflito/mercador/threat; Rule Tile de borda.
- Reescrever o snapshot/save da fable_78 (só aditivo se necessário).
```

## 13. Regras de não duplicação

```text
- ESTENDER CaveEnvironmentElementPlanner (contexto+cluster) — NÃO criar segundo planner.
- Arte por contexto no CaveBiomeArtProfileSO (CV01) — NÃO na ElementEntry da fable_78.
- Reusar CaveLayoutStableHash (FNV-1a) — sem Random/GetHashCode.
- Sem GameObject.Find; sem namespace Debug.
```

## 14. Critérios de aceite

### 14.1 Colocação por contexto
- CeilingHang só em célula de topo-de-parede (WallTile com sul walkable); nunca em chão aberto.
- WallHug só em célula walkable adjacente a parede.
- FloorCluster em chão aberto, agrupado (cluster de 2-4), não singletons espalhados.
- Evidência: EditMode `CaveDecorContextTests` (cada contexto respeita sua regra; estalactite nunca em chão aberto).

### 14.2 Clusters determinísticos
- Um cluster = semente + 1-3 vizinhos, escolhidos por hash estável; mesmo seed → mesmo cluster.
- Evidência: EditMode `CaveDecorClusterTests` (tamanho de cluster no range; determinismo; vizinhos livres).

### 14.3 Arte por contexto
- Bioma 1: cada contexto puxa do seu pool (estalactite do Ceiling, carrinho do WallHug, cogumelo do FloorCluster).
- Evidência: EditMode do resolver por contexto + verificação estática do materializer.

### 14.4 Stable-run + back-compat + builds
- Nova colocação determinística por (worldSeed,runSeed,caveLevel); revisita idêntica; GenerationConfigVersion++.
- Save antigo carrega (default seguro + regen determinística).
- `dotnet build` runtime+editor exit 0; replay validator PASS.

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Ecosystem/
  CaveDecorPlacementContext.cs            (enum: CeilingHang, WallHug, FloorCluster)
  CaveDecorContextClassifier.cs           (puro: célula → contexto, a partir de Walkable/WallTiles)
  CaveEnvironmentElementPlanner.cs        (ESTENDER: colocação por contexto + clusters)
  CaveEnvironmentElementPlacement.cs      (+ campo Context aditivo)
Assets/_Game/Scripts/Cave/Art/
  CaveBiomeArtProfileSO.cs                (pools por contexto)
  CaveBiomeArtResolver.cs                 (TryGetDecorSprite por contexto)
Assets/_Game/Scripts/Cave/Runtime/
  CaveEnvironmentElementMaterializer.cs   (resolver por contexto)
Assets/_Game/Scripts/Cave/Data/
  CaveEcosystemBalanceSO.cs               (só se a densidade reduzida virar campo — senão peso no perfil)
Assets/_Game/Scripts/Editor/Cave/
  GenerateCaveBiomeArtProfiles.cs         (preencher pools por contexto do bioma 1)
  ValidateCaveBiomeArtProfiles.cs         (WARNING pool de contexto vazio)
Assets/_Game/Tests/EditMode/Cave/
  CaveDecorContextTests.cs / CaveDecorClusterTests.cs
docs/validation/spec_cave_decor_composition_execution_report.md
docs/validation/playmode/spec_cave_decor_composition_human_test_scenario.md
```

## 16. Contratos

- **Runtime:** `CaveDecorPlacementContext CaveDecorContextClassifier.Classify(Vector2Int cell, CaveGeneratedLevel level)` (puro). `CaveEnvironmentElementPlanner` emite placements com Context. `CaveBiomeArtResolver.TryGetDecorSprite(band, context, stableHash, out sprite)`.
- **Save:** Context é derivável da posição (classifier) → NÃO precisa persistir; se o subagent achar mais seguro persistir, campo aditivo simples. Sem refs Unity.
- **Eventos/UI:** N/A.

## 20. Estratégia (fases)

```text
Fase 0 — Auditar planner (Build/PickEntry/EnsureGuaranteedPresence), Placement, materializer, pools CV02.
  Decidir: contexto como enum separado (recomendado) vs. novos Kinds. system-reuse-audit.
Fase 1 — Enum de contexto + classifier puro + EditMode do classifier.
Fase 2 — Planner por contexto + clusters (determinístico) + EditMode (contexto/cluster/estalactite-nunca-no-chão).
Fase 3 — Pools por contexto no profile + resolver por contexto + materializer + gerador preenche bioma 1.
Fase 4 — Densidade reduzida (tunável) + GenerationConfigVersion++ + back-compat test.
Fase 5 — Builds + replay validator + docs validation + cenário humano + report.
```

## 22. Paralelização

Parallelizable: NO. Must not run with fable_78 nem specs de Cave/**. Reason: estende planner/materializer/save da cave.

## 23. Impacto save/load

```text
Changes save schema? MAYBE (aditivo se persistir contexto; preferir derivar → NO). Migration? NO (default + version bump). Persists Unity refs? NO.
```

## 25. Impacto UI/Unity

```text
Changes UI/scenes/prefabs manual? NO. Changes SO/assets? YES (pools por contexto, via gerador). Play Mode final? YES. Human timing: DEFERRED_TO_FINAL_VALIDATION.
```

## 26. Riscos

```text
Risco: cluster/contexto quebrar determinismo → stable-run. Mitigação: só CaveLayoutStableHash; EditMode de determinismo; replay validator.
Risco: CeilingHang em WallTile pode colidir com o Tilemap de parede (Z/order). Mitigação: render acima da parede (sortingOrder), sem collider; overlap controlado (pende ~1 célula p/ dentro da sala).
Risco: densidade reduzida deixar a sala vazia demais. Mitigação: manter EnsureGuaranteedPresence (pedra+minério) + tunável; ajustar em Play Mode.
Risco: GenerationConfigVersion bump invalida saves de teste. Mitigação: esperado/documentado (regen determinística).
```

# /speckit.tasks

## 28. Tasks

```text
- [x] T001 — Phase 0 audit + decisão enum-de-contexto vs novos Kinds.
- [x] T002 — CaveDecorPlacementContext + CaveDecorContextClassifier (puro) + EditMode.
- [x] T003 — Planner: colocação por contexto (estalactite→CeilingHang; carrinho/minério/entulho→WallHug; chão→FloorCluster) + clusters determinísticos + EditMode.
- [x] T004 — Placement + Context; pools por contexto no CaveBiomeArtProfileSO + TryGetDecorSprite(context) + EditMode.
- [x] T005 — CaveEnvironmentElementMaterializer resolve por contexto; CaveRuntimeMaterializer inalterado no wiring (já passa resolver+db).
- [x] T006 — GenerateCaveBiomeArtProfiles preenche pools por contexto do bioma 1 (reclassificar sprites) + validator WARNING.
- [x] T007 — Densidade de chão reduzida (tunável) + GenerationConfigVersion++ + back-compat test.
- [x] T008 — Builds + docs validation + cenário humano + report; replay/Play Mode humano permanecem pendentes para ACCEPTED.
```

## 29. Validações obrigatórias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
# Unity Test Runner EditMode (Cave) + replay validator stable-run quando o Editor estiver disponível
```

## 30. Testing Quality Gate

```md
- Changed deterministic logic: YES (classifier, planner por contexto, clusters)
- Requires EditMode tests: YES (contexto, cluster, determinismo, back-compat)
- Requires PlayMode automated or final human scenario: YES (decor composto: estalactite no teto, cluster de mineração, sem confete)
- Requires regression test: YES (stable-run replay + save back-compat)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: builds exit 0 + EditMode PASS + replay validator PASS + Play Mode humano
```

## 31. Definition of Done

Colocação por contexto + clusters nos arquivos permitidos; planner estendido (não recriado); bioma 1 lê como cena
(estalactite no teto, cluster de mineração, chão agrupado, menos confete); builds exit 0; report criado; máx. BUILD_VALIDATED.

## 31.1 Evidência de execução e closeout

```text
Status final: BUILD_VALIDATED, não ACCEPTED.
Data do closeout: 2026-07-07.

Implementado:
- Enum/context classifier puro para CeilingHang, WallHug, FloorCluster e None.
- Planner de CaveEnvironmentElement estendido por contexto, com clusters determinísticos e garantia de pedra/minério preservada.
- CaveBiomeArtProfileSO com pools por contexto e resolver/materializer consumindo contexto sem alterar shape de save.
- GenerateCaveBiomeArtProfiles atualiza os 8 profiles de bioma; assets foram regenerados em batchmode.
- ValidateCaveBiomeArtProfiles ajustado para aceitar o alias conhecido biome_core + biome_final no band 7, porque existem 8 biomas declarados e 7 bands efetivos em CaveBandScaling.

Validação real:
- Invoke-UnityGeneratedProjectsBuild.ps1: exit 0; 7/7 projetos; 0 warnings; 0 errors.
- RunUnityEditModeTests.ps1: exit 0; 2747/2747 PASS; resultados em TestResults/cv03-postfix-editmode.xml.
- GenerateCaveBiomeArtProfiles.Generate: exit 0; 8 profiles atualizados.
- ValidateCaveBiomeArtProfiles.Validate: primeira execução detectou band 7 duplicado; após correção do alias conhecido, exit 0 / PASS com warnings esperados de arte ausente em biomas incompletos.
- ValidateCaveEcosystem.Run: exit 0; Errors=0; Warnings=0.
- tools/docs/validate_docs.ps1: exit 1 apenas por dívida documental legada/future specs/placeholders; sem erro específico da CV03.

Pendências:
- Play Mode humano/visual ainda pendente para promover de BUILD_VALIDATED para ACCEPTED.
- Replay validator dedicado não foi executado; determinismo coberto por EditMode nesta fatia.
- Biomas sem arte completa continuam usando fallback/avisos esperados até specs futuras de arte.
```

## 32. Anti-regressão

```text
- Não quebrar o determinismo/stable-run (só CaveLayoutStableHash).
- Não recriar planner/snapshot; estender.
- Manter EnsureGuaranteedPresence (pedra+minério garantidos).
- Sem Random/GetHashCode; sem GameObject.Find; sem refs Unity no save.
- CeilingHang render acima da parede, sem collider (não bloquear passagem).
```

## 33. Notas para execução posterior

```text
- Stamps de salas-herói (tesouro/boss/entrada) = próxima spec da série (a parte "pré-moldada" curada que o humano quer p/ set-pieces).
- Aumentar tamanho de nível por banda = knob no layout profile/balance (decidido: compor primeiro, aumentar depois).
- Biomas 2–8 reusam este sistema trocando os pools por contexto conforme a arte de cada bioma for gerada.
```
