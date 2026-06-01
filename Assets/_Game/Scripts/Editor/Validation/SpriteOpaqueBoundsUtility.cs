using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class SpriteOpaqueBoundsUtility
    {
        /// <summary>
        /// Returns the local-space Bounds of the opaque pixels of <paramref name="sprite"/>.
        /// Falls back to the full rect / pixelsPerUnit when the texture is not readable
        /// or is a built-in resource (e.g. UISprite.psd).
        /// </summary>
        public static bool TryGetOpaqueLocalBounds(Sprite sprite, float alphaThreshold, out Bounds bounds)
        {
            bounds = default;
            if (sprite == null) return false;

            var texture = sprite.texture;
            if (texture == null) return TryGetRectLocalBounds(sprite, out bounds);

            var assetPath = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(assetPath))
            {
                // Built-in / non-asset texture — use full rect as bounds
                return TryGetRectLocalBounds(sprite, out bounds);
            }

            if (!texture.isReadable)
            {
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) return TryGetRectLocalBounds(sprite, out bounds);

                importer.isReadable = true;
                importer.SaveAndReimport();
                try
                {
                    return TryGetOpaquePixelBounds(sprite, alphaThreshold, out bounds);
                }
                finally
                {
                    importer.isReadable = false;
                    importer.SaveAndReimport();
                }
            }

            return TryGetOpaquePixelBounds(sprite, alphaThreshold, out bounds);
        }

        private static bool TryGetOpaquePixelBounds(Sprite sprite, float alphaThreshold, out Bounds bounds)
        {
            var texture = sprite.texture;
            var rect = sprite.rect;
            var startX = Mathf.RoundToInt(rect.x);
            var startY = Mathf.RoundToInt(rect.y);
            var w = Mathf.RoundToInt(rect.width);
            var h = Mathf.RoundToInt(rect.height);

            var minPx = w;
            var maxPx = -1;
            var minPy = h;
            var maxPy = -1;

            for (var py = 0; py < h; py++)
            {
                for (var px = 0; px < w; px++)
                {
                    if (texture.GetPixel(startX + px, startY + py).a > alphaThreshold)
                    {
                        if (px < minPx) minPx = px;
                        if (px > maxPx) maxPx = px;
                        if (py < minPy) minPy = py;
                        if (py > maxPy) maxPy = py;
                    }
                }
            }

            if (maxPx < 0) return TryGetRectLocalBounds(sprite, out bounds);

            // sprite.pivot is pixel offset from sprite-rect origin to pivot
            var ppu = sprite.pixelsPerUnit;
            var pivot = sprite.pivot;

            var localMinX = (minPx - pivot.x) / ppu;
            var localMaxX = (maxPx + 1 - pivot.x) / ppu;
            var localMinY = (minPy - pivot.y) / ppu;
            var localMaxY = (maxPy + 1 - pivot.y) / ppu;

            var cx = (localMinX + localMaxX) * 0.5f;
            var cy = (localMinY + localMaxY) * 0.5f;
            bounds = new Bounds(
                new Vector3(cx, cy, 0f),
                new Vector3(localMaxX - localMinX, localMaxY - localMinY, 0f));
            return true;
        }

        private static bool TryGetRectLocalBounds(Sprite sprite, out Bounds bounds)
        {
            bounds = default;
            if (sprite == null) return false;

            var ppu = sprite.pixelsPerUnit;
            var pivot = sprite.pivot; // pixels from rect-origin to pivot
            var rect = sprite.rect;

            var localMinX = (0f - pivot.x) / ppu;
            var localMaxX = (rect.width - pivot.x) / ppu;
            var localMinY = (0f - pivot.y) / ppu;
            var localMaxY = (rect.height - pivot.y) / ppu;

            var cx = (localMinX + localMaxX) * 0.5f;
            var cy = (localMinY + localMaxY) * 0.5f;
            bounds = new Bounds(
                new Vector3(cx, cy, 0f),
                new Vector3(localMaxX - localMinX, localMaxY - localMinY, 0f));
            return true;
        }
    }
}
