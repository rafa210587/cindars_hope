using UnityEngine;

namespace CindarsHope.Combat
{
    public static class DamageCalculator
    {
        public static DamageResult Calculate(
            DamageRequest request,
            int defense = 0,
            CombatResistanceProfile resistanceProfile = null,
            float vulnerabilityMultiplier = 1f,
            float statusReceivedDamageMultiplier = 1f,
            float elementMaterialMultiplier = 1f)
        {
            if (request == null)
                request = new DamageRequest();

            var result = new DamageResult(request);

            result.Defense = Mathf.Max(0, defense);
            result.VulnerabilityMultiplier = Mathf.Max(0f, vulnerabilityMultiplier);
            result.StatusReceivedDamageMultiplier = Mathf.Max(0f, statusReceivedDamageMultiplier);

            // fable_06: multiplicador de elemento/material da família (perfil de vulnerabilidade
            // do inimigo). Default 1.0 = neutro (assets antigos / chamadores legados inalterados).
            float elementMaterialMult = Mathf.Max(0f, elementMaterialMultiplier);

            // Raw damage: BaseDamage + AttributeBonus + SourceFlatBonus
            int rawDamage = result.BaseDamage + result.AttributeBonus + result.SourceFlatBonus;
            rawDamage = Mathf.Max(0, rawDamage);

            if (rawDamage <= 0)
            {
                result.FinalDamage = 0;
                result.DebugBreakdown = "RawDamage=0";
                return result;
            }

            // Apply resistance/weakness/immunity multiplier
            int mitigatedDamage = rawDamage;
            float elementAdjustedDamage = rawDamage;

            if (result.DamageType == DamageType.True)
            {
                // True damage ignores Defense and CombatResistanceMultiplier
                mitigatedDamage = rawDamage;
                elementAdjustedDamage = rawDamage;
                result.CombatResistanceMultiplier = 1f;
            }
            else
            {
                // Apply Defense flat mitigation
                mitigatedDamage = Mathf.Max(0, rawDamage - result.Defense);

                // Get combat resistance multiplier
                if (resistanceProfile != null)
                {
                    result.CombatResistanceMultiplier = resistanceProfile.GetMultiplier(result.DamageType);
                }
                else
                {
                    result.CombatResistanceMultiplier = 1f;
                }

                // Apply resistance multiplier
                elementAdjustedDamage = mitigatedDamage * result.CombatResistanceMultiplier;
            }

            // Check for immunity
            if (Mathf.Approximately(result.CombatResistanceMultiplier, 0f) && result.DamageType != DamageType.True)
            {
                result.WasImmune = true;
                result.FinalDamage = 0;
                result.DebugBreakdown = $"Raw={rawDamage},Defense={result.Defense},Immune=true";
                return result;
            }

            // Apply vulnerability multiplier (timed window)
            float vulnerabilityAdjustedDamage = elementAdjustedDamage * result.VulnerabilityMultiplier;
            if (vulnerabilityMultiplier > 1f && !Mathf.Approximately(vulnerabilityMultiplier, 1f))
            {
                result.WasVulnerable = true;
            }

            // fable_06: ordem canônica documentada =
            //   resistance → vulnerability window → element/material → status.
            // Element/material vem do EnemyVulnerabilityProfileSO (eixos ElementMultipliers /
            // MaterialMultipliers). Aplicado APÓS a janela e ANTES do status para não inflar a
            // janela temporária com o bônus permanente da família.
            float elementMaterialAdjustedDamage = vulnerabilityAdjustedDamage * elementMaterialMult;
            if (Mathf.Approximately(elementMaterialMult, 0f) && result.DamageType != DamageType.True)
            {
                result.WasImmune = true;
                result.FinalDamage = 0;
                result.DebugBreakdown = $"Raw={rawDamage},Defense={result.Defense},ElementMaterialImmune=true";
                return result;
            }

            if (elementMaterialMult > 1f && !Mathf.Approximately(elementMaterialMult, 1f))
            {
                result.WasVulnerable = true;
            }

            // Apply status received damage multiplier
            float statusAdjustedDamage = elementMaterialAdjustedDamage * result.StatusReceivedDamageMultiplier;

            // Round to integer and apply minimum damage rule
            int finalDamage = Mathf.RoundToInt(statusAdjustedDamage);

            if (result.BaseDamage > 0 && !result.WasImmune && finalDamage < 1)
            {
                finalDamage = 1;
            }

            result.FinalDamage = Mathf.Max(0, finalDamage);

            // Debug breakdown
            result.DebugBreakdown = $"Raw={rawDamage},Attr={result.AttributeBonus},SrcFlat={result.SourceFlatBonus}," +
                $"Defense={result.Defense},Mitigated={mitigatedDamage},ResistMult={result.CombatResistanceMultiplier:F2}," +
                $"ElemAdj={elementAdjustedDamage:F1},VulnMult={result.VulnerabilityMultiplier:F2}," +
                $"ElemMatMult={elementMaterialMult:F2}," +
                $"StatusMult={result.StatusReceivedDamageMultiplier:F2},Final={result.FinalDamage}," +
                $"Immune={result.WasImmune},Vulnerable={result.WasVulnerable}";

            return result;
        }

        // Backward compatible method
        public static DamageResult CalculateDirectDamage(
            int baseDamage,
            int attributeBonus = 0,
            float typeMultiplier = 1f,
            Equipment.EquipmentManager equipmentManager = null,
            float durabilityDamageMultiplier = 0.1f)
        {
            var request = new DamageRequest(
                targetId: "",
                baseDamage: baseDamage,
                damageType: DamageType.Physical,
                attributeBonus: attributeBonus);

            var result = Calculate(request, 0, null, 1f, 1f);

            if (equipmentManager != null)
            {
                equipmentManager.RegisterEquipmentUsage();
            }

            return result;
        }
    }
}
