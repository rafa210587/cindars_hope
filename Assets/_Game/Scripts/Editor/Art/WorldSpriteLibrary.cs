using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Art
{
    /// <summary>
    /// Carrega sprites de MUNDO (staged em Assets/_Game/Art/Generated/World/&lt;categoria&gt;/&lt;nome&gt;.png)
    /// para uso pelos scene creators em tempo de editor. Centraliza o path e emite um wiring-error
    /// claro quando o sprite esta ausente, deixando o chamador aplicar seu proprio fallback
    /// (ex.: GetBuiltinSprite) sem mascarar a falha.
    ///
    /// A arte e importada com config pixel-art automatica pelo GeneratedSpriteImporter
    /// (tudo sob Assets/_Game/Art/Generated/). Staging: art/world_gpt/_stage_world_to_assets.py.
    ///
    /// Categorias: "tiles" (chao/textura), "building" (paredes/telhado/porta/janela),
    /// "trees", "foliage", "props", "interior", "animals", "crops".
    /// </summary>
    public static class WorldSpriteLibrary
    {
        private const string Root = "Assets/_Game/Art/Generated/World";

        /// <summary>
        /// Retorna o sprite de mundo para (categoria, nome), ou null (com wiring-error logado)
        /// se ausente. O chamador deve tratar null com fallback proprio.
        /// </summary>
        // Paths ja garantidos como Full Rect nesta sessao de editor (evita reimport repetido).
        private static readonly HashSet<string> _fullRectEnsured = new HashSet<string>();

        public static Sprite Load(string category, string name)
        {
            var path = $"{Root}/{category}/{name}.png";
            EnsureFullRect(path);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning(
                    $"[WorldSpriteLibrary] Sprite de mundo ausente: '{path}'. " +
                    "Rode art/world_gpt/_stage_world_to_assets.py e reimporte em Unity, " +
                    "ou confira categoria/nome. O chamador usara fallback.");
            }
            return sprite;
        }

        // Garante Mesh Type = Full Rect no asset (necessario p/ SpriteRenderer.drawMode = Tiled).
        // Se o sprite foi importado com Tight (default antigo), reimporta uma vez com FullRect —
        // evita o warning "Sprite Tiling might not appear correctly ... not generated with Full Rect"
        // sem exigir reimport manual do humano.
        private static void EnsureFullRect(string path)
        {
            if (_fullRectEnsured.Contains(path)) return;
            _fullRectEnsured.Add(path);
            if (AssetImporter.GetAtPath(path) is not TextureImporter imp) return;
            var s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            if (s.spriteMeshType == SpriteMeshType.FullRect) return;
            s.spriteMeshType = SpriteMeshType.FullRect;
            imp.SetTextureSettings(s);
            imp.SaveAndReimport();
        }

        public static Sprite Ground(string name) => Load("tiles", name);
        public static Sprite Building(string name) => Load("building", name);

        /// <summary>
        /// Pecas de casa modular (roof/walls/door A-B-C, 64 px/tile) em
        /// Assets/_Game/Art/Generated/World/building/houses_modular/&lt;name&gt;.png.
        /// </summary>
        public static Sprite HouseModular(string name) => Load("building/houses_modular", name);
        public static Sprite Tree(string name) => Load("trees", name);
        public static Sprite Foliage(string name) => Load("foliage", name);
        public static Sprite Prop(string name) => Load("props", name);
        public static Sprite Interior(string name) => Load("interior", name);
        public static Sprite Animal(string name) => Load("animals", name);
        public static Sprite Crop(string name) => Load("crops", name);

        /// <summary>
        /// Sprite bespoke de um LOCAL/prédio inteiro (landmark) em
        /// Assets/_Game/Art/Generated/World/locations/&lt;id&gt;/&lt;id&gt;.png — um prédio por pasta.
        /// Retorna null (sem erro barulhento) quando o local ainda não tem arte bespoke: o chamador
        /// cai no kit modular genérico. É a fonte única; o manifest.locations.json rastreia o status.
        /// </summary>
        public static Sprite Location(string id)
        {
            var path = $"{Root}/locations/{id}/{id}.png";
            EnsureFullRect(path);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>
        /// Configura um SpriteRenderer para cobrir uma area de chao com a textura dada.
        /// Usa drawMode Tiled quando a area cabe no limite de mesh do Unity (~127 tiles/eixo);
        /// acima disso cai para Simple (esticado) como STOPGAP — evita o erro
        /// "Cannot generate 9 slice most likely because the size is too big" que estoura o mesh.
        /// A solucao definitiva para chao grande e migrar para Tilemap (best practice 2D).
        /// </summary>
        public static void ConfigureGround(SpriteRenderer sr, Transform tf, Sprite tile, Vector2 sizeUnits)
        {
            sr.sprite = tile;
            sr.color = Color.white;
            float tileUnits = tile.pixelsPerUnit > 0f ? tile.rect.width / tile.pixelsPerUnit : 0.5f;
            if (tileUnits <= 0f) tileUnits = 0.5f;
            int tilesX = Mathf.CeilToInt(sizeUnits.x / tileUnits);
            int tilesY = Mathf.CeilToInt(sizeUnits.y / tileUnits);
            if (tilesX <= 100 && tilesY <= 100)
            {
                tf.localScale = Vector3.one;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.tileMode = SpriteTileMode.Continuous;
                sr.size = sizeUnits;
            }
            else
            {
                // Area grande demais para Tiled -> estica um unico quad (sem explosao de mesh).
                sr.drawMode = SpriteDrawMode.Simple;
                tf.localScale = new Vector3(sizeUnits.x, sizeUnits.y, 1f);
            }
        }
    }
}
