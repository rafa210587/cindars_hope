using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
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
        // arch: quebra do par mutuo Player|Skills (2026-07-15) — porta ISkillTreeRuntime em vez do
        // tipo concreto SkillTreeManager.
        private ISkillTreeRuntime _skillTreeRuntime;
        private int _baseMaxHP;
        private int _baseMaxStamina;
        private int _baseMaxMana;
        private bool _basesCaptured;
        private DerivedStatsCalculator.DerivedStats _lastStats;
        private float _manaBaseRegenBonusFraction;
        private float _statusRecoveryReduction;
        private float _terrainPenaltyRecovery;
        private System.Func<float> _ownedManaRegenSource;
        private System.Func<float> _ownedStatusRecoverySource;
        private System.Func<float> _ownedTerrainRecoverySource;
        private System.Func<DamageType, int> _ownedResistanceSource;
        private System.Func<float> _ownedDodgeTrainingSource;
        private System.Func<float> _ownedCraftTimeSource;
        private System.Func<int, int> _ownedRepairEfficiencySource;
        private float _naturalHealthRegenAccumulator;

        public static PlayerVitalsApplier Instance => _instance;

        /// <summary>fable_47 (follow-up 2): redução de tempo de craft derivada (F18). Consumida por CraftingRuntime.</summary>
        public static System.Func<float> CraftTimeReductionSource;

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

        /// <summary>Resistência por tipo de dano (consumida via ResistanceProvider pelo Combat).</summary>
        public static int ResistanceFor(DerivedStatsCalculator.DerivedStats stats, DamageType type)
        {
            if (stats == null)
            {
                return 0;
            }

            switch (type)
            {
                case DamageType.Toxic: return stats.ToxicResistance;
                case DamageType.Ice: return stats.ColdResistance;
                case DamageType.Fire: return stats.HeatResistance;
                default: return 0;
            }
        }

        public static PlayerResistancesChangedEvent CreateResistanceSnapshot(
            DerivedStatsCalculator.DerivedStats stats)
        {
            int toxic = ResistanceFor(stats, DamageType.Toxic);
            int cold = ResistanceFor(stats, DamageType.Ice);
            int heat = ResistanceFor(stats, DamageType.Fire);
            return new PlayerResistancesChangedEvent(
                toxic,
                cold,
                heat,
                1f - DerivedFollowupFormulas.StatusDurationMultiplier(toxic),
                1f - DerivedFollowupFormulas.StatusDurationMultiplier(cold),
                1f - DerivedFollowupFormulas.StatusDurationMultiplier(heat));
        }

        public void Reapply()
        {
            if (_playerManager == null)
            {
                return;
            }

            var skillTree = _skillTreeRuntime;
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

            _manaBaseRegenBonusFraction = Mathf.Clamp01(_lastStats.ManaRegenBasePercent);
            _statusRecoveryReduction = Mathf.Clamp01(_lastStats.StatusDurationReduction);
            _terrainPenaltyRecovery = Mathf.Clamp01(_lastStats.TerrainPenaltyRecovery);
            EnsureOwnedProviderDelegates();
            PassiveSurvivalModifierProvider.ManaBaseRegenBonusFractionSource = _ownedManaRegenSource;
            PassiveSurvivalModifierProvider.StatusRecoveryReductionSource = _ownedStatusRecoverySource;
            PassiveSurvivalModifierProvider.TerrainPenaltyRecoverySource = _ownedTerrainRecoverySource;

            _playerManager.SetMaxHP(Mathf.Max(1, _lastStats.MaxHP));
            if (_staminaManager != null)
            {
                _staminaManager.SetMaxStamina(Mathf.Max(1, _lastStats.MaxStamina));
                _staminaManager.ExternalRegenBonus = _lastStats.StaminaRegen;
            }

            if (_manaManager != null)
            {
                _manaManager.SetMaxManaPreservingRatio(Mathf.Max(1, _baseMaxMana + _lastStats.MaxMana));
                _manaManager.ExternalRegenBonus = Mathf.Max(0f, _lastStats.ManaRegen) +
                    PassiveSurvivalModifierProvider.ResolveManaBaseRegen(
                        _manaManager.BaseRegenPerSecond);
            }

            if (_hungerManager != null)
            {
                _hungerManager.DrainMultiplier = Mathf.Clamp(1f - _lastStats.HungerDrainReduction, 0.25f, 1f);
            }

            // F18: resistências alimentam o redutor central (F03), via porta neutra (arch: quebra
            // do par mutuo Combat|Player, 2026-07-16) — Player deixa de nomear PlayerDamageReceiver.
            ResistanceProvider.Source = _ownedResistanceSource;
            DodgeCostModifierProvider.TrainingReductionSource = _ownedDodgeTrainingSource;
            GameEventBus.Publish(CreateResistanceSnapshot(_lastStats));

            // fable_47 (follow-up 2): expõe craft/repair derivados como fontes únicas (padrão F18).
            CraftTimeReductionSource = _ownedCraftTimeSource;
            // arch: quebra Equipment|Player — Equipment aplica o reparo efetivo via porta neutra
            // RepairEfficiencyProvider (Foundation); o Player compoe aqui o bonus atual com a formula.
            CindarsHope.Foundation.RepairEfficiencyProvider.EffectiveRepairAmountSource =
                _ownedRepairEfficiencySource;

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

            _playerManager = bootstrap.PlayerManager as PlayerManager;
            _staminaManager = bootstrap.StaminaManager as StaminaManager;
            _manaManager = bootstrap.ManaManager as ManaManager;
            _hungerManager = bootstrap.GetComponent<HungerManager>();
            _skillTreeRuntime = DomainManagerRegistry.Get<ISkillTreeRuntime>();

            if (!_basesCaptured && _playerManager != null)
            {
                _baseMaxHP = _playerManager.MaxHP;
                _baseMaxStamina = _staminaManager != null ? _staminaManager.MaxStamina : 100;
                _baseMaxMana = _manaManager != null ? _manaManager.MaxMana : 0;
                _basesCaptured = true;
            }

            Reapply();
        }

        private void Update()
        {
            if (_playerManager == null || _playerManager.CurrentHP <= 0 ||
                _playerManager.CurrentHP >= _playerManager.MaxHP ||
                CombatStateProvider.IsInCombat?.Invoke() == true)
            {
                _naturalHealthRegenAccumulator = 0f;
                return;
            }

            var player = PlayerController.ActiveInstance;
            var balance = player != null ? player.NeedsBalance : null;
            float baseRate = balance != null ? balance.BaseNaturalHealthRegenPerSecond : 0f;
            if (baseRate <= 0f) return;

            var position = player != null ? player.transform.position : transform.position;
            float multiplier = NaturalSurvivalRateModifierProvider.Resolve(
                NaturalSurvivalRateChannel.HealthRegen, position.x, position.y);
            _naturalHealthRegenAccumulator += baseRate * multiplier * Time.deltaTime;
            int wholeHp = Mathf.FloorToInt(_naturalHealthRegenAccumulator);
            if (wholeHp <= 0) return;

            _naturalHealthRegenAccumulator -= wholeHp;
            _playerManager.RestoreHP(wholeHp);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<EquipmentSlotChangedEvent>(OnInvalidatingEvent);
            GameEventBus.Subscribe<SkillDerivedStatsChangedEvent>(OnSkillDerivedStatsChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EquipmentSlotChangedEvent>(OnInvalidatingEvent);
            GameEventBus.Unsubscribe<SkillDerivedStatsChangedEvent>(OnSkillDerivedStatsChanged);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            ClearOwnedProviderDelegates();
        }

        private void OnInvalidatingEvent(EquipmentSlotChangedEvent evt)
        {
            Reapply();
        }

        private void OnSkillDerivedStatsChanged(SkillDerivedStatsChangedEvent evt)
        {
            _skillTreeRuntime = DomainManagerRegistry.Get<ISkillTreeRuntime>();
            Reapply();
        }

        private void EnsureOwnedProviderDelegates()
        {
            _ownedManaRegenSource ??= () => _manaBaseRegenBonusFraction;
            _ownedStatusRecoverySource ??= () => _statusRecoveryReduction;
            _ownedTerrainRecoverySource ??= () => _terrainPenaltyRecovery;
            _ownedResistanceSource ??= type => ResistanceFor(_lastStats, type);
            _ownedDodgeTrainingSource ??= () =>
                Mathf.Clamp01(_lastStats?.DodgeCostReduction ?? 0f);
            _ownedCraftTimeSource ??= () => _lastStats?.CraftTimeReduction ?? 0f;
            _ownedRepairEfficiencySource ??= baseRestore =>
                DerivedFollowupFormulas.EffectiveRepairAmount(
                    baseRestore,
                    _lastStats?.RepairEfficiencyBonus ?? 0f);
        }

        private void ClearOwnedProviderDelegates()
        {
            if (ReferenceEquals(PassiveSurvivalModifierProvider.ManaBaseRegenBonusFractionSource,
                    _ownedManaRegenSource))
                PassiveSurvivalModifierProvider.ManaBaseRegenBonusFractionSource = null;
            if (ReferenceEquals(PassiveSurvivalModifierProvider.StatusRecoveryReductionSource,
                    _ownedStatusRecoverySource))
                PassiveSurvivalModifierProvider.StatusRecoveryReductionSource = null;
            if (ReferenceEquals(PassiveSurvivalModifierProvider.TerrainPenaltyRecoverySource,
                    _ownedTerrainRecoverySource))
                PassiveSurvivalModifierProvider.TerrainPenaltyRecoverySource = null;
            if (ReferenceEquals(ResistanceProvider.Source, _ownedResistanceSource))
                ResistanceProvider.Source = null;
            if (ReferenceEquals(DodgeCostModifierProvider.TrainingReductionSource,
                    _ownedDodgeTrainingSource))
                DodgeCostModifierProvider.TrainingReductionSource = null;
            if (ReferenceEquals(CraftTimeReductionSource, _ownedCraftTimeSource))
                CraftTimeReductionSource = null;
            if (ReferenceEquals(RepairEfficiencyProvider.EffectiveRepairAmountSource,
                    _ownedRepairEfficiencySource))
                RepairEfficiencyProvider.EffectiveRepairAmountSource = null;
        }

    }

    /// <summary>Garante o applier em runtime (padrão bootstrap do projeto).</summary>
    public static class PlayerVitalsApplierBootstrap
    {
        public static void Install(Transform owner)
        {
            if (PlayerVitalsApplier.Instance != null)
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
