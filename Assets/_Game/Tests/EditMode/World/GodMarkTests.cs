using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.World.Altars;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_68 — testes determinísticos das Marcas dos Deuses (CA-1..CA-5 + save round-trip).
    /// 100% EditMode / pure C# (sem Unity, sem cena): catálogo, regras de oração 1/dia, expiração ao
    /// virar o dia, não-stack com relíquia do mesmo deus, condicionais e determinismo do sorteio na
    /// caverna.
    /// </summary>
    [TestFixture]
    public class GodMarkTests
    {
        private static DelegateGodMarkWorldContext Ctx(
            int day = 1, bool night = false, bool alihana = false, bool senya = false,
            bool hasCrop = false, MarkGod relicGod = MarkGod.None)
        {
            var consumed = false;
            return new DelegateGodMarkWorldContext
            {
                DayNumberSource = () => day,
                IsNightSource = () => night,
                IsAlihanaLunarPeakSource = () => alihana,
                IsSenyaFestivalOrPeakSource = () => senya,
                HasCropOfferingSource = () => hasCrop && !consumed,
                ConsumeCropOfferingSource = () => { if (!hasCrop || consumed) return false; consumed = true; return true; },
                EquippedRelicGodSource = () => relicGod
            };
        }

        // ── CA-1 — Catálogo completo e fiel ──────────────────────────────────────────────────────

        [Test]
        public void Catalog_HasElevenMarks()
        {
            Assert.AreEqual(11, GodMarkCatalog.All.Count);
        }

        [Test]
        public void Catalog_FiveCaveAndSixOthers()
        {
            int cave = 0, other = 0;
            foreach (var d in GodMarkCatalog.All)
            {
                if (d.Location == MarkLocation.Cave) cave++; else other++;
            }

            Assert.AreEqual(5, cave, "5 Marcas de caverna");
            Assert.AreEqual(6, other, "5 cidade/fazenda + 1 Anya");
        }

        [Test]
        public void Catalog_AnyaIsLoreOnly_NoBonus()
        {
            var anya = GodMarkCatalog.GetById(GodMarkCatalog.AnyaWaters);
            Assert.IsNotNull(anya);
            Assert.AreEqual(MarkGod.Anya, anya.God);
            Assert.IsTrue(anya.IsLoreOnly, "Anya nunca tem bonus de oracao");
            Assert.AreEqual(MarkEffectType.None, anya.Effect);
            Assert.AreEqual(0f, anya.Magnitude, 0.0001f);
        }

        [Test]
        public void Catalog_EffectsMatchAppendixA5()
        {
            AssertMark(GodMarkCatalog.FinanCoin, MarkGod.Finan, MarkEffectType.GoldFindPercent, 0.05f, MarkCondition.None);
            AssertMark(GodMarkCatalog.ThorenAnvil, MarkGod.Thoren, MarkEffectType.DurabilityLossReductionPercent, 0.05f, MarkCondition.None);
            AssertMark(GodMarkCatalog.KaandStone, MarkGod.Kaand, MarkEffectType.DamageDealtAndTakenPercent, 0.03f, MarkCondition.None);
            AssertMark(GodMarkCatalog.NyxPool, MarkGod.Nyx, MarkEffectType.SecretItemChancePercent, 0.05f, MarkCondition.NightOnly);
            AssertMark(GodMarkCatalog.TandraRoot, MarkGod.Tandra, MarkEffectType.BeastDropPercent, 0.05f, MarkCondition.None);
            AssertMark(GodMarkCatalog.ThandraNiche, MarkGod.Thandra, MarkEffectType.TomorrowSilverPercent, 0.02f, MarkCondition.CropOffering);
            AssertMark(GodMarkCatalog.KanthorOath, MarkGod.Kanthor, MarkEffectType.BlockStabilityPercent, 0.02f, MarkCondition.None);
            AssertMark(GodMarkCatalog.MerithusSeal, MarkGod.Merithus, MarkEffectType.ShippingValuePercent, 0.02f, MarkCondition.None);
            AssertMark(GodMarkCatalog.AlihanaMirror, MarkGod.Alihana, MarkEffectType.RareSeedChancePercent, 0.05f, MarkCondition.AlihanaLunarPeak);
            AssertMark(GodMarkCatalog.SenyaMast, MarkGod.Senya, MarkEffectType.MagicXpPercent, 0.05f, MarkCondition.SenyaFestivalOrPeak);
        }

        [Test]
        public void Catalog_AllBonusesAtMostFivePercent()
        {
            foreach (var d in GodMarkCatalog.All)
            {
                Assert.LessOrEqual(d.Magnitude, 0.05f + 0.0001f, $"{d.Id} bonus <= 5%");
            }
        }

        private static void AssertMark(string id, MarkGod god, MarkEffectType effect, float mag, MarkCondition cond)
        {
            var d = GodMarkCatalog.GetById(id);
            Assert.IsNotNull(d, id);
            Assert.AreEqual(god, d.God, $"{id} god");
            Assert.AreEqual(effect, d.Effect, $"{id} effect");
            Assert.AreEqual(mag, d.Magnitude, 0.0001f, $"{id} magnitude");
            Assert.AreEqual(cond, d.Condition, $"{id} condition");
        }

        // ── CA-2 — Oração diária com expiração ───────────────────────────────────────────────────

        [Test]
        public void Pray_GrantsBuff_FirstTime()
        {
            var svc = new GodMarkService();
            var r = svc.TryPray(GodMarkCatalog.KanthorOath, Ctx(day: 1));
            Assert.AreEqual(GodMarkPrayResult.Granted, r);
            Assert.IsTrue(svc.HasPrayedToday(GodMarkCatalog.KanthorOath));
            Assert.Greater(svc.GetActiveEffectMagnitude(MarkEffectType.BlockStabilityPercent), 0f);
        }

        [Test]
        public void Pray_SecondTimeSameDay_Refused()
        {
            var svc = new GodMarkService();
            svc.TryPray(GodMarkCatalog.KanthorOath, Ctx(day: 1));
            var r2 = svc.TryPray(GodMarkCatalog.KanthorOath, Ctx(day: 1));
            Assert.AreEqual(GodMarkPrayResult.AlreadyPrayedToday, r2);
        }

        [Test]
        public void DayRollover_ExpiresBuffAndResetsFlag()
        {
            var svc = new GodMarkService();
            svc.TryPray(GodMarkCatalog.KanthorOath, Ctx(day: 1));
            Assert.Greater(svc.GetActiveEffectMagnitude(MarkEffectType.BlockStabilityPercent), 0f);

            svc.OnDayStarted(2);
            Assert.AreEqual(0f, svc.GetActiveEffectMagnitude(MarkEffectType.BlockStabilityPercent), 0.0001f, "buff expira ao dormir");
            Assert.IsFalse(svc.HasPrayedToday(GodMarkCatalog.KanthorOath), "flag reseta");

            // Pode orar de novo no novo dia.
            var r = svc.TryPray(GodMarkCatalog.KanthorOath, Ctx(day: 2));
            Assert.AreEqual(GodMarkPrayResult.Granted, r);
        }

        // ── CA-3 — Não-stack com relíquia do mesmo deus ──────────────────────────────────────────

        [Test]
        public void NonStack_SameGodRelic_TakesMax_NotSum()
        {
            var svc = new GodMarkService();
            // Kanthor: Marca +2% block stability; relíquia hipotética do mesmo deus daria +10%.
            svc.TryPray(GodMarkCatalog.KanthorOath, Ctx(day: 1, relicGod: MarkGod.Kanthor));

            var relicMagnitude = 0.10f;
            var effective = svc.GetEffectiveMarkMagnitude(MarkEffectType.BlockStabilityPercent, MarkGod.Kanthor, relicMagnitude);
            Assert.AreEqual(0.10f, effective, 0.0001f, "vale o MAIOR (reliquia), nunca a soma");

            // Marca maior que a relíquia ⇒ vale a Marca.
            var effective2 = svc.GetEffectiveMarkMagnitude(MarkEffectType.BlockStabilityPercent, MarkGod.Kanthor, 0.01f);
            Assert.AreEqual(0.02f, effective2, 0.0001f);
        }

        [Test]
        public void NonStack_FlagForFourRelicGods()
        {
            var svc = new GodMarkService();
            // Os 4 deuses com relíquia (Kanthor/Kaand/Anya/Alihana) — não-stack quando relíquia do mesmo deus.
            Assert.IsTrue(svc.IsNonStackingWithRelic(MarkGod.Kanthor, MarkGod.Kanthor));
            Assert.IsTrue(svc.IsNonStackingWithRelic(MarkGod.Kaand, MarkGod.Kaand));
            Assert.IsTrue(svc.IsNonStackingWithRelic(MarkGod.Anya, MarkGod.Anya));
            Assert.IsTrue(svc.IsNonStackingWithRelic(MarkGod.Alihana, MarkGod.Alihana));
            // Deuses diferentes stackam normalmente.
            Assert.IsFalse(svc.IsNonStackingWithRelic(MarkGod.Kanthor, MarkGod.Kaand));
            Assert.IsFalse(svc.IsNonStackingWithRelic(MarkGod.Finan, MarkGod.None));
        }

        // ── CA-4 — Determinismo na caverna ───────────────────────────────────────────────────────

        [Test]
        public void CaveResolve_SameSeedAndLevel_SameResult()
        {
            const string seed = "RUN_SEED_ABC";
            for (var level = 1; level <= 100; level++)
            {
                var a = GodMarkCaveResolver.Resolve(seed, level);
                var b = GodMarkCaveResolver.Resolve(seed, level);
                Assert.AreEqual(a?.Id, b?.Id, $"nivel {level}: revisita deve dar a MESMA Marca");
            }
        }

        [Test]
        public void CaveResolve_RespectsBands()
        {
            const string seed = "RUN_SEED_BANDS";
            for (var level = 1; level <= 100; level++)
            {
                var d = GodMarkCaveResolver.Resolve(seed, level);
                if (d == null) continue;
                Assert.GreaterOrEqual(level, d.CaveBandMin, $"{d.Id} fora da banda (min) no nivel {level}");
                Assert.LessOrEqual(level, d.CaveBandMax, $"{d.Id} fora da banda (max) no nivel {level}");
                // Nunca uma marca não-caverna.
                Assert.AreEqual(MarkLocation.Cave, d.Location);
            }
        }

        [Test]
        public void CaveResolve_NyxOnlyInBand71to85_TandraOnly11to25()
        {
            const string seed = "RUN_SEED_X";
            for (var level = 1; level <= 100; level++)
            {
                var d = GodMarkCaveResolver.Resolve(seed, level);
                if (d == null) continue;
                if (d.Id == GodMarkCatalog.NyxPool)
                {
                    Assert.IsTrue(level >= 71 && level <= 85, $"Nyx em nivel {level}");
                }
                else if (d.Id == GodMarkCatalog.TandraRoot)
                {
                    Assert.IsTrue(level >= 11 && level <= 25, $"Tandra em nivel {level}");
                }
            }
        }

        [Test]
        public void CaveResolve_DifferentSeeds_CanDiffer()
        {
            // Pelo menos um nivel difere entre duas seeds distintas (sorteio realmente usa a seed).
            var anyDiff = false;
            for (var level = 1; level <= 100 && !anyDiff; level++)
            {
                var a = GodMarkCaveResolver.Resolve("SEED_A", level)?.Id;
                var b = GodMarkCaveResolver.Resolve("SEED_B", level)?.Id;
                if (a != b) anyDiff = true;
            }

            Assert.IsTrue(anyDiff, "seeds diferentes devem divergir em algum nivel");
        }

        // ── CA-5 — Condicionais ──────────────────────────────────────────────────────────────────

        [Test]
        public void Nyx_RefusedByDay_GrantedAtNight()
        {
            var svc = new GodMarkService();
            Assert.AreEqual(GodMarkPrayResult.ConditionNotMet,
                svc.TryPray(GodMarkCatalog.NyxPool, Ctx(day: 1, night: false)));

            var svc2 = new GodMarkService();
            Assert.AreEqual(GodMarkPrayResult.Granted,
                svc2.TryPray(GodMarkCatalog.NyxPool, Ctx(day: 1, night: true)));
        }

        [Test]
        public void Alihana_RequiresLunarPeak()
        {
            var svc = new GodMarkService();
            Assert.AreEqual(GodMarkPrayResult.ConditionNotMet,
                svc.TryPray(GodMarkCatalog.AlihanaMirror, Ctx(day: 1, alihana: false)));

            var svc2 = new GodMarkService();
            Assert.AreEqual(GodMarkPrayResult.Granted,
                svc2.TryPray(GodMarkCatalog.AlihanaMirror, Ctx(day: 1, alihana: true)));
        }

        [Test]
        public void Senya_RequiresFestivalOrPeak()
        {
            var svc = new GodMarkService();
            Assert.AreEqual(GodMarkPrayResult.ConditionNotMet,
                svc.TryPray(GodMarkCatalog.SenyaMast, Ctx(day: 1, senya: false)));

            var svc2 = new GodMarkService();
            Assert.AreEqual(GodMarkPrayResult.Granted,
                svc2.TryPray(GodMarkCatalog.SenyaMast, Ctx(day: 1, senya: true)));
        }

        [Test]
        public void Thandra_RequiresCropOffering_AndConsumesIt()
        {
            var svc = new GodMarkService();
            Assert.AreEqual(GodMarkPrayResult.OfferingMissing,
                svc.TryPray(GodMarkCatalog.ThandraNiche, Ctx(day: 1, hasCrop: false)));

            var svc2 = new GodMarkService();
            var ctx = Ctx(day: 1, hasCrop: true);
            Assert.AreEqual(GodMarkPrayResult.Granted, svc2.TryPray(GodMarkCatalog.ThandraNiche, ctx));
            // Oferenda foi consumida: a fonte agora reporta sem crop.
            Assert.IsFalse(ctx.HasCropOffering, "1 crop consumido como oferenda");
        }

        [Test]
        public void Thandra_BonusAppliesNextDay()
        {
            var svc = new GodMarkService();
            svc.TryPray(GodMarkCatalog.ThandraNiche, Ctx(day: 1, hasCrop: true));
            // Hoje ainda não há bônus de Silver ativo (é "amanhã").
            Assert.AreEqual(0f, svc.GetActiveEffectMagnitude(MarkEffectType.TomorrowSilverPercent), 0.0001f);

            svc.OnDayStarted(2);
            // Amanhã o bônus está ativo.
            Assert.AreEqual(0.02f, svc.GetActiveEffectMagnitude(MarkEffectType.TomorrowSilverPercent), 0.0001f);

            // E expira no dia seguinte.
            svc.OnDayStarted(3);
            Assert.AreEqual(0f, svc.GetActiveEffectMagnitude(MarkEffectType.TomorrowSilverPercent), 0.0001f);
        }

        [Test]
        public void Anya_PrayIsLoreOnly_NoFlagNoBuff()
        {
            var svc = new GodMarkService();
            var r = svc.TryPray(GodMarkCatalog.AnyaWaters, Ctx(day: 1));
            Assert.AreEqual(GodMarkPrayResult.LoreOnly, r);
            Assert.IsFalse(svc.HasPrayedToday(GodMarkCatalog.AnyaWaters), "Anya nao gasta flag diaria");
            Assert.AreEqual(0, svc.ActiveBuffs.Count, "Anya nao concede buff");
        }

        [Test]
        public void UnknownMark_ReturnsUnknown()
        {
            var svc = new GodMarkService();
            Assert.AreEqual(GodMarkPrayResult.UnknownMark, svc.TryPray("mark_does_not_exist", Ctx()));
        }

        // ── Save round-trip (Testing Quality Gate) ───────────────────────────────────────────────

        [Test]
        public void Save_RoundTrip_PreservesState()
        {
            var svc = new GodMarkService();
            svc.TryPray(GodMarkCatalog.KanthorOath, Ctx(day: 3));
            svc.TryPray(GodMarkCatalog.ThandraNiche, Ctx(day: 3, hasCrop: true));

            var data = svc.Capture();
            Assert.AreEqual(3, data.DayNumber);
            Assert.Contains(GodMarkCatalog.KanthorOath, data.PrayedTodayMarkIds);
            Assert.AreEqual(1, data.ActiveBuffs.Count, "Kanthor ativo");
            Assert.AreEqual(1, data.PendingTomorrowBonuses.Count, "Thandra pendente p/ amanha");

            var restored = new GodMarkService();
            restored.Restore(data);
            Assert.AreEqual(3, restored.CurrentDay);
            Assert.IsTrue(restored.HasPrayedToday(GodMarkCatalog.KanthorOath));
            Assert.Greater(restored.GetActiveEffectMagnitude(MarkEffectType.BlockStabilityPercent), 0f);

            // Pendência preservada: aplica amanhã.
            restored.OnDayStarted(4);
            Assert.AreEqual(0.02f, restored.GetActiveEffectMagnitude(MarkEffectType.TomorrowSilverPercent), 0.0001f);
        }

        [Test]
        public void Restore_Null_YieldsEmptyState()
        {
            var svc = new GodMarkService();
            svc.TryPray(GodMarkCatalog.KanthorOath, Ctx(day: 1));
            svc.Restore(null);
            Assert.AreEqual(0, svc.CurrentDay);
            Assert.AreEqual(0, svc.ActiveBuffs.Count);
            Assert.IsFalse(svc.HasPrayedToday(GodMarkCatalog.KanthorOath));
        }

        [Test]
        public void Restore_LegacyMissingSection_NeverPrayed()
        {
            // Migração: seção ausente = DTO default (DayNumber 0, listas vazias).
            var svc = new GodMarkService();
            svc.Restore(new GodMarkSaveData());
            Assert.AreEqual(0, svc.CurrentDay);
            Assert.IsFalse(svc.HasPrayedToday(GodMarkCatalog.FinanCoin));
        }
    }
}
