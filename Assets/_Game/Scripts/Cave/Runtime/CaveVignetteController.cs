using System.Collections.Generic;
using CindarsHope.Cave.Art;
using CindarsHope.Cave.Generation;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// spec_cave_visual_polish_runtime (CV04), T005 — mood de luz FAKE (sem URP 2D Lights): vinheta
    /// ESTÁTICA que escurece progressivamente as células de chão/parede quanto mais perto da borda
    /// externa do NÍVEL (não da câmera/tela) + 1 sprite de feixe de luz (light_shaft) perto da entrada.
    /// Ambos são overlays criados 1x por materialização (sem custo por-frame, nenhum Update) — puramente
    /// de apresentação, nunca influenciam layout/spawn/loot/snapshot (cave-stable-run/ADR-0005): não
    /// participam do CaveEnvironmentElementPlan nem do snapshot de stable-run, são recomputados a cada
    /// materialização a partir do CaveGeneratedLevel/CaveBiomeArtProfileSO atuais (mesmo espírito do
    /// overlay de borda de rocha do CaveTileMaterializer).
    ///
    /// A textura de gradiente é gerada 1x em código (nunca via Resources/AssetDatabase — não há PNG
    /// novo para a vinheta) e cacheada estaticamente para reuso entre materializações/níveis.
    /// </summary>
    internal static class CaveVignetteController
    {
        private const int VignetteTextureSize = 64;
        private const float VignetteMaxAlpha = 0.55f;
        // Fração do "raio" (0..1, distância normalizada até a borda do retângulo do nível) a partir da
        // qual a vinheta começa a escurecer — abaixo disso a célula fica com alpha 0 (sem escurecimento).
        private const float VignetteInnerEdgeFraction = 0.35f;
        private const string VignetteObjectName = "CaveVignetteOverlay";
        private const string LightShaftObjectName = "CaveEntranceLightShaft";
        // O PNG anterior contém um xadrez claro opaco em todos os pixels. Um sprite gerado com alpha
        // real evita que o fundo seja ampliado sobre a sala e independe de shader aditivo no build.
        private const int LightShaftTextureSize = 64;
        private static readonly Vector3 LightShaftScale = new Vector3(3f, 5f, 1f);
        // Vinheta acima de tudo (paredes/decor/inimigos/player); feixe logo abaixo dela.
        private const int VignetteSortingOrder = 50;
        private const int LightShaftSortingOrder = 40;

        private static Sprite _cachedVignetteSprite;
        private static Sprite _cachedLightShaftSprite;

        /// <summary>Cria a vinheta estática do nível + (se o profile do bioma fornecer) o feixe de luz
        /// perto da entrada. Ambos são adicionados a materializedObjects (limpos/recriados a cada
        /// materialização, mesmo ciclo de vida dos tiles/decor). Null-safe: level/parent nulos = no-op;
        /// resolver ausente ou sem LightShaftSprite = só a vinheta é criada (sem feixe).</summary>
        internal static void ApplyVignetteAndLightShaft(
            CaveGeneratedLevel level,
            Transform parent,
            CaveBiomeArtResolver biomeArtResolver,
            int bandId,
            List<GameObject> materializedObjects)
        {
            if (level == null || parent == null || materializedObjects == null)
            {
                return;
            }

            CreateVignetteOverlay(level, parent, materializedObjects);
            CreateLightShaft(level, parent, biomeArtResolver, bandId, materializedObjects);
        }

        private static void CreateVignetteOverlay(CaveGeneratedLevel level, Transform parent, List<GameObject> materializedObjects)
        {
            var sprite = GetOrCreateVignetteSprite();
            if (sprite == null)
            {
                return;
            }

            var go = new GameObject(VignetteObjectName);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = CaveWorldSortingLayers.World;
            renderer.sortingOrder = VignetteSortingOrder;
            renderer.color = Color.white; // alpha/tint já vêm da textura do gradiente (preto translúcido).

            // Sprite base tem 1x1 unidade de mundo (pixelsPerUnit = VignetteTextureSize) — escala para
            // cobrir o nível inteiro (+ margem pequena para não deixar frestas sem vinheta no canto).
            var scaleX = Mathf.Max(1f, level.Width) * 1.1f;
            var scaleY = Mathf.Max(1f, level.Height) * 1.1f;
            go.transform.localScale = new Vector3(scaleX, scaleY, 1f);

            materializedObjects.Add(go);
        }

        private static Sprite GetOrCreateVignetteSprite()
        {
            if (_cachedVignetteSprite != null)
            {
                return _cachedVignetteSprite;
            }

            var texture = new Texture2D(VignetteTextureSize, VignetteTextureSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "CaveVignetteGeneratedTexture"
            };

            var center = (VignetteTextureSize - 1) * 0.5f;
            for (var y = 0; y < VignetteTextureSize; y++)
            {
                for (var x = 0; x < VignetteTextureSize; x++)
                {
                    var dx = Mathf.Abs(x - center) / center;
                    var dy = Mathf.Abs(y - center) / center;
                    var edgeDistance = Mathf.Max(dx, dy); // 0 no centro do nível, 1 na borda do retângulo.
                    var t = Mathf.InverseLerp(VignetteInnerEdgeFraction, 1f, edgeDistance);
                    var alpha = Mathf.Clamp01(t) * VignetteMaxAlpha;
                    texture.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
                }
            }

            texture.Apply(false);
            _cachedVignetteSprite = Sprite.Create(
                texture,
                new Rect(0, 0, VignetteTextureSize, VignetteTextureSize),
                new Vector2(0.5f, 0.5f),
                VignetteTextureSize);
            _cachedVignetteSprite.name = "CaveVignetteGeneratedSprite";
            return _cachedVignetteSprite;
        }

        private static void CreateLightShaft(
            CaveGeneratedLevel level,
            Transform parent,
            CaveBiomeArtResolver biomeArtResolver,
            int bandId,
            List<GameObject> materializedObjects)
        {
            if (biomeArtResolver == null || !biomeArtResolver.TryGetLightShaftSprite(bandId, out var sourceSprite) || sourceSprite == null)
            {
                return;
            }

            var sprite = GetOrCreateLightShaftSprite();
            if (sprite == null)
            {
                return;
            }

            var worldPos = CaveTileMaterializer.GridToWorld(level.Entrance, level);
            var go = new GameObject(LightShaftObjectName);
            go.transform.SetParent(parent);
            go.transform.position = worldPos;
            // Sprite gerado tem 1x1 unidade; o feixe ocupa apenas 3x5 unidades perto da entrada.
            go.transform.localScale = LightShaftScale;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = CaveWorldSortingLayers.World;
            renderer.sortingOrder = LightShaftSortingOrder;

            renderer.color = Color.white;

            materializedObjects.Add(go);
        }

        /// <summary>Textura pequena, transparente e determinística; não depende do PNG opaco do
        /// profile nem de shader Legacy. O profile ainda controla se há feixe neste bioma.</summary>
        internal static Sprite GetOrCreateLightShaftSprite()
        {
            if (_cachedLightShaftSprite != null)
            {
                return _cachedLightShaftSprite;
            }

            var texture = new Texture2D(LightShaftTextureSize, LightShaftTextureSize, TextureFormat.RGBA32, false)
            {
                name = "CaveLightShaftGeneratedTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < LightShaftTextureSize; y++)
            {
                var depth = 1f - y / (float)(LightShaftTextureSize - 1);
                var halfWidth = Mathf.Lerp(0.035f, 0.42f, depth);
                var verticalFade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(depth / 0.08f))
                    * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - depth) / 0.22f));
                for (var x = 0; x < LightShaftTextureSize; x++)
                {
                    var horizontalDistance = Mathf.Abs(x / (float)(LightShaftTextureSize - 1) - 0.5f);
                    var sideFade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((halfWidth - horizontalDistance) / 0.12f));
                    var alpha = 0.28f * verticalFade * sideFade;
                    texture.SetPixel(x, y, new Color(1f, 0.88f, 0.61f, alpha));
                }
            }

            texture.Apply(false);
            _cachedLightShaftSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, LightShaftTextureSize, LightShaftTextureSize),
                new Vector2(0.5f, 0.5f),
                LightShaftTextureSize);
            _cachedLightShaftSprite.name = "CaveLightShaftGeneratedSprite";
            return _cachedLightShaftSprite;
        }
    }
}
