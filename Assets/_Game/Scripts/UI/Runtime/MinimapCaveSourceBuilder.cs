using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using UnityEngine;

namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_38 — converte (PURAMENTE, sem tocar a cena) um <see cref="VisitedLevelSnapshot"/> do
    /// estado de nível do stable-run em um <see cref="MinimapGridSource"/> que o renderer consome:
    ///  - WalkableTilesList → Floor; WallTilesList → Wall; células de armadilha → Hazard;
    ///  - reveal = RevealedCells do snapshot (fog intra-run, ADR-0005);
    ///  - ícones: escadas (entrada=sub, saída=desc) sempre que a célula estiver revelada.
    ///
    /// NÃO reconstrói layout nem reroll: lê o grid já materializado no snapshot (regra de não
    /// duplicação da spec). EditMode-testável (recebe o snapshot, devolve a source).
    /// </summary>
    public static class MinimapCaveSourceBuilder
    {
        /// <summary>
        /// fable_38 — passo de fog: revela no snapshot as células num raio do player (6 padrão, 9
        /// com a lantern_of_true_sight — hook F31) e devolve quantas células NOVAS foram reveladas.
        /// Escreve EXCLUSIVAMENTE no snapshot do stable-run (ADR-0005). Determinístico (CA-1).
        /// </summary>
        public static int RevealAroundPlayer(
            VisitedLevelSnapshot snapshot, Vector2Int playerCell, bool hasLanternOfTrueSight)
        {
            if (snapshot == null)
            {
                return 0;
            }

            int radius = MinimapRenderer.ResolveRevealRadius(hasLanternOfTrueSight);
            var revealed = new List<Vector2Int>();
            MinimapRenderer.CollectRevealedCells(
                playerCell, radius, snapshot.Width, snapshot.Height, revealed);
            return snapshot.RevealCells(revealed);
        }

        public static MinimapGridSource Build(VisitedLevelSnapshot snapshot, Vector2Int playerCell)
        {
            if (snapshot == null)
            {
                return new MinimapGridSource(
                    0, 0, null, null, useFog: true, centerOnPlayer: true,
                    playerCell, System.Array.Empty<MinimapIcon>());
            }

            var cells = new Dictionary<Vector2Int, MinimapCellKind>();

            if (snapshot.WalkableTilesList != null)
            {
                foreach (var c in snapshot.WalkableTilesList)
                {
                    cells[c] = MinimapCellKind.Floor;
                }
            }

            if (snapshot.WallTilesList != null)
            {
                foreach (var c in snapshot.WallTilesList)
                {
                    if (!cells.ContainsKey(c))
                    {
                        cells[c] = MinimapCellKind.Wall;
                    }
                }
            }

            // Armadilhas conhecidas pintam como hazard placeholder (apenas as já armadas/telegrafadas;
            // disarmed/triggered viram chão de novo). Estado lido do snapshot, não inferido da cena.
            if (snapshot.TrapStates != null)
            {
                foreach (var trap in snapshot.TrapStates)
                {
                    if (trap == null)
                    {
                        continue;
                    }

                    // 0=Armed, 1=Telegraphing (pinta hazard); 2=Triggered, 3=Disarmed (volta a chão).
                    if (trap.State <= 1)
                    {
                        cells[trap.Cell] = MinimapCellKind.Hazard;
                    }
                }
            }

            var icons = new List<MinimapIcon>
            {
                new MinimapIcon(playerCell, MinimapIconKind.Player)
            };

            var entrance = Vector2Int.RoundToInt(snapshot.EntrancePosition);
            var exit = Vector2Int.RoundToInt(snapshot.ExitPosition);
            icons.Add(new MinimapIcon(entrance, MinimapIconKind.StairsUp));
            icons.Add(new MinimapIcon(exit, MinimapIconKind.StairsDown));

            return new MinimapGridSource(
                snapshot.Width,
                snapshot.Height,
                cells,
                snapshot.RevealedCells,
                useFog: true,
                centerOnPlayer: true,
                playerCell,
                icons);
        }
    }
}
