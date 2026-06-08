namespace CindarsHope.Companions
{
    public static class CompanionAvailabilityResolver
    {
        public static AvailabilityReason ResolveAvailability(
            CompanionEligibilityFlags eligibility,
            CompanionUnlockState unlockState,
            CompanionBondState bondState,
            int currentDay,
            string requestContext = "Farm")
        {
            if (eligibility == null || unlockState == null || bondState == null)
                return AvailabilityReason.Unavailable;

            if (eligibility.CompanionUnavailable)
                return AvailabilityReason.Unavailable;

            if (unlockState.State == UnlockState.Locked)
                return AvailabilityReason.LockedByReputation;

            if (unlockState.State == UnlockState.Unavailable)
                return AvailabilityReason.Unavailable;

            if (eligibility.CompanionLockedByStory)
                return AvailabilityReason.LockedByStory;

            if (bondState.InjuryState == InjuryState.Incapacitated)
                return AvailabilityReason.UnavailableByInjury;

            if (bondState.Fatigue > 80)
                return AvailabilityReason.UnavailableByFatigue;

            if (unlockState.State == UnlockState.StoryOnly && requestContext != "Story")
                return AvailabilityReason.LockedByStory;

            return AvailabilityReason.Available;
        }

        public static bool CanInviteCompanion(
            CompanionEligibilityFlags eligibility,
            CompanionUnlockState unlockState,
            CompanionBondState bondState,
            int currentDay,
            string context)
        {
            var reason = ResolveAvailability(eligibility, unlockState, bondState, currentDay, context);
            return reason == AvailabilityReason.Available;
        }

        public static bool IsRoleUnlocked(CompanionUnlockState unlockState, string role)
        {
            if (unlockState == null)
                return false;
            return unlockState.UnlockedRoles.Contains(role);
        }

        public static void UnlockRole(CompanionUnlockState unlockState, string role)
        {
            if (unlockState != null && !unlockState.UnlockedRoles.Contains(role))
                unlockState.UnlockedRoles.Add(role);
        }

        public static void SetInjury(CompanionBondState bondState, InjuryState injury)
        {
            if (bondState != null)
                bondState.InjuryState = injury;
        }

        public static bool CanRecruitByReputation(CompanionUnlockState unlockState, int reputationTier)
        {
            if (unlockState == null)
                return false;
            if (unlockState.State == UnlockState.LockedByReputation)
                return reputationTier >= unlockState.UnlockedByReputationTier;
            return unlockState.State != UnlockState.Locked;
        }

        public static bool CanRecruitByQuest(CompanionUnlockState unlockState, string questId)
        {
            if (unlockState == null)
                return false;
            return unlockState.UnlockedByQuestIds.Contains(questId);
        }
    }
}
