using CindarsHope.Cave.Art;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// spec_cave_visual_polish_runtime (CV04), T002 — cobre TryGetWallEdgeSprite/TryGetGroundScatterSprite/
    /// TryGetWallSurfaceSprite/TryGetLightShaftSprite: determinismo (critério 14.1/14.2) e null-safety
    /// (profile ausente/pool vazio/tamanho errado nunca lança, sempre retorna false — fallback do
    /// CaveTileMaterializer/CaveVignetteController permanece intacto). Mesmo padrão de
    /// CaveDecorSpritePoolTests (CV02).
    /// </summary>
    [TestFixture]
    public class CaveVisualPolishSpritePoolTests
    {
        private static Sprite CreateDummySprite(string name)
        {
            var texture = new Texture2D(1, 1);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero);
            sprite.name = name;
            return sprite;
        }

        // ── TryGetWallEdgeSprite ─────────────────────────────────────────────────────────────

        private static CaveBiomeArtProfileSO CreateProfileWithWallEdge(int bandId, Sprite top, Sprite side, Sprite cornerA, Sprite cornerB)
        {
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(bandId);
            profile.EditorSetWallEdgeSprites(top, side, cornerA, cornerB);
            return profile;
        }

        [Test]
        public void TryGetWallEdgeSprite_ResolvesCorrectIndexPerKind()
        {
            var top = CreateDummySprite("top");
            var side = CreateDummySprite("side");
            var cornerA = CreateDummySprite("cornerA");
            var cornerB = CreateDummySprite("cornerB");
            var profile = CreateProfileWithWallEdge(1, top, side, cornerA, cornerB);
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            Assert.IsTrue(resolver.TryGetWallEdgeSprite(1, CaveWallEdgeKind.Top, out var resolvedTop));
            Assert.AreSame(top, resolvedTop);

            Assert.IsTrue(resolver.TryGetWallEdgeSprite(1, CaveWallEdgeKind.Side, out var resolvedSide));
            Assert.AreSame(side, resolvedSide);

            Assert.IsTrue(resolver.TryGetWallEdgeSprite(1, CaveWallEdgeKind.CornerA, out var resolvedCornerA));
            Assert.AreSame(cornerA, resolvedCornerA);

            Assert.IsTrue(resolver.TryGetWallEdgeSprite(1, CaveWallEdgeKind.CornerB, out var resolvedCornerB));
            Assert.AreSame(cornerB, resolvedCornerB);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TryGetWallEdgeSprite_NoProfileForBand_ReturnsFalse()
        {
            var resolver = new CaveBiomeArtResolver(new CaveBiomeArtProfileSO[0]);

            Assert.IsFalse(resolver.TryGetWallEdgeSprite(1, CaveWallEdgeKind.Top, out var sprite));
            Assert.IsNull(sprite);
        }

        [Test]
        public void TryGetWallEdgeSprite_EmptyPool_ReturnsFalse_DoesNotThrow()
        {
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(1);
            // Nunca chama EditorSetWallEdgeSprites — pool fica vazio (Array.Empty), tamanho != 4.
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            Assert.DoesNotThrow(() => resolver.TryGetWallEdgeSprite(1, CaveWallEdgeKind.Top, out _));
            Assert.IsFalse(resolver.TryGetWallEdgeSprite(1, CaveWallEdgeKind.Top, out var sprite));
            Assert.IsNull(sprite);

            Object.DestroyImmediate(profile);
        }

        // ── TryGetGroundScatterSprite ────────────────────────────────────────────────────────

        private static CaveBiomeArtProfileSO CreateProfileWithGroundScatter(int bandId, Sprite[] pool)
        {
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(bandId);
            profile.EditorSetGroundScatterSprites(pool);
            return profile;
        }

        [Test]
        public void TryGetGroundScatterSprite_SameHash_AlwaysReturnsSameSprite()
        {
            var pool = new[] { CreateDummySprite("a"), CreateDummySprite("b"), CreateDummySprite("c") };
            var profile = CreateProfileWithGroundScatter(1, pool);
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            var hash = CaveBiomeArtResolver.ComputeCellHash("world_1", "run_1", 3, 5, -2);

            Assert.IsTrue(resolver.TryGetGroundScatterSprite(1, hash, out var first));
            Assert.IsTrue(resolver.TryGetGroundScatterSprite(1, hash, out var second));
            Assert.AreSame(first, second, "Revisita (mesmo hash) deve escolher exatamente o mesmo sprite — stable-run.");

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TryGetGroundScatterSprite_IndexIsHashModuloPoolLength()
        {
            var pool = new[] { CreateDummySprite("a"), CreateDummySprite("b"), CreateDummySprite("c"), CreateDummySprite("d") };
            var profile = CreateProfileWithGroundScatter(1, pool);
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            const long hash = 9L; // 9 % 4 == 1
            Assert.IsTrue(resolver.TryGetGroundScatterSprite(1, hash, out var sprite));
            Assert.AreSame(pool[1], sprite);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TryGetGroundScatterSprite_EmptyPool_ReturnsFalse()
        {
            var profile = CreateProfileWithGroundScatter(1, System.Array.Empty<Sprite>());
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            Assert.IsFalse(resolver.TryGetGroundScatterSprite(1, 42L, out var sprite));
            Assert.IsNull(sprite);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TryGetGroundScatterSprite_NegativeHash_NormalizesAndDoesNotThrow()
        {
            var pool = new[] { CreateDummySprite("a"), CreateDummySprite("b") };
            var profile = CreateProfileWithGroundScatter(1, pool);
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            Assert.DoesNotThrow(() => resolver.TryGetGroundScatterSprite(1, -123L, out _));
            Assert.IsTrue(resolver.TryGetGroundScatterSprite(1, -123L, out var sprite));
            Assert.IsNotNull(sprite);

            Object.DestroyImmediate(profile);
        }

        // ── TryGetWallSurfaceSprite ──────────────────────────────────────────────────────────

        [Test]
        public void TryGetWallSurfaceSprite_SameHash_AlwaysReturnsSameSprite()
        {
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(1);
            var pool = new[] { CreateDummySprite("moss"), CreateDummySprite("mushroom") };
            profile.EditorSetWallSurfaceSprites(pool);
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            var hash = CaveBiomeArtResolver.ComputeCellHash("world_1", "run_1", 3, 5, -2);

            Assert.IsTrue(resolver.TryGetWallSurfaceSprite(1, hash, out var first));
            Assert.IsTrue(resolver.TryGetWallSurfaceSprite(1, hash, out var second));
            Assert.AreSame(first, second);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TryGetWallSurfaceSprite_EmptyPool_ReturnsFalse()
        {
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(1);
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            Assert.IsFalse(resolver.TryGetWallSurfaceSprite(1, 42L, out var sprite));
            Assert.IsNull(sprite);

            Object.DestroyImmediate(profile);
        }

        // ── TryGetLightShaftSprite ───────────────────────────────────────────────────────────

        [Test]
        public void TryGetLightShaftSprite_Assigned_ReturnsTrue()
        {
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(1);
            var lightShaft = CreateDummySprite("light_shaft");
            profile.EditorSetLightShaftSprite(lightShaft);
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            Assert.IsTrue(resolver.TryGetLightShaftSprite(1, out var sprite));
            Assert.AreSame(lightShaft, sprite);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TryGetLightShaftSprite_Unassigned_ReturnsFalse_DoesNotThrow()
        {
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(1);
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            Assert.DoesNotThrow(() => resolver.TryGetLightShaftSprite(1, out _));
            Assert.IsFalse(resolver.TryGetLightShaftSprite(1, out var sprite));
            Assert.IsNull(sprite);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TryGetLightShaftSprite_NoProfileForBand_ReturnsFalse()
        {
            var resolver = new CaveBiomeArtResolver(new CaveBiomeArtProfileSO[0]);

            Assert.IsFalse(resolver.TryGetLightShaftSprite(1, out var sprite));
            Assert.IsNull(sprite);
        }
    }
}
