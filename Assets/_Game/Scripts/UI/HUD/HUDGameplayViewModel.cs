namespace CindarsHope.UI.HUD
{
    /// <summary>
    /// SPEC 04: Main gameplay HUD projection for health, mana, stamina, day/weather.
    /// </summary>
    public class HUDGameplayViewModel
    {
        // Character state
        public int HealthCurrent { get; set; }
        public int HealthMax { get; set; }
        public int ManaCurrent { get; set; }
        public int ManaMax { get; set; }
        public int StaminaCurrent { get; set; }
        public int StaminaMax { get; set; }

        // World state
        public string CurrentSeason { get; set; }
        public int DayOfSeason { get; set; }
        public string CurrentWeather { get; set; }
        public string CurrentLunarPhase { get; set; }

        // Computed
        public float HealthPercent => HealthMax > 0 ? (float)HealthCurrent / HealthMax : 1f;
        public float ManaPercent => ManaMax > 0 ? (float)ManaCurrent / ManaMax : 1f;
        public float StaminaPercent => StaminaMax > 0 ? (float)StaminaCurrent / StaminaMax : 1f;

        public bool IsHealthLow => HealthPercent < 0.25f;
        public bool IsManaLow => ManaPercent < 0.25f;
        public bool IsStaminaLow => StaminaPercent < 0.25f;
    }
}
