using UnityEngine;
using CindarsHope.Core;
using CindarsHope.Core.Events;

namespace CindarsHope.Skills.Runtime.Effects
{
    [DisallowMultipleComponent]
    public sealed class MarkedPreyState : MonoBehaviour, CindarsHope.Foundation.IRangedDamageModifierRuntime
    {
        private int _casterRuntimeId;
        private float _bonusFraction;
        private float _expiresAt;

        public bool IsRevealed => Time.time < _expiresAt;
        public float ExpiresAt => _expiresAt;

        private void OnEnable()
            => GameEventBus.Subscribe<SkillTreeRespecCompletedEvent>(OnRespec);

        private void OnDisable()
            => GameEventBus.Unsubscribe<SkillTreeRespecCompletedEvent>(OnRespec);

        public void Apply(int casterRuntimeId, float durationSeconds, float bonusFraction)
        {
            _casterRuntimeId = casterRuntimeId;
            _bonusFraction = Mathf.Max(0f, bonusFraction);
            _expiresAt = Time.time + Mathf.Max(0f, durationSeconds);
        }

        public void Clear()
        {
            _casterRuntimeId = 0;
            _bonusFraction = 0f;
            _expiresAt = 0f;
        }

        public float ResolveDamageMultiplier(int casterRuntimeId)
        {
            return IsRevealed && casterRuntimeId != 0 && casterRuntimeId == _casterRuntimeId
                ? 1f + _bonusFraction
                : 1f;
        }

        public float ResolveRangedDamageMultiplier(int casterRuntimeId)
            => ResolveDamageMultiplier(casterRuntimeId);

        private void OnRespec(SkillTreeRespecCompletedEvent evt) => Clear();
    }
}
