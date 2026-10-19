using CindarsHope.Companions;
using NUnit.Framework;

namespace CindarsHope.Tests.Companions
{
    public class CompanionAvailabilityResolverTests
    {
        private CompanionEligibilityFlags _eligibility;
        private CompanionUnlockState _unlockState;
        private CompanionBondState _bondState;

        [SetUp]
        public void Setup()
        {
            _eligibility = new CompanionEligibilityFlags
            {
                NpcId = "npc_aria",
                CanBeFarmCompanion = true,
                CanBeCaveCompanion = true,
                CanBeQuestCompanion = true,
                CompanionLockedByStory = false,
                CompanionLockedByReputation = false,
                CompanionLockedByQuest = false,
                CompanionUnavailable = false
            };

            _unlockState = new CompanionUnlockState
            {
                CompanionId = "companion_aria",
                NpcId = "npc_aria",
                State = UnlockState.Unlocked
            };

            _bondState = new CompanionBondState
            {
                CompanionId = "companion_aria",
                BondLevel = 3,
                TrustPoints = 50,
                Fatigue = 20,
                InjuryState = InjuryState.Healthy
            };
        }

        [Test]
        public void Available_WhenEligibleUnlockedHealthy()
        {
            var result = CompanionAvailabilityResolver.ResolveAvailability(
                _eligibility, _unlockState, _bondState, 10, "Farm");
            Assert.AreEqual(AvailabilityReason.Available, result);
        }

        [Test]
        public void LockedByStory_WhenStoryLocked()
        {
            _eligibility.CompanionLockedByStory = true;
            var result = CompanionAvailabilityResolver.ResolveAvailability(
                _eligibility, _unlockState, _bondState, 10, "Farm");
            Assert.AreEqual(AvailabilityReason.LockedByStory, result);
        }

        [Test]
        public void Locked_WhenUnlockStateLocked()
        {
            _unlockState.State = UnlockState.Locked;
            var result = CompanionAvailabilityResolver.ResolveAvailability(
                _eligibility, _unlockState, _bondState, 10, "Farm");
            Assert.AreEqual(AvailabilityReason.LockedByReputation, result);
        }

        [Test]
        public void UnavailableByInjury_WhenIncapacitated()
        {
            _bondState.InjuryState = InjuryState.Incapacitated;
            var result = CompanionAvailabilityResolver.ResolveAvailability(
                _eligibility, _unlockState, _bondState, 10, "Farm");
            Assert.AreEqual(AvailabilityReason.UnavailableByInjury, result);
        }

        [Test]
        public void UnavailableByFatigue_WhenHighFatigue()
        {
            _bondState.Fatigue = 85;
            var result = CompanionAvailabilityResolver.ResolveAvailability(
                _eligibility, _unlockState, _bondState, 10, "Farm");
            Assert.AreEqual(AvailabilityReason.UnavailableByFatigue, result);
        }

        [Test]
        public void CanInvite_WhenAvailable()
        {
            var canInvite = CompanionAvailabilityResolver.CanInviteCompanion(
                _eligibility, _unlockState, _bondState, 10, "Farm");
            Assert.IsTrue(canInvite);
        }

        [Test]
        public void CannotInvite_WhenInjured()
        {
            _bondState.InjuryState = InjuryState.Injured;
            var canInvite = CompanionAvailabilityResolver.CanInviteCompanion(
                _eligibility, _unlockState, _bondState, 10, "Farm");
            Assert.IsTrue(canInvite);
        }

        [Test]
        public void CannotInvite_WhenIncapacitated()
        {
            _bondState.InjuryState = InjuryState.Incapacitated;
            var canInvite = CompanionAvailabilityResolver.CanInviteCompanion(
                _eligibility, _unlockState, _bondState, 10, "Farm");
            Assert.IsFalse(canInvite);
        }

        [Test]
        public void UnlockRole_AddsRoleToList()
        {
            CompanionAvailabilityResolver.UnlockRole(_unlockState, "FarmHelper");
            Assert.IsTrue(CompanionAvailabilityResolver.IsRoleUnlocked(_unlockState, "FarmHelper"));
        }

        [Test]
        public void IsRoleUnlocked_ReturnsFalse_WhenNotUnlocked()
        {
            var isUnlocked = CompanionAvailabilityResolver.IsRoleUnlocked(_unlockState, "CaveHelper");
            Assert.IsFalse(isUnlocked);
        }

        [Test]
        public void SetInjury_UpdatesBondState()
        {
            CompanionAvailabilityResolver.SetInjury(_bondState, InjuryState.Injured);
            Assert.AreEqual(InjuryState.Injured, _bondState.InjuryState);
        }

        [Test]
        public void CanRecruitByReputation_WhenReputationTierMeets()
        {
            _unlockState.State = UnlockState.LockedByReputation;
            _unlockState.UnlockedByReputationTier = 2;
            var canRecruit = CompanionAvailabilityResolver.CanRecruitByReputation(_unlockState, 3);
            Assert.IsTrue(canRecruit);
        }

        [Test]
        public void CanRecruitByReputation_ReturnsFalse_WhenReputationInsufficient()
        {
            _unlockState.State = UnlockState.LockedByReputation;
            _unlockState.UnlockedByReputationTier = 3;
            var canRecruit = CompanionAvailabilityResolver.CanRecruitByReputation(_unlockState, 2);
            Assert.IsFalse(canRecruit);
        }

        [Test]
        public void CanRecruitByQuest_WhenQuestCompleted()
        {
            _unlockState.UnlockedByQuestIds.Add("quest_aria_meeting");
            var canRecruit = CompanionAvailabilityResolver.CanRecruitByQuest(_unlockState, "quest_aria_meeting");
            Assert.IsTrue(canRecruit);
        }

        [Test]
        public void CanRecruitByQuest_ReturnsFalse_WhenQuestNotCompleted()
        {
            var canRecruit = CompanionAvailabilityResolver.CanRecruitByQuest(_unlockState, "quest_aria_meeting");
            Assert.IsFalse(canRecruit);
        }

        [Test]
        public void Unavailable_WhenEligibilityFlagsNull()
        {
            var result = CompanionAvailabilityResolver.ResolveAvailability(
                null, _unlockState, _bondState, 10, "Farm");
            Assert.AreEqual(AvailabilityReason.Unavailable, result);
        }

        [Test]
        public void Unavailable_WhenCompanionMarkedUnavailable()
        {
            _eligibility.CompanionUnavailable = true;
            var result = CompanionAvailabilityResolver.ResolveAvailability(
                _eligibility, _unlockState, _bondState, 10, "Farm");
            Assert.AreEqual(AvailabilityReason.Unavailable, result);
        }
    }
}
