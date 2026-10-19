using CindarsHope.Farm.Scene;
using CindarsHope.SceneManagement;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Tests.EditMode.World
{
    public sealed class PlaytestPropPhysicsInstallerTests
    {
        [Test]
        public void Farm_OnlyNamedVolumetricPropsGetIdempotentFootSupports()
        {
            var scene = EditorSceneManager.NewPreviewScene();
            var fence = SpriteFor("fence", 79, 64);
            var hay = SpriteFor("hay_bale", 58, 64);
            var crate = SpriteFor("crate", 61, 64);
            try
            {
                var decoration = Root(scene, "FarmDecoration");
                var details = Child(decoration, "FarmSettlementDetails");
                Visual(details, "Visual_SouthYardWestPost", fence);
                Visual(details, "Visual_SouthYardEastPost", fence);
                Visual(details, "Visual_Bush", fence);
                Visual(Root(scene, "FarmHouse"), "Visual_HomesteadHay", hay);
                var richness = Root(scene, "FarmRichness");
                Visual(richness, "Visual_HouseCrates", crate);
                Visual(richness, "Visual_ShedCrate", crate);

                Assert.That(FarmPropPhysicsInstaller.Ensure(scene).Count, Is.EqualTo(5));
                Assert.That(FarmPropPhysicsInstaller.Ensure(scene).Count, Is.EqualTo(5));
                Assert.That(details.Find("Visual_Bush/SolidFloorContact"), Is.Null);
                Assert.That(details.Find("Visual_SouthYardWestPost").GetComponentsInChildren<Collider2D>().Length, Is.EqualTo(1));
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
                Object.DestroyImmediate(fence.texture);
                Object.DestroyImmediate(hay.texture);
                Object.DestroyImmediate(crate.texture);
                Object.DestroyImmediate(fence);
                Object.DestroyImmediate(hay);
                Object.DestroyImmediate(crate);
            }
        }

        [Test]
        public void Town_WellAndGardenFenceGetSupports_BridgeAndBushRemainUnchanged()
        {
            var scene = EditorSceneManager.NewPreviewScene();
            var fence = SpriteFor("fence", 79, 64);
            var marker = SpriteFor("marker", 32, 32);
            try
            {
                Visual(Root(scene, "TownDecorations"), "TownWell", marker);
                var props = Root(scene, "TownProps");
                Visual(props, "GardenFence_00", fence);
                Visual(props, "GardenBush_00", fence);
                Visual(props, "RiverBridgeArt_00", marker);

                Assert.That(TownPropPhysicsInstaller.Ensure(scene).Count, Is.EqualTo(2));
                Assert.That(TownPropPhysicsInstaller.Ensure(scene).Count, Is.EqualTo(2));
                Assert.That(props.Find("GardenBush_00/SolidFloorContact"), Is.Null);
                Assert.That(props.Find("RiverBridgeArt_00/SolidFloorContact"), Is.Null);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
                Object.DestroyImmediate(fence.texture);
                Object.DestroyImmediate(marker.texture);
                Object.DestroyImmediate(fence);
                Object.DestroyImmediate(marker);
            }
        }

        private static Transform Root(Scene scene, string name)
        {
            var root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = new Vector3(name == "FarmDecoration" || name == "TownDecorations" ? 1000f :
                name == "FarmHouse" ? 1100f : 1200f, 1000f);
            return root.transform;
        }

        private static Transform Child(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static void Visual(Transform parent, string name, Sprite sprite)
        {
            var visual = Child(parent, name);
            visual.localPosition = new Vector3(parent.childCount * 4f, 0f);
            visual.gameObject.AddComponent<SpriteRenderer>().sprite = sprite;
        }

        private static Sprite SpriteFor(string name, int width, int height)
        {
            var texture = new Texture2D(width, height);
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, .5f), 64f);
            sprite.name = name;
            return sprite;
        }
    }
}
