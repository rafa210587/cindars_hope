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
            => TryRespec(state, playerLevel, ref gold, true);

        internal bool TryRespec(SkillTreeState state, int playerLevel, ref int gold, bool publishEvents)
        {
            if (publishEvents) GameEventBus.Publish(new SkillTreeRespecRequestedEvent());

            int restoredPoints = checked(state.AvailableSkillPoints + state.SpentSkillPoints);

            if (state.RespecCount >= FreeRespecCount)
            {
                if (gold < _respecCostGold)
                {
                    if (publishEvents) GameEventBus.Publish(new SkillTreeRespecFailedEvent($"Not enough gold. Need {_respecCostGold}."));
                    return false;
                }
                gold -= _respecCostGold;
            }

            state.FullRespec(restoredPoints);

            if (publishEvents)
            {
                GameEventBus.Publish(new SkillTreeRespecCompletedEvent(restoredPoints, state.RespecCount));
                GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
            }
            return true;
        }

        public int GetRespecCost(SkillTreeState state)
        {
            return state.RespecCount < FreeRespecCount ? 0 : _respecCostGold;
        }
    }
}
