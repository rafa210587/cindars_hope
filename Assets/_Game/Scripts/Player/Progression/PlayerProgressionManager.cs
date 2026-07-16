using CindarsHope.Core;
using CindarsHope.Core.Events;
// arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) —
// PlayerAttributeType agora vive em CindarsHope.Foundation.
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Player.Progression
{
    [DisallowMultipleComponent]
    public class PlayerProgressionManager : MonoBehaviour, IPlayerProgressionRuntime
    {
        // arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) —
        // self-registro estatico (molde Craft/Economy/Skills/Equipment) para o GameBootstrap parar
        // de segurar esta referencia serializada. Nao proibido pelo ratchet GlobalGoldAccess (que so
        // cobre PlayerManager/GoldManager/EconomyManager). Registrado tambem sob a porta marcadora
        // IPlayerProgressionRuntime (Foundation, 2026-07-15): GameBootstrap so expoe a referencia
        // (nao chama nenhum metodo de dominio), entao a porta nao precisa de membros.
        public static PlayerProgressionManager Instance { get; private set; }

        [SerializeField] private PlayerProgressionSaveData _state = new PlayerProgressionSaveData();

        public int Level => _state.Level;
        public int CurrentXp => _state.CurrentXp;
        public int XpToNextLevel => _state.XpToNextLevel;
        public int UnspentAttributePoints => _state.UnspentAttributePoints;
        public int UnspentSkillPoints => _state.UnspentSkillPoints;
        public int Strength => _state.Strength;
        public int Dexterity => _state.Dexterity;
        public int Intelligence => _state.Intelligence;
        public int Willpower => _state.Willpower;
        public int Constitution => _state.Constitution;
        public int Breath => _state.Breath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            NormalizeState();
            CindarsHope.Foundation.DomainManagerRegistry.Register<IPlayerProgressionRuntime>(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            CindarsHope.Foundation.DomainManagerRegistry.Unregister<IPlayerProgressionRuntime>(this);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Subscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Subscribe<CropHarvestedEvent>(OnCropHarvested);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Unsubscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Unsubscribe<CropHarvestedEvent>(OnCropHarvested);
        }

        // F42: XP total é a fonte de verdade; nível/parcial derivados pela curva canônica.
        public void AddXp(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            NormalizeState();
            int oldLevel = _state.Level;
            _state.TotalXp += amount;
            RecomputeDerivedProgression();
            GameEventBus.Publish(new PlayerXpChangedEvent(amount, _state.CurrentXp, _state.XpToNextLevel, _state.Level));

            for (int reachedLevel = oldLevel + 1; reachedLevel <= _state.Level; reachedLevel++)
            {
                int grantedAttributePoints = PlayerProgressionRules.CalculateAttributePointsGrantedOnLevelUp(reachedLevel);
                int grantedSkillPoints = PlayerProgressionRules.CalculateSkillPointsGrantedOnLevelUp(reachedLevel);
                _state.UnspentAttributePoints += grantedAttributePoints;
                _state.UnspentSkillPoints += grantedSkillPoints;
                GameEventBus.Publish(new PlayerLevelChangedEvent(reachedLevel - 1, reachedLevel, grantedAttributePoints, grantedSkillPoints));
            }
        }

        public bool TrySpendAttributePoint(PlayerAttributeType attributeType)
        {
            NormalizeState();
            if (_state.UnspentAttributePoints <= 0)
            {
                return false;
            }

            int value;
            switch (attributeType)
            {
                case PlayerAttributeType.Strength:
                    value = ++_state.Strength;
                    break;
                case PlayerAttributeType.Dexterity:
                    value = ++_state.Dexterity;
                    break;
                case PlayerAttributeType.Intelligence:
                    value = ++_state.Intelligence;
                    break;
                case PlayerAttributeType.Willpower:
                    value = ++_state.Willpower;
                    break;
                case PlayerAttributeType.Constitution:
                    value = ++_state.Constitution;
                    break;
                case PlayerAttributeType.Breath:
                    value = ++_state.Breath;
                    break;
                default:
                    return false;
            }

            _state.UnspentAttributePoints--;
            GameEventBus.Publish(new PlayerAttributeChangedEvent(attributeType, value, _state.UnspentAttributePoints));
            return true;
        }

        public bool TrySpendSkillPoints(int amount)
        {
            NormalizeState();
            if (amount <= 0 || _state.UnspentSkillPoints < amount)
            {
                return false;
            }

            _state.UnspentSkillPoints -= amount;
            return true;
        }

        // fable_34 — main-quest acts grant +1 skill point each (decision Q6.2b / quest_rules Rule 6).
        // The idempotency guard (an act is rewarded at most once, even after reload) lives in the
        // quest save section (RewardedMainActIds); this method only adds the granted points to the
        // canonical ledger. Publishes SkillPointGrantedEvent so the skill tree reflects the gain.
        public void GrantSkillPoints(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            NormalizeState();
            _state.UnspentSkillPoints += amount;
            GameEventBus.Publish(new SkillPointGrantedEvent(amount, _state.UnspentSkillPoints, _state.Level));
        }

        public PlayerProgressionSaveData CaptureSaveData()
        {
            NormalizeState();
            return new PlayerProgressionSaveData
            {
                Level = _state.Level,
                CurrentXp = _state.CurrentXp,
                XpToNextLevel = _state.XpToNextLevel,
                TotalXp = _state.TotalXp,
                DeepestXpAwardedCaveLevel = _state.DeepestXpAwardedCaveLevel,
                FirstHarvestXpSeedIds = new System.Collections.Generic.List<string>(_state.FirstHarvestXpSeedIds ?? new System.Collections.Generic.List<string>()),
                UnspentAttributePoints = _state.UnspentAttributePoints,
                UnspentSkillPoints = _state.UnspentSkillPoints,
                Strength = _state.Strength,
                Dexterity = _state.Dexterity,
                Intelligence = _state.Intelligence,
                Willpower = _state.Willpower,
                Constitution = _state.Constitution,
                Breath = _state.Breath
            };
        }

        public void RestoreFromSaveData(PlayerProgressionSaveData saveData)
        {
            _state = saveData ?? new PlayerProgressionSaveData();
            NormalizeState();
        }

        public int ResetCurrentLevelXp()
        {
            NormalizeState();
            var lostXp = _state.CurrentXp;
            if (lostXp <= 0)
            {
                return 0;
            }

            _state.TotalXp = System.Math.Max(0, _state.TotalXp - lostXp);
            RecomputeDerivedProgression();
            GameEventBus.Publish(new PlayerXpChangedEvent(-lostXp, _state.CurrentXp, _state.XpToNextLevel, _state.Level));
            return lostXp;
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (evt.XpReward > 0)
            {
                AddXp(evt.XpReward);
            }
        }

        // F42: fonte de XP — descoberta de nível novo da caverna (+15 × banda de dezena).
        private void OnCaveLevelEntered(CaveLevelEnteredEvent evt)
        {
            NormalizeState();
            if (evt.CaveLevel <= _state.DeepestXpAwardedCaveLevel)
            {
                return;
            }

            _state.DeepestXpAwardedCaveLevel = evt.CaveLevel;
            var band = ((evt.CaveLevel - 1) / 10) + 1;
            AddXp(15 * band);
        }

        // F42: fonte de XP — primeira colheita de cada cultivo (+10, idempotente por seedId).
        private void OnCropHarvested(CropHarvestedEvent evt)
        {
            if (string.IsNullOrWhiteSpace(evt.SeedId))
            {
                return;
            }

            NormalizeState();
            if (_state.FirstHarvestXpSeedIds.Contains(evt.SeedId))
            {
                return;
            }

            _state.FirstHarvestXpSeedIds.Add(evt.SeedId);
            AddXp(10);
        }

        // F42: deriva nível/parcial/próximo do XP total (recompute idempotente, sem grants).
        private void RecomputeDerivedProgression()
        {
            _state.Level = ProgressionCurve.LevelForTotalXp(_state.TotalXp);
            _state.CurrentXp = ProgressionCurve.XpIntoCurrentLevel(_state.TotalXp);
            _state.XpToNextLevel = PlayerProgressionRules.CalculateXpToNextLevel(_state.Level);
        }

        private void NormalizeState()
        {
            if (_state == null)
            {
                _state = new PlayerProgressionSaveData();
            }

            if (_state.FirstHarvestXpSeedIds == null)
            {
                _state.FirstHarvestXpSeedIds = new System.Collections.Generic.List<string>();
            }

            // F42: migração de save legado — TotalXp 0 com nível/parcial avançados.
            // NÃO re-concede pontos (Unspent/atributos preservados como salvos).
            if (_state.TotalXp <= 0 && (_state.Level > 1 || _state.CurrentXp > 0))
            {
                _state.TotalXp = ProgressionCurve.MigrateLegacy(_state.Level, _state.CurrentXp);
            }

            if (_state.TotalXp > 0 || _state.Level > 1)
            {
                RecomputeDerivedProgression();
            }

            _state.Level = Mathf.Clamp(_state.Level, 1, PlayerProgressionRules.MaxLevel);
            _state.CurrentXp = Mathf.Max(0, _state.CurrentXp);
            _state.XpToNextLevel = Mathf.Max(1, _state.XpToNextLevel);
            _state.UnspentAttributePoints = Mathf.Max(0, _state.UnspentAttributePoints);
            _state.UnspentSkillPoints = Mathf.Max(0, _state.UnspentSkillPoints);
            _state.Strength = Mathf.Max(1, _state.Strength);
            _state.Dexterity = Mathf.Max(1, _state.Dexterity);
            _state.Intelligence = Mathf.Max(1, _state.Intelligence);
            _state.Willpower = Mathf.Max(1, _state.Willpower);
            _state.Constitution = Mathf.Max(1, _state.Constitution);
            _state.Breath = Mathf.Max(1, _state.Breath);
        }
    }
}
