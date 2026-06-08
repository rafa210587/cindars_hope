#if UNITY_EDITOR

using CindarsHope.Farm;
using CindarsHope.EditorTools.Validation;

namespace CindarsHope.Editor.Validation
{
    public class ValidateFarmLevel1LayoutContract : IProjectValidator
    {
        public string ValidatorId => "farm_level1_layout_contract";
        public string DisplayName => "Farm Level 1 Layout Contract";

        public ValidationReport Run()
        {
            var report = new ValidationReport();

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
                Area = "Level 1 Layout",
                Code = "LEVEL1_SIZE_CONTRACT",
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
                    Area = "Fixed Anchors",
                    Code = "FONTE_ANCHOR",
                    Message = $"Fonte de Anya (FIXED): ({FarmLevel1LayoutContract.FonteAnchorX}, {FarmLevel1LayoutContract.FonteAnchorY})",
                    Severity = ValidationSeverity.Info
                });
            }

            // Lake
            if (FarmLevel1LayoutContract.IsLakeInBounds())
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "Fixed Anchors",
                    Code = "LAKE_ANCHOR",
                    Message = $"Main lake (FIXED): center ({FarmLevel1LayoutContract.LakeCenterX}, {FarmLevel1LayoutContract.LakeCenterY}), size {FarmLevel1LayoutContract.LakeWidthTiles}x{FarmLevel1LayoutContract.LakeHeightTiles} tiles",
                    Severity = ValidationSeverity.Info
                });
            }

            // Cave entrance
            if (FarmLevel1LayoutContract.IsCaveEntranceInBounds())
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "Fixed Anchors",
                    Code = "CAVE_ENTRANCE_ANCHOR",
                    Message = $"Cave entrance (FIXED): ({FarmLevel1LayoutContract.CaveEntranceX}, {FarmLevel1LayoutContract.CaveEntranceY})",
                    Severity = ValidationSeverity.Info
                });
            }

            // City exit
            if (FarmLevel1LayoutContract.IsCityExitAccessible())
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "Fixed Anchors",
                    Code = "CITY_EXIT_ANCHOR",
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
                    Area = "Farm Zones",
                    Code = "INITIAL_FIELD_ZONE",
                    Message = $"Initial field (FREE): ({FarmLevel1LayoutContract.InitialFieldStartX}, {FarmLevel1LayoutContract.InitialFieldStartY}), " +
                              $"size {FarmLevel1LayoutContract.InitialFieldWidthTiles}x{FarmLevel1LayoutContract.InitialFieldHeightTiles} tiles",
                    Severity = ValidationSeverity.Info
                });
            }

            report.AddIssue(new ValidationIssue
            {
                Area = "Farm Zones",
                Code = "HOUSE_ZONE",
                Message = $"House (MOVABLE): starts at ({FarmLevel1LayoutContract.HouseStartX}, {FarmLevel1LayoutContract.HouseStartY})",
                Severity = ValidationSeverity.Info
            });

            report.AddIssue(new ValidationIssue
            {
                Area = "Farm Zones",
                Code = "SELLPOINT_ZONE",
                Message = $"SellPoint (MOVABLE): starts at ({FarmLevel1LayoutContract.SellPointStartX}, {FarmLevel1LayoutContract.SellPointStartY})",
                Severity = ValidationSeverity.Info
            });
        }

        private void ValidateAccessibility(ValidationReport report)
        {
            report.AddIssue(new ValidationIssue
            {
                Area = "Access Contract",
                Code = "FARM_ACCESS_CONTRACT",
                Message = "Anchors fixed: Fonte, lake, cave entrance, city exit. Zones free: initial field, house (movable), SellPoint (movable), expansion areas.",
                Severity = ValidationSeverity.Info
            });
        }
    }
}

#endif
