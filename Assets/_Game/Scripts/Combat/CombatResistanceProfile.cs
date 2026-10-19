using System;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Combat
{
    [System.Serializable]
    public class CombatResistanceProfile
    {
        [System.Serializable]
        public class ResistanceEntry
        {
            public DamageType DamageType;
            public float Multiplier = 1f;
        }

        public ResistanceEntry[] Resistances = new ResistanceEntry[0];

        public float GetMultiplier(DamageType damageType)
        {
            if (Resistances == null || Resistances.Length == 0)
                return 1f;

            foreach (var entry in Resistances)
            {
                if (entry != null && entry.DamageType == damageType)
                    return entry.Multiplier;
            }

            return 1f;
        }

        public void SetMultiplier(DamageType damageType, float multiplier)
        {
            if (Resistances == null)
                Resistances = new ResistanceEntry[0];

            var existing = System.Array.Find(Resistances, e => e?.DamageType == damageType);
            if (existing != null)
            {
                existing.Multiplier = multiplier;
            }
            else
            {
                System.Array.Resize(ref Resistances, Resistances.Length + 1);
                Resistances[Resistances.Length - 1] = new ResistanceEntry { DamageType = damageType, Multiplier = multiplier };
            }
        }
    }
}
