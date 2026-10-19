using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class StatusEffectManager : MonoBehaviour, CindarsHope.Foundation.IStatusEffectRuntime
    {
        // arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) —
        // self-registro estatico (molde Craft/Economy/Skills/Equipment) para o GameBootstrap parar
        // de segurar esta referencia serializada. Nao proibido pelo ratchet GlobalGoldAccess (que so
        // cobre PlayerManager/GoldManager/EconomyManager). Registrado tambem sob a porta
        // IStatusEffectRuntime (Foundation, 2026-07-15) para GameBootstrap chamar Initialize/Shutdown
        // sem nomear CindarsHope.Player.
        public static StatusEffectManager Instance { get; private set; }

        private const string PlayerTargetId = "player";

        private Dictionary<string, StatusEffect> _activeEffects = new Dictionary<string, StatusEffect>();
        private readonly HashSet<string> _removedEffects = new HashSet<string>();
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;
        public IReadOnlyDictionary<string, StatusEffect> ActiveEffects => _activeEffects;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CindarsHope.Foundation.DomainManagerRegistry.Register<CindarsHope.Foundation.IStatusEffectRuntime>(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            CindarsHope.Foundation.DomainManagerRegistry.Unregister<CindarsHope.Foundation.IStatusEffectRuntime>(this);
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _activeEffects.Clear();
            _removedEffects.Clear();
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
            _removedEffects.Clear();
            _isInitialized = false;
        }

        public bool TryAddEffect(string effectId, float durationSeconds)
        {
            if (string.IsNullOrWhiteSpace(effectId))
                return false;

            if (_activeEffects.ContainsKey(effectId))
            {
                _activeEffects[effectId].Refresh(durationSeconds);
                GameEventBus.Publish(new StatusRefreshedEvent(PlayerTargetId, effectId, durationSeconds));
                return true;
            }

            var effectGO = new GameObject($"StatusEffect_{effectId}");
            effectGO.transform.SetParent(transform);
            var effect = effectGO.AddComponent<StatusEffect>();
            effect.Initialize(effectId, durationSeconds, OnEffectExpired);

            _activeEffects[effectId] = effect;
            GameEventBus.Publish(new StatusAppliedEvent(PlayerTargetId, effectId, string.Empty, durationSeconds));
            return true;
        }

        public bool TryRemoveEffect(string effectId)
        {
            if (!_activeEffects.TryGetValue(effectId, out var effect))
                return false;

            _removedEffects.Add(effectId);
            GameEventBus.Publish(new StatusRemovedEvent(PlayerTargetId, effectId));
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
                    if (!_removedEffects.Remove(key))
                    {
                        GameEventBus.Publish(new StatusExpiredEvent(PlayerTargetId, key));
                    }

                    Destroy(effect.gameObject);
                }
            }
        }
    }
}
