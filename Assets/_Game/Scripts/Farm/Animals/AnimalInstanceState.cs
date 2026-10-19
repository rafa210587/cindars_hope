namespace CindarsHope.Farm.Animals
{
    public enum AnimalHealthState
    {
        Healthy = 0,
        Hungry = 1,
        Tired = 2,
        SickLight = 3,
        Unavailable = 4,
        Recovering = 5,
        // fable_12 (EMENDA 2026-06-12-D, decisão 5.1-A): morte permanente por negligência
        // prolongada (N=7 dias consecutivos sem alimentação). Estado terminal — sem reviver.
        // Valor APPENDED (não renumera os existentes) para manter saves/testes WAVE 05 estáveis.
        Dead = 6
    }

    public class AnimalInstanceState
    {
        public string AnimalInstanceId { get; set; }
        public string AnimalDataId { get; set; }
        public string Name { get; set; }
        public string HomeBuildingId { get; set; }
        public bool FedToday { get; set; }
        public int LastFedDay { get; set; } = -1;
        public int DaysWithoutFood { get; set; } = 0;
        public AnimalHealthState HealthState { get; set; } = AnimalHealthState.Healthy;
        public int CareScore { get; set; } = 0;
        public bool ProductReady { get; set; } = false;
        public int ProductQualityBias { get; set; } = 0;
        public int LastProductDay { get; set; } = -1;

        public AnimalInstanceState()
        {
        }

        public AnimalInstanceState(string animalInstanceId, string animalDataId, string name, string homeBuildingId)
        {
            AnimalInstanceId = animalInstanceId;
            AnimalDataId = animalDataId;
            Name = name;
            HomeBuildingId = homeBuildingId;
            FedToday = false;
            DaysWithoutFood = 0;
            HealthState = AnimalHealthState.Healthy;
            CareScore = 0;
            ProductReady = false;
            ProductQualityBias = 0;
        }

        public void MarkFed(int currentDay)
        {
            FedToday = true;
            LastFedDay = currentDay;
            DaysWithoutFood = 0;
            if (HealthState == AnimalHealthState.Hungry)
            {
                HealthState = AnimalHealthState.Healthy;
            }
        }

        public void MarkUnfed()
        {
            FedToday = false;
            DaysWithoutFood++;

            if (DaysWithoutFood >= 3)
            {
                HealthState = AnimalHealthState.Unavailable;
            }
            else if (DaysWithoutFood >= 1)
            {
                HealthState = AnimalHealthState.Hungry;
            }
        }

        public void ResetDailyState()
        {
            FedToday = false;
        }
    }
}
