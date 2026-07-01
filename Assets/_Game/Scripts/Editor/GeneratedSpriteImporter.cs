using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    /// <summary>
    /// Aplica config de pixel art automaticamente a TODA textura sob
    /// Assets/_Game/Art/Generated/ (sprites gerados): Sprite single, Point filter,
    /// sem compressao, sem mipmap, PPU 48, pivot bottom-center.
    /// </summary>
    public class GeneratedSpriteImporter : AssetPostprocessor
    {
        private const string Root = "Assets/_Game/Art/Generated/";
        private const string ResourcesRoot = "Assets/_Game/Resources/EnemySprites/";
        private const string PlayerSpritesRoot = "Assets/_Game/Resources/PlayerSprites/";
        // 64px de altura / 128 PPU = 0.5 unidade base; com EnemyScaleResolver (medium=2x)
        // => ~1 unidade (~2 tiles), ~2x o player. Ajuste aqui se quiser inimigos maiores/menores.
        private const float Ppu = 128f;

        private void OnPreprocessTexture()
        {
            var p = assetPath.Replace('\\', '/');
            if (!p.StartsWith(Root) && !p.StartsWith(ResourcesRoot) && !p.StartsWith(PlayerSpritesRoot)) return;

            var imp = (TextureImporter)assetImporter;
            imp.textureType = TextureImporterType.Sprite;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Point;

            var s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            s.textureType = TextureImporterType.Sprite;
            s.spriteMode = (int)SpriteImportMode.Single;
            // Full Rect (nao Tight): necessario para SpriteRenderer.drawMode = Tiled (chao/paredes)
            // — Tight dispara "Sprite Tiling might not appear correctly ... not generated with Full Rect".
            s.spriteMeshType = SpriteMeshType.FullRect;
            s.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            s.spritePixelsPerUnit = Ppu;
            s.filterMode = FilterMode.Point;
            s.mipmapEnabled = false;
            imp.SetTextureSettings(s);
        }
    }
}
