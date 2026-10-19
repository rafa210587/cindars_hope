using NUnit.Framework;
using CindarsHope.NPC.Schedule;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// fable_11 (CA-1 / CA-4) — deterministic schedule block resolution and availability by hour for
    /// each archetype. This is the testable core that closes the WAVE25 TIME_BLOCK_DEBT. No Unity
    /// types required — pure logic with synthetic hours.
    /// </summary>
    [TestFixture]
    public class NpcScheduleBlocksTests
    {
        // ── CA-1: block transitions by hour, per archetype ──────────────────────────────────────

        [Test]
        public void Shopkeeper_Work_DuringDay()
        {
            Assert.AreEqual(NpcRuntimeBlock.Work, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Shopkeeper, 10));
            Assert.AreEqual(NpcRuntimeBlock.Work, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Shopkeeper, 17));
        }

        [Test]
        public void Shopkeeper_Social_InEvening()
        {
            Assert.AreEqual(NpcRuntimeBlock.Social, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Shopkeeper, 19));
        }

        [Test]
        public void Shopkeeper_Home_LateEvening_And_Night_Deep()
        {
            Assert.AreEqual(NpcRuntimeBlock.Home, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Shopkeeper, 23));
            Assert.AreEqual(NpcRuntimeBlock.Home, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Shopkeeper, 7));
            Assert.AreEqual(NpcRuntimeBlock.Night, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Shopkeeper, 3));
        }

        [Test]
        public void Guard_OnDuty_MostOfDay_OffDeepNight()
        {
            Assert.AreEqual(NpcRuntimeBlock.Work, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Guard, 6));
            Assert.AreEqual(NpcRuntimeBlock.Work, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Guard, 21));
            Assert.AreEqual(NpcRuntimeBlock.Night, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Guard, 3));
        }

        [Test]
        public void Night_OpensNightMarket_WrapsMidnight()
        {
            // 20:00-02:00 work (night market); resting at home otherwise.
            Assert.AreEqual(NpcRuntimeBlock.Work, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Night, 21));
            Assert.AreEqual(NpcRuntimeBlock.Work, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Night, 1));
            Assert.AreEqual(NpcRuntimeBlock.Home, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Night, 14));
        }

        [Test]
        public void Wanderer_Social_ByDay_Home_AtNight()
        {
            Assert.AreEqual(NpcRuntimeBlock.Social, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Wanderer, 12));
            Assert.AreEqual(NpcRuntimeBlock.Home, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Wanderer, 21));
            Assert.AreEqual(NpcRuntimeBlock.Night, NpcScheduleBlockResolver.ResolveBlock(NpcScheduleArchetype.Wanderer, 4));
        }

        // ── CA-4: availability (Yael inverted: 14h unavailable, 22h available) ───────────────────

        [Test]
        public void NightVendor_Unavailable_AtMidday()
        {
            Assert.IsFalse(NpcScheduleBlockResolver.IsAvailable(NpcScheduleArchetype.Night, 14));
        }

        [Test]
        public void NightVendor_Available_AtNight()
        {
            Assert.IsTrue(NpcScheduleBlockResolver.IsAvailable(NpcScheduleArchetype.Night, 22));
        }

        [Test]
        public void Shopkeeper_Available_DuringWork_Unavailable_DeepNight()
        {
            Assert.IsTrue(NpcScheduleBlockResolver.IsAvailable(NpcScheduleArchetype.Shopkeeper, 11));
            Assert.IsFalse(NpcScheduleBlockResolver.IsAvailable(NpcScheduleArchetype.Shopkeeper, 3));
        }

        [Test]
        public void Shopkeeper_Available_DuringEveningSocial()
        {
            Assert.IsTrue(NpcScheduleBlockResolver.IsAvailable(NpcScheduleArchetype.Shopkeeper, 19));
        }

        // ── Hour normalization (wrap) ───────────────────────────────────────────────────────────

        [Test]
        public void NormalizeHour_Wraps()
        {
            Assert.AreEqual(1, NpcScheduleBlockResolver.NormalizeHour(25));
            Assert.AreEqual(23, NpcScheduleBlockResolver.NormalizeHour(-1));
            Assert.AreEqual(0, NpcScheduleBlockResolver.NormalizeHour(24));
        }

        // ── Anchor suffix mapping per block ──────────────────────────────────────────────────────

        [Test]
        public void AnchorSuffix_Work_Social_Home()
        {
            Assert.AreEqual("work", NpcScheduleBlockResolver.AnchorSuffixForBlock(NpcRuntimeBlock.Work));
            Assert.AreEqual("social", NpcScheduleBlockResolver.AnchorSuffixForBlock(NpcRuntimeBlock.Social));
            Assert.AreEqual("home", NpcScheduleBlockResolver.AnchorSuffixForBlock(NpcRuntimeBlock.Home));
            // Night NPCs sleep at home, so Night maps to the home anchor too.
            Assert.AreEqual("home", NpcScheduleBlockResolver.AnchorSuffixForBlock(NpcRuntimeBlock.Night));
        }

        // ── Archetype mapping from movement profile ──────────────────────────────────────────────

        [Test]
        public void Archetype_FromMovementProfile_NightAndPatrolAndShop()
        {
            Assert.AreEqual(NpcScheduleArchetype.Night,
                NpcScheduleBlockResolver.ArchetypeFromMovementProfile("NightOnly/WanderHidden", true));
            Assert.AreEqual(NpcScheduleArchetype.Guard,
                NpcScheduleBlockResolver.ArchetypeFromMovementProfile("Patrol/TownRoad", false));
            Assert.AreEqual(NpcScheduleArchetype.Shopkeeper,
                NpcScheduleBlockResolver.ArchetypeFromMovementProfile("ShopKeeperFixed", true));
            Assert.AreEqual(NpcScheduleArchetype.Shopkeeper,
                NpcScheduleBlockResolver.ArchetypeFromMovementProfile("Stationary/ChamberDesk", false),
                "Velorin and other fixed civic workers must remain at work during business hours.");
            Assert.AreEqual(NpcScheduleArchetype.Shopkeeper,
                NpcScheduleBlockResolver.ArchetypeFromMovementProfile("Stationary/Cemetery", false),
                "Tibbet must work at the cemetery instead of wandering all day.");
            Assert.AreEqual(NpcScheduleArchetype.Wanderer,
                NpcScheduleBlockResolver.ArchetypeFromMovementProfile("WanderWithinZone", false));
        }

        // ── Profile factory produces the 4 canonical blocks with stable anchor IDs ───────────────

        [Test]
        public void Profile_CreateForArchetype_HasFourBlocks_AndStableAnchorIds()
        {
            var profile = NpcScheduleProfile.CreateForArchetype("npc_yael", NpcScheduleArchetype.Night);
            Assert.AreEqual("npc_yael", profile.NpcId);
            Assert.AreEqual(NpcScheduleArchetype.Night, profile.Archetype);
            Assert.AreEqual(4, profile.Blocks.Count);

            // Work block points at npc_yael_work and is interactable; home block is not.
            var work = profile.Blocks.Find(b => b.RuntimeBlock == NpcRuntimeBlock.Work);
            Assert.IsNotNull(work);
            Assert.AreEqual("npc_yael_work", work.AnchorId);
            Assert.IsTrue(work.CanInteract);

            var home = profile.Blocks.Find(b => b.RuntimeBlock == NpcRuntimeBlock.Home);
            Assert.IsNotNull(home);
            Assert.AreEqual("npc_yael_home", home.AnchorId);
            Assert.IsFalse(home.CanInteract);
        }

        [Test]
        public void Profile_ResolveAnchorId_FollowsHour()
        {
            var yael = NpcScheduleProfile.CreateForArchetype("npc_yael", NpcScheduleArchetype.Night);
            // Night vendor at 22h → work anchor; at 14h → home anchor.
            Assert.AreEqual("npc_yael_work", yael.ResolveAnchorId(22));
            Assert.AreEqual("npc_yael_home", yael.ResolveAnchorId(14));

            var shop = NpcScheduleProfile.CreateForArchetype("npc_renko", NpcScheduleArchetype.Shopkeeper);
            Assert.AreEqual("npc_renko_work", shop.ResolveAnchorId(11));
            Assert.AreEqual("npc_renko_social", shop.ResolveAnchorId(19));
        }
    }
}
