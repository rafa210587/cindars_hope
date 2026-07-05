using System.Collections.Generic;
using CindarsHope.Farm.Animals;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Farm
{
    /// <summary>
    /// fable_12 — testes da camada de integração de animais de fazenda (registry de runtime + save).
    ///
    /// Cobre os critérios de aceite de forma determinística:
    /// CA-1 ciclo pecuário (fed → produz; sem ração não produz);
    /// CA-2 capacidade (5º recusado) + IDs estáveis/únicos (animal_&lt;housing&gt;_&lt;index&gt;);
    /// CA-3 persistência (round-trip; save legado/null = zero animais);
    /// EMENDA 5.1-A: morte permanente após N=7 dias sem comida (sem reviver);
    /// EMENDA §18: qualidade do produto por dias consecutivos de alimentação (silver/gold);
    /// idempotência de coleta.
    ///
    /// O registry é MonoBehaviour; criamos via GameObject (Awake roda em EditMode) e dirigimos
    /// ProcessDayTransition diretamente (sem depender do DayStartedEvent).
    /// </summary>
    [TestFixture]
    public class FarmAnimalsRuntimeTests
    {
        private GameObject _go;
        private FarmAnimalRegistry _registry;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("FarmAnimalRegistry_Test");
            _registry = _go.AddComponent<FarmAnimalRegistry>();
            FarmAnimalRuntimeBootstrap.RegisterCanonicalDefinitions(_registry);

            _registry.RegisterHousing("coop_01", AnimalHousingBuildingType.Coop, 4);
            _registry.RegisterHousing("barn_01", AnimalHousingBuildingType.Barn, 4);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.DestroyImmediate(_go);
            }
        }

        private string ReleaseChicken()
        {
            var result = _registry.TryRelease(FarmAnimalCatalog.AnimalChicken, "coop_01", out var id);
            Assert.AreEqual(FarmAnimalRegistry.ReleaseResult.Success, result, "Galinha deveria ser solta no galinheiro.");
            return id;
        }

        // ───────────────────────── CA-1: ciclo pecuário ─────────────────────────

        [Test]
        public void Cycle_FedAnimal_ProducesNextDay()
        {
            var id = ReleaseChicken();

            Assert.AreEqual(FarmAnimalRegistry.FeedResult.Success, _registry.Feed(id));
            _registry.ProcessDayTransition(1);

            var animal = _registry.GetAnimal(id);
            Assert.IsTrue(animal.ProductReady, "Animal alimentado ontem deve ter produto pronto hoje.");
        }

        [Test]
        public void Cycle_UnfedAnimal_DoesNotProduce()
        {
            var id = ReleaseChicken();

            // Sem alimentar: passa o dia.
            _registry.ProcessDayTransition(1);

            var animal = _registry.GetAnimal(id);
            Assert.IsFalse(animal.ProductReady, "Animal não alimentado não deve produzir.");
        }

        [Test]
        public void Collect_ReturnsBaseProduct_AndConsumesReadiness()
        {
            var id = ReleaseChicken();
            _registry.Feed(id);
            _registry.ProcessDayTransition(1);

            var result = _registry.Collect(id, out var productItemId, out var quantity);

            Assert.AreEqual(FarmAnimalRegistry.CollectResult.Success, result);
            Assert.AreEqual(FarmAnimalCatalog.ItemEgg, productItemId, "1 dia alimentado = qualidade Normal (ovo base).");
            Assert.AreEqual(1, quantity);
            Assert.IsFalse(_registry.GetAnimal(id).ProductReady, "ProductReady deve ser consumido após coleta.");
        }

        [Test]
        public void Collect_IsIdempotent_SecondCollectFails()
        {
            var id = ReleaseChicken();
            _registry.Feed(id);
            _registry.ProcessDayTransition(1);

            var first = _registry.Collect(id, out _, out _);
            var second = _registry.Collect(id, out _, out _);

            Assert.AreEqual(FarmAnimalRegistry.CollectResult.Success, first);
            Assert.AreEqual(FarmAnimalRegistry.CollectResult.ProductNotReady, second,
                "Coleta dupla no mesmo dia deve falhar (idempotência).");
        }

        // ───────────────── EMENDA §18: qualidade por dias consecutivos ─────────────────

        [Test]
        public void Quality_ThreeConsecutiveFedDays_YieldsSilver()
        {
            var id = ReleaseChicken();

            // 3 dias consecutivos alimentando; coleta no dia em que streak atinge 3.
            for (int day = 1; day <= 3; day++)
            {
                _registry.Feed(id);
                _registry.ProcessDayTransition(day);
            }

            Assert.AreEqual(3, _registry.GetConsecutiveFedDays(id));
            var result = _registry.Collect(id, out var productItemId, out _);
            Assert.AreEqual(FarmAnimalRegistry.CollectResult.Success, result);
            Assert.AreEqual(FarmAnimalCatalog.ItemEgg + "_silver", productItemId,
                ">=3 dias consecutivos = produto Prata (§18).");
        }

        [Test]
        public void Quality_SevenConsecutiveFedDays_YieldsGold()
        {
            var id = ReleaseChicken();

            for (int day = 1; day <= 7; day++)
            {
                _registry.Feed(id);
                _registry.ProcessDayTransition(day);
            }

            Assert.AreEqual(7, _registry.GetConsecutiveFedDays(id));
            var result = _registry.Collect(id, out var productItemId, out _);
            Assert.AreEqual(FarmAnimalRegistry.CollectResult.Success, result);
            Assert.AreEqual(FarmAnimalCatalog.ItemEgg + "_gold", productItemId,
                ">=7 dias consecutivos = produto Ouro (§18).");
        }

        [Test]
        public void Quality_ResolveProductItemId_DirectMapping()
        {
            Assert.AreEqual("item_animal_egg", FarmAnimalCatalog.ResolveProductItemId("item_animal_egg", 0));
            Assert.AreEqual("item_animal_egg", FarmAnimalCatalog.ResolveProductItemId("item_animal_egg", 2));
            Assert.AreEqual("item_animal_egg_silver", FarmAnimalCatalog.ResolveProductItemId("item_animal_egg", 3));
            Assert.AreEqual("item_animal_egg_silver", FarmAnimalCatalog.ResolveProductItemId("item_animal_egg", 6));
            Assert.AreEqual("item_animal_egg_gold", FarmAnimalCatalog.ResolveProductItemId("item_animal_egg", 7));
        }

        // ───────────────── EMENDA 5.1-A: morte permanente N=7 ─────────────────

        [Test]
        public void Neglect_SevenConsecutiveDaysWithoutFood_KillsPermanently()
        {
            var id = ReleaseChicken();

            // Nunca alimenta. Progressão: dia1 Hungry, dia3 Unavailable (doente/crítico), dia7 Dead.
            for (int day = 1; day <= 6; day++)
            {
                _registry.ProcessDayTransition(day);
                Assert.AreNotEqual(AnimalHealthState.Dead, _registry.GetAnimal(id).HealthState,
                    $"Animal não pode morrer antes de N=7 (dia {day}).");
            }

            _registry.ProcessDayTransition(7);
            Assert.AreEqual(AnimalHealthState.Dead, _registry.GetAnimal(id).HealthState,
                "Animal deve morrer permanentemente no 7º dia sem comida.");
        }

        [Test]
        public void Neglect_ProgressiveWarnings_HungryThenCritical()
        {
            var id = ReleaseChicken();

            _registry.ProcessDayTransition(1);
            Assert.AreEqual(AnimalHealthState.Hungry, _registry.GetAnimal(id).HealthState, "Dia 1 sem comida = faminto.");

            _registry.ProcessDayTransition(2);
            _registry.ProcessDayTransition(3);
            Assert.AreEqual(AnimalHealthState.Unavailable, _registry.GetAnimal(id).HealthState,
                "Dia 3 sem comida = doente/crítico.");
        }

        [Test]
        public void Neglect_FeedingResetsCountdown_PreventsDeath()
        {
            var id = ReleaseChicken();

            // 6 dias sem comida, então alimenta — não pode morrer.
            for (int day = 1; day <= 6; day++)
            {
                _registry.ProcessDayTransition(day);
            }
            Assert.AreEqual(FarmAnimalRegistry.FeedResult.Success, _registry.Feed(id),
                "Animal ainda vivo (doente) deve poder ser alimentado.");
            _registry.ProcessDayTransition(7);

            Assert.AreNotEqual(AnimalHealthState.Dead, _registry.GetAnimal(id).HealthState,
                "Alimentar antes do 7º dia consecutivo evita a morte.");
            Assert.AreEqual(0, _registry.GetAnimal(id).DaysWithoutFood, "Alimentar zera o contador de negligência.");
        }

        [Test]
        public void Death_IsPermanent_CannotFeedDead()
        {
            var id = ReleaseChicken();
            for (int day = 1; day <= 7; day++)
            {
                _registry.ProcessDayTransition(day);
            }

            Assert.AreEqual(AnimalHealthState.Dead, _registry.GetAnimal(id).HealthState);
            Assert.AreEqual(FarmAnimalRegistry.FeedResult.AnimalDead, _registry.Feed(id),
                "Animal morto não pode ser alimentado (sem reviver).");
        }

        [Test]
        public void Death_RemovesAnimalFromHousingCapacity()
        {
            var id = ReleaseChicken();
            var housing = _registry.GetHousing("coop_01");
            Assert.AreEqual(1, housing.AnimalInstanceIds.Count);

            for (int day = 1; day <= 7; day++)
            {
                _registry.ProcessDayTransition(day);
            }

            Assert.IsFalse(housing.AnimalInstanceIds.Contains(id),
                "Animal morto deve liberar a vaga do abrigo.");
        }

        // ───────────────────────── CA-2: capacidade + IDs ─────────────────────────

        [Test]
        public void Capacity_FifthAnimalInCoopOfFour_IsRejected()
        {
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(FarmAnimalRegistry.ReleaseResult.Success,
                    _registry.TryRelease(FarmAnimalCatalog.AnimalChicken, "coop_01", out _));
            }

            var fifth = _registry.TryRelease(FarmAnimalCatalog.AnimalChicken, "coop_01", out var fifthId);
            Assert.AreEqual(FarmAnimalRegistry.ReleaseResult.HousingFull, fifth,
                "5º animal em galinheiro de capacidade 4 deve ser recusado.");
            Assert.IsNull(fifthId);
        }

        [Test]
        public void Ids_AreDeterministicAndUnique()
        {
            _registry.TryRelease(FarmAnimalCatalog.AnimalChicken, "coop_01", out var id0);
            _registry.TryRelease(FarmAnimalCatalog.AnimalChicken, "coop_01", out var id1);

            Assert.AreEqual("animal_coop_01_0", id0);
            Assert.AreEqual("animal_coop_01_1", id1);
            Assert.AreNotEqual(id0, id1);
        }

        [Test]
        public void Release_WrongHousingForSpecies_IsRejected()
        {
            // Vaca exige Barn; tentar no galinheiro deve falhar.
            var result = _registry.TryRelease(FarmAnimalCatalog.AnimalCow, "coop_01", out var id);
            Assert.AreEqual(FarmAnimalRegistry.ReleaseResult.HousingNotFound, result);
            Assert.IsNull(id);
        }

        // ───────────────────────── CA-3: persistência ─────────────────────────

        [Test]
        public void Save_RoundTrip_PreservesAnimalsAndState()
        {
            var id = ReleaseChicken();
            _registry.Feed(id);
            _registry.ProcessDayTransition(1); // produto pronto, streak=1

            var saveData = _registry.CaptureSaveData();
            Assert.AreEqual(1, saveData.Animals.Count);

            // Novo registry simula reload.
            var go2 = new GameObject("FarmAnimalRegistry_Reload");
            var reload = go2.AddComponent<FarmAnimalRegistry>();
            FarmAnimalRuntimeBootstrap.RegisterCanonicalDefinitions(reload);
            reload.RegisterHousing("coop_01", AnimalHousingBuildingType.Coop, 4);

            reload.RestoreFromSaveData(saveData);

            var restored = reload.GetAnimal(id);
            Assert.IsNotNull(restored, "Animal deve sobreviver ao round-trip.");
            Assert.AreEqual(FarmAnimalCatalog.AnimalChicken, restored.AnimalDataId);
            Assert.AreEqual("coop_01", restored.HomeBuildingId);
            Assert.AreEqual(1, reload.GetConsecutiveFedDays(id), "Streak de alimentação deve persistir.");
            Assert.IsTrue(reload.GetHousing("coop_01").AnimalInstanceIds.Contains(id),
                "Animal vivo deve voltar à associação do abrigo após load.");

            Object.DestroyImmediate(go2);
        }

        [Test]
        public void Save_LegacyNull_LoadsZeroAnimals()
        {
            ReleaseChicken();
            _registry.RestoreFromSaveData(null);

            Assert.AreEqual(0, _registry.CaptureSaveData().Animals.Count,
                "Save legado (null) deve carregar com zero animais (CA-3).");
        }

        [Test]
        public void Save_EmptySection_LoadsZeroAnimals()
        {
            _registry.RestoreFromSaveData(new FarmAnimalsSaveData());
            Assert.AreEqual(0, _registry.CaptureSaveData().Animals.Count);
        }

        [Test]
        public void Save_DeadAnimal_RestoresAsDead_AndNotInHousing()
        {
            var id = ReleaseChicken();
            for (int day = 1; day <= 7; day++)
            {
                _registry.ProcessDayTransition(day);
            }

            var saveData = _registry.CaptureSaveData();

            var go2 = new GameObject("FarmAnimalRegistry_ReloadDead");
            var reload = go2.AddComponent<FarmAnimalRegistry>();
            FarmAnimalRuntimeBootstrap.RegisterCanonicalDefinitions(reload);
            reload.RegisterHousing("coop_01", AnimalHousingBuildingType.Coop, 4);
            reload.RestoreFromSaveData(saveData);

            Assert.AreEqual(AnimalHealthState.Dead, reload.GetAnimal(id).HealthState);
            Assert.IsFalse(reload.GetHousing("coop_01").AnimalInstanceIds.Contains(id),
                "Animal morto não ocupa vaga após reload.");

            Object.DestroyImmediate(go2);
        }

        [Test]
        public void Save_IdCounter_StaysUniqueAfterRestore()
        {
            _registry.TryRelease(FarmAnimalCatalog.AnimalChicken, "coop_01", out _); // _0
            _registry.TryRelease(FarmAnimalCatalog.AnimalChicken, "coop_01", out _); // _1
            var saveData = _registry.CaptureSaveData();

            var go2 = new GameObject("FarmAnimalRegistry_ReloadCounter");
            var reload = go2.AddComponent<FarmAnimalRegistry>();
            FarmAnimalRuntimeBootstrap.RegisterCanonicalDefinitions(reload);
            reload.RegisterHousing("coop_01", AnimalHousingBuildingType.Coop, 4);
            reload.RestoreFromSaveData(saveData);

            reload.TryRelease(FarmAnimalCatalog.AnimalChicken, "coop_01", out var nextId);
            Assert.AreEqual("animal_coop_01_2", nextId,
                "Contador de IDs deve continuar à frente dos índices restaurados.");

            Object.DestroyImmediate(go2);
        }

        // ───────────────────────── Catálogo ─────────────────────────

        [Test]
        public void Catalog_HasFourCanonicalAnimals()
        {
            var all = FarmAnimalCatalog.GetAll();
            Assert.AreEqual(4, all.Count);
        }

        [Test]
        public void Catalog_ProductItemIdsMatchCanonicalCatalog()
        {
            var byId = new Dictionary<string, FarmAnimalCatalog.AnimalCatalogEntry>();
            foreach (var e in FarmAnimalCatalog.GetAll())
            {
                byId[e.AnimalId] = e;
            }

            Assert.AreEqual("item_animal_egg", byId[FarmAnimalCatalog.AnimalChicken].ProductItemId);
            Assert.AreEqual("item_animal_goat_milk", byId[FarmAnimalCatalog.AnimalGoat].ProductItemId);
            Assert.AreEqual("item_animal_cow_milk", byId[FarmAnimalCatalog.AnimalCow].ProductItemId);
            Assert.AreEqual("item_animal_wool", byId[FarmAnimalCatalog.AnimalSheep].ProductItemId);
        }
    }
}
