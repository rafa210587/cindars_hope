using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Items;
using CindarsHope.NPC;
using CindarsHope.NPC.Friendship;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// fable_26 — testes determinísticos do contrato de amizade (núcleo puro FriendshipState +
    /// GiftTasteClassifier). Cobre CA-1..CA-6:
    ///   CA-1 cap diário de conversa; CA-2 thresholds + transição de nível; CA-3 persistência +
    ///   load legado; CA-4 API de gate; CA-5 demais fontes com caps; CA-6 gosto por NPC (5 níveis,
    ///   precedência, Giftable gate, clamp em 0, descida de nível).
    /// Pure logic — sem cena/Unity lifecycle.
    /// </summary>
    [TestFixture]
    public class FriendshipTests
    {
        private const string Npc = "npc_renko";

        private static ItemDefinition ItemWith(string itemId, ItemTag tags, params string[] giftTags)
        {
            return new ItemDefinition
            {
                ItemId = itemId,
                Tags = tags,
                LoreTags = new List<string>(giftTags ?? new string[0])
            };
        }

        private static NpcGiftPreferences Prefs()
        {
            return new NpcGiftPreferences
            {
                LovedItemIds = new List<string> { "item_loved_personal" },
                LikedItemTags = new List<string> { "gift_food_fine" },
                NeutralItemTags = new List<string>(),
                DislikedItemTags = new List<string> { "gift_junk_lowvalue" },
                HatedItemTags = new List<string> { "gift_undocumented_blackmarket" },
                DailyGiftLimit = 1
            };
        }

        // ── CA-2 / thresholds ────────────────────────────────────────────────────────────────────

        [Test]
        public void LevelForPoints_MatchesCanonicalThresholds()
        {
            Assert.AreEqual(0, FriendshipState.LevelForPoints(0));
            Assert.AreEqual(0, FriendshipState.LevelForPoints(9));
            Assert.AreEqual(1, FriendshipState.LevelForPoints(10));
            Assert.AreEqual(1, FriendshipState.LevelForPoints(29));
            Assert.AreEqual(2, FriendshipState.LevelForPoints(30));
            Assert.AreEqual(3, FriendshipState.LevelForPoints(60));
            Assert.AreEqual(4, FriendshipState.LevelForPoints(100));
            Assert.AreEqual(5, FriendshipState.LevelForPoints(150));
            Assert.AreEqual(5, FriendshipState.LevelForPoints(9999));
        }

        [Test]
        public void AddPoints_ReportsLevelTransition_OncePerCrossing()
        {
            var s = new FriendshipState();
            // 9 -> nível 0 ainda
            var r1 = s.AddPoints(Npc, 9);
            Assert.IsFalse(r1.LevelChanged);
            Assert.AreEqual(0, r1.NewLevel);

            // +1 => 10 => nível 1 (transição)
            var r2 = s.AddPoints(Npc, 1);
            Assert.IsTrue(r2.LevelChanged);
            Assert.AreEqual(0, r2.PreviousLevel);
            Assert.AreEqual(1, r2.NewLevel);

            // +1 => 11 => continua nível 1 (sem transição)
            var r3 = s.AddPoints(Npc, 1);
            Assert.IsFalse(r3.LevelChanged);
            Assert.AreEqual(1, r3.NewLevel);
        }

        // ── CA-1 conversa: 1×/dia/NPC ──────────────────────────────────────────────────────────────

        [Test]
        public void DailyConversation_FirstOfDayGains_SecondSameDayCapped_NextDayGainsAgain()
        {
            var s = new FriendshipState();

            var d1a = s.RegisterDailyConversation(Npc, 1);
            Assert.IsTrue(d1a.Applied);
            Assert.AreEqual(1, s.GetPoints(Npc));

            var d1b = s.RegisterDailyConversation(Npc, 1); // mesmo dia => cap
            Assert.IsFalse(d1b.Applied);
            Assert.AreEqual(1, s.GetPoints(Npc));

            var d2 = s.RegisterDailyConversation(Npc, 2); // dia seguinte
            Assert.IsTrue(d2.Applied);
            Assert.AreEqual(2, s.GetPoints(Npc));
        }

        // ── CA-5 quest: +8 idempotente por questId ─────────────────────────────────────────────────

        [Test]
        public void QuestCompleted_Grants8_IdempotentPerQuestId()
        {
            var s = new FriendshipState();

            var q1 = s.RegisterQuestCompleted(Npc, "quest_help_renko");
            Assert.IsTrue(q1.Applied);
            Assert.AreEqual(8, s.GetPoints(Npc));

            // mesma quest de novo => idempotente, sem ganho
            var q1again = s.RegisterQuestCompleted(Npc, "quest_help_renko");
            Assert.IsFalse(q1again.Applied);
            Assert.AreEqual(8, s.GetPoints(Npc));

            // quest diferente => soma
            var q2 = s.RegisterQuestCompleted(Npc, "quest_renko_followup");
            Assert.IsTrue(q2.Applied);
            Assert.AreEqual(16, s.GetPoints(Npc));
        }

        // ── CA-5 compra: 1×/dia/NPC ────────────────────────────────────────────────────────────────

        [Test]
        public void ShopPurchase_FirstOfDayGains_SecondSameDayCapped()
        {
            var s = new FriendshipState();

            var p1 = s.RegisterShopPurchase(Npc, 5);
            Assert.IsTrue(p1.Applied);
            Assert.AreEqual(1, s.GetPoints(Npc));

            var p2 = s.RegisterShopPurchase(Npc, 5); // mesmo dia => cap
            Assert.IsFalse(p2.Applied);
            Assert.AreEqual(1, s.GetPoints(Npc));

            var p3 = s.RegisterShopPurchase(Npc, 6); // dia seguinte
            Assert.IsTrue(p3.Applied);
            Assert.AreEqual(2, s.GetPoints(Npc));
        }

        // ── CA-6 gosto por NPC: 5 níveis e deltas ───────────────────────────────────────────────────

        [Test]
        public void Classify_FiveLevels_MapToExpectedDeltas()
        {
            var prefs = Prefs();

            // loved (por id)
            var loved = ItemWith("item_loved_personal", ItemTag.Giftable);
            Assert.AreEqual(GiftTaste.Loved, GiftTasteClassifier.Classify(prefs, loved));
            Assert.AreEqual(12, GiftTasteClassifier.DeltaForGift(prefs, loved));

            // liked (por tag)
            var liked = ItemWith("item_food", ItemTag.Giftable, "gift_food_fine");
            Assert.AreEqual(GiftTaste.Liked, GiftTasteClassifier.Classify(prefs, liked));
            Assert.AreEqual(6, GiftTasteClassifier.DeltaForGift(prefs, liked));

            // neutral (default: Giftable sem classificação)
            var neutral = ItemWith("item_random", ItemTag.Giftable, "gift_unmapped");
            Assert.AreEqual(GiftTaste.Neutral, GiftTasteClassifier.Classify(prefs, neutral));
            Assert.AreEqual(2, GiftTasteClassifier.DeltaForGift(prefs, neutral));

            // disliked (por tag)
            var disliked = ItemWith("item_junk", ItemTag.Giftable, "gift_junk_lowvalue");
            Assert.AreEqual(GiftTaste.Disliked, GiftTasteClassifier.Classify(prefs, disliked));
            Assert.AreEqual(-2, GiftTasteClassifier.DeltaForGift(prefs, disliked));

            // hated (por tag)
            var hated = ItemWith("item_contraband", ItemTag.Giftable, "gift_undocumented_blackmarket");
            Assert.AreEqual(GiftTaste.Hated, GiftTasteClassifier.Classify(prefs, hated));
            Assert.AreEqual(-6, GiftTasteClassifier.DeltaForGift(prefs, hated));
        }

        // ── CA-6 precedência: hated > disliked > loved > liked > neutral ─────────────────────────────

        [Test]
        public void Classify_Precedence_HatedWinsOverLovedAndLiked()
        {
            var prefs = Prefs();
            // item que é simultaneamente loved (id) E hated (tag) => hated vence (rejeição pessoal).
            var conflicted = ItemWith("item_loved_personal", ItemTag.Giftable, "gift_undocumented_blackmarket");
            Assert.AreEqual(GiftTaste.Hated, GiftTasteClassifier.Classify(prefs, conflicted));
        }

        [Test]
        public void Classify_Precedence_DislikedWinsOverLoved()
        {
            var prefs = Prefs();
            var conflicted = ItemWith("item_loved_personal", ItemTag.Giftable, "gift_junk_lowvalue");
            Assert.AreEqual(GiftTaste.Disliked, GiftTasteClassifier.Classify(prefs, conflicted));
        }

        [Test]
        public void Classify_Precedence_LovedWinsOverLiked()
        {
            var prefs = Prefs();
            // loved por id E liked por tag, sem negativos => loved vence.
            var conflicted = ItemWith("item_loved_personal", ItemTag.Giftable, "gift_food_fine");
            Assert.AreEqual(GiftTaste.Loved, GiftTasteClassifier.Classify(prefs, conflicted));
        }

        // ── CA-6 Giftable gate ──────────────────────────────────────────────────────────────────────

        [Test]
        public void Giftable_Gate_ItemWithoutTagIsNotGiftable()
        {
            var noTag = ItemWith("item_loved_personal", ItemTag.None);
            Assert.IsFalse(GiftTasteClassifier.IsGiftable(noTag));

            var withTag = ItemWith("item_loved_personal", ItemTag.Giftable);
            Assert.IsTrue(GiftTasteClassifier.IsGiftable(withTag));
        }

        // ── CA-6 clamp no piso 0 + descida de nível por presente hated ──────────────────────────────

        [Test]
        public void Gift_HatedReducesPoints_ClampsAtZero()
        {
            var s = new FriendshipState();
            // começa com 4 pontos
            s.AddPoints(Npc, 4);
            // hated (-6) com clamp => 0, não -2
            var r = s.RegisterGift(Npc, GiftTaste.Hated, 1, 1);
            Assert.IsTrue(r.Accepted);
            Assert.AreEqual(0, s.GetPoints(Npc));
        }

        [Test]
        public void Gift_HatedCanDropLevel_AndReportsTransition()
        {
            var s = new FriendshipState();
            // 12 pontos => nível 1
            s.AddPoints(Npc, 12);
            Assert.AreEqual(1, s.GetLevel(Npc));

            // hated (-6) => 6 pontos => nível 0 (descida) — em dia diferente do AddPoints (sem cap aqui)
            var r = s.RegisterGift(Npc, GiftTaste.Hated, 1, 1);
            Assert.IsTrue(r.Accepted);
            Assert.AreEqual(6, s.GetPoints(Npc));
            Assert.AreEqual(0, s.GetLevel(Npc));
            Assert.IsTrue(r.Apply.LevelChanged);
            Assert.AreEqual(1, r.Apply.PreviousLevel);
            Assert.AreEqual(0, r.Apply.NewLevel);
        }

        // ── CA-5/CA-6 cap diário de presente (1×/dia/NPC) ───────────────────────────────────────────

        [Test]
        public void Gift_DailyCap_SecondGiftSameDayRefused_NotConsumed()
        {
            var s = new FriendshipState();

            var g1 = s.RegisterGift(Npc, GiftTaste.Liked, 1, 1);
            Assert.IsTrue(g1.Accepted);
            Assert.AreEqual(6, s.GetPoints(Npc));

            var g2 = s.RegisterGift(Npc, GiftTaste.Liked, 1, 1); // mesmo dia => recusa
            Assert.IsFalse(g2.Accepted);
            Assert.AreEqual(6, s.GetPoints(Npc)); // sem ganho/perda

            var g3 = s.RegisterGift(Npc, GiftTaste.Liked, 2, 1); // dia seguinte => aceita
            Assert.IsTrue(g3.Accepted);
            Assert.AreEqual(12, s.GetPoints(Npc));
        }

        // ── CA-4 API de gate ────────────────────────────────────────────────────────────────────────

        [Test]
        public void IsAtLeast_UnknownNpc_ReturnsFalse_Level0()
        {
            var s = new FriendshipState();
            Assert.AreEqual(0, s.GetLevel("npc_never_seen"));
            Assert.IsFalse(s.IsAtLeast("npc_never_seen", 1));
            Assert.IsTrue(s.IsAtLeast("npc_never_seen", 0)); // nível 0 sempre satisfeito
        }

        [Test]
        public void IsAtLeast_RespondsCorrectlyAcrossLevels()
        {
            var s = new FriendshipState();
            s.AddPoints(Npc, 60); // nível 3
            Assert.IsTrue(s.IsAtLeast(Npc, 0));
            Assert.IsTrue(s.IsAtLeast(Npc, 3));
            Assert.IsTrue(s.IsAtLeast(Npc, FriendshipLevel.Friend));
            Assert.IsFalse(s.IsAtLeast(Npc, 4));
            Assert.IsFalse(s.IsAtLeast(Npc, FriendshipLevel.Close));
        }

        [Test]
        public void IsAtLeast_NullOrEmptyNpc_IsSafe()
        {
            var s = new FriendshipState();
            Assert.IsFalse(s.IsAtLeast(null, 1));
            Assert.IsFalse(s.IsAtLeast("", 1));
            Assert.AreEqual(0, s.GetPoints(null));
        }

        // ── CA-3 persistência: round-trip + load legado ─────────────────────────────────────────────

        [Test]
        public void SaveRoundTrip_PreservesPointsAndDayMarkers()
        {
            var s = new FriendshipState();
            s.RegisterDailyConversation(Npc, 3);          // +1, lastTalkDay=3
            s.RegisterShopPurchase(Npc, 3);               // +1, lastPurchaseDay=3
            s.RegisterGift(Npc, GiftTaste.Liked, 3, 1);   // +6, lastGiftDay=3
            s.RegisterDailyConversation("npc_pip", 4);    // outro NPC

            var captured = s.CaptureSaveData();
            Assert.AreEqual(2, captured.Entries.Count);

            var restored = new FriendshipState();
            restored.RestoreFromSaveData(captured);

            Assert.AreEqual(8, restored.GetPoints(Npc));     // 1+1+6
            Assert.AreEqual(1, restored.GetPoints("npc_pip"));

            // marcadores de dia preservados: re-conversar no MESMO dia 3 ainda está capado
            var sameDay = restored.RegisterDailyConversation(Npc, 3);
            Assert.IsFalse(sameDay.Applied);
            Assert.AreEqual(8, restored.GetPoints(Npc));

            // compra no mesmo dia 3 também capada
            var samePurchase = restored.RegisterShopPurchase(Npc, 3);
            Assert.IsFalse(samePurchase.Applied);

            // presente no mesmo dia 3 também recusado
            var sameGift = restored.RegisterGift(Npc, GiftTaste.Liked, 3, 1);
            Assert.IsFalse(sameGift.Accepted);
        }

        [Test]
        public void LegacyLoad_NullOrEmpty_AllNpcsLevelZero_NoError()
        {
            var s = new FriendshipState();
            s.AddPoints(Npc, 100); // sujeira pré-load

            s.RestoreFromSaveData(null); // legado: sem seção
            Assert.AreEqual(0, s.GetPoints(Npc));
            Assert.AreEqual(0, s.GetLevel(Npc));

            s.AddPoints(Npc, 100);
            s.RestoreFromSaveData(new FriendshipSaveData()); // seção vazia
            Assert.AreEqual(0, s.GetPoints(Npc));
        }

        [Test]
        public void Restore_InvalidNpcId_Ignored_NegativePointsClamped()
        {
            var data = new FriendshipSaveData();
            data.Entries.Add(new FriendshipEntrySaveData { NpcId = "", Points = 50 });     // id inválido
            data.Entries.Add(new FriendshipEntrySaveData { NpcId = null, Points = 50 });   // id nulo
            data.Entries.Add(new FriendshipEntrySaveData { NpcId = Npc, Points = -10 });   // negativo

            var s = new FriendshipState();
            s.RestoreFromSaveData(data);

            Assert.AreEqual(0, s.GetPoints(Npc));      // clamp em 0
            Assert.AreEqual(0, s.GetPoints(""));       // ignorado (sem entrada)
        }

        // ── Gosto: fallback neutral quando preferências ausentes (emenda V3 §6) ─────────────────────

        [Test]
        public void Classify_NullPreferences_FallsBackNeutral()
        {
            var item = ItemWith("item_anything", ItemTag.Giftable, "gift_food_fine");
            Assert.AreEqual(GiftTaste.Neutral, GiftTasteClassifier.Classify(null, item));
            Assert.AreEqual(2, GiftTasteClassifier.DeltaForGift(null, item));
        }
    }
}
