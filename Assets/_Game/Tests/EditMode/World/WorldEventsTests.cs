using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.World.Calendar;
using CindarsHope.World.Events;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_37 — cobre os critérios de aceite do WorldEventService de forma PURA (sem Unity scene):
    /// CA-1 agenda determinística dos 8 festivais; CA-2 efeito de pico (Lua Verde via hook + economia/
    /// spawn/fonte); CA-3 determinismo do evento aleatório por StableHash(worldSeed|dia); CA-4 mural sem
    /// spoiler; gate da loja noturna (Nyx + quest); regressão do dia comum (sem efeito ativo = neutro).
    /// </summary>
    [TestFixture]
    public class WorldEventsTests
    {
        [TearDown]
        public void TearDown()
        {
            // Garante que os hooks estáticos não vazam entre testes.
            WorldEventHooks.Reset();
        }

        // Helper: AbsoluteDay para (estação, diaNaEstação) no ano 1.
        private static int AbsoluteDay(Season season, int dayInSeason)
        {
            return (int)season * GameDate.DaysPerSeason + dayInSeason; // ano 1
        }

        // ── CA-1 — Festival no dia certo (agenda determinística dos 8) ────────────────────────────

        [Test]
        public void CA1_AllEightFestivals_ResolveOnCanonicalDays()
        {
            var expected = new (Season Season, int Day, string Id)[]
            {
                (Season.Primavera, 7,  "festival_plantio"),
                (Season.Primavera, 21, "festival_caravana"),
                (Season.Verao,     14, "festival_luas"),
                (Season.Verao,     28, "festival_torneio"),
                (Season.Outono,    7,  "festival_colheita"),
                (Season.Outono,    21, "festival_veus"),
                (Season.Inverno,   14, "festival_vigilia"),
                (Season.Inverno,   28, "festival_ano_novo"),
            };

            foreach (var e in expected)
            {
                var day = AbsoluteDay(e.Season, e.Day);
                var festival = WorldEventResolver.ResolveFestival(day);
                Assert.IsNotNull(festival, $"Esperava festival em {e.Season} d{e.Day}.");
                Assert.AreEqual(e.Id, festival.FestivalId, $"Festival errado em {e.Season} d{e.Day}.");
            }
        }

        [Test]
        public void CA1_NoFestival_OnNonFestivalDay()
        {
            // Primavera d10 não é dia de festival.
            var festival = WorldEventResolver.ResolveFestival(AbsoluteDay(Season.Primavera, 10));
            Assert.IsNull(festival);
        }

        [Test]
        public void CA1_FestivalClearsNextDay_ResolutionFlag()
        {
            var festivalDay = AbsoluteDay(Season.Primavera, 7);
            var resOn = WorldEventResolver.ResolveDay("seed", festivalDay);
            Assert.IsTrue(resOn.HasFestival);
            Assert.AreEqual("festival_plantio", resOn.FestivalId);

            var resNext = WorldEventResolver.ResolveDay("seed", festivalDay + 1);
            Assert.IsFalse(resNext.HasFestival, "O festival deve limpar no dia seguinte (derivável).");
        }

        // ── CA-2 — Pico de Lua Verde avança crops (via hook nomeado) ──────────────────────────────

        [Test]
        public void CA2_GreenMoonPeak_ResolvesOnDay14_WithGrowthEffect()
        {
            var resolution = WorldEventResolver.ResolveDay("seed", AbsoluteDay(Season.Primavera, 14));
            Assert.AreEqual("peak_green", resolution.LunarPeakId);
            Assert.AreEqual(WorldEventDefinitions.LunarPeakEffect.GreenCropGrowth, resolution.LunarEffect);
            Assert.AreEqual(1f, resolution.LunarMagnitude);
        }

        [Test]
        public void CA2_GreenMoonHook_ReportsExtraStages_OnlyOnGreenPeak()
        {
            WorldEventHooks.Active = WorldEventResolver.ResolveDay("seed", AbsoluteDay(Season.Primavera, 14));
            Assert.IsTrue(WorldEventHooks.IsGreenMoonGrowthDay());
            Assert.AreEqual(1, WorldEventHooks.GetGreenMoonExtraStages());

            // Dia sem pico verde: hook neutro.
            WorldEventHooks.Active = WorldEventResolver.ResolveDay("seed", AbsoluteDay(Season.Primavera, 15));
            Assert.IsFalse(WorldEventHooks.IsGreenMoonGrowthDay());
            Assert.AreEqual(0, WorldEventHooks.GetGreenMoonExtraStages());
        }

        [Test]
        public void CA2_AmberPeak_SellMultiplier_Plus10Percent()
        {
            WorldEventHooks.Active = WorldEventResolver.ResolveDay("seed", AbsoluteDay(Season.Outono, 21));
            Assert.AreEqual("peak_amber", WorldEventHooks.Active.LunarPeakId);
            // 100 ouro + Lua Âmbar (+10%) => 110.
            Assert.AreEqual(110, WorldEventHooks.ApplySellGold(100));
        }

        [Test]
        public void CA2_AshPeak_CaveSpawnMultiplier_Above1()
        {
            WorldEventHooks.Active = WorldEventResolver.ResolveDay("seed", AbsoluteDay(Season.Primavera, 7));
            Assert.AreEqual("peak_ash", WorldEventHooks.Active.LunarPeakId);
            Assert.Greater(WorldEventHooks.GetCaveSpawnMultiplier(), 1f);
        }

        [Test]
        public void CA2_PalePeak_ExtraAguaViva_OnDay28()
        {
            WorldEventHooks.Active = WorldEventResolver.ResolveDay("seed", AbsoluteDay(Season.Inverno, 28));
            Assert.AreEqual("peak_pale", WorldEventHooks.Active.LunarPeakId);
            Assert.AreEqual(1, WorldEventHooks.GetExtraAguaViva());
        }

        // ── CA-3 — Determinismo do evento aleatório (StableHash(worldSeed|dia)) ────────────────────

        [Test]
        public void CA3_SameSeedSameDay_SameEvent_Always()
        {
            const string seed = "world_seed_123";
            const int day = 305;
            var first = WorldEventResolver.ResolveRandomEvent(seed, day);
            for (var i = 0; i < 50; i++)
            {
                var again = WorldEventResolver.ResolveRandomEvent(seed, day);
                Assert.AreEqual(first?.WorldEventId, again?.WorldEventId,
                    "Mesmo seed+dia deve sempre produzir o mesmo evento (ou ausência).");
            }
        }

        [Test]
        public void CA3_DifferentSeeds_ProduceVariation_OverManyDays()
        {
            // Ao longo de muitos dias, dois seeds diferentes devem divergir em pelo menos um dia.
            var diverged = false;
            for (var day = 1; day <= 60 && !diverged; day++)
            {
                var a = WorldEventResolver.ResolveRandomEvent("seedA", day)?.WorldEventId ?? "";
                var b = WorldEventResolver.ResolveRandomEvent("seedB", day)?.WorldEventId ?? "";
                if (a != b) diverged = true;
            }

            Assert.IsTrue(diverged, "Seeds diferentes devem variar o pool ao longo de 60 dias.");
        }

        [Test]
        public void CA3_ResolveDay_IsFullyRecomputable_AfterReload()
        {
            const string seed = "persist_seed";
            const int day = 411;
            var before = WorldEventResolver.ResolveDay(seed, day);
            // Simula "reload": recomputa do zero (nada persistido).
            var after = WorldEventResolver.ResolveDay(seed, day);
            Assert.AreEqual(before.FestivalId, after.FestivalId);
            Assert.AreEqual(before.LunarPeakId, after.LunarPeakId);
            Assert.AreEqual(before.WorldEventId, after.WorldEventId);
        }

        [Test]
        public void CA3_DroughtEvent_IsReachable_AndDeterministic()
        {
            // Encontra um dia que dispara seca para seedA; confirma estabilidade.
            int droughtDay = -1;
            for (var day = 1; day <= 400; day++)
            {
                var e = WorldEventResolver.ResolveRandomEvent("seedA", day);
                if (e != null && e.Effect == WorldEventDefinitions.WorldEventEffect.Drought)
                {
                    droughtDay = day;
                    break;
                }
            }

            Assert.Greater(droughtDay, 0, "A seca deve ser alcançável no pool em 400 dias.");
            WorldEventHooks.Active = WorldEventResolver.ResolveDay("seedA", droughtDay);
            Assert.IsTrue(WorldEventHooks.IsDroughtToday());
        }

        // ── CA-4 — Mural sem spoiler (próximos festivais CONHECIDOS) ──────────────────────────────

        [Test]
        public void CA4_NextKnownFestivals_ReturnsChronologicalPublicFestivals()
        {
            // A partir do dia 1, os 2 primeiros festivais conhecidos são Plantio (d7) e Caravana (d21).
            var next = WorldEventResolver.NextKnownFestivals(1, 2);
            Assert.AreEqual(2, next.Count);
            Assert.AreEqual("festival_plantio", next[0].FestivalId);
            Assert.AreEqual("festival_caravana", next[1].FestivalId);
        }

        [Test]
        public void CA4_NextKnownFestivals_FiltersUnknown_WhenKnownSetProvided()
        {
            // Só "festival_caravana" é conhecido: Plantio (d7) some, Caravana (d21) aparece primeiro.
            var known = new HashSet<string> { "festival_caravana" };
            var next = WorldEventResolver.NextKnownFestivals(1, 1, known);
            Assert.AreEqual(1, next.Count);
            Assert.AreEqual("festival_caravana", next[0].FestivalId);
        }

        [Test]
        public void CA4_RandomEvents_AreNotListedInMural()
        {
            // O mural usa NextKnownFestivals — eventos aleatórios nunca entram (spoiler gate).
            var next = WorldEventResolver.NextKnownFestivals(1, 8);
            foreach (var festival in next)
            {
                Assert.IsNotNull(WorldEventDefinitions.FindFestival(festival.FestivalId),
                    "O mural só pode conter festivais (nunca eventos aleatórios).");
            }
        }

        // ── Loja noturna (decisão v2 6.2-B): só em PICO DE NYX + quest ─────────────────────────────

        [Test]
        public void NightShop_Closed_WithoutQuest_EvenOnNyxPeak()
        {
            WorldEventHooks.Active = WorldEventResolver.ResolveDay("seed", AbsoluteDay(Season.Primavera, 7));
            WorldEventHooks.NightShopQuestUnlockedSource = () => false;
            Assert.IsTrue(WorldEventHooks.IsNyxPeakToday());
            Assert.IsFalse(WorldEventHooks.IsNightShopOpen(), "Sem a quest, a loja noturna fica fechada.");
        }

        [Test]
        public void NightShop_Closed_WithQuest_ButNotNyxPeak()
        {
            WorldEventHooks.Active = WorldEventResolver.ResolveDay("seed", AbsoluteDay(Season.Primavera, 14)); // pico verde, não Nyx
            WorldEventHooks.NightShopQuestUnlockedSource = () => true;
            Assert.IsFalse(WorldEventHooks.IsNyxPeakToday());
            Assert.IsFalse(WorldEventHooks.IsNightShopOpen(), "Fora do pico de Nyx, a loja noturna fica fechada.");
        }

        [Test]
        public void NightShop_Open_OnNyxPeak_WithQuest()
        {
            WorldEventHooks.Active = WorldEventResolver.ResolveDay("seed", AbsoluteDay(Season.Primavera, 7));
            WorldEventHooks.NightShopQuestUnlockedSource = () => true;
            Assert.IsTrue(WorldEventHooks.IsNightShopOpen());
        }

        // ── Regressão: dia comum sem efeito ativo é NEUTRO ────────────────────────────────────────

        [Test]
        public void Regression_NoActiveResolution_AllHooksNeutral()
        {
            WorldEventHooks.Active = null;
            Assert.AreEqual(1f, WorldEventHooks.GetCaveSpawnMultiplier());
            Assert.AreEqual(1f, WorldEventHooks.GetSellGoldMultiplier());
            Assert.AreEqual(100, WorldEventHooks.ApplySellGold(100));
            Assert.AreEqual(0, WorldEventHooks.GetExtraAguaViva());
            Assert.AreEqual(0, WorldEventHooks.GetGreenMoonExtraStages());
            Assert.AreEqual(0f, WorldEventHooks.GetExtraRareLootChance());
            Assert.AreEqual(1, WorldEventHooks.GetShopStockFactor());
            Assert.IsFalse(WorldEventHooks.IsDroughtToday());
            Assert.IsFalse(WorldEventHooks.IsNightShopOpen());
        }

        [Test]
        public void Regression_NormalDay_NoFestivalNoPeak_SellGoldUnchanged()
        {
            // Primavera d3: sem festival, sem pico. Venda inalterada.
            WorldEventHooks.Active = WorldEventResolver.ResolveDay("seed", AbsoluteDay(Season.Primavera, 3));
            Assert.IsFalse(WorldEventHooks.Active.HasFestival);
            Assert.IsFalse(WorldEventHooks.Active.HasLunarPeak);
            Assert.AreEqual(100, WorldEventHooks.ApplySellGold(100));
            Assert.AreEqual(1f, WorldEventHooks.GetCaveSpawnMultiplier());
        }

        [Test]
        public void StarShower_GivesExtraLootChance()
        {
            int starDay = -1;
            for (var day = 1; day <= 400; day++)
            {
                var e = WorldEventResolver.ResolveRandomEvent("seedLoot", day);
                if (e != null && e.Effect == WorldEventDefinitions.WorldEventEffect.StarShowerLuck)
                {
                    starDay = day;
                    break;
                }
            }

            Assert.Greater(starDay, 0, "A chuva de estrelas deve ser alcançável no pool.");
            WorldEventHooks.Active = WorldEventResolver.ResolveDay("seedLoot", starDay);
            Assert.AreEqual(0.05f, WorldEventHooks.GetExtraRareLootChance());
        }
    }
}
