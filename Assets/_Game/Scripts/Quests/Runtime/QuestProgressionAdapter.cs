using CindarsHope.Player.Progression;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// fable_34 — thin adapter wrapping PlayerProgressionManager for QuestService scaled XP rewards
    /// and the main-act +1 skill point grant. Mirrors QuestGoldAdapter (WAVE15 pattern).
    /// </summary>
    public sealed class QuestProgressionAdapter : IQuestProgressionAccess
    {
        private readonly PlayerProgressionManager _progressionManager;

        public QuestProgressionAdapter(PlayerProgressionManager progressionManager)
        {
            _progressionManager = progressionManager;
        }

        public void AddXp(int amount)
        {
            if (_progressionManager == null)
            {
                Debug.LogWarning("[QuestProgressionAdapter] PlayerProgressionManager is null — XP reward not applied.");
                return;
            }
            _progressionManager.AddXp(amount);
        }

        public void GrantSkillPoints(int amount)
        {
            if (_progressionManager == null)
            {
                Debug.LogWarning("[QuestProgressionAdapter] PlayerProgressionManager is null — skill point not granted.");
                return;
            }
            _progressionManager.GrantSkillPoints(amount);
        }
    }
}
