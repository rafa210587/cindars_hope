#if UNITY_EDITOR

using CindarsHope.Farm;
using CindarsHope.EditorTools.Validation;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// Valida o FarmLevel1LayoutContract v4 (48x34 tiles, origem centrada).
    /// Atualizado em 2026-06-26 (spec_farm_scene_relayout_v4).
    /// </summary>
    public class ValidateFarmLevel1LayoutContract : IProjectValidator
    {
        public string ValidatorId => "farm_level1_layout_contract";
        public string DisplayName => "Farm Level 1 Layout Contract v4";

        public ValidationReport Run()
        {
            var report = new ValidationReport();

            ValidateLevel1Size(report);
            ValidateFixedAnchors(report);
            ValidateNewAnchorsV4(report);
            ValidateAccessibility(report);

            return report;
        }

        private void ValidateLevel1Size(ValidationReport report)
        {
            report.AddIssue(new ValidationIssue
            {
                Area = "Level 1 Layout v4",
                Code = "LEVEL1_SIZE_CONTRACT",
                Message = $"Farm level 1 contract v4: {FarmLevel1LayoutContract.Level1WidthTiles}x{FarmLevel1LayoutContract.Level1HeightTiles} tiles" +
                          $" (bounds x=[{FarmLevel1LayoutContract.MinX},{FarmLevel1LayoutContract.MaxX}]" +
                          $" y=[{FarmLevel1LayoutContract.MinY},{FarmLevel1LayoutContract.MaxY}])",
                Severity = ValidationSeverity.Info
            });

            bool sizeOk = FarmLevel1LayoutContract.IsLevel1SizeValid(
                FarmLevel1LayoutContract.Level1WidthTiles, FarmLevel1LayoutContract.Level1HeightTiles);

            if (!sizeOk)
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "Level 1 Layout v4",
                    Code = "LEVEL1_SIZE_MISMATCH",
                    Message = "Farm level 1 dimensoes nao batem com o contrato v4 (48x34).",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        private void ValidateFixedAnchors(ValidationReport report)
        {
            // Fonte
            if (FarmLevel1LayoutContract.IsFonteInBounds())
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "Fixed Anchors v4",
                    Code = "FONTE_ANCHOR",
                    Message = $"Fonte de Anya (FIXED): ({FarmLevel1LayoutContract.FonteAnchorX}, {FarmLevel1LayoutContract.FonteAnchorY})",
                    Severity = ValidationSeverity.Info
                });
            }
            else
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "Fixed Anchors v4",
                    Code = "FONTE_ANCHOR_OUT_OF_BOUNDS",
                    Message = $"Fonte de Anya fora dos bounds: ({FarmLevel1LayoutContract.FonteAnchorX}, {FarmLevel1LayoutContract.FonteAnchorY})",
                    Severity = ValidationSeverity.Error
                });
            }

            // Lake
            if (FarmLevel1LayoutContract.IsLakeInBounds())
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "Fixed Anchors v4",
                    Code = "LAKE_ANCHOR",
                    Message = $"Lago (FIXED): centro ({FarmLevel1LayoutContract.LakeCenterX}, {FarmLevel1LayoutContract.LakeCenterY}), tamanho {FarmLevel1LayoutContract.LakeWidthTiles}x{FarmLevel1LayoutContract.LakeHeightTiles} tiles",
                    Severity = ValidationSeverity.Info
                });
            }

            // Cave entrance
            if (FarmLevel1LayoutContract.IsCaveEntranceInBounds())
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "Fixed Anchors v4",
                    Code = "CAVE_ENTRANCE_ANCHOR",
                    Message = $"Entrada da caverna (canto NO, FIXED): ({FarmLevel1LayoutContract.CaveEntranceX}, {FarmLevel1LayoutContract.CaveEntranceY})",
                    Severity = ValidationSeverity.Info
                });
            }
            else
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "Fixed Anchors v4",
                    Code = "CAVE_ENTRANCE_OUT_OF_BOUNDS",
                    Message = $"Entrada da caverna fora dos bounds: ({FarmLevel1LayoutContract.CaveEntranceX}, {FarmLevel1LayoutContract.CaveEntranceY})",
                    Severity = ValidationSeverity.Error
                });
            }

            // City exit
            if (FarmLevel1LayoutContract.IsCityExitAccessible())
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "Fixed Anchors v4",
                    Code = "CITY_EXIT_ANCHOR",
                    Message = $"Saida da cidade (extrema direita, FIXED): ({FarmLevel1LayoutContract.CityExitX}, {FarmLevel1LayoutContract.CityExitY})",
                    Severity = ValidationSeverity.Info
                });
            }
        }

        private void ValidateNewAnchorsV4(ValidationReport report)
        {
            // Mountain base
            if (FarmLevel1LayoutContract.IsMountainInBounds())
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "New Anchors v4",
                    Code = "MOUNTAIN_BASE_ANCHOR",
                    Message = $"Montanha (colisao norte, FIXED): base em y={FarmLevel1LayoutContract.MountainBaseY}",
                    Severity = ValidationSeverity.Info
                });
            }

            // Bridge
            if (FarmLevel1LayoutContract.IsBridgeInBounds())
            {
                report.AddIssue(new ValidationIssue
                {
                    Area = "New Anchors v4",
                    Code = "BRIDGE_CENTER_ANCHOR",
                    Message = $"Ponte (andavel, FIXED): centro ({FarmLevel1LayoutContract.BridgeCenterX}, {FarmLevel1LayoutContract.BridgeCenterY})",
                    Severity = ValidationSeverity.Info
                });
            }

            // Evolution board
            report.AddIssue(new ValidationIssue
            {
                Area = "New Anchors v4",
                Code = "EVOLUTION_BOARD_ANCHOR",
                Message = $"Quadro de Evolucoes (FIXED): ({FarmLevel1LayoutContract.EvolutionBoardX}, {FarmLevel1LayoutContract.EvolutionBoardY})",
                Severity = ValidationSeverity.Info
            });

            // Spawn anchors
            report.AddIssue(new ValidationIssue
            {
                Area = "New Anchors v4",
                Code = "SPAWN_ANCHORS",
                Message = $"Spawns: default=({FarmLevel1LayoutContract.DefaultSpawnX},{FarmLevel1LayoutContract.DefaultSpawnY})" +
                          $" from_town=({FarmLevel1LayoutContract.SpawnFromTownX},{FarmLevel1LayoutContract.SpawnFromTownY})" +
                          $" from_cave=({FarmLevel1LayoutContract.SpawnFromCaveX},{FarmLevel1LayoutContract.SpawnFromCaveY})",
                Severity = ValidationSeverity.Info
            });
        }

        private void ValidateAccessibility(ValidationReport report)
        {
            report.AddIssue(new ValidationIssue
            {
                Area = "Access Contract v4",
                Code = "FARM_ACCESS_CONTRACT_V4",
                Message = "Contrato v4: montanha(N) intransponivel; caverna(NO) embutida na montanha; " +
                          "base+casa(L) junto a saida da cidade; bosque denso(NO); " +
                          "construcoes(centro-sul); lago+rio+ponte(SE). Solo aravel por tile (Spec B).",
                Severity = ValidationSeverity.Info
            });
        }
    }
}

#endif
