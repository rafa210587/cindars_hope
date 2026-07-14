# SPEC — Cave Visual Polish: bordas de parede, cascalho, decor de parede e luz fake (CV04)

> **Spec ID:** `spec_cave_visual_polish_runtime`
> **Status:** A implementar
> **Wave:** CAVE_VISUALS — polimento p/ aproximar da keyart
> **Priority:** P1
> **Type:** Runtime / Data / Editor
> **Domain:** Cave
> **Parallelizable:** NO
> **Must not run with:** fable_78 e specs tocando Cave/Ecosystem, Cave/Runtime materializers, CaveBiomeArtProfileSO
> **Repo lock scope:** `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Editor/Cave/**`, `Assets/_Game/Data/Cave/Biomes/**`
> **Depende de:** CV01/CV02/CV03 (feitas), fable_78 (planner), ADR-0005
> **Bloqueia:** nada
> **Scope:** aproximar a cave da keyart (`gpt_cave_keyart_stone_cavern`) sem trocar o gerador procedural — (1) borda de rocha arredondada nas paredes (overlay determinístico das peças `wall_edge_*`, NÃO autotile), (2) cascalho denso no chão (scatter das 9 peças `litter_*`), (3) decor de superfície de parede (musgo/vegetação nas células de parede que encostam no chão), (4) luz FAKE de mood: vinheta escura nas bordas do nível + sprite de feixe de luz perto da entrada. Determinístico, stable-run.
> **Out of scope:** URP 2D Lights / iluminação dinâmica real (reforma de render — fora); stamps de sala-herói (spec futura); veios de minério dentro da parede com arte própria (usa o que há); biomas 2–8.

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## 5. Contexto

CV01/CV02/CV03 entregaram terreno, arte de decor por contexto e composição. Em Play Mode a cave
melhorou mas ainda está aquém da keyart-alvo (`art/world_gpt/raw/cave_guides/gpt_cave_keyart_stone_cavern.png`):
paredes em bloco reto (a keyart tem borda de rocha arredondada + vegetação/veios), chão ainda esparso
(a keyart é densamente detalhada), e sem mood de luz (a keyart tem feixe do teto + vinheta). Toda a
arte necessária JÁ existe no disco (`wall_edge_*`, `litter_*`), exceto o sprite de feixe de luz (gerado
nesta sessão). Esta spec wira/coloca essas peças + a luz fake. Reforça a regra de checar arte existente
antes de gerar ([[check-existing-art-before-generating]]).

## 7. Objetivo

A cave lê muito mais próxima da keyart: paredes com contorno de rocha arredondado, chão coberto de
cascalho/detalhe, musgo/vegetação nas bases de parede, e mood de luz (vinheta + feixe). Sem tocar o
gerador procedural nem o pipeline de render (luz é fake). Determinístico por seed.

## 9. Estado atual do repo (Phase 0 confirma)

```text
JÁ NO DISCO (reusar): Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/
  wall_edge_top/side/corner_a/corner_b.png (borda de rocha — geradas, nunca wiradas);
  litter_pebbles/rocks/crack_a/gravel/moss/mushrooms_small/rock_single/bone/crack_b.png (9 peças, órfãs);
  + o sprite de feixe de luz light_shaft.png (gerado nesta sessão, mover p/ a pasta).
Sistemas: CaveTileMaterializer (pinta floor/wall Tilemap; já tem sombra de borda chão↔parede);
  CaveEnvironmentElementPlanner/Materializer (decor por contexto — CV03); CaveBiomeArtResolver/ProfileSO
  (pools por contexto); CaveDecorContextClassifier (Ceiling/WallHug/FloorCluster). Câmera da CaveScene
  (CreateMvpCaveScene) com backgroundColor quase-preto.
NÃO EXISTE: overlay de borda de rocha; scatter denso de cascalho; decor em célula de parede;
  vinheta; feixe de luz.
```

## 11. Escopo (inclui)

```text
1. BORDA DE ROCHA (overlay determinístico, NÃO autotile):
   - No CaveTileMaterializer, para cada célula de PAREDE que encosta em chão, sobrepor a peça wall_edge
     apropriada (topo/lateral/quina) escolhida pelos vizinhos walkable (S walkable→edge_top; E/O→edge_side;
     quina→corner) — render acima da parede (sortingOrder), sem collider. Pool de wall_edge no
     CaveBiomeArtProfileSO (novo campo _wallEdgeSprites por posição: top/side/cornerA/cornerB).
   - Objetivo: contorno arredondado; a massa de parede continua o Tilemap atual por baixo.
2. CASCALHO DENSO (novo contexto de scatter):
   - Novo contexto CaveDecorPlacementContext.GroundScatter: MUITAS células de chão aberto (densidade
     ALTA, tunável) recebem UMA peça litter pequena, não-bloqueante, sem collider, sortingOrder baixo.
     Determinístico por hash. Pool GroundScatterSprites no profile (as 9 litter). Distinto do FloorCluster
     (props grandes, esparsos) — GroundScatter é miúdo e denso, preenche o vazio.
3. DECOR DE PAREDE (musgo/vegetação na base):
   - Colocar litter_moss + litter_mushrooms_small em células de PAREDE que encostam em chão (base de
     parede), render acima da parede, sem collider. Reusa o classifier (célula wall com vizinho floor).
4. LUZ FAKE (mood, sem URP 2D Lights):
   - VINHETA: escurecer progressivamente as células de chão/parede quanto mais perto da borda externa do
     nível (ou um overlay de gradiente radial no Canvas/SpriteRenderer grande DontDestroy na cave). Tunável.
   - FEIXE: instanciar 1 sprite light_shaft translúcido perto da entrada (ou de 1-2 células determinísticas),
     sortingOrder alto, blending normal/aditivo, sem collider.
   - Ambos determinísticos e sem custo por-frame (estáticos).
5. Densidade de FloorCluster/decor reavaliada (tunável no CaveEcosystemBalanceSO) — subir levemente se necessário.
6. Geradores/validators atualizados; EditMode tests (overlay de borda determinístico; scatter denso
   determinístico; contexto de parede; nada bloqueia o caminho).
```

## 12. Fora de escopo

```text
- URP 2D Lights / iluminação dinâmica (reforma de render).
- Stamps de sala-herói (spec futura separada).
- Autotile RuleTile preciso (usamos overlay determinístico, não 47-tile).
- Arte nova além do feixe (tudo já existe); biomas 2-8.
```

## 13. Não duplicação

```text
- Estender CaveTileMaterializer (overlay de borda) e o planner/contexto (CV03) — não recriar.
- Arte nos pools do CaveBiomeArtProfileSO — não na ElementEntry da fable_78.
- Só CaveLayoutStableHash (sem Random). Sem GameObject.Find. Sem editar .unity/.prefab YAML.
- Vinheta/feixe: se um overlay de câmera/canvas for melhor, criar via runtime/scene creator, sem YAML manual.
```

## 14. Critérios de aceite

```text
14.1 Paredes com borda arredondada (overlay determinístico nas células parede-encosta-chão); revisita idêntica.
14.2 Chão com cascalho DENSO (muitas células de chão com litter pequeno); determinístico; não bloqueia caminho.
14.3 Musgo/vegetação nas bases de parede.
14.4 Vinheta escurece bordas do nível + 1 feixe de luz visível perto da entrada.
14.5 Builds runtime+editor exit 0; replay validator PASS; EditMode dos itens determinísticos PASS.
Evidência: EditMode (overlay/scatter/contexto determinismo + não-bloqueio) + cenário Play Mode humano.
```

# /speckit.plan

## 15. Arquitetura alvo

```text
Cave/Art/CaveBiomeArtProfileSO.cs        (+ _wallEdgeSprites[4], _groundScatterSprites[])
Cave/Art/CaveBiomeArtResolver.cs         (+ TryGetWallEdgeSprite, TryGetGroundScatterSprite)
Cave/Ecosystem/CaveDecorPlacementContext.cs  (+ GroundScatter, WallSurface)
Cave/Ecosystem/CaveEnvironmentElementPlanner.cs (+ pass de GroundScatter denso + WallSurface base)
Cave/Runtime/CaveTileMaterializer.cs     (+ overlay de wall_edge nas células de borda)
Cave/Runtime/CaveEnvironmentElementMaterializer.cs (+ materializar GroundScatter/WallSurface por contexto)
Cave/Runtime/CaveVignetteController.cs   (novo — vinheta + feixe; runtime, sem YAML)
Editor/Cave/GenerateCaveBiomeArtProfiles.cs (+ preencher wallEdge/groundScatter pools do bioma 1)
Tests/EditMode/Cave/*                    (overlay/scatter/contexto)
docs/validation/spec_cave_visual_polish_execution_report.md + playmode scenario
```

## 20. Estratégia (fases)

```text
Fase 0 — Audit (materializer/planner/classifier/câmera). system-reuse-audit.
Fase 1 — Pools novos no profile + resolver + gerador preenche bioma 1 (wall_edge + litter).
Fase 2 — Overlay de borda de rocha no CaveTileMaterializer (determinístico) + EditMode.
Fase 3 — GroundScatter denso + WallSurface base no planner/materializer (determinístico, não-bloqueio) + EditMode.
Fase 4 — Vinheta + feixe (CaveVignetteController, runtime) + tuning de densidade.
Fase 5 — Builds + replay validator + docs validation + cenário humano + report. GenerationConfigVersion++ se placement mudar snapshot.
```

# /speckit.tasks

## 28. Tasks

```text
- [ ] T001 — Phase 0 audit (materializer/planner/classifier/câmera) + system-reuse-audit.
- [ ] T002 — Pools no profile (_wallEdgeSprites[4], _groundScatterSprites[]) + resolver TryGet* + gerador preenche bioma 1 (wall_edge + litter) + light_shaft importado.
- [ ] T003 — Overlay de borda de rocha no CaveTileMaterializer (célula parede-encosta-chão → wall_edge por vizinho, sortingOrder acima, sem collider) + EditMode determinismo.
- [ ] T004 — Contexto GroundScatter (denso, miúdo) + WallSurface (musgo/vegetação base de parede) no classifier/planner/materializer, determinístico, não-bloqueio + EditMode.
- [ ] T005 — CaveVignetteController (runtime): vinheta escura nas bordas + 1 feixe light_shaft perto da entrada, determinístico, estático (sem custo por-frame).
- [ ] T006 — Tuning de densidade (CaveEcosystemBalanceSO) + GenerationConfigVersion++ se placement mudar snapshot.
- [ ] T007 — Builds + replay validator + docs validation + cenário humano + report.
```

## 30. Testing Quality Gate

```md
- Changed deterministic logic: YES (overlay, scatter, contexto)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (borda arredondada, cascalho denso, musgo na parede, vinheta+feixe)
- Requires regression test: YES (não-bloqueio + stable-run replay)
- Human timing: DEFERRED_TO_FINAL_VALIDATION
- Min evidence ACCEPTED: builds exit 0 + EditMode PASS + replay PASS + Play Mode humano
```

## 32. Anti-regressão

```text
- Não bloquear caminho (overlays e scatter sem collider; scatter passa por não-bloqueio).
- Só CaveLayoutStableHash; sem Random. Sem GameObject.Find. Sem refs Unity no save.
- Manter EnsureGuaranteedPresence; não recriar planner/materializer.
- Luz é FAKE (overlay), não tocar render pipeline/URP.
```

## 33. Notas

```text
- Stamps de sala-herói = próxima spec (a sala igual à keyart, curada).
- Se a vinheta por-célula ficar cara/feia, trocar por overlay de gradiente no Canvas (runtime).
- Biomas 2-8 reusam quando a arte de cada um existir.
```
