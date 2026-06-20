using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm;
using CindarsHope.Farm.Processing;
using CindarsHope.Farm.Watering;

namespace CindarsHope.Tests.EditMode.Farm
{
    /// <summary>
    /// fable_55 — testes puros do processamento de fazenda (queijaria/barril) + estufa.
    /// Cobre CA-2 (timing por dia + consumo + idempotência), CA-3 (round-trip no meio/fim),
    /// CA-4 (estufa ignora estação/chuva + regressão do canteiro comum) e CA-5 (save legado).
    /// Sem cena: usa o motor puro FarmProcessingStationModel + porta de inventário fake.
    /// </summary>
    [TestFixture]
    public class ProcessingGreenhouseTests
    {
        // ── Inventário fake (porta IProcessingInventory) ─────────────────────────────
        private sealed class FakeInventory : IProcessingInventory
        {
            private readonly Dictionary<string, int> _items = new Dictionary<string, int>();
            public bool RejectAdds;

            public void Seed(string itemId, int amount) => _items[itemId] = amount;
            public int Count(string itemId) => _items.TryGetValue(itemId, out var v) ? v : 0;

            public bool HasItem(string itemId, int amount) => Count(itemId) >= amount;

            public bool RemoveItem(string itemId, int amount)
            {
                if (Count(itemId) < amount) return false;
                _items[itemId] = Count(itemId) - amount;
                return true;
            }

            public bool AddItem(string itemId, int amount)
            {
                if (RejectAdds) return false;
                _items[itemId] = Count(itemId) + amount;
                return true;
            }
        }

        private static FakeInventory CheeseReadyInventory()
        {
            var inv = new FakeInventory();
            inv.Seed(ProcessingRecipeCatalog.ItemGoatMilkId, 2);
            return inv;
        }

        private static FakeInventory WineReadyInventory()
        {
            var inv = new FakeInventory();
            inv.Seed(ProcessingRecipeCatalog.ItemGrapeId, 5);
            return inv;
        }

        // ── CA-1/Fase 0: catálogo confere com os números do ITEM_CATALOG §5 ──────────
        [Test]
        public void Catalog_CheeseRecipe_MatchesItemCatalog()
        {
            Assert.IsTrue(ProcessingRecipeCatalog.TryGetById(ProcessingRecipeCatalog.RecipeGoatCheeseId, out var cheese));
            Assert.AreEqual(ProcessingRecipeCatalog.ItemGoatMilkId, cheese.InputItemId);
            Assert.AreEqual(2, cheese.InputQuantity);
            Assert.AreEqual(ProcessingRecipeCatalog.ItemGoatCheeseId, cheese.OutputItemId);
            Assert.AreEqual(1, cheese.OutputQuantity);
            Assert.AreEqual(1, cheese.ProcessingDays, "queijo = 1 dia");
        }

        [Test]
        public void Catalog_WineRecipe_MatchesItemCatalog()
        {
            Assert.IsTrue(ProcessingRecipeCatalog.TryGetById(ProcessingRecipeCatalog.RecipeValeWineId, out var wine));
            Assert.AreEqual(ProcessingRecipeCatalog.ItemGrapeId, wine.InputItemId);
            Assert.AreEqual(5, wine.InputQuantity);
            Assert.AreEqual(ProcessingRecipeCatalog.ItemValeWineId, wine.OutputItemId);
            Assert.AreEqual(2, wine.ProcessingDays, "vinho = 2 dias");
        }

        [Test]
        public void Catalog_StationMapsToSingleRecipe()
        {
            Assert.IsTrue(ProcessingRecipeCatalog.TryGetByStation(ProcessingRecipeCatalog.StationCheesePressId, out var cheese));
            Assert.AreEqual(ProcessingRecipeCatalog.RecipeGoatCheeseId, cheese.RecipeId);
            Assert.IsTrue(ProcessingRecipeCatalog.TryGetByStation(ProcessingRecipeCatalog.StationWineBarrelId, out var wine));
            Assert.AreEqual(ProcessingRecipeCatalog.RecipeValeWineId, wine.RecipeId);
        }

        // ── CA-2: timing por dia + consumo + idempotência ────────────────────────────
        [Test]
        public void Cheese_ReadyNextDay_AfterStart()
        {
            var model = new FarmProcessingStationModel();
            var inv = CheeseReadyInventory();

            var start = model.StartJob(ProcessingRecipeCatalog.StationCheesePressId, ProcessingRecipeCatalog.RecipeGoatCheeseId, currentDay: 1, inv);
            Assert.AreEqual(FarmProcessingStationModel.StartResult.Started, start);

            // Insumos consumidos no início.
            Assert.AreEqual(0, inv.Count(ProcessingRecipeCatalog.ItemGoatMilkId), "goat_milk consumido no start");

            var job = model.GetJob(ProcessingRecipeCatalog.StationCheesePressId);
            Assert.AreEqual(ProcessingJobState.Processing, job.State);
            Assert.AreEqual(2, job.FinishDay, "start dia 1 + 1 dia = pronto no dia 2");

            // No mesmo dia ainda não está pronto.
            CollectionAssert.IsEmpty(model.AdvanceDay(1));
            Assert.AreEqual(ProcessingJobState.Processing, model.GetJob(ProcessingRecipeCatalog.StationCheesePressId).State);

            // Dia 2 → pronto.
            var ready = model.AdvanceDay(2);
            Assert.AreEqual(1, ready.Count);
            Assert.AreEqual(ProcessingJobState.ReadyToCollect, model.GetJob(ProcessingRecipeCatalog.StationCheesePressId).State);
        }

        [Test]
        public void Wine_ReadyInTwoDays()
        {
            var model = new FarmProcessingStationModel();
            var inv = WineReadyInventory();

            model.StartJob(ProcessingRecipeCatalog.StationWineBarrelId, ProcessingRecipeCatalog.RecipeValeWineId, currentDay: 3, inv);
            Assert.AreEqual(0, inv.Count(ProcessingRecipeCatalog.ItemGrapeId), "grape ×5 consumido no start");

            CollectionAssert.IsEmpty(model.AdvanceDay(4), "dia 4 (1 dia) ainda não pronto");
            Assert.AreEqual(ProcessingJobState.Processing, model.GetJob(ProcessingRecipeCatalog.StationWineBarrelId).State);

            var ready = model.AdvanceDay(5);
            Assert.AreEqual(1, ready.Count, "dia 5 (2 dias) pronto");
            Assert.AreEqual(ProcessingJobState.ReadyToCollect, model.GetJob(ProcessingRecipeCatalog.StationWineBarrelId).State);
        }

        [Test]
        public void Collect_DeliversOutput_Once_SecondCollectFails()
        {
            var model = new FarmProcessingStationModel();
            var inv = CheeseReadyInventory();
            model.StartJob(ProcessingRecipeCatalog.StationCheesePressId, ProcessingRecipeCatalog.RecipeGoatCheeseId, currentDay: 1, inv);
            model.AdvanceDay(2);

            var first = model.Collect(ProcessingRecipeCatalog.StationCheesePressId, inv);
            Assert.IsTrue(first.Success);
            Assert.AreEqual(ProcessingRecipeCatalog.ItemGoatCheeseId, first.OutputItemId);
            Assert.AreEqual(1, inv.Count(ProcessingRecipeCatalog.ItemGoatCheeseId), "queijo entregue 1×");

            // 2ª coleta (duplo clique / reload) falha — não duplica.
            var second = model.Collect(ProcessingRecipeCatalog.StationCheesePressId, inv);
            Assert.IsFalse(second.Success);
            Assert.AreEqual(1, inv.Count(ProcessingRecipeCatalog.ItemGoatCheeseId), "sem duplicação");
            Assert.IsFalse(model.HasActiveJob(ProcessingRecipeCatalog.StationCheesePressId), "estação livre após coletar");
        }

        [Test]
        public void Start_FailsWhenMissingInput_NoConsumption()
        {
            var model = new FarmProcessingStationModel();
            var inv = new FakeInventory();
            inv.Seed(ProcessingRecipeCatalog.ItemGoatMilkId, 1); // precisa 2

            var result = model.StartJob(ProcessingRecipeCatalog.StationCheesePressId, ProcessingRecipeCatalog.RecipeGoatCheeseId, currentDay: 1, inv);
            Assert.AreEqual(FarmProcessingStationModel.StartResult.MissingInput, result);
            Assert.AreEqual(1, inv.Count(ProcessingRecipeCatalog.ItemGoatMilkId), "insumo NÃO consumido em falha de start");
            Assert.IsFalse(model.HasActiveJob(ProcessingRecipeCatalog.StationCheesePressId));
        }

        [Test]
        public void Start_FailsWhenStationBusy()
        {
            var model = new FarmProcessingStationModel();
            var inv = CheeseReadyInventory();
            inv.Seed(ProcessingRecipeCatalog.ItemGoatMilkId, 4); // suficiente p/ 2 tentativas

            Assert.AreEqual(FarmProcessingStationModel.StartResult.Started,
                model.StartJob(ProcessingRecipeCatalog.StationCheesePressId, ProcessingRecipeCatalog.RecipeGoatCheeseId, 1, inv));
            Assert.AreEqual(FarmProcessingStationModel.StartResult.StationBusy,
                model.StartJob(ProcessingRecipeCatalog.StationCheesePressId, ProcessingRecipeCatalog.RecipeGoatCheeseId, 1, inv),
                "estação ocupada recusa novo job");
            Assert.AreEqual(2, inv.Count(ProcessingRecipeCatalog.ItemGoatMilkId), "2º start não consome");
        }

        [Test]
        public void Collect_InventoryFull_KeepsJobReady()
        {
            var model = new FarmProcessingStationModel();
            var inv = CheeseReadyInventory();
            model.StartJob(ProcessingRecipeCatalog.StationCheesePressId, ProcessingRecipeCatalog.RecipeGoatCheeseId, 1, inv);
            model.AdvanceDay(2);

            inv.RejectAdds = true;
            var blocked = model.Collect(ProcessingRecipeCatalog.StationCheesePressId, inv);
            Assert.IsFalse(blocked.Success);
            Assert.IsTrue(blocked.InventoryFull);
            Assert.AreEqual(ProcessingJobState.ReadyToCollect, model.GetJob(ProcessingRecipeCatalog.StationCheesePressId).State,
                "output permanece pronto na estação se o inventário está cheio");
        }

        // ── CA-3: round-trip de save no meio e no fim do job ─────────────────────────
        [Test]
        public void SaveLoad_MidJob_PreservesStationRecipeAndFinishDay()
        {
            var model = new FarmProcessingStationModel();
            var inv = WineReadyInventory();
            model.StartJob(ProcessingRecipeCatalog.StationWineBarrelId, ProcessingRecipeCatalog.RecipeValeWineId, currentDay: 1, inv);
            // meio do job (dia 2, ainda Processing — finishDay = 3).
            model.AdvanceDay(2);

            var save = model.Capture();
            Assert.AreEqual(1, save.Jobs.Count);
            Assert.AreEqual(ProcessingRecipeCatalog.StationWineBarrelId, save.Jobs[0].StationId);
            Assert.AreEqual(ProcessingRecipeCatalog.RecipeValeWineId, save.Jobs[0].RecipeId);
            Assert.AreEqual(3, save.Jobs[0].FinishDay);

            var restored = new FarmProcessingStationModel();
            restored.Restore(save);
            var job = restored.GetJob(ProcessingRecipeCatalog.StationWineBarrelId);
            Assert.IsNotNull(job);
            Assert.AreEqual(ProcessingJobState.Processing, job.State);
            Assert.AreEqual(3, job.FinishDay, "dias restantes preservados");

            // Termina no dia certo após o load.
            CollectionAssert.IsEmpty(restored.AdvanceDay(2));
            Assert.AreEqual(1, restored.AdvanceDay(3).Count, "termina no dia 3");
        }

        [Test]
        public void SaveLoad_AfterReady_DoesNotDuplicateOutput()
        {
            var model = new FarmProcessingStationModel();
            var inv = CheeseReadyInventory();
            model.StartJob(ProcessingRecipeCatalog.StationCheesePressId, ProcessingRecipeCatalog.RecipeGoatCheeseId, 1, inv);
            model.AdvanceDay(2); // ReadyToCollect

            var save = model.Capture();
            Assert.AreEqual(ProcessingJobState.ReadyToCollect, (ProcessingJobState)save.Jobs[0].State);

            var restored = new FarmProcessingStationModel();
            restored.Restore(save);

            // Coleta 1× após reload.
            var collectInv = new FakeInventory();
            var collect = restored.Collect(ProcessingRecipeCatalog.StationCheesePressId, collectInv);
            Assert.IsTrue(collect.Success);
            Assert.AreEqual(1, collectInv.Count(ProcessingRecipeCatalog.ItemGoatCheeseId));

            // Re-salvar após coleta não deixa job pendente; novo load não entrega de novo.
            var save2 = restored.Capture();
            CollectionAssert.IsEmpty(save2.Jobs, "job coletado não persiste");

            var restored2 = new FarmProcessingStationModel();
            restored2.Restore(save2);
            var collectInv2 = new FakeInventory();
            Assert.IsFalse(restored2.Collect(ProcessingRecipeCatalog.StationCheesePressId, collectInv2).Success);
            Assert.AreEqual(0, collectInv2.Count(ProcessingRecipeCatalog.ItemGoatCheeseId), "sem duplicação pós-reload");
        }

        // ── CA-5: save legado seguro ─────────────────────────────────────────────────
        [Test]
        public void Restore_NullSave_NoJobs_NoError()
        {
            var model = new FarmProcessingStationModel();
            Assert.DoesNotThrow(() => model.Restore(null));
            Assert.IsFalse(model.HasActiveJob(ProcessingRecipeCatalog.StationCheesePressId));
            Assert.IsFalse(model.HasActiveJob(ProcessingRecipeCatalog.StationWineBarrelId));
        }

        [Test]
        public void Restore_LegacySave_EmptyJobsList_NoStations()
        {
            var model = new FarmProcessingStationModel();
            Assert.DoesNotThrow(() => model.Restore(new FarmProcessingSaveData { Jobs = null }));
            Assert.IsNull(model.GetJob(ProcessingRecipeCatalog.StationCheesePressId));
        }

        [Test]
        public void Restore_IgnoresUnknownRecipeId()
        {
            var save = new FarmProcessingSaveData
            {
                Jobs = new List<FarmProcessingJobSaveData>
                {
                    new FarmProcessingJobSaveData { StationId = "station_bogus", RecipeId = "processing_unknown", StartDay = 1, FinishDay = 2, State = (int)ProcessingJobState.Processing }
                }
            };
            var model = new FarmProcessingStationModel();
            model.Restore(save);
            Assert.IsFalse(model.HasActiveJob("station_bogus"), "receita desconhecida ignorada");
        }

        // ── CA-4: estufa ignora estação e chuva; canteiro comum não ──────────────────
        [Test]
        public void Greenhouse_OverridesSeason_ForRegisteredPlot()
        {
            var provider = new GreenhouseContextProvider();
            provider.RegisterGreenhousePlot("plot_200");
            provider.SetGreenhouseUnlocked(true);

            // Semente de verão, estação atual inverno → fora de estação.
            var summerTags = new[] { "Verao" };

            // Canteiro da estufa: aceita fora de estação.
            Assert.IsTrue(FarmSeasonGate.IsPlantingAllowed(summerTags, "Inverno", provider.CanOverrideSeason("plot_200")),
                "estufa planta fora de estação");

            // Canteiro comum (não registrado): recusa fora de estação.
            Assert.IsFalse(FarmSeasonGate.IsPlantingAllowed(summerTags, "Inverno", provider.CanOverrideSeason("plot_0")),
                "canteiro comum recusa semente fora de estação");
        }

        [Test]
        public void CommonPlot_AllowsSeed_InMatchingSeason()
        {
            Assert.IsTrue(FarmSeasonGate.IsPlantingAllowed(new[] { "Verao" }, "Verao", canOverrideSeason: false));
            // alias inglês também casa
            Assert.IsTrue(FarmSeasonGate.IsPlantingAllowed(new[] { "Summer" }, "Verao", canOverrideSeason: false));
        }

        [Test]
        public void SeasonGate_NoTags_AlwaysAllowed_Regression()
        {
            // Sementes sem SeasonTags (caso atual do repo) continuam plantáveis o ano todo.
            Assert.IsTrue(FarmSeasonGate.IsPlantingAllowed(null, "Inverno", canOverrideSeason: false));
            Assert.IsTrue(FarmSeasonGate.IsPlantingAllowed(new string[0], "Inverno", canOverrideSeason: false));
        }

        [Test]
        public void SeasonGate_NoCalendarInfo_AllowedToPlant()
        {
            // Sem estação atual (calendário ausente) não bloqueia.
            Assert.IsTrue(FarmSeasonGate.IsPlantingAllowed(new[] { "Verao" }, "", canOverrideSeason: false));
        }

        [Test]
        public void Greenhouse_RainExcluded_ForRegisteredPlotOnly()
        {
            var provider = new GreenhouseContextProvider();
            provider.RegisterGreenhousePlot("plot_201");

            Assert.IsTrue(provider.IsRainExcluded("plot_201"), "estufa excluída da chuva");
            Assert.IsFalse(provider.IsRainExcluded("plot_5"), "canteiro comum continua regado pela chuva");
        }

        [Test]
        public void Greenhouse_LockedProvider_DoesNotOverrideSeason()
        {
            var provider = new GreenhouseContextProvider();
            provider.RegisterGreenhousePlot("plot_200");
            // Sem SetGreenhouseUnlocked(true) → não sobrepõe.
            Assert.IsFalse(provider.CanOverrideSeason("plot_200"));
        }
    }
}
