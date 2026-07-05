using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.City;
using CindarsHope.NPC;
using CindarsHope.NPC.Friendship;
using CindarsHope.NPC.Social;
using CindarsHope.UI.Calendar;
using CindarsHope.World.Calendar;

namespace CindarsHope.Tests.EditMode.NPC
{
    /// <summary>
    /// fable_57 — testes determinísticos de aniversários de NPC, multiplicador de presente ×2, cama
    /// paga da estalagem e ausência de tracker social paralelo (a amizade F26 absorve "reputação").
    /// Cobre CA-1 (tabela/calendário), CA-2 (×2 + cap), CA-3 (cobrança/recusa) e a anti-regressão.
    /// Lógica pura — sem cena/Unity lifecycle.
    /// </summary>
    [TestFixture]
    public class BirthdayInnTests
    {
        private static GameDate DateFor(Season season, int dayOfSeason)
        {
            int absolute = (int)season * GameDate.DaysPerSeason + dayOfSeason; // dia 1..28 → absoluto
            return GameDate.FromAbsoluteDay(absolute);
        }

        // ── CA-1 — tabela de aniversários ─────────────────────────────────────────────────────────

        [Test]
        public void BirthdayTable_CoversAllRosterNpcs_23of23()
        {
            Assert.AreEqual(NpcTownRosterRegistry.CanonicalCount, NpcBirthdayTable.Count,
                "A tabela de aniversários deve cobrir os 23 NPCs canônicos do roster.");

            foreach (var rosterEntry in NpcTownRosterRegistry.AllEntries)
            {
                Assert.IsTrue(NpcBirthdayTable.TryGet(rosterEntry.NpcId, out _),
                    $"NPC do roster sem aniversário: {rosterEntry.NpcId}");
            }
        }

        [Test]
        public void BirthdayTable_AllDatesValidInGameCalendar()
        {
            foreach (var b in NpcBirthdayTable.All)
            {
                Assert.GreaterOrEqual((int)b.Season, 0);
                Assert.LessOrEqual((int)b.Season, GameDate.SeasonsPerYear - 1);
                Assert.GreaterOrEqual(b.DayOfSeason, 1, $"{b.NpcId} dia < 1");
                Assert.LessOrEqual(b.DayOfSeason, GameDate.DaysPerSeason, $"{b.NpcId} dia > {GameDate.DaysPerSeason}");
            }
        }

        [Test]
        public void BirthdayTable_AtMostOneBirthdayPerDayOfYear()
        {
            var seen = new HashSet<int>();
            foreach (var b in NpcBirthdayTable.All)
            {
                Assert.IsTrue(seen.Add(b.DayInYear),
                    $"Dois NPCs no mesmo dia do ano ({b.DayInYear}) — máx. 1/dia exigido pela CA-1.");
            }
        }

        [Test]
        public void BirthdayTable_NoBirthdayCollidesWithPublicFestival()
        {
            // Festivais públicos canônicos (FestivalCalendar): dias-no-ano 14, 56, 98.
            var festivalDays = new HashSet<int>
            {
                FestivalCalendar.PlantingFestivalDayInYear,
                FestivalCalendar.MarketFestivalDayInYear,
                FestivalCalendar.HarvestFestivalDayInYear
            };

            foreach (var b in NpcBirthdayTable.All)
            {
                Assert.IsFalse(festivalDays.Contains(b.DayInYear),
                    $"Aniversário de {b.NpcId} colide com festival no dia {b.DayInYear} do ano.");
            }
        }

        [Test]
        public void BirthdayService_IsBirthdayToday_OnlyOnExactDate()
        {
            Assert.IsTrue(NpcBirthdayTable.TryGet("npc_pip", out var pip));
            var onDay = DateFor(pip.Season, pip.DayOfSeason);
            var offDay = DateFor(pip.Season, pip.DayOfSeason == 28 ? 1 : pip.DayOfSeason + 1);

            Assert.IsTrue(NpcBirthdayService.IsBirthdayToday("npc_pip", onDay));
            Assert.IsFalse(NpcBirthdayService.IsBirthdayToday("npc_pip", offDay));
            Assert.IsFalse(NpcBirthdayService.IsBirthdayToday("npc_unknown", onDay));
        }

        [Test]
        public void CalendarProjection_ExposesBirthdayOfTheDay()
        {
            Assert.IsTrue(NpcBirthdayTable.TryGet("npc_thalindra", out var t));
            var date = DateFor(t.Season, t.DayOfSeason);

            var list = CalendarBirthdayProjectionBuilder.BuildBirthdaysForDay(date);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("npc_thalindra", list[0].NpcId);
            Assert.IsFalse(string.IsNullOrEmpty(list[0].DisplayName));

            // Dia sem aniversário (mesmo dia que um festival público) ⇒ lista vazia.
            var festivalDate = GameDate.FromAbsoluteDay(FestivalCalendar.PlantingFestivalDayInYear);
            Assert.AreEqual(0, CalendarBirthdayProjectionBuilder.BuildBirthdaysForDay(festivalDate).Count);
        }

        // ── CA-2 — multiplicador de presente ×2 + cap preservado ────────────────────────────────────

        [Test]
        public void GiftMultiplier_DoublesPointsOnlyOnBirthday()
        {
            Assert.IsTrue(NpcBirthdayTable.TryGet("npc_renko", out var r));
            int birthdayAbs = (int)r.Season * GameDate.DaysPerSeason + r.DayOfSeason;
            int normalAbs = birthdayAbs + 1;

            Assert.AreEqual(NpcBirthdayService.BirthdayGiftMultiplier,
                NpcBirthdayService.GiftMultiplierToday("npc_renko", birthdayAbs));
            Assert.AreEqual(1, NpcBirthdayService.GiftMultiplierToday("npc_renko", normalAbs));
        }

        [Test]
        public void RegisterGift_BirthdayMultiplier_DoublesDelta()
        {
            const string npc = "npc_renko";
            const int day = 5;

            // Presente "Liked" = +5 (GiftTasteClassifier.DeltaFor). ×2 = +10.
            var normal = new FriendshipState();
            var normalResult = normal.RegisterGift(npc, GiftTaste.Liked, day, dailyGiftLimit: 1, pointsMultiplier: 1);
            Assert.IsTrue(normalResult.Accepted);
            int normalDelta = normalResult.Apply.PointsDelta;

            var birthday = new FriendshipState();
            var birthdayResult = birthday.RegisterGift(npc, GiftTaste.Liked, day, dailyGiftLimit: 1, pointsMultiplier: 2);
            Assert.IsTrue(birthdayResult.Accepted);
            Assert.AreEqual(normalDelta * 2, birthdayResult.Apply.PointsDelta,
                "O presente de aniversário deve valer o dobro dos pontos.");
        }

        [Test]
        public void RegisterGift_BirthdayMultiplier_DoesNotBreakDailyCap()
        {
            const string npc = "npc_renko";
            const int day = 7;
            var state = new FriendshipState();

            // 1º presente do dia (aniversário, ×2) — aceito.
            var first = state.RegisterGift(npc, GiftTaste.Liked, day, dailyGiftLimit: 1, pointsMultiplier: 2);
            Assert.IsTrue(first.Accepted);
            int pointsAfterFirst = state.GetPoints(npc);

            // 2º presente no MESMO dia — recusado pelo cap, mesmo com multiplicador (não fura o cap).
            var second = state.RegisterGift(npc, GiftTaste.Liked, day, dailyGiftLimit: 1, pointsMultiplier: 2);
            Assert.IsFalse(second.Accepted, "O cap diário deve recusar o 2º presente mesmo no aniversário.");
            Assert.AreEqual(pointsAfterFirst, state.GetPoints(npc), "Recusa não pode somar pontos.");
        }

        [Test]
        public void RegisterGift_NoMultiplier_KeepsBaselineF26Behavior()
        {
            // Anti-regressão F26: presente comum fora de aniversário continua +5 (Liked) com cap 1×/dia.
            const string npc = "npc_renko";
            var state = new FriendshipState();
            var result = state.RegisterGift(npc, GiftTaste.Liked, currentDay: 3, dailyGiftLimit: 1);
            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(GiftTasteClassifier.DeltaFor(GiftTaste.Liked), result.Apply.PointsDelta);
        }

        // ── CA-3 — cobrança da diária da estalagem ──────────────────────────────────────────────────

        [Test]
        public void InnPayment_WithEnoughGold_DebitsOnce()
        {
            int gold = 120;
            int debits = 0;
            var result = InnLodgingPaymentResolver.TryPayNightlyRate(
                InnLodgingPaymentResolver.DefaultNightlyRate,
                () => gold,
                cost => { gold -= cost; debits++; return true; });

            Assert.AreEqual(InnLodgingPaymentResolver.Outcome.Paid, result.Outcome);
            Assert.AreEqual(1, debits, "Deve debitar exatamente uma vez.");
            Assert.AreEqual(120 - InnLodgingPaymentResolver.DefaultNightlyRate, gold);
        }

        [Test]
        public void InnPayment_WithoutGold_RefusesAndDoesNotDebit()
        {
            int gold = 10; // < 50
            int debits = 0;
            var result = InnLodgingPaymentResolver.TryPayNightlyRate(
                InnLodgingPaymentResolver.DefaultNightlyRate,
                () => gold,
                cost => { gold -= cost; debits++; return true; });

            Assert.AreEqual(InnLodgingPaymentResolver.Outcome.NotEnoughGold, result.Outcome);
            Assert.AreEqual(0, debits, "Sem ouro suficiente: nenhum débito (risco 'cobrança sem dormir').");
            Assert.AreEqual(10, gold);
        }

        [Test]
        public void InnPayment_InvalidConfig_DoesNotDebit()
        {
            var result = InnLodgingPaymentResolver.TryPayNightlyRate(50, null, cost => true);
            Assert.AreEqual(InnLodgingPaymentResolver.Outcome.Invalid, result.Outcome);
            Assert.IsFalse(result.Paid);
        }

        [Test]
        public void InnPayment_SpendFailureTreatedAsRefusal_NoSideEffect()
        {
            // Saldo diz que dá, mas o débito falha (corrida/erro): trate como recusa, sem efeito.
            var result = InnLodgingPaymentResolver.TryPayNightlyRate(50, () => 200, cost => false);
            Assert.AreEqual(InnLodgingPaymentResolver.Outcome.NotEnoughGold, result.Outcome);
            Assert.IsFalse(result.Paid);
        }

        // ── Anti-regressão — reputação absorvida pela amizade (sem tracker paralelo) ─────────────────

        [Test]
        public void NoParallelReputationTracker_FriendshipIsTheOnlySocialState()
        {
            // ADR-0017: "reputação" foi absorvida pela amizade F26. Não existe ReputationService/State.
            // Guard: nenhum tipo cujo nome simples seja "ReputationService"/"ReputationState" está
            // carregado nos assemblies do jogo (o ADR proíbe um tracker social paralelo).
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                System.Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (System.Reflection.ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }

                foreach (var type in types)
                {
                    if (type?.Namespace == null || !type.Namespace.StartsWith("CindarsHope")) continue;
                    Assert.AreNotEqual("ReputationService", type.Name,
                        "ADR-0017 proíbe um ReputationService paralelo — a amizade F26 é o único tracker social.");
                    Assert.AreNotEqual("ReputationState", type.Name,
                        "ADR-0017 proíbe um ReputationState paralelo — a amizade F26 é o único tracker social.");
                }
            }
        }
    }
}
