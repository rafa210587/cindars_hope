namespace CindarsHope.Core.Events
{
    /// <summary>F02 — postura do inimigo quebrou (stagger + CoreExposed abertos).</summary>
    public readonly struct EnemyPostureBrokenEvent
    {
        public string EnemyId { get; }

        public EnemyPostureBrokenEvent(string enemyId)
        {
            EnemyId = enemyId;
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

        public PlayerPerfectBlockEvent(string sourceId, int negatedDamage)
        {
            SourceId = sourceId;
            NegatedDamage = negatedDamage;
        }
    }
}
