using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// Thin adapter wrapping PlayerManager.AddGold for QuestService reward application.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    /// </summary>
    public class QuestGoldAdapter : IQuestGoldAccess
    {
        private readonly PlayerManager _playerManager;

        public QuestGoldAdapter(PlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        public void AddGold(int amount)
        {
            if (_playerManager == null)
            {
                Debug.LogWarning("[QuestGoldAdapter] PlayerManager is null — gold reward not applied.");
                return;
            }
            _playerManager.AddGold(amount);
        }
    }
}
