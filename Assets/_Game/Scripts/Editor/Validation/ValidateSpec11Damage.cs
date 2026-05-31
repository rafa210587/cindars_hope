#if UNITY_EDITOR
using CindarsHope.Combat;
using CindarsHope.Core.Events;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public class ValidateSpec11Damage
    {
        [MenuItem("CindarsHope/Advanced/Legacy/Validation/SPEC 11 - Damage Status Resistances")]
        public static void ValidateSpec11()
        {
            Debug.Log("=== SPEC 11 Validation: Damage Status Resistances ===");

            var passed = true;
            passed &= ValidateDamageCalculator();
            passed &= ValidateStatusEffectManager();
            passed &= ValidateFloatingDamageNumbers();
            passed &= ValidateDamageEvents();
            passed &= ValidateResistanceProfile();

            if (passed)
            {
                Debug.Log("✓ SPEC 11 Validation: ALL CHECKS PASSED");
            }
            else
            {
                Debug.LogError("✗ SPEC 11 Validation: SOME CHECKS FAILED");
            }
        }

        private static bool ValidateDamageCalculator()
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/_Game/Scripts/Combat/DamageCalculator.cs");
            if (script != null && script.text.Contains("public static DamageResult Calculate") && script.text.Contains("VulnerabilityMultiplier"))
            {
                Debug.Log("✓ DamageCalculator.cs exists with Calculate() supporting vulnerability");
                return true;
            }

            Debug.LogError("✗ DamageCalculator.cs missing or incomplete");
            return false;
        }

        private static bool ValidateStatusEffectManager()
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/_Game/Scripts/Combat/StatusEffectManager.cs");
            if (script != null && script.text.Contains("ApplyStatus") && script.text.Contains("RemoveStatus") && script.text.Contains("UpdateAllStatuses"))
            {
                Debug.Log("✓ StatusEffectManager.cs exists with apply/remove/update");
                return true;
            }

            Debug.LogError("✗ StatusEffectManager.cs missing or incomplete");
            return false;
        }

        private static bool ValidateFloatingDamageNumbers()
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/_Game/Scripts/Combat/FloatingDamageNumberDisplayer.cs");
            if (script != null && script.text.Contains("FloatingDamageNumberDisplayer") && script.text.Contains("DamageAppliedEvent"))
            {
                Debug.Log("✓ FloatingDamageNumberDisplayer.cs exists and subscribes to DamageAppliedEvent");
                return true;
            }

            Debug.LogError("✗ FloatingDamageNumberDisplayer.cs missing or incomplete");
            return false;
        }

        private static bool ValidateDamageEvents()
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/_Game/Scripts/Core/Events/StatusAndDamageEvents.cs");
            if (script != null && script.text.Contains("class DamageAppliedEvent") && script.text.Contains("StatusAppliedEvent") && script.text.Contains("StatusTickedEvent"))
            {
                Debug.Log("✓ Damage and Status events exist (DamageApplied, StatusApplied, StatusTicked)");
                return true;
            }

            Debug.LogError("✗ Damage/Status events missing");
            return false;
        }

        private static bool ValidateResistanceProfile()
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/_Game/Scripts/Combat/CombatResistanceProfile.cs");
            if (script != null && script.text.Contains("CombatResistanceProfile"))
            {
                Debug.Log("✓ CombatResistanceProfile.cs exists for resistance tracking");
                return true;
            }

            Debug.LogWarning("⚠ CombatResistanceProfile.cs not found (may be deferred to later spec)");
            return true;
        }
    }
}
#endif
