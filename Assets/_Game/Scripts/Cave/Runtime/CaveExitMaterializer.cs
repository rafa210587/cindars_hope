using System.Collections.Generic;
using CindarsHope.Cave.Art;
using CindarsHope.Cave.Generation;
using CindarsHope.Combat;
using CindarsHope.DebugTools;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// Responsável por materializar as saídas (BackExit e ForwardExit) de um CaveGeneratedLevel.
    /// Depende de CaveTileMaterializer.GetBuiltinSprite para o placeholder procedural.
    /// </summary>
    internal sealed class CaveExitMaterializer
    {
        /// <summary>
        /// Materializa a entrada (BackExit) e a saída (ForwardExit) do nível como portais.
        /// Preenche backExit e forwardExit; atualiza result.BackExitPosition e ForwardExitPosition.
        /// biomeArtResolver é opcional (null = comportamento atual, cores placeholder).
        /// </summary>
        internal void Materialize(
            CaveGeneratedLevel level,
            Transform parent,
            CaveExitPortal exitPortalPrefab,
            CaveRunManager caveRunManager,
            CaveLevelRuntimeController levelController,
            List<GameObject> materializedObjects,
            CaveRuntimeMaterializationResult result,
            out CaveExitPortal backExit,
            out CaveExitPortal forwardExit,
            CaveBiomeArtResolver biomeArtResolver = null)
        {
            var bandId = CaveBiomeArtDebug.ResolveBandForArt(Runtime.CaveBandScaling.BandForLevel(level.CaveLevel));
            Sprite exitUpSprite = null;
            Sprite exitDownSprite = null;
            if (biomeArtResolver != null)
            {
                biomeArtResolver.TryGetExitSprite(bandId, isForwardExit: false, out exitUpSprite);
                biomeArtResolver.TryGetExitSprite(bandId, isForwardExit: true, out exitDownSprite);
            }

            var portalsParent = new GameObject("GeneratedExits");
            portalsParent.transform.SetParent(parent);
            portalsParent.transform.localPosition = Vector3.zero;

            // BackExit na posição de entrada
            var backExitPos = CaveTileMaterializer.GridToWorld(level.Entrance, level);
            if (exitPortalPrefab != null)
            {
                backExit = Object.Instantiate(exitPortalPrefab, backExitPos, Quaternion.identity, portalsParent.transform);
                backExit.gameObject.name = "GeneratedBackExit";
                backExit.InitializeBackExit(caveRunManager, levelController);
            }
            else
            {
                var backExitGO = new GameObject("GeneratedBackExit");
                backExitGO.transform.SetParent(portalsParent.transform);
                backExitGO.transform.position = backExitPos;

                var spriteRenderer = backExitGO.AddComponent<SpriteRenderer>();
                // spec_cave_biome_art_profiles_runtime (CV01): sprite do bioma vence quando presente;
                // ausência mantém o placeholder de cor atual (fallback-first).
                if (exitUpSprite != null)
                {
                    spriteRenderer.sprite = exitUpSprite;
                    spriteRenderer.color = Color.white;
                }
                else
                {
                    spriteRenderer.sprite = CaveTileMaterializer.GetBuiltinSprite();
                    spriteRenderer.color = new Color(0f, 1f, 1f, 0.7f);
                }
                spriteRenderer.sortingOrder = 0;
                spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                spriteRenderer.sortingLayerName = CaveWorldSortingLayers.World;

                var collider = backExitGO.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
                collider.isTrigger = true;

                backExit = backExitGO.AddComponent<CaveExitPortal>();
                backExit.InitializeBackExit(caveRunManager, levelController);
            }

            if (backExit != null)
            {
                var collider = backExit.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    collider = backExit.gameObject.AddComponent<BoxCollider2D>();
                    collider.size = Vector2.one;
                    collider.isTrigger = true;
                }
                materializedObjects.Add(backExit.gameObject);
                result.BackExitPosition = backExitPos;
            }

            // ForwardExit na posição de saída
            var forwardExitPos = CaveTileMaterializer.GridToWorld(level.Exit, level);
            if (exitPortalPrefab != null)
            {
                forwardExit = Object.Instantiate(exitPortalPrefab, forwardExitPos, Quaternion.identity, portalsParent.transform);
                forwardExit.gameObject.name = "GeneratedForwardExit";
                forwardExit.InitializeForwardExit(caveRunManager, levelController);
            }
            else
            {
                var forwardExitGO = new GameObject("GeneratedForwardExit");
                forwardExitGO.transform.SetParent(portalsParent.transform);
                forwardExitGO.transform.position = forwardExitPos;

                var spriteRenderer = forwardExitGO.AddComponent<SpriteRenderer>();
                // spec_cave_biome_art_profiles_runtime (CV01): sprite do bioma vence quando presente;
                // ausência mantém o placeholder de cor atual (fallback-first).
                if (exitDownSprite != null)
                {
                    spriteRenderer.sprite = exitDownSprite;
                    spriteRenderer.color = Color.white;
                }
                else
                {
                    spriteRenderer.sprite = CaveTileMaterializer.GetBuiltinSprite();
                    spriteRenderer.color = new Color(1f, 0f, 1f, 0.7f);
                }
                spriteRenderer.sortingOrder = 0;
                spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                spriteRenderer.sortingLayerName = CaveWorldSortingLayers.World;

                var collider = forwardExitGO.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
                collider.isTrigger = true;

                forwardExit = forwardExitGO.AddComponent<CaveExitPortal>();
                forwardExit.InitializeForwardExit(caveRunManager, levelController);
            }

            if (forwardExit != null)
            {
                var collider = forwardExit.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    collider = forwardExit.gameObject.AddComponent<BoxCollider2D>();
                    collider.size = Vector2.one;
                    collider.isTrigger = true;
                }
                materializedObjects.Add(forwardExit.gameObject);
                result.ForwardExitPosition = forwardExitPos;
            }

            CombatLog.Log(
                $"CaveExitMaterializer: BackExit at ({level.Entrance.x}, {level.Entrance.y}), ForwardExit at ({level.Exit.x}, {level.Exit.y}).",
                null);
        }
    }
}
