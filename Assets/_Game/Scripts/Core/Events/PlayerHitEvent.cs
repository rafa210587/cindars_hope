namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando o jogador leva dano de um ataque inimigo.
    /// </summary>
    public readonly struct PlayerHitEvent
    {
        public int DamageAmount { get; }

        public PlayerHitEvent(int damageAmount)
        {
            DamageAmount = damageAmount;
        }
    }
}
