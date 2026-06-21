using CindarsHope.Core.Bootstrap;
using UnityEngine;

namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// fable_65 — sink de produção. Resolve os canais ÚNICOS existentes do player via
    /// GameBootstrap.Instance (sem GameObject.Find / FindObjectOfType — wiring idiom do projeto):
    /// ouro por PlayerManager.AddGold; XP por PlayerProgressionManager.AddXp (curva F42 intacta).
    /// Não cria nenhum canal paralelo de economia/progressão.
    /// </summary>
    public sealed class GameBootstrapDailyGoalRewardSink : IDailyGoalRewardSink
    {
        public void Grant(int gold, int xp)
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogWarning("[GameBootstrapDailyGoalRewardSink] GameBootstrap.Instance ausente — recompensa de meta não aplicada (gold=" + gold + ", xp=" + xp + ").");
                return;
            }

            if (gold > 0)
            {
                if (bootstrap.PlayerManager != null)
                {
                    bootstrap.PlayerManager.AddGold(gold);
                }
                else
                {
                    Debug.LogWarning("[GameBootstrapDailyGoalRewardSink] PlayerManager ausente — ouro de meta não aplicado (gold=" + gold + ").");
                }
            }

            if (xp > 0)
            {
                if (bootstrap.PlayerProgressionManager != null)
                {
                    bootstrap.PlayerProgressionManager.AddXp(xp);
                }
                else
                {
                    Debug.LogWarning("[GameBootstrapDailyGoalRewardSink] PlayerProgressionManager ausente — XP de meta não aplicado (xp=" + xp + ").");
                }
            }
        }
    }
}
