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
        public DamageSourceKind SourceKind { get; set; }
        public SpellDiscipline SpellDiscipline { get; set; }
        public string SourceInstanceId { get; set; }
        public string TargetInstanceId { get; set; }
        public string ActionToken { get; set; }
        public bool IsCritical { get; set; }
        public bool IsPrimaryDamage { get; set; }
        public bool CanTriggerCapstones { get; set; }
        public bool CanTriggerStatusEffects { get; set; }
        public bool CanTriggerReactions { get; set; }

        public DamageResult() { }

        public DamageResult(DamageRequest request)
        {
            SourceId = request?.SourceId ?? string.Empty;
            TargetId = request?.TargetId ?? string.Empty;
            BaseDamage = request?.BaseDamage ?? 0;
            AttributeBonus = request?.AttributeBonus ?? 0;
            SourceFlatBonus = request?.SourceFlatBonus ?? 0;
            DamageType = request?.DamageType ?? DamageType.Physical;
            SourceKind = request?.SourceKind ?? DamageSourceKind.None;
            SpellDiscipline = request?.SpellDiscipline ?? SpellDiscipline.None;
            SourceInstanceId = request?.SourceInstanceId ?? string.Empty;
            TargetInstanceId = request?.TargetInstanceId ?? string.Empty;
            ActionToken = request?.ActionToken ?? string.Empty;
            IsCritical = request?.IsCritical ?? false;
            IsPrimaryDamage = request?.IsPrimaryDamage ?? true;
            CanTriggerCapstones = request?.CanTriggerCapstones ?? false;
            CanTriggerStatusEffects = request?.CanTriggerStatusEffects ?? true;
            CanTriggerReactions = request?.CanTriggerReactions ?? true;
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
