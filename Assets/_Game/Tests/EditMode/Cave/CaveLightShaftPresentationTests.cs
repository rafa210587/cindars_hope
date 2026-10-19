using System.Collections.Generic;
using CindarsHope.Cave.Art;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    [TestFixture]
    public sealed class CaveLightShaftPresentationTests
    {
        [Test]
        public void FloorTiles_ImportedWithCenteredPivot_DoNotLeaveTilemapGaps()
        {
            const string folder = "Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/";
            foreach (var name in new[] { "floor_a", "floor_b", "floor_c", "floor_detail" })
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(folder + name + ".png");
                Assert.IsNotNull(sprite, name);
                Assert.AreEqual(64f, sprite.pivot.x, 0.01f, name);
                Assert.AreEqual(64f, sprite.pivot.y, 0.01f, name);
                Assert.AreEqual(0f, sprite.bounds.center.y, 0.001f, name);
            }
        }

        [Test]
        public void LightShaft_GeneratedSpriteHasTransparentBackgroundAndSmallWorldBounds()
        {
            var sprite = CaveVignetteController.GetOrCreateLightShaftSprite();
            Assert.IsNotNull(sprite);
            Assert.AreEqual(FilterMode.Point, sprite.texture.filterMode);
            Assert.AreEqual(1f, sprite.bounds.size.x, 0.001f);
            Assert.AreEqual(1f, sprite.bounds.size.y, 0.001f);

            var texture = sprite.texture;
            Assert.AreEqual(0f, texture.GetPixel(0, 0).a, 0.001f);
            Assert.AreEqual(0f, texture.GetPixel(63, 63).a, 0.001f);
            Assert.Greater(texture.GetPixel(32, 32).a, 0.1f);
        }

        [Test]
        public void LightShaft_ProfileEnablesSmallTransparentOverlay_AbsentProfileLeavesOnlyVignette()
        {
            var sourceTexture = new Texture2D(1, 1);
            var sourceSprite = Sprite.Create(sourceTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
            var profile = ScriptableObject.CreateInstance<CaveBiomeArtProfileSO>();
            profile.Id = "biome_stone_cavern";
            profile.EditorSetBandId(1);
            profile.EditorSetLightShaftSprite(sourceSprite);
            var resolver = new CaveBiomeArtResolver(new[] { profile });
            var level = new CaveGeneratedLevel { Width = 16, Height = 16, Entrance = new Vector2Int(8, 8) };
            var root = new GameObject("CaveLightShaftTestRoot");
            var objects = new List<GameObject>();

            try
            {
                CaveVignetteController.ApplyVignetteAndLightShaft(level, root.transform, resolver, 1, objects);
                Assert.AreEqual(2, objects.Count);
                var renderer = objects[1].GetComponent<SpriteRenderer>();
                Assert.IsNotNull(renderer);
                Assert.AreNotSame(sourceSprite, renderer.sprite);
                Assert.AreEqual(3f, renderer.bounds.size.x, 0.001f);
                Assert.AreEqual(5f, renderer.bounds.size.y, 0.001f);
                Assert.AreEqual(0f, renderer.sprite.texture.GetPixel(0, 0).a, 0.001f);

                objects.Clear();
                CaveVignetteController.ApplyVignetteAndLightShaft(level, root.transform,
                    new CaveBiomeArtResolver(new CaveBiomeArtProfileSO[0]), 1, objects);
                Assert.AreEqual(1, objects.Count);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(sourceSprite);
                Object.DestroyImmediate(sourceTexture);
            }
        }
    }
}
