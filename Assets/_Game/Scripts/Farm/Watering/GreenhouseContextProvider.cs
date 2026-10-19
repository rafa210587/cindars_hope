using System.Collections.Generic;

namespace CindarsHope.Farm.Watering
{
    public class GreenhouseContextProvider
    {
        private readonly HashSet<string> _registeredGreenhousePlotIds = new HashSet<string>();
        private bool _greenhouseUnlocked;

        public void RegisterGreenhousePlot(string plotId)
        {
            if (!string.IsNullOrEmpty(plotId))
                _registeredGreenhousePlotIds.Add(plotId);
        }

        public void SetGreenhouseUnlocked(bool unlocked)
        {
            _greenhouseUnlocked = unlocked;
        }

        public bool IsGreenhousePlot(string plotId)
        {
            return !string.IsNullOrEmpty(plotId) && _registeredGreenhousePlotIds.Contains(plotId);
        }

        public bool CanOverrideSeason(string plotId)
        {
            return _greenhouseUnlocked && IsGreenhousePlot(plotId);
        }

        public bool IsRainExcluded(string plotId)
        {
            return IsGreenhousePlot(plotId);
        }
    }
}
