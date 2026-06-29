using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Combat
{
    /// <summary>
    /// Mapeia WeaponType para PlayerAttackAnimArchetype.
    /// Ponto canonico unico de mapeamento — nunca duplicar esta logica em outro lugar.
    /// Vive em Combat (conhece WeaponType); o resultado e um tipo de Core (PlayerAttackAnimArchetype),
    /// mantendo Core ignorante de Combat.
    /// </summary>
    public static class WeaponAttackArchetypeMapper
    {
        /// <summary>
        /// Retorna o arquetipo de animacao de ataque para o tipo de arma informado.
        /// Valores nao mapeados (extensoes futuras do enum) caem em Sword com LogWarning.
        /// </summary>
        public static PlayerAttackAnimArchetype FromWeaponType(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Sword:   return PlayerAttackAnimArchetype.Sword;
                case WeaponType.Bow:     return PlayerAttackAnimArchetype.Bow;
                case WeaponType.Axe:     return PlayerAttackAnimArchetype.Heavy;
                case WeaponType.Hammer:  return PlayerAttackAnimArchetype.Heavy;
                case WeaponType.Spear:   return PlayerAttackAnimArchetype.Thrust;
                case WeaponType.Dagger:  return PlayerAttackAnimArchetype.Dagger;
                case WeaponType.Staff:   return PlayerAttackAnimArchetype.Cast;
                case WeaponType.Wand:    return PlayerAttackAnimArchetype.Cast;
                case WeaponType.Tool:    return PlayerAttackAnimArchetype.Sword;
                case WeaponType.None:    return PlayerAttackAnimArchetype.Sword;
                default:
                    Debug.LogWarning($"[WeaponAttackArchetypeMapper] WeaponType nao mapeado: {type}. " +
                                     "Usando fallback Sword.");
                    return PlayerAttackAnimArchetype.Sword;
            }
        }
    }
}
