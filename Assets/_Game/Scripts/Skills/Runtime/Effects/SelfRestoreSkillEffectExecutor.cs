using CindarsHope.Core.Bootstrap;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>
    /// Real utility/survival skill executor: restores HP, stamina, and/or mana on the caster
    /// (emergency kit, survival instinct, safe camp). Long cooldowns balance the free restore.
    /// </summary>
    public sealed class SelfRestoreSkillEffectExecutor : ISkillEffectExecutor
    {
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly int _restoreHp;
        private readonly int _restoreStamina;
        private readonly int _restoreMana;
        private readonly float _cooldownSeconds;

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Utility;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public SelfRestoreSkillEffectExecutor(
            string effectId,
            string displayName,
            int restoreHp,
            int restoreStamina,
            int restoreMana,
            float cooldownSeconds = 30f)
        {
            _effectId = effectId;
            _displayName = displayName;
            _restoreHp = restoreHp;
            _restoreStamina = restoreStamina;
            _restoreMana = restoreMana;
            _cooldownSeconds = cooldownSeconds;
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
                return SkillEffectResult.Failed("NoBootstrap", "Sistemas do jogador indisponiveis.");

            bool restoredAnything = false;

            if (_restoreHp > 0 && bootstrap.PlayerManager != null)
            {
                bootstrap.PlayerManager.RestoreHP(_restoreHp);
                restoredAnything = true;
            }

            if (_restoreStamina > 0 && bootstrap.StaminaManager != null)
            {
                bootstrap.StaminaManager.AddStamina(_restoreStamina);
                restoredAnything = true;
            }

            if (_restoreMana > 0 && bootstrap.ManaManager != null)
            {
                bootstrap.ManaManager.RestoreMana(_restoreMana);
                restoredAnything = true;
            }

            if (!restoredAnything)
                return SkillEffectResult.Failed("NothingToRestore", $"{_displayName}: nenhum recurso para restaurar.");

            CindarsHope.Combat.CombatLog.Log($"CombatLog: SkillSelfRestore. EffectId={_effectId}, HP=+{_restoreHp}, Stamina=+{_restoreStamina}, Mana=+{_restoreMana}");
            return SkillEffectResult.Succeeded(
                $"{_displayName}: recursos restaurados.",
                costSpent: false,
                cooldownStarted: true,
                cooldownSeconds: _cooldownSeconds);
        }
    }
}
