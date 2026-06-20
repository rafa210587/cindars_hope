using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Lots;
using CindarsHope.Farm.Resources;

namespace CindarsHope.Tests.EditMode.Farm
{
    // fable_41 — testes puros do sistema de lotes de expansão.
    // Cobre CA-1 (travados no início), CA-2 (compra/destrava idempotente + round-trip),
    // CA-3 (pomar sazonal via FarmResourceNodeService) e CA-4 (save legado seguro).
    [TestFixture]
    public class FarmLotsTests
    {
        // ── CA-1: estado inicial — tudo Locked ──────────────────────────────────────
        [Test]
        public void NewProgression_AllLots_StartLocked()
        {
            var prog = new FarmLotProgression();

            foreach (var lotId in FarmLotId.All)
            {
                Assert.AreEqual(FarmLotState.Locked, prog.GetState(lotId), $"{lotId} deveria nascer Locked");
                Assert.IsTrue(prog.IsLocked(lotId));
                Assert.IsFalse(prog.IsOwned(lotId));
            }

            Assert.AreEqual(3, FarmLotId.All.Length, "Exatamente 3 lotes (norte/leste/oeste)");
            CollectionAssert.IsEmpty(prog.GetOwnedLotIds());
        }

        // ── CA-2: destravamento idempotente ─────────────────────────────────────────
        [Test]
        public void Unlock_FirstTime_ReturnsTrue_AndMarksOwned()
        {
            var prog = new FarmLotProgression();

            Assert.IsTrue(prog.Unlock(FarmLotId.North), "1º unlock deve retornar true");
            Assert.AreEqual(FarmLotState.Owned, prog.GetState(FarmLotId.North));
            Assert.IsTrue(prog.IsOwned(FarmLotId.North));
        }

        [Test]
        public void Unlock_SecondTime_IsNoOp_ReturnsFalse()
        {
            var prog = new FarmLotProgression();
            prog.Unlock(FarmLotId.East);

            Assert.IsFalse(prog.Unlock(FarmLotId.East), "2º unlock (re-uso/reload) deve ser no-op");
            Assert.AreEqual(FarmLotState.Owned, prog.GetState(FarmLotId.East));
            Assert.AreEqual(1, prog.GetOwnedLotIds().Count, "Não deve duplicar posse");
        }

        [Test]
        public void Unlock_UnknownLot_ReturnsFalse_AndChangesNothing()
        {
            var prog = new FarmLotProgression();

            Assert.IsFalse(prog.Unlock("lot_unknown"));
            Assert.IsFalse(prog.Unlock(null));
            Assert.IsFalse(prog.Unlock(string.Empty));
            CollectionAssert.IsEmpty(prog.GetOwnedLotIds());
        }

        [Test]
        public void Unlock_OneLot_DoesNotUnlockOthers()
        {
            var prog = new FarmLotProgression();
            prog.Unlock(FarmLotId.West);

            Assert.IsTrue(prog.IsOwned(FarmLotId.West));
            Assert.IsTrue(prog.IsLocked(FarmLotId.North));
            Assert.IsTrue(prog.IsLocked(FarmLotId.East));
        }

        // ── CA-2: round-trip de save (ownedLots) ────────────────────────────────────
        [Test]
        public void Capture_Then_Restore_PreservesOwnedLots()
        {
            var prog = new FarmLotProgression();
            prog.Unlock(FarmLotId.North);
            prog.Unlock(FarmLotId.West);

            var save = prog.Capture();
            Assert.AreEqual(2, save.OwnedLots.Count);
            CollectionAssert.Contains(save.OwnedLots, FarmLotId.North);
            CollectionAssert.Contains(save.OwnedLots, FarmLotId.West);

            var restored = new FarmLotProgression();
            restored.Restore(save);

            Assert.IsTrue(restored.IsOwned(FarmLotId.North));
            Assert.IsTrue(restored.IsOwned(FarmLotId.West));
            Assert.IsTrue(restored.IsLocked(FarmLotId.East), "Lote não comprado continua Locked após load");
        }

        [Test]
        public void Restore_IsIdempotent_AppliedTwice_SameResult()
        {
            var save = new FarmLotsSaveData { OwnedLots = new List<string> { FarmLotId.East } };
            var prog = new FarmLotProgression();

            prog.Restore(save);
            prog.Restore(save);

            Assert.AreEqual(1, prog.GetOwnedLotIds().Count);
            Assert.IsTrue(prog.IsOwned(FarmLotId.East));
        }

        [Test]
        public void Restore_ResetsPreviouslyOwned_NotInSave()
        {
            var prog = new FarmLotProgression();
            prog.Unlock(FarmLotId.North); // estado pré-existente

            // save só tem o leste → norte deve voltar a Locked
            prog.Restore(new FarmLotsSaveData { OwnedLots = new List<string> { FarmLotId.East } });

            Assert.IsTrue(prog.IsLocked(FarmLotId.North));
            Assert.IsTrue(prog.IsOwned(FarmLotId.East));
        }

        // ── CA-4: save legado seguro ────────────────────────────────────────────────
        [Test]
        public void Restore_NullSave_AllLocked_NoError()
        {
            var prog = new FarmLotProgression();
            prog.Unlock(FarmLotId.North);

            Assert.DoesNotThrow(() => prog.Restore(null));

            foreach (var lotId in FarmLotId.All)
            {
                Assert.IsTrue(prog.IsLocked(lotId), $"{lotId} deve estar Locked após restore de save legado (null)");
            }
        }

        [Test]
        public void Restore_LegacySave_MissingOwnedLotsList_AllLocked()
        {
            // Save legado: campo OwnedLots ausente é desserializado como lista vazia/null pelo JsonUtility.
            var legacy = new FarmLotsSaveData { OwnedLots = null };
            var prog = new FarmLotProgression();

            Assert.DoesNotThrow(() => prog.Restore(legacy));
            CollectionAssert.IsEmpty(prog.GetOwnedLotIds());
        }

        [Test]
        public void Restore_IgnoresUnknownLotIds_InSave()
        {
            var save = new FarmLotsSaveData { OwnedLots = new List<string> { "lot_bogus", FarmLotId.West } };
            var prog = new FarmLotProgression();

            prog.Restore(save);

            Assert.IsTrue(prog.IsOwned(FarmLotId.West));
            Assert.AreEqual(1, prog.GetOwnedLotIds().Count, "ID desconhecido no save é ignorado, não quebra");
        }

        // ── Catálogo: escrituras, preços e mapeamento deed→lot ──────────────────────
        [Test]
        public void Catalog_HasThreeLots_WithCanonicalPrices()
        {
            Assert.AreEqual(2500, FarmLotCatalog.GetDeedPrice(FarmLotId.North));
            Assert.AreEqual(4000, FarmLotCatalog.GetDeedPrice(FarmLotId.East));
            Assert.AreEqual(3500, FarmLotCatalog.GetDeedPrice(FarmLotId.West));
            Assert.AreEqual(3, FarmLotCatalog.All.Count);
        }

        [Test]
        public void Catalog_DeedItemId_ResolvesToCorrectLot()
        {
            Assert.AreEqual(FarmLotId.North, FarmLotCatalog.ResolveLotIdForDeed("item_key_lot_deed_north"));
            Assert.AreEqual(FarmLotId.East, FarmLotCatalog.ResolveLotIdForDeed("item_key_lot_deed_east"));
            Assert.AreEqual(FarmLotId.West, FarmLotCatalog.ResolveLotIdForDeed("item_key_lot_deed_west"));
        }

        [Test]
        public void Catalog_NonDeedItem_ResolvesToNull()
        {
            Assert.IsNull(FarmLotCatalog.ResolveLotIdForDeed("item_wood"));
            Assert.IsNull(FarmLotCatalog.ResolveLotIdForDeed(null));
            Assert.IsFalse(FarmLotCatalog.IsDeedItem("item_wood"));
            Assert.IsTrue(FarmLotCatalog.IsDeedItem("item_key_lot_deed_east"));
        }

        [Test]
        public void Catalog_EachLotMapsToDistinctDeedItem()
        {
            var deedIds = new HashSet<string>();
            foreach (var def in FarmLotCatalog.All)
            {
                Assert.IsTrue(FarmLotId.IsKnown(def.LotId), $"{def.LotId} deve ser um lote conhecido");
                Assert.IsTrue(deedIds.Add(def.DeedItemId), $"Escritura {def.DeedItemId} duplicada");
                Assert.Greater(def.DeedPrice, 0, "Preço da escritura deve ser > 0 (sink de ouro 5.3)");
            }
            Assert.AreEqual(3, deedIds.Count);
        }

        // ── CA-3: pomar sazonal via FarmResourceNodeService (reuso do motor existente) ─
        private FarmResourceNodeService OrchardService()
            => new FarmResourceNodeService(FarmOrchardCatalog.BuildDefinitions());

        private static ResourceNodeInstanceState FreshTree(string nodeId) =>
            new ResourceNodeInstanceState
            {
                NodeInstanceId = nodeId + "_inst", NodeId = nodeId,
                ZoneId = FarmOrchardCatalog.ZoneId,
                CurrentState = ResourceNodeCurrentState.Available, RemainingHits = 1
            };

        [Test]
        public void Orchard_HasFourTrees_CoveringFourSeasons()
        {
            var defs = FarmOrchardCatalog.BuildDefinitions();
            Assert.AreEqual(4, defs.Count, "Pomar tem 4 árvores frutíferas");

            var seasons = new HashSet<string>();
            foreach (var def in defs.Values)
            {
                Assert.IsNotEmpty(def.RequiredSeason, "Cada árvore tem uma estação obrigatória (sazonal)");
                seasons.Add(def.RequiredSeason);
                Assert.AreEqual(0, def.RequiredFarmLevel, "5.3: sem gate de nível de fazenda/caverna no pomar");
            }
            Assert.AreEqual(4, seasons.Count, "As 4 árvores cobrem 4 estações distintas");
        }

        [Test]
        public void Orchard_AppleTree_GivesFruit_OnlyInSpring()
        {
            var service = OrchardService();

            // Estação errada → sem fruta.
            var wrong = service.Harvest(FreshTree(FarmOrchardCatalog.NodeApple), currentDay: 5, toolTier: "Basic", currentSeason: "Winter");
            Assert.IsFalse(wrong.Success);
            Assert.AreEqual("WrongSeason", wrong.FailureReason);

            // Estação certa → fruta.
            var right = service.Harvest(FreshTree(FarmOrchardCatalog.NodeApple), currentDay: 5, toolTier: "Basic", currentSeason: "Spring");
            Assert.IsTrue(right.Success);
            CollectionAssert.Contains(right.DroppedItemIds, "item_crop_apple");
        }

        [Test]
        public void Orchard_Harvest_IsIdempotent_SameDaySecondCall_NotAvailable()
        {
            var service = OrchardService();
            var tree = FreshTree(FarmOrchardCatalog.NodePlum);

            var first = service.Harvest(tree, currentDay: 10, toolTier: "Basic", currentSeason: "Winter");
            Assert.IsTrue(first.Success);

            // 2ª colheita no mesmo dia: nó já despletado → NotAvailable (idempotente).
            var second = service.Harvest(tree, currentDay: 10, toolTier: "Basic", currentSeason: "Winter");
            Assert.IsFalse(second.Success);
            Assert.AreEqual("NodeNotAvailable", second.FailureReason);
        }

        [Test]
        public void Orchard_AllSeasons_EachTreeFruitsInItsSeason()
        {
            var service = OrchardService();
            var map = new (string node, string season, string fruit)[]
            {
                (FarmOrchardCatalog.NodeApple, "Spring", "item_crop_apple"),
                (FarmOrchardCatalog.NodeCherry, "Summer", "item_crop_cherry"),
                (FarmOrchardCatalog.NodePear, "Autumn", "item_crop_pear"),
                (FarmOrchardCatalog.NodePlum, "Winter", "item_crop_plum"),
            };

            foreach (var (node, season, fruit) in map)
            {
                var res = service.Harvest(FreshTree(node), currentDay: 1, toolTier: "Basic", currentSeason: season);
                Assert.IsTrue(res.Success, $"{node} deveria dar fruta em {season}");
                CollectionAssert.Contains(res.DroppedItemIds, fruit);
            }
        }
    }
}
