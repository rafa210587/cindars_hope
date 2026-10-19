# Cave wall enemy physics — execução parcial

**Spec:** `spec_playtest_cave_wall_enemy_physics_v1` · **Data:** 2026-09-27 · **Status:** gate focal PASS, aceite de circulação/revisit pendente; sem promoção.

Antes: `CaveEnemyNavigationLayerPlayModeTests.GeneratedWallCollider_IsIncludedInEnemyWorldSolidQuery` falhou 0/1: `WallTile_24_17` em Default não apareceu na query WorldSolid usada pelo desvio dos monstros. O XML no mesmo caminho foi sobrescrito pelo rerun pós-correção; o resultado anterior e sua mensagem foram inspecionados antes da substituição. `CaveTileMaterializer.MaterializeWalls` agora atribui o layer canônico no ponto comum após construir `wallTile` via prefab ou fallback e antes de adicionar o `BoxCollider2D`. `ProjectSettings/TagManager.asset` já contém o layer.

| Critério | Evidência | Estado |
|---|---|---|
| AC01 | `TestResults/playtest-cave-enemy-layer-playmode.xml`: PlayMode pós-correção 1/1 PASS, exit 0, ScanUnityLogs PASS | PASS focal |
| AC02 | Diff não altera `collider.size`, world position, quantidade, sorting, tint ou Tilemap | PASS estático; colisão Player não executada |
| AC03 | Sem mudança em gerador, seed, snapshots, exits, inimigos, resource nodes e save | PASS estático; first-visit/revisit em Unity NOT RUN |

**Unity validation após a correção:** PlayMode focal 1/1 PASS, exit 0; log scanner PASS (coordenador). **Residual risk:** comportamento de movimento/colisão Player e replay/revisit não exercitados. `git diff --check` da alteração C#: PASS.

Regra stable run: a mudança ocorre somente na apresentação física do `WallTile_*` já materializado; conteúdo de primeiro acesso e revisita não é recalculado nem alterado. O snapshot/save não foi editado.
