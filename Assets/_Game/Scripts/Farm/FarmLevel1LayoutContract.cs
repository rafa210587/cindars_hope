using UnityEngine;

namespace CindarsHope.Farm
{
    /// <summary>
    /// Farm level 1 layout contract.
    /// Defines fixed anchors, free zones, and initial layout constraints.
    /// Anchors: Fonte, lake, cave entrance, city exit, natural borders.
    /// Free zones: initial field, buildable areas, decoration tiles.
    /// </summary>
    public static class FarmLevel1LayoutContract
    {
        /// <summary>
        /// Farm level 1 width in tiles (40 tiles).
        /// </summary>
        public const float Level1WidthTiles = 40f;

        /// <summary>
        /// Farm level 1 height in tiles (32 tiles).
        /// </summary>
        public const float Level1HeightTiles = 32f;

        /// <summary>
        /// Initial field area width in tiles (roughly 16 tiles, centered).
        /// </summary>
        public const float InitialFieldWidthTiles = 16f;

        /// <summary>
        /// Initial field area height in tiles (roughly 12 tiles).
        /// </summary>
        public const float InitialFieldHeightTiles = 12f;

        /// <summary>
        /// Initial field area start X (center-left).
        /// </summary>
        public const float InitialFieldStartX = 8f;

        /// <summary>
        /// Initial field area start Y (center-top).
        /// </summary>
        public const float InitialFieldStartY = 8f;

        /// <summary>
        /// Fixed anchor: Fonte de Anya location X (world units or tile coords).
        /// Central landmark, immovable.
        /// </summary>
        public const float FonteAnchorX = 20f;

        /// <summary>
        /// Fixed anchor: Fonte de Anya location Y.
        /// </summary>
        public const float FonteAnchorY = 16f;

        /// <summary>
        /// Fixed anchor: Main lake bounds center X.
        /// </summary>
        public const float LakeCenterX = 5f;

        /// <summary>
        /// Fixed anchor: Main lake bounds center Y.
        /// </summary>
        public const float LakeCenterY = 24f;

        /// <summary>
        /// Fixed anchor: Lake width in tiles.
        /// </summary>
        public const float LakeWidthTiles = 8f;

        /// <summary>
        /// Fixed anchor: Lake height in tiles.
        /// </summary>
        public const float LakeHeightTiles = 6f;

        /// <summary>
        /// Fixed anchor: Cave entrance location X.
        /// </summary>
        public const float CaveEntranceX = 32f;

        /// <summary>
        /// Fixed anchor: Cave entrance location Y.
        /// </summary>
        public const float CaveEntranceY = 28f;

        /// <summary>
        /// Fixed anchor: City exit location X.
        /// </summary>
        public const float CityExitX = 36f;

        /// <summary>
        /// Fixed anchor: City exit location Y.
        /// </summary>
        public const float CityExitY = 2f;

        /// <summary>
        /// House location X (starting position, may move with building mode).
        /// </summary>
        public const float HouseStartX = 2f;

        /// <summary>
        /// House location Y (starting position).
        /// </summary>
        public const float HouseStartY = 4f;

        /// <summary>
        /// SellPoint location X (starting position, may move with preservation).
        /// </summary>
        public const float SellPointStartX = 36f;

        /// <summary>
        /// SellPoint location Y (starting position).
        /// </summary>
        public const float SellPointStartY = 18f;

        /// <summary>
        /// Validate that level 1 dimensions are correct.
        /// </summary>
        public static bool IsLevel1SizeValid(float widthTiles, float heightTiles)
        {
            return Mathf.Approximately(widthTiles, Level1WidthTiles) &&
                   Mathf.Approximately(heightTiles, Level1HeightTiles);
        }

        /// <summary>
        /// Validate that initial field is within level 1 bounds.
        /// </summary>
        public static bool IsInitialFieldInBounds()
        {
            float fieldEndX = InitialFieldStartX + InitialFieldWidthTiles;
            float fieldEndY = InitialFieldStartY + InitialFieldHeightTiles;

            return InitialFieldStartX >= 0 && fieldEndX <= Level1WidthTiles &&
                   InitialFieldStartY >= 0 && fieldEndY <= Level1HeightTiles;
        }

        /// <summary>
        /// Validate that Fonte is in level 1 bounds.
        /// </summary>
        public static bool IsFonteInBounds()
        {
            return FonteAnchorX >= 0 && FonteAnchorX < Level1WidthTiles &&
                   FonteAnchorY >= 0 && FonteAnchorY < Level1HeightTiles;
        }

        /// <summary>
        /// Validate that lake is in level 1 bounds.
        /// </summary>
        public static bool IsLakeInBounds()
        {
            float lakeStartX = LakeCenterX - LakeWidthTiles / 2f;
            float lakeStartY = LakeCenterY - LakeHeightTiles / 2f;
            float lakeEndX = lakeStartX + LakeWidthTiles;
            float lakeEndY = lakeStartY + LakeHeightTiles;

            return lakeStartX >= 0 && lakeEndX <= Level1WidthTiles &&
                   lakeStartY >= 0 && lakeEndY <= Level1HeightTiles;
        }

        /// <summary>
        /// Validate that cave entrance is in level 1 bounds.
        /// </summary>
        public static bool IsCaveEntranceInBounds()
        {
            return CaveEntranceX >= 0 && CaveEntranceX < Level1WidthTiles &&
                   CaveEntranceY >= 0 && CaveEntranceY < Level1HeightTiles;
        }

        /// <summary>
        /// Validate that city exit is accessible (not blocked by anchors).
        /// </summary>
        public static bool IsCityExitAccessible()
        {
            return CityExitX >= 0 && CityExitX < Level1WidthTiles &&
                   CityExitY >= 0 && CityExitY < Level1HeightTiles;
        }
    }
}
