namespace CindarsHope.Equipment
{
    public class EnvironmentalResistanceManager
    {
        public int HeatResistance { get; set; }
        public int ColdResistance { get; set; }

        public EnvironmentalResistanceManager(int heat = 0, int cold = 0)
        {
            HeatResistance = heat;
            ColdResistance = cold;
        }

        public int GetEnvironmentalDamage(EnvironmentalType envType, int baseDamage)
        {
            int resistance = envType == EnvironmentalType.Heat ? HeatResistance : ColdResistance;
            int reducedDamage = UnityEngine.Mathf.Max(0, baseDamage - resistance);
            return reducedDamage;
        }

        public void AddResistance(EnvironmentalType envType, int amount)
        {
            if (envType == EnvironmentalType.Heat)
                HeatResistance += amount;
            else
                ColdResistance += amount;
        }
    }

    public enum EnvironmentalType
    {
        None,
        Heat,
        Cold
    }
}
