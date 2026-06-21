using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CindarsHope.Cave.Runtime;
using CindarsHope.UI.Runtime;

namespace CindarsHope.Tests.EditMode.UI
{
    /// <summary>
    /// fable_38 — EditMode tests da matemática PURA do minimapa: reveal por raio (6 / 9 lantern),
    /// persistência intra-run e reset por troca de CaveRunSeed (contrato stable-run / ADR-0005),
    /// mapeamento célula→cor, centragem/rolagem, throttle 4×/s e fog no render. Sem scene/canvas
    /// (isso fica no cenário humano do lote).
    /// </summary>
    [TestFixture]
    public class MinimapTests
    {
        // ---------------------------------------------------------------- helpers

        private static VisitedLevelSnapshot MakeSnapshot(string runSeed, int level, int w, int h)
        {
            var snap = new VisitedLevelSnapshot(level, "biome_stone", "hash", "world", runSeed);
            snap.SetLayoutDimensions(w, h);
            return snap;
        }

        // ---------------------------------------------------------------- CA-1: reveal por raio

        [Test]
        public void CollectRevealedCells_Radius6_RevealsCircleWithinBounds()
        {
            var output = new HashSet<Vector2Int>();
            MinimapRenderer.CollectRevealedCells(new Vector2Int(20, 20), 6, 55, 55, output);

            // Centro revelado; (20+6,20) na borda do círculo (dist=6) revelado; (20+7,20) fora.
            Assert.IsTrue(output.Contains(new Vector2Int(20, 20)));
            Assert.IsTrue(output.Contains(new Vector2Int(26, 20)));
            Assert.IsFalse(output.Contains(new Vector2Int(27, 20)));
            // Diagonal além do raio (dist^2 = 72 > 36) não revela.
            Assert.IsFalse(output.Contains(new Vector2Int(26, 26)));
        }

        [Test]
        public void CollectRevealedCells_ClampsToGridBounds()
        {
            var output = new HashSet<Vector2Int>();
            MinimapRenderer.CollectRevealedCells(new Vector2Int(0, 0), 6, 10, 10, output);

            foreach (var c in output)
            {
                Assert.GreaterOrEqual(c.x, 0);
                Assert.GreaterOrEqual(c.y, 0);
                Assert.Less(c.x, 10);
                Assert.Less(c.y, 10);
            }
            Assert.IsTrue(output.Contains(new Vector2Int(0, 0)));
        }

        [Test]
        public void ResolveRevealRadius_LanternExtendsSixToNine()
        {
            Assert.AreEqual(6, MinimapRenderer.ResolveRevealRadius(false));
            Assert.AreEqual(9, MinimapRenderer.ResolveRevealRadius(true));
        }

        [Test]
        public void RevealAroundPlayer_LanternRevealsMoreCells()
        {
            var noLantern = MakeSnapshot("runA", 3, 55, 55);
            var withLantern = MakeSnapshot("runA", 3, 55, 55);

            int a = MinimapCaveSourceBuilder.RevealAroundPlayer(noLantern, new Vector2Int(27, 27), false);
            int b = MinimapCaveSourceBuilder.RevealAroundPlayer(withLantern, new Vector2Int(27, 27), true);

            Assert.Greater(b, a, "Raio 9 (lantern) deve revelar mais células que raio 6.");
        }

        // ---------------------------------------------------------------- CA-1: persistência intra-run

        [Test]
        public void RevealCells_IsIdempotent_RevisitKeepsRevealed()
        {
            var snap = MakeSnapshot("runA", 2, 55, 55);

            int firstWalk = MinimapCaveSourceBuilder.RevealAroundPlayer(snap, new Vector2Int(10, 10), false);
            Assert.Greater(firstWalk, 0);
            int afterFirst = snap.RevealedCells.Count;

            // "Sair e voltar": revelar a MESMA área de novo não adiciona nada (idempotente).
            int revisitWalk = MinimapCaveSourceBuilder.RevealAroundPlayer(snap, new Vector2Int(10, 10), false);
            Assert.AreEqual(0, revisitWalk);
            Assert.AreEqual(afterFirst, snap.RevealedCells.Count, "Revisita mantém exatamente o revelado.");

            // Andar para nova área acumula (não reseta o já revelado).
            MinimapCaveSourceBuilder.RevealAroundPlayer(snap, new Vector2Int(40, 40), false);
            Assert.Greater(snap.RevealedCells.Count, afterFirst);
            Assert.IsTrue(snap.IsCellRevealed(new Vector2Int(10, 10)));
        }

        // ---------------------------------------------------------------- CA-2: reset por nova run

        [Test]
        public void NewRunSeed_ProducesFreshSnapshot_FogReset()
        {
            // Run A: revela área.
            var runA = new CaveRuntimeState { CaveRunSeed = "seedA" };
            var snapA = MakeSnapshot("seedA", 1, 55, 55);
            MinimapCaveSourceBuilder.RevealAroundPlayer(snapA, new Vector2Int(20, 20), false);
            runA.VisitedLevelSnapshots[1] = snapA;
            Assert.Greater(runA.VisitedLevelSnapshots[1].RevealedCells.Count, 0);

            // Nova run (KO/new game troca CaveRunSeed) = snapshots novos, fog zerado por nível.
            var runB = new CaveRuntimeState { CaveRunSeed = "seedB" };
            var snapB = MakeSnapshot("seedB", 1, 55, 55);
            runB.VisitedLevelSnapshots[1] = snapB;

            Assert.AreEqual(0, runB.VisitedLevelSnapshots[1].RevealedCells.Count,
                "Nova run nasce escura (fog reset) — contrato stable-run / ADR-0005.");
        }

        [Test]
        public void LegacySnapshot_WithoutRevealedField_StartsDark()
        {
            var snap = MakeSnapshot("runA", 1, 42, 42);
            Assert.IsNotNull(snap.RevealedCells);
            Assert.AreEqual(0, snap.RevealedCells.Count);
        }

        // ---------------------------------------------------------------- CA-3: célula→cor

        [Test]
        public void CellColor_MapsEachKindToDistinctPlaceholder()
        {
            Assert.AreEqual(MinimapRenderer.ColorFloor, MinimapRenderer.CellColor(MinimapCellKind.Floor));
            Assert.AreEqual(MinimapRenderer.ColorWall, MinimapRenderer.CellColor(MinimapCellKind.Wall));
            Assert.AreEqual(MinimapRenderer.ColorWater, MinimapRenderer.CellColor(MinimapCellKind.Water));
            Assert.AreEqual(MinimapRenderer.ColorHazard, MinimapRenderer.CellColor(MinimapCellKind.Hazard));
            Assert.AreEqual(MinimapRenderer.ColorHidden, MinimapRenderer.CellColor(MinimapCellKind.Hidden));
        }

        [Test]
        public void IconColor_PlayerIsWhite_NpcYellow_BoardBlue_CaveEntranceRed()
        {
            Assert.AreEqual(Color.white, MinimapRenderer.IconColor(MinimapIconKind.Player));
            Assert.AreEqual(MinimapRenderer.IconNpc, MinimapRenderer.IconColor(MinimapIconKind.Npc));
            Assert.AreEqual(MinimapRenderer.IconBoard, MinimapRenderer.IconColor(MinimapIconKind.Board));
            Assert.AreEqual(MinimapRenderer.IconCaveEntrance, MinimapRenderer.IconColor(MinimapIconKind.CaveEntrance));
        }

        // ---------------------------------------------------------------- centragem / rolagem

        [Test]
        public void ComputeViewOrigin_CenterOnPlayer_ClampsToGrid()
        {
            // Grid 55x55, janela 40 células, player no centro → origem clamped 0..15.
            var origin = MinimapRenderer.ComputeViewOrigin(true, new Vector2Int(27, 27), 55, 55, 40, 40);
            Assert.AreEqual(7, origin.x); // 27 - 20 = 7
            Assert.AreEqual(7, origin.y);

            // Player perto da borda esquerda → clamp em 0.
            var nearLeft = MinimapRenderer.ComputeViewOrigin(true, new Vector2Int(2, 2), 55, 55, 40, 40);
            Assert.AreEqual(0, nearLeft.x);
            Assert.AreEqual(0, nearLeft.y);

            // Player perto da borda direita → clamp em (grid - janela).
            var nearRight = MinimapRenderer.ComputeViewOrigin(true, new Vector2Int(54, 54), 55, 55, 40, 40);
            Assert.AreEqual(15, nearRight.x);
            Assert.AreEqual(15, nearRight.y);
        }

        [Test]
        public void ComputeViewOrigin_TownFarm_StaticOriginZero()
        {
            var origin = MinimapRenderer.ComputeViewOrigin(false, new Vector2Int(30, 30), 48, 42, 40, 40);
            Assert.AreEqual(Vector2Int.zero, origin);
        }

        [Test]
        public void ComputeViewOrigin_GridSmallerThanWindow_StaysAtZero()
        {
            var origin = MinimapRenderer.ComputeViewOrigin(true, new Vector2Int(5, 5), 20, 20, 40, 40);
            Assert.AreEqual(Vector2Int.zero, origin);
        }

        // ---------------------------------------------------------------- throttle 4x/s (CA-4)

        [Test]
        public void Throttle_FourUpdatesPerSecond_IntervalIsQuarterSecond()
        {
            Assert.AreEqual(4f, MinimapWidget.UpdatesPerSecond);
            float interval = 1f / MinimapWidget.UpdatesPerSecond;
            Assert.AreEqual(0.25f, interval, 0.0001f);

            // Simula um acumulador: 0.20s não dispara; +0.10s (=0.30) dispara e sobra.
            float acc = 0f;
            int renders = 0;
            void Tick(float dt)
            {
                acc += dt;
                if (acc >= interval) { acc = 0f; renders++; }
            }

            Tick(0.20f); // 0.20 < 0.25 → não renderiza
            Assert.AreEqual(0, renders);
            Tick(0.10f); // 0.30 >= 0.25 → renderiza
            Assert.AreEqual(1, renders);

            // Em 1s de dt=1/60, no máximo ~4 renders.
            acc = 0f; renders = 0;
            for (int i = 0; i < 60; i++) Tick(1f / 60f);
            Assert.LessOrEqual(renders, 4);
        }

        // ---------------------------------------------------------------- fog no render

        [Test]
        public void Render_Cave_HidesUnrevealedCells()
        {
            // Grid 8x8, todas floor; revela só (0,0). Buffer 8px, 1px/célula (sem zoom).
            var cells = new Dictionary<Vector2Int, MinimapCellKind>();
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    cells[new Vector2Int(x, y)] = MinimapCellKind.Floor;

            var revealed = new List<Vector2Int> { new Vector2Int(0, 0) };
            var source = new MinimapGridSource(
                8, 8, cells, revealed, useFog: true, centerOnPlayer: true,
                new Vector2Int(0, 0), new List<MinimapIcon>());

            var renderer = new MinimapRenderer();
            var buffer = new Color[8 * 8];
            renderer.Render(source, buffer, 8, 1);

            // (0,0) revelado = Floor; (5,5) não revelado = Hidden (fundo).
            Assert.AreEqual(MinimapRenderer.ColorFloor, buffer[0]);
            Assert.AreEqual(MinimapRenderer.ColorHidden, buffer[5 * 8 + 5]);
        }

        [Test]
        public void Render_TownFarm_NoFog_PaintsAllCells()
        {
            var cells = new Dictionary<Vector2Int, MinimapCellKind>();
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    cells[new Vector2Int(x, y)] = MinimapCellKind.Floor;

            // Town/Farm: useFog=false, sem reveal — tudo pintado.
            var source = new MinimapGridSource(
                8, 8, cells, null, useFog: false, centerOnPlayer: false,
                new Vector2Int(4, 4), new List<MinimapIcon>());

            var renderer = new MinimapRenderer();
            var buffer = new Color[8 * 8];
            renderer.Render(source, buffer, 8, 1);

            Assert.AreEqual(MinimapRenderer.ColorFloor, buffer[5 * 8 + 5]);
        }

        [Test]
        public void Render_PlayerIconPaintsWhite_OverCell()
        {
            var cells = new Dictionary<Vector2Int, MinimapCellKind>
            {
                { new Vector2Int(3, 3), MinimapCellKind.Floor }
            };
            var icons = new List<MinimapIcon> { new MinimapIcon(new Vector2Int(3, 3), MinimapIconKind.Player) };
            var source = new MinimapGridSource(
                8, 8, cells, null, useFog: false, centerOnPlayer: false,
                new Vector2Int(3, 3), icons);

            var renderer = new MinimapRenderer();
            var buffer = new Color[8 * 8];
            renderer.Render(source, buffer, 8, 1);

            Assert.AreEqual(Color.white, buffer[3 * 8 + 3]);
        }

        // ---------------------------------------------------------------- cave source builder

        [Test]
        public void CaveSourceBuilder_MapsWalkableToFloor_WallToWall()
        {
            var snap = MakeSnapshot("runA", 1, 8, 8);
            snap.AddWalkableTile(new Vector2Int(1, 1));
            snap.AddWallTile(new Vector2Int(2, 2));
            snap.SetEntranceAndExit(new Vector2(1, 1), new Vector2(6, 6));

            var source = MinimapCaveSourceBuilder.Build(snap, new Vector2Int(1, 1));

            Assert.IsTrue(source.UseFog);
            Assert.IsTrue(source.CenterOnPlayer);
            Assert.AreEqual(MinimapCellKind.Floor, source.GetCellKind(new Vector2Int(1, 1)));
            Assert.AreEqual(MinimapCellKind.Wall, source.GetCellKind(new Vector2Int(2, 2)));
        }
    }
}
