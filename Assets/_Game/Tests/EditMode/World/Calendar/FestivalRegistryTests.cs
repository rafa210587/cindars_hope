using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using CindarsHope.World.Calendar;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World.Calendar
{
    /// <summary>
    /// spec_cleanup_uac_serialization_6000_5_v1 — caracterizacao do Caso 2 (UAC1010): adicionar
    /// [System.Serializable] a struct aninhada Festival NAO pode mudar os festivais default, pois
    /// InitializeDefaultFestivals so popula _festivals quando Count == 0.
    ///
    /// O teste varre o ano inteiro e coleta os festivais que disparam, em vez de depender do dia exato:
    /// GetFestivalOnDay casa `festival.DayInYear == date.DayInSeason + season*28`, e como DayInSeason e
    /// 1-indexed, o valor efetivo e `dayInYear + 1` (convencao pre-existente do calendario). Varrer
    /// evita acoplar o teste a esse off-by-one.
    /// </summary>
    [TestFixture]
    public class FestivalRegistryTests
    {
        [Test]
        public void InitializeDefaultFestivals_ProducesThe3CanonicalFestivals()
        {
            var go = new GameObject("TestFestivalRegistry");
            var registry = go.AddComponent<FestivalRegistry>();

            try
            {
                // EditMode NAO dispara OnEnable de forma confiavel ao AddComponent, entao invocamos
                // InitializeDefaultFestivals explicitamente (idempotente: so popula se _festivals.Count == 0).
                typeof(FestivalRegistry)
                    .GetMethod("InitializeDefaultFestivals", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(registry, null);

                var found = new Dictionary<FestivalType, FestivalRegistry.Festival>();
                for (int absDay = 1; absDay <= GameDate.DaysPerYear; absDay++)
                {
                    var f = registry.GetFestivalOnDay(GameDate.FromAbsoluteDay(absDay));
                    if (f.Type != FestivalType.None)
                    {
                        found[f.Type] = f;
                    }
                }

                Assert.AreEqual(3, found.Count, "Exatamente 3 festivais default (o [Serializable] nao pode mudar isso).");

                Assert.IsTrue(found.ContainsKey(FestivalType.PlantingFestival), "PlantingFestival ausente.");
                Assert.AreEqual("Planting Festival", found[FestivalType.PlantingFestival].DisplayName);
                Assert.IsFalse(found[FestivalType.PlantingFestival].IsHidden);

                Assert.IsTrue(found.ContainsKey(FestivalType.MarketFestival), "MarketFestival ausente.");
                Assert.AreEqual("Market Festival", found[FestivalType.MarketFestival].DisplayName);

                Assert.IsTrue(found.ContainsKey(FestivalType.HarvestFestival), "HarvestFestival ausente.");
                Assert.AreEqual("Harvest Festival", found[FestivalType.HarvestFestival].DisplayName);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
