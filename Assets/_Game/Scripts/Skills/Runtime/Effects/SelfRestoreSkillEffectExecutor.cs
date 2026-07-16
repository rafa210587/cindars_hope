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

            // arch: quebra do par mutuo Core|Player (2026-07-15) — bootstrap.PlayerManager/
            // StaminaManager/ManaManager agora retornam MonoBehaviour; cast local para os tipos
            // concretos.
            var playerManager = bootstrap.PlayerManager as CindarsHope.Player.PlayerManager;
            var staminaManager = bootstrap.StaminaManager as CindarsHope.Player.StaminaManager;
            var manaManager = bootstrap.ManaManager as CindarsHope.Player.ManaManager;

            if (_restoreHp > 0 && playerManager != null)
            {
                playerManager.RestoreHP(_restoreHp);
                restoredAnything = true;
            }

            if (_restoreStamina > 0 && staminaManager != null)
            {
                staminaManager.AddStamina(_restoreStamina);
                restoredAnything = true;
            }

            if (_restoreMana > 0 && manaManager != null)
            {
                manaManager.RestoreMana(_restoreMana);
                restoredAnything = true;
            }

            if (!restoredAnything)
                return SkillEffectResult.Failed("NothingToRestore", $"{_displayName}: nenhum recurso para restaurar.");

            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: SkillSelfRestore. EffectId={_effectId}, HP=+{_restoreHp}, Stamina=+{_restoreStamina}, Mana=+{_restoreMana}");
            return SkillEffectResult.Succeeded(
                $"{_displayName}: recursos restaurados.",
                costSpent: false,
                cooldownStarted: true,
                cooldownSeconds: _cooldownSeconds);
        }
    }
}
