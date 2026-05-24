using UnityEngine;

namespace CindarsHope.Combat
{
    public class StatusEffectInstance
    {
        public StatusEffectSO EffectData { get; }
        public string TargetId { get; }
        public string SourceId { get; }
        public float RemainingDuration { get; set; }
        public float TickProgress { get; set; }
        public int Power { get; }

        public StatusEffectInstance(
            StatusEffectSO effectData,
            string targetId,
            string sourceId = "")
        {
            EffectData = effectData;
            TargetId = targetId ?? string.Empty;
            SourceId = sourceId ?? string.Empty;
            RemainingDuration = effectData?.DurationSeconds ?? 0f;
            TickProgress = 0f;
            Power = effectData?.Power ?? 0;
        }

        public bool IsExpired => RemainingDuration <= 0;

        public bool ShouldTick
        {
            get
            {
                if (EffectData == null)
                    return false;

                return TickProgress >= EffectData.TickIntervalSeconds;
            }
        }

        public void ResetTickProgress()
        {
            TickProgress = 0f;
        }

        public void Refresh(StatusEffectSO newEffectData)
        {
            if (newEffectData == null)
                return;

            RemainingDuration = newEffectData.DurationSeconds;
            ResetTickProgress();
        }
    }
}
