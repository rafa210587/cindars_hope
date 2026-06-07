using System;

namespace CindarsHope.Save
{
    /// <summary>
    /// SPEC 01.06: Save Provider Architecture Incremental Hardening
    /// Roadmap for extracting save sections to providers incrementally.
    /// ISaveSectionProvider already exists. This documents safe migration order and constraints.
    /// </summary>
    public static class SaveProviderArchitectureRoadmap
    {
        [Serializable]
        public class ProviderMigrationStep
        {
            public int Phase;                    // 1, 2, 3, ...
            public string SectionName;           // e.g., "Hotbar"
            public string ProviderClassName;     // e.g., "HotbarSectionProvider"
            public bool AlreadyImplemented;      // YES if provider exists
            public int RiskLevel;                // 1=LOW, 2=MEDIUM, 3=HIGH
            public string[] PrerequisiteSections; // Must restore before this provider
            public string[] DependentSections;    // Sections that depend on this
            public string Notes;
        }

        /// <summary>
        /// Incremental provider migration roadmap.
        /// Phases ordered by risk and independence. Each phase is reversible.
        /// </summary>
        public static readonly ProviderMigrationStep[] Roadmap = new[]
        {
            // PHASE 1: HOTBAR (Already implemented; just consolidate)
            new ProviderMigrationStep
            {
                Phase = 1,
                SectionName = "Hotbar",
                ProviderClassName = "HotbarSectionProvider",
                AlreadyImplemented = true,
                RiskLevel = 1, // LOW
                PrerequisiteSections = new[] { "Inventory", "Equipment" },
                DependentSections = Array.Empty<string>(),
                Notes = "HotbarSectionProvider already exists and is integrated. No migration needed; consolidate in this spec."
            },

            // PHASE 2: STAMINA (Independent; no gameplay impact; low risk)
            new ProviderMigrationStep
            {
                Phase = 2,
                SectionName = "Stamina",
                ProviderClassName = "StaminaSectionProvider", // Future
                AlreadyImplemented = false,
                RiskLevel = 1, // LOW
                PrerequisiteSections = Array.Empty<string>(),
                DependentSections = Array.Empty<string>(),
                Notes = "Pure state container; no dependencies. Recommended first provider migration target."
            },

            // PHASE 3: PROGRESSION (Low risk; only depends on restore order)
            new ProviderMigrationStep
            {
                Phase = 3,
                SectionName = "Progression",
                ProviderClassName = "ProgressionSectionProvider", // Future
                AlreadyImplemented = false,
                RiskLevel = 1, // LOW
                PrerequisiteSections = Array.Empty<string>(),
                DependentSections = new[] { "SkillTree" },
                Notes = "Can extract after Stamina. SkillTree depends on Progression.Level."
            },

            // PHASE 4: GAME TIME (Low risk; enabler for other sections)
            new ProviderMigrationStep
            {
                Phase = 4,
                SectionName = "GameTime",
                ProviderClassName = "GameTimeSectionProvider", // Future
                AlreadyImplemented = false,
                RiskLevel = 1, // LOW
                PrerequisiteSections = new[] { "CurrentDay" },
                DependentSections = Array.Empty<string>(),
                Notes = "Depends on CurrentDay. Enable after day/time foundations are solid."
            },

            // PHASE 5: EQUIPMENT DURABILITY (Low risk; optional feature)
            new ProviderMigrationStep
            {
                Phase = 5,
                SectionName = "EquipmentDurability",
                ProviderClassName = "EquipmentDurabilitySectionProvider", // Future
                AlreadyImplemented = false,
                RiskLevel = 2, // MEDIUM
                PrerequisiteSections = new[] { "Equipment" },
                DependentSections = Array.Empty<string>(),
                Notes = "Depends on Equipment being restored first. Can migrate after Hotbar."
            },

            // PHASE 6: BESTIARY (Low risk; optional discovery feature)
            new ProviderMigrationStep
            {
                Phase = 6,
                SectionName = "Bestiary",
                ProviderClassName = "BestiarySectionProvider", // Future
                AlreadyImplemented = false,
                RiskLevel = 2, // MEDIUM
                PrerequisiteSections = Array.Empty<string>(),
                DependentSections = Array.Empty<string>(),
                Notes = "Pure knowledge state; no gameplay dependencies. Low risk."
            },

            // LATER PHASES: High-risk sections deferred to specialized domain specs
            // - Inventory (Phase N): High coupling; inventory fixes, equipment bindings
            // - Equipment (Phase N+1): Requires durability and item definitions
            // - SkillTree (Phase N+2): Depends on Progression.Level; complex state
            // - Farm (Phase N+3): Day-cycle dependencies; crop growth state
            // - Economy (Phase N+4): Shop stock, restock logic; complex
            // - Cave (Phase N+5): Run state, checkpoint management; very complex
            // - Death (Phase N+6): Corpse recovery, special rules; handled last
        };

        /// <summary>
        /// Query provider migration info for a given section.
        /// </summary>
        public static ProviderMigrationStep GetMigrationStep(string sectionName)
        {
            foreach (var step in Roadmap)
            {
                if (step.SectionName == sectionName)
                    return step;
            }
            return null;
        }

        /// <summary>
        /// Current state: ISaveSectionProvider exists and is used by HotbarSectionProvider.
        /// SaveManager.ApplySaveData() does NOT iterate providers; each provider is called directly or via SaveManager.
        /// Future spec (01.07 or later) may create a SaveProviderRegistry to allow SaveManager to discover and call providers generically.
        /// </summary>
        public static string ArchitectureNotes => @"
CURRENT STATE (as of 01.06):
- ISaveSectionProvider interface exists in Save/ folder
- HotbarSectionProvider implements ISaveSectionProvider for Hotbar section
- SaveManager does not iterate providers; each provider is called by name
- Ownership registry (01.05) documents which sections have owners

FUTURE VISION (01.07+):
- Create SaveProviderRegistry to discover providers by section name
- Allow SaveManager.ApplySaveData() to query provider for each section
- Gradually migrate low-risk sections (Stamina, Progression) to providers
- Keep high-risk sections (Inventory, Equipment, Cave) in SaveManager for now
- Each domain spec can propose provider extraction with safety justification

CONSTRAINTS:
- Provider must be reversible without breaking existing saves
- Provider cannot change schema or gameplay behavior
- Provider must respect ownership registry and restore order
- Invalid ID fallback must integrate with provider's section owner
";
    }
}
