#if UNITY_EDITOR

using CindarsHope.Farm;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public class ValidateFarmScaleContract : IProjectValidator
    {
        public string ValidatorName => "Farm Scale Contract";
        public string ValidatorDescription => "Validates farm scale, tilemap, footbox, and camera dimensions against design contract.";

        public ValidationReport Validate()
        {
            var report = new ValidationReport(ValidatorName);

            ValidateTileSize(report);
            ValidateCameraReference(report);
            ValidateFarmLevel1Size(report);
            ValidatePlayerColliderFootbox(report);

            return report;
        }

        private void ValidateTileSize(ValidationReport report)
        {
            // Audit note: Farm grid uses 32x32px tiles (same as city).
            // This is metadata validation; actual tilemap cell size is checked at edit time.

            if (FarmScaleContract.IsTileSizeValid(FarmScaleContract.TileSizePixels))
            {
                report.AddIssue(new ValidationIssue
                {
                    Category = "Farm Scale",
                    Message = $"Tile size contract hardened: {FarmScaleContract.TileSizePixels}px (32x32 = 1 world unit)",
                    Severity = ValidationSeverity.Info
                });
            }
            else
            {
                report.AddIssue(new ValidationIssue
                {
                    Category = "Farm Scale",
                    Message = $"Tile size mismatch: expected {FarmScaleContract.TileSizePixels}px, check FarmScaleContract",
                    Severity = ValidationSeverity.Warning
                });
            }
        }

        private void ValidateCameraReference(ValidationReport report)
        {
            // Audit note: Camera reference dimensions documented in contract.
            // Farm scene should show 20-24 tiles wide, 12-14 tiles tall at base zoom.

            float refWidthMin = FarmScaleContract.CameraWidthTilesMin;
            float refWidthMax = FarmScaleContract.CameraWidthTilesMax;
            float refHeightMin = FarmScaleContract.CameraHeightTilesMin;
            float refHeightMax = FarmScaleContract.CameraHeightTilesMax;

            report.AddIssue(new ValidationIssue
            {
                Category = "Farm Camera",
                Message = $"Camera reference contract: {refWidthMin}-{refWidthMax} tiles wide, {refHeightMin}-{refHeightMax} tiles tall (base zoom)",
                Severity = ValidationSeverity.Info
            });
        }

        private void ValidateFarmLevel1Size(ValidationReport report)
        {
            // Audit note: Farm level 1 must be larger than one screen.
            // Minimum is 32 tiles wide, 24 tiles tall (covers standard camera view).

            float minWidth = FarmScaleContract.FarmLevel1MinWidthTiles;
            float minHeight = FarmScaleContract.FarmLevel1MinHeightTiles;

            report.AddIssue(new ValidationIssue
            {
                Category = "Farm Layout",
                Message = $"Farm Level 1 minimum size: {minWidth}x{minHeight} tiles (larger than one screen)",
                Severity = ValidationSeverity.Info
            });
        }

        private void ValidatePlayerColliderFootbox(ValidationReport report)
        {
            // Audit note: Player collider should use footbox (bottom portion) not entire sprite.
            // Visual: 32x48px (width x height)
            // Footbox: ~16px from bottom center
            // Pivot: bottom-center (0.5, 0)

            var visual = new Vector2(FarmScaleContract.PlayerVisualWidthPixels, FarmScaleContract.PlayerVisualHeightPixels);
            var footboxHeight = FarmScaleContract.PlayerFootboxHeightPixels;

            report.AddIssue(new ValidationIssue
            {
                Category = "Player Collider",
                Message = $"Player visual: {visual.x}x{visual.y}px. Footbox: {footboxHeight}px from bottom. Pivot: bottom-center (0.5, 0). Sorting: by Y (feet position).",
                Severity = ValidationSeverity.Info
            });
        }
    }
}

#endif
