using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.World.Calendar
{
    public class FestivalRegistry : MonoBehaviour
    {
        public struct Festival
        {
            public FestivalType Type;
            public int DayInYear; // 1-112
            public string DisplayName;
            public bool IsHidden; // True = not revealed until discovered
        }

        [SerializeField] private List<Festival> _festivals = new List<Festival>();
        private HashSet<string> _discoveredFestivals = new HashSet<string>();

        private void OnEnable()
        {
            InitializeDefaultFestivals();
        }

        private void InitializeDefaultFestivals()
        {
            if (_festivals.Count == 0)
            {
                _festivals.Add(new Festival { Type = FestivalType.PlantingFestival, DayInYear = 14, DisplayName = "Planting Festival", IsHidden = false });
                _festivals.Add(new Festival { Type = FestivalType.HarvestFestival, DayInYear = 98, DisplayName = "Harvest Festival", IsHidden = false });
                _festivals.Add(new Festival { Type = FestivalType.MarketFestival, DayInYear = 56, DisplayName = "Market Festival", IsHidden = false });
            }
        }

        public Festival GetFestivalOnDay(GameDate date)
        {
            foreach (var festival in _festivals)
            {
                if (festival.DayInYear == date.DayInSeason + ((int)date.CurrentSeason * 28))
                {
                    if (!festival.IsHidden || _discoveredFestivals.Contains(festival.DisplayName))
                    {
                        return festival;
                    }
                }
            }

            return new Festival { Type = FestivalType.None };
        }

        public void DiscoverFestival(string festivalName)
        {
            _discoveredFestivals.Add(festivalName);
        }

        public bool IsFestivalToday(GameDate today)
        {
            return GetFestivalOnDay(today).Type != FestivalType.None;
        }
    }
}
