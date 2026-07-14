using System.Collections.Generic;
using CindarsHope.Cave.Art;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Ecosystem;
using CindarsHope.Cave.Generation;
using CindarsHope.Combat;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// Responsável por materializar tiles de chão e paredes de um CaveGeneratedLevel.
    /// Não possui estado — pode ser reutilizado entre materializações.
    ///
    /// spec_cave_biome_art_profiles_runtime (CV01): quando o CaveBiomeArtProfileSO da banda fornece
    /// floorTiles/wallFaceTile/wallTopTile, pinta um Grid+Tilemap (camada 1 da arquitetura de 3
    /// camadas — CAVE_BIOME_VISUAL_REFERENCE §3.9) ao lado dos GameObjects de placeholder atuais
    /// (que continuam existindo para colisão de parede/BoxCollider2D — sem mudança de comportamento
    /// de gameplay). Profile sem tiles = SEM Tilemap, comportamento 100% idêntico ao atual.
    /// </summary>
    internal sealed class CaveTileMaterializer
    {
        private const float TilemapCellSize = 1f; // casa com GridToWorld (1 unidade por célula)

        // Fix pós-Play-Mode 2026-07-04 (2ª rodada, escurecido na 3ª): parede-miolo (nenhum vizinho
        // ortogonal walkable) pintada com wallTopTile "claro" lia visualmente como um segundo piso
        // andável — o jogador via um screenshot da massa de parede indistinguível do chão. Tint
        // quase-preto (estilo minas do Stardew) separa visualmente aro (borda, 1 célula, tile normal)
        // de miolo (massa interior, tile escurecido). 3ª rodada: aro ainda lia largo demais — miolo
        // escurecido mais forte (de 0.35/0.33/0.32 para quase-preto) para separação nítida aro fino
        // vs. massa escura. Valor de apresentação (não-balance) — const nomeada aqui por ser local a
        // este materializer; regra no-magic-balance-values cobre valores de gameplay, este é
        // puramente visual.
        private static readonly Color WallInteriorTint = new Color(0.22f, 0.21f, 0.20f);

        // Fix pós-Play-Mode 2026-07-04 (3ª rodada): degradê aro->miolo. Célula de TOPO que é borda
        // (tem vizinho ortogonal walkable) mas NÃO é a face sul-voltada (wallFaceTile) recebe este
        // tint intermediário — sem isso, toda borda não-face ficava tão clara quanto a face, então a
        // "moldura" de 1 célula lia larga (face + topo-borda, ambos claros) em vez de fina (só a
        // face). Mais escuro que o tile normal, mais claro que WallInteriorTint (degradê legível).
        private static readonly Color WallTopEdgeTint = new Color(0.55f, 0.52f, 0.5f);

        // Lote 3 (spec_cave_biome_art_profiles, CV01 follow-up): sombra de borda chão↔parede — uma
        // célula de chão encostada em pelo menos 1 parede (4-dir) recebe um leve escurecimento para
        // dar profundidade/aro onde o chão encontra a parede (complementa o WallInteriorTint/
        // WallTopEdgeTint já existentes, que fazem o mesmo do lado da parede). Valor de apresentação
        // (não-balance) — const nomeada local a este materializer, mesmo raciocínio dos tints acima.
        private static readonly Color FloorEdgeShadowTint = new Color(0.62f, 0.6f, 0.58f);

        // Fix pós-Play-Mode 2026-07-04: guard one-shot por instância (skill observability-and-logging)
        // — o materializer é recriado por CaveRuntimeMaterializer.EnsureCollaborators() apenas 1x por
        // sessão de Editor/Play, então 1 log por causa já cobre toda a run.
        private bool _floorPaintLogged;
        private bool _wallPaintLogged;

        /// <summary>
        /// Materializa todos os tiles de chão do nível, adicionando-os a materializedObjects e
        /// incrementando result.CreatedFloorTiles. biomeArtResolver é opcional (null = comportamento
        /// atual); quando fornece floorTiles, pinta também um Tilemap visual por baixo dos GameObjects.
        /// </summary>
        // spec_cave_visual_polish_runtime (CV04): default usado quando o caller não fornece um
        // CaveEcosystemBalanceSO — espelha CaveEcosystemBalanceSO.GroundScatterDensity default.
        private const float DefaultGroundScatterDensity = 0.35f;

        internal void MaterializeFloor(
            CaveGeneratedLevel level,
            Transform parent,
            SpriteRenderer floorPrefab,
            List<GameObject> materializedObjects,
            CaveRuntimeMaterializationResult result,
            CaveBiomeArtResolver biomeArtResolver = null,
            string worldSeed = null,
            string runSeed = null,
            CaveEcosystemBalanceSO ecosystemBalance = null)
        {
            var floorParent = new GameObject("GeneratedFloor");
            floorParent.transform.SetParent(parent);
            floorParent.transform.localPosition = Vector3.zero;

            var bandId = CaveBiomeArtDebug.ResolveBandForArt(Runtime.CaveBandScaling.BandForLevel(level.CaveLevel));
            var hasFloorTiles = biomeArtResolver != null
                && biomeArtResolver.TryGetProfile(bandId, out var floorProfile)
                && floorProfile.FloorTiles.Length > 0;
            var groundScatterDensity = ecosystemBalance != null ? ecosystemBalance.GroundScatterDensity : DefaultGroundScatterDensity;

            Tilemap floorTilemap = null;
            if (hasFloorTiles)
            {
                floorTilemap = CreateTilemapLayer(floorParent.transform, "FloorTilemap", CaveWorldSortingLayers.Ground, -1, level);
            }

            var edgeShadowCount = 0;
            var groundScatterCount = 0;

            foreach (var tilePos in level.WalkableTiles)
            {
                var worldPos = GridToWorld(tilePos, level);
                var cellHash = biomeArtResolver != null
                    ? CaveBiomeArtResolver.ComputeCellHash(worldSeed, runSeed, level.CaveLevel, tilePos.x, tilePos.y)
                    : 0L;

                if (hasFloorTiles)
                {
                    if (biomeArtResolver.TryGetFloorTile(bandId, cellHash, out var tile) && tile != null)
                    {
                        var tileCellPos = new Vector3Int(tilePos.x, tilePos.y, 0);
                        floorTilemap.SetTile(tileCellPos, tile);

                        // Lote 3 (CV01 follow-up): sombra de borda chão↔parede — dá profundidade/aro
                        // escuro onde o chão encontra a parede, sem asset novo (complementa o rim que
                        // fica para depois). Célula longe de parede fica com cor cheia (sem tint).
                        if (IsFloorEdgeNextToWall(tilePos, level))
                        {
                            edgeShadowCount++;
                            floorTilemap.SetTileFlags(tileCellPos, TileFlags.None);
                            floorTilemap.SetColor(tileCellPos, FloorEdgeShadowTint);
                        }
                    }
                }

                GameObject floorTile;

                if (floorPrefab != null)
                {
                    var spriteRenderer = Object.Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent.transform);
                    floorTile = spriteRenderer.gameObject;
                    spriteRenderer.sortingOrder = 0;
                    spriteRenderer.sortingLayerName = CaveWorldSortingLayers.Ground;
                }
                else
                {
                    floorTile = new GameObject($"FloorTile_{tilePos.x}_{tilePos.y}");
                    floorTile.transform.SetParent(floorParent.transform);
                    floorTile.transform.position = worldPos;

                    var spriteRenderer = floorTile.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = GetBuiltinSprite();
                    spriteRenderer.color = new Color(0.4f, 0.35f, 0.3f);
                    spriteRenderer.sortingOrder = 0;
                    spriteRenderer.sortingLayerName = CaveWorldSortingLayers.Ground;

                    // spec_cave_biome_art_profiles_runtime (CV01): sem isto, o placeholder marrom
                    // sólido (mesma sortingLayer/Order do Tilemap) cobriria a arte real por cima —
                    // o GameObject permanece (nome estável usado por outros sistemas, ex.
                    // RelocateGuardians) mas fica invisível quando o Tilemap já está pintado.
                    if (hasFloorTiles)
                    {
                        spriteRenderer.enabled = false;
                    }
                }

                floorTile.name = $"FloorTile_{tilePos.x}_{tilePos.y}";
                materializedObjects.Add(floorTile);
                result.CreatedFloorTiles++;

                // spec_cave_visual_polish_runtime (CV04), T004: cascalho/litter denso em chão aberto
                // (GroundScatter). Puramente visual (sem collider, sortingOrder baixo, acima do tile de
                // chão) — nunca bloqueia o caminho. Determinístico: elegibilidade + hit-roll + pick de
                // sprite vêm 100% de CaveLayoutStableHash (nunca Random/GetHashCode).
                if (biomeArtResolver != null
                    && ShouldPlaceGroundScatter(tilePos, level, worldSeed, runSeed, groundScatterDensity)
                    && biomeArtResolver.TryGetGroundScatterSprite(bandId, cellHash, out var scatterSprite)
                    && scatterSprite != null)
                {
                    var scatterGO = new GameObject($"GroundScatter_{tilePos.x}_{tilePos.y}");
                    scatterGO.transform.SetParent(floorParent.transform);
                    scatterGO.transform.position = worldPos;

                    var scatterRenderer = scatterGO.AddComponent<SpriteRenderer>();
                    scatterRenderer.sprite = scatterSprite;
                    scatterRenderer.sortingLayerName = CaveWorldSortingLayers.Ground;
                    scatterRenderer.sortingOrder = 1; // acima do tile de chão (order 0), sem colidir com nada.

                    materializedObjects.Add(scatterGO);
                    groundScatterCount++;
                }
            }

            LogFloorPaintStatusOnce(hasFloorTiles, bandId, level.WalkableTiles.Count, edgeShadowCount, groundScatterCount);
        }

        /// <summary>
        /// spec_cave_visual_polish_runtime (CV04), T004 — decide, de forma PURA e DETERMINÍSTICA, se uma
        /// célula de chão aberto (GroundScatter-elegível, mesma regra geométrica de FloorCluster) recebe
        /// uma peça de cascalho/litter. Hash salgado ("ground_scatter") distinto do hash de pick de sprite
        /// (cellHash) para não correlacionar as duas decisões. Extraído de MaterializeFloor para ser
        /// testável em EditMode sem instanciar GameObjects (mesmo espírito de IsWallInterior/IsFloorEdgeNextToWall).
        /// </summary>
        internal static bool ShouldPlaceGroundScatter(Vector2Int tilePos, CaveGeneratedLevel level, string worldSeed, string runSeed, float density)
        {
            if (density <= 0f)
            {
                return false;
            }

            if (!CaveDecorContextClassifier.IsGroundScatterCell(tilePos, level))
            {
                return false;
            }

            var hitHash = CaveLayoutStableHash.Compute($"{worldSeed}|{runSeed}|{level.CaveLevel}|{tilePos.x}|{tilePos.y}|ground_scatter");
            var unit = (hitHash & 0x7fffffff) / (float)int.MaxValue;
            return unit < density;
        }

        // Fix pós-Play-Mode 2026-07-04: hoje era 100% silencioso — nem pintar nem pular deixava
        // rastro. 1 log por sessão diz exatamente o que aconteceu (banda, célula, motivo do skip).
        // Lote 3: inclui a contagem de células de borda que receberam a sombra chão↔parede.
        // CV04: inclui a contagem de células com cascalho/litter (GroundScatter).
        private void LogFloorPaintStatusOnce(bool hasFloorTiles, int bandId, int cellCount, int edgeShadowCount, int groundScatterCount)
        {
            if (_floorPaintLogged)
            {
                return;
            }

            _floorPaintLogged = true;

            if (hasFloorTiles)
            {
                CombatLog.Log(
                    $"[Cave] CaveTileMaterializer: FloorTilemap pintado para bandId={bandId} ({cellCount} celula(s) walkable; " +
                    $"borda-sombreada={edgeShadowCount}, cascalho={groundScatterCount}).",
                    null);
            }
            else
            {
                Debug.Log(
                    $"[Cave] CaveTileMaterializer: nenhum FloorTilemap pintado para bandId={bandId} " +
                    "(profile de arte sem FloorTiles ou ausente). Usando somente placeholders proceduais.");
            }
        }

        /// <summary>
        /// Materializa todos os tiles de parede do nível, adicionando colisão e incrementando
        /// result.CreatedWallTiles. biomeArtResolver é opcional (null = comportamento atual); quando
        /// fornece wallFaceTile/wallTopTile, pinta também um Tilemap visual (SEM collider próprio —
        /// a colisão continua vindo do BoxCollider2D por-GameObject já existente, risco 26 da spec).
        /// Aproximação v1: célula de parede com chão imediatamente ao sul usa wallFaceTile (face
        /// frontal, 2 tiles de altura visual); demais células de parede usam wallTopTile.
        /// </summary>
        // spec_cave_visual_polish_runtime (CV04): default usado quando o caller não fornece um
        // CaveEcosystemBalanceSO — espelha CaveEcosystemBalanceSO.WallSurfaceChance default.
        private const float DefaultWallSurfaceChance = 0.12f;

        internal void MaterializeWalls(
            CaveGeneratedLevel level,
            Transform parent,
            SpriteRenderer wallPrefab,
            List<GameObject> materializedObjects,
            CaveRuntimeMaterializationResult result,
            CaveBiomeArtResolver biomeArtResolver = null,
            string worldSeed = null,
            string runSeed = null,
            CaveEcosystemBalanceSO ecosystemBalance = null)
        {
            var wallParent = new GameObject("GeneratedWalls");
            wallParent.transform.SetParent(parent);
            wallParent.transform.localPosition = Vector3.zero;

            var bandId = CaveBiomeArtDebug.ResolveBandForArt(Runtime.CaveBandScaling.BandForLevel(level.CaveLevel));
            TileBase wallFaceTile = null;
            TileBase wallTopTile = null;
            var hasWallTiles = biomeArtResolver != null
                && biomeArtResolver.TryGetWallTiles(bandId, out wallFaceTile, out wallTopTile)
                && (wallFaceTile != null || wallTopTile != null);
            var wallSurfaceChance = ecosystemBalance != null ? ecosystemBalance.WallSurfaceChance : DefaultWallSurfaceChance;

            Tilemap wallTilemap = null;
            if (hasWallTiles)
            {
                wallTilemap = CreateTilemapLayer(wallParent.transform, "WallTilemap", CaveWorldSortingLayers.World, 0, level);
            }

            var edgeCount = 0;
            var interiorCount = 0;
            var topEdgeTintedCount = 0;
            var wallEdgeCount = 0;
            var wallSurfaceCount = 0;

            foreach (var tilePos in level.WallTiles)
            {
                var worldPos = GridToWorld(tilePos, level);

                if (hasWallTiles)
                {
                    var southIsFloor = level.WalkableTiles.Contains(new Vector2Int(tilePos.x, tilePos.y - 1));
                    var tile = southIsFloor ? wallFaceTile : wallTopTile;
                    tile ??= southIsFloor ? wallTopTile : wallFaceTile; // um dos dois ausente: usa o outro
                    if (tile != null)
                    {
                        var tileCellPos = new Vector3Int(tilePos.x, tilePos.y, 0);
                        wallTilemap.SetTile(tileCellPos, tile);

                        // Fix pós-Play-Mode 2026-07-04 (2ª rodada, degradê na 3ª): miolo (nenhum
                        // vizinho ortogonal walkable) recebe tint escurecido — sem isso a massa de
                        // parede lia como um segundo piso andável (mesmo wallTopTile claro do aro).
                        // 3ª rodada: borda-topo (tem vizinho walkable, mas não é a face sul-voltada)
                        // recebe tint intermediário — sem isso a "moldura" lia larga (face + topo,
                        // ambos claros) em vez de fina (só a face iluminada).
                        if (IsWallInterior(tilePos, level))
                        {
                            interiorCount++;
                            wallTilemap.SetTileFlags(tileCellPos, TileFlags.None);
                            wallTilemap.SetColor(tileCellPos, WallInteriorTint);
                        }
                        else if (!southIsFloor)
                        {
                            edgeCount++;
                            topEdgeTintedCount++;
                            wallTilemap.SetTileFlags(tileCellPos, TileFlags.None);
                            wallTilemap.SetColor(tileCellPos, WallTopEdgeTint);
                        }
                        else
                        {
                            edgeCount++;
                        }
                    }
                }

                GameObject wallTile;

                if (wallPrefab != null)
                {
                    var spriteRenderer = Object.Instantiate(wallPrefab, worldPos, Quaternion.identity, wallParent.transform);
                    wallTile = spriteRenderer.gameObject;
                    spriteRenderer.sortingOrder = 0;
                    spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                    spriteRenderer.sortingLayerName = CaveWorldSortingLayers.World;
                }
                else
                {
                    wallTile = new GameObject($"WallTile_{tilePos.x}_{tilePos.y}");
                    wallTile.transform.SetParent(wallParent.transform);
                    wallTile.transform.position = worldPos;

                    var spriteRenderer = wallTile.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = GetBuiltinSprite();
                    spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
                    spriteRenderer.sortingOrder = 0;
                    spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                    spriteRenderer.sortingLayerName = CaveWorldSortingLayers.World;

                    // spec_cave_biome_art_profiles_runtime (CV01): sem isto, o placeholder cinza
                    // sólido cobriria a arte real do Tilemap por cima (mesma sortingLayer/Order) —
                    // o GameObject permanece para o BoxCollider2D (colisão inalterada), só o sprite
                    // some quando o Tilemap já pintou pelo menos wallFaceTile ou wallTopTile.
                    if (hasWallTiles)
                    {
                        spriteRenderer.enabled = false;
                    }
                }

                wallTile.name = $"WallTile_{tilePos.x}_{tilePos.y}";

                var collider = wallTile.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;

                materializedObjects.Add(wallTile);
                result.CreatedWallTiles++;

                // spec_cave_visual_polish_runtime (CV04), T003: overlay determinístico de borda de rocha
                // (NÃO autotile) — puramente visual, sem collider, renderiza acima da massa de parede.
                if (biomeArtResolver != null
                    && TryResolveWallEdgeKind(tilePos, level, out var edgeKind, out var mirrorX)
                    && biomeArtResolver.TryGetWallEdgeSprite(bandId, edgeKind, out var edgeSprite)
                    && edgeSprite != null)
                {
                    var edgeGO = new GameObject($"WallEdge_{tilePos.x}_{tilePos.y}");
                    edgeGO.transform.SetParent(wallParent.transform);
                    edgeGO.transform.position = worldPos;

                    var edgeRenderer = edgeGO.AddComponent<SpriteRenderer>();
                    edgeRenderer.sprite = edgeSprite;
                    edgeRenderer.sortingLayerName = CaveWorldSortingLayers.World;
                    edgeRenderer.sortingOrder = 2; // acima do tilemap de parede (order 0) e do decor de teto CV03 (order 1).
                    edgeRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                    edgeRenderer.flipX = mirrorX;

                    materializedObjects.Add(edgeGO);
                    wallEdgeCount++;
                }

                // spec_cave_visual_polish_runtime (CV04), T004: musgo/vegetação de base de parede
                // (WallSurface) — baixa chance determinística, puramente visual, sem collider.
                if (biomeArtResolver != null
                    && ShouldPlaceWallSurfaceDecor(tilePos, level, worldSeed, runSeed, wallSurfaceChance))
                {
                    var wallSurfaceHash = CaveBiomeArtResolver.ComputeCellHash(worldSeed, runSeed, level.CaveLevel, tilePos.x, tilePos.y);
                    if (biomeArtResolver.TryGetWallSurfaceSprite(bandId, wallSurfaceHash, out var wallSurfaceSprite) && wallSurfaceSprite != null)
                    {
                        var wallSurfaceGO = new GameObject($"WallSurface_{tilePos.x}_{tilePos.y}");
                        wallSurfaceGO.transform.SetParent(wallParent.transform);
                        wallSurfaceGO.transform.position = worldPos;

                        var wallSurfaceRenderer = wallSurfaceGO.AddComponent<SpriteRenderer>();
                        wallSurfaceRenderer.sprite = wallSurfaceSprite;
                        wallSurfaceRenderer.sortingLayerName = CaveWorldSortingLayers.World;
                        wallSurfaceRenderer.sortingOrder = 3; // acima do overlay de borda (order 2).
                        wallSurfaceRenderer.spriteSortPoint = SpriteSortPoint.Pivot;

                        materializedObjects.Add(wallSurfaceGO);
                        wallSurfaceCount++;
                    }
                }
            }

            LogWallPaintStatusOnce(hasWallTiles, bandId, level.WallTiles.Count, edgeCount, interiorCount, topEdgeTintedCount, wallEdgeCount, wallSurfaceCount);
        }

        /// <summary>
        /// Fix pós-Play-Mode 2026-07-04 (2ª rodada): classifica uma célula de parede como BORDA (aro —
        /// pelo menos um vizinho ortogonal walkable) ou MIOLO (interior da massa — nenhum vizinho
        /// ortogonal walkable). Lógica pura de vizinhança em grid, sem dependência de Unity — extraída
        /// para ser testável em EditMode (editmode-test-authoring).
        /// </summary>
        internal static bool IsWallInterior(Vector2Int wallPos, CaveGeneratedLevel level)
        {
            return !level.WalkableTiles.Contains(new Vector2Int(wallPos.x, wallPos.y - 1))
                && !level.WalkableTiles.Contains(new Vector2Int(wallPos.x, wallPos.y + 1))
                && !level.WalkableTiles.Contains(new Vector2Int(wallPos.x - 1, wallPos.y))
                && !level.WalkableTiles.Contains(new Vector2Int(wallPos.x + 1, wallPos.y));
        }

        /// <summary>
        /// Lote 3 (CV01 follow-up): classifica uma célula de CHÃO como BORDA (encostada em pelo
        /// menos 1 parede, 4-dir) para receber a sombra de contato chão↔parede — espelho de
        /// IsWallInterior, mesma lógica pura de vizinhança em grid testável em EditMode.
        /// </summary>
        internal static bool IsFloorEdgeNextToWall(Vector2Int floorPos, CaveGeneratedLevel level)
        {
            return level.WallTiles.Contains(new Vector2Int(floorPos.x, floorPos.y - 1))
                || level.WallTiles.Contains(new Vector2Int(floorPos.x, floorPos.y + 1))
                || level.WallTiles.Contains(new Vector2Int(floorPos.x - 1, floorPos.y))
                || level.WallTiles.Contains(new Vector2Int(floorPos.x + 1, floorPos.y));
        }

        /// <summary>
        /// spec_cave_visual_polish_runtime (CV04), T003 — decide, de forma PURA e DETERMINÍSTICA (sem
        /// RNG, só vizinhança em grid), qual peça de overlay de borda de rocha uma célula de PAREDE deve
        /// receber: sul walkable + leste/oeste walkable = quina (cornerA para sul+leste, cornerB para
        /// sul+oeste); só sul walkable = topo; só leste ou só oeste walkable = lateral (espelhada em X
        /// para o lado oeste). Célula sem nenhum vizinho walkable relevante (miolo de parede, ou só norte
        /// walkable) retorna false — sem overlay, fallback seguro (o Tilemap por baixo continua idêntico).
        /// Extraído de MaterializeWalls para ser testável em EditMode sem instanciar GameObjects.
        /// </summary>
        internal static bool TryResolveWallEdgeKind(Vector2Int wallPos, CaveGeneratedLevel level, out CaveWallEdgeKind kind, out bool mirrorX)
        {
            kind = CaveWallEdgeKind.Top;
            mirrorX = false;

            if (level == null || level.WalkableTiles == null)
            {
                return false;
            }

            var south = level.WalkableTiles.Contains(new Vector2Int(wallPos.x, wallPos.y - 1));
            var east = level.WalkableTiles.Contains(new Vector2Int(wallPos.x + 1, wallPos.y));
            var west = level.WalkableTiles.Contains(new Vector2Int(wallPos.x - 1, wallPos.y));

            if (south && east)
            {
                kind = CaveWallEdgeKind.CornerA;
                return true;
            }

            if (south && west)
            {
                kind = CaveWallEdgeKind.CornerB;
                return true;
            }

            if (south)
            {
                kind = CaveWallEdgeKind.Top;
                return true;
            }

            if (east)
            {
                kind = CaveWallEdgeKind.Side;
                return true;
            }

            if (west)
            {
                kind = CaveWallEdgeKind.Side;
                mirrorX = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// spec_cave_visual_polish_runtime (CV04), T004 — decide, de forma PURA e DETERMINÍSTICA, se uma
        /// célula de PAREDE que encosta em chão (<see cref="CaveDecorContextClassifier.IsWallSurfaceCell"/>)
        /// recebe musgo/vegetação de base. Hash salgado ("wall_surface") distinto do hash de pick de
        /// sprite para não correlacionar as duas decisões.
        /// </summary>
        internal static bool ShouldPlaceWallSurfaceDecor(Vector2Int wallPos, CaveGeneratedLevel level, string worldSeed, string runSeed, float chance)
        {
            if (chance <= 0f)
            {
                return false;
            }

            if (!CaveDecorContextClassifier.IsWallSurfaceCell(wallPos, level))
            {
                return false;
            }

            var hitHash = CaveLayoutStableHash.Compute($"{worldSeed}|{runSeed}|{level.CaveLevel}|{wallPos.x}|{wallPos.y}|wall_surface");
            var unit = (hitHash & 0x7fffffff) / (float)int.MaxValue;
            return unit < chance;
        }

        // Fix pós-Play-Mode 2026-07-04: mesmo raciocínio de LogFloorPaintStatusOnce, para paredes.
        // 2ª rodada: log agora inclui a contagem borda/miolo (evidência da classificação visual).
        // 3ª rodada: inclui quantas dessas bordas são topo-tintado (degradê aro->miolo).
        // CV04: inclui a contagem de overlays de borda de rocha e de decor de superfície de parede.
        private void LogWallPaintStatusOnce(bool hasWallTiles, int bandId, int cellCount, int edgeCount, int interiorCount, int topEdgeTintedCount, int wallEdgeCount, int wallSurfaceCount)
        {
            if (_wallPaintLogged)
            {
                return;
            }

            _wallPaintLogged = true;

            if (hasWallTiles)
            {
                CombatLog.Log(
                    $"[Cave] CaveTileMaterializer: WallTilemap pintado para bandId={bandId} ({cellCount} celula(s) de parede; " +
                    $"borda={edgeCount} (topo-tintado={topEdgeTintedCount}), miolo={interiorCount}, " +
                    $"borda-de-rocha={wallEdgeCount}, decor-superficie-parede={wallSurfaceCount}).",
                    null);
            }
            else
            {
                Debug.Log(
                    $"[Cave] CaveTileMaterializer: nenhum WallTilemap pintado para bandId={bandId} " +
                    "(profile de arte sem WallFaceTile/WallTopTile ou ausente). Usando somente placeholders proceduais. " +
                    $"borda-de-rocha={wallEdgeCount}, decor-superficie-parede={wallSurfaceCount}.");
            }
        }

        /// <summary>Cria um Grid+Tilemap simples para uso em runtime (não persiste como asset — a
        /// cave é procedural). cellSize casado com GridToWorld (1 unidade = 1 célula).
        /// Fix pós-Play-Mode 2026-07-04 (2º teste): a origem do Grid precisa casar com GridToWorld,
        /// que centraliza o nível na origem — sem este offset os tiles eram pintados em (x, y) puro,
        /// meio nível fora da câmera. O -0.5 extra alinha o CENTRO da célula do Tilemap (anchor
        /// padrão 0.5,0.5) com a posição que GridToWorld dá aos placeholders.</summary>
        private static Tilemap CreateTilemapLayer(Transform parent, string name, string sortingLayerName, int sortingOrder, CaveGeneratedLevel level)
        {
            var gridGo = new GameObject($"{name}Grid");
            gridGo.transform.SetParent(parent);
            gridGo.transform.position = new Vector3(
                -level.Width * 0.5f - 0.5f,
                -level.Height * 0.5f - 0.5f,
                0f);
            var grid = gridGo.AddComponent<Grid>();
            grid.cellSize = new Vector3(TilemapCellSize, TilemapCellSize, 0f);

            var tmGo = new GameObject(name);
            tmGo.transform.SetParent(gridGo.transform);
            tmGo.transform.localPosition = Vector3.zero;
            var tilemap = tmGo.AddComponent<Tilemap>();
            var renderer = tmGo.AddComponent<TilemapRenderer>();
            renderer.mode = TilemapRenderer.Mode.Chunk;
            renderer.sortingOrder = sortingOrder;

            foreach (var layer in SortingLayer.layers)
            {
                if (layer.name == sortingLayerName)
                {
                    renderer.sortingLayerName = sortingLayerName;
                    break;
                }
            }

            return tilemap;
        }

        /// <summary>
        /// Converte uma posição de grid para coordenadas de mundo, centralizado na origem.
        /// </summary>
        internal static Vector3 GridToWorld(Vector2Int gridPosition, CaveGeneratedLevel level)
        {
            var offsetX = level.Width * 0.5f;
            var offsetY = level.Height * 0.5f;
            return new Vector3(gridPosition.x - offsetX, gridPosition.y - offsetY, 0f);
        }

        /// <summary>
        /// Retorna o sprite builtin do Unity Editor para uso como placeholder procedural.
        /// Retorna null em builds de produção.
        /// </summary>
        internal static Sprite GetBuiltinSprite()
        {
#if UNITY_EDITOR
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#else
            return null;
#endif
        }
    }
}
