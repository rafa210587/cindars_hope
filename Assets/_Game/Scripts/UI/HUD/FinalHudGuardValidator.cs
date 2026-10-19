using System.Collections.Generic;

namespace CindarsHope.UI.HUD
{
    public static class FinalHudGuardValidator
    {
        private const int MaxActiveSlots = 4;

        // Forbidden identifiers in final HUD projection
        private static readonly HashSet<string> ForbiddenFieldIds = new HashSet<string>
        {
            "Breath", "Folego", "BR", "breath", "folego", "br",
            "debug_coord", "spawn_budget", "raw_quest_flag", "raw_save_state"
        };

        public static List<string> Validate(GameplayHudViewModel hud)
        {
            var errors = new List<string>();
            if (hud == null) { errors.Add("HUD_NULL"); return errors; }

            // Active slot cap
            if (hud.ActiveSkillSlots != null && hud.ActiveSkillSlots.Count > MaxActiveSlots)
                errors.Add($"ACTIVE_SLOTS_EXCEEDED: {hud.ActiveSkillSlots.Count} > {MaxActiveSlots}");

            // Debug visible in final HUD
            if (hud.DebugVisible)
                errors.Add("DEBUG_VISIBLE_IN_FINAL_HUD");

            // Check forbidden field content in hotbar item IDs
            if (hud.HotbarSlots != null)
            {
                foreach (var slot in hud.HotbarSlots)
                {
                    if (ContainsForbidden(slot.ItemId))
                        errors.Add($"FORBIDDEN_ITEM_ID_IN_HOTBAR: {slot.ItemId}");
                }
            }

            // Validate active slots
            if (hud.ActiveSkillSlots != null)
            {
                foreach (var slot in hud.ActiveSkillSlots)
                {
                    if (ContainsForbidden(slot.SkillId))
                        errors.Add($"FORBIDDEN_SKILL_ID: {slot.SkillId}");
                    // Dash/Dodge/Block must not occupy active slots
                    if (IsDashDodgeBlock(slot.SkillId))
                        errors.Add($"DASH_DODGE_BLOCK_IN_ACTIVE_SLOT: {slot.SkillId}");
                }
            }

            return errors;
        }

        public static bool IsForbiddenFieldId(string fieldId) =>
            !string.IsNullOrEmpty(fieldId) && ForbiddenFieldIds.Contains(fieldId);

        private static bool ContainsForbidden(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            foreach (var f in ForbiddenFieldIds)
                if (id.Contains(f)) return true;
            return false;
        }

        private static bool IsDashDodgeBlock(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return false;
            var lower = skillId.ToLowerInvariant();
            return lower.StartsWith("skill_dash") || lower.StartsWith("skill_dodge") || lower.StartsWith("skill_block");
        }
    }
}
