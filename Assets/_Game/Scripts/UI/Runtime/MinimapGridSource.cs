using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_38 — view PURO e imutável do estado que o <see cref="MinimapRenderer"/> precisa para
    /// pintar um frame: dimensões do grid, classificação por célula, conjunto de células reveladas
    /// (fog), flag de fog/centragem, célula do player e a lista de ícones.
    ///
    /// É um SNAPSHOT de leitura montado pelo widget a cada update (4×/s) a partir das fontes já
    /// existentes (grid do nível de caverna via snapshot do stable-run; posições conhecidas dos
    /// geradores em Town/Farm). Não toca o mundo nem reconstrói layout — só lê. Sem refs Unity de
    /// scene objects (apenas Vector2Int/coleções), o que o mantém EditMode-testável.
    /// </summary>
    public sealed class MinimapGridSource
    {
        private readonly Dictionary<Vector2Int, MinimapCellKind> _cells;
        private readonly HashSet<Vector2Int> _revealed;

        public int Width { get; }
        public int Height { get; }
        public bool UseFog { get; }
        public bool CenterOnPlayer { get; }
        public Vector2Int PlayerCell { get; }
        public IReadOnlyList<MinimapIcon> Icons { get; }

        public MinimapGridSource(
            int width,
            int height,
            IReadOnlyDictionary<Vector2Int, MinimapCellKind> cells,
            IEnumerable<Vector2Int> revealedCells,
            bool useFog,
            bool centerOnPlayer,
            Vector2Int playerCell,
            IReadOnlyList<MinimapIcon> icons)
        {
            Width = Mathf.Max(0, width);
            Height = Mathf.Max(0, height);
            UseFog = useFog;
            CenterOnPlayer = centerOnPlayer;
            PlayerCell = playerCell;
            Icons = icons ?? System.Array.Empty<MinimapIcon>();

            _cells = new Dictionary<Vector2Int, MinimapCellKind>();
            if (cells != null)
            {
                foreach (var kvp in cells)
                {
                    _cells[kvp.Key] = kvp.Value;
                }
            }

            _revealed = new HashSet<Vector2Int>();
            if (revealedCells != null)
            {
                foreach (var c in revealedCells)
                {
                    _revealed.Add(c);
                }
            }
        }

        public MinimapCellKind GetCellKind(Vector2Int cell)
        {
            return _cells.TryGetValue(cell, out var kind) ? kind : MinimapCellKind.Hidden;
        }

        public bool IsRevealed(Vector2Int cell)
        {
            return _revealed.Contains(cell);
        }

        public int RevealedCount => _revealed.Count;
    }
}
