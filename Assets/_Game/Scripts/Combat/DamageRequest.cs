using System;

namespace CindarsHope.Combat
{
    public class DamageRequest
    {
        public string SourceId { get; set; }
        public string TargetId { get; set; }
        public int BaseDamage { get; set; }
        public DamageType DamageType { get; set; }
        public int AttributeBonus { get; set; }
        public int SourceFlatBonus { get; set; }
        public bool CanTriggerVulnerability { get; set; } = true;
        public string[] StatusApplicationRules { get; set; }
        public bool IsDamageOverTimeTick { get; set; }

        public DamageRequest() { }

        public DamageRequest(
            string targetId,
            int baseDamage,
            DamageType damageType = DamageType.Physical,
            string sourceId = "",
            int attributeBonus = 0,
            int sourceFlatBonus = 0)
        {
            SourceId = sourceId ?? string.Empty;
            TargetId = targetId ?? string.Empty;
            BaseDamage = baseDamage;
            DamageType = damageType;
            AttributeBonus = attributeBonus;
            SourceFlatBonus = sourceFlatBonus;
            CanTriggerVulnerability = true;
            IsDamageOverTimeTick = false;
        }
    }
}
