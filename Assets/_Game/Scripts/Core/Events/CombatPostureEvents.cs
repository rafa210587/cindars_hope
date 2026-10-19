namespace CindarsHope.Core.Events
{
    /// <summary>F02 — postura do inimigo quebrou (stagger + CoreExposed abertos).</summary>
    public readonly struct EnemyPostureBrokenEvent
    {
        public string EnemyId { get; }
        public string EnemyInstanceId { get; }
        public string SourceId { get; }
        public string SourceInstanceId { get; }
        public bool CausedByPlayer { get; }
        public string ResolutionId { get; }

        public EnemyPostureBrokenEvent(string enemyId)
            : this(enemyId, string.Empty, string.Empty, string.Empty, false, string.Empty)
        {
        }

        public EnemyPostureBrokenEvent(string enemyId, string enemyInstanceId, string sourceId,
            string sourceInstanceId, bool causedByPlayer, string resolutionId)
        {
            EnemyId = enemyId ?? string.Empty;
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            SourceId = sourceId ?? string.Empty;
            SourceInstanceId = sourceInstanceId ?? string.Empty;
            CausedByPlayer = causedByPlayer;
            ResolutionId = resolutionId ?? string.Empty;
        }
    }

    /// <summary>F02 — player liberou um ataque com peso (HUD de carga futuro).</summary>
    public readonly struct PlayerChargedAttackEvent
    {
        public int Weight { get; }

        public PlayerChargedAttackEvent(int weight)
        {
            Weight = weight;
        }
    }

    /// <summary>F27 — perfect block executado (hook da relíquia de Kanthor — F23).</summary>
    public readonly struct PlayerPerfectBlockEvent
    {
        public string SourceId { get; }
        public int NegatedDamage { get; }
        public string ResolutionId { get; }

        public PlayerPerfectBlockEvent(string sourceId, int negatedDamage)
            : this(sourceId, negatedDamage, string.Empty)
        {
        }

        public PlayerPerfectBlockEvent(string sourceId, int negatedDamage, string resolutionId)
        {
            SourceId = sourceId ?? string.Empty;
            NegatedDamage = negatedDamage;
            ResolutionId = resolutionId ?? string.Empty;
        }
    }

    /// <summary>Player mitigou um hit com block normal (fora da janela de perfect block).</summary>
    public readonly struct PlayerNormalBlockEvent
    {
        public string SourceId { get; }
        public int IncomingDamage { get; }
        public int MitigatedDamage { get; }

        public PlayerNormalBlockEvent(string sourceId, int incomingDamage, int mitigatedDamage)
        {
            SourceId = sourceId ?? string.Empty;
            IncomingDamage = incomingDamage;
            MitigatedDamage = mitigatedDamage;
        }
    }
}
