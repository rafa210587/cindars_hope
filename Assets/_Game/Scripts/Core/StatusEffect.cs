using System;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Core
{
    [DisallowMultipleComponent]
    public class StatusEffect : MonoBehaviour
    {
        private string _effectId;
        private float _remainingSeconds;
        private Action<StatusEffect> _onExpired;

        public string EffectId => _effectId;
        public float RemainingSeconds => _remainingSeconds;
        public bool IsActive => _remainingSeconds > 0;

        public void Initialize(string effectId, float durationSeconds, Action<StatusEffect> onExpired = null)
        {
            _effectId = effectId;
            _remainingSeconds = Mathf.Max(0, durationSeconds);
            _onExpired = onExpired;
        }

        private void Update()
        {
            if (!IsActive)
                return;

            _remainingSeconds -= UnityEngine.Time.deltaTime;
            if (_remainingSeconds <= 0)
            {
                _remainingSeconds = 0;
                _onExpired?.Invoke(this);
            }
        }

        public void Refresh(float newDurationSeconds)
        {
            _remainingSeconds = Mathf.Max(0, newDurationSeconds);
        }

        public void Extend(float additionalSeconds)
        {
            _remainingSeconds += Mathf.Max(0, additionalSeconds);
        }

        public void Expire()
        {
            _remainingSeconds = 0;
            _onExpired?.Invoke(this);
        }
    }
}
