using System.Collections.Generic;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Combat
{
    /// <summary>F01 — valida os 13 IDs canônicos no StatusEffectDatabase (únicos e completos).</summary>
    public static class ValidateStatusEffectDatabase
    {
        private const string DatabasePath = "Assets/_Game/Data/Combat/StatusEffectDatabase.asset";

        public static readonly string[] CanonicalIds =
        {
            "status_bleed", "status_burn", "status_chill", "status_poison", "status_stun",
            "status_root", "status_fear", "status_confusion_lite", "status_durability_stress",
            "status_corruption", "status_slow", "status_heat_stress", "status_cold_stress"
        };

        public static void Validate()
        {
            var errors = ValidateAndReport();
            if (errors == 0)
            {
                Debug.Log($"[ValidateStatusEffectDatabase] PASS — {CanonicalIds.Length} IDs canônicos presentes e únicos.");
            }
            else
            {
                Debug.LogError($"[ValidateStatusEffectDatabase] FAIL — {errors} problema(s).");
            }
        }

        public static int ValidateAndReport()
        {
            var database = AssetDatabase.LoadAssetAtPath<StatusEffectDatabaseSO>(DatabasePath);
            if (database == null)
            {
                Debug.LogError($"[ValidateStatusEffectDatabase] Database ausente: {DatabasePath}");
                return 1;
            }

            var errors = 0;
            var seen = new HashSet<string>();
            foreach (var entry in database.All)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Id))
                {
                    continue;
                }

                if (!seen.Add(entry.Id))
                {
                    Debug.LogError($"[ValidateStatusEffectDatabase] ID duplicado: {entry.Id}");
                    errors++;
                }
            }

            foreach (var id in CanonicalIds)
            {
                if (!database.TryGetById(id, out var effect) || effect == null)
                {
                    Debug.LogError($"[ValidateStatusEffectDatabase] ID canônico ausente: {id} (rode CindarsHope/Combat/Generate Canonical Status Effects)");
                    errors++;
                }
            }

            return errors;
        }
    }
}
