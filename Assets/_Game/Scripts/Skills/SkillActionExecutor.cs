using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Skills
{
    [DisallowMultipleComponent]
    public class SkillActionExecutor : MonoBehaviour
    {
        [SerializeField] private ManaManager _manaManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private SkillActionDatabaseSO _skillDatabase;

        public bool TryExecuteSkill(string skillActionId)
        {
            if (_skillDatabase == null || !_skillDatabase.TryGetById(skillActionId, out var skill))
            {
                Debug.LogWarning($"SkillActionExecutor: Skill not found: {skillActionId}", this);
                return false;
            }

            if (skill.StaminaCost > 0 && _staminaManager != null)
            {
                if (!_staminaManager.TrySpendStamina(Mathf.RoundToInt(skill.StaminaCost)))
                {
                    GameEventBus.Publish(new SkillActionExecutedEvent(skillActionId));
                    return false;
                }
            }

            if (skill.ManaCost > 0 && _manaManager != null)
            {
                if (!_manaManager.TrySpendMana(Mathf.RoundToInt(skill.ManaCost)))
                {
                    if (skill.StaminaCost > 0 && _staminaManager != null)
                    {
                        _staminaManager.AddStamina(Mathf.RoundToInt(skill.StaminaCost));
                    }
                    return false;
                }
            }

            if (skill.BaseDamage > 0)
            {
                var damageResult = DamageCalculator.CalculateDirectDamage(skill.BaseDamage);
                GameEventBus.Publish(new DamageAppliedEvent(damageResult, transform.position));
            }

            Debug.Log($"SkillActionExecutor: Executed skill {skill.DisplayName}, damage={skill.BaseDamage}", this);
            GameEventBus.Publish(new SkillActionExecutedEvent(skillActionId));
            return true;
        }
    }
}
