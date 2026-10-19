using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    // WAVE_INTEGRATION_11: Central registry that maps EffectId to ISkillEffectExecutor.
    // Executors register themselves or are registered by the controller.
    // This registry is the single point of extension for new skill effects.
    public sealed class SkillEffectRegistry
    {
        private readonly Dictionary<string, ISkillEffectExecutor> _executors
            = new Dictionary<string, ISkillEffectExecutor>(System.StringComparer.Ordinal);

        // Register an executor. Only one executor per EffectId is allowed.
        public void Register(ISkillEffectExecutor executor)
        {
            if (executor == null)
            {
                Debug.LogWarning("[SkillEffectRegistry] Attempted to register null executor.");
                return;
            }

            if (string.IsNullOrEmpty(executor.EffectId))
            {
                Debug.LogWarning("[SkillEffectRegistry] Executor has empty EffectId; skipping.");
                return;
            }

            if (_executors.ContainsKey(executor.EffectId))
            {
                Debug.LogWarning($"[SkillEffectRegistry] Executor already registered for EffectId '{executor.EffectId}'. Overwriting.");
            }

            _executors[executor.EffectId] = executor;
        }

        // Resolve executor by EffectId. Returns null if not found.
        public ISkillEffectExecutor Resolve(string effectId)
        {
            if (string.IsNullOrEmpty(effectId))
                return null;

            _executors.TryGetValue(effectId, out var executor);
            return executor;
        }

        // Returns true if an executor is registered for the given EffectId.
        public bool HasExecutor(string effectId)
        {
            return !string.IsNullOrEmpty(effectId) && _executors.ContainsKey(effectId);
        }

        // Returns all registered EffectIds (for debug/editor tools).
        public IReadOnlyCollection<string> RegisteredEffectIds => _executors.Keys;
    }
}
