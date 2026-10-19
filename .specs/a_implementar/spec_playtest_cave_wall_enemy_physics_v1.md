# SPEC — Cave: paredes no layer de obstáculos dos monstros

> Spec ID: spec_playtest_cave_wall_enemy_physics_v1 · Revisão: 1 · Status: implementação e gate focal PASS; aceite de circulação/revisit pendente
> Type: Runtime/physics · Domain: Cave/Enemy navigation · Priority: P1 · Parallelizable: no · Repo lock: `CaveTileMaterializer.cs`.
> Depende de: FASE9F stable run e teste de caracterização `CaveEnemyNavigationLayerPlayModeTests`.
> Bloqueia: avaliação headless de movimento dos monstros. Ordem de execucao: após diagnóstico de layer, antes de aceite de AI na Cave.
> required_adrs: [] · required_game_rules: []

# /speckit.specify

## Problema e Phase 0

`CaveTileMaterializer.MaterializeWalls` cria cada `WallTile_*` com `BoxCollider2D`, mas não atribui layer. Em 2026-09-27, o teste PlayMode `CaveEnemyNavigationLayerPlayModeTests.GeneratedWallCollider_IsIncludedInEnemyWorldSolidQuery` falhou: `WallTile_24_17` ficou em `Default` e `EnemyBrain` consultou `WorldSolid`, retornando vazio (`TestResults/playtest-cave-enemy-layer-playmode.xml`, total 1, failed 1). `ProjectSettings/TagManager.asset` contém `WorldSolid`; `CaveEnemyMaterializer` já configura o desvio com esse nome canônico. O erro afeta ambos os ramos de criação de parede (prefab e fallback).

## Requisitos e critérios

- **AC01**: toda parede materializada com collider, independentemente de prefab ou fallback, pertence ao layer `WorldSolid`; a mesma query de obstáculos do inimigo encontra seu `BoxCollider2D`.
- **AC02**: quantidade/posição/tamanho de paredes e colisão Player não mudam; sorting, tint, tilemap e decoração permanecem iguais.
- **AC03**: layout, seed, snapshots visitados, entrada/saída, inimigos, nós e save não mudam em first-visit ou revisit.

## Escopo

Permitidos: `Assets/_Game/Scripts/Cave/Runtime/CaveTileMaterializer.cs`, esta spec e relatório `docs/validation/spec_playtest_cave_wall_enemy_physics_v1/REPORT.md`. O teste PlayMode existente é de ownership da fatia Enemy/Movement; não editar. Proibido alterar `CaveGeneratedLevel`, geração, save/DTO, materialização de inimigos, prefabs, cenas/YAML, colliders de decoração ou configurações de layer. Não controlar tela.

# /speckit.plan

Reusar `CindarsHope.Core.Physics.GameplayLayerNames.TryAssignRuntimeLayer(GameObject, GameplayLayerNames.WorldSolid)` imediatamente após unir os ramos de `wallPrefab != null` e fallback, antes de adicionar `BoxCollider2D`. O helper deixa Default e emite alerta one-shot se o layer não existir; o layer canônico já está no projeto, portanto o teste deve exigir WorldSolid. Não alterar collider nem ordem de materialização. O `CaveGeneratedLevel` é somente lido por esta rotina; o diff deve confirmar ausência de mutação de conteúdo e save. Se prefab tiver filhos, o collider criado permanece no root atribuído ao WorldSolid.

Gate único de aceitação: rerun PlayMode do teste de regressão `CaveEnemyNavigationLayerPlayModeTests.GeneratedWallCollider_IsIncludedInEnemyWorldSolidQuery`, com 1/1 PASS e exit 0, acompanhado de revisão do diff para AC02/03. O teste deve continuar cobrindo `_wallTilePrefab=null` (cena atual); a revisão do ponto comum confirma o ramo prefab. Sem replay/revisit executado, declarar somente evidência limitada ao layer e manter pendência de stable run.

# /speckit.tasks

- [x] **T01 / AC01-02**: layer canônico atribuído no ponto comum dos ramos de `MaterializeWalls`, sem mudar collider/render.
- [x] **T02 / AC01**: PlayMode focal pós-correção 1/1 PASS, exit 0, ScanUnityLogs PASS; XML `TestResults/playtest-cave-enemy-layer-playmode.xml`.
- [x] **T03 / AC02-03**: diff revisado; relatório de limite estático e first-visit/revisit pendente; `/finish-spec` sem promoção.

## Edge cases

Layer ausente: helper mantém Default e alerta, sem exceção; teste de aceitação falha nesse caso para não mascarar desvio. Prefab com layer próprio: o root que recebe `BoxCollider2D` é normalizado para WorldSolid. Decoração visual sem collider não recebe layer sólido. Conteúdo/snapshot da Cave não deve ser re-rolado ou regravado por esta mudança.
