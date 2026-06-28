using System.Collections.Generic;
using CindarsHope.Cave.Generation;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// Responsável por materializar tiles de chão e paredes de um CaveGeneratedLevel.
    /// Não possui estado — pode ser reutilizado entre materializações.
    /// </summary>
    internal sealed class CaveTileMaterializer
    {
        /// <summary>
        /// Materializa todos os tiles de chão do nível, adicionando-os a materializedObjects e
        /// incrementando result.CreatedFloorTiles.
        /// </summary>
        internal void MaterializeFloor(
            CaveGeneratedLevel level,
            Transform parent,
            SpriteRenderer floorPrefab,
            List<GameObject> materializedObjects,
            CaveRuntimeMaterializationResult result)
        {
            var floorParent = new GameObject("GeneratedFloor");
            floorParent.transform.SetParent(parent);
            floorParent.transform.localPosition = Vector3.zero;

            foreach (var tilePos in level.WalkableTiles)
            {
                var worldPos = GridToWorld(tilePos, level);
                GameObject floorTile;

                if (floorPrefab != null)
                {
                    var spriteRenderer = Object.Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent.transform);
                    floorTile = spriteRenderer.gameObject;
                    spriteRenderer.sortingOrder = 0;
                }
                else
                {
                    floorTile = new GameObject($"FloorTile_{tilePos.x}_{tilePos.y}");
                    floorTile.transform.SetParent(floorParent.transform);
                    floorTile.transform.position = worldPos;

                    var spriteRenderer = floorTile.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = GetBuiltinSprite();
                    spriteRenderer.color = new Color(0.4f, 0.35f, 0.3f);
                    spriteRenderer.sortingOrder = 0;
                }

                floorTile.name = $"FloorTile_{tilePos.x}_{tilePos.y}";
                materializedObjects.Add(floorTile);
                result.CreatedFloorTiles++;
            }
        }

        /// <summary>
        /// Materializa todos os tiles de parede do nível, adicionando colisão e incrementando
        /// result.CreatedWallTiles.
        /// </summary>
        internal void MaterializeWalls(
            CaveGeneratedLevel level,
            Transform parent,
            SpriteRenderer wallPrefab,
            List<GameObject> materializedObjects,
            CaveRuntimeMaterializationResult result)
        {
            var wallParent = new GameObject("GeneratedWalls");
            wallParent.transform.SetParent(parent);
            wallParent.transform.localPosition = Vector3.zero;

            foreach (var tilePos in level.WallTiles)
            {
                var worldPos = GridToWorld(tilePos, level);
                GameObject wallTile;

                if (wallPrefab != null)
                {
                    var spriteRenderer = Object.Instantiate(wallPrefab, worldPos, Quaternion.identity, wallParent.transform);
                    wallTile = spriteRenderer.gameObject;
                    spriteRenderer.sortingOrder = 1;
                }
                else
                {
                    wallTile = new GameObject($"WallTile_{tilePos.x}_{tilePos.y}");
                    wallTile.transform.SetParent(wallParent.transform);
                    wallTile.transform.position = worldPos;

                    var spriteRenderer = wallTile.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = GetBuiltinSprite();
                    spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
                    spriteRenderer.sortingOrder = 1;
                }

                wallTile.name = $"WallTile_{tilePos.x}_{tilePos.y}";

                var collider = wallTile.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;

                materializedObjects.Add(wallTile);
                result.CreatedWallTiles++;
            }
        }

        /// <summary>
        /// Converte uma posição de grid para coordenadas de mundo, centralizado na origem.
        /// </summary>
        internal static Vector3 GridToWorld(Vector2Int gridPosition, CaveGeneratedLevel level)
        {
            var offsetX = level.Width * 0.5f;
            var offsetY = level.Height * 0.5f;
            return new Vector3(gridPosition.x - offsetX, gridPosition.y - offsetY, 0f);
        }

        /// <summary>
        /// Retorna o sprite builtin do Unity Editor para uso como placeholder procedural.
        /// Retorna null em builds de produção.
        /// </summary>
        internal static Sprite GetBuiltinSprite()
        {
#if UNITY_EDITOR
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#else
            return null;
#endif
        }
    }
}
