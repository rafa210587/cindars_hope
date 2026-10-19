using UnityEngine;

namespace CindarsHope.Core.Data
{
    [CreateAssetMenu(fileName = "GameTimeBalance", menuName = "CindarsHope/Data/GameTimeBalance")]
    public class GameTimeBalanceSO : ScriptableObject
    {
        [Header("Day/Night Durations (in minutes, real-world time)")]
        [SerializeField] private float _dayDurationMinutes = 20f;
        [SerializeField] private float _nightDurationMinutes = 10f;

        public float DayDurationMinutes => _dayDurationMinutes;
        public float NightDurationMinutes => _nightDurationMinutes;

        public float DayDurationSeconds => _dayDurationMinutes * 60f;
        public float NightDurationSeconds => _nightDurationMinutes * 60f;
        public float FullCycleDurationSeconds => DayDurationSeconds + NightDurationSeconds;
    }
}
