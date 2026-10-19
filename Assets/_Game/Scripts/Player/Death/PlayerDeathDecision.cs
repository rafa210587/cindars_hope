namespace CindarsHope.Player.Death
{
    /// <summary>
    /// Regra de decisao de morte em C# puro (sem UnityEngine), testavel em EditMode.
    ///
    /// Contrato:
    ///  - HP &lt;= 0 e ainda nao morto  -> dispara morte uma vez (ShouldDie = true), fica morto.
    ///  - HP &lt;= 0 e ja morto         -> nao redispara (anti-spam de HPChangedEvent).
    ///  - HP &gt; 0                     -> rearma (NextIsDead = false), nunca dispara morte.
    /// </summary>
    public readonly struct PlayerDeathDecision
    {
        public bool ShouldDie { get; }
        public bool NextIsDead { get; }

        private PlayerDeathDecision(bool shouldDie, bool nextIsDead)
        {
            ShouldDie = shouldDie;
            NextIsDead = nextIsDead;
        }

        public static PlayerDeathDecision Evaluate(int currentHp, bool isDead)
        {
            if (currentHp > 0)
            {
                // Revive/cura rearma para a proxima morte.
                return new PlayerDeathDecision(shouldDie: false, nextIsDead: false);
            }

            if (isDead)
            {
                // Ja morreu; nao redispara.
                return new PlayerDeathDecision(shouldDie: false, nextIsDead: true);
            }

            // HP<=0 pela primeira vez: dispara morte.
            return new PlayerDeathDecision(shouldDie: true, nextIsDead: true);
        }
    }
}
