using System.Collections.Generic;
using CindarsHope.Core.Physics;
using UnityEngine;

namespace CindarsHope.Farm.Scene
{
    /// <summary>Installs the authored floor-contact bands in the existing FarmScene without resaving its tilemaps.</summary>
    public static class FarmPropPhysicsInstaller
    {
        private readonly struct Support
        {
            public Support(string root, string path, string sprite, int width, int height, Rect pixels)
            {
                Root = root; Path = path; Sprite = sprite; Width = width; Height = height; Pixels = pixels;
            }
            public readonly string Root, Path, Sprite;
            public readonly int Width, Height;
            // Pixel coordinates originate at the lower left of the native sprite.
            public readonly Rect Pixels;
        }

        private static readonly Support[] Supports =
        {
            new Support("FarmDecoration", "FarmSettlementDetails/Visual_SouthYardWestPost", "fence", 79, 64, new Rect(2, 0, 75, 18)),
            new Support("FarmDecoration", "FarmSettlementDetails/Visual_SouthYardEastPost", "fence", 79, 64, new Rect(2, 0, 75, 18)),
            new Support("FarmHouse", "Visual_HomesteadHay", "hay_bale", 58, 64, new Rect(4, 0, 49, 21)),
            new Support("FarmRichness", "Visual_HouseCrates", "crate", 61, 64, new Rect(2, 0, 57, 22)),
            new Support("FarmRichness", "Visual_ShedCrate", "crate", 61, 64, new Rect(2, 0, 57, 22))
        };

        public static IReadOnlyList<Collider2D> Ensure(UnityEngine.SceneManagement.Scene scene)
        {
            var installed = new List<Collider2D>();
            if (!scene.IsValid() || !scene.isLoaded) return installed;
            Physics2D.SyncTransforms();
            var roots = scene.GetRootGameObjects();
            foreach (var support in Supports)
            {
                Transform root = null;
                foreach (var candidate in roots)
                    if (candidate.name == support.Root) { root = candidate.transform; break; }
                var visual = root == null ? null : root.Find(support.Path);
                if (visual == null) continue;
                var renderer = visual.GetComponent<SpriteRenderer>();
                if (renderer == null || renderer.sprite == null) continue;
                var sprite = renderer.sprite;
                if (sprite.name != support.Sprite || sprite.rect.width != support.Width || sprite.rect.height != support.Height)
                {
                    Debug.LogWarning("Farm prop support needs remeasurement: " + support.Path);
                    continue;
                }

                var existing = visual.Find("SolidFloorContact");
                if (existing != null)
                {
                    var existingCollider = existing.GetComponent<BoxCollider2D>();
                    if (existingCollider != null && existingCollider.enabled && !existingCollider.isTrigger)
                        installed.Add(existingCollider);
                    continue;
                }
                if (HasFullExistingSupport(visual, sprite)) continue;

                var body = new GameObject("SolidFloorContact");
                body.transform.SetParent(visual, false);
                body.layer = LayerMask.NameToLayer(GameplayLayerNames.WorldSolid);
                if (body.layer < 0) body.layer = visual.gameObject.layer;
                var collider = body.AddComponent<BoxCollider2D>();
                var ppu = sprite.pixelsPerUnit;
                var pixelCenter = support.Pixels.center - sprite.pivot;
                collider.offset = pixelCenter / ppu;
                collider.size = support.Pixels.size / ppu;
                installed.Add(collider);
            }
            Physics2D.SyncTransforms();
            return installed;
        }

        private static bool HasFullExistingSupport(Transform visual, Sprite sprite)
        {
            var ppu = sprite.pixelsPerUnit;
            var y = sprite.rect.height * .1f - sprite.pivot.y;
            var filter = new ContactFilter2D { useTriggers = false };
            var overlapBuffer = new Collider2D[128];
            var sampleLocal = new[]
            {
                new Vector2(sprite.rect.width * .5f - sprite.pivot.x, y) / ppu,
                new Vector2(sprite.rect.width * .2f - sprite.pivot.x, y) / ppu,
                new Vector2(sprite.rect.width * .8f - sprite.pivot.x, y) / ppu
            };
            Collider2D common = null;
            foreach (var local in sampleLocal)
            {
                var point = visual.TransformPoint(local);
                var count = Physics2D.OverlapPoint(point, filter, overlapBuffer);
                if (count >= overlapBuffer.Length)
                {
                    Debug.LogError("Farm prop support overlap buffer saturated at " + visual.name + "; no new collider was added.");
                    return true;
                }
                Collider2D found = null;
                for (var index = 0; index < count; index++)
                {
                    var hit = overlapBuffer[index];
                    if (hit == null || !hit.enabled || hit.isTrigger || !hit.gameObject.activeInHierarchy ||
                        hit.gameObject.scene != visual.gameObject.scene ||
                        (hit.attachedRigidbody != null && hit.attachedRigidbody.bodyType != RigidbodyType2D.Static)) continue;
                    if (common != null && hit != common) continue;
                    found = hit;
                    break;
                }
                if (found == null) return false;
                common = found;
            }
            return common != null;
        }
    }
}
