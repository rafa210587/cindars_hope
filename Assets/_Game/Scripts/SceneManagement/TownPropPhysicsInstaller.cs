using System.Collections.Generic;
using CindarsHope.Core.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.SceneManagement
{
    /// <summary>Restores the floor-contact supports of accessible legacy Town props in the loaded scene.</summary>
    public static class TownPropPhysicsInstaller
    {
        public static IReadOnlyList<Collider2D> Ensure(Scene scene)
        {
            var supports = new List<Collider2D>();
            if (!scene.IsValid() || !scene.isLoaded) return supports;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name != "TownDecorations" && root.name != "TownProps") continue;
                foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    var visual = renderer.transform;
                    if (!renderer.enabled || !visual.gameObject.activeInHierarchy) continue;
                    bool well = root.name == "TownDecorations" && visual.name == "TownWell";
                    bool fence = root.name == "TownProps" && visual.name.StartsWith("GardenFence_", System.StringComparison.Ordinal);
                    if (!well && !fence) continue;
                    var sprite = renderer.sprite;
                    if (sprite == null) continue;
                    if (fence && (sprite.name != "fence" || sprite.rect.width != 79 || sprite.rect.height != 64))
                    {
                        Debug.LogWarning("Town garden fence support needs remeasurement: " + visual.name);
                        continue;
                    }
                    var existing = visual.Find("SolidFloorContact");
                    if (existing != null)
                    {
                        var collider = existing.GetComponent<BoxCollider2D>();
                        if (collider != null && collider.enabled && !collider.isTrigger) supports.Add(collider);
                        continue;
                    }

                    Rect pixels = fence
                        ? new Rect(2f, 0f, 75f, 18f)
                        : new Rect(sprite.rect.width * .15f, 0f, sprite.rect.width * .7f, sprite.rect.height * .35f);
                    if (HasExistingSupport(visual, sprite)) continue;
                    var body = new GameObject("SolidFloorContact");
                    body.transform.SetParent(visual, false);
                    var worldLayer = LayerMask.NameToLayer(GameplayLayerNames.WorldSolid);
                    body.layer = worldLayer >= 0 ? worldLayer : visual.gameObject.layer;
                    var added = body.AddComponent<BoxCollider2D>();
                    added.offset = (pixels.center - sprite.pivot) / sprite.pixelsPerUnit;
                    added.size = pixels.size / sprite.pixelsPerUnit;
                    supports.Add(added);
                }
            }
            Physics2D.SyncTransforms();
            return supports;
        }

        private static bool HasExistingSupport(Transform visual, Sprite sprite)
        {
            Physics2D.SyncTransforms();
            var y = sprite.rect.height * .1f;
            var filter = new ContactFilter2D { useTriggers = false };
            var overlapBuffer = new Collider2D[128];
            var points = new[]
            {
                new Vector2(sprite.rect.width * .5f, y),
                new Vector2(sprite.rect.width * .2f, y),
                new Vector2(sprite.rect.width * .8f, y)
            };
            Collider2D common = null;
            foreach (var pixel in points)
            {
                var point = visual.TransformPoint((pixel - sprite.pivot) / sprite.pixelsPerUnit);
                var count = Physics2D.OverlapPoint(point, filter, overlapBuffer);
                if (count >= overlapBuffer.Length)
                {
                    Debug.LogError("Town prop support overlap buffer saturated at " + visual.name + "; no new collider was added.");
                    return true;
                }
                Collider2D found = null;
                for (var index = 0; index < count; index++)
                {
                    var collider = overlapBuffer[index];
                    if (collider == null || !collider.enabled || collider.isTrigger || !collider.gameObject.activeInHierarchy ||
                        collider.gameObject.scene != visual.gameObject.scene ||
                        (collider.attachedRigidbody != null && collider.attachedRigidbody.bodyType != RigidbodyType2D.Static)) continue;
                    if (common != null && collider != common) continue;
                    found = collider;
                    break;
                }
                if (found == null) return false;
                common = found;
            }
            return common != null;
        }
    }
}
