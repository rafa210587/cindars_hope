using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Items;
using CindarsHope.NPC;
using CindarsHope.NPC.Events;
using CindarsHope.NPC.Friendship;
using CindarsHope.NPC.Gifting;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// fable_72 — testes determinísticos do FLUXO de dar presente (camada desta spec), distintos dos
    /// testes da F26 (FriendshipTests cobre classificação/precedência/deltas/clamp do núcleo). Aqui:
    /// porteiro Giftable (no-op), cap diário (recusa sem consumir), aceite (consome 1 + delta correto +
    /// evento), delta negativo no hated, fallback neutral sem preferências, integridade da matriz §4,
    /// e round-trip dos campos estendidos da struct NpcGiftPreferences.
    /// Pure logic — sem cena/Unity lifecycle.
    /// </summary>
    [TestFixture]
    public class GiftGivingTasteTests
    {
        private const string Npc = "npc_test";

        // ── Fakes para isolar o fluxo (sem Unity) ──────────────────────────────────────────────────

        /// <summary>Adapta o núcleo puro FriendshipState ao contrato IGiftFriendship da spec.</summary>
        private sealed class StateFriendship : IGiftFriendship
        {
            public readonly FriendshipState State = new FriendshipState();
            public int GiveGiftCalls;

            public FriendshipState.GiftResult GiveGift(
                string npcId, NpcGiftPreferences preferences, ItemDefinition item)
            {
                GiveGiftCalls++;
                // Espelha FriendshipService.GiveGift: porteiro Giftable + classificação + cap + delta.
                if (!GiftTasteClassifier.IsGiftable(item))
                {
                    return new FriendshipState.GiftResult(false, GiftTaste.Neutral, default);
                }
                var taste = GiftTasteClassifier.Classify(preferences, item);
                int limit = preferences != null ? preferences.DailyGiftLimit : 1;
                return State.RegisterGift(npcId, taste, _day, limit);
            }

            private int _day = 1;
            public void SetDay(int day) => _day = day;
        }

        private sealed class FakeInventory : IGiftInventory
        {
            private readonly Dictionary<string, int> _counts = new Dictionary<string, int>();
            public int RemoveCalls;
            public int LastRemovedAmount;

            public void Give(string itemId, int amount) => _counts[itemId] = amount;

            public bool HasItem(string itemId, int amount)
                => _counts.TryGetValue(itemId, out var c) && c >= amount;

            public bool RemoveItem(string itemId, int amount)
            {
                RemoveCalls++;
                LastRemovedAmount = amount;
                if (!_counts.TryGetValue(itemId, out var c) || c < amount) return false;
                _counts[itemId] = c - amount;
                return true;
            }

            public int CountOf(string itemId) => _counts.TryGetValue(itemId, out var c) ? c : 0;
        }

        private static ItemDefinition Giftable(string itemId, params string[] giftTags)
        {
            return new ItemDefinition
            {
                ItemId = itemId,
                Tags = ItemTag.Giftable,
                LoreTags = new List<string>(giftTags ?? new string[0])
            };
        }

        private static NpcGiftPreferences Prefs()
        {
            return new NpcGiftPreferences
            {
                LovedItemIds = new List<string> { "item_loved" },
                LikedItemTags = new List<string> { "gift_food_fine" },
                DislikedItemTags = new List<string> { "gift_junk_lowvalue" },
                HatedItemTags = new List<string> { "gift_undocumented_blackmarket" },
                DailyGiftLimit = 1
            };
        }

        // ── CA-2: porteiro Giftable — item sem a tag é no-op (não consome, sem evento, sem AddPoints) ─

        [Test]
        public void Process_ItemNotGiftable_NoOp_NotConsumed_NoEvent()
        {
            var fr = new StateFriendship();
            var inv = new FakeInventory();
            inv.Give("item_plain", 3);

            var notGiftable = new ItemDefinition
            {
                ItemId = "item_plain",
                Tags = ItemTag.None,
                LoreTags = new List<string> { "gift_food_fine" }
            };

            var r = GiftGivingProcessor.Process(Npc, "item_plain", notGiftable, Prefs(), fr, inv);

            Assert.AreEqual(GiftGivingOutcome.RefusedNotGiftable, r.Outcome);
            Assert.IsFalse(r.ItemConsumed);
            Assert.IsFalse(r.ReactionPublished);
            Assert.AreEqual(0, inv.RemoveCalls, "item não-Giftable não deve ser consumido");
            Assert.AreEqual(3, inv.CountOf("item_plain"));
            Assert.AreEqual(0, fr.GiveGiftCalls, "não-Giftable nem chega à F26 (gate antes)");
            Assert.AreEqual(0, fr.State.GetPoints(Npc));
        }

        // ── CA-4: aceite — chama F26 1×, consome exatamente 1, evento publicado, delta correto ───────

        [Test]
        public void Process_LikedAccepted_ConsumesOne_AppliesDelta_PublishesEvent()
        {
            var fr = new StateFriendship();
            var inv = new FakeInventory();
            inv.Give("item_food", 2);

            var item = Giftable("item_food", "gift_food_fine"); // liked => +6
            var r = GiftGivingProcessor.Process(Npc, "item_food", item, Prefs(), fr, inv);

            Assert.AreEqual(GiftGivingOutcome.Accepted, r.Outcome);
            Assert.AreEqual(GiftTaste.Liked, r.Taste);
            Assert.AreEqual(6, r.AppliedDelta);
            Assert.IsTrue(r.ItemConsumed);
            Assert.IsTrue(r.ReactionPublished);
            Assert.AreEqual(1, fr.GiveGiftCalls, "F26.GiveGift chamado exatamente 1×");
            Assert.AreEqual(1, inv.RemoveCalls);
            Assert.AreEqual(1, inv.LastRemovedAmount, "consome exatamente 1 unidade");
            Assert.AreEqual(1, inv.CountOf("item_food"), "2 - 1 = 1 restante");
            Assert.AreEqual(6, fr.State.GetPoints(Npc));
        }

        [Test]
        public void Process_LovedById_AppliesPlus12()
        {
            var fr = new StateFriendship();
            var inv = new FakeInventory();
            inv.Give("item_loved", 1);

            var item = Giftable("item_loved"); // loved por id => +12
            var r = GiftGivingProcessor.Process(Npc, "item_loved", item, Prefs(), fr, inv);

            Assert.AreEqual(GiftGivingOutcome.Accepted, r.Outcome);
            Assert.AreEqual(GiftTaste.Loved, r.Taste);
            Assert.AreEqual(12, r.AppliedDelta);
            Assert.AreEqual(0, inv.CountOf("item_loved"));
        }

        // ── CA-4: presente hated entrega delta negativo (clamp em 0 é da F26) ────────────────────────

        [Test]
        public void Process_HatedAccepted_NegativeDelta()
        {
            var fr = new StateFriendship();
            fr.State.AddPoints(Npc, 20); // base para enxergar a descida sem clamp no piso
            var inv = new FakeInventory();
            inv.Give("item_contraband", 1);

            var item = Giftable("item_contraband", "gift_undocumented_blackmarket"); // hated => -6
            var r = GiftGivingProcessor.Process(Npc, "item_contraband", item, Prefs(), fr, inv);

            Assert.AreEqual(GiftGivingOutcome.Accepted, r.Outcome);
            Assert.AreEqual(GiftTaste.Hated, r.Taste);
            Assert.AreEqual(-6, r.AppliedDelta, "hated entrega delta negativo à F26");
            Assert.IsTrue(r.ItemConsumed);
            Assert.AreEqual(14, fr.State.GetPoints(Npc));
        }

        // ── CA-3: cap diário — 2º presente no mesmo dia recusado, NÃO consome; dia+1 libera ──────────

        [Test]
        public void Process_SecondGiftSameDay_RefusedDailyLimit_NotConsumed()
        {
            var fr = new StateFriendship();
            var inv = new FakeInventory();
            inv.Give("item_food", 5);
            var item = Giftable("item_food", "gift_food_fine");

            var first = GiftGivingProcessor.Process(Npc, "item_food", item, Prefs(), fr, inv);
            Assert.AreEqual(GiftGivingOutcome.Accepted, first.Outcome);
            Assert.AreEqual(4, inv.CountOf("item_food"));

            var second = GiftGivingProcessor.Process(Npc, "item_food", item, Prefs(), fr, inv);
            Assert.AreEqual(GiftGivingOutcome.RefusedDailyLimit, second.Outcome);
            Assert.IsFalse(second.ItemConsumed);
            Assert.IsFalse(second.ReactionPublished);
            Assert.AreEqual(4, inv.CountOf("item_food"), "2º presente do dia não consome");
            Assert.AreEqual(6, fr.State.GetPoints(Npc), "sem ganho/perda no 2º");

            // dia seguinte libera
            fr.SetDay(2);
            var third = GiftGivingProcessor.Process(Npc, "item_food", item, Prefs(), fr, inv);
            Assert.AreEqual(GiftGivingOutcome.Accepted, third.Outcome);
            Assert.AreEqual(3, inv.CountOf("item_food"));
            Assert.AreEqual(12, fr.State.GetPoints(Npc));
        }

        // ── CA-2: fallback neutral quando o NPC não tem preferências (null) ──────────────────────────

        [Test]
        public void Process_NullPreferences_FallsBackNeutralPlus2()
        {
            var fr = new StateFriendship();
            var inv = new FakeInventory();
            inv.Give("item_any", 1);

            var item = Giftable("item_any", "gift_food_fine"); // sem prefs => neutral (+2)
            var r = GiftGivingProcessor.Process(Npc, "item_any", item, null, fr, inv);

            Assert.AreEqual(GiftGivingOutcome.Accepted, r.Outcome);
            Assert.AreEqual(GiftTaste.Neutral, r.Taste);
            Assert.AreEqual(2, r.AppliedDelta);
            Assert.AreEqual(2, fr.State.GetPoints(Npc));
        }

        // ── Pré-condições: item ausente do inventário => Failed sem efeito ───────────────────────────

        [Test]
        public void Process_ItemNotInInventory_Failed_NoEffect()
        {
            var fr = new StateFriendship();
            var inv = new FakeInventory(); // vazio
            var item = Giftable("item_food", "gift_food_fine");

            var r = GiftGivingProcessor.Process(Npc, "item_food", item, Prefs(), fr, inv);

            Assert.AreEqual(GiftGivingOutcome.Failed, r.Outcome);
            Assert.IsFalse(r.ItemConsumed);
            Assert.AreEqual(0, inv.RemoveCalls);
            Assert.AreEqual(0, fr.GiveGiftCalls, "sem o item, nem chega à F26");
        }

        [Test]
        public void Process_NullItemDefinition_TreatedAsNotGiftable()
        {
            var fr = new StateFriendship();
            var inv = new FakeInventory();
            inv.Give("item_unmapped", 1);

            // item não mapeado na matriz ⇒ BuildGiftItemDefinition devolve null ⇒ recusa silenciosa.
            var r = GiftGivingProcessor.Process(Npc, "item_unmapped", null, Prefs(), fr, inv);

            Assert.AreEqual(GiftGivingOutcome.RefusedNotGiftable, r.Outcome);
            Assert.IsFalse(r.ItemConsumed);
            Assert.AreEqual(0, fr.GiveGiftCalls);
        }

        // ── CA-1 (via matriz): precedência hated > loved aplicada de ponta a ponta no fluxo ──────────

        [Test]
        public void Process_LovedIdButHatedTag_ResolvesHated()
        {
            var fr = new StateFriendship();
            fr.State.AddPoints(Npc, 20);
            var inv = new FakeInventory();
            inv.Give("item_loved", 1);

            // item é loved por id E hated por tag => hated vence (rejeição pessoal).
            var item = Giftable("item_loved", "gift_undocumented_blackmarket");
            var r = GiftGivingProcessor.Process(Npc, "item_loved", item, Prefs(), fr, inv);

            Assert.AreEqual(GiftGivingOutcome.Accepted, r.Outcome);
            Assert.AreEqual(GiftTaste.Hated, r.Taste);
            Assert.AreEqual(-6, r.AppliedDelta);
        }

        // ── CA-5: integridade da matriz §4 (autoria em código) ───────────────────────────────────────

        [Test]
        public void Matrix_EveryRosterNpc_HasPreferences_WithAtLeastOneHated()
        {
            foreach (var entry in NpcTownRosterRegistry.AllEntries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.NpcId) ||
                    entry.PriorityTier == NpcTownRosterRegistry.NpcPriorityTier.Legacy) continue;
                var prefs = GiftTasteMatrixData.TryGetPreferences(entry.NpcId);
                Assert.IsNotNull(prefs, $"NPC '{entry.NpcId}' do roster deve ter GiftPreferences na matriz");
                Assert.IsTrue(prefs.HatedItemTags != null && prefs.HatedItemTags.Count >= 1,
                    $"NPC '{entry.NpcId}' deve ter >=1 hated");
            }
        }

        [Test]
        public void Matrix_AllCitedTags_AreInVocabulary()
        {
            var vocab = new HashSet<string>(GiftTasteMatrixData.AllGiftTags);
            foreach (var npcId in GiftTasteMatrixData.MappedNpcIds)
            {
                var p = GiftTasteMatrixData.TryGetPreferences(npcId);
                AssertTagsInVocab(p.LikedItemTags, vocab, npcId);
                AssertTagsInVocab(p.DislikedItemTags, vocab, npcId);
                AssertTagsInVocab(p.HatedItemTags, vocab, npcId);
                AssertTagsInVocab(p.NeutralItemTags, vocab, npcId);
            }
        }

        private static void AssertTagsInVocab(List<string> tags, HashSet<string> vocab, string npcId)
        {
            if (tags == null) return;
            foreach (var t in tags)
            {
                Assert.IsTrue(vocab.Contains(t), $"NPC '{npcId}': tag '{t}' fora do vocabulário gift_*");
            }
        }

        [Test]
        public void Matrix_BuildGiftItemDefinition_MappedItemIsGiftable_UnmappedIsNull()
        {
            var mapped = GiftTasteMatrixData.BuildGiftItemDefinition("item_material_iron_ore");
            Assert.IsNotNull(mapped);
            Assert.IsTrue(GiftTasteClassifier.IsGiftable(mapped), "item mapeado deve ser Giftable");
            Assert.Contains("gift_ore_metal", mapped.LoreTags);

            var unmapped = GiftTasteMatrixData.BuildGiftItemDefinition("item_does_not_exist");
            Assert.IsNull(unmapped, "item não mapeado => null (fallback de recusa silenciosa)");
        }

        // ── CA-5: round-trip dos campos estendidos da struct (default vazio, preservação) ────────────

        [Test]
        public void NpcGiftPreferences_ExtendedFields_DefaultEmpty_AndAssignable()
        {
            var fresh = new NpcGiftPreferences();
            Assert.IsNotNull(fresh.NeutralItemTags);
            Assert.IsNotNull(fresh.HatedItemTags);
            Assert.AreEqual(0, fresh.NeutralItemTags.Count, "NeutralItemTags default vazio");
            Assert.AreEqual(0, fresh.HatedItemTags.Count, "HatedItemTags default vazio");
            Assert.AreEqual(1, fresh.DailyGiftLimit, "DailyGiftLimit default = 1 (intacto)");

            fresh.HatedItemTags.Add("gift_undocumented_blackmarket");
            fresh.NeutralItemTags.Add("gift_food_hearty");
            Assert.AreEqual(1, fresh.HatedItemTags.Count);
            Assert.AreEqual("gift_food_hearty", fresh.NeutralItemTags[0]);

            // campos originais intactos
            fresh.LikedItemTags.Add("gift_food_fine");
            fresh.LovedItemIds.Add("item_loved");
            fresh.DislikedItemTags.Add("gift_junk_lowvalue");
            Assert.AreEqual(1, fresh.LikedItemTags.Count);
            Assert.AreEqual(1, fresh.LovedItemIds.Count);
            Assert.AreEqual(1, fresh.DislikedItemTags.Count);
        }

        // ── Evento de reação: shape/defaults ─────────────────────────────────────────────────────────

        [Test]
        public void NpcGiftReactionEvent_CarriesPayload()
        {
            var evt = new NpcGiftReactionEvent(
                "npc_x", "item_y", GiftTaste.Loved, 12);
            Assert.AreEqual("npc_x", evt.NpcId);
            Assert.AreEqual("item_y", evt.ItemId);
            Assert.AreEqual(GiftTaste.Loved, evt.Taste);
            Assert.AreEqual(12, evt.AppliedDelta);
        }
    }
}
