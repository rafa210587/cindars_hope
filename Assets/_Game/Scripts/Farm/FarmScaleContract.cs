using UnityEngine;

namespace CindarsHope.Farm
{
    /// <summary>
    /// Farm scale and tilemap contract.
    /// Defines visual/gameplay scale constants for farm and city consistency.
    /// This is metadata and validation only; systems consume these constants.
    /// </summary>
    public static class FarmScaleContract
    {
        /// <summary>
        /// Base tile size in pixels. Both farm and city use 32x32px tiles.
        /// </summary>
        public const float TileSizePixels = 32f;

        /// <summary>
        /// Player visual height in pixels. Sprite is 32 wide, 48 tall.
        /// </summary>
        public const float PlayerVisualHeightPixels = 48f;

        /// <summary>
        /// Player visual width in pixels.
        /// </summary>
        public const float PlayerVisualWidthPixels = 32f;

        /// <summary>
        /// NPC common visual height in pixels. Same as player (32x48).
        /// </summary>
        public const float NPCVisualHeightPixels = 48f;

        /// <summary>
        /// NPC common visual width in pixels.
        /// </summary>
        public const float NPCVisualWidthPixels = 32f;

        /// <summary>
        /// Player footbox height. Collider covers only bottom portion of sprite,
        /// not entire 48px visual height. Typically ~16-20px from bottom.
        /// </summary>
        public const float PlayerFootboxHeightPixels = 16f;

        /// <summary>
        /// Player footbox is positioned at the bottom-center of the sprite.
        /// Pivot should be bottom-center (0.5, 0) in sprite import.
        /// </summary>
        public const float FootboxPivotX = 0.5f;
        public const float FootboxPivotY = 0f;

        /// <summary>
        /// Sorting is by Y position (bottom of sprite / feet position).
        /// Higher Y = further back in draw order.
        /// </summary>
        public const string SortingMethod = "Y_Foot";

        /// <summary>
        /// Interaction hitbox distance from player center (forward direction).
        /// </summary>
        public const float InteractionHitboxDistance = 1f;

        /// <summary>
        /// Camera reference dimensions in tiles (not pixels).
        /// Farm scene should show approximately 20-24 tiles wide, 12-14 tiles tall.
        /// </summary>
        public const float CameraWidthTilesMin = 20f;
        public const float CameraWidthTilesMax = 24f;
        public const float CameraHeightTilesMin = 12f;
        public const float CameraHeightTilesMax = 14f;

        /// <summary>
        /// Farm level 1 minimum dimensions.
        /// Level 1 must be larger than one screen.
        /// </summary>
        public const float FarmLevel1MinWidthTiles = 32f;
        public const float FarmLevel1MinHeightTiles = 24f;

        /// <summary>
        /// Validate that tile size matches contract.
        /// </summary>
        public static bool IsTileSizeValid(float tileSize)
        {
            return Mathf.Approximately(tileSize, TileSizePixels);
        }

        /// <summary>
        /// Validate that camera dimensions are in acceptable range.
        /// </summary>
        public static bool IsCameraDimensionValid(float widthInTiles, float heightInTiles)
        {
            bool widthValid = widthInTiles >= CameraWidthTilesMin && widthInTiles <= CameraWidthTilesMax;
            bool heightValid = heightInTiles >= CameraHeightTilesMin && heightInTiles <= CameraHeightTilesMax;
            return widthValid && heightValid;
        }

        /// <summary>
        /// Validate that farm level meets minimum size contract.
        /// </summary>
        public static bool IsFarmLevel1SizeValid(float widthInTiles, float heightInTiles)
        {
            bool widthValid = widthInTiles >= FarmLevel1MinWidthTiles;
            bool heightValid = heightInTiles >= FarmLevel1MinHeightTiles;
            return widthValid && heightValid;
        }
    }
}
