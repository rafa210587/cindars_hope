using System.Collections.Generic;

namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// Tabela única e imutável de recompensas por meta diária de farm.
    /// {goalId -> goldReward, xpReward}. Tipos simples — sem referências Unity.
    ///
    /// Ponto único de verdade da economia de metas (fable_65): nenhum handler espalha
    /// valores de recompensa; tudo lê desta tabela. Toda meta do catálogo TEM uma entrada
    /// (garantido por teste de integridade).
    ///
    /// Valores v1 conservadores (DAILY_GOAL_REWARD_DEFERRED do WI-24 fechado aqui):
    /// ouro pequeno por meta (10-25) + XP coerente com a curva canônica F42 (5-15).
    /// Total diário documentado no execution report como tuning humano pendente.
    /// </summary>
    public static class FarmDailyGoalRewardTable
    {
        /// <summary>Recompensa imutável de uma meta: ouro + XP.</summary>
        public readonly struct Reward
        {
            public readonly int Gold;
            public readonly int Xp;

            public Reward(int gold, int xp)
            {
                Gold = gold;
                Xp = xp;
            }
        }

        // Tabela v1 conservadora. Ids estáveis — NUNCA renomear (ver id-stability).
        private static readonly Dictionary<string, Reward> Rewards = new Dictionary<string, Reward>
        {
            // Metas originais (WI-24)
            { "daily_goal_first_harvest", new Reward(15, 10) },
            { "daily_goal_sell_first_crop", new Reward(20, 10) },
            // Metas novas (fable_65)
            { "daily_goal_harvest_three", new Reward(20, 12) },
            { "daily_goal_plant_three", new Reward(10, 8) },
            { "daily_goal_gather_resource", new Reward(15, 8) },
            { "daily_goal_talk_to_npc", new Reward(10, 5) },
        };

        /// <summary>
        /// Recompensa de uma meta. Retorna false (sem mutação) se a meta não tem entrada —
        /// nunca paga uma recompensa inventada para um id desconhecido.
        /// </summary>
        public static bool TryGet(string goalId, out Reward reward)
        {
            if (string.IsNullOrWhiteSpace(goalId))
            {
                reward = default;
                return false;
            }

            return Rewards.TryGetValue(goalId, out reward);
        }

        /// <summary>True se o id existe na tabela.</summary>
        public static bool Has(string goalId)
        {
            return !string.IsNullOrWhiteSpace(goalId) && Rewards.ContainsKey(goalId);
        }

        /// <summary>Todos os ids com recompensa (para teste de integridade do catálogo).</summary>
        public static IReadOnlyCollection<string> AllGoalIds => Rewards.Keys;

        /// <summary>Soma do ouro de todas as metas — total diário máximo (economia contida — CA-5).</summary>
        public static int MaxDailyGold
        {
            get
            {
                int total = 0;
                foreach (var r in Rewards.Values)
                {
                    total += r.Gold;
                }
                return total;
            }
        }

        /// <summary>Soma do XP de todas as metas — total diário máximo de XP.</summary>
        public static int MaxDailyXp
        {
            get
            {
                int total = 0;
                foreach (var r in Rewards.Values)
                {
                    total += r.Xp;
                }
                return total;
            }
        }
    }
}
