using System;
using CindarsHope.Editor.Physics;
using UnityEngine;

namespace CindarsHope.Editor.Art
{
    /// <summary>Measured furniture presentation and independent solid contact bases for the farm interior.</summary>
    internal static class FarmHouseInteriorAuthoring
    {
        internal static GameObject CreateFurniture<T>(Transform parent, string name, Sprite sprite,
            Vector2 foot, float height, Vector2 solidSize, float solidBottomInset) where T : Component
        {
            if (sprite == null) throw new InvalidOperationException("Missing farm interior sprite: " + name);
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = foot;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            // Reviewed bed61x64/chest128x115 both fill their alpha bounds. Uniform scaling
            // belongs to the visual only, so interaction/solid dimensions remain world units.
            var scale = height / sprite.bounds.size.y;
            var support = new Vector3(sprite.bounds.center.x, sprite.bounds.min.y, 0f);
            visual.transform.localPosition = -support * scale;
            visual.transform.localScale = new Vector3(scale, scale, 1f);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            renderer.sortingLayerName = "World";
            var solid = new GameObject("SolidBase");
            solid.transform.SetParent(root.transform, false);
            GenerateGameplayPhysicsLayers.TryAssignLayer(solid, GenerateGameplayPhysicsLayers.WorldSolid);
            var body = solid.AddComponent<BoxCollider2D>();
            body.size = solidSize;
            body.offset = new Vector2(0f, solidBottomInset + solidSize.y * 0.5f);
            var interaction = root.AddComponent<BoxCollider2D>();
            interaction.isTrigger = true;
            // Feet stop a body-height south of the base. Keep the selector's frontal
            // closest point within its0.45u range before that solid contact.
            interaction.size = new Vector2(solidSize.x + 0.35f, 2f);
            interaction.offset = new Vector2(0f, 0.1f);
            root.AddComponent<T>();
            return root;
        }
    }
}
