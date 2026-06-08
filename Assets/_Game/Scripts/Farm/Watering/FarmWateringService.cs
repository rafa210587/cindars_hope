using System.Collections.Generic;

namespace CindarsHope.Farm.Watering
{
    public class FarmWateringService
    {
        public void ApplyManualWatering(FarmPlotWaterState state, int dayNumber)
        {
            if (state == null) return;
            state.MarkWatered(WaterSource.Manual, dayNumber);
        }

        public void ApplyRainWatering(IEnumerable<FarmPlotWaterState> plots, int dayNumber, bool isStorm = false)
        {
            if (plots == null) return;

            var source = isStorm ? WaterSource.Storm : WaterSource.Rain;
            foreach (var state in plots)
            {
                if (state == null) continue;
                // Rain/Storm only waters external plots — never greenhouse or interior
                if (!state.IsExternal || state.IsGreenhouse || state.IsInterior) continue;
                state.MarkWatered(source, dayNumber);
            }
        }

        public void ApplyIrrigationCoverage(IEnumerable<FarmPlotWaterState> plots, string coverageId, int dayNumber)
        {
            if (plots == null || string.IsNullOrEmpty(coverageId)) return;

            foreach (var state in plots)
            {
                if (state == null) continue;
                if (state.IrrigationCoverageId != coverageId) continue;
                if (state.IsGreenhouse) continue;
                state.MarkWatered(WaterSource.IrrigationSimple, dayNumber);
            }
        }

        public void ResetDayWaterStates(IEnumerable<FarmPlotWaterState> plots)
        {
            if (plots == null) return;
            foreach (var state in plots)
                state?.ResetDayWaterState();
        }
    }
}
