using System;

namespace CindarsHope.World.Events
{
    /// <summary>
    /// fable_37 — ponto ÚNICO nomeado por sistema-alvo (padrão F23 / AccessoryEffectRouter). O
    /// WorldEventService publica a resolução do dia em <see cref="Active"/>; cada sistema-alvo (cave
    /// spawn, farm, economia, fonte) lê AQUI num único ponto nomeado — nunca consulta o serviço por
    /// conta própria em vários lugares, e o sistema-alvo NÃO conhece o WorldEventService. Sem evento
    /// ativo (teste puro / cena sem serviço) ⇒ todos os hooks são NEUTROS (multiplicador 1.0, extra 0).
    ///
    /// Determinístico: <see cref="Active"/> é sempre fruto de WorldEventResolver.ResolveDay (recomputável).
    /// </summary>
    public static class WorldEventHooks
    {
        /// <summary>Resolução do dia corrente, definida pelo dono (WorldEventService) a cada DayStartedEvent.
        /// Null fora de runtime ou antes do primeiro dia ⇒ hooks neutros.</summary>
        public static WorldDayResolution Active { get; set; }

        /// <summary>Fonte do estado da quest de desbloqueio da loja noturna (decisão v2 6.2-B). O dono
        /// injeta um predicado que consulta o QuestFlagService; default false ⇒ loja noturna fechada.</summary>
        public static Func<bool> NightShopQuestUnlockedSource = () => false;

        // ─── Cave spawn (Cinza +undead / infestação / -spawn / dia calmo) ─────────────────────────
        /// <summary>
        /// Multiplicador determinístico aplicado ao alvo de inimigos da caverna num único ponto
        /// (CaveEnemySpawnPlanner.ResolveTargetEnemyCount). Combina pico de Cinza (+30% — interpretado
        /// como densidade undead aproximada nessa noite), infestação (+20%) e dia nublado calmo (−15%).
        /// Neutro = 1.0. Clamp em [0.5, 2.0] para nunca zerar nem explodir a densidade.
        /// </summary>
        public static float GetCaveSpawnMultiplier()
        {
            var resolution = Active;
            if (resolution == null) return 1f;

            var multiplier = 1f;

            if (resolution.LunarEffect == WorldEventDefinitions.LunarPeakEffect.AshUndeadSpawn)
            {
                multiplier += resolution.LunarMagnitude; // +0.30
            }

            switch (resolution.WorldEffect)
            {
                case WorldEventDefinitions.WorldEventEffect.Infestation:
                    multiplier += resolution.WorldEventMagnitude; // +0.20
                    break;
                case WorldEventDefinitions.WorldEventEffect.CalmCloudyDay:
                    multiplier -= resolution.WorldEventMagnitude; // −0.15
                    break;
            }

            if (multiplier < 0.5f) multiplier = 0.5f;
            if (multiplier > 2f) multiplier = 2f;
            return multiplier;
        }

        // ─── Economia (Âmbar +10% venda / cultivo em alta +25% / dia de feira stock ×2) ───────────
        /// <summary>
        /// Multiplicador de OURO em vendas aplicado num único ponto (EconomyManager.HandleSellAllRequested).
        /// Pico de Âmbar (+10%) e procura em alta de cultivo (+25%) somam-se aqui. Neutro = 1.0.
        /// </summary>
        public static float GetSellGoldMultiplier()
        {
            var resolution = Active;
            if (resolution == null) return 1f;

            var multiplier = 1f;
            if (resolution.LunarEffect == WorldEventDefinitions.LunarPeakEffect.AmberSellPrices)
            {
                multiplier += resolution.LunarMagnitude; // +0.10
            }

            if (resolution.WorldEffect == WorldEventDefinitions.WorldEventEffect.CropPriceSurge)
            {
                multiplier += resolution.WorldEventMagnitude; // +0.25
            }

            return multiplier;
        }

        /// <summary>Aplica o multiplicador de venda a um valor base de ouro (ponto único, sem if espalhado).
        /// Ex.: 100 ouro em Lua Âmbar ⇒ 110. Sem evento ⇒ valor base.</summary>
        public static int ApplySellGold(int baseGold)
        {
            if (baseGold <= 0) return baseGold;
            var mult = GetSellGoldMultiplier();
            if (mult <= 1f) return baseGold;
            return (int)Math.Round(baseGold * mult);
        }

        /// <summary>Fator de stock das lojas no dia de feira (×2). Neutro = 1.</summary>
        public static int GetShopStockFactor()
        {
            var resolution = Active;
            if (resolution != null
                && resolution.WorldEffect == WorldEventDefinitions.WorldEventEffect.MarketFairDay)
            {
                var factor = (int)Math.Round(resolution.WorldEventMagnitude);
                return factor < 1 ? 2 : factor + 1; // magnitude 1 ⇒ ×2
            }

            return 1;
        }

        // ─── Loot (chuva de estrelas +5% loot raro) ──────────────────────────────────────────────
        /// <summary>Chance (0..1) de 1 roll extra de loot raro nessa data (chuva de estrelas). Neutro = 0.</summary>
        public static float GetExtraRareLootChance()
        {
            var resolution = Active;
            if (resolution != null
                && resolution.WorldEffect == WorldEventDefinitions.WorldEventEffect.StarShowerLuck)
            {
                return resolution.WorldEventMagnitude; // 0.05
            }

            return 0f;
        }

        // ─── Fonte de Anya (Pálida: Água Viva extra — F17) ───────────────────────────────────────
        /// <summary>Unidades EXTRAS de Água Viva concedidas pela Fonte nessa noite (pico de Lua Pálida).
        /// Lido num único ponto pelo sistema da Fonte. Neutro = 0.</summary>
        public static int GetExtraAguaViva()
        {
            var resolution = Active;
            if (resolution != null
                && resolution.LunarEffect == WorldEventDefinitions.LunarPeakEffect.PaleAguaViva)
            {
                var extra = (int)Math.Round(resolution.LunarMagnitude);
                return extra < 0 ? 0 : extra; // magnitude 1 ⇒ +1
            }

            return 0;
        }

        // ─── Farm (Lua Verde: crops +1 estágio) ──────────────────────────────────────────────────
        /// <summary>True quando hoje é pico de Lua Verde (crops avançam +1 estágio via hook do farm).</summary>
        public static bool IsGreenMoonGrowthDay()
        {
            var resolution = Active;
            return resolution != null
                && resolution.LunarEffect == WorldEventDefinitions.LunarPeakEffect.GreenCropGrowth;
        }

        /// <summary>Quantos estágios extras a Lua Verde concede (magnitude do pico). Neutro = 0.</summary>
        public static int GetGreenMoonExtraStages()
        {
            var resolution = Active;
            if (IsGreenMoonGrowthDay())
            {
                var stages = (int)Math.Round(resolution.LunarMagnitude);
                return stages < 0 ? 0 : stages;
            }

            return 0;
        }

        // ─── Clima (EMENDA seca: a chuva pode falhar) ────────────────────────────────────────────
        /// <summary>True quando o evento de seca está ativo hoje (a chuva pode falhar — integra com F15).
        /// O dono do clima (F15/RainIrrigation) lê AQUI num único ponto; sem evento ⇒ false.</summary>
        public static bool IsDroughtToday()
        {
            var resolution = Active;
            return resolution != null
                && resolution.WorldEffect == WorldEventDefinitions.WorldEventEffect.Drought;
        }

        // ─── Loja noturna (decisão v2 6.2-B: só em PICO DE NYX + quest) ───────────────────────────
        /// <summary>
        /// Gate ÚNICO da loja noturna: abre SOMENTE se hoje é pico de Nyx (peak_nyx via efeito de
        /// AshUndeadSpawn ancorado na lua oculta D7 — ver nota abaixo) E a quest de desbloqueio está
        /// concluída. Determinístico e puro quanto ao pico; o estado da quest vem do
        /// <see cref="NightShopQuestUnlockedSource"/>.
        ///
        /// Nota de mapeamento (Fase 0): no modelo nomeado, Nyx é a lua oculta da noite/segredos. O pico
        /// de Nyx é representado por "peak_ash" (a noite de pico de spawn undead, D7) — a mesma noite que
        /// a direção associa à lua escura. Se uma futura spec separar Nyx de Cinza, troque o id aqui.
        /// </summary>
        public static bool IsNightShopOpen()
        {
            return IsNyxPeakToday() && (NightShopQuestUnlockedSource?.Invoke() ?? false);
        }

        /// <summary>True quando hoje é o pico de Nyx (loja noturna). Puro/determinístico.</summary>
        public static bool IsNyxPeakToday()
        {
            var resolution = Active;
            return resolution != null && resolution.LunarPeakId == "peak_ash";
        }

        /// <summary>Reseta os hooks para o estado neutro (uso: testes / shutdown do serviço).</summary>
        public static void Reset()
        {
            Active = null;
            NightShopQuestUnlockedSource = () => false;
        }
    }
}
