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

        // Casas modulares (roof/walls/door A-B-C, sessao 2026-07): geradas a 64 px/tile, nao 128 —
        // ADR-0011 sera emendada 32->64 em tarefa separada. Path especifico sobrescreve o PPU default.
        private const string HousesModularRoot = "Assets/_Game/Art/Generated/World/building/houses_modular/";
        private const float HousesModularPpu = 64f;

        // Tiles de bioma da cave (spec CV01): 128 px/tile consumidos por Tilemap (Grid cellSize 1).
        // PPU 128 (tile = 1 unidade) e pivot Center (tile anchor padrao 0.5,0.5 preenche a celula).
        // v2 2026-07-04: 64px triturava as pedras; 128px por celula preserva o pixel-art da fonte.
        // Lote 2 de arte (2026-07): o carve-out de tile e restrito aos 5 NOMES de arquivo de tile —
        // chest_*/hazard_*/exit_*/prop_* na mesma pasta sao sprites de OBJETO (pivot bottom-center
        // default), nao tiles, e nao devem herdar PPU/pivot de tile so por estarem sob cave/.
        private const string CaveBiomeTilesRoot = "Assets/_Game/Art/Generated/World/cave/";
        private const float CaveBiomeTilePpu = 128f;

        private static readonly string[] CaveBiomeTileFileNames =
        {
            "floor_a.png", "floor_b.png", "floor_detail.png", "wall_face.png", "wall_top.png"
        };

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
            var isCaveBiomeTile = p.StartsWith(CaveBiomeTilesRoot) && System.Array.Exists(
                CaveBiomeTileFileNames, tileFileName => p.EndsWith("/" + tileFileName));
            s.spriteAlignment = isCaveBiomeTile ? (int)SpriteAlignment.Center : (int)SpriteAlignment.BottomCenter;
            s.spritePixelsPerUnit = isCaveBiomeTile ? CaveBiomeTilePpu
                : p.StartsWith(HousesModularRoot) ? HousesModularPpu : Ppu;
            s.filterMode = FilterMode.Point;
            s.mipmapEnabled = false;
            imp.SetTextureSettings(s);
        }
    }
}
