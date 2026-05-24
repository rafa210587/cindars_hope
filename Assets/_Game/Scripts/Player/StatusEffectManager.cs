using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class StatusEffectManager : MonoBehaviour
    {
        private Dictionary<string, StatusEffect> _activeEffects = new Dictionary<string, StatusEffect>();
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;
        public IReadOnlyDictionary<string, StatusEffect> ActiveEffects => _activeEffects;

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _activeEffects.Clear();
            _isInitialized = true;
        }

        public void Shutdown()
        {
            foreach (var effect in _activeEffects.Values)
            {
                if (effect != null)
                    Destroy(effect.gameObject);
            }
            _activeEffects.Clear();
            _isInitialized = false;
        }

        public bool TryAddEffect(string effectId, float durationSeconds)
        {
            if (string.IsNullOrWhiteSpace(effectId))
                return false;

            if (_activeEffects.ContainsKey(effectId))
            {
                _activeEffects[effectId].Refresh(durationSeconds);
                return true;
            }

            var effectGO = new GameObject($"StatusEffect_{effectId}");
            effectGO.transform.SetParent(transform);
            var effect = effectGO.AddComponent<StatusEffect>();
            effect.Initialize(effectId, durationSeconds, OnEffectExpired);

            _activeEffects[effectId] = effect;
            return true;
        }

        public bool TryRemoveEffect(string effectId)
        {
            if (!_activeEffects.TryGetValue(effectId, out var effect))
                return false;

            effect.Expire();
            return true;
        }

        public bool HasEffect(string effectId)
        {
            return _activeEffects.ContainsKey(effectId) && _activeEffects[effectId].IsActive;
        }

        public float GetEffectRemainingSeconds(string effectId)
        {
            if (!_activeEffects.TryGetValue(effectId, out var effect))
                return 0;

            return effect.RemainingSeconds;
        }

        private void OnEffectExpired(StatusEffect effect)
        {
            if (effect != null && _activeEffects.ContainsValue(effect))
            {
                string key = null;
                foreach (var kvp in _activeEffects)
                {
                    if (kvp.Value == effect)
                    {
                        key = kvp.Key;
                        break;
                    }
                }

                if (key != null)
                {
                    _activeEffects.Remove(key);
                    Destroy(effect.gameObject);
                }
            }
        }
    }
}
