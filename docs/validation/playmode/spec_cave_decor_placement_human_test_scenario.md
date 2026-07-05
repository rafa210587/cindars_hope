# Human Play Mode Scenario — spec_cave_decor_placement_runtime (CV02 art pass)

> **Spec:** `spec_cave_decor_placement_runtime`
> **Status:** DEFERRED_TO_FINAL_VALIDATION (Play Mode não rodado nesta sessão de implementação —
> nenhuma instância do Unity Editor disponível).
> **Escopo:** só ARTE do decor já posicionado pela fable_78 — planner/snapshot/densidade NÃO mudaram
> nesta spec. Este cenário testa apenas se o decor no bioma 1 (`biome_stone_cavern`) passa a mostrar
> sprites reais e se a revisita mantém o mesmo conjunto (stable-run).

---

## PRÉ-REQUISITOS (fazer no Unity Editor ANTES de testar)

1. `CindarsHope/Inicializar Projeto` (RunStep único — best-effort, log por passo):
   - Regenera os 8 `CaveBiomeArtProfileSO.asset` via `GenerateCaveBiomeArtProfiles.Generate()`.
   - Confirmar no log: `GenerateCaveBiomeArtProfiles: N profile(s) criado(s)/atualizado(s)` sem
     exceção.
2. `CindarsHope/Validar Projeto`:
   - Rodar `ValidateCaveBiomeArtProfiles` → conferir que **não há** warning
     `"DecorNonBlockingSprites vazio apesar de haver pasta de arte"` nem
     `"DecorBlockingSprites vazio apesar de haver pasta de arte"` para `biome_stone_cavern`
     (biomas 2-8 ainda não têm pasta de arte — warning esperado neles, não é falha).
   - Rodar `ValidateCaveEcosystem (fable_78)` → `Errors=0` (pré-requisito já documentado no cenário
     da fable_78; sem isto o ecossistema de decor não materializa e o teste abaixo não tem o que
     validar).
3. Confirmar que a `CaveScene` tem o `CaveRuntimeMaterializer` com `_environmentElementDatabase` e
   `_ecosystemBalance` wireados (dependência pré-existente da fable_78 — se ausente, nenhum elemento
   ambiental materializa e este cenário não pode prosseguir; ver
   `docs/validation/playmode/fable_78_human_test_scenario.md`).
4. Unity Test Runner → EditMode → suíte `Cave` → confirmar `CaveDecorSpritePoolTests` (9/9 PASS) e
   que nenhuma suíte pré-existente (`CaveBiomeArtProfilesTests`, `CaveEnvironmentElementPlannerTests`
   etc.) quebrou.

---

## Setup do teste

1. Novo jogo (ou save com `CaveRunSeed` já fixado) para garantir determinismo reproduzível.
2. Entrar na `CaveScene` e descer até um nível do bioma 1 (`biome_stone_cavern`, níveis 1-10).
3. Confirmar nos logs que **não** aparece o warning de "element prefab not assigned, using
   procedural placeholder" para os elementos de decor deste nível (indicaria que o resolver falhou e
   caiu no fallback antigo — sinal de wiring incompleto, não necessariamente um bug desta spec).

## Scenario A — Sala povoada com props reais (critério 14.1)

1. Explorar 2-3 salas do nível.
2. **EXPECT:** elementos de decor não-bloqueante aparecem com sprites reais reconhecíveis — cogumelos
   gigantes (`chunk_mushrooms_giant`), estalactites (`chunk_stalactites`), carrinho de mina
   (`prop_mine_cart`), picareta quebrada (`prop_broken_pickaxe`), trilho de tábuas
   (`prop_planks_rail`), poça d'água (`prop_water_puddle`) ou aglomerado de cogumelos
   (`mushroom_cluster`) — **não** mais o placeholder de cor chapada cinza/marrom.
3. **EXPECT:** elementos de decor bloqueante aparecem como entulho (`chunk_rubble`), monte de minério
   (`chunk_ore_mound`) ou uma das rochas com veio (`rock_ore_0` a `rock_ore_5`) — e continuam
   bloqueando o movimento do player (colisão preservada, `BoxCollider2D` intacto).
4. **FAIL** se: a sala continuar mostrando só placeholders de cor sólida (pool vazio — reconferir
   pré-requisito 1-2) ou se algum decor bloqueante deixar de colidir (regressão de colisão).

## Scenario B — Revisita idêntica (stable-run, critério 14.1)

1. Anotar (ou printar) 2-3 posições e sprites de decor específicos numa sala do bioma 1.
2. Sair do nível (avançar/retroceder por `ForwardExit`/`BackExit`, ou salvar e recarregar).
3. Retornar à mesma sala.
4. **EXPECT:** os MESMOS sprites aparecem nas MESMAS posições (mesmo objeto do pool, mesmo lugar) —
   nem o layout, nem a escolha de sprite mudam entre visitas dentro do mesmo `CaveRunSeed`.
5. **FAIL** se: o sprite de algum elemento mudar entre visitas (quebra de stable-run) ou se a posição
   mudar (isso seria regressão do planner, fora do escopo desta spec — reportar separadamente).

## Scenario C — Fallback preservado em biomas sem pool (critério 14.2, regressão)

1. Descer a um bioma sem arte de decor ainda (bioma 2+, ex. `biome_forest`).
2. **EXPECT:** decor continua aparecendo via o comportamento ANTIGO — prefab genérico
   (`_decorElementPrefab`, se wireado) ou o placeholder de cor chapada (builtin), exatamente como
   antes desta spec. Nenhum erro/exception no console.
3. **FAIL** se: aparecer qualquer erro de null-reference relacionado a `_biomeArtResolver` ou ao pool
   de decor — indicaria que o null-safety do resolver ou do materializer falhou.

---

## Resultado esperado consolidado

| Critério da spec | Cenário | Resultado esperado |
|---|---|---|
| 14.1 Sprite real por Kind, determinístico | A + B | Props reais visíveis; revisita idêntica |
| 14.2 Fallback preservado | C | Biomas sem pool continuam com o comportamento anterior, sem erro |
| 14.3 Builds | (verificado fora do Play Mode) | `dotnet build` runtime+editor exit 0 (já confirmado nesta sessão) |

Se qualquer FAIL acima ocorrer, registrar no execution report
(`docs/validation/spec_cave_decor_placement_execution_report.md`) como regressão e não promover a
spec além de `BUILD_VALIDATED`.
