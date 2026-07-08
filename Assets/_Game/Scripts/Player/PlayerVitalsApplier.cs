using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player
{
    /// <summary>
    /// F18 — aplica as saídas do DerivedStatsCalculator (órfão WAVE 05) aos managers de
    /// vitals: MaxHP/MaxStamina/MaxMana (proporção preservada), regens, resistências
    /// (PlayerDamageReceiver) e redução de drain de fome. Recalcula nos eventos de F02.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerVitalsApplier : MonoBehaviour
    {
        private static PlayerVitalsApplier _instance;

        private PlayerManager _playerManager;
        private StaminaManager _staminaManager;
        private ManaManager _manaManager;
        private HungerManager _hungerManager;
        private SkillTreeManagerRef _skillTreeRef;
        private int _baseMaxHP;
        private int _baseMaxStamina;
        private int _baseMaxMana;
        private bool _basesCaptured;
        private DerivedStatsCalculator.DerivedStats _lastStats;

        public static PlayerVitalsApplier Instance => _instance;

        /// <summary>fable_47 (follow-up 2): redução de tempo de craft derivada (F18). Consumida por CraftingRuntime.</summary>
        public static System.Func<float> CraftTimeReductionSource;

        /// <summary>fable_47 (follow-up 2): bônus de eficiência de reparo derivado (F18). Consumido por EquipmentManager.</summary>
        public static System.Func<float> RepairEfficiencyBonusSource;

        private sealed class SkillTreeManagerRef
        {
            public CindarsHope.Skills.SkillTreeManager Manager;
        }

        /// <summary>Proporção preservada com floor 1 (puro, testável).</summary>
        public static int PreserveRatio(int current, int oldMax, int newMax)
        {
            if (newMax <= 0)
            {
                return 1;
            }

            var ratio = oldMax > 0 ? (float)current / oldMax : 1f;
            return Mathf.Max(1, Mathf.RoundToInt(newMax * ratio));
        }

        /// <summary>Resistência por tipo de dano (consumida pelo PlayerDamageReceiver).</summary>
        public static int ResistanceFor(DerivedStatsCalculator.DerivedStats stats, CindarsHope.Combat.DamageType type)
        {
            if (stats == null)
            {
                return 0;
            }

            switch (type)
            {
                case CindarsHope.Combat.DamageType.Toxic: return stats.ToxicResistance;
                case CindarsHope.Combat.DamageType.Ice: return stats.ColdResistance;
                case CindarsHope.Combat.DamageType.Fire: return stats.HeatResistance;
                default: return 0;
            }
        }

        public void Reapply()
        {
            if (_playerManager == null)
            {
                return;
            }

            var skillTree = _skillTreeRef?.Manager;
            _lastStats = DerivedStatsCalculator.Calculate(
                baseMaxHP: _baseMaxHP,
                baseAttack: 0,
                baseDefense: 0,
                baseMoveSpeed: 0f,
                baseMaxStamina: _baseMaxStamina,
                baseStaminaRegen: 0f,
                baseAttackSpeed: 1f,
                equippedItems: null,
                passiveModifiers: skillTree != null ? skillTree.GetAllActivePassiveModifiers() : null);

            _playerManager.SetMaxHP(Mathf.Max(1, _lastStats.MaxHP));
            if (_staminaManager != null)
            {
                _staminaManager.SetMaxStamina(Mathf.Max(1, _lastStats.MaxStamina));
                _staminaManager.ExternalRegenBonus = _lastStats.StaminaRegen;
            }

            if (_manaManager != null)
            {
                _manaManager.SetMaxManaPreservingRatio(Mathf.Max(1, _baseMaxMana + _lastStats.MaxMana));
                _manaManager.ExternalRegenBonus = _lastStats.ManaRegen;
            }

            if (_hungerManager != null)
            {
                _hungerManager.DrainMultiplier = Mathf.Clamp(1f - _lastStats.HungerDrainReduction, 0.25f, 1f);
            }

            // F18: resistências alimentam o redutor central (F03).
            CindarsHope.Combat.PlayerDamageReceiver.ResistanceSource = type => ResistanceFor(_lastStats, type);

            // fable_47 (follow-up 2): expõe craft/repair derivados como fontes únicas (padrão F18).
            CraftTimeReductionSource = () => PlayerVitalsApplier.Instance?._lastStats?.CraftTimeReduction ?? 0f;
            RepairEfficiencyBonusSource = () => PlayerVitalsApplier.Instance?._lastStats?.RepairEfficiencyBonus ?? 0f;

            // fable_47 (follow-up 1): MoveSpeed derivado vira fator no composer do player.
            ReapplyDerivedMoveSpeed();
        }

        /// <summary>
        /// fable_47 — empurra o fator DerivedMoveSpeed para o composer do PlayerController ativo
        /// (auto-registrado, sem global search). Idempotente: limpa o fator quando não há bônus.
        /// Chamado no mesmo evento de invalidação (EquipmentSlotChangedEvent) e quando um novo
        /// PlayerController entra em cena.
        /// </summary>
        public void ReapplyDerivedMoveSpeed()
        {
            var controller = CindarsHope.Player.PlayerController.ActiveInstance;
            if (controller == null || _lastStats == null)
            {
                return;
            }

            var factor = DerivedFollowupFormulas.DerivedMoveSpeedFactor(_lastStats.MoveSpeed, controller.BaseMoveSpeed);
            if (Mathf.Approximately(factor, 1f))
            {
                controller.SpeedComposer.ClearFactor(CindarsHope.Player.Movement.SpeedFactorKind.DerivedMoveSpeed);
            }
            else
            {
                controller.SpeedComposer.SetFactor(CindarsHope.Player.Movement.SpeedFactorKind.DerivedMoveSpeed, factor);
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
        }

        private void Start()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return;
            }

            _playerManager = bootstrap.PlayerManager;
            _staminaManager = bootstrap.StaminaManager;
            _manaManager = bootstrap.ManaManager;
            _hungerManager = bootstrap.GetComponent<HungerManager>();
            _skillTreeRef = new SkillTreeManagerRef { Manager = CindarsHope.Skills.SkillTreeManager.Instance };

            if (!_basesCaptured && _playerManager != null)
            {
                _baseMaxHP = _playerManager.MaxHP;
                _baseMaxStamina = _staminaManager != null ? _staminaManager.MaxStamina : 100;
                _baseMaxMana = _manaManager != null ? _manaManager.MaxMana : 0;
                _basesCaptured = true;
            }

            Reapply();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<EquipmentSlotChangedEvent>(OnInvalidatingEvent);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EquipmentSlotChangedEvent>(OnInvalidatingEvent);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnInvalidatingEvent(EquipmentSlotChangedEvent evt)
        {
            Reapply();
        }
    }

    /// <summary>Garante o applier em runtime (padrão bootstrap do projeto).</summary>
    public static class PlayerVitalsApplierBootstrap
    {
        public static void Install(Transform owner)
        {
            if (Object.FindAnyObjectByType<PlayerVitalsApplier>() != null)
            {
                return;
            }

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return;
            }

            bootstrap.gameObject.AddComponent<PlayerVitalsApplier>();
            Debug.Log("[PlayerVitalsApplierBootstrap] PlayerVitalsApplier instanciado via bootstrap.");
        }
    }
}
