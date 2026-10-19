using System.Collections.Generic;
using System.Linq;

namespace CindarsHope.NPC
{
    public class NpcValidationIssue
    {
        public string NpcId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsBlocker { get; set; }
    }

    public class NpcDefinitionValidator
    {
        // D&D class names that must not be used as mechanical gameplay classes
        private static readonly HashSet<string> ForbiddenDnDClassNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "Cleric", "Paladin", "Fighter", "Wizard", "Rogue", "Bard",
            "Druid", "Ranger", "Warlock", "Sorcerer", "Monk", "Barbarian"
        };

        public List<NpcValidationIssue> Validate(NpcDefinition npc)
        {
            var issues = new List<NpcValidationIssue>();
            if (npc == null) { issues.Add(new NpcValidationIssue { Code = "NPC_NULL", Message = "NPC is null", IsBlocker = true }); return issues; }

            if (string.IsNullOrEmpty(npc.NpcId))
                issues.Add(new NpcValidationIssue { NpcId = npc.NpcId, Code = "NPC_NO_ID", Message = "NPC has no NpcId", IsBlocker = true });

            if (string.IsNullOrEmpty(npc.DisplayName))
                issues.Add(new NpcValidationIssue { NpcId = npc.NpcId, Code = "NPC_NO_NAME", Message = $"NPC '{npc.NpcId}' has no DisplayName", IsBlocker = false });

            // Functional class must not be D&D mechanical names (checked via RoleTags as plain strings)
            foreach (var tag in npc.RoleTags)
            {
                if (ForbiddenDnDClassNames.Contains(tag))
                    issues.Add(new NpcValidationIssue { NpcId = npc.NpcId, Code = "NPC_DND_CLASS_IN_TAGS", Message = $"NPC '{npc.NpcId}' has forbidden D&D class '{tag}' in RoleTags", IsBlocker = true });
            }

            // Fixed married NPC must not be romance eligible
            if (npc.RelationshipStatus == RelationshipStatus.MarriedToNpc &&
                npc.RomanceEligibility == RomanceEligibility.EligibleAnyGender)
                issues.Add(new NpcValidationIssue { NpcId = npc.NpcId, Code = "NPC_MARRIED_ROMANCE_ELIGIBLE", Message = $"NPC '{npc.NpcId}' is MarriedToNpc but also RomanceEligibleAnyGender — fixed couples cannot be romance candidates", IsBlocker = true });

            // MarriedToNpc requires SpouseNpcId
            if (npc.RelationshipStatus == RelationshipStatus.MarriedToNpc && string.IsNullOrEmpty(npc.SpouseNpcId))
                issues.Add(new NpcValidationIssue { NpcId = npc.NpcId, Code = "NPC_MARRIED_NO_SPOUSE", Message = $"NPC '{npc.NpcId}' is MarriedToNpc but SpouseNpcId is empty", IsBlocker = true });

            // TooYoung/NarrativelyBlocked must not be MarriageEligible
            if (npc.RelationshipStatus == RelationshipStatus.TooYoungOrNarrativelyBlocked &&
                npc.MarriageEligibility != MarriageEligibility.NotEligible)
                issues.Add(new NpcValidationIssue { NpcId = npc.NpcId, Code = "NPC_TOO_YOUNG_MARRIAGE", Message = $"NPC '{npc.NpcId}' is TooYoungOrNarrativelyBlocked but has MarriageEligibility", IsBlocker = true });

            // Validate religion: no active Anya public temple here
            // (HasPrivateAnyaSympathy as narrative is OK; no mechanical Anya service tag)
            if (npc.ServiceTags.Any(t => t.Contains("AnyaTemple") || t.Contains("anya_temple") || t.Contains("AltarAnya")))
                issues.Add(new NpcValidationIssue { NpcId = npc.NpcId, Code = "NPC_ANYA_ACTIVE_TEMPLE", Message = $"NPC '{npc.NpcId}' has active Anya temple service tag — Anya has no public active temple/cult per canon", IsBlocker = true });

            return issues;
        }

        public List<NpcValidationIssue> ValidateRoster(IEnumerable<NpcDefinition> roster)
        {
            var allIssues = new List<NpcValidationIssue>();
            var seenIds = new HashSet<string>();
            foreach (var npc in roster)
            {
                var issues = Validate(npc);
                allIssues.AddRange(issues);
                if (!string.IsNullOrEmpty(npc?.NpcId))
                {
                    if (!seenIds.Add(npc.NpcId))
                        allIssues.Add(new NpcValidationIssue { NpcId = npc.NpcId, Code = "NPC_DUPLICATE_ID", Message = $"Duplicate NpcId '{npc.NpcId}'", IsBlocker = true });
                }
            }
            return allIssues;
        }
    }
}
