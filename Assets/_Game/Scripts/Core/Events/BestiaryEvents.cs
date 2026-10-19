namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_21 — published once each time a new bestiary knowledge category is unlocked for a
    /// creature (by combat threshold, external grant, or boss-defeat reveal). Consumed by the
    /// existing toast/HUD feedback layer (the bestiary tab F14 listens for entry refreshes).
    /// Carries only stable string ids and the creature's SpoilerTier (no Unity refs).
    /// Category names are the stable strings from <c>CindarsHope.Enemy.BestiaryKnowledgeCategory</c>.
    /// </summary>
    public class BestiaryKnowledgeUnlockedEvent
    {
        public string EnemyId;
        public string Category;
        public int SpoilerTier;

        public BestiaryKnowledgeUnlockedEvent(string enemyId, string category, int spoilerTier)
        {
            EnemyId = enemyId ?? string.Empty;
            Category = category ?? string.Empty;
            SpoilerTier = spoilerTier;
        }
    }

    /// <summary>
    /// fable_21 (EMENDA 2026-06-12-B) — published once when the player crosses a "10 creatures
    /// Studied" milestone, granting +1 skill point (max 5 total). Idempotent: the bestiary save
    /// section persists the number of milestones already granted, so a milestone is never
    /// re-published after save/load. The skill-tree layer (F34/F42 grant hook) consumes this to
    /// add the point; <see cref="MilestoneIndex"/> is 1-based (1..5).
    /// </summary>
    public class BestiaryMilestoneReachedEvent
    {
        public int MilestoneIndex;
        public int SkillPointsGranted;
        public int StudiedCount;

        public BestiaryMilestoneReachedEvent(int milestoneIndex, int skillPointsGranted, int studiedCount)
        {
            MilestoneIndex = milestoneIndex;
            SkillPointsGranted = skillPointsGranted;
            StudiedCount = studiedCount;
        }
    }
}
