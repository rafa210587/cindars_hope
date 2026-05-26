using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player.Progression;

namespace CindarsHope.Skills
{
    public class SkillRespecService
    {
        private const int FreeRespecCount = 1;
        private const int DefaultRespecCostGold = 250;

        private readonly int _respecCostGold;

        public SkillRespecService(int respecCostGold = DefaultRespecCostGold)
        {
            _respecCostGold = respecCostGold;
        }

        public bool TryRespec(SkillTreeState state, int playerLevel, ref int gold)
        {
            GameEventBus.Publish(new SkillTreeRespecRequestedEvent());

            if (state.RespecCount >= FreeRespecCount)
            {
                if (gold < _respecCostGold)
                {
                    GameEventBus.Publish(new SkillTreeRespecFailedEvent($"Not enough gold. Need {_respecCostGold}."));
                    return false;
                }
                gold -= _respecCostGold;
            }

            int totalPointsForLevel = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(playerLevel);
            state.FullRespec(totalPointsForLevel);

            GameEventBus.Publish(new SkillTreeRespecCompletedEvent(totalPointsForLevel, state.RespecCount));
            GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
            return true;
        }

        public int GetRespecCost(SkillTreeState state)
        {
            return state.RespecCount < FreeRespecCount ? 0 : _respecCostGold;
        }
    }
}
