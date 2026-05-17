using System.Collections.Generic;
using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.Farm
{
    [DisallowMultipleComponent]
    public class FarmPlotRegistry : MonoBehaviour
    {
        [SerializeField] private FarmPlot[] _plots;

        public IReadOnlyList<FarmPlot> Plots => _plots;

        public void Configure(FarmPlot[] plots)
        {
            _plots = plots ?? new FarmPlot[0];
        }

        public FarmSaveData CaptureSaveData()
        {
            var saveData = new FarmSaveData();
            if (_plots == null)
            {
                return saveData;
            }

            foreach (var plot in _plots)
            {
                if (plot == null)
                {
                    continue;
                }

                saveData.Plots.Add(plot.CaptureSaveData());
            }

            return saveData;
        }

        public void RestoreFromSaveData(FarmSaveData saveData)
        {
            if (saveData == null || saveData.Plots == null || _plots == null)
            {
                return;
            }

            foreach (var plotData in saveData.Plots)
            {
                if (plotData == null)
                {
                    continue;
                }

                var plot = GetPlotByIndex(plotData.PlotIndex);
                if (plot == null)
                {
                    Debug.LogWarning($"FarmPlotRegistry skipped saved plot index {plotData.PlotIndex} because no matching plot exists.", this);
                    continue;
                }

                plot.RestoreFromSaveData(plotData);
            }
        }

        private FarmPlot GetPlotByIndex(int plotIndex)
        {
            if (_plots == null)
            {
                return null;
            }

            foreach (var plot in _plots)
            {
                if (plot == null)
                {
                    continue;
                }

                var data = plot.CaptureSaveData();
                if (data.PlotIndex == plotIndex)
                {
                    return plot;
                }
            }

            return null;
        }
    }
}
