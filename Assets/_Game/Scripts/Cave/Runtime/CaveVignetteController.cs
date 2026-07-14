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
        // Bugfix 2026-07-10 (Play Mode CV04, BUG 1): light_shaft.png é uma imagem grande (1254x1254 @
        // 128 PPU ~= 9.8 unidades) desenhada com névoa/gradiente semi-transparente ao redor do brilho.
        // Com blending Normal (Sprites/Default) e o alpha antigo (0.55) essa névoa lavava a sala inteira
        // como um retângulo cinza. Blending ADITIVO (ver GetOrCreateLightShaftMaterial) faz a névoa
        // escura/transparente não contribuir em nada — só o núcleo claro do feixe soma luz — e a escala
        // reduzida (LightShaftScale) volta o feixe a um cone estreito perto da entrada em vez do sprite
        // no tamanho nativo. Alpha aditivo pode ficar mais alto que o alpha Normal antigo sem lavar a
        // cena porque adição satura em branco, não em cinza opaco.
        private const float LightShaftAdditiveAlpha = 0.6f;
        // Fallback só usado se nem o shader aditivo nem Sprites/Default forem resolvíveis no shader
        // aditivo (ambiente sem Legacy Shaders empacotados) — alpha bem baixo para não repetir o bug 1
        // mesmo sem aditivo disponível.
        private const float LightShaftFallbackAlpha = 0.22f;
        private static readonly Vector3 LightShaftScale = new Vector3(3f, 5f, 1f);
        // Vinheta acima de tudo (paredes/decor/inimigos/player); feixe logo abaixo dela.
        private const int VignetteSortingOrder = 50;
        private const int LightShaftSortingOrder = 40;

        private static Sprite _cachedVignetteSprite;
        private static Material _cachedLightShaftMaterial;
        private static bool _lightShaftMaterialIsAdditive;
        private static bool _lightShaftMaterialResolved;

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
            if (biomeArtResolver == null || !biomeArtResolver.TryGetLightShaftSprite(bandId, out var sprite) || sprite == null)
            {
                return;
            }

            var worldPos = CaveTileMaterializer.GridToWorld(level.Entrance, level);
            var go = new GameObject(LightShaftObjectName);
            go.transform.SetParent(parent);
            go.transform.position = worldPos;
            // Bugfix 2026-07-10 (BUG 1): sprite nativo tem ~9.8 unidades de lado; escala estreita e alta
            // volta o feixe a um cone perto da entrada em vez de cobrir a sala inteira.
            go.transform.localScale = LightShaftScale;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = CaveWorldSortingLayers.World;
            renderer.sortingOrder = LightShaftSortingOrder;

            var material = GetOrCreateLightShaftMaterial(out var isAdditive);
            if (material != null)
            {
                renderer.material = material;
            }

            var alpha = isAdditive ? LightShaftAdditiveAlpha : LightShaftFallbackAlpha;
            renderer.color = new Color(1f, 1f, 1f, alpha);

            materializedObjects.Add(go);
        }

        /// <summary>Bugfix 2026-07-10 (BUG 1) — resolve (1x, cacheado estaticamente) um material de
        /// blending ADITIVO para o feixe de luz: com aditivo, a névoa escura/semi-transparente do
        /// sprite não contribui (preto aditivo = nenhuma luz somada), só o núcleo claro brilha — sem
        /// isso o feixe lia como um quadrado cinza translúcido sobre a névoa (blending Normal soma a
        /// própria névoa como cor opaca). Fallback null-safe: shader aditivo ausente do build (Legacy
        /// Shaders não empacotados) usa Sprites/Default com alpha bem reduzido (LightShaftFallbackAlpha)
        /// — nunca lança, nunca deixa o feixe sem material.</summary>
        private static Material GetOrCreateLightShaftMaterial(out bool isAdditive)
        {
            if (!_lightShaftMaterialResolved)
            {
                _lightShaftMaterialResolved = true;

                var additiveShader = Shader.Find("Legacy Shaders/Particles/Additive")
                    ?? Shader.Find("Particles/Additive");
                if (additiveShader != null)
                {
                    _cachedLightShaftMaterial = new Material(additiveShader) { name = "CaveLightShaftAdditiveMaterial" };
                    _lightShaftMaterialIsAdditive = true;
                }
                else
                {
                    Debug.LogWarning(
                        "[Cave][Wiring] CaveVignetteController: shader aditivo " +
                        "'Legacy Shaders/Particles/Additive' nao encontrado neste build. Feixe de luz da " +
                        "entrada da cave usa Sprites/Default com alpha reduzido como fallback (visual mais " +
                        "fraco, porem sem lavar a sala). Verifique se Legacy Shaders esta incluido no " +
                        "projeto/build.");
                    var fallbackShader = Shader.Find("Sprites/Default");
                    _cachedLightShaftMaterial = fallbackShader != null
                        ? new Material(fallbackShader) { name = "CaveLightShaftFallbackMaterial" }
                        : null;
                    _lightShaftMaterialIsAdditive = false;
                }
            }

            isAdditive = _lightShaftMaterialIsAdditive;
            return _cachedLightShaftMaterial;
        }
    }
}
