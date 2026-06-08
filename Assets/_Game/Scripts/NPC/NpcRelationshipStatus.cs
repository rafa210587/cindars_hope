namespace CindarsHope.NPC
{
    public enum RelationshipStatus
    {
        Unknown = 0,
        Single,
        MarriedToNpc,
        RomanceEligibleAnyPlayerGender,
        UnavailableForRomance,
        TooYoungOrNarrativelyBlocked,
        LateRomanceEligible
    }

    public enum RomanceEligibility { NotEligible = 0, EligibleAnyGender, EligibleConditional }

    public enum MarriageEligibility { NotEligible = 0, EligibleAfterQuestline, EligibleAfterRelationship }
}
