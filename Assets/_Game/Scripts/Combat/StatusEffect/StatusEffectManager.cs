using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Combat.StatusEffect
{
    public class StatusEffectManager
    {
        private List<ActiveStatusEffect> _activeEffects = new List<ActiveStatusEffect>();

        public void ApplyStatusEffect(StatusEffectSO statusEffect)
        {
            if (statusEffect == null)
                return;

            var active = new ActiveStatusEffect(statusEffect.Id, statusEffect.DurationTurns);
            _activeEffects.Add(active);
        }

        public void RemoveStatusEffect(string statusEffectId)
        {
            _activeEffects.RemoveAll(e => e.StatusEffectId == statusEffectId);
        }

        public void RemoveAllStatusEffects()
        {
            _activeEffects.Clear();
        }

        public void TickStatusEffects()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                _activeEffects[i].Tick();
                if (!_activeEffects[i].IsActive)
                {
                    _activeEffects.RemoveAt(i);
                }
            }
        }

        public int GetStatusDamageThisTurn(StatusEffectSO statusEffect)
        {
            if (statusEffect == null || !HasStatusEffect(statusEffect.Id))
                return 0;

            return statusEffect.DamagePerTurn;
        }

        public bool HasStatusEffect(string statusEffectId)
        {
            return _activeEffects.Find(e => e.StatusEffectId == statusEffectId) != null;
        }

        public int GetTotalStatusDamage(StatusEffectSO[] allStatusEffects)
        {
            int totalDamage = 0;
            foreach (var effect in _activeEffects)
            {
                foreach (var statusEffect in allStatusEffects)
                {
                    if (statusEffect != null && statusEffect.Id == effect.StatusEffectId)
                    {
                        totalDamage += statusEffect.DamagePerTurn;
                        break;
                    }
                }
            }
            return totalDamage;
        }

        public List<ActiveStatusEffect> GetActiveEffects() => new List<ActiveStatusEffect>(_activeEffects);
    }
}
