using UnityEngine;

namespace CindarsHope.Cave.Data
{
    /// <summary>
    /// fable_78 — todas as constantes TUNÁVEIS do ecossistema da caverna (seção 16.1 da spec).
    /// Nenhum magic value de balance vive no código (rule no-magic-balance-values): planners e runtime
    /// leem daqui. Defaults sensatos; calibragem final = fable_59. Arrays por banda são indexados pela
    /// banda de bioma 0..6 (Stone, Fungal, Ice, Fire, Ruins, Deep, Void).
    /// </summary>
    [CreateAssetMenu(fileName = "CaveEcosystemBalance", menuName = "CindarsHope/Cave/Ecosystem Balance")]
    public sealed class CaveEcosystemBalanceSO : ScriptableObject
    {
        public const int BandCount = 7;

        [Header("Conflito inter-monstro (chance por entrada)")]
        [SerializeField, Range(0f, 1f)] private float _interMonsterConflictChance = 0.05f;
        [SerializeField, Range(0f, 1f)] private float _interMonsterConflictReducedChance = 0.005f;

        [Header("Conflito inter-monstro (combate)")]
        [SerializeField, Range(0f, 1f)] private float _interMonsterDamageMultiplier = 0.10f;
        [SerializeField, Range(0f, 1f)] private float _interMonsterKillLootMultiplier = 0.40f;

        [Header("Status leve \"Ferido\" (gancho tático)")]
        [SerializeField, Range(0f, 1f)] private float _woundedDefenseMultiplier = 0.85f;
        [SerializeField, Min(0f)] private float _woundedDurationSeconds = 6f;

        [Header("Pesos de aggro (empate = mais próximo)")]
        [SerializeField, Min(0f)] private float _playerAggroWeight = 1f;
        [SerializeField, Min(0f)] private float _rivalAggroWeight = 1f;

        [Header("Threat budget por banda (piso / teto)")]
        [SerializeField] private int[] _threatBudgetMinByBand = { 6, 8, 11, 14, 17, 21, 25 };
        [SerializeField] private int[] _threatBudgetMaxByBand = { 12, 15, 19, 23, 28, 33, 40 };

        [Header("Densidade de inimigos por banda")]
        [SerializeField] private int[] _enemyDensityMinByBand = { 16, 18, 20, 22, 24, 27, 30 };
        [SerializeField] private int[] _enemyDensityMaxByBand = { 24, 26, 28, 30, 32, 36, 40 };
        [SerializeField, Min(1)] private int _enemyDensityHardCap = 44;

        [Header("Densidade de elementos ambientais por banda (fração de tiles candidatos)")]
        [SerializeField] private float[] _environmentElementDensityByBand = { 0.06f, 0.08f, 0.08f, 0.09f, 0.09f, 0.10f, 0.11f };

        [Header("Composição de decor por contexto (spec_cave_decor_composition_runtime, CV03)")]
        [Tooltip("Multiplicador aplicado à densidade de banda SÓ para o contexto FloorCluster (chão aberto). " +
                 "Valor < 1 reduz a quantidade de SEMENTES de cluster no chão (cada semente ainda expande " +
                 "para 2-4 elementos), cortando a sensação de 'confete' sem mexer na densidade de " +
                 "CeilingHang/WallHug nem no total garantido (EnsureGuaranteedPresence).")]
        [SerializeField, Range(0.1f, 1f)] private float _floorClusterDensityMultiplier = 0.5f;

        [Header("Polimento visual (spec_cave_visual_polish_runtime, CV04)")]
        [Tooltip("Fração de células de chão aberto (FloorCluster-elegíveis) que recebem UMA peça de " +
                 "cascalho/litter miúdo (CaveBiomeArtProfileSO.GroundScatterSprites). Denso por design — " +
                 "distinto da densidade de FloorCluster (props grandes, esparsos). Puramente visual, " +
                 "recomputado a cada materialização (não persiste no snapshot).")]
        [SerializeField, Range(0f, 1f)] private float _groundScatterDensity = 0.35f;
        [Tooltip("Chance determinística (por célula de parede que encosta em chão) de receber musgo/" +
                 "vegetação de base (CaveBiomeArtProfileSO.WallSurfaceSprites). Baixa por design — só " +
                 "algumas células de parede devem ter musgo, não todas.")]
        [SerializeField, Range(0f, 1f)] private float _wallSurfaceChance = 0.12f;

        [Header("Entrada segura")]
        [SerializeField, Min(0f)] private float _safeEntryRadius = 4f;

        [Header("Balance de HP/dano dos inimigos")]
        [Tooltip("Multiplicador global aplicado ao HP base dos inimigos comuns e elites durante o spawn. " +
                 "Valor 1.0 = HP do catálogo (muito baixo vs dano do player). " +
                 "Alvo: inimigo Stone comum (lv1) sobrevive 3-5 hits de ataque básico.")]
        [SerializeField, Min(1f)] private float _enemyHpBaseMultiplier = 3.5f;

        public float EnemyHpBaseMultiplier => _enemyHpBaseMultiplier;
        public float InterMonsterConflictChance => _interMonsterConflictChance;
        public float InterMonsterConflictReducedChance => _interMonsterConflictReducedChance;
        public float InterMonsterDamageMultiplier => _interMonsterDamageMultiplier;
        public float InterMonsterKillLootMultiplier => _interMonsterKillLootMultiplier;
        public float WoundedDefenseMultiplier => _woundedDefenseMultiplier;
        public float WoundedDurationSeconds => _woundedDurationSeconds;
        public float PlayerAggroWeight => _playerAggroWeight;
        public float RivalAggroWeight => _rivalAggroWeight;
        public int EnemyDensityHardCap => _enemyDensityHardCap;
        public float SafeEntryRadius => _safeEntryRadius;

        public int GetThreatBudgetMin(int bandIndex) => ReadBand(_threatBudgetMinByBand, bandIndex, 6);
        public int GetThreatBudgetMax(int bandIndex) => ReadBand(_threatBudgetMaxByBand, bandIndex, 12);
        public int GetEnemyDensityMin(int bandIndex) => ReadBand(_enemyDensityMinByBand, bandIndex, 16);
        public int GetEnemyDensityMax(int bandIndex) => ReadBand(_enemyDensityMaxByBand, bandIndex, 24);
        public float GetEnvironmentElementDensity(int bandIndex) => ReadBand(_environmentElementDensityByBand, bandIndex, 0.06f);

        /// <summary>spec_cave_decor_composition_runtime (CV03): multiplicador de densidade só para
        /// sementes de FloorCluster (chão aberto). Default 0.5 — metade das sementes que o antigo "1
        /// singleton por célula" colocaria, compensado pelo cluster (cada semente vira 2-4 elementos).</summary>
        public float FloorClusterDensityMultiplier => Mathf.Clamp(_floorClusterDensityMultiplier <= 0f ? 0.5f : _floorClusterDensityMultiplier, 0.1f, 1f);

        /// <summary>spec_cave_visual_polish_runtime (CV04): fração de células GroundScatter-elegíveis
        /// (chão aberto, FloorCluster) que recebem 1 peça de litter. Default 0.35 (denso).</summary>
        public float GroundScatterDensity => Mathf.Clamp01(_groundScatterDensity);

        /// <summary>spec_cave_visual_polish_runtime (CV04): chance por célula de parede-encosta-chão de
        /// receber decor de musgo/vegetação na base. Default 0.12 (baixa).</summary>
        public float WallSurfaceChance => Mathf.Clamp01(_wallSurfaceChance);

        private static int ReadBand(int[] source, int bandIndex, int fallback)
        {
            if (source == null || source.Length == 0)
            {
                return fallback;
            }

            var clamped = Mathf.Clamp(bandIndex, 0, source.Length - 1);
            return source[clamped];
        }

        private static float ReadBand(float[] source, int bandIndex, float fallback)
        {
            if (source == null || source.Length == 0)
            {
                return fallback;
            }

            var clamped = Mathf.Clamp(bandIndex, 0, source.Length - 1);
            return source[clamped];
        }

        private void OnValidate()
        {
            _woundedDurationSeconds = Mathf.Max(0f, _woundedDurationSeconds);
            _playerAggroWeight = Mathf.Max(0f, _playerAggroWeight);
            _rivalAggroWeight = Mathf.Max(0f, _rivalAggroWeight);
            _enemyDensityHardCap = Mathf.Max(1, _enemyDensityHardCap);
            _safeEntryRadius = Mathf.Max(0f, _safeEntryRadius);
            _floorClusterDensityMultiplier = Mathf.Clamp(_floorClusterDensityMultiplier, 0.1f, 1f);
            _groundScatterDensity = Mathf.Clamp01(_groundScatterDensity);
            _wallSurfaceChance = Mathf.Clamp01(_wallSurfaceChance);
        }
    }
}
