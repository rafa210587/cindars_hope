namespace CindarsHope.Farm.Crops
{
    public class CropGrowthProcessor
    {
        public CropGrowthState Process(CropGrowthState state, CropGrowthInput input)
        {
            if (state == null || input?.Definition == null)
                return state;

            if (state.IsDead)
                return state;

            if (state.IsReadyToHarvest)
                return state;

            // Idempotency: do not process the same day twice
            if (state.LastProcessedDay == input.CurrentDay && input.CurrentDay > 0)
                return state;

            var next = state.Clone();
            next.LastProcessedDay = input.CurrentDay;

            // Season check — no growth, no death (dormant)
            if (!input.IsValidSeason)
                return next;

            // Unwatered
            if (!input.IsWateredToday && input.Definition.RequiresWater)
            {
                next.DaysWithoutWater++;
                if (next.DaysWithoutWater >= input.Definition.DiesAfterDaysWithoutWater)
                    next.IsDead = true;
                return next;
            }

            // Watered and valid season — grow
            next.DaysWithoutWater = 0;
            next.DaysGrown++;

            if (next.DaysGrown >= input.Definition.GrowthDays)
                next.IsReadyToHarvest = true;

            return next;
        }
    }
}
