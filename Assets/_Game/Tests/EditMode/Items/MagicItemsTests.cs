using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Items;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Items
{
    // fable_31 — EditMode coverage for the PURE magic-item logic (no Play Mode, no Unity scene):
    // identification swap (CA-1), invalid-id fallback, deterministic reveal + loot weight (stable-run),
    // pouch +slots gain/loss/overflow (CA-2), mirror stable-run guard (CA-3), Veska weekly rotation (CA-4),
    // passive flags, and the four use-effects incl. the whetstone once-per-day gate.
    [TestFixture]
    public class MagicItemsTests
    {
        // In-memory inventory fake implementing the swap surface. Reveal mapping is supplied per-test.
        private sealed class FakeSwapInventory : IItemSwapInventory
        {
            private readonly Dictionary<string, int> _items = new Dictionary<string, int>();
            private readonly Dictionary<string, string> _reveal = new Dictionary<string, string>();
            public bool FailNextAdd;

            public void Seed(string id, int amount) => _items[id] = amount;
            public void MapReveal(string unidentified, string revealed) => _reveal[unidentified] = revealed;

            public int GetAmount(string itemId) =>
                !string.IsNullOrEmpty(itemId) && _items.TryGetValue(itemId, out var a) ? a : 0;

            public bool RemoveItem(string itemId, int amount)
            {
                if (GetAmount(itemId) < amount) return false;
                _items[itemId] -= amount;
                return true;
            }

            public bool AddItem(string itemId, int amount)
            {
                if (FailNextAdd)
                {
                    FailNextAdd = false;
                    return false;
                }
                _items[itemId] = GetAmount(itemId) + amount;
                return true;
            }

            public bool TryResolveRevealedId(string unidentifiedItemId, out string revealedItemId) =>
                _reveal.TryGetValue(unidentifiedItemId, out revealedItemId) && !string.IsNullOrEmpty(revealedItemId);
        }

        [SetUp]
        public void SetUp() => GameEventBus.ClearAll();

        [TearDown]
        public void TearDown() => GameEventBus.ClearAll();

        // ── CA-1: identifying performs a 1:1 swap and publishes ItemIdentifiedEvent ──────────
        [Test]
        public void IdentifyItem_SwapsOneForOne_AndPublishesEvent()
        {
            var inv = new FakeSwapInventory();
            inv.Seed(MagicItemCatalog.UnidentifiedTrinketId, 1);
            inv.MapReveal(MagicItemCatalog.UnidentifiedTrinketId, MagicItemCatalog.PendantOfEchoes);

            string revealed = null;
            void Handler(ItemIdentifiedEvent e) => revealed = e.RevealedItemId;
            GameEventBus.Subscribe<ItemIdentifiedEvent>(Handler);

            var service = new ItemIdentificationService(inv);
            var result = service.IdentifyItem(MagicItemCatalog.UnidentifiedTrinketId);

            GameEventBus.Unsubscribe<ItemIdentifiedEvent>(Handler);

            Assert.IsTrue(result.Success, "Identification should succeed.");
            Assert.AreEqual(0, inv.GetAmount(MagicItemCatalog.UnidentifiedTrinketId), "Unidentified consumed.");
            Assert.AreEqual(1, inv.GetAmount(MagicItemCatalog.PendantOfEchoes), "Real item added 1:1.");
            Assert.AreEqual(MagicItemCatalog.PendantOfEchoes, revealed, "Event carries revealed id.");
        }

        [Test]
        public void IdentifyItem_NotPresent_FailsCleanly()
        {
            var inv = new FakeSwapInventory();
            inv.MapReveal(MagicItemCatalog.UnidentifiedTrinketId, MagicItemCatalog.PendantOfEchoes);

            var result = new ItemIdentificationService(inv).IdentifyItem(MagicItemCatalog.UnidentifiedTrinketId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(IdentifyOutcome.NotPresent, result.Outcome);
        }

        // ── invalid-id fallback: no reveal mapping ⇒ no swap, unidentified preserved ─────────
        [Test]
        public void IdentifyItem_NoRevealMapping_KeepsUnidentified()
        {
            var inv = new FakeSwapInventory();
            inv.Seed(MagicItemCatalog.UnidentifiedTrinketId, 2);

            var result = new ItemIdentificationService(inv).IdentifyItem(MagicItemCatalog.UnidentifiedTrinketId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(IdentifyOutcome.NoRevealMapping, result.Outcome);
            Assert.AreEqual(2, inv.GetAmount(MagicItemCatalog.UnidentifiedTrinketId), "Nothing consumed on bad mapping.");
        }

        // ── atomic rollback: if the real item cannot be added, the unidentified is restored ──
        [Test]
        public void IdentifyItem_AddFails_RollsBack_NothingLost()
        {
            var inv = new FakeSwapInventory();
            inv.Seed(MagicItemCatalog.UnidentifiedTrinketId, 1);
            inv.MapReveal(MagicItemCatalog.UnidentifiedTrinketId, MagicItemCatalog.PouchOfHolding);
            inv.FailNextAdd = true;

            var result = new ItemIdentificationService(inv).IdentifyItem(MagicItemCatalog.UnidentifiedTrinketId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(IdentifyOutcome.AddFailed, result.Outcome);
            Assert.AreEqual(1, inv.GetAmount(MagicItemCatalog.UnidentifiedTrinketId), "Unidentified restored on rollback.");
            Assert.AreEqual(0, inv.GetAmount(MagicItemCatalog.PouchOfHolding), "Real item not present after failed add.");
        }

        // ── deterministic reveal (stable-run): same run+level+salt ⇒ same revealed item ──────
        [Test]
        public void ResolveRevealedItemId_IsDeterministic_PerRunAndLevel()
        {
            var a = MagicItemCatalog.ResolveRevealedItemId("run_abc", 12, "chest_07");
            var b = MagicItemCatalog.ResolveRevealedItemId("run_abc", 12, "chest_07");
            Assert.AreEqual(a, b, "Same inputs must reveal the same item (stable-run).");
            Assert.Contains(a, (System.Collections.ICollection)MagicItemCatalog.MagicItemIds, "Reveal must be one of the 8 magic items.");
        }

        [Test]
        public void ResolveRevealedItemId_DiffersAcrossSources()
        {
            // Different salts generally produce different reveals; assert at least the function is salt-sensitive
            // across a small spread (not all identical).
            var set = new HashSet<string>();
            for (int i = 0; i < 8; i++)
            {
                set.Add(MagicItemCatalog.ResolveRevealedItemId("run_xyz", 15, "chest_" + i));
            }
            Assert.GreaterOrEqual(set.Count, 2, "Reveal should vary across different chest salts.");
        }

        // ── CA-1: loot weight is gated by level >= 10 and deterministic by seed ──────────────
        [Test]
        public void LootWeight_BelowLevel10_NeverDropsTrinket()
        {
            for (int level = 1; level < MagicItemLootWeighting.MinCaveLevel; level++)
            {
                Assert.IsFalse(
                    MagicItemLootWeighting.RollUnidentifiedTrinket("run_1", level, "chest_a", 100),
                    $"Level {level} must never drop the trinket, even at weight 100.");
            }
        }

        [Test]
        public void LootWeight_AtLevel10_Weight100_AlwaysDrops_AndIsDeterministic()
        {
            Assert.IsTrue(MagicItemLootWeighting.RollUnidentifiedTrinket("run_1", 10, "chest_a", 100));
            // Determinism: same inputs ⇒ same result across calls.
            var first = MagicItemLootWeighting.RollUnidentifiedTrinket("run_1", 14, "chest_b", 18);
            var second = MagicItemLootWeighting.RollUnidentifiedTrinket("run_1", 14, "chest_b", 18);
            Assert.AreEqual(first, second, "Loot roll must be deterministic per run/level/chest (stable-run).");
        }

        [Test]
        public void LootWeight_ZeroWeight_NeverDrops()
        {
            Assert.IsFalse(MagicItemLootWeighting.RollUnidentifiedTrinket("run_1", 30, "chest_a", 0));
        }

        // ── passive flags: presence of each passive item raises exactly its flag ─────────────
        [Test]
        public void PassiveState_RaisesFlagsForPresentItems()
        {
            var state = new MagicItemPassiveState();
            var changed = state.Recompute(new[] { MagicItemCatalog.PendantOfEchoes, MagicItemCatalog.LanternOfTrueSight });

            Assert.IsTrue(changed);
            Assert.IsTrue(state.IsFlagActive(MagicItemCatalog.FlagShowEnemyHealth), "Pendant → show enemy HP.");
            Assert.IsTrue(state.IsFlagActive(MagicItemCatalog.FlagRevealAmbush), "Lantern → reveal ambush.");
            Assert.IsFalse(state.IsFlagActive(MagicItemCatalog.FlagPersistentLight), "Candle absent → no light flag.");
            Assert.AreEqual(0, state.SlotBonus, "No pouch → no slot bonus.");
        }

        [Test]
        public void PassiveState_RemovingItem_ClearsFlag()
        {
            var state = new MagicItemPassiveState();
            state.Recompute(new[] { MagicItemCatalog.CandleOfTheDepths });
            Assert.IsTrue(state.IsFlagActive(MagicItemCatalog.FlagPersistentLight));

            var changed = state.Recompute(new string[0]);
            Assert.IsTrue(changed, "State changed when the candle left the inventory.");
            Assert.IsFalse(state.IsFlagActive(MagicItemCatalog.FlagPersistentLight));
        }

        // ── CA-2: pouch +6 on gain, −6 on loss ───────────────────────────────────────────────
        [Test]
        public void Pouch_GrantsSixSlots_WhenPresent_RemovesWhenAbsent()
        {
            var state = new MagicItemPassiveState();

            state.Recompute(new[] { MagicItemCatalog.PouchOfHolding });
            Assert.AreEqual(MagicItemCatalog.PouchSlotBonus, state.SlotBonus, "Pouch present → +6 slots.");
            Assert.IsTrue(state.IsFlagActive(MagicItemCatalog.FlagInventorySlotBonus));

            state.Recompute(new string[0]);
            Assert.AreEqual(0, state.SlotBonus, "Pouch absent → bonus removed.");
        }

        [Test]
        public void Pouch_EffectiveCapacity_AddsBonusOnlyWhenPresent()
        {
            Assert.AreEqual(36, PouchCapacityGuard.EffectiveCapacity(30, true));
            Assert.AreEqual(30, PouchCapacityGuard.EffectiveCapacity(30, false));
        }

        // ── CA-2: overflow-safe — dropping the pouch with a full inventory is blocked ─────────
        [Test]
        public void Pouch_DropWithOverflow_IsBlocked_NothingDestroyed()
        {
            // 34 occupied slots while pouch grants capacity 36; removing pouch shrinks capacity to 30 → overflow 4.
            var decision = PouchCapacityGuard.EvaluateDropPouch(occupiedSlots: 34, baseCapacity: 30);

            Assert.IsFalse(decision.Allowed, "Drop must be blocked when it would overflow.");
            Assert.AreEqual(30, decision.CapacityAfter);
            Assert.AreEqual(4, decision.Overflow, "Overflow surfaced for 'mochila transbordando'.");
        }

        [Test]
        public void Pouch_DropWithoutOverflow_IsAllowed()
        {
            var decision = PouchCapacityGuard.EvaluateDropPouch(occupiedSlots: 28, baseCapacity: 30);
            Assert.IsTrue(decision.Allowed);
            Assert.AreEqual(0, decision.Overflow);
        }

        // ── CA-3: mirror preserves the stable run (same seed + level) ─────────────────────────
        [Test]
        public void Mirror_SameRunAndLevel_PreservesStableRun()
        {
            var decision = MirrorOfReturnDecision.Evaluate("run_seed_1", 7, "run_seed_1", 7);
            Assert.IsTrue(decision.StableRunPreserved);
            Assert.IsTrue(decision.CanTeleport);
        }

        [Test]
        public void Mirror_ChangedSeed_IsRejected()
        {
            var decision = MirrorOfReturnDecision.Evaluate("run_seed_1", 7, "run_seed_2", 7);
            Assert.IsFalse(decision.CanTeleport, "A changed run seed means regeneration — must reject.");
            Assert.AreEqual("run_seed_changed", decision.Reason);
        }

        [Test]
        public void Mirror_ChangedLevel_IsRejected()
        {
            var decision = MirrorOfReturnDecision.Evaluate("run_seed_1", 7, "run_seed_1", 8);
            Assert.IsFalse(decision.CanTeleport);
            Assert.AreEqual("cave_level_changed", decision.Reason);
        }

        [Test]
        public void Mirror_NoActiveRun_IsRejected()
        {
            var decision = MirrorOfReturnDecision.Evaluate("", 1, "", 1);
            Assert.IsFalse(decision.CanTeleport);
            Assert.AreEqual("no_active_cave_run", decision.Reason);
        }

        // ── CA-4: Veska weekly rotation is deterministic per week ─────────────────────────────
        [Test]
        public void Veska_SameWeek_SameRotationToken()
        {
            // days 8..14 → week 1.
            var tokenDay8 = VeskaWeeklyRotationService.GetWeeklyRotationToken(8);
            var tokenDay14 = VeskaWeeklyRotationService.GetWeeklyRotationToken(14);
            Assert.AreEqual(tokenDay8, tokenDay14, "Same week ⇒ same rotation.");
            Assert.AreEqual(1, VeskaWeeklyRotationService.WeekFromDay(8));
            Assert.AreEqual(1, VeskaWeeklyRotationService.WeekFromDay(14));
        }

        [Test]
        public void Veska_NextWeek_RotationChanges()
        {
            var week1 = VeskaWeeklyRotationService.GetWeeklyRotationToken(10); // week 1
            var week2 = VeskaWeeklyRotationService.GetWeeklyRotationToken(15); // week 2
            Assert.AreNotEqual(week1, week2, "A new week must rotate the offer.");
            Assert.IsTrue(VeskaWeeklyRotationService.RotationChangedBetween(10, 15));
            Assert.IsFalse(VeskaWeeklyRotationService.RotationChangedBetween(10, 12));
        }

        [Test]
        public void Veska_OffersUnidentifiedTrinket()
        {
            Assert.AreEqual(MagicItemCatalog.UnidentifiedTrinketId, VeskaWeeklyRotationService.GetWeeklyOfferItemId(3));
        }

        // ── use effects: each magic use item resolves to the right effect ─────────────────────
        [Test]
        public void UseService_ResolvesEachEffect()
        {
            var svc = new MagicItemUseService();
            Assert.AreEqual(MagicUseEffect.WardEnemies, svc.Resolve(MagicItemCatalog.BellOfWarding, 1).Effect);
            Assert.AreEqual(MagicUseEffect.ReturnToEntrance, svc.Resolve(MagicItemCatalog.MirrorOfReturn, 1).Effect);
            Assert.AreEqual(MagicUseEffect.AdvanceToDawn, svc.Resolve(MagicItemCatalog.HourglassOfDawn, 1).Effect);
        }

        [Test]
        public void UseService_NonMagicUseItem_IsRefused()
        {
            var result = new MagicItemUseService().Resolve("item_consumable_health_potion", 1);
            Assert.IsFalse(result.Consumed);
            Assert.AreEqual(MagicUseEffect.None, result.Effect);
        }

        // ── whetstone: once-per-day gate refuses a second use the same day, allows it next day ─
        [Test]
        public void Whetstone_OncePerDay_RefusesSecondUseSameDay()
        {
            var svc = new MagicItemUseService();

            var firstToday = svc.Resolve(MagicItemCatalog.WhetstoneEternal, 5);
            Assert.IsTrue(firstToday.Consumed, "First whetstone use of the day succeeds.");
            Assert.AreEqual(MagicUseEffect.FreeRepair, firstToday.Effect);

            var secondToday = svc.Resolve(MagicItemCatalog.WhetstoneEternal, 5);
            Assert.IsFalse(secondToday.Consumed, "Second whetstone use the same day is refused (not consumed).");

            var nextDay = svc.Resolve(MagicItemCatalog.WhetstoneEternal, 6);
            Assert.IsTrue(nextDay.Consumed, "A new day resets the free repair.");
        }

        // ── catalog taxonomy sanity: each item is classified exactly once ─────────────────────
        [Test]
        public void Catalog_ClassifiesItemsCorrectly()
        {
            Assert.AreEqual(8, MagicItemCatalog.MagicItemIds.Count);
            // The four passive items raise a flag; the four use items are use-routed; disjoint sets.
            foreach (var id in MagicItemCatalog.MagicItemIds)
            {
                var passive = MagicItemCatalog.IsPassiveItem(id);
                var use = MagicItemCatalog.IsUseItem(id);
                Assert.IsTrue(passive ^ use, $"'{id}' must be either passive or use, not both/neither.");
            }
            Assert.IsTrue(MagicItemCatalog.IsUnidentifiedTrinket(MagicItemCatalog.UnidentifiedTrinketId));
            Assert.IsFalse(MagicItemCatalog.IsMagicItem(MagicItemCatalog.UnidentifiedTrinketId), "Trinket is not one of the 8.");
        }
    }
}
