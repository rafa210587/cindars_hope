# Human Play Mode Scenario — spec_cave_decor_composition_runtime (CV03 composição por contexto)

> **Spec:** `spec_cave_decor_composition_runtime`
> **Status:** DEFERRED_TO_FINAL_VALIDATION (Play Mode humano ainda pendente; generator/validators
> equivalentes já foram executados em batchmode no follow-up de 2026-07-07).
> **Escopo:** o PLANNER e o RESOLVER agora colocam decor POR CONTEXTO (teto/wall-hug/chão em cluster)
> em vez de "1 elemento por célula andável aleatória". Este cenário confirma que a cena lê como
> composta (não confete) e que a revisita é idêntica (stable-run).

---

## PRÉ-REQUISITOS (confirmar no Unity Editor ANTES de testar)

1. Estado de assets:
   - O follow-up Codex de 2026-07-07 já rodou `GenerateCaveBiomeArtProfiles.Generate()` em batchmode:
     8 profiles atualizados, registry com 8 profiles.
   - Se o Editor local mostrar assets desatualizados ou se houver dúvida de import, rodar
     `CindarsHope/Inicializar Projeto` 1x para regenerar os profiles de forma idempotente.
   - O gerador preenche `CaveBiomeArtProfile_biome_stone_cavern.asset`
     com os 4 pools NOVOS por contexto preenchidos: `CeilingSprites` (chunk_stalactites),
     `WallHugSprites` (prop_mine_cart, chunk_ore_mound, chunk_rubble, rock_ore_0..5),
     `FloorClusterSprites` (chunk_mushrooms_giant, mushroom_cluster, prop_broken_pickaxe,
     prop_planks_rail, prop_water_puddle), `BlockingSprites` (chunk_rubble, chunk_ore_mound, rock_ore_0..5).
   - **Importante:** o `GenerationConfigVersion` do `CaveGenerationConfigSO` subiu de 4 para 5 no
     código; o asset `CaveGenerationConfig_Default.asset` precisa refletir esse valor no Editor
     (mesmo padrão do bump 3→4 anterior) para que snapshots antigos (com decor "confete") sejam
     invalidados e regenerados deterministicamente na próxima visita.
2. `CindarsHope/Validar Projeto`:
   - O follow-up Codex já validou `ValidateCaveBiomeArtProfiles`: PASS após ajustar o alias conhecido
     `biome_core` + `biome_final` no band 7. Warnings de biomas sem arte completa são esperados.
   - `ValidateCaveEcosystem (fable_78)` também foi validado em batchmode: `Errors=0`, `Warnings=0`.
   - No Editor, confirmar que não aparece ERROR novo antes de entrar em Play Mode.
3. Confirmar que a `CaveScene` tem `CaveRuntimeMaterializer` wireado com `_environmentElementDatabase`
   e `_ecosystemBalance` (dependência pré-existente; se ausente, nada materializa).
4. Unity Test Runner → EditMode → suíte `Cave` → confirmar `CaveDecorContextTests` (13/13),
   `CaveDecorClusterTests` (10/10), e que `CaveEnvironmentElementPlannerTests`/`CaveDecorSpritePoolTests`
   pré-existentes continuam passando (296/296 confirmado nesta sessão via batchmode).

---

## Setup do teste

1. Novo jogo (ou save com `CaveRunSeed` já fixado) — o `GenerationConfigVersion` bump garante que
   nenhum snapshot legado com decor "confete" sobrevive; a próxima visita gera o plano novo.
2. Entrar na `CaveScene` e descer até um nível do bioma 1 (`biome_stone_cavern`, níveis 1-10).

## Scenario A — Decor de teto (CeilingHang, critério 14.1)

1. Observar o topo das salas (bordas superiores de parede visíveis, onde a parede encontra o chão
   logo abaixo).
2. **EXPECT:** estalactites (`chunk_stalactites`) aparecem PENDENDO da parede/teto, renderizadas ACIMA
   da parede (sem ficar atrás dela), nunca no chão andável aberto.
3. **FAIL** se: alguma estalactite aparecer no meio do chão andável (o bug original que esta spec
   corrige) ou se colidir com o player (deveria ser puramente visual, sem `BoxCollider2D`).

## Scenario B — Decor encostado em parede (WallHug, critério 14.1)

1. Andar ao longo das paredes das salas.
2. **EXPECT:** carrinho de mina (`prop_mine_cart`), monte de minério (`chunk_ore_mound`), entulho
   (`chunk_rubble`) ou rochas com veio (`rock_ore_0..5`) aparecem ENCOSTADOS/adjacentes à parede — não
   soltos no meio do chão aberto.
3. **FAIL** se: esses elementos aparecerem isolados no miolo da sala, longe de qualquer parede.

## Scenario C — Decor de chão em clusters (FloorCluster, critérios 14.2/14.4)

1. Explorar o miolo aberto das salas (longe de qualquer parede).
2. **EXPECT:** cogumelos (`chunk_mushrooms_giant`, `mushroom_cluster`), picareta quebrada
   (`prop_broken_pickaxe`), trilho de tábuas (`prop_planks_rail`) e poça d'água
   (`prop_water_puddle`) aparecem AGRUPADOS (2-4 elementos próximos, formando uma composição), não
   como pontos únicos espalhados aleatoriamente pela sala inteira.
3. **EXPECT:** a densidade geral de decor de chão parece MENOR e mais intencional do que antes desta
   spec — menos "confete" cobrindo a sala inteira.
4. **FAIL** se: a sala ainda parecer com dezenas de elementos soltos sem agrupamento visível, ou se o
   chão estiver vazio demais (sem nenhum cluster) — nesse caso reportar como calibragem de densidade
   pendente (`CaveEcosystemBalanceSO.FloorClusterDensityMultiplier`), não bug de posicionamento.

## Scenario D — Pedra e minério garantidos (anti-regressão, EnsureGuaranteedPresence)

1. Percorrer o nível completo.
2. **EXPECT:** ao menos 1 pedra (decor) e 1 nó de minério (mineável, com colisão de interação) estão
   presentes — igual ao comportamento da fable_78/CV02, sem regressão.
3. **FAIL** se: nível gerado sem nenhuma pedra ou sem nenhum minério.

## Scenario E — Revisita idêntica (stable-run, critério 14.4)

1. Anotar 2-3 posições e tipos de decor específicos (teto, parede, cluster de chão) numa sala do
   bioma 1.
2. Sair do nível (`ForwardExit`/`BackExit`, ou salvar e recarregar) e retornar à mesma sala.
3. **EXPECT:** os MESMOS elementos aparecem nos MESMOS contextos e posições — nenhum reroll de layout,
   contexto ou sprite dentro do mesmo `CaveRunSeed`.
4. **FAIL** se: qualquer elemento mudar de posição, contexto ou sprite entre visitas.

## Scenario F — Fallback preservado em biomas sem pool por contexto (regressão)

1. Descer a um bioma sem arte de decor ainda (bioma 2+, ex. `biome_forest`).
2. **EXPECT:** decor continua aparecendo via o comportamento de fallback (prefab genérico ou
   placeholder builtin), sem nenhum erro/exception relacionado a `CaveDecorPlacementContext` ou aos
   pools novos.
3. **FAIL** se: aparecer erro de null-reference ou o decor sumir completamente nesse bioma.

---

## Resultado esperado consolidado

| Critério da spec | Cenário | Resultado esperado |
|---|---|---|
| 14.1 Colocação por contexto (Ceiling/WallHug) | A + B | Estalactite no teto/parede; carrinho/entulho/minério encostados em parede |
| 14.2 Clusters determinísticos | C | Cogumelo/picareta/tábua/poça agrupados (2-4), não singletons |
| 14.3 Arte por contexto | A + B + C | Cada contexto puxa do seu pool (Ceiling/WallHug/FloorCluster) |
| 14.4 Stable-run + densidade reduzida | E + C (menos confete) | Revisita idêntica; sala menos lotada que antes |
| Anti-regressão | D + F | Pedra+minério garantidos; fallback intacto em biomas sem pool |

Se qualquer FAIL acima ocorrer, registrar no execution report
(`docs/validation/spec_cave_decor_composition_execution_report.md`) como regressão e não promover a
spec além de `BUILD_VALIDATED`.
