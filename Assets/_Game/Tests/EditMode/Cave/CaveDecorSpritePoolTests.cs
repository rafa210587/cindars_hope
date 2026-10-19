using CindarsHope.Cave.Art;
using CindarsHope.Cave.Ecosystem;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// spec_cave_decor_placement_runtime (CV02), T002/T006 — cobre o pick determinístico de sprite
    /// de decor por (banda, CaveEnvironmentElementKind, hash estável) e o null-safety exigido pelo
    /// critério 14.1/14.2 da spec: pool vazio ou profile ausente nunca lança, sempre retorna false
    /// (fallback do CaveEnvironmentElementMaterializer permanece intacto).
    /// </summary>
    [TestFixture]
    public class CaveDecorSpritePoolTests
    {
        private static CaveBiomeArtProfileSO CreateProfileWithDecor(
            string biomeId,
            int bandId,
            Sprite[] nonBlocking,
            Sprite[] blocking)
        {
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = biomeId;
            profile.EditorSetBandId(bandId);
            profile.EditorSetDecorSprites(nonBlocking, blocking);
            return profile;
        }

        private static Sprite CreateDummySprite(string name)
        {
            var texture = new Texture2D(1, 1);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero);
            sprite.name = name;
            return sprite;
        }

        // ── Determinismo (critério 14.1) ────────────────────────────────────────────────────────

        [Test]
        public void TryGetDecorSprite_SameBandKindHash_AlwaysReturnsSameSprite()
        {
            var pool = new[] { CreateDummySprite("a"), CreateDummySprite("b"), CreateDummySprite("c") };
            var profile = CreateProfileWithDecor("biome_stone_cavern", 1, pool, System.Array.Empty<Sprite>());
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            var stableHash = CaveBiomeArtResolver.ComputeCellHash("world_1", "run_1", 3, 5, -2);

            Assert.IsTrue(resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.DecorNonBlocking, stableHash, out var first));
            Assert.IsTrue(resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.DecorNonBlocking, stableHash, out var second));
            Assert.AreSame(first, second, "Revisita (mesmo hash) deve escolher exatamente o mesmo sprite — stable-run.");

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TryGetDecorSprite_IndexIsHashModuloPoolLength()
        {
            var pool = new[] { CreateDummySprite("a"), CreateDummySprite("b"), CreateDummySprite("c"), CreateDummySprite("d") };
            var profile = CreateProfileWithDecor("biome_stone_cavern", 1, pool, System.Array.Empty<Sprite>());
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            const long hash = 9L; // 9 % 4 == 1
            Assert.IsTrue(resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.DecorNonBlocking, hash, out var sprite));
            Assert.AreSame(pool[1], sprite);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TryGetDecorSprite_DifferentKindsCanResolveDifferentPools()
        {
            var nonBlockingPool = new[] { CreateDummySprite("nb0") };
            var blockingPool = new[] { CreateDummySprite("b0") };
            var profile = CreateProfileWithDecor("biome_stone_cavern", 1, nonBlockingPool, blockingPool);
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            Assert.IsTrue(resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.DecorNonBlocking, 0L, out var nonBlockingSprite));
            Assert.AreSame(nonBlockingPool[0], nonBlockingSprite);

            Assert.IsTrue(resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.DecorBlocking, 0L, out var blockingSprite));
            Assert.AreSame(blockingPool[0], blockingSprite);

            Object.DestroyImmediate(profile);
        }

        // ── Null-safety (critério 14.2 / fallback preservado) ───────────────────────────────────

        [Test]
        public void TryGetDecorSprite_EmptyPool_ReturnsFalse()
        {
            var profile = CreateProfileWithDecor("biome_stone_cavern", 1, System.Array.Empty<Sprite>(), System.Array.Empty<Sprite>());
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            Assert.IsFalse(resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.DecorNonBlocking, 42L, out var sprite));
            Assert.IsNull(sprite);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TryGetDecorSprite_NoProfileForBand_ReturnsFalse()
        {
            var resolver = new CaveBiomeArtResolver(new CaveBiomeArtProfileSO[0]);

            Assert.IsFalse(resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.DecorNonBlocking, 42L, out var sprite));
            Assert.IsNull(sprite);
        }

        [Test]
        public void TryGetDecorSprite_NullProfileList_DoesNotThrow_ReturnsFalse()
        {
            var resolver = new CaveBiomeArtResolver(null);

            Assert.DoesNotThrow(() => resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.DecorBlocking, 7L, out _));
            Assert.IsFalse(resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.DecorBlocking, 7L, out var sprite));
            Assert.IsNull(sprite);
        }

        [Test]
        public void TryGetDecorSprite_NonDecorKind_AlwaysReturnsFalse()
        {
            var pool = new[] { CreateDummySprite("a") };
            var profile = CreateProfileWithDecor("biome_stone_cavern", 1, pool, pool);
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            Assert.IsFalse(resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.WaterTile, 0L, out var waterSprite));
            Assert.IsNull(waterSprite);

            Assert.IsFalse(resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.MineableNode, 0L, out var mineableSprite));
            Assert.IsNull(mineableSprite);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void TryGetDecorSprite_NegativeHash_NormalizesAndDoesNotThrow()
        {
            var pool = new[] { CreateDummySprite("a"), CreateDummySprite("b") };
            var profile = CreateProfileWithDecor("biome_stone_cavern", 1, pool, System.Array.Empty<Sprite>());
            var resolver = new CaveBiomeArtResolver(new[] { profile });

            Assert.DoesNotThrow(() => resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.DecorNonBlocking, -123L, out _));
            Assert.IsTrue(resolver.TryGetDecorSprite(1, CaveEnvironmentElementKind.DecorNonBlocking, -123L, out var sprite));
            Assert.IsNotNull(sprite);

            Object.DestroyImmediate(profile);
        }
    }
}
