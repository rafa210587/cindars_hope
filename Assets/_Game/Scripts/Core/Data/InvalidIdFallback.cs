using System;

namespace CindarsHope.Core.Data
{
    /// <summary>
    /// SPEC 01.03: Invalid ID Fallback Rules
    /// Defines policy for handling missing or invalid persisted IDs during load.
    /// </summary>

    /// <summary>
    /// Category of invalid ID finding.
    /// </summary>
    public enum InvalidIdCategory
    {
        MissingOptionalReference,    // Referenced item/NPC/quest ID not found; gameplay continues
        MissingRequiredReference,    // Critical resource missing; must use safe default or block load
        UnknownLegacyId,             // ID format/naming doesn't match current convention; may indicate old save
        RemovedContentId,            // ID was valid but content was intentionally removed in patch
        DuplicateId,                 // Same ID appears twice in registry; ambiguous resolution
        EmptyId,                     // Persisted field is null or whitespace
        MalformedId,                 // ID contains invalid characters or structure
        SectionOwnerUnknown,         // Save section found but no owning system identified
    }

    /// <summary>
    /// Severity of invalid ID finding.
    /// </summary>
    public enum InvalidIdSeverity
    {
        Info = 0,       // Non-persisted/local-only, no gameplay impact
        Warning = 1,    // Degraded but recoverable; gameplay may be affected
        Error = 2,      // State cannot be restored correctly; load continues in safe mode
        Blocker = 3,    // Save cannot continue safely; migration/manual fix required
    }

    /// <summary>
    /// Result of invalid ID resolution attempt.
    /// </summary>
    public readonly struct InvalidIdFinding
    {
        public InvalidIdCategory Category { get; }
        public InvalidIdSeverity Severity { get; }
        public string SectionName { get; }
        public string FieldName { get; }
        public string InvalidValue { get; }
        public string SectionOwner { get; }
        public string FallbackResolution { get; }
        public DateTime Timestamp { get; }

        public InvalidIdFinding(
            InvalidIdCategory category,
            InvalidIdSeverity severity,
            string sectionName,
            string fieldName,
            string invalidValue,
            string sectionOwner = "Unknown",
            string fallbackResolution = "")
        {
            Category = category;
            Severity = severity;
            SectionName = sectionName ?? "Unknown";
            FieldName = fieldName ?? "Unknown";
            InvalidValue = invalidValue ?? "(null)";
            SectionOwner = sectionOwner ?? "Unknown";
            FallbackResolution = fallbackResolution ?? "";
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString()
            => $"[{Severity.ToString().ToUpper()}] {SectionName}.{FieldName}='{InvalidValue}' ({Category}) @ {SectionOwner}. Fallback: {FallbackResolution}";
    }

    /// <summary>
    /// Policy document: Invalid ID Fallback by Section Type.
    /// SPEC 01.03 defines behavior for each save section when an ID fails to resolve.
    /// </summary>
    public static class InvalidIdFallbackPolicy
    {
        // FALLBACK RULES BY SECTION (documented for future domain-specific save specs)

        public const string INVENTORY_POLICY = @"
            Section: Inventory/Items/Equipment/Hotbar
            Owner: InventoryManager, EquipmentManager

            - Missing Item ID (required, persisted):
              Severity: ERROR
              Fallback: Remove from inventory; log as WARNING
              Reason: No valid item to restore; silently dropping would hide inventory loss

            - Missing Equipment ID (required, persisted):
              Severity: ERROR
              Fallback: Clear slot; restore as unequipped
              Reason: Slot must be valid; undefined item is unsafe

            - Missing Hotbar Skill/Action ID (required, persisted):
              Severity: WARNING
              Fallback: Clear hotbar slot to empty
              Reason: Binding becomes invalid; gameplay continues
        ";

        public const string QUEST_POLICY = @"
            Section: Quest Flags/Objectives/Rewards
            Owner: QuestManager

            - Missing Quest ID (required, persisted):
              Severity: ERROR
              Fallback: Quarantine quest entry; mark completed to prevent soft-lock
              Reason: Undefined quest is unplayable; auto-completion prevents infinite wait

            - Missing Objective ID (required, persisted):
              Severity: ERROR
              Fallback: Mark objective as unknown; allow progression
              Reason: Cannot evaluate unknown objective; mark completed to unblock quest

            - Missing Reward ID (required, persisted):
              Severity: WARNING
              Fallback: Skip reward; log discrepancy
              Reason: Player may have already received reward; unknown item is unsafe to grant
        ";

        public const string BESTIARY_POLICY = @"
            Section: Bestiary/Knowledge
            Owner: BestiaryManager

            - Missing Enemy ID (optional, persisted):
              Severity: WARNING
              Fallback: Mark entry as unknown; retain knowledge state
              Reason: Bestiary entry may unlock again; no critical gameplay impact

            - Unknown Enemy Taxonomy (optional):
              Severity: INFO
              Fallback: Retain entry as discovered; ignore specific enemy data
              Reason: Knowledge remains; specific enemy may be readded later
        ";

        public const string CAVE_POLICY = @"
            Section: Cave Run/Corpse/Loot
            Owner: CaveRuntimeMaterializer, CorpseRecoveryManager

            - Missing Enemy ID in spawn plan (required):
              Severity: ERROR
              Fallback: Remove enemy from spawn; regenerate run may be needed
              Reason: Undefined enemy cannot be instantiated

            - Missing Item ID in corpse loot (required):
              Severity: WARNING
              Fallback: Remove item from corpse; log discrepancy
              Reason: Corpse recovery continues; missing item prevents soft-lock

            - Missing Resource Node ID (optional):
              Severity: INFO
              Fallback: Skip node; restore proceeds
              Reason: Resource is cosmetic to cave progress
        ";

        public const string FARM_POLICY = @"
            Section: Farm Crops/World Resources
            Owner: FarmManager, TreeRegistry

            - Missing Seed ID (required, persisted):
              Severity: ERROR
              Fallback: Clear plot; mark for manual replanting
              Reason: Undefined seed cannot grow; plot becomes available

            - Missing Tree ID (required, persisted):
              Severity: ERROR
              Fallback: Remove tree from world; log location
              Reason: Undefined tree cannot render/interact; space becomes available
        ";

        public const string NPC_POLICY = @"
            Section: NPC/Services/Schedules
            Owner: NpcManager, ShopManager

            - Missing NPC ID (required, persisted):
              Severity: ERROR
              Fallback: Remove from active NPC list; mark schedule data for cleanup
              Reason: Cannot render/interact with undefined NPC

            - Missing Shop Stock Item ID (required, persisted):
              Severity: WARNING
              Fallback: Remove item from shop; trigger restock regeneration
              Reason: Stock becomes invalid; restock can provide replacement
        ";

        public const string PLAYER_POLICY = @"
            Section: Player Skills/Spells/Status
            Owner: PlayerSkillTreeManager, PlayerStatusManager

            - Missing Skill Node ID (optional, persisted):
              Severity: WARNING
              Fallback: Treat as unlearned; retain skill points
              Reason: Player may unlock again through progression

            - Missing Spell ID (required, persisted):
              Severity: ERROR
              Fallback: Remove spell from player spellbook; log loss
              Reason: Undefined spell cannot be cast

            - Missing Status Effect ID (optional, persisted):
              Severity: INFO
              Fallback: Skip status; remove from active list
              Reason: Status is transient; removing does not affect progression
        ";

        public const string CALENDAR_POLICY = @"
            Section: Calendar/Weather/Lunar References
            Owner: GameTimeManager, WeatherManager

            - Missing Weather ID (optional):
              Severity: INFO
              Fallback: Use default weather; progression continues
              Reason: Weather is environmental; no critical gameplay impact

            - Invalid Date/Day (required):
              Severity: WARNING
              Fallback: Clamp to valid day range; warn on load
              Reason: Calendar must be consistent; clamping prevents infinite loops
        ";

        /// <summary>
        /// Query fallback behavior for a given section and category.
        /// Returns recommended (Severity, FallbackDescription) or null if unknown.
        /// </summary>
        public static (InvalidIdSeverity severity, string fallback)? GetFallbackForSection(
            string sectionName,
            InvalidIdCategory category)
        {
            // This table is populated as specs 01.04-01.07 define section ownership and provider rules.
            // For now, default to conservative policy:

            return category switch
            {
                InvalidIdCategory.EmptyId or InvalidIdCategory.MalformedId =>
                    (InvalidIdSeverity.Error, "Skip field; treat as unset"),
                InvalidIdCategory.MissingOptionalReference =>
                    (InvalidIdSeverity.Info, "Omit reference; continue"),
                InvalidIdCategory.MissingRequiredReference =>
                    (InvalidIdSeverity.Error, "Use safe default or skip if no default exists"),
                InvalidIdCategory.UnknownLegacyId =>
                    (InvalidIdSeverity.Warning, "Attempt to match legacy naming; if no match, treat as unknown"),
                InvalidIdCategory.RemovedContentId =>
                    (InvalidIdSeverity.Warning, "Skip content; note removal"),
                InvalidIdCategory.DuplicateId =>
                    (InvalidIdSeverity.Error, "Use first occurrence; log ambiguity"),
                InvalidIdCategory.SectionOwnerUnknown =>
                    (InvalidIdSeverity.Warning, "Log as unknown section; defer to validator"),
                _ => null
            };
        }
    }
}
