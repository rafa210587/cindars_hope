namespace CindarsHope.Player.Conditions
{
    public class PlayerConditionSnapshot
    {
        public float HP { get; set; }
        public float MaxHP { get; set; }
        public float Stamina { get; set; }
        public float MaxStamina { get; set; }
        public float Hunger { get; set; }
        public float MaxHunger { get; set; }
        public float Fatigue { get; set; }
        public FatigueThreshold FatigueThreshold { get; set; }
        public float MP { get; set; } // future
        public float MaxMP { get; set; }
        public bool IsExhausted => FatigueThreshold.IsExhausted();
        public bool IsHungry => MaxHunger > 0 && Hunger / MaxHunger < 0.2f;
        public bool IsLowStamina => MaxStamina > 0 && Stamina / MaxStamina < 0.25f;
        public bool IsLowHP => MaxHP > 0 && HP / MaxHP < 0.25f;
    }
}
