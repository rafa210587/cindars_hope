using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_38 — renderer PURO e determinístico do minimapa (sem MonoBehaviour, sem segunda câmera,
    /// sem RenderTexture). Recebe um <see cref="MinimapGridSource"/> (grid + células reveladas +
    /// posições de player/ícones) e pinta um buffer de cor reutilizado (uma cor por pixel). O
    /// <see cref="MinimapWidget"/> (casca fina) só aplica esse buffer numa Texture2D.
    ///
    /// Dois modos:
    ///  - Cave: fog-of-war (só células reveladas aparecem), player CENTRADO e o mapa rola.
    ///  - Town/Farm: mapa estático completo (sem fog), enquadrado para caber no buffer.
    ///
    /// Toda a matemática (reveal por raio, centragem/rolagem, célula→cor, ícones) é testável em
    /// EditMode. Cores são placeholder (v1: shapes/cores, não sprites reais — fora de escopo).
    /// </summary>
    public sealed class MinimapRenderer
    {
        // ---- Paleta placeholder (v1) ----
        public static readonly Color ColorHidden = new Color(0.05f, 0.05f, 0.07f, 1f);
        public static readonly Color ColorFloor = new Color(0.45f, 0.42f, 0.38f, 1f);
        public static readonly Color ColorWall = new Color(0.16f, 0.15f, 0.18f, 1f);
        public static readonly Color ColorWater = new Color(0.20f, 0.45f, 0.70f, 1f);
        public static readonly Color ColorHazard = new Color(0.75f, 0.30f, 0.15f, 1f);

        public static readonly Color IconPlayer = Color.white;
        public static readonly Color IconNpc = new Color(1f, 0.85f, 0.15f, 1f);   // amarelo
        public static readonly Color IconBoard = new Color(0.20f, 0.50f, 1f, 1f); // azul
        public static readonly Color IconCaveEntrance = new Color(0.90f, 0.15f, 0.15f, 1f); // vermelho
        public static readonly Color IconStairsDown = new Color(0.60f, 0.20f, 0.85f, 1f);
        public static readonly Color IconStairsUp = new Color(0.35f, 0.85f, 0.55f, 1f);
        public static readonly Color IconMerchant = new Color(0.95f, 0.55f, 0.85f, 1f);

        public const int DefaultRevealRadius = 6;   // raio padrão de revelação (CA-1)
        public const int LanternRevealRadius = 9;   // lantern_of_true_sight (F31 — hook)

        public static Color CellColor(MinimapCellKind kind)
        {
            switch (kind)
            {
                case MinimapCellKind.Floor: return ColorFloor;
                case MinimapCellKind.Wall: return ColorWall;
                case MinimapCellKind.Water: return ColorWater;
                case MinimapCellKind.Hazard: return ColorHazard;
                default: return ColorHidden;
            }
        }

        public static Color IconColor(MinimapIconKind kind)
        {
            switch (kind)
            {
                case MinimapIconKind.Player: return IconPlayer;
                case MinimapIconKind.Npc: return IconNpc;
                case MinimapIconKind.Board: return IconBoard;
                case MinimapIconKind.CaveEntrance: return IconCaveEntrance;
                case MinimapIconKind.StairsDown: return IconStairsDown;
                case MinimapIconKind.StairsUp: return IconStairsUp;
                case MinimapIconKind.WanderingMerchant: return IconMerchant;
                default: return IconPlayer;
            }
        }

        /// <summary>
        /// Resolve o raio de revelação efetivo: 6 padrão, 9 com a lantern_of_true_sight equipada
        /// (hook F31). Mantém o contrato simples/determinístico (CA-1).
        /// </summary>
        public static int ResolveRevealRadius(bool hasLanternOfTrueSight)
        {
            return hasLanternOfTrueSight ? LanternRevealRadius : DefaultRevealRadius;
        }

        /// <summary>
        /// fog-of-war: dado o centro do player e o raio, retorna as células (de dentro dos limites
        /// do grid) que devem passar a ser reveladas. Usa distância euclidiana (raio circular).
        /// Determinístico — núcleo do CA-1. NÃO escreve estado; o caller persiste no snapshot.
        /// </summary>
        public static void CollectRevealedCells(
            Vector2Int center, int radius, int width, int height, ICollection<Vector2Int> output)
        {
            if (output == null || radius < 0 || width <= 0 || height <= 0)
            {
                return;
            }

            int r2 = radius * radius;
            int minX = Mathf.Max(0, center.x - radius);
            int maxX = Mathf.Min(width - 1, center.x + radius);
            int minY = Mathf.Max(0, center.y - radius);
            int maxY = Mathf.Min(height - 1, center.y + radius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = x - center.x;
                    int dy = y - center.y;
                    if (dx * dx + dy * dy <= r2)
                    {
                        output.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        /// <summary>
        /// Mapeia o canto inferior-esquerdo (em células) da janela visível do buffer.
        /// Cave: player centrado, a janela rola com ele e é presa (clamp) aos limites do grid.
        /// Town/Farm: janela fixa começando na origem (mapa estático completo, enquadrado).
        /// pixelsPerCell &gt;= 1 dá o zoom; bufferCells = pixelDim / pixelsPerCell.
        /// </summary>
        public static Vector2Int ComputeViewOrigin(
            bool centerOnPlayer, Vector2Int player, int gridWidth, int gridHeight,
            int bufferCellsX, int bufferCellsY)
        {
            if (!centerOnPlayer)
            {
                return Vector2Int.zero;
            }

            int originX = player.x - bufferCellsX / 2;
            int originY = player.y - bufferCellsY / 2;

            // Clamp para não rolar além do grid quando ele é maior que a janela; quando o grid é
            // menor, mantém origem 0 (cabe inteiro).
            if (gridWidth > bufferCellsX)
            {
                originX = Mathf.Clamp(originX, 0, gridWidth - bufferCellsX);
            }
            else
            {
                originX = 0;
            }

            if (gridHeight > bufferCellsY)
            {
                originY = Mathf.Clamp(originY, 0, gridHeight - bufferCellsY);
            }
            else
            {
                originY = 0;
            }

            return new Vector2Int(originX, originY);
        }

        /// <summary>
        /// Pinta o <paramref name="buffer"/> (reutilizado, sem alloc por update) a partir do
        /// <paramref name="source"/>. O buffer tem pixelDim × pixelDim pixels; cada célula ocupa
        /// pixelsPerCell × pixelsPerCell. Em Cave aplica fog (células não reveladas = Hidden);
        /// em Town/Farm pinta tudo. Ícones desenham por cima da célula. Determinístico (CA-3).
        /// </summary>
        public void Render(MinimapGridSource source, Color[] buffer, int pixelDim, int pixelsPerCell)
        {
            if (source == null || buffer == null || pixelDim <= 0 || pixelsPerCell <= 0)
            {
                return;
            }

            int needed = pixelDim * pixelDim;
            if (buffer.Length < needed)
            {
                return;
            }

            // Fundo
            for (int i = 0; i < needed; i++)
            {
                buffer[i] = ColorHidden;
            }

            int bufferCells = pixelDim / pixelsPerCell;
            if (bufferCells <= 0)
            {
                return;
            }

            Vector2Int origin = ComputeViewOrigin(
                source.CenterOnPlayer, source.PlayerCell, source.Width, source.Height,
                bufferCells, bufferCells);

            // Células
            for (int cy = 0; cy < bufferCells; cy++)
            {
                for (int cx = 0; cx < bufferCells; cx++)
                {
                    int gridX = origin.x + cx;
                    int gridY = origin.y + cy;
                    if (gridX < 0 || gridX >= source.Width || gridY < 0 || gridY >= source.Height)
                    {
                        continue;
                    }

                    var cell = new Vector2Int(gridX, gridY);
                    MinimapCellKind kind = source.GetCellKind(cell);

                    // Fog: na caverna só pinta células reveladas.
                    if (source.UseFog && !source.IsRevealed(cell))
                    {
                        continue;
                    }

                    if (kind == MinimapCellKind.Hidden)
                    {
                        continue;
                    }

                    PaintCell(buffer, pixelDim, pixelsPerCell, cx, cy, CellColor(kind));
                }
            }

            // Ícones (desenham por cima; respeitam fog na caverna)
            if (source.Icons != null)
            {
                foreach (var icon in source.Icons)
                {
                    int gridX = icon.Cell.x;
                    int gridY = icon.Cell.y;

                    if (source.UseFog && icon.Kind != MinimapIconKind.Player && !source.IsRevealed(icon.Cell))
                    {
                        continue; // ícones de caverna só aparecem em célula revelada (player sempre).
                    }

                    int cx = gridX - origin.x;
                    int cy = gridY - origin.y;
                    if (cx < 0 || cx >= bufferCells || cy < 0 || cy >= bufferCells)
                    {
                        continue;
                    }

                    PaintCell(buffer, pixelDim, pixelsPerCell, cx, cy, IconColor(icon.Kind));
                }
            }
        }

        private static void PaintCell(
            Color[] buffer, int pixelDim, int pixelsPerCell, int cellX, int cellY, Color color)
        {
            int px0 = cellX * pixelsPerCell;
            int py0 = cellY * pixelsPerCell;

            for (int py = 0; py < pixelsPerCell; py++)
            {
                int row = (py0 + py) * pixelDim;
                for (int px = 0; px < pixelsPerCell; px++)
                {
                    int idx = row + px0 + px;
                    if (idx >= 0 && idx < buffer.Length)
                    {
                        buffer[idx] = color;
                    }
                }
            }
        }
    }

    /// <summary>
    /// fable_38 — entrada imutável de um ícone no minimapa (célula + tipo). Sem refs Unity.
    /// </summary>
    public readonly struct MinimapIcon
    {
        public readonly Vector2Int Cell;
        public readonly MinimapIconKind Kind;

        public MinimapIcon(Vector2Int cell, MinimapIconKind kind)
        {
            Cell = cell;
            Kind = kind;
        }
    }
}
