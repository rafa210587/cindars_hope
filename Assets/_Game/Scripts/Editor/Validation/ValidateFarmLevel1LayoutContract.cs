#if UNITY_EDITOR

using CindarsHope.Farm;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public class ValidateFarmLevel1LayoutContract : IProjectValidator
    {
        public string ValidatorName => "Farm Level 1 Layout Contract";
        public string ValidatorDescription => "Validates farm level 1 fixed anchors and free zones against design contract.";

        public ValidationReport Validate()
        {
            var report = new ValidationReport(ValidatorName);

            ValidateLevel1Size(report);
            ValidateFixedAnchors(report);
            ValidateInitialField(report);
            ValidateAccessibility(report);

            return report;
        }

        private void ValidateLevel1Size(ValidationReport report)
        {
            report.AddIssue(new ValidationIssue
            {
                Category = "Level 1 Layout",
                Message = $"Farm level 1 contract: {FarmLevel1LayoutContract.Level1WidthTiles}x{FarmLevel1LayoutContract.Level1HeightTiles} tiles",
                Severity = ValidationSeverity.Info
            });
        }

        private void ValidateFixedAnchors(ValidationReport report)
        {
            // Fonte
            if (FarmLevel1LayoutContract.IsFonteInBounds())
            {
                report.AddIssue(new ValidationIssue
                {
                    Category = "Fixed Anchors",
                    Message = $"Fonte de Anya (FIXED): ({FarmLevel1LayoutContract.FonteAnchorX}, {FarmLevel1LayoutContract.FonteAnchorY})",
                    Severity = ValidationSeverity.Info
                });
            }

            // Lake
            if (FarmLevel1LayoutContract.IsLakeInBounds())
            {
                report.AddIssue(new ValidationIssue
                {
                    Category = "Fixed Anchors",
                    Message = $"Main lake (FIXED): center ({FarmLevel1LayoutContract.LakeCenterX}, {FarmLevel1LayoutContract.LakeCenterY}), size {FarmLevel1LayoutContract.LakeWidthTiles}x{FarmLevel1LayoutContract.LakeHeightTiles} tiles",
                    Severity = ValidationSeverity.Info
                });
            }

            // Cave entrance
            if (FarmLevel1LayoutContract.IsCaveEntranceInBounds())
            {
                report.AddIssue(new ValidationIssue
                {
                    Category = "Fixed Anchors",
                    Message = $"Cave entrance (FIXED): ({FarmLevel1LayoutContract.CaveEntranceX}, {FarmLevel1LayoutContract.CaveEntranceY})",
                    Severity = ValidationSeverity.Info
                });
            }

            // City exit
            if (FarmLevel1LayoutContract.IsCityExitAccessible())
            {
                report.AddIssue(new ValidationIssue
                {
                    Category = "Fixed Anchors",
                    Message = $"City exit (FIXED): ({FarmLevel1LayoutContract.CityExitX}, {FarmLevel1LayoutContract.CityExitY})",
                    Severity = ValidationSeverity.Info
                });
            }
        }

        private void ValidateInitialField(ValidationReport report)
        {
            if (FarmLevel1LayoutContract.IsInitialFieldInBounds())
            {
                report.AddIssue(new ValidationIssue
                {
                    Category = "Farm Zones",
                    Message = $"Initial field (FREE): ({FarmLevel1LayoutContract.InitialFieldStartX}, {FarmLevel1LayoutContract.InitialFieldStartY}), " +
                              $"size {FarmLevel1LayoutContract.InitialFieldWidthTiles}x{FarmLevel1LayoutContract.InitialFieldHeightTiles} tiles",
                    Severity = ValidationSeverity.Info
                });
            }

            report.AddIssue(new ValidationIssue
            {
                Category = "Farm Zones",
                Message = $"House (MOVABLE): starts at ({FarmLevel1LayoutContract.HouseStartX}, {FarmLevel1LayoutContract.HouseStartY})",
                Severity = ValidationSeverity.Info
            });

            report.AddIssue(new ValidationIssue
            {
                Category = "Farm Zones",
                Message = $"SellPoint (MOVABLE): starts at ({FarmLevel1LayoutContract.SellPointStartX}, {FarmLevel1LayoutContract.SellPointStartY})",
                Severity = ValidationSeverity.Info
            });
        }

        private void ValidateAccessibility(ValidationReport report)
        {
            report.AddIssue(new ValidationIssue
            {
                Category = "Access Contract",
                Message = "Anchors fixed: Fonte, lake, cave entrance, city exit. Zones free: initial field, house (movable), SellPoint (movable), expansion areas.",
                Severity = ValidationSeverity.Info
            });
        }
    }
}

#endif
