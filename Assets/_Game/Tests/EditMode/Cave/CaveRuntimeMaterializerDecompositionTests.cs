using NUnit.Framework;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// Testes de decomposição do CaveRuntimeMaterializer.
    /// Verifica que os colaboradores extraídos preservam o comportamento original byte-for-byte.
    /// </summary>
    public class CaveRuntimeMaterializerDecompositionTests
    {
        // ──────────────────────────────────────────────────────────────────────────
        // CaveTileMaterializer.GridToWorld
        // ──────────────────────────────────────────────────────────────────────────

        [Test]
        public void GridToWorld_Origin_ReturnsNegativeHalfWidthHeight()
        {
            // Arrange
            var level = CreateFakeLevel(width: 10, height: 8);

            // Act
            var result = CaveTileMaterializer.GridToWorld(new Vector2Int(0, 0), level);

            // Assert — (0 - 10*0.5, 0 - 8*0.5, 0) = (-5, -4, 0)
            Assert.AreEqual(-5f, result.x, 0.001f, "GridToWorld X at origin should be -halfWidth");
            Assert.AreEqual(-4f, result.y, 0.001f, "GridToWorld Y at origin should be -halfHeight");
            Assert.AreEqual(0f, result.z, 0.001f, "GridToWorld Z should always be 0");
        }

        [Test]
        public void GridToWorld_Center_ReturnsNearZero()
        {
            // Arrange — width=10, height=8: center = (5,4)
            var level = CreateFakeLevel(width: 10, height: 8);

            // Act
            var result = CaveTileMaterializer.GridToWorld(new Vector2Int(5, 4), level);

            // Assert — (5 - 5, 4 - 4, 0) = (0, 0, 0)
            Assert.AreEqual(0f, result.x, 0.001f);
            Assert.AreEqual(0f, result.y, 0.001f);
        }

        [Test]
        public void GridToWorld_ArbitraryPosition_MatchesFormula()
        {
            // Arrange
            var level = CreateFakeLevel(width: 20, height: 16);
            var gridPos = new Vector2Int(3, 7);

            // Act
            var result = CaveTileMaterializer.GridToWorld(gridPos, level);

            // Assert — (3 - 10, 7 - 8, 0) = (-7, -1, 0)
            Assert.AreEqual(3f - 10f, result.x, 0.001f);
            Assert.AreEqual(7f - 8f,  result.y, 0.001f);
            Assert.AreEqual(0f, result.z, 0.001f);
        }

        // ──────────────────────────────────────────────────────────────────────────
        // CaveEnemyMaterializer.ResolveColliderRadius
        // ──────────────────────────────────────────────────────────────────────────

        [Test]
        public void ResolveColliderRadius_Tiny_Returns025()
        {
            Assert.AreEqual(0.25f, CaveEnemyMaterializer.ResolveColliderRadius("Tiny"), 0.001f);
        }

        [Test]
        public void ResolveColliderRadius_Small_Returns035()
        {
            Assert.AreEqual(0.35f, CaveEnemyMaterializer.ResolveColliderRadius("Small"), 0.001f);
        }

        [Test]
        public void ResolveColliderRadius_Medium_ReturnsFallback045()
        {
            // "Medium" não é mapeado explicitamente — cai no default 0.45f
            Assert.AreEqual(0.45f, CaveEnemyMaterializer.ResolveColliderRadius("Medium"), 0.001f);
        }

        [Test]
        public void ResolveColliderRadius_Large_Returns065()
        {
            Assert.AreEqual(0.65f, CaveEnemyMaterializer.ResolveColliderRadius("Large"), 0.001f);
        }

        [Test]
        public void ResolveColliderRadius_Huge_Returns095()
        {
            Assert.AreEqual(0.95f, CaveEnemyMaterializer.ResolveColliderRadius("Huge"), 0.001f);
        }

        [Test]
        public void ResolveColliderRadius_Boss_Returns12()
        {
            Assert.AreEqual(1.2f, CaveEnemyMaterializer.ResolveColliderRadius("Boss"), 0.001f);
        }

        [Test]
        public void ResolveColliderRadius_NullOrEmpty_ReturnsFallback045()
        {
            Assert.AreEqual(0.45f, CaveEnemyMaterializer.ResolveColliderRadius(null), 0.001f);
            Assert.AreEqual(0.45f, CaveEnemyMaterializer.ResolveColliderRadius(""), 0.001f);
            Assert.AreEqual(0.45f, CaveEnemyMaterializer.ResolveColliderRadius("Unknown"), 0.001f);
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────────

        private static CaveGeneratedLevel CreateFakeLevel(int width, int height)
        {
            return new CaveGeneratedLevel
            {
                Width = width,
                Height = height
            };
        }
    }
}
