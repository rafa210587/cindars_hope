using System.Collections.Generic;

namespace CindarsHope.Farm.Animals
{
    /// <summary>
    /// fable_12 — DTO de save da seção de animais de fazenda. Seção ADITIVA padrão (sem migration):
    /// save legado (sem o campo) carrega com lista vazia = zero animais. Apenas tipos simples e IDs
    /// estáveis (ADR-0006 / rule save-dto-simple-types-only) — SEM referências Unity.
    /// </summary>
    [System.Serializable]
    public class FarmAnimalsSaveData
    {
        public List<FarmAnimalRecord> Animals = new List<FarmAnimalRecord>();
    }

    /// <summary>
    /// fable_12 — estado persistido de um animal. Inclui morte permanente (EMENDA 5.1-A) via
    /// HealthState=Dead + DaysWithoutFood. ConsecutiveFedDays alimenta a qualidade do produto (§18).
    /// </summary>
    [System.Serializable]
    public class FarmAnimalRecord
    {
        public string AnimalInstanceId;
        public string AnimalDataId;
        public string HousingId;
        public bool FedToday;
        public bool ProducedToday;
        public int DaysOwned;
        public int DaysWithoutFood;
        public int ConsecutiveFedDays;
        public int HealthState;
        public int LastProductDay = -1;
    }
}
