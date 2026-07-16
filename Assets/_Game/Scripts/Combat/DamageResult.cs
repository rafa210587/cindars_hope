using System.Collections.Generic;
using CindarsHope.Foundation;

namespace CindarsHope.Combat
{
    public class DamageResult
    {
        public string SourceId { get; set; }
        public string TargetId { get; set; }
        public int BaseDamage { get; set; }
        public int AttributeBonus { get; set; }
        public int SourceFlatBonus { get; set; }
        public int Defense { get; set; }
        public DamageType DamageType { get; set; }
        public float CombatResistanceMultiplier { get; set; }
        public float EnvironmentalResistanceContribution { get; set; }
        public float VulnerabilityMultiplier { get; set; }
        public float StatusReceivedDamageMultiplier { get; set; }
        public int FinalDamage { get; set; }
        public bool WasImmune { get; set; }
        public bool WasVulnerable { get; set; }
        public string[] AppliedStatusIds { get; set; }
        public string DebugBreakdown { get; set; }

        public DamageResult() { }

        public DamageResult(DamageRequest request)
        {
            SourceId = request?.SourceId ?? string.Empty;
            TargetId = request?.TargetId ?? string.Empty;
            BaseDamage = request?.BaseDamage ?? 0;
            AttributeBonus = request?.AttributeBonus ?? 0;
            SourceFlatBonus = request?.SourceFlatBonus ?? 0;
            DamageType = request?.DamageType ?? DamageType.Physical;
            FinalDamage = 0;
            WasImmune = false;
            WasVulnerable = false;
            AppliedStatusIds = new string[0];
            CombatResistanceMultiplier = 1f;
            EnvironmentalResistanceContribution = 0f;
            VulnerabilityMultiplier = 1f;
            StatusReceivedDamageMultiplier = 1f;
        }
    }
}
