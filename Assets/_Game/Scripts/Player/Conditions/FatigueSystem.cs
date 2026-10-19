namespace CindarsHope.Player.Conditions
{
    // Pure C# fatigue system — independent from HungerManager (which does NOT own fatigue)
    public class FatigueSystem
    {
        private readonly FatigueState _state;
        private const float HungerLowMultiplier = 1.5f; // low hunger increases fatigue gain
        private const float CaveMultiplier = 1.3f;      // cave is more fatiguing

        public FatigueSystem(FatigueState state = null)
        {
            _state = state ?? new FatigueState();
        }

        public FatigueState State => _state;
        public FatigueThreshold CurrentThreshold => _state.Threshold;
        public bool IsExhausted => _state.IsExhausted;

        public float AddFatigue(FatigueGainContext ctx)
        {
            if (ctx == null) return _state.FatigueValue;

            float amount = ctx.FinalAmount;

            // Apply context multipliers
            if (ctx.Source == FatigueGainSource.HungerLow)
                amount *= HungerLowMultiplier;

            if (ctx.Source == FatigueGainSource.CaveExploration)
                amount *= CaveMultiplier;

            _state.FatigueValue += amount;
            _state.Clamp();
            return _state.FatigueValue;
        }

        public float AddFatigueFromStaminaSpend(float staminaSpent, float hungerPercent = 1.0f)
        {
            float baseFatigue = staminaSpent * 0.1f; // 10% of stamina spent converts to fatigue
            if (hungerPercent < 0.2f)
                baseFatigue *= HungerLowMultiplier;

            return AddFatigue(new FatigueGainContext
            {
                Source = FatigueGainSource.StaminaSpend,
                BaseAmount = baseFatigue,
                Multiplier = 1.0f
            });
        }

        public SleepRecoveryResult ApplySleepRecovery(SleepRecoveryContext ctx)
        {
            var result = SleepRecoveryCalculator.Calculate(ctx);
            _state.FatigueValue -= result.FatigueReduction;
            _state.Clamp();
            _state.LastSleepQuality = result.SleepQuality;
            return result;
        }

        public void AddTimePassingFatigue(float hours)
        {
            // Natural fatigue: ~2 fatigue per hour awake.
            // fable_23 — ponto ÚNICO do NightFatigueModifier (Amuleto de Nyx -30% sobre o desgaste
            // noturno/awake). Consulta síncrona ao AccessoryEffectRouter; sem acessório => sem redução
            // (helper trata default 0 como neutro). Sem if espalhado em outros sistemas.
            float baseAmount = hours * 2f;
            baseAmount = CindarsHope.Equipment.AccessoryEffectRouter.ApplyNightFatigueReduction(baseAmount);

            AddFatigue(new FatigueGainContext
            {
                Source = FatigueGainSource.TimePassing,
                BaseAmount = baseAmount,
                Multiplier = 1.0f
            });
        }
    }
}
