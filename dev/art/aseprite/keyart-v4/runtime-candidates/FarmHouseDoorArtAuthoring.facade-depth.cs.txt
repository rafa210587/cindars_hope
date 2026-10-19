using System;
using CindarsHope.Farm;
using CindarsHope.World;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Art
{
    /// <summary>Registers the isolated leaf against the source facade's hinge, leaving its steps fixed.</summary>
    internal static class FarmHouseDoorArtAuthoring
    {
        internal static float SourcePixelWorldSize => FarmSceneCompositionContract.HomesteadRoofTargetWidth / 177f;
        // Source stair support is y192; the actual leaf foot is y165 (top-left image coordinates).
        internal static float DoorGroundY => FarmLevel1LayoutContract.HouseMinY + 27f * SourcePixelWorldSize;

        internal static Sprite FacadeAtDoorDepth()
        {
            var source = WorldSpriteLibrary.Building("farmhouse_opening_keyart_v4");
            if (source == null || source.texture.width != 212 || source.texture.height != 205)
                throw new InvalidOperationException("Farm facade depth requires the unchanged212x205 opening source.");
            // Ground sorting follows the leaf support(y165 from top), while the creator still
            // aligns source stair support(122,13). All rendered pixels retain their world positions.
            return PersistSprite(source, "farmhouse_opening_depth_v4", new Rect(0f, 0f, 212f, 205f),
                new Vector2(122f / 212f, 40f / 205f), "building", source.pixelsPerUnit);
        }

        internal static void Configure(HouseDoorInteractable door, Transform leaf, Transform threshold, BoxCollider2D blocker)
        {
            var sheet = WorldSpriteLibrary.Prop("farmhouse_door_open_v4");
            var opening = WorldSpriteLibrary.Prop("farmhouse_door_threshold_v4");
            if (sheet == null || sheet.texture.width != 135 || sheet.texture.height != 27 ||
                opening == null || opening.texture.width != 27 || opening.texture.height != 27)
                throw new InvalidOperationException("Farm door source must be five27x27 poses and a matching threshold.");
            var frames = new Sprite[5];
            for (var frame = 0; frame < frames.Length; frame++)
                frames[frame] = PersistSprite(sheet, "farmhouse_door_pose_" + frame,
                    new Rect(frame * 27, 0, 27, 27), new Vector2(3f / 27f, 5f / 27f));
            var thresholdSprite = PersistSprite(opening, "farmhouse_door_threshold",
                new Rect(0, 0, 27, 27), new Vector2(3f / 27f, 5f / 27f));
            var hinge = new Vector3(FarmLevel1LayoutContract.HouseStartX - 11f * SourcePixelWorldSize, DoorGroundY, 0f);
            var leafRenderer = ConfigureRenderer(leaf, frames[0], hinge, 0);
            ConfigureRenderer(threshold, thresholdSprite, hinge, -1);
            door.Configure(leaf, blocker, leaf.localPosition, leaf.localPosition);
            door.ConfigurePresentation(leafRenderer, frames, 0.08f);
            EditorUtility.SetDirty(door);
        }

        private static SpriteRenderer ConfigureRenderer(Transform target, Sprite sprite, Vector3 hinge, int order)
        {
            target.position = hinge;
            var scale = sprite.pixelsPerUnit * SourcePixelWorldSize;
            target.localScale = new Vector3(scale / target.parent.lossyScale.x, scale / target.parent.lossyScale.y, 1f);
            var renderer = target.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            renderer.sortingLayerName = "World";
            renderer.sortingOrder = order;
            return renderer;
        }

        private static Sprite PersistSprite(Sprite source, string name, Rect rect, Vector2 pivot,
            string category = "props", float pixelsPerUnit = 32f)
        {
            var path = "Assets/_Game/Art/Generated/World/" + category + "/" + name + ".asset";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                if (sprite.texture != source.texture || sprite.rect != rect || !Mathf.Approximately(sprite.pixelsPerUnit, pixelsPerUnit) ||
                    Vector2.Distance(sprite.pivot, Vector2.Scale(pivot, rect.size)) > 0.001f)
                    throw new InvalidOperationException("Existing farm door sprite contract differs: " + name);
                return sprite;
            }
            sprite = Sprite.Create(source.texture, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            AssetDatabase.CreateAsset(sprite, path);
            return sprite;
        }
    }
}
