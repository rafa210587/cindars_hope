using System.Collections.Generic;
using CindarsHope.Cave.Art;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// spec_cave_biome_art_profiles_runtime (CV01), T009 — cobre os 4 aspectos determinísticos e
    /// puros exigidos pela spec (critérios 14.1, 14.3, 14.4): fallback null-safe do resolver,
    /// determinismo do hash de variação de tile, mapeamento banda->profile, e a decisão pura de
    /// troca de banda usada pelo CaveBiomeChangedEvent.
    /// </summary>
    [TestFixture]
    public class CaveBiomeArtProfilesTests
    {
        // ── Fallback null-safe (critério 14.1) ──────────────────────────────────────────────────

        [Test]
        public void Resolver_EmptyProfileList_TryGetProfile_ReturnsFalse()
        {
            var resolver = new CaveBiomeArtResolver(new List<CaveBiomeArtProfileSO>());
            Assert.IsFalse(resolver.TryGetProfile(1, out var profile));
            Assert.IsNull(profile);
        }

        [Test]
        public void Resolver_NullProfileList_DoesNotThrow_AndAllTryGetReturnFalse()
        {
            var resolver = new CaveBiomeArtResolver(null);

            Assert.IsFalse(resolver.TryGetFloorTile(1, 0L, out var tile));
            Assert.IsNull(tile);

            Assert.IsFalse(resolver.TryGetWallTiles(1, out var face, out var top));
            Assert.IsNull(face);
            Assert.IsNull(top);

            Assert.IsFalse(resolver.TryGetHazardSprite(1, CaveHazardKind.ToxicPool, out var hazardSprite));
            Assert.IsNull(hazardSprite);

            Assert.IsFalse(resolver.TryGetChestSprite(1, CaveChestVisualState.Closed, out var chestSprite));
            Assert.IsNull(chestSprite);

            Assert.IsFalse(resolver.TryGetTrapSprite(1, "trap_spike_floor", out var trapSprite));
            Assert.IsNull(trapSprite);

            Assert.IsFalse(resolver.TryGetExitSprite(1, true, out var exitSprite));
            Assert.IsNull(exitSprite);
        }

        [Test]
        public void Resolver_ProfileWithoutFloorTiles_TryGetFloorTile_ReturnsFalse()
        {
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(1);
            // FloorTiles nunca setado — array vazio por padrão.

            var resolver = new CaveBiomeArtResolver(new[] { profile });
            Assert.IsTrue(resolver.TryGetProfile(1, out _), "Profile deve ser encontrado pela banda.");
            Assert.IsFalse(resolver.TryGetFloorTile(1, 0L, out var tile));
            Assert.IsNull(tile);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void Resolver_UnknownTrapId_TryGetTrapSprite_ReturnsFalse()
        {
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(1);

            var resolver = new CaveBiomeArtResolver(new[] { profile });
            Assert.IsFalse(resolver.TryGetTrapSprite(1, "trap_does_not_exist", out var sprite));
            Assert.IsNull(sprite);

            Object.DestroyImmediate(profile);
        }

        // ── Mapeamento banda -> profile (critério 14.1) ─────────────────────────────────────────

        [Test]
        public void Resolver_MultipleProfiles_TryGetProfile_ReturnsCorrectBand()
        {
            var profileBand1 = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profileBand1.Id = "biome_stone_cavern";
            profileBand1.EditorSetBandId(1);

            var profileBand2 = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profileBand2.Id = "biome_forest";
            profileBand2.EditorSetBandId(2);

            var resolver = new CaveBiomeArtResolver(new[] { profileBand1, profileBand2 });

            Assert.IsTrue(resolver.TryGetProfile(1, out var resolvedBand1));
            Assert.AreEqual("biome_stone_cavern", resolvedBand1.BiomeId);

            Assert.IsTrue(resolver.TryGetProfile(2, out var resolvedBand2));
            Assert.AreEqual("biome_forest", resolvedBand2.BiomeId);

            Assert.IsFalse(resolver.TryGetProfile(3, out var resolvedBand3));
            Assert.IsNull(resolvedBand3);

            Object.DestroyImmediate(profileBand1);
            Object.DestroyImmediate(profileBand2);
        }

        [Test]
        public void Resolver_DuplicateBandId_FirstEntryWins()
        {
            var first = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            first.Id = "biome_stone_cavern";
            first.EditorSetBandId(1);

            var duplicate = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            duplicate.Id = "biome_duplicate";
            duplicate.EditorSetBandId(1);

            var resolver = new CaveBiomeArtResolver(new[] { first, duplicate });

            Assert.IsTrue(resolver.TryGetProfile(1, out var resolved));
            Assert.AreEqual("biome_stone_cavern", resolved.BiomeId);

            Object.DestroyImmediate(first);
            Object.DestroyImmediate(duplicate);
        }

        // ── Determinismo do hash de variação (critério 14.3) ────────────────────────────────────

        [Test]
        public void ComputeCellHash_SameInputs_ProducesSameHash()
        {
            var a = CaveBiomeArtResolver.ComputeCellHash("world1", "run1", 5, 10, 20);
            var b = CaveBiomeArtResolver.ComputeCellHash("world1", "run1", 5, 10, 20);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void ComputeCellHash_DifferentCell_ProducesDifferentHash()
        {
            var a = CaveBiomeArtResolver.ComputeCellHash("world1", "run1", 5, 10, 20);
            var b = CaveBiomeArtResolver.ComputeCellHash("world1", "run1", 5, 11, 20);
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void ComputeCellHash_IsNonNegative()
        {
            // Normalizado para uso em módulo (índice de array) — nunca negativo.
            for (var x = -5; x <= 5; x++)
            {
                var hash = CaveBiomeArtResolver.ComputeCellHash("world_seed", "run_seed", 42, x, -x);
                Assert.GreaterOrEqual(hash, 0L);
            }
        }

        [Test]
        public void TryGetFloorTile_SameCellHash_ReturnsSameTileAcrossCalls()
        {
            var tileA = ScriptableObject.CreateInstance<Tile>();
            var tileB = ScriptableObject.CreateInstance<Tile>();

            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(1);
            profile.EditorSetFloorTiles(new TileBase[] { tileA, tileB });

            var resolver = new CaveBiomeArtResolver(new[] { profile });

            var cellHash = CaveBiomeArtResolver.ComputeCellHash("world1", "run1", 1, 3, 4);
            resolver.TryGetFloorTile(1, cellHash, out var first);
            resolver.TryGetFloorTile(1, cellHash, out var second);

            Assert.AreSame(first, second, "Revisitar a mesma célula deve produzir o mesmo tile (stable-run).");

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(tileA);
            Object.DestroyImmediate(tileB);
        }

        // ── Decisão pura de troca de banda (critério 14.4) ──────────────────────────────────────

        [Test]
        public void BiomeChangeDecision_FirstEntry_AlwaysCountsAsChange()
        {
            Assert.IsTrue(CaveBiomeChangeDecision.HasBandChanged(CaveBiomeChangeDecision.NoPreviousBand, 1));
        }

        [Test]
        public void BiomeChangeDecision_SameBand_IsNotAChange()
        {
            Assert.IsFalse(CaveBiomeChangeDecision.HasBandChanged(3, 3));
        }

        [Test]
        public void BiomeChangeDecision_DifferentBand_IsAChange()
        {
            Assert.IsTrue(CaveBiomeChangeDecision.HasBandChanged(2, 3));
        }

        // ── CaveBandScaling.BandForLevel usado para resolução de arte (sanity, não redefine spec) ──

        [Test]
        public void BandForLevel_CoversAllEightBiomeBands()
        {
            Assert.AreEqual(1, CaveBandScaling.BandForLevel(1));
            Assert.AreEqual(1, CaveBandScaling.BandForLevel(10));
            Assert.AreEqual(2, CaveBandScaling.BandForLevel(11));
            Assert.AreEqual(2, CaveBandScaling.BandForLevel(25));
            Assert.AreEqual(3, CaveBandScaling.BandForLevel(26));
            Assert.AreEqual(4, CaveBandScaling.BandForLevel(41));
            Assert.AreEqual(5, CaveBandScaling.BandForLevel(56));
            Assert.AreEqual(6, CaveBandScaling.BandForLevel(71));
            Assert.AreEqual(7, CaveBandScaling.BandForLevel(86));
            Assert.AreEqual(7, CaveBandScaling.BandForLevel(100)); // biome_final compartilha banda 7 (void)
        }

        // ── Fix pós-Play-Mode 2026-07-04: registry (fallback Resources) alimenta o resolver ─────
        // exatamente como o array serializado (CaveRuntimeMaterializer.ResolveBiomeArtProfiles usa
        // registry.Profiles como IEnumerable<CaveBiomeArtProfileSO> quando o array da cena está
        // vazio). Cobrimos aqui a parte C# pura: o registry devolve a mesma lista que foi setada, e
        // essa lista resolve no CaveBiomeArtResolver do mesmo jeito que o array serializado.

        [Test]
        public void Registry_EditorSetProfiles_ProfilesResolveThroughResolver_SameAsSerializedArray()
        {
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(1);

            var registry = ScriptableObject.CreateInstance<CaveBiomeArtProfileRegistrySO>();
            registry.EditorSetProfiles(new List<CaveBiomeArtProfileSO> { profile });

            var resolver = new CaveBiomeArtResolver(registry.Profiles);

            Assert.IsTrue(resolver.TryGetProfile(1, out var resolved));
            Assert.AreEqual("biome_stone_cavern", resolved.BiomeId);

            Object.DestroyImmediate(registry);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public void Registry_EditorSetProfiles_Null_BecomesEmptyList_NeverNull()
        {
            var registry = ScriptableObject.CreateInstance<CaveBiomeArtProfileRegistrySO>();
            registry.EditorSetProfiles(null);

            Assert.IsNotNull(registry.Profiles);
            Assert.AreEqual(0, registry.Profiles.Count);

            Object.DestroyImmediate(registry);
        }

        // ── Toggle dev T011: afeta só a resolução de arte, nunca gameplay ──────────────────────

        [Test]
        public void CaveBiomeArtDebug_Default_ResolvesNaturalBand()
        {
            CaveBiomeArtDebug.ForcedBandId = null;
            Assert.AreEqual(4, CaveBiomeArtDebug.ResolveBandForArt(4));
        }

        [Test]
        public void CaveBiomeArtDebug_Forced_OverridesArtBandOnly()
        {
            CaveBiomeArtDebug.ForcedBandId = 1;
            try
            {
                Assert.AreEqual(1, CaveBiomeArtDebug.ResolveBandForArt(6));
                // Gameplay (CaveBandScaling.BandForLevel) nunca é afetado pelo toggle de arte.
                Assert.AreEqual(6, CaveBandScaling.BandForLevel(75));
            }
            finally
            {
                CaveBiomeArtDebug.ForcedBandId = null;
            }
        }

        // ── Fix pós-Play-Mode 2026-07-04 (2ª rodada) — classificação borda/miolo da parede ──────
        // (bugfix: massa de parede lia como segundo piso andável). CaveTileMaterializer.IsWallInterior
        // é lógica pura de vizinhança em grid (sem Unity), usada só para leitura visual (tint) — não
        // toca em WalkableTiles/WallTiles/layout/spawn/snapshot.

        [Test]
        public void IsWallInterior_WallCellWithFloorToSouth_IsEdge_NotInterior()
        {
            var level = new CaveGeneratedLevel();
            var wallPos = new Vector2Int(5, 5);
            level.WalkableTiles.Add(new Vector2Int(5, 4)); // sul walkable

            Assert.IsFalse(CaveTileMaterializer.IsWallInterior(wallPos, level));
        }

        [Test]
        public void IsWallInterior_WallCellWithFloorToNorth_IsEdge_NotInterior()
        {
            var level = new CaveGeneratedLevel();
            var wallPos = new Vector2Int(5, 5);
            level.WalkableTiles.Add(new Vector2Int(5, 6)); // norte walkable

            Assert.IsFalse(CaveTileMaterializer.IsWallInterior(wallPos, level));
        }

        [Test]
        public void IsWallInterior_WallCellWithFloorToEastOrWest_IsEdge_NotInterior()
        {
            var level = new CaveGeneratedLevel();

            var wallPosEast = new Vector2Int(5, 5);
            level.WalkableTiles.Add(new Vector2Int(6, 5)); // leste walkable
            Assert.IsFalse(CaveTileMaterializer.IsWallInterior(wallPosEast, level));

            var levelWest = new CaveGeneratedLevel();
            var wallPosWest = new Vector2Int(5, 5);
            levelWest.WalkableTiles.Add(new Vector2Int(4, 5)); // oeste walkable
            Assert.IsFalse(CaveTileMaterializer.IsWallInterior(wallPosWest, levelWest));
        }

        [Test]
        public void IsWallInterior_WallCellWithNoWalkableNeighbor_IsInterior()
        {
            var level = new CaveGeneratedLevel();
            var wallPos = new Vector2Int(5, 5);
            // Todos os 4 vizinhos ortogonais são parede (ausentes de WalkableTiles) — miolo da massa.
            level.WallTiles.Add(new Vector2Int(5, 4));
            level.WallTiles.Add(new Vector2Int(5, 6));
            level.WallTiles.Add(new Vector2Int(4, 5));
            level.WallTiles.Add(new Vector2Int(6, 5));

            Assert.IsTrue(CaveTileMaterializer.IsWallInterior(wallPos, level));
        }

        [Test]
        public void IsWallInterior_DiagonalWalkableNeighbor_DoesNotCountAsEdge()
        {
            var level = new CaveGeneratedLevel();
            var wallPos = new Vector2Int(5, 5);
            // Apenas diagonal walkable (nao ortogonal) — ainda deve contar como miolo.
            level.WalkableTiles.Add(new Vector2Int(6, 6));

            Assert.IsTrue(CaveTileMaterializer.IsWallInterior(wallPos, level));
        }

        // ── Lote 3 (CV01 follow-up) — sombra de borda chão↔parede ───────────────────────────────
        // CaveTileMaterializer.IsFloorEdgeNextToWall é lógica pura de vizinhança em grid (espelho de
        // IsWallInterior), usada só para leitura visual (tint) — não toca em WalkableTiles/WallTiles/
        // layout/spawn/snapshot.

        [Test]
        public void IsFloorEdgeNextToWall_WallToSouth_IsEdge()
        {
            var level = new CaveGeneratedLevel();
            var floorPos = new Vector2Int(5, 5);
            level.WallTiles.Add(new Vector2Int(5, 4));

            Assert.IsTrue(CaveTileMaterializer.IsFloorEdgeNextToWall(floorPos, level));
        }

        [Test]
        public void IsFloorEdgeNextToWall_WallToNorth_IsEdge()
        {
            var level = new CaveGeneratedLevel();
            var floorPos = new Vector2Int(5, 5);
            level.WallTiles.Add(new Vector2Int(5, 6));

            Assert.IsTrue(CaveTileMaterializer.IsFloorEdgeNextToWall(floorPos, level));
        }

        [Test]
        public void IsFloorEdgeNextToWall_WallToEastOrWest_IsEdge()
        {
            var floorPosEast = new Vector2Int(5, 5);
            var levelEast = new CaveGeneratedLevel();
            levelEast.WallTiles.Add(new Vector2Int(6, 5));
            Assert.IsTrue(CaveTileMaterializer.IsFloorEdgeNextToWall(floorPosEast, levelEast));

            var floorPosWest = new Vector2Int(5, 5);
            var levelWest = new CaveGeneratedLevel();
            levelWest.WallTiles.Add(new Vector2Int(4, 5));
            Assert.IsTrue(CaveTileMaterializer.IsFloorEdgeNextToWall(floorPosWest, levelWest));
        }

        [Test]
        public void IsFloorEdgeNextToWall_NoWallNeighbor_IsNotEdge()
        {
            var level = new CaveGeneratedLevel();
            var floorPos = new Vector2Int(5, 5);
            // Vizinhos ortogonais todos walkable — longe de parede.
            level.WalkableTiles.Add(new Vector2Int(5, 4));
            level.WalkableTiles.Add(new Vector2Int(5, 6));
            level.WalkableTiles.Add(new Vector2Int(4, 5));
            level.WalkableTiles.Add(new Vector2Int(6, 5));

            Assert.IsFalse(CaveTileMaterializer.IsFloorEdgeNextToWall(floorPos, level));
        }

        [Test]
        public void IsFloorEdgeNextToWall_DiagonalWallNeighbor_DoesNotCountAsEdge()
        {
            var level = new CaveGeneratedLevel();
            var floorPos = new Vector2Int(5, 5);
            // Apenas diagonal e parede (nao ortogonal) — nao deve contar como borda.
            level.WallTiles.Add(new Vector2Int(6, 6));

            Assert.IsFalse(CaveTileMaterializer.IsFloorEdgeNextToWall(floorPos, level));
        }
    }
}
