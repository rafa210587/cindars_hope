namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Arquetipo de animacao de ataque melee do player. Mapeia o tipo de arma equipada
    /// a um conjunto de sprites/pasta de arte, sem acoplar Core a Combat.
    /// Valores explícitos para estabilidade (nunca deixar shiftar por reordenacao de enum).
    /// </summary>
    public enum PlayerAttackAnimArchetype
    {
        Sword  = 0,
        Bow    = 1,
        Heavy  = 2,
        Thrust = 3,
        Dagger = 4,
        Cast   = 5,
    }
}
