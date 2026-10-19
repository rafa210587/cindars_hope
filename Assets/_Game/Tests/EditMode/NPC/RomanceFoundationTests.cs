using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.NPC;
using CindarsHope.NPC.Friendship;

namespace CindarsHope.Tests.EditMode.NPC
{
    /// <summary>
    /// fable_46 — testes determinísticos da fundação de romance (núcleo puro RomanceState +
    /// RomanceEligibility + condição IsPartner de diálogo + extensão de presente do FriendshipState).
    /// Cobre CA-1 (confissão gated por condição isolada), CA-2 (limite de 2 + recusa), CA-3 (late
    /// gates Maelor/Nymiriano), CA-4 (IsPartner muda falas + presente +50% com cap), CA-5 (save).
    /// Pure logic — sem cena/Unity lifecycle.
    /// </summary>
    [TestFixture]
    public class RomanceFoundationTests
    {
        private const string Eligible = "npc_sylveth";
        private const string Maelor = "npc_maelor";
        private const string Nymiriano = RomanceEligibilityTable.NymirianoNpcId;
        private const string Unavailable = "npc_mara";

        // Probe builder: por padrão todos os gates PASSAM (amizade 5, cadeia completa, flags setadas).
        private static RomanceState.GateProbe Probe(
            bool friendshipOk = true, bool chainOk = true, bool flagOk = true)
        {
            return new RomanceState.GateProbe(
                friendshipAtLeast: (npc, lvl) => friendshipOk,
                chainComplete: npc => chainOk,
                flagSet: flag => flagOk);
        }

        // ── CA-1: confissão gated, cada condição isolada ────────────────────────────────────────────

        [Test]
        public void Confess_AllGatesPass_GoesToInteresse()
        {
            var s = new RomanceState();
            var r = s.Confess(Eligible, Probe(), currentDay: 3);
            Assert.IsTrue(r.Accepted);
            Assert.AreEqual(RomanceConfessionRejection.None, r.Rejection);
            Assert.AreEqual(RomanceStage.Interesse, s.GetStage(Eligible));
        }

        [Test]
        public void Confess_FriendshipTooLow_Rejected_StateUnchanged()
        {
            var s = new RomanceState();
            var r = s.Confess(Eligible, Probe(friendshipOk: false), currentDay: 1);
            Assert.IsFalse(r.Accepted);
            Assert.AreEqual(RomanceConfessionRejection.FriendshipTooLow, r.Rejection);
            Assert.AreEqual(RomanceStage.None, s.GetStage(Eligible));
        }

        [Test]
        public void Confess_ChainIncomplete_Rejected()
        {
            var s = new RomanceState();
            var r = s.Confess(Eligible, Probe(chainOk: false), currentDay: 1);
            Assert.AreEqual(RomanceConfessionRejection.ChainIncomplete, r.Rejection);
            Assert.AreEqual(RomanceStage.None, s.GetStage(Eligible));
        }

        [Test]
        public void Confess_UnavailableNpc_NeverEligible()
        {
            var s = new RomanceState();
            var r = s.Confess(Unavailable, Probe(), currentDay: 1);
            Assert.AreEqual(RomanceConfessionRejection.NotEligible, r.Rejection);
            Assert.AreEqual(RomanceStage.None, s.GetStage(Unavailable));
        }

        [Test]
        public void Confess_Twice_SecondIsAlreadyRomanced()
        {
            var s = new RomanceState();
            Assert.IsTrue(s.Confess(Eligible, Probe(), 1).Accepted);
            var r2 = s.Confess(Eligible, Probe(), 2);
            Assert.IsFalse(r2.Accepted);
            Assert.AreEqual(RomanceConfessionRejection.AlreadyRomanced, r2.Rejection);
        }

        // ── CA-2: limite de 2 parceiros simultâneos + recusa do 3º ──────────────────────────────────

        [Test]
        public void PartnerLimit_ThirdConfession_RejectedWithDignity()
        {
            var s = new RomanceState();
            // Promove 2 NPCs a parceiro (Namoro+).
            PromoteToNamoro(s, "npc_zrix");
            PromoteToNamoro(s, "npc_yael");
            Assert.AreEqual(2, s.PartnerCount());

            var third = s.Confess("npc_dagna", Probe(), currentDay: 10);
            Assert.IsFalse(third.Accepted);
            Assert.AreEqual(RomanceConfessionRejection.PartnerLimitReached, third.Rejection);
            Assert.AreEqual(RomanceStage.None, s.GetStage("npc_dagna"));
        }

        [Test]
        public void PartnerLimit_RejectionTextPresentInPool()
        {
            string key = RomanceDialoguePool.RejectionKey(RomanceConfessionRejection.PartnerLimitReached);
            Assert.IsFalse(string.IsNullOrEmpty(key));
            Assert.AreEqual("dialogue.romance.reject.partner_limit", key);
        }

        [Test]
        public void MaxSimultaneousPartners_IsTwo_PerDecision13()
        {
            Assert.AreEqual(2, RomanceEligibilityTable.MaxSimultaneousPartners);
        }

        // ── CA-3: late gates ────────────────────────────────────────────────────────────────────────

        [Test]
        public void Maelor_RequiresChainBeyondFriendship()
        {
            var s = new RomanceState();
            // Amizade ok, mas cadeia própria incompleta ⇒ recusa por cadeia.
            var r = s.Confess(Maelor, Probe(chainOk: false), currentDay: 1);
            Assert.AreEqual(RomanceConfessionRejection.ChainIncomplete, r.Rejection);
            // Com a cadeia completa, passa.
            Assert.IsTrue(s.Confess(Maelor, Probe(), currentDay: 2).Accepted);
        }

        [Test]
        public void Nymiriano_NotConfessableWithoutActFlag()
        {
            var s = new RomanceState();
            var blocked = s.Confess(Nymiriano, Probe(flagOk: false), currentDay: 1);
            Assert.AreEqual(RomanceConfessionRejection.ActGateNotMet, blocked.Rejection);
            Assert.AreEqual(RomanceStage.None, s.GetStage(Nymiriano));

            var ok = s.Confess(Nymiriano, Probe(flagOk: true), currentDay: 2);
            Assert.IsTrue(ok.Accepted);
        }

        [Test]
        public void Eligibility_TableHasElevenCandidatesPlusTwoLate()
        {
            int eligible = 0, late = 0;
            foreach (var npc in RomanceEligibilityTable.AllRomanceableNpcIds())
            {
                var st = RomanceEligibilityTable.StatusOf(npc);
                if (st == RomanceEligibilityStatus.Eligible) eligible++;
                else if (st == RomanceEligibilityStatus.LateActGated) late++;
            }
            Assert.AreEqual(10, eligible, "10 candidatos Eligible (Maelor é LateActGated).");
            Assert.AreEqual(2, late, "Maelor + Nymiriano são LateActGated.");
        }

        // ── CA-1 progressão: marcos simples por estágio ─────────────────────────────────────────────

        [Test]
        public void Progression_RequiresInteractionsAndGift_Advances()
        {
            var s = new RomanceState();
            Assert.IsTrue(s.Confess(Eligible, Probe(), 1).Accepted); // Interesse

            // Só interações, sem presente ⇒ não avança.
            for (int i = 0; i < RomanceEligibilityTable.PartnerInteractionsPerStage; i++)
                s.RegisterPartnerInteraction(Eligible);
            Assert.AreEqual(RomanceStage.Interesse, s.GetStage(Eligible));

            // + 1 presente bate o marco ⇒ avança a Namoro.
            var adv = s.RegisterPartnerGift(Eligible);
            Assert.IsTrue(adv.Changed);
            Assert.AreEqual(RomanceStage.Namoro, s.GetStage(Eligible));
            Assert.IsTrue(s.IsPartner(Eligible));
        }

        [Test]
        public void Progression_StopsAtCompromisso()
        {
            var s = new RomanceState();
            s.Confess(Eligible, Probe(), 1);
            AdvanceFully(s, Eligible);           // Namoro
            AdvanceFully(s, Eligible);           // Compromisso
            Assert.AreEqual(RomanceStage.Compromisso, s.GetStage(Eligible));
            // Mais marcos não passam de Compromisso.
            AdvanceFully(s, Eligible);
            Assert.AreEqual(RomanceStage.Compromisso, s.GetStage(Eligible));
        }

        // ── CA-4: IsPartner muda falas + presente +50% (cap diário respeitado) ──────────────────────

        [Test]
        public void PartnerLines_OnlyEligibleWhenPartner()
        {
            var pool = RomanceDialoguePool.PartnerLines(Eligible);
            // Contexto de conhecido (None) — nenhuma fala de parceiro é elegível.
            var notPartner = Ctx(Eligible, romanceStage: (int)RomanceStage.None);
            foreach (var line in pool)
                Assert.IsFalse(line.Condition.IsMet(notPartner), "Conhecido não vê falas de parceiro.");

            // Contexto de Namoro — pelo menos as falas de Namoro ficam elegíveis.
            var partner = Ctx(Eligible, romanceStage: (int)RomanceStage.Namoro);
            int eligibleCount = 0;
            foreach (var line in pool)
                if (line.Condition.IsMet(partner)) eligibleCount++;
            Assert.GreaterOrEqual(eligibleCount, RomanceDialoguePool.MinLinesPerStage);
        }

        [Test]
        public void PartnerGift_AppliesFiftyPercentBonus_AfterDailyCap()
        {
            // Gift base "Loved" (delta alto). Presente normal vs. presente de parceiro (x1.5).
            var fs = new FriendshipState();
            const string npc = "npc_zrix";

            // Presente normal no dia 1.
            var normal = fs.RegisterGift(npc, GiftTaste.Loved, currentDay: 1, dailyGiftLimit: 1,
                pointsMultiplier: 1, bonusMultiplier: 1f);
            Assert.IsTrue(normal.Accepted);
            int normalDelta = normal.Apply.PointsDelta;

            var fs2 = new FriendshipState();
            var partner = fs2.RegisterGift(npc, GiftTaste.Loved, currentDay: 1, dailyGiftLimit: 1,
                pointsMultiplier: 1, bonusMultiplier: RomanceEligibilityTable.PartnerGiftPointMultiplier);
            Assert.IsTrue(partner.Accepted);
            int partnerDelta = partner.Apply.PointsDelta;

            Assert.Greater(partnerDelta, normalDelta, "Parceiro rende mais que conhecido.");
            int expected = (int)System.Math.Round(normalDelta * RomanceEligibilityTable.PartnerGiftPointMultiplier,
                System.MidpointRounding.AwayFromZero);
            Assert.AreEqual(expected, partnerDelta);

            // CAP diário: 2º presente de parceiro no MESMO dia é recusado (o bônus não fura o cap).
            var second = fs2.RegisterGift(npc, GiftTaste.Loved, currentDay: 1, dailyGiftLimit: 1,
                pointsMultiplier: 1, bonusMultiplier: RomanceEligibilityTable.PartnerGiftPointMultiplier);
            Assert.IsFalse(second.Accepted);
        }

        [Test]
        public void PartnerGiftMultiplier_IsFiftyPercent()
        {
            Assert.AreEqual(1.5f, RomanceEligibilityTable.PartnerGiftPointMultiplier, 0.0001f);
        }

        // ── CA-5: persistência (round-trip + legado) ────────────────────────────────────────────────

        [Test]
        public void Save_RoundTrip_PreservesStageAndProgress()
        {
            var s = new RomanceState();
            s.Confess(Eligible, Probe(), currentDay: 4);
            s.RegisterPartnerInteraction(Eligible); // progresso parcial no estágio

            // A seção de amizade já tem a entrada do NPC (romance exige amizade); simula isso.
            var data = new FriendshipSaveData();
            data.Entries.Add(new FriendshipEntrySaveData { NpcId = Eligible, Points = 150 });
            s.WriteInto(data);

            var restored = new RomanceState();
            restored.RestoreFrom(data);
            Assert.AreEqual(RomanceStage.Interesse, restored.GetStage(Eligible));

            // E o progresso parcial sobrevive: faltavam (N-1) interações + 1 presente para avançar.
            for (int i = 1; i < RomanceEligibilityTable.PartnerInteractionsPerStage; i++)
                restored.RegisterPartnerInteraction(Eligible);
            var adv = restored.RegisterPartnerGift(Eligible);
            Assert.IsTrue(adv.Changed);
            Assert.AreEqual(RomanceStage.Namoro, restored.GetStage(Eligible));
        }

        [Test]
        public void Save_LegacyData_LoadsAllNone()
        {
            // Save legado: entradas de amizade SEM campos de romance (defaults 0 / confessedDay -1).
            var data = new FriendshipSaveData();
            data.Entries.Add(new FriendshipEntrySaveData { NpcId = Eligible, Points = 150 });
            data.Entries.Add(new FriendshipEntrySaveData { NpcId = "npc_zrix", Points = 60 });

            var s = new RomanceState();
            s.RestoreFrom(data);
            Assert.AreEqual(RomanceStage.None, s.GetStage(Eligible));
            Assert.AreEqual(RomanceStage.None, s.GetStage("npc_zrix"));
            Assert.AreEqual(0, s.PartnerCount());
        }

        [Test]
        public void Save_NullData_NoThrow_AllNone()
        {
            var s = new RomanceState();
            Assert.DoesNotThrow(() => s.RestoreFrom(null));
            Assert.AreEqual(RomanceStage.None, s.GetStage(Eligible));
        }

        // ── helpers ──────────────────────────────────────────────────────────────────────────────

        private static DialogueConditionContext Ctx(string npcId, int romanceStage)
        {
            return new DialogueConditionContext(
                npcId, day: 1,
                season: CindarsHope.World.Calendar.Season.Primavera,
                weather: CindarsHope.World.Weather.WeatherType.Clear,
                timeBand: DialogueTimeBand.Manha,
                friendshipLevel: 5,
                isFestivalDay: false,
                activeFlags: null,
                inferredTitleId: null,
                romanceStage: romanceStage);
        }

        private static void PromoteToNamoro(RomanceState s, string npc)
        {
            Assert.IsTrue(s.Confess(npc, Probe(), 1).Accepted);
            AdvanceFully(s, npc);
            Assert.IsTrue(s.IsPartner(npc));
        }

        private static void AdvanceFully(RomanceState s, string npc)
        {
            for (int i = 0; i < RomanceEligibilityTable.PartnerInteractionsPerStage; i++)
                s.RegisterPartnerInteraction(npc);
            s.RegisterPartnerGift(npc);
        }
    }
}
